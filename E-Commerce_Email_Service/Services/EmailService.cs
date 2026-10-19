using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace ECommerce.EmailService.Services;

public class EmailService : IEmailService
{
	private readonly IConfiguration _configuration;
	private readonly ILogger<EmailService> _logger;
	public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
	{
		_configuration = configuration;
		_logger = logger;
	}

	public async Task SendEmailAsync(string toEmail, 
		string subject, string htmlBody, 
		CancellationToken cancellationToken = default)
	{
		var emailSettings = _configuration.GetSection("SmtpSettings");
		var smtpServer = emailSettings["SmtpServer"] ?? string.Empty;
		var port = emailSettings.GetValue<int>("Port");
		var username = emailSettings["Username"] ?? string.Empty;
		var password = emailSettings["Password"] ?? string.Empty;
		var fromEmail = emailSettings["FromEmail"] ?? string.Empty;

		if (string.IsNullOrEmpty(smtpServer) || port <= 0 ||
			string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(fromEmail))
		{
			throw new InvalidOperationException("SmtpSettings are not configured properly.");
		}

		if (string.IsNullOrEmpty(toEmail) || string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(htmlBody))
		{
			throw new ArgumentException("Email parameters cannot be null or empty.");
		}

		using var client = new SmtpClient();
		try
		{
			await client.ConnectAsync(smtpServer, port, SecureSocketOptions.StartTls);
			await client.AuthenticateAsync(username, password);

			var message = new MimeMessage();
			message.From.Add(MailboxAddress.Parse(fromEmail));
			message.To.Add(MailboxAddress.Parse(toEmail));
			message.Subject = subject;

			var builder = new BodyBuilder
			{
				TextBody = "Your email client does not support HTML. Please use a modern email client.",
				HtmlBody = htmlBody
			};
			message.Body = builder.ToMessageBody();

			await client.SendAsync(message);
			_logger.LogInformation("Email sent successfully to {To} with subject {Subject}", toEmail, subject);

		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to send email to {To}", toEmail);
		}
		finally
		{
			await client.DisconnectAsync(true);
		}
	}

	//public Task SendOtpAsync(string email, string otp)
	//{

	//	return SendAsync(email, "Your OTP Code", $"Your OTP is: {otp}");
	//}
}
