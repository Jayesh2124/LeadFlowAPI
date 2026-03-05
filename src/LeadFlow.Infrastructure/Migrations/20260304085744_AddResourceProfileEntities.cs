using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeadFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddResourceProfileEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "resource_application_details",
                columns: table => new
                {
                    resource_id = table.Column<Guid>(type: "uuid", nullable: false),
                    current_ctc = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    expected_ctc = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    notice_period_days = table.Column<int>(type: "integer", nullable: true),
                    preferred_location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    availability_date = table.Column<DateOnly>(type: "date", nullable: true),
                    willing_to_relocate = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    work_mode_preference = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    skills = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    certifications = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    portfolio_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_resource_application_details", x => x.resource_id);
                    table.ForeignKey(
                        name: "fk_resource_application_details_resources_id",
                        column: x => x.resource_id,
                        principalTable: "resources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "resource_documents",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    resource_id = table.Column<Guid>(type: "uuid", nullable: false),
                    document_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    kyc_document_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    file_name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    file_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    file_size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    uploaded_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_resource_documents", x => x.id);
                    table.ForeignKey(
                        name: "fk_resource_documents_resources_id",
                        column: x => x.resource_id,
                        principalTable: "resources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_resource_documents_users_id",
                        column: x => x.uploaded_by_user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "resource_employments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    resource_id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    designation = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    employment_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    is_current = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    responsibilities = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_resource_employments", x => x.id);
                    table.ForeignKey(
                        name: "fk_resource_employments_resources_id",
                        column: x => x.resource_id,
                        principalTable: "resources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "resource_references",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    resource_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reference_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    referred_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    referred_by_lead_id = table.Column<Guid>(type: "uuid", nullable: true),
                    vendor_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    portal_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    contact_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    contact_phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    contact_email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_resource_references", x => x.id);
                    table.ForeignKey(
                        name: "fk_resource_references_leads_id",
                        column: x => x.referred_by_lead_id,
                        principalTable: "leads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_resource_references_resources_id",
                        column: x => x.resource_id,
                        principalTable: "resources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_resource_references_users_id",
                        column: x => x.referred_by_user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "ix_resource_documents_document_type",
                table: "resource_documents",
                column: "document_type");

            migrationBuilder.CreateIndex(
                name: "ix_resource_documents_resource_id",
                table: "resource_documents",
                column: "resource_id");

            migrationBuilder.CreateIndex(
                name: "IX_resource_documents_uploaded_by_user_id",
                table: "resource_documents",
                column: "uploaded_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_resource_employments_resource_id",
                table: "resource_employments",
                column: "resource_id");

            migrationBuilder.CreateIndex(
                name: "ix_resource_references_reference_type",
                table: "resource_references",
                column: "reference_type");

            migrationBuilder.CreateIndex(
                name: "IX_resource_references_referred_by_lead_id",
                table: "resource_references",
                column: "referred_by_lead_id");

            migrationBuilder.CreateIndex(
                name: "IX_resource_references_referred_by_user_id",
                table: "resource_references",
                column: "referred_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_resource_references_resource_id",
                table: "resource_references",
                column: "resource_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "resource_application_details");

            migrationBuilder.DropTable(
                name: "resource_documents");

            migrationBuilder.DropTable(
                name: "resource_employments");

            migrationBuilder.DropTable(
                name: "resource_references");
        }
    }
}
