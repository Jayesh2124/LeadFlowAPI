using LeadFlow.Domain.Common;
using LeadFlow.Domain.Enums;

namespace LeadFlow.Domain.Entities;

public class SystemSettings : BaseEntity
{
    public int DefaultMaxRetries { get; private set; } = 3;
    public int RetryDelayBaseHours { get; private set; } = 1;
    public bool AutoFollowup { get; private set; } = true;

    // Stored as JSON column
    public List<FollowupRuleConfig> FollowupRules { get; private set; } = [];

    protected SystemSettings() { }

    public static SystemSettings CreateDefault() => new SystemSettings();

    public void Update(int maxRetries, int retryDelayBase,
        bool autoFollowup, List<FollowupRuleConfig> rules)
    {
        DefaultMaxRetries = maxRetries;
        RetryDelayBaseHours = retryDelayBase;
        AutoFollowup = autoFollowup;
        FollowupRules = rules;
        Touch();
    }
}

public record FollowupRuleConfig(
    string Name,
    int DelayDays,
    FollowupCondition Condition,
    Guid? TemplateId,
    int Order,
    bool Enabled);
