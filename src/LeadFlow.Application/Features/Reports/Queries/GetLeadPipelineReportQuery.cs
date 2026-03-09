using System;
using MediatR;
using LeadFlow.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using LeadFlow.Domain.Enums;

namespace LeadFlow.Application.Features.Reports.Queries;

public class GetLeadPipelineReportQuery : IRequest<LeadPipelineReportDto>
{
    public Guid? LeadId { get; }
    public Guid? OpportunityId { get; }
    public string? Stage { get; }
    public DateTime? DateFrom { get; }
    public DateTime? DateTo { get; }

    public GetLeadPipelineReportQuery(
        Guid? leadId,
        Guid? opportunityId,
        string? stage,
        DateTime? dateFrom,
        DateTime? dateTo)
    {
        LeadId = leadId;
        OpportunityId = opportunityId;
        Stage = stage;
        DateFrom = dateFrom;
        DateTo = dateTo;
    }
}

public class LeadPipelineReportDto
{
    public LeadPipelineSummaryDto Summary { get; }
    public List<LeadPipelineResourceDto> Resources { get; }

    public LeadPipelineReportDto(LeadPipelineSummaryDto summary, List<LeadPipelineResourceDto> resources)
    {
        Summary = summary;
        Resources = resources;
    }
}

public class LeadPipelineSummaryDto
{
    public int TotalOpportunities { get; }
    public int TotalPositions { get; }
    public int TotalResources { get; }
    public int SelectedCount { get; }
    public int RejectedCount { get; }
    public int PipelineCount { get; }

    public LeadPipelineSummaryDto(
        int totalOpportunities,
        int totalPositions,
        int totalResources,
        int selectedCount,
        int rejectedCount,
        int pipelineCount)
    {
        TotalOpportunities = totalOpportunities;
        TotalPositions = totalPositions;
        TotalResources = totalResources;
        SelectedCount = selectedCount;
        RejectedCount = rejectedCount;
        PipelineCount = pipelineCount;
    }
}

public class LeadPipelineResourceDto
{
    public string LeadName { get; }
    public string OpportunityName { get; }
    public string? PositionTitle { get; }
    public string? ResourceName { get; }
    public decimal? Experience { get; }
    public string Stage { get; }
    public string? InterviewerName { get; }
    public DateTime? InterviewDate { get; }
    public string? Result { get; }
    public DateTime? AssignedDate { get; }

    public LeadPipelineResourceDto(
        string leadName,
        string opportunityName,
        string? positionTitle,
        string? resourceName,
        decimal? experience,
        string stage,
        string? interviewerName,
        DateTime? interviewDate,
        string? result,
        DateTime? assignedDate)
    {
        LeadName = leadName;
        OpportunityName = opportunityName;
        PositionTitle = positionTitle;
        ResourceName = resourceName;
        Experience = experience;
        Stage = stage;
        InterviewerName = interviewerName;
        InterviewDate = interviewDate;
        Result = result;
        AssignedDate = assignedDate;
    }
}

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
                    join pos in _context.OpportunityPositions.AsNoTracking() on opp.Id equals pos.OpportunityId into posGroup
                    from pos in posGroup.DefaultIfEmpty()
                    join assign in _context.ResourceAssignments.AsNoTracking() on pos.Id equals assign.PositionId into assignGroup
                    from assign in assignGroup.DefaultIfEmpty()
                    join res in _context.Resources.AsNoTracking() on assign.ResourceId equals res.Id into resGroup
                    from res in resGroup.DefaultIfEmpty()
                    join interview in _context.AssignmentInterviews.AsNoTracking() on (assign != null ? assign.Id : Guid.Empty) equals interview.AssignmentId into interviews
                    from interview in interviews.DefaultIfEmpty()
                    select new
                    {
                        LeadId = lead.Id,
                        LeadName = lead.FirstName + " " + lead.LastName,
                        OpportunityId = opp.Id,
                        OpportunityName = opp.Title,
                        PositionId = (Guid?)pos.Id,
                        PositionTitle = pos != null ? pos.RoleTitle : null,
                        ResourceId = (Guid?)res.Id,
                        ResourceName = res != null ? res.FullName : null,
                        Experience = res != null ? (decimal?)res.TotalExperience : null,
                        Stage = assign != null ? (AssignmentStage?)assign.Stage : null,
                        AssignedDate = assign != null ? (DateTime?)assign.AssignedAt : null,
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
            x.Stage.HasValue ? x.Stage.Value.ToString() : "Not Allocated",
            x.InterviewerName,
            x.InterviewDate,
            x.Result,
            x.AssignedDate
        )).ToList();

        // Calculate summaries
        var distinctOpportunitiesCount = results.Select(x => x.OpportunityId).Distinct().Count();
        var distinctPositionsCount = results.Select(x => x.PositionId).Where(id => id.HasValue).Distinct().Count();
        var distinctResourcesCount = results.Select(x => x.ResourceId).Where(id => id.HasValue).Distinct().Count();
        
        var selectedCount = results.Count(x => x.Stage == AssignmentStage.Selected || x.Stage == AssignmentStage.Onboarded);
        var rejectedCount = results.Count(x => x.Stage == AssignmentStage.Rejected);
        
        var pipelineCount = results.Count(x => x.Stage.HasValue && 
                                               x.Stage != AssignmentStage.Selected && 
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
