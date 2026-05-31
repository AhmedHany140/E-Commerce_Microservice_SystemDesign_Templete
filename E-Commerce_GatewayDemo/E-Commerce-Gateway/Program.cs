using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Threading.RateLimiting;

DotNetEnv.Env.Load();
JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

var builder = WebApplication.CreateBuilder(args);

// ==================== Configuration ====================
var secretKey = builder.Configuration["Jwt:Key"]
	?? Environment.GetEnvironmentVariable("JWT_KEY")
	?? throw new InvalidOperationException("JWT Key is not configured.");

var issuer = builder.Configuration["Jwt:Issuer"]
	?? Environment.GetEnvironmentVariable("JWT_ISSUER")
	?? throw new InvalidOperationException("JWT Issuer is not configured.");

var audience = builder.Configuration["Jwt:Audience"]
	?? Environment.GetEnvironmentVariable("JWT_AUDIENCE")
	?? throw new InvalidOperationException("JWT Audience is not configured.");

// ==================== Authentication ====================
builder.Services
	.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer("Bearer", options =>
	{
		options.MapInboundClaims = false;
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuerSigningKey = true,
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
			ValidateIssuer = true,
			ValidIssuer = issuer,
			ValidateAudience = true,
			ValidAudience = audience,
			ValidateLifetime = true,
			ClockSkew = TimeSpan.Zero,
			RoleClaimType = "role",
			NameClaimType = "sub"
		};

		options.Events = new JwtBearerEvents
		{
			OnTokenValidated = context =>
			{
				var userId = context.Principal?.FindFirst("sub")?.Value;
				var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
				logger.LogInformation("Token validated for user: {UserId}", userId);
				return Task.CompletedTask;
			},
			OnAuthenticationFailed = context =>
			{
				var logger = context.HttpContext.RequestServices
				.GetRequiredService<ILogger<Program>>();
				logger.LogError("Authentication failed: {Error}", context.Exception?.Message);
				return Task.CompletedTask;
			},
			OnChallenge = context =>
			{
				context.HandleResponse();
				context.Response.StatusCode = StatusCodes.Status401Unauthorized;
				context.Response.ContentType = "application/json";
				return context.Response.WriteAsJsonAsync(new
				{
					error = "Unauthorized",
					message = "Missing or invalid authentication token"
				});
			}
		};
	});

// ==================== Authorization ====================
builder.Services.AddAuthorization(options =>
{
	options.AddPolicy("RequireAuthenticatedUser", policy =>
	{
		policy.RequireAuthenticatedUser();
	});

});

// ==================== CORS ====================
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowGateway", policy =>
	{
		policy.AllowAnyOrigin()
			  .AllowAnyMethod()
			  .AllowAnyHeader();
	});
});

// ==================== Rate Limiting ====================
builder.Services.AddRateLimiter(options =>
{
	options.AddFixedWindowLimiter("global", opt =>
	{
		opt.PermitLimit = int.Parse(builder.Configuration["RateLimiting:PermitLimit"] ?? "100");
		opt.Window = TimeSpan.Parse(builder.Configuration["RateLimiting:Window"] ?? "00:01:00");
		opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
		opt.QueueLimit = 2;
	});

	options.AddFixedWindowLimiter("strict", opt =>
	{
		opt.PermitLimit = 10;
		opt.Window = TimeSpan.FromMinutes(1);
	});

	options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

	options.OnRejected = async (context, _) =>
	{
		context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
		await context.HttpContext.Response.WriteAsJsonAsync(new
		{
			error = "Too Many Requests",
			message = "Rate limit exceeded. Please try again later."
		});
	};
});

// ==================== Reverse Proxy ====================
builder.Services
	.AddReverseProxy()
	.LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// ==================== Logging ====================

Log.Logger = new LoggerConfiguration()
	.Enrich.FromLogContext()
	.WriteTo.Console()
	.WriteTo.File(
		"logs/log-.txt",
		rollingInterval: RollingInterval.Day)
	.CreateLogger();

builder.Host.UseSerilog();


// ==================== Health Checks ====================
builder.Services.AddHealthChecks()
	.AddCheck("gateway", () =>
	{
		var logger = builder.Services.BuildServiceProvider()
		.GetRequiredService<ILogger<Program>>();
		logger.LogInformation("Gateway health check performed");
		return Microsoft.Extensions.Diagnostics.HealthChecks
		.HealthCheckResult.Healthy();
	});

// ==================== Build ====================
var app = builder.Build();

// ==================== Middleware ====================
// CORS
app.UseCors("AllowGateway");

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

// Exception handling
app.UseExceptionHandler(errorApp =>
{
	errorApp.Run(async context =>
	{
		var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
		var exception = context.Features.Get<Microsoft.AspNetCore.Diagnostics
			.IExceptionHandlerFeature>();

		logger.LogError(exception?.Error, "An unhandled exception occurred");

		context.Response.StatusCode = StatusCodes.Status500InternalServerError;
		context.Response.ContentType = "application/json";

		await context.Response.WriteAsJsonAsync(new
		{
			error = "Internal Server Error",
			message = "An unexpected error occurred. Please try again later.",
			traceId = context.TraceIdentifier
		});
	});
});

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Rate limiting
app.UseRateLimiter();

// Health check
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");

// Reverse proxy
app.MapReverseProxy();

// Fallback route
app.MapFallback(context =>
{
	context.Response.StatusCode = StatusCodes.Status404NotFound;
	return context.Response.WriteAsJsonAsync(new
	{
		error = "Not Found",
		message = $"Route {context.Request.Path} not found"
	});
});

await app.RunAsync();

