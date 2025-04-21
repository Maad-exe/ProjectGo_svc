using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddedStudentEvalCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add RequiredEvaluatorsCount column to StudentEvaluations table
            migrationBuilder.AddColumn<int>(
                name: "RequiredEvaluatorsCount",
                table: "StudentEvaluations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Update existing records with panel member count
            migrationBuilder.Sql(@"
                UPDATE se
                SET RequiredEvaluatorsCount = (
                    SELECT COUNT(*)
                    FROM PanelMembers pm
                    WHERE pm.PanelId = ge.PanelId
                )
                FROM StudentEvaluations se
                JOIN GroupEvaluations ge ON se.GroupEvaluationId = ge.Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequiredEvaluatorsCount",
                table: "StudentEvaluations");
        }
    }
}
