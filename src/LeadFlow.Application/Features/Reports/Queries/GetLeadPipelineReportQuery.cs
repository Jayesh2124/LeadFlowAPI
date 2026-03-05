using MediatR;
using LeadFlow.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using LeadFlow.Domain.Enums;

namespace LeadFlow.Application.Features.Reports.Queries;

public record GetLeadPipelineReportQuery(
    Guid? LeadId,
    Guid? OpportunityId,
    string? Stage,
    DateTime? DateFrom,
    DateTime? DateTo) : IRequest<LeadPipelineReportDto>;

public record LeadPipelineReportDto(
    LeadPipelineSummaryDto Summary,
    List<LeadPipelineResourceDto> Resources
);

public record LeadPipelineSummaryDto(
    int TotalOpportunities,
    int TotalPositions,
    int TotalResources,
    int SelectedCount,
    int RejectedCount,
    int PipelineCount
);

public record LeadPipelineResourceDto(
    string LeadName,
    string OpportunityName,
    string PositionTitle,
    string ResourceName,
    decimal? Experience,
    string Stage,
    string? InterviewerName,
    DateTime? InterviewDate,
    string? Result,
    DateTime AssignedDate
);

public class GetLeadPipelineReportQueryHandler : IRequestHandler<GetLeadPipelineReportQuery, LeadPipelineReportDto>
{
    private readonly IApplicationDbContext _context;

    public GetLeadPipelineReportQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LeadPipelineReportDto> Handle(GetLeadPipelineReportQuery request, CancellationToken cancellationToken)
    {
        var query = from lead in _context.Leads.AsNoTracking()
                    join opp in _context.Opportunities.AsNoTracking() on lead.Id equals opp.LeadId
                    join pos in _context.OpportunityPositions.AsNoTracking() on opp.Id equals pos.OpportunityId
                    join assign in _context.ResourceAssignments.AsNoTracking() on pos.Id equals assign.PositionId
                    join res in _context.Resources.AsNoTracking() on assign.ResourceId equals res.Id
                    join interview in _context.AssignmentInterviews.AsNoTracking() on assign.Id equals interview.AssignmentId into interviews
                    from interview in interviews.DefaultIfEmpty()
                    select new
                    {
                        LeadId = lead.Id,
                        LeadName = lead.FirstName + " " + lead.LastName,
                        OpportunityId = opp.Id,
                        OpportunityName = opp.Title,
                        PositionId = pos.Id,
                        PositionTitle = pos.RoleTitle,
                        ResourceId = res.Id,
                        ResourceName = res.FullName,
                        Experience = res.TotalExperience,
                        Stage = assign.Stage,
                        AssignedDate = assign.AssignedAt,
                        InterviewerName = interview != null ? interview.InterviewerName : null,
                        InterviewDate = interview != null ? interview.ScheduledAt : (DateTime?)null,
                        Result = interview != null ? interview.Status : null
                    };

        if (request.LeadId.HasValue)
        {
            query = query.Where(x => x.LeadId == request.LeadId.Value);
        }

        if (request.OpportunityId.HasValue)
        {
            query = query.Where(x => x.OpportunityId == request.OpportunityId.Value);
        }

        if (!string.IsNullOrEmpty(request.Stage))
        {
            if (Enum.TryParse<AssignmentStage>(request.Stage, true, out var stageEnum))
            {
                query = query.Where(x => x.Stage == stageEnum);
            }
        }

        if (request.DateFrom.HasValue)
        {
            query = query.Where(x => x.AssignedDate >= request.DateFrom.Value);
        }

        if (request.DateTo.HasValue)
        {
            query = query.Where(x => x.AssignedDate <= request.DateTo.Value);
        }

        var results = await query.ToListAsync(cancellationToken);

        var resourceList = results.Select(x => new LeadPipelineResourceDto(
            x.LeadName,
            x.OpportunityName,
            x.PositionTitle,
            x.ResourceName,
            x.Experience,
            x.Stage.ToString(),
            x.InterviewerName,
            x.InterviewDate,
            x.Result,
            x.AssignedDate
        )).ToList();

        // Calculate summaries
        var distinctOpportunitiesCount = results.Select(x => x.OpportunityId).Distinct().Count();
        var distinctPositionsCount = results.Select(x => x.PositionId).Distinct().Count();
        var distinctResourcesCount = results.Select(x => x.ResourceId).Distinct().Count();
        
        var selectedCount = results.Count(x => x.Stage == AssignmentStage.Selected || x.Stage == AssignmentStage.Onboarded);
        var rejectedCount = results.Count(x => x.Stage == AssignmentStage.Rejected);
        
        var pipelineCount = results.Count(x => x.Stage != AssignmentStage.Selected && 
                                               x.Stage != AssignmentStage.Onboarded &&
                                               x.Stage != AssignmentStage.Rejected);

        var summary = new LeadPipelineSummaryDto(
            distinctOpportunitiesCount,
            distinctPositionsCount,
            distinctResourcesCount,
            selectedCount,
            rejectedCount,
            pipelineCount
        );

        return new LeadPipelineReportDto(summary, resourceList);
    }
}
