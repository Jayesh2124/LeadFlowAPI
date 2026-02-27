using LeadFlow.Domain.Common;
using LeadFlow.Domain.Enums;

namespace LeadFlow.Domain.Entities;

public class EmailFollowup : BaseEntity
{
    public Guid EmailTaskId { get; private set; }
    public Guid TemplateId { get; private set; }
    public int DelayDays { get; private set; }
    public FollowupCondition Condition { get; private set; }
    public int Order { get; private set; }
    public bool IsEnabled { get; private set; } = true;
    public bool Generated { get; private set; }  // true once a child EmailTask is created

    public EmailTask EmailTask { get; private set; } = null!;

    protected EmailFollowup() { }

    public static EmailFollowup Create(Guid emailTaskId, Guid templateId,
        int delayDays, FollowupCondition condition, int order)
        => new EmailFollowup
        {
            EmailTaskId = emailTaskId, TemplateId = templateId,
            DelayDays = delayDays, Condition = condition, Order = order
        };

    public void MarkGenerated() { Generated = true; }
}
