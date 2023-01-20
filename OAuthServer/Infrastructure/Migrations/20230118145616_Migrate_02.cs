using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OAuthServer.Infrastructure.Migrations
{
    public partial class Migrate_02 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Password",
                table: "Users",
                newName: "PasswordHashed");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PasswordHashed",
                table: "Users",
                newName: "Password");
        }
    }
}
