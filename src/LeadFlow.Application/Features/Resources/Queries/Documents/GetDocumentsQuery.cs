using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Application.Features.Resources.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeadFlow.Application.Features.Resources.Queries.Documents;

public record GetDocumentsQuery(Guid ResourceId) : IRequest<List<DocumentResponse>>;

public class GetDocumentsQueryHandler : IRequestHandler<GetDocumentsQuery, List<DocumentResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _user;

    public GetDocumentsQueryHandler(IApplicationDbContext db, ICurrentUserService user)
    {
        _db   = db;
        _user = user;
    }

    public async Task<List<DocumentResponse>> Handle(GetDocumentsQuery request, CancellationToken ct)
    {
        var resource = await _db.Resources
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.ResourceId && !r.IsDeleted, ct);

        if (resource is null) throw new Exception("Resource not found.");
        if (!_user.IsAdmin && resource.UserId != _user.UserId)
            throw new UnauthorizedAccessException("You do not have permission.");

        return await _db.ResourceDocuments
            .AsNoTracking()
            .Where(d => d.ResourceId == request.ResourceId)
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new DocumentResponse(
                d.Id, d.ResourceId, d.DocumentType, d.KycDocumentType,
                d.FileName, d.FileUrl, d.FileSizeBytes,
                d.UploadedByUserId, d.CreatedAt))
            .ToListAsync(ct);
    }
}
