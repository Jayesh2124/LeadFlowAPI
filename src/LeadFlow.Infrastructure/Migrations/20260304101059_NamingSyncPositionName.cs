using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeadFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NamingSyncPositionName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PositionName",
                table: "resource_application_details",
                newName: "position_name");

            migrationBuilder.AlterColumn<string>(
                name: "position_name",
                table: "resource_application_details",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "position_name",
                table: "resource_application_details",
                newName: "PositionName");

            migrationBuilder.AlterColumn<string>(
                name: "PositionName",
                table: "resource_application_details",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);
        }
    }
}
