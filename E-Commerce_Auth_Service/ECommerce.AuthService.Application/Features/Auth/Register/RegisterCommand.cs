namespace ECommerce.AuthService.Application.Features.Auth.Register;

public record RegisterCommand(string Email,
	string Password,
	string FirstName,
	string LastName);
