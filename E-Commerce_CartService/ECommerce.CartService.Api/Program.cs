using DotNetEnv;
using ECommerce.CartService.Api.Endpoints;
using ECommerce.CartService.Application.Commands.AddItemToCart;
using ECommerce.CartService.Infrastructure;
using ECommerce.CartService.Infrastructure.Services;
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

builder.Services.AddValidatorsFromAssembly(typeof(AddItemToCartCommandValidator).Assembly);


builder.Services.AddWolverineHttp();

builder.Services.AddGrpc();

builder.Host.UseWolverine(opts =>
{
    opts.Discovery.IncludeAssembly(typeof(AddItemToCartHandler).Assembly);//in application layer for command handlers
    opts.ApplicationAssembly = typeof(CartEndpoints).Assembly;//in api host for endpoints 
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

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


app.MapWolverineEndpoints(opts =>
{
    
    opts.UseFluentValidationProblemDetailMiddleware();
});

app.MapGrpcService<ImpCartGrpcService>();

app.Run();
