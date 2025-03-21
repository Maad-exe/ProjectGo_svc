using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class PanelANDEvaluationSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StudentEvaluationId",
                table: "Teachers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsComplete",
                table: "StudentEvaluations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "RubricId",
                table: "StudentEvaluations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RubricId",
                table: "EvaluationEvents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Weight",
                table: "EvaluationEvents",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateTable(
                name: "EvaluationRubrics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvaluationRubrics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RubricCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RubricId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Weight = table.Column<double>(type: "float", nullable: false),
                    MaxScore = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RubricCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RubricCategories_EvaluationRubrics_RubricId",
                        column: x => x.RubricId,
                        principalTable: "EvaluationRubrics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentCategoryScores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentEvaluationId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    Feedback = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EvaluatorId = table.Column<int>(type: "int", nullable: false),
                    EvaluatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentCategoryScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentCategoryScores_RubricCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "RubricCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentCategoryScores_StudentEvaluations_StudentEvaluationId",
                        column: x => x.StudentEvaluationId,
                        principalTable: "StudentEvaluations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Teachers_StudentEvaluationId",
                table: "Teachers",
                column: "StudentEvaluationId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentEvaluations_RubricId",
                table: "StudentEvaluations",
                column: "RubricId");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationEvents_RubricId",
                table: "EvaluationEvents",
                column: "RubricId");

            migrationBuilder.CreateIndex(
                name: "IX_RubricCategories_RubricId",
                table: "RubricCategories",
                column: "RubricId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentCategoryScores_CategoryId",
                table: "StudentCategoryScores",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentCategoryScores_StudentEvaluationId",
                table: "StudentCategoryScores",
                column: "StudentEvaluationId");

            migrationBuilder.AddForeignKey(
                name: "FK_EvaluationEvents_EvaluationRubrics_RubricId",
                table: "EvaluationEvents",
                column: "RubricId",
                principalTable: "EvaluationRubrics",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentEvaluations_EvaluationRubrics_RubricId",
                table: "StudentEvaluations",
                column: "RubricId",
                principalTable: "EvaluationRubrics",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Teachers_StudentEvaluations_StudentEvaluationId",
                table: "Teachers",
                column: "StudentEvaluationId",
                principalTable: "StudentEvaluations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EvaluationEvents_EvaluationRubrics_RubricId",
                table: "EvaluationEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentEvaluations_EvaluationRubrics_RubricId",
                table: "StudentEvaluations");

            migrationBuilder.DropForeignKey(
                name: "FK_Teachers_StudentEvaluations_StudentEvaluationId",
                table: "Teachers");

            migrationBuilder.DropTable(
                name: "StudentCategoryScores");

            migrationBuilder.DropTable(
                name: "RubricCategories");

            migrationBuilder.DropTable(
                name: "EvaluationRubrics");

            migrationBuilder.DropIndex(
                name: "IX_Teachers_StudentEvaluationId",
                table: "Teachers");

            migrationBuilder.DropIndex(
                name: "IX_StudentEvaluations_RubricId",
                table: "StudentEvaluations");

            migrationBuilder.DropIndex(
                name: "IX_EvaluationEvents_RubricId",
                table: "EvaluationEvents");

            migrationBuilder.DropColumn(
                name: "StudentEvaluationId",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "IsComplete",
                table: "StudentEvaluations");

            migrationBuilder.DropColumn(
                name: "RubricId",
                table: "StudentEvaluations");

            migrationBuilder.DropColumn(
                name: "RubricId",
                table: "EvaluationEvents");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "EvaluationEvents");
        }
    }
}
