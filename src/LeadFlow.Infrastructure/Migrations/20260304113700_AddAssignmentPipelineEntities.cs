using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeadFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignmentPipelineEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "resource_assignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Stage = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_resource_assignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_resource_assignments_opportunity_positions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "opportunity_positions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_resource_assignments_resources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "resources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_resource_assignments_users_AssignedByUserId",
                        column: x => x.AssignedByUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "assignment_interviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    InterviewStage = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    InterviewerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    InterviewerEmail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ScheduledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Feedback = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assignment_interviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_assignment_interviews_resource_assignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "resource_assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "assignment_stage_history",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreviousStage = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    NewStage = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ChangedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assignment_stage_history", x => x.Id);
                    table.ForeignKey(
                        name: "FK_assignment_stage_history_resource_assignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "resource_assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_assignment_stage_history_users_ChangedByUserId",
                        column: x => x.ChangedByUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_assignment_interviews_AssignmentId",
                table: "assignment_interviews",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_assignment_interviews_InterviewStage",
                table: "assignment_interviews",
                column: "InterviewStage");

            migrationBuilder.CreateIndex(
                name: "IX_assignment_stage_history_AssignmentId",
                table: "assignment_stage_history",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_assignment_stage_history_ChangedByUserId",
                table: "assignment_stage_history",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_resource_assignments_AssignedByUserId",
                table: "resource_assignments",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_resource_assignments_PositionId",
                table: "resource_assignments",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_resource_assignments_PositionId_ResourceId",
                table: "resource_assignments",
                columns: new[] { "PositionId", "ResourceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_resource_assignments_ResourceId",
                table: "resource_assignments",
                column: "ResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_resource_assignments_Stage",
                table: "resource_assignments",
                column: "Stage");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "assignment_interviews");

            migrationBuilder.DropTable(
                name: "assignment_stage_history");

            migrationBuilder.DropTable(
                name: "resource_assignments");
        }
    }
}
