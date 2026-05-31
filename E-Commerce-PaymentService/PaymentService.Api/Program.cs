using DotNetEnv;
using MassTransit;
using PaymentService.Api.Dtos;
using PaymentService.Api.Endpoints;
using PaymentService.Api.Interfaces;
using PaymentService.Api.Services;
using Serilog;

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


builder.Services.AddMassTransit(x =>
{
	x.UsingRabbitMq((context, cfg) =>
	{
		var rabbitUrl = builder.Configuration["RabbitMqUrl:url"];

		if (string.IsNullOrEmpty(rabbitUrl))
		{
			throw new ArgumentNullException("RabbitMqUrl:url", "RabbitMQ URL is missing from configuration.");
		}

		cfg.Host(new Uri(rabbitUrl));

		cfg.UseRawJsonSerializer(RawSerializerOptions.AnyMessageType);
	});
});


builder.Services.AddOpenApi();
builder.Services.AddGrpc();

builder.Configuration.AddEnvironmentVariables();

builder.Services.Configure<PaymobSettings>(
	builder.Configuration.GetSection("Paymob"));

builder.Services.AddHttpClient<IPaymobService, PaymobService>();

var app = builder.Build();

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


if (app.Environment.IsDevelopment())
	app.MapOpenApi();

PaymentEndpoints.Map(app);

app.UseWhen(
	ctx => !(ctx.Request.ContentType?.StartsWith("application/grpc") ?? false),
	b => b.UseHttpsRedirection()
);

app.MapGrpcService<PaymentGrpcServiceImpl>();

app.Run();