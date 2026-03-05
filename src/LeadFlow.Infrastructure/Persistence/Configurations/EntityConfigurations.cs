using LeadFlow.Domain.Entities;
using LeadFlow.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeadFlow.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.ToTable("users");
        b.HasKey(u => u.Id);
        b.Property(u => u.Name).IsRequired().HasMaxLength(200);
        b.Property(u => u.Email).IsRequired().HasMaxLength(300);
        b.Property(u => u.PasswordHash).IsRequired();
        b.Property(u => u.Role).IsRequired().HasMaxLength(20);
        b.HasIndex(u => u.Email).IsUnique();

        b.HasMany(u => u.Leads)
            .WithOne(l => l.User)
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasMany(u => u.Templates)
            .WithOne(t => t.User)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(u => u.SmtpSettings)
            .WithOne(s => s.User)
            .HasForeignKey<UserSmtpSettings>(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class LeadConfiguration : IEntityTypeConfiguration<Lead>
{
    public void Configure(EntityTypeBuilder<Lead> b)
    {
        b.ToTable("leads");
        b.HasKey(l => l.Id);
        b.Property(l => l.FirstName).IsRequired().HasMaxLength(100);
        b.Property(l => l.LastName).IsRequired().HasMaxLength(100);
        b.Property(l => l.Email).IsRequired().HasMaxLength(300);
        b.Property(l => l.Company).IsRequired().HasMaxLength(200);
        b.Property(l => l.Status).IsRequired().HasMaxLength(30);
        b.Property(l => l.Source).IsRequired().HasMaxLength(100);
        b.Property(l => l.Tags).HasColumnType("text[]");
        b.Property(l => l.Technologies).HasColumnType("text[]");
        b.HasIndex(l => new { l.UserId, l.Email });
    }
}

public class TechnologyConfiguration : IEntityTypeConfiguration<Technology>
{
    public void Configure(EntityTypeBuilder<Technology> b)
    {
        b.ToTable("technologies");
        b.HasKey(t => t.Id);
        b.Property(t => t.Name).IsRequired().HasMaxLength(150);
        b.HasIndex(t => t.Name).IsUnique();
    }
}

public class EmailTemplateConfiguration : IEntityTypeConfiguration<EmailTemplate>
{
    public void Configure(EntityTypeBuilder<EmailTemplate> b)
    {
        b.ToTable("email_templates");
        b.HasKey(t => t.Id);
        b.Property(t => t.Name).IsRequired().HasMaxLength(100);
        b.Property(t => t.Subject).IsRequired().HasMaxLength(500);
        b.Property(t => t.Body).IsRequired();
        b.Property(t => t.Variables).HasColumnType("text[]");
        b.Property(t => t.Attachments).HasColumnType("text[]");
    }
}

public class UserSmtpSettingsConfiguration : IEntityTypeConfiguration<UserSmtpSettings>
{
    public void Configure(EntityTypeBuilder<UserSmtpSettings> b)
    {
        b.ToTable("user_smtp_settings");
        b.HasKey(s => s.Id);
        b.Property(s => s.Host).IsRequired().HasMaxLength(300);
        b.Property(s => s.Username).IsRequired().HasMaxLength(300);
        b.Property(s => s.EncryptedPassword).IsRequired();
        b.Property(s => s.FromName).IsRequired().HasMaxLength(200);
        b.Property(s => s.FromEmail).IsRequired().HasMaxLength(300);
        b.HasIndex(s => s.UserId).IsUnique();
    }
}

public class EmailTaskConfiguration : IEntityTypeConfiguration<EmailTask>
{
    public void Configure(EntityTypeBuilder<EmailTask> b)
    {
        b.ToTable("email_tasks");
        b.HasKey(t => t.Id);
        b.Property(t => t.Status).HasConversion<string>().HasMaxLength(20);
        b.Property(t => t.RenderedSubject).IsRequired().HasMaxLength(500);
        b.Property(t => t.RenderedBody).IsRequired();
        b.Property(t => t.IdempotencyKey).HasMaxLength(300);
        b.HasIndex(t => t.IdempotencyKey).IsUnique()
            .HasFilter("\"Status\" NOT IN ('Cancelled', 'Failed')");

        b.HasIndex(t => new { t.UserId, t.LeadId });
        b.HasIndex(t => t.Status);
        b.HasIndex(t => t.ScheduledAt);

        b.HasMany(t => t.Attempts)
            .WithOne(a => a.EmailTask)
            .HasForeignKey(a => a.EmailTaskId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasMany(t => t.Followups)
            .WithOne(f => f.EmailTask)
            .HasForeignKey(f => f.EmailTaskId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class EmailAttemptConfiguration : IEntityTypeConfiguration<EmailAttempt>
{
    public void Configure(EntityTypeBuilder<EmailAttempt> b)
    {
        b.ToTable("email_attempts");
        b.HasKey(a => a.Id);
        b.Property(a => a.Result).HasConversion<string>().HasMaxLength(20);
        b.Property(a => a.ErrorMessage).HasMaxLength(2000);
        b.Property(a => a.SmtpResponse).HasMaxLength(500);
        // Denying EF from generating UPDATE statements on this table would require owned entity or shadow property tricks.
        // For simplicity: no navigation FK back — audits are write-only via factory methods.
    }
}

public class EmailFollowupConfiguration : IEntityTypeConfiguration<EmailFollowup>
{
    public void Configure(EntityTypeBuilder<EmailFollowup> b)
    {
        b.ToTable("email_followups");
        b.HasKey(f => f.Id);
        b.Property(f => f.Condition).HasConversion<string>().HasMaxLength(30);
    }
}

public class SystemSettingsConfiguration : IEntityTypeConfiguration<SystemSettings>
{
    public void Configure(EntityTypeBuilder<SystemSettings> b)
    {
        b.ToTable("system_settings");
        b.HasKey(s => s.Id);
        b.Property(s => s.FollowupRules)
            .HasColumnType("jsonb")
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v,
                    (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<Domain.Entities.FollowupRuleConfig>>(v,
                    (System.Text.Json.JsonSerializerOptions?)null) ?? new());
    }
}

public class OpportunityConfiguration : IEntityTypeConfiguration<Opportunity>
{
    public void Configure(EntityTypeBuilder<Opportunity> b)
    {
        b.ToTable("opportunities");
        b.HasKey(o => o.Id);
        
        b.Property(o => o.Id).HasColumnName("id");
        b.Property(o => o.CreatedAt).HasColumnName("created_at");
        b.Property(o => o.UpdatedAt).HasColumnName("updated_at");

        b.Property(o => o.LeadId).HasColumnName("lead_id");
        b.Property(o => o.CreatedByUserId).HasColumnName("created_by_user_id");
        b.Property(o => o.OwnerUserId).HasColumnName("owner_user_id");
        
        b.Property(o => o.Title).IsRequired().HasMaxLength(200).HasColumnName("title");
        b.Property(o => o.Description).HasMaxLength(2000).HasColumnName("description");
        
        b.Property(o => o.Type).HasConversion<string>().HasMaxLength(50).HasColumnName("type");
        b.Property(o => o.Status).HasConversion<string>().HasMaxLength(50).HasColumnName("status");
        b.Property(o => o.Priority).HasConversion<string>().HasMaxLength(50).HasColumnName("priority");
        
        b.Property(o => o.ExpectedValue).HasColumnType("numeric(18,2)").HasColumnName("expected_value");
        b.Property(o => o.ExpectedStartDate).HasColumnName("expected_start_date");
        b.Property(o => o.ExpectedEndDate).HasColumnName("expected_end_date");

        b.HasIndex(o => o.LeadId);
        b.HasIndex(o => o.OwnerUserId);
        b.HasIndex(o => o.Status);

        b.HasOne(o => o.Lead)
            .WithMany()
            .HasForeignKey(o => o.LeadId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(o => o.CreatedByUser)
            .WithMany()
            .HasForeignKey(o => o.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(o => o.OwnerUser)
            .WithMany()
            .HasForeignKey(o => o.OwnerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasMany(o => o.Documents)
            .WithOne(d => d.Opportunity)
            .HasForeignKey(d => d.OpportunityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class OpportunityDocumentConfiguration : IEntityTypeConfiguration<OpportunityDocument>
{
    public void Configure(EntityTypeBuilder<OpportunityDocument> b)
    {
        b.ToTable("opportunity_documents");
        b.HasKey(d => d.Id);

        b.Property(d => d.Id).HasColumnName("id");
        b.Property(d => d.CreatedAt).HasColumnName("created_at");
        b.Property(d => d.UpdatedAt).HasColumnName("updated_at");

        b.Property(d => d.OpportunityId).HasColumnName("opportunity_id");
        b.Property(d => d.FileName).IsRequired().HasMaxLength(255).HasColumnName("file_name");
        b.Property(d => d.FileUrl).IsRequired().HasMaxLength(1000).HasColumnName("file_url");
        b.Property(d => d.DocumentType).IsRequired().HasMaxLength(50).HasColumnName("document_type");
        b.Property(d => d.UploadedByUserId).HasColumnName("uploaded_by_user_id");

        b.HasOne(d => d.UploadedByUser)
            .WithMany()
            .HasForeignKey(d => d.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class OpportunityPositionConfiguration : IEntityTypeConfiguration<OpportunityPosition>
{
    public void Configure(EntityTypeBuilder<OpportunityPosition> b)
    {
        b.ToTable("opportunity_positions");
        b.HasKey(p => p.Id);

        // ── Primary key ──────────────────────────────────────────────────────
        b.Property(p => p.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .ValueGeneratedNever(); // Guid.NewGuid() is set in BaseEntity

        // ── Audit columns (from BaseEntity) ─────────────────────────────────
        b.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        b.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at");

        // ── Foreign key ──────────────────────────────────────────────────────
        b.Property(p => p.OpportunityId)
            .HasColumnName("opportunity_id")
            .HasColumnType("uuid")
            .IsRequired();

        // ── Core fields ──────────────────────────────────────────────────────
        b.Property(p => p.RoleTitle)
            .HasColumnName("role_title")
            .IsRequired()
            .HasMaxLength(200);

        b.Property(p => p.QuantityRequired)
            .HasColumnName("quantity_required")
            .IsRequired();

        b.Property(p => p.ExperienceMin)
            .HasColumnName("experience_min");

        b.Property(p => p.ExperienceMax)
            .HasColumnName("experience_max");

        b.Property(p => p.Skills)
            .HasColumnName("skills")
            .HasMaxLength(2000);

        b.Property(p => p.Location)
            .HasColumnName("location")
            .HasMaxLength(200);

        // ── Enum columns (stored as string) ──────────────────────────────────
        b.Property(p => p.EmploymentType)
            .HasColumnName("employment_type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        b.Property(p => p.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        // ── Indexes ──────────────────────────────────────────────────────────
        b.HasIndex(p => p.OpportunityId)
            .HasDatabaseName("ix_opportunity_positions_opportunity_id");

        b.HasIndex(p => p.Status)
            .HasDatabaseName("ix_opportunity_positions_status");

        // ── Relationships ────────────────────────────────────────────────────
        b.HasOne(p => p.Opportunity)
            .WithMany(o => o.Positions)
            .HasForeignKey(p => p.OpportunityId)
            .HasConstraintName("fk_opportunity_positions_opportunities_id")
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ResourceConfiguration : IEntityTypeConfiguration<Resource>
{
    public void Configure(EntityTypeBuilder<Resource> b)
    {
        b.ToTable("resources");
        b.HasKey(r => r.Id);

        b.Property(r => r.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .ValueGeneratedNever();

        b.Property(r => r.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uuid")
            .IsRequired();

        b.Property(r => r.FullName)
            .HasColumnName("full_name")
            .IsRequired()
            .HasMaxLength(200);

        b.Property(r => r.Email)
            .HasColumnName("email")
            .IsRequired()
            .HasMaxLength(200);

        b.Property(r => r.Phone)
            .HasColumnName("phone")
            .HasMaxLength(20);

        b.Property(r => r.TotalExperience)
            .HasColumnName("total_experience")
            .HasColumnType("numeric(4,1)");

        b.Property(r => r.CurrentLocation)
            .HasColumnName("current_location")
            .HasMaxLength(200);

        b.Property(r => r.Summary)
            .HasColumnName("summary")
            .HasMaxLength(2000);

        b.Property(r => r.Source)
            .HasColumnName("source")
            .HasMaxLength(100);

        b.Property(r => r.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(ResourceStatus.Active)
            .IsRequired();

        b.Property(r => r.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false)
            .IsRequired();

        b.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        b.Property(r => r.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired(false);

        // Required constraints & indexes
        b.HasIndex(r => new { r.UserId, r.Email }).IsUnique();
        b.HasIndex(r => r.UserId);
        b.HasIndex(r => r.Email);

        b.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

// ══════════════════════════════════════════════════════════════
// Resource Profile — child entity configurations
// ══════════════════════════════════════════════════════════════

public class ResourceEmploymentConfiguration : IEntityTypeConfiguration<ResourceEmployment>
{
    public void Configure(EntityTypeBuilder<ResourceEmployment> b)
    {
        b.ToTable("resource_employments");
        b.HasKey(e => e.Id);

        b.Property(e => e.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .ValueGeneratedNever();

        b.Property(e => e.ResourceId)
            .HasColumnName("resource_id")
            .HasColumnType("uuid")
            .IsRequired();

        b.Property(e => e.CompanyName)
            .HasColumnName("company_name")
            .IsRequired()
            .HasMaxLength(200);

        b.Property(e => e.Designation)
            .HasColumnName("designation")
            .IsRequired()
            .HasMaxLength(200);

        b.Property(e => e.EmploymentType)
            .HasColumnName("employment_type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        b.Property(e => e.StartDate)
            .HasColumnName("start_date")
            .HasColumnType("date");

        b.Property(e => e.EndDate)
            .HasColumnName("end_date")
            .HasColumnType("date");

        b.Property(e => e.IsCurrent)
            .HasColumnName("is_current")
            .HasDefaultValue(false)
            .IsRequired();

        b.Property(e => e.Responsibilities)
            .HasColumnName("responsibilities")
            .HasMaxLength(2000);

        b.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        b.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at");

        // Indexes
        b.HasIndex(e => e.ResourceId)
            .HasDatabaseName("ix_resource_employments_resource_id");

        // Relationship — cascade delete when the parent resource is removed
        b.HasOne(e => e.Resource)
            .WithMany(r => r.Employments)
            .HasForeignKey(e => e.ResourceId)
            .HasConstraintName("fk_resource_employments_resources_id")
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ResourceApplicationDetailConfiguration : IEntityTypeConfiguration<ResourceApplicationDetail>
{
    public void Configure(EntityTypeBuilder<ResourceApplicationDetail> b)
    {
        b.ToTable("resource_application_details");

        // Shared PK — resource_id IS the primary key
        b.HasKey(a => a.ResourceId);

        b.Property(a => a.ResourceId)
            .HasColumnName("resource_id")
            .HasColumnType("uuid")
            .ValueGeneratedNever();

        b.Property(a => a.CurrentCtc)
            .HasColumnName("current_ctc")
            .HasColumnType("numeric(18,2)");

        b.Property(a => a.ExpectedCtc)
            .HasColumnName("expected_ctc")
            .HasColumnType("numeric(18,2)");

        b.Property(a => a.NoticePeriodDays)
            .HasColumnName("notice_period_days");

        b.Property(a => a.PreferredLocation)
            .HasColumnName("preferred_location")
            .HasMaxLength(200);

        b.Property(a => a.AvailabilityDate)
            .HasColumnName("availability_date")
            .HasColumnType("date");

        b.Property(a => a.WillingToRelocate)
            .HasColumnName("willing_to_relocate")
            .HasDefaultValue(false)
            .IsRequired();

        b.Property(a => a.WorkModePreference)
            .HasColumnName("work_mode_preference")
            .HasConversion<string>()
            .HasMaxLength(50);

        b.Property(a => a.Skills)
            .HasColumnName("skills")
            .HasMaxLength(2000);

        b.Property(a => a.Certifications)
            .HasColumnName("certifications")
            .HasMaxLength(2000);

        b.Property(a => a.PortfolioUrl)
            .HasColumnName("portfolio_url")
            .HasMaxLength(500);

        b.Property(a => a.PositionName)
            .HasColumnName("position_name")
            .HasMaxLength(200);

        b.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        b.Property(a => a.UpdatedAt)
            .HasColumnName("updated_at");

        // 1:1 relationship — FK is also the PK
        b.HasOne(a => a.Resource)
            .WithOne(r => r.ApplicationDetail)
            .HasForeignKey<ResourceApplicationDetail>(a => a.ResourceId)
            .HasConstraintName("fk_resource_application_details_resources_id")
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ResourceReferenceConfiguration : IEntityTypeConfiguration<ResourceReference>
{
    public void Configure(EntityTypeBuilder<ResourceReference> b)
    {
        b.ToTable("resource_references");
        b.HasKey(r => r.Id);

        b.Property(r => r.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .ValueGeneratedNever();

        b.Property(r => r.ResourceId)
            .HasColumnName("resource_id")
            .HasColumnType("uuid")
            .IsRequired();

        b.Property(r => r.ReferenceType)
            .HasColumnName("reference_type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        b.Property(r => r.ReferredByUserId)
            .HasColumnName("referred_by_user_id")
            .HasColumnType("uuid");

        b.Property(r => r.ReferredByLeadId)
            .HasColumnName("referred_by_lead_id")
            .HasColumnType("uuid");

        b.Property(r => r.VendorName)
            .HasColumnName("vendor_name")
            .HasMaxLength(200);

        b.Property(r => r.PortalName)
            .HasColumnName("portal_name")
            .HasMaxLength(200);

        b.Property(r => r.ContactName)
            .HasColumnName("contact_name")
            .IsRequired()
            .HasMaxLength(200);

        b.Property(r => r.ContactPhone)
            .HasColumnName("contact_phone")
            .HasMaxLength(20);

        b.Property(r => r.ContactEmail)
            .HasColumnName("contact_email")
            .HasMaxLength(200);

        b.Property(r => r.Notes)
            .HasColumnName("notes")
            .HasMaxLength(2000);

        b.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        b.Property(r => r.UpdatedAt)
            .HasColumnName("updated_at");

        // Indexes
        b.HasIndex(r => r.ResourceId)
            .HasDatabaseName("ix_resource_references_resource_id");

        b.HasIndex(r => r.ReferenceType)
            .HasDatabaseName("ix_resource_references_reference_type");

        // Relationships
        b.HasOne(r => r.Resource)
            .WithMany(res => res.References)
            .HasForeignKey(r => r.ResourceId)
            .HasConstraintName("fk_resource_references_resources_id")
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(r => r.ReferredByUser)
            .WithMany()
            .HasForeignKey(r => r.ReferredByUserId)
            .HasConstraintName("fk_resource_references_users_id")
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        b.HasOne(r => r.ReferredByLead)
            .WithMany()
            .HasForeignKey(r => r.ReferredByLeadId)
            .HasConstraintName("fk_resource_references_leads_id")
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class ResourceDocumentConfiguration : IEntityTypeConfiguration<ResourceDocument>
{
    public void Configure(EntityTypeBuilder<ResourceDocument> b)
    {
        b.ToTable("resource_documents");
        b.HasKey(d => d.Id);

        b.Property(d => d.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .ValueGeneratedNever();

        b.Property(d => d.ResourceId)
            .HasColumnName("resource_id")
            .HasColumnType("uuid")
            .IsRequired();

        b.Property(d => d.DocumentType)
            .HasColumnName("document_type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        b.Property(d => d.KycDocumentType)
            .HasColumnName("kyc_document_type")
            .HasConversion<string>()
            .HasMaxLength(50);

        b.Property(d => d.FileName)
            .HasColumnName("file_name")
            .IsRequired()
            .HasMaxLength(300);

        b.Property(d => d.FileUrl)
            .HasColumnName("file_url")
            .IsRequired()
            .HasMaxLength(1000);

        b.Property(d => d.FileSizeBytes)
            .HasColumnName("file_size_bytes")
            .IsRequired();

        b.Property(d => d.UploadedByUserId)
            .HasColumnName("uploaded_by_user_id")
            .HasColumnType("uuid")
            .IsRequired();

        b.Property(d => d.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        b.Property(d => d.UpdatedAt)
            .HasColumnName("updated_at");

        // Indexes
        b.HasIndex(d => d.ResourceId)
            .HasDatabaseName("ix_resource_documents_resource_id");

        b.HasIndex(d => d.DocumentType)
            .HasDatabaseName("ix_resource_documents_document_type");

        // Relationships
        b.HasOne(d => d.Resource)
            .WithMany(r => r.Documents)
            .HasForeignKey(d => d.ResourceId)
            .HasConstraintName("fk_resource_documents_resources_id")
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(d => d.UploadedByUser)
            .WithMany()
            .HasForeignKey(d => d.UploadedByUserId)
            .HasConstraintName("fk_resource_documents_users_id")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
