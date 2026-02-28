using LeadFlow.Domain.Common;

namespace LeadFlow.Domain.Entities;

public class OpportunityDocument : BaseEntity
{
    public Guid OpportunityId { get; private set; }
    public string FileName { get; private set; } = default!;
    public string FileUrl { get; private set; } = default!;
    public string DocumentType { get; private set; } = default!;
    public Guid UploadedByUserId { get; private set; }

    // Navigation properties
    public Opportunity Opportunity { get; private set; } = null!;
    public User UploadedByUser { get; private set; } = null!;

    protected OpportunityDocument() { }

    public static OpportunityDocument Create(
        Guid opportunityId, 
        string fileName, 
        string fileUrl, 
        string documentType, 
        Guid uploadedByUserId)
    {
        return new OpportunityDocument
        {
            OpportunityId = opportunityId,
            FileName = fileName,
            FileUrl = fileUrl,
            DocumentType = documentType,
            UploadedByUserId = uploadedByUserId
        };
    }
}
