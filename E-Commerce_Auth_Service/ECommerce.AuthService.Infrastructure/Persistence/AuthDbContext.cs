using ECommerce.AuthService.Domain.Entities;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ECommerce.AuthService.Infrastructure.Persistence;


public class AuthDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
{
	public AuthDbContext CreateDbContext(string[] args)
	{
		
		var configuration = new ConfigurationBuilder()
			.SetBasePath(Path.Combine(Directory.GetCurrentDirectory(),
            "../ECommerce.AuthService.Api"))
			.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
			
			.Build();

		var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>();

		var connectionString = configuration.GetConnectionString("Constr");

		optionsBuilder.UseSqlServer(connectionString);

		return new AuthDbContext(optionsBuilder.Options);
	}
}

public class AuthDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) 
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
}
