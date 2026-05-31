using ECommerce.OrderService.Api.Consumers;
using ECommerce.OrderService.Api.Queries;
using ECommerce.OrderService.Application.Features.Orders.Cancel;
using ECommerce.OrderService.Infrastructure;
using ECommerce.OrderService.Infrastructure.Services;
using FluentValidation;
using MassTransit;
using Serilog;
using Wolverine;
using Wolverine.Http;
using Wolverine.Http.FluentValidation;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);


Log.Logger = new LoggerConfiguration()
	.Enrich.FromLogContext()
	.WriteTo.Console()
	.WriteTo.File(
		"logs/log-.txt",
		rollingInterval: RollingInterval.Day)
	.CreateLogger();

builder.Host.UseSerilog();


// Add Infrastructure services
builder.Services.AddInfrastructure(builder.Configuration);

// Validation
builder.Services.AddValidatorsFromAssemblyContaining<CancelOrderCommand>();

// Authorization
builder.Services.AddHttpContextAccessor();

// Wolverine setup
builder.Host.UseWolverine(opts =>
{
	opts.Discovery.IncludeAssembly(typeof(CancelOrderCommand).Assembly);//in application layer for command handlers
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


builder.Services.AddMassTransit(x =>
{
	// Register Consumer
	x.AddConsumer<PaymentSucceededConsumer>();
	x.AddConsumer<PaymentFailedConsumer>();

	x.UsingRabbitMq((context, cfg) =>
	{
		var rabbitUrl = builder.Configuration["RabbitMqUrl:url"];

		cfg.Host(new Uri(rabbitUrl));


		cfg.UseRawJsonSerializer(RawSerializerOptions.AnyMessageType);

		cfg.ReceiveEndpoint("payment-queue", e =>
		{
			e.ConfigureConsumer<PaymentSucceededConsumer>(context);
			e.ConfigureConsumer<PaymentFailedConsumer>(context);


			e.UseMessageRetry(r =>
			{
				r.Interval(3, TimeSpan.FromSeconds(5));
			});
		});
	});
});


var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Order API v1"));


app.MapGraphQL("/graphql");

app.UseHttpsRedirection();
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
app.MapGrpcService<ImplementationOrderService>();

app.Run();
