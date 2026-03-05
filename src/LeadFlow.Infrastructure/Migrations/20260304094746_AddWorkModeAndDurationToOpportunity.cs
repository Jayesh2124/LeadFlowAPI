using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeadFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkModeAndDurationToOpportunity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Duration",
                table: "opportunities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkMode",
                table: "opportunities",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Duration",
                table: "opportunities");

            migrationBuilder.DropColumn(
                name: "WorkMode",
                table: "opportunities");
        }
    }
}
