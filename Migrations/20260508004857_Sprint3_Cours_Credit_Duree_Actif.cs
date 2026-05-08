using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Skolaris.Migrations
{
    /// <inheritdoc />
    public partial class Sprint3_Cours_Credit_Duree_Actif : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Actif",
                table: "Cours",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Credit",
                table: "Cours",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Duree",
                table: "Cours",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Actif",
                table: "Cours");

            migrationBuilder.DropColumn(
                name: "Credit",
                table: "Cours");

            migrationBuilder.DropColumn(
                name: "Duree",
                table: "Cours");
        }
    }
}
