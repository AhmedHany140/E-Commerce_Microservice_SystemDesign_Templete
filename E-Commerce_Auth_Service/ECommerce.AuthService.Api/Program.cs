using DotNetEnv;
using ECommerce.AuthService.Api.Grpc;
using ECommerce.AuthService.Api.Middleware;
using ECommerce.AuthService.Application.Features.Auth.Login;
using ECommerce.AuthService.Infrastructure;
using ECommerce.AuthService.Presentation;
using ECommerce.Shared.Messages;
using FluentValidation;
using Serilog;
using Wolverine;
using Wolverine.Http;
using Wolverine.RabbitMQ;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

// ── Logging ──────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
	.Enrich.FromLogContext()
	.WriteTo.Console()
	.WriteTo.File(
		"logs/log-.txt",
		rollingInterval: RollingInterval.Day)
	.CreateLogger();

builder.Host.UseSerilog();


// ── Infrastructure (Persistence, Identity, JWT, MassTransit, Services) ──
builder.Services.AddInfrastructure(builder.Configuration);

// ── Presentation / Validation ────────────────────────────────────────
builder.Services.AddValidatorsFromAssemblyContaining<PresentationMarker>();

// ── Wolverine ────────────────────────────────────────────────────────
builder.Host.UseWolverine(opts =>
{
	opts.ApplicationAssembly = typeof(LoginCommand).Assembly;
	opts.Discovery.IncludeAssembly(typeof(PresentationMarker).Assembly);

	var rabbitUrl = builder.Configuration["RabbitMq:Url"];

	opts.UseRabbitMq(new Uri(rabbitUrl))
		.AutoProvision();

	opts.UseSystemTextJsonForSerialization();

	opts.PublishMessage<RabbitMqEmailMessage>()
		.ToRabbitQueue("email-queue");

	opts.Policies.AutoApplyTransactions();
	opts.Policies.UseDurableLocalQueues();
});

// ── Swagger / HTTP / gRPC ────────────────────────────────────────────
builder.Services.AddSwaggerGen();
builder.Services.AddWolverineHttp();
builder.Services.AddGrpc();

var app = builder.Build();

// ── Middleware pipeline ──────────────────────────────────────────────
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Auth API v1"));

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



app.UseMiddleware<TokenBlacklistMiddleware>();

app.MapWolverineEndpoints();
app.MapGrpcService<AuthorizationGrpcServiceImplementation>();

app.Run();
