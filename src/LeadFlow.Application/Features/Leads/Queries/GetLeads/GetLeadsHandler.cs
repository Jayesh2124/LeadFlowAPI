using System;
using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeadFlow.Application.Features.Leads.Queries.GetLeads;

public class GetLeadsQuery : IRequest<Result<PagedLeadsResult>>
{
    public string? Search { get; }
    public string? Status { get; }
    public int Page { get; }
    public int PageSize { get; }

    public GetLeadsQuery(
        string? search = null,
        string? status = null,
        int page = 1,
        int pageSize = 20)
    {
        Search = search;
        Status = status;
        Page = page;
        PageSize = pageSize;
    }
}

public class LeadDto
{
    public Guid Id { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public string Email { get; }
    public string? Phone { get; }
    public string Company { get; }
    public string? Position { get; }
    public string Status { get; }
    public string Source { get; }
    public string? Notes { get; }
    public List<string> Tags { get; }
    public bool IsActive { get; }
    public DateTime CreatedAt { get; }
    public DateTime? UpdatedAt { get; }
    public string Country { get; }
    public string? City { get; }
    public string? State { get; }
    public string? Address { get; }
    public string? ZipCode { get; }
    public string? Website { get; }
    public List<string> Technologies { get; }

    public LeadDto(
        Guid Id, string FirstName, string LastName, string Email,
        string? Phone, string Company, string? Position, string Status,
        string Source, string? Notes, List<string> Tags, bool IsActive,
        DateTime CreatedAt, DateTime? UpdatedAt, string Country, string? City,
        string? State, string? Address, string? ZipCode, string? Website,
        List<string> Technologies)
    {
        this.Id = Id;
        this.FirstName = FirstName;
        this.LastName = LastName;
        this.Email = Email;
        this.Phone = Phone;
        this.Company = Company;
        this.Position = Position;
        this.Status = Status;
        this.Source = Source;
        this.Notes = Notes;
        this.Tags = Tags;
        this.IsActive = IsActive;
        this.CreatedAt = CreatedAt;
        this.UpdatedAt = UpdatedAt;
        this.Country = Country;
        this.City = City;
        this.State = State;
        this.Address = Address;
        this.ZipCode = ZipCode;
        this.Website = Website;
        this.Technologies = Technologies;
    }
}

public class PagedLeadsResult
{
    public List<LeadDto> Items { get; }
    public int Total { get; }
    public int Page { get; }
    public int PageSize { get; }

    public PagedLeadsResult(List<LeadDto> Items, int Total, int Page, int PageSize)
    {
        this.Items = Items;
        this.Total = Total;
        this.Page = Page;
        this.PageSize = PageSize;
    }
}

public class GetLeadsHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    : IRequestHandler<GetLeadsQuery, Result<PagedLeadsResult>>
{
    public async Task<Result<PagedLeadsResult>> Handle(GetLeadsQuery q, CancellationToken ct)
    {
        var query = db.Leads.AsQueryable();

        if (currentUser.IsAdmin)
        {
            // Admin sees everything
        }
        else
        {
            // User sees only own active leads
            query = query.Where(l => l.UserId == currentUser.UserId && l.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(q.Status))
            query = query.Where(l => l.Status == q.Status);

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var s = q.Search.ToLower();
            query = query.Where(l =>
                l.FirstName.ToLower().Contains(s) ||
                l.LastName.ToLower().Contains(s) ||
                l.Email.ToLower().Contains(s) ||
                l.Company.ToLower().Contains(s));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .Select(l => new LeadDto(l.Id, l.FirstName, l.LastName, l.Email,
                l.Phone, l.Company, l.Position, l.Status, l.Source,
                l.Notes, l.Tags, l.IsActive, l.CreatedAt, l.UpdatedAt,
                l.Country, l.City, l.State, l.Address, l.ZipCode, l.Website,
                l.Technologies))
            .ToListAsync(ct);

        return Result<PagedLeadsResult>.Success(new PagedLeadsResult(items, total, q.Page, q.PageSize));
    }
}
