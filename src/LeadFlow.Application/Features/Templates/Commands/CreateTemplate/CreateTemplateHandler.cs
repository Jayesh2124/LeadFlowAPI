using FluentValidation;
using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Application.Common.Models;
using LeadFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeadFlow.Application.Features.Templates.Commands.CreateTemplate;

public record CreateTemplateCommand(string Name, string Subject, string Body, List<string>? Attachments = null) : IRequest<Result<Guid>>;

public class CreateTemplateValidator : AbstractValidator<CreateTemplateCommand>
{
    public CreateTemplateValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Subject).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Body).NotEmpty();
    }
}

public class CreateTemplateHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    : IRequestHandler<CreateTemplateCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateTemplateCommand cmd, CancellationToken ct)
    {
        var template = EmailTemplate.Create(currentUser.UserId, cmd.Name, cmd.Subject, cmd.Body, cmd.Attachments);
        db.EmailTemplates.Add(template);
        await db.SaveChangesAsync(ct);
        return Result<Guid>.Success(template.Id);
    }
}
