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
using ApiDocuments = Acme.Documents.V1;
using ApiDrivers = Acme.Drivers.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiRouting = Acme.Routing.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Performance;

/// <summary>
/// Routes feedbacks through approval, with escalation and delegation.
/// </summary>
public sealed partial class FeedbackApprovalEndpoint : ApiPerformance.FeedbackApprovalService.FeedbackApprovalServiceBase
{
    private readonly ApiDocuments.DocumentFolderQueryService.DocumentFolderQueryServiceClient _documentsDocumentFolderQueryService;
    private readonly ApiIdentity.UserService.UserServiceClient _identityUserService;
    private readonly CallInvoker _invoker;
    private readonly ILogger<FeedbackApprovalEndpoint> _logger;
    private readonly ApiPerformance.FeedbackValidationService.FeedbackValidationServiceClient _performanceFeedbackValidationService;
    private readonly ApiPerformance.GoalApprovalService.GoalApprovalServiceClient _performanceGoalApprovalService;
    private readonly ApiPerformance.GoalQueryService.GoalQueryServiceClient _performanceGoalQueryService;
    private readonly ApiPerformance.ReviewApprovalService.ReviewApprovalServiceClient _performanceReviewApprovalService;
    private readonly ApiPerformance.ReviewService.ReviewServiceClient _performanceReviewService;
    private readonly ApiPerformance.ReviewWorkflowService.ReviewWorkflowServiceClient _performanceReviewWorkflowService;
    private readonly RetryPolicy _policy;
    private readonly ApiRecruiting.CandidateValidationService.CandidateValidationServiceClient _recruitingCandidateValidationService;
    private readonly ApiRecruiting.JobOpeningService.JobOpeningServiceClient _recruitingJobOpeningService;
    private readonly ApiRecruiting.OfferCalculationService.OfferCalculationServiceClient _recruitingOfferCalculationService;
    private readonly ApiRecruiting.OfferValidationService.OfferValidationServiceClient _recruitingOfferValidationService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiRouting.RouteCalculationService.RouteCalculationServiceClient _routingRouteCalculationService;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTravel.TravelPolicyReportingService.TravelPolicyReportingServiceClient _travelTravelPolicyReportingService;
    private readonly ApiWorkplaces.RoomBookingReportingService.RoomBookingReportingServiceClient _workplacesRoomBookingReportingService;

    public FeedbackApprovalEndpoint(
        ApiDocuments.DocumentFolderQueryService.DocumentFolderQueryServiceClient documentsDocumentFolderQueryService,
        ApiIdentity.UserService.UserServiceClient identityUserService,
        CallInvoker invoker,
        ILogger<FeedbackApprovalEndpoint> logger,
        ApiPerformance.FeedbackValidationService.FeedbackValidationServiceClient performanceFeedbackValidationService,
        ApiPerformance.GoalApprovalService.GoalApprovalServiceClient performanceGoalApprovalService,
        ApiPerformance.GoalQueryService.GoalQueryServiceClient performanceGoalQueryService,
        ApiPerformance.ReviewApprovalService.ReviewApprovalServiceClient performanceReviewApprovalService,
        ApiPerformance.ReviewService.ReviewServiceClient performanceReviewService,
        ApiPerformance.ReviewWorkflowService.ReviewWorkflowServiceClient performanceReviewWorkflowService,
        RetryPolicy policy,
        ApiRecruiting.CandidateValidationService.CandidateValidationServiceClient recruitingCandidateValidationService,
        ApiRecruiting.JobOpeningService.JobOpeningServiceClient recruitingJobOpeningService,
        ApiRecruiting.OfferCalculationService.OfferCalculationServiceClient recruitingOfferCalculationService,
        ApiRecruiting.OfferValidationService.OfferValidationServiceClient recruitingOfferValidationService,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiRouting.RouteCalculationService.RouteCalculationServiceClient routingRouteCalculationService,
        IOptions<MeshOptions> settings,
        ApiTravel.TravelPolicyReportingService.TravelPolicyReportingServiceClient travelTravelPolicyReportingService,
        ApiWorkplaces.RoomBookingReportingService.RoomBookingReportingServiceClient workplacesRoomBookingReportingService)
    {
        _documentsDocumentFolderQueryService = documentsDocumentFolderQueryService;
        _identityUserService = identityUserService;
        _invoker = invoker;
        _logger = logger;
        _performanceFeedbackValidationService = performanceFeedbackValidationService;
        _performanceGoalApprovalService = performanceGoalApprovalService;
        _performanceGoalQueryService = performanceGoalQueryService;
        _performanceReviewApprovalService = performanceReviewApprovalService;
        _performanceReviewService = performanceReviewService;
        _performanceReviewWorkflowService = performanceReviewWorkflowService;
        _policy = policy;
        _recruitingCandidateValidationService = recruitingCandidateValidationService;
        _recruitingJobOpeningService = recruitingJobOpeningService;
        _recruitingOfferCalculationService = recruitingOfferCalculationService;
        _recruitingOfferValidationService = recruitingOfferValidationService;
        _registry = registry;
        _repository = repository;
        _routingRouteCalculationService = routingRouteCalculationService;
        _settings = settings;
        _travelTravelPolicyReportingService = travelTravelPolicyReportingService;
        _workplacesRoomBookingReportingService = workplacesRoomBookingReportingService;
    }
}
