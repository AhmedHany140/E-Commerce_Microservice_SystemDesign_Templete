using DotNetEnv;
using ECommerce.ProductService.Api.Endpoints;
using ECommerce.ProductService.Api.GraphQL;
using ECommerce.ProductService.Application.Features.Products.Create;
using ECommerce.ProductService.Infrastructure;
using ECommerce.ProductService.Infrastructure.Services;
using FluentValidation;
using Serilog;
using Wolverine;
using Wolverine.Http;
using Wolverine.Http.FluentValidation;


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
