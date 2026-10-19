using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
namespace QueryGateqay;

public class Program
{
	public static void Main(string[] args)
	{
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


		var secretKey = builder.Configuration["Jwt:Key"];

		if (string.IsNullOrEmpty(secretKey))
			secretKey = Environment.GetEnvironmentVariable("JWT_KEY");

		if (string.IsNullOrEmpty(secretKey))
			throw new InvalidOperationException("JWT Key is not configured.");

		var issuer = builder.Configuration["Jwt:Issuer"];
		if (string.IsNullOrEmpty(issuer))
			issuer = Environment.GetEnvironmentVariable("JWT_ISSUER");

		var audience = builder.Configuration["Jwt:Audience"];
		if (string.IsNullOrEmpty(audience))
			audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");


		builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
			.AddJwtBearer(options =>
			{
				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuer = true,
					ValidIssuer = issuer,
					ValidateAudience = true,
					ValidAudience = audience,
					ValidateIssuerSigningKey = true,
					IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
			   .GetBytes(secretKey))
				};
			});

		builder.Services.AddAuthorization();

		builder.Services.AddHeaderPropagation(options =>
		{
			options.Headers.Add("Authorization");
		});

		// The Fusion configuration uses defaultClientName: "Fusion".
		// Keep the exact name so HeaderPropagation forwards Authorization
		// to the Orders and Products subgraphs.
		builder.Services.AddHttpClient("Fusion")
			.AddHeaderPropagation();

		builder.Services.AddHttpClient("ProductService", client =>
		{
			client.BaseAddress = new Uri(builder.Configuration["Services:ProductGraphQl"]
				?? "http://localhost:5090/graphql");
		}).AddHeaderPropagation();

		builder.Services.AddHttpClient("OrderService", client =>
		{
			client.BaseAddress = new Uri(builder.Configuration["Services:OrderGraphQl"]
				?? "http://localhost:5072/graphql");
		}).AddHeaderPropagation();

	

		builder.Services
			.AddFusionGatewayServer()
			.ConfigureFromFile("gateway.fgp");

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


		app.UseAuthentication();
		app.UseAuthorization();
		app.UseHeaderPropagation();

		app.MapGraphQL();

		app.Run();
	}
}
