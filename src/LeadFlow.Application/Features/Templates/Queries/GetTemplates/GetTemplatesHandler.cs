using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeadFlow.Application.Features.Templates.Queries.GetTemplates;

public record GetTemplatesQuery(string? Search = null, int Page = 1, int PageSize = 50)
    : IRequest<Result<List<TemplateDto>>>;

public record TemplateDto(Guid Id, string Name, string Subject, string Body,
    List<string> Variables, List<string> Attachments, int UsageCount, string Status, DateTime CreatedAt);

public class GetTemplatesHandler(IApplicationDbContext db)
    : IRequestHandler<GetTemplatesQuery, Result<List<TemplateDto>>>
{
    public async Task<Result<List<TemplateDto>>> Handle(GetTemplatesQuery q, CancellationToken ct)
    {
        // All created templates should display to every user/admin
        var query = db.EmailTemplates.AsQueryable();

        if (!string.IsNullOrWhiteSpace(q.Search))
            query = query.Where(t => t.Name.Contains(q.Search));

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .Select(t => new TemplateDto(t.Id, t.Name, t.Subject, t.Body,
                t.Variables ?? new List<string>(), t.Attachments ?? new List<string>(), t.UsageCount, t.IsActive ? "active" : "draft", t.CreatedAt))
            .ToListAsync(ct);

        return Result<List<TemplateDto>>.Success(items);
    }
}
