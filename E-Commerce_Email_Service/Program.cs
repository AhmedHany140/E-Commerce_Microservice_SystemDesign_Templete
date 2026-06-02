using E_Commerce_Email_Service.Consumers;
using ECommerce.EmailService.Services;
using ECommerce.EmailService.Settings;
using Microsoft.EntityFrameworkCore;
using MassTransit;
using Serilog;

var builder = WebApplication.CreateBuilder(args);


Log.Logger = new LoggerConfiguration()
	.WriteTo.Console()
	.CreateLogger();

builder.Host.UseSerilog();

// Configure Options
builder.Services.Configure<SmtpSettings>(
	builder.Configuration.GetSection("SmtpSettings"));

// Register Services
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddDbContext<ECommerce.EmailService.Persistence.EmailDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Server=(localdb)\\mssqllocaldb;Database=EmailDb;Trusted_Connection=True;");
});

builder.Services.AddSingleton<ECommerce.EmailService.Idempotency.Context.IIdempotencyContextAccessor, ECommerce.EmailService.Idempotency.Context.IdempotencyContextAccessor>();
builder.Services.AddSingleton<ECommerce.EmailService.Idempotency.IIdempotencyKeyProvider, ECommerce.EmailService.Idempotency.IdempotencyKeyProvider>();

builder.Services.AddScoped<ECommerce.EmailService.Idempotency.IIdempotencyStore, ECommerce.EmailService.Idempotency.IdempotencyStore>();
builder.Services.AddScoped<ECommerce.EmailService.Idempotency.IEmailIdempotencyProcessor, ECommerce.EmailService.Idempotency.EmailIdempotencyProcessor>();
builder.Services.AddHostedService<ECommerce.EmailService.Idempotency.IdempotencyCleanupService>();

// ? Configure MassTransit
builder.Services.AddMassTransit(x =>
{
	// Register Consumer
	x.AddConsumer<EmailConsumer>();

	x.UsingRabbitMq((context, cfg) =>
	{
		var rabbitUrl = builder.Configuration["RabbitMqUrl:url"];

		cfg.Host(new Uri(rabbitUrl));

	
		cfg.UseRawJsonSerializer(RawSerializerOptions.AnyMessageType);

		cfg.ReceiveEndpoint("email-queue", e =>
		{
			e.UseConsumeFilter(typeof(ECommerce.EmailService.Idempotency.Context.MassTransitIncomingIdempotencyFilter<>), context);
			
			e.ConfigureConsumer<EmailConsumer>(context);



			e.UseMessageRetry(r =>
			{
				r.Interval(3, TimeSpan.FromSeconds(5));
			});
		});
	});
});

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


await app.RunAsync();
