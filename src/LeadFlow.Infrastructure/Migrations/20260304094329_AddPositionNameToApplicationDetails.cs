using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeadFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPositionNameToApplicationDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PositionName",
                table: "resource_application_details",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PositionName",
                table: "resource_application_details");
        }
    }
}
