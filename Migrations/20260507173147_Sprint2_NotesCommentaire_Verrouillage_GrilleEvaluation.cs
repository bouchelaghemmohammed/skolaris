using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Skolaris.Migrations
{
    /// <inheritdoc />
    public partial class Sprint2_NotesCommentaire_Verrouillage_GrilleEvaluation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateLimiteSaisieNotes",
                table: "Sessions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Commentaire",
                table: "Notes",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdCategorie",
                table: "Notes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GrillesEvaluation",
                columns: table => new
                {
                    IdGrille = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCoursOffert = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DateCreation = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrillesEvaluation", x => x.IdGrille);
                    table.ForeignKey(
                        name: "FK_GrillesEvaluation_CoursOfferts_IdCoursOffert",
                        column: x => x.IdCoursOffert,
                        principalTable: "CoursOfferts",
                        principalColumn: "IdCoursOffert",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CategoriesEvaluation",
                columns: table => new
                {
                    IdCategorie = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdGrille = table.Column<int>(type: "int", nullable: false),
                    Nom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Ponderation = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriesEvaluation", x => x.IdCategorie);
                    table.ForeignKey(
                        name: "FK_CategoriesEvaluation_GrillesEvaluation_IdGrille",
                        column: x => x.IdGrille,
                        principalTable: "GrillesEvaluation",
                        principalColumn: "IdGrille",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notes_IdCategorie",
                table: "Notes",
                column: "IdCategorie");

            migrationBuilder.CreateIndex(
                name: "IX_CategoriesEvaluation_IdGrille",
                table: "CategoriesEvaluation",
                column: "IdGrille");

            migrationBuilder.CreateIndex(
                name: "IX_GrillesEvaluation_IdCoursOffert",
                table: "GrillesEvaluation",
                column: "IdCoursOffert",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_CategoriesEvaluation_IdCategorie",
                table: "Notes",
                column: "IdCategorie",
                principalTable: "CategoriesEvaluation",
                principalColumn: "IdCategorie",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notes_CategoriesEvaluation_IdCategorie",
                table: "Notes");

            migrationBuilder.DropTable(
                name: "CategoriesEvaluation");

            migrationBuilder.DropTable(
                name: "GrillesEvaluation");

            migrationBuilder.DropIndex(
                name: "IX_Notes_IdCategorie",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "DateLimiteSaisieNotes",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "Commentaire",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "IdCategorie",
                table: "Notes");
        }
    }
}
