using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.AuthService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class update_usertable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SecretKey",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SecretKey",
                table: "Users");
        }
    }
}
