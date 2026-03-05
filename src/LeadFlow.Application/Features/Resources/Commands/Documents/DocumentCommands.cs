using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Application.Features.Resources.DTOs;
using LeadFlow.Domain.Entities;
using LeadFlow.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeadFlow.Application.Features.Resources.Commands.Documents;

// ─── Upload Document ──────────────────────────────────────────

public record UploadDocumentCommand(
    Guid ResourceId,
    ResourceDocumentType DocumentType,
    KycDocumentType? KycDocumentType,
    string FileName,
    string FileUrl,
    long FileSizeBytes) : IRequest<Guid>;

public class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _user;

    public UploadDocumentCommandHandler(IApplicationDbContext db, ICurrentUserService user)
    {
        _db = db;
        _user = user;
    }

    public async Task<Guid> Handle(UploadDocumentCommand request, CancellationToken ct)
    {
        var resource = await _db.Resources
            .FirstOrDefaultAsync(r => r.Id == request.ResourceId && !r.IsDeleted, ct);

        if (resource is null) throw new Exception("Resource not found.");
        if (!_user.IsAdmin && resource.UserId != _user.UserId)
            throw new UnauthorizedAccessException("You do not have permission.");

        var doc = ResourceDocument.Create(
            resourceId:       request.ResourceId,
            documentType:     request.DocumentType,
            fileName:         request.FileName,
            fileUrl:          request.FileUrl,
            fileSizeBytes:    request.FileSizeBytes,
            uploadedByUserId: _user.UserId,
            kycDocumentType:  request.KycDocumentType);

        _db.ResourceDocuments.Add(doc);
        await _db.SaveChangesAsync(ct);
        return doc.Id;
    }
}

// ─── Delete Document ──────────────────────────────────────────

public record DeleteDocumentCommand(Guid DocumentId) : IRequest;

public class DeleteDocumentCommandHandler : IRequestHandler<DeleteDocumentCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _user;

    public DeleteDocumentCommandHandler(IApplicationDbContext db, ICurrentUserService user)
    {
        _db = db;
        _user = user;
    }

    public async Task Handle(DeleteDocumentCommand request, CancellationToken ct)
    {
        var doc = await _db.ResourceDocuments
            .Include(d => d.Resource)
            .FirstOrDefaultAsync(d => d.Id == request.DocumentId, ct);

        if (doc is null) throw new Exception("Document not found.");
        if (!_user.IsAdmin && doc.Resource.UserId != _user.UserId)
            throw new UnauthorizedAccessException("You do not have permission.");

        _db.ResourceDocuments.Remove(doc);
        await _db.SaveChangesAsync(ct);
    }
}
