using FluentValidation;
using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Application.Common.Models;
using LeadFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeadFlow.Application.Features.Leads.Commands.CreateLead;

public record CreateLeadCommand(
    string FirstName, string LastName, string Email,
    string? Phone, string Company, string? Position,
    string Status, string Source, string? Notes,
    List<string> Tags) : IRequest<Result<Guid>>;

public class CreateLeadValidator : AbstractValidator<CreateLeadCommand>
{
    public CreateLeadValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Company).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Status).Must(s => new[] { "new","contacted","qualified","converted","lost" }.Contains(s))
            .WithMessage("Invalid status value.");
    }
}

public class CreateLeadHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    : IRequestHandler<CreateLeadCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateLeadCommand cmd, CancellationToken ct)
    {
        var duplicate = await db.Leads
            .AnyAsync(l => l.Email == cmd.Email && l.UserId == currentUser.UserId, ct);
        if (duplicate)
            return Result<Guid>.Failure("A lead with this email already exists.");

        var lead = Lead.Create(currentUser.UserId, cmd.FirstName, cmd.LastName,
            cmd.Email, cmd.Company, cmd.Source, cmd.Status);
        lead.Update(cmd.FirstName, cmd.LastName, cmd.Email, cmd.Phone, cmd.Company,
            cmd.Position, cmd.Status, cmd.Source, cmd.Notes, cmd.Tags ?? []);

        db.Leads.Add(lead);
        await db.SaveChangesAsync(ct);
        return Result<Guid>.Success(lead.Id);
    }
}
