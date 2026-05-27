using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Skolaris.Data;

#nullable disable

namespace Skolaris.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260526000000_AddTelephoneToUtilisateur")]
    public partial class AddTelephoneToUtilisateur : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Telephone",
                table: "Utilisateurs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Telephone",
                table: "Utilisateurs");
        }
    }
}
