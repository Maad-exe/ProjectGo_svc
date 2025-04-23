using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStudentCategoryScoresConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StudentCategoryScores_StudentEvaluationId_CategoryId",
                table: "StudentCategoryScores");

            migrationBuilder.CreateIndex(
                name: "IX_StudentCategoryScores_StudentEvaluationId_CategoryId_EvaluatorId",
                table: "StudentCategoryScores",
                columns: new[] { "StudentEvaluationId", "CategoryId", "EvaluatorId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StudentCategoryScores_StudentEvaluationId_CategoryId_EvaluatorId",
                table: "StudentCategoryScores");

            migrationBuilder.CreateIndex(
                name: "IX_StudentCategoryScores_StudentEvaluationId_CategoryId",
                table: "StudentCategoryScores",
                columns: new[] { "StudentEvaluationId", "CategoryId" },
                unique: true);
        }
    }
}
