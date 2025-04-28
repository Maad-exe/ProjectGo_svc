using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddedNullAbleToEval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StudentCategoryScores_StudentEvaluationId_CategoryId_EvaluatorId",
                table: "StudentCategoryScores");

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "StudentCategoryScores",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_StudentCategoryScores_StudentEvaluationId_CategoryId_EvaluatorId",
                table: "StudentCategoryScores",
                columns: new[] { "StudentEvaluationId", "CategoryId", "EvaluatorId" },
                unique: true,
                filter: "[CategoryId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StudentCategoryScores_StudentEvaluationId_CategoryId_EvaluatorId",
                table: "StudentCategoryScores");

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "StudentCategoryScores",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentCategoryScores_StudentEvaluationId_CategoryId_EvaluatorId",
                table: "StudentCategoryScores",
                columns: new[] { "StudentEvaluationId", "CategoryId", "EvaluatorId" },
                unique: true);
        }
    }
}
