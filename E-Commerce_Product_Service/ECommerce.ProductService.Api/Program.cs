using DotNetEnv;
using ECommerce.ProductService.Api.Endpoints;
using ECommerce.ProductService.Api.GraphQL;
using ECommerce.ProductService.Application.Features.Products.Create;
using ECommerce.ProductService.Infrastructure;
using ECommerce.ProductService.Infrastructure.Services;
using FluentValidation;
using Humanizer;
using Microsoft.Data.SqlClient;
using Serilog;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.ErrorHandling;
using Wolverine.Http;
using Wolverine.Http.FluentValidation;
using Wolverine.Persistence;
using Wolverine.SqlServer;


Env.Load();
var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
	.Enrich.FromLogContext()
	.WriteTo.Console()
	.WriteTo.File(
		"logs/log-.txt",
		rollingInterval: RollingInterval.Day)
	.CreateLogger();

builder.Host.UseSerilog();

//   Wolverine Configuration with Idempotency 
builder.Host.UseWolverine(opts =>
{
	var connectionString = builder.Configuration.GetConnectionString("Constr");

	opts.Discovery.IncludeAssembly(typeof(CreateProductCommand).Assembly);
	opts.ApplicationAssembly = typeof(ProductEndpoints).Assembly;

	opts.PersistMessagesWithSqlServer(connectionString, "CartService");


	opts.UseEntityFrameworkCoreTransactions();

	//Auto -apply idempotency to all handlers that are Has DB Transactions ( those that use Entity Framework Core transactions). This means that any handler that interacts with the database will automatically have idempotency applied, ensuring that duplicate messages are not processed multiple times.
	opts.Policies.AutoApplyTransactions(IdempotencyStyle.Eager);

	//Auto -apply idempotency to all handlers that do not have DB Transactions. This means that even handlers that do not interact with the database will still benefit from idempotency, preventing duplicate processing of messages in scenarios where transactions are not used.
	opts.Policies.AutoApplyIdempotencyOnNonTransactionalHandlers();

	opts.Durability.KeepAfterMessageHandling = 24.Hours();

	opts.Durability.Mode = DurabilityMode.Solo;

	opts.Policies
	.OnException<SqlException>()
	.RetryWithCooldown(
		1.Seconds(),
		5.Seconds(),
		15.Seconds());

	opts.Policies
		.OnException<ValidationException>()
		.Discard();

	opts.Policies
		.OnAnyException()
		.MoveToErrorQueue();

	opts.Policies.AutoApplyTransactions();



});



builder.Services.AddInfrastructureServices(builder.Configuration);


builder.Services.AddHttpContextAccessor();


builder.Services.AddAuthorization();

builder.Services.AddValidatorsFromAssembly(typeof(ECommerce.ProductService.Api.Requests.CreateProductRequestValidator).Assembly);
builder.Host.UseWolverine(opts =>
{
	opts.Discovery.IncludeAssembly(typeof(CreateProductCommand).Assembly);//in application layer for command handlers
	opts.ApplicationAssembly = typeof(ProductEndpoints).Assembly;//in api host for endpoints 
});

builder.Services.AddWolverineHttp();
builder.Services.AddGrpc();

builder.Services
	.AddGraphQLServer()
	.AddAuthorization()
	.AddQueryType<Query>()
	.AddProjections()
	.AddFiltering()
	.AddSorting()
	.ModifyRequestOptions(opt =>
	{
		opt.IncludeExceptionDetails = true;
	});


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// ─────────────────────────────────────────────────────────────────────────────
var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Product API v1"));


app.MapGraphQL("/graphql");


app.UseSerilogRequestLogging(options =>
{
	options.MessageTemplate =
		"Handled {RequestMethod} {RequestPath} => {StatusCode} in {Elapsed:0.0000} ms";

	options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
	{
		diagnosticContext.Set("RemoteIP",
			httpContext.Connection.RemoteIpAddress);

		diagnosticContext.Set("UserAgent",
			httpContext.Request.Headers.UserAgent.ToString());
	};
});


// Authorization middleware must come BEFORE endpoint mapping
app.UseAuthentication();
app.UseAuthorization();
app.MapGrpcService<ImplementedProductGrpcService>();
// ── Wolverine Endpoints ────────────────────────────────────────────────────────
app.MapWolverineEndpoints(opts =>
{
    opts.UseFluentValidationProblemDetailMiddleware();
});

app.Run();
