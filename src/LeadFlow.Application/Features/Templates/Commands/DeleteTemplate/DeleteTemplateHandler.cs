using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeadFlow.Application.Features.Templates.Commands.DeleteTemplate;

public record DeleteTemplateCommand(Guid Id) : IRequest<Result>;

public class DeleteTemplateHandler(IApplicationDbContext db)
    : IRequestHandler<DeleteTemplateCommand, Result>
{
    public async Task<Result> Handle(DeleteTemplateCommand cmd, CancellationToken ct)
    {
        var template = await db.EmailTemplates.FindAsync(new object[] { cmd.Id }, ct);
        if (template is null) return Result.Failure("Template not found");

        db.EmailTemplates.Remove(template);
        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
