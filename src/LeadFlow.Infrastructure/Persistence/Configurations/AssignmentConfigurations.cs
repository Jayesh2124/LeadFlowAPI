using LeadFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeadFlow.Infrastructure.Persistence.Configurations;

public class ResourceAssignmentConfiguration : IEntityTypeConfiguration<ResourceAssignment>
{
    public void Configure(EntityTypeBuilder<ResourceAssignment> b)
    {
        b.ToTable("resource_assignments");
        b.HasKey(e => e.Id);

        b.Property(e => e.Stage)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        b.Property(e => e.Status)
            .IsRequired()
            .HasMaxLength(50);

        b.Property(e => e.Notes)
            .HasMaxLength(2000);

        // Unique constraint: A resource cannot be assigned twice to the same position.
        b.HasIndex(e => new { e.PositionId, e.ResourceId }).IsUnique();

        // Indexes
        b.HasIndex(e => e.PositionId);
        b.HasIndex(e => e.ResourceId);
        b.HasIndex(e => e.Stage);

        // Relationships and Cascade behavior
        // If position deleted -> assignments deleted
        b.HasOne(e => e.Position)
            .WithMany()
            .HasForeignKey(e => e.PositionId)
            .OnDelete(DeleteBehavior.Cascade);

        // If resource deleted -> prevent delete
        b.HasOne(e => e.Resource)
            .WithMany()
            .HasForeignKey(e => e.ResourceId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(e => e.AssignedByUser)
            .WithMany()
            .HasForeignKey(e => e.AssignedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class AssignmentInterviewConfiguration : IEntityTypeConfiguration<AssignmentInterview>
{
    public void Configure(EntityTypeBuilder<AssignmentInterview> b)
    {
        b.ToTable("assignment_interviews");
        b.HasKey(e => e.Id);

        b.Property(e => e.InterviewStage)
            .IsRequired()
            .HasMaxLength(50);

        b.Property(e => e.InterviewerName)
            .HasMaxLength(200);

        b.Property(e => e.InterviewerEmail)
            .HasMaxLength(200);

        b.Property(e => e.Status)
            .IsRequired()
            .HasMaxLength(50);

        b.Property(e => e.Feedback)
            .HasMaxLength(2000);

        b.HasIndex(e => e.AssignmentId);
        b.HasIndex(e => e.InterviewStage);

        b.HasOne(e => e.Assignment)
            .WithMany(a => a.Interviews)
            .HasForeignKey(e => e.AssignmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class AssignmentStageHistoryConfiguration : IEntityTypeConfiguration<AssignmentStageHistory>
{
    public void Configure(EntityTypeBuilder<AssignmentStageHistory> b)
    {
        b.ToTable("assignment_stage_history");
        b.HasKey(e => e.Id);

        b.Property(e => e.PreviousStage)
            .HasMaxLength(50);

        b.Property(e => e.NewStage)
            .IsRequired()
            .HasMaxLength(50);

        b.HasIndex(e => e.AssignmentId);

        b.HasOne(e => e.Assignment)
            .WithMany(a => a.StageHistories)
            .HasForeignKey(e => e.AssignmentId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(e => e.ChangedByUser)
            .WithMany()
            .HasForeignKey(e => e.ChangedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
