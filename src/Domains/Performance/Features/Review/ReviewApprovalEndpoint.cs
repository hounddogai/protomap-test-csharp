using Acme.Workforce.Platform;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSearch = Acme.Search.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Performance;

/// <summary>
/// Routes reviews through approval, with escalation and delegation.
/// </summary>
public sealed partial class ReviewApprovalEndpoint : ApiPerformance.ReviewApprovalService.ReviewApprovalServiceBase
{
    private readonly ApiCorporateevents.VenueSyncService.VenueSyncServiceClient _corporateeventsVenueSyncService;
    private readonly ApiDocuments.DocumentFolderHistoryService.DocumentFolderHistoryServiceClient _documentsDocumentFolderHistoryService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.IncidentWorkflowService.IncidentWorkflowServiceClient _itsmIncidentWorkflowService;
    private readonly ApiItsm.ProblemReportingService.ProblemReportingServiceClient _itsmProblemReportingService;
    private readonly ILogger<ReviewApprovalEndpoint> _logger;
    private readonly ApiPerformance.FeedbackValidationService.FeedbackValidationServiceClient _performanceFeedbackValidationService;
    private readonly ApiPerformance.GoalValidationService.GoalValidationServiceClient _performanceGoalValidationService;
    private readonly ApiPerformance.ReviewQueryService.ReviewQueryServiceClient _performanceReviewQueryService;
    private readonly ApiRecruiting.OfferValidationService.OfferValidationServiceClient _recruitingOfferValidationService;
    private readonly IRecordRepository _repository;
    private readonly IReviewApprovalEndpointSendGetReviewApprovalStatus0Port _reviewApprovalEndpointSendGetReviewApprovalStatus0Port;
    private readonly ApiSearch.SearchIndexQueryService.SearchIndexQueryServiceClient _searchSearchIndexQueryService;
    private readonly ApiTravel.ItineraryReportingService.ItineraryReportingServiceClient _travelItineraryReportingService;

    public ReviewApprovalEndpoint(
        ApiCorporateevents.VenueSyncService.VenueSyncServiceClient corporateeventsVenueSyncService,
        ApiDocuments.DocumentFolderHistoryService.DocumentFolderHistoryServiceClient documentsDocumentFolderHistoryService,
        CallInvoker invoker,
        ApiItsm.IncidentWorkflowService.IncidentWorkflowServiceClient itsmIncidentWorkflowService,
        ApiItsm.ProblemReportingService.ProblemReportingServiceClient itsmProblemReportingService,
        ILogger<ReviewApprovalEndpoint> logger,
        ApiPerformance.FeedbackValidationService.FeedbackValidationServiceClient performanceFeedbackValidationService,
        ApiPerformance.GoalValidationService.GoalValidationServiceClient performanceGoalValidationService,
        ApiPerformance.ReviewQueryService.ReviewQueryServiceClient performanceReviewQueryService,
        ApiRecruiting.OfferValidationService.OfferValidationServiceClient recruitingOfferValidationService,
        IRecordRepository repository,
        IReviewApprovalEndpointSendGetReviewApprovalStatus0Port reviewApprovalEndpointSendGetReviewApprovalStatus0PortAdapter,
        ApiSearch.SearchIndexQueryService.SearchIndexQueryServiceClient searchSearchIndexQueryService,
        ApiTravel.ItineraryReportingService.ItineraryReportingServiceClient travelItineraryReportingService)
    {
        _corporateeventsVenueSyncService = corporateeventsVenueSyncService;
        _documentsDocumentFolderHistoryService = documentsDocumentFolderHistoryService;
        _invoker = invoker;
        _itsmIncidentWorkflowService = itsmIncidentWorkflowService;
        _itsmProblemReportingService = itsmProblemReportingService;
        _logger = logger;
        _performanceFeedbackValidationService = performanceFeedbackValidationService;
        _performanceGoalValidationService = performanceGoalValidationService;
        _performanceReviewQueryService = performanceReviewQueryService;
        _recruitingOfferValidationService = recruitingOfferValidationService;
        _repository = repository;
        _reviewApprovalEndpointSendGetReviewApprovalStatus0Port = reviewApprovalEndpointSendGetReviewApprovalStatus0PortAdapter;
        _searchSearchIndexQueryService = searchSearchIndexQueryService;
        _travelItineraryReportingService = travelItineraryReportingService;
    }
}
