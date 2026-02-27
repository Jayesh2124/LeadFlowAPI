using LeadFlow.Domain.Entities;
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
        b.HasIndex(l => new { l.UserId, l.Email });
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
