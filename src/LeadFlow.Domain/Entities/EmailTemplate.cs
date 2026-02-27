using System.Text.RegularExpressions;
using LeadFlow.Domain.Common;

namespace LeadFlow.Domain.Entities;

public class EmailTemplate : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = default!;
    public string Subject { get; private set; } = default!;
    public string Body { get; private set; } = default!;   // HTML
    public List<string> Variables { get; private set; } = [];
    public List<string> Attachments { get; private set; } = [];
    public bool IsActive { get; private set; } = true;
    public int UsageCount { get; private set; }

    public User User { get; private set; } = null!;

    protected EmailTemplate() { }

    public static EmailTemplate Create(Guid userId, string name, string subject, string body, List<string>? attachments = null)
        => new EmailTemplate
        {
            UserId = userId, Name = name, Subject = subject, Body = body,
            Attachments = attachments ?? [],
            Variables = ExtractVariables(subject + body)
        };

    public void Update(string name, string subject, string body, List<string>? attachments = null)
    {
        Name = name; Subject = subject; Body = body;
        Attachments = attachments ?? [];
        Variables = ExtractVariables(subject + body);
        Touch();
    }

    public void SetActive(bool active) { IsActive = active; Touch(); }
    public void IncrementUsage() { UsageCount++; Touch(); }

    private static List<string> ExtractVariables(string content)
        => Regex.Matches(content, @"\{\{(\w+)\}\}")
                .Select(m => m.Groups[1].Value)
                .Distinct()
                .ToList();
}
