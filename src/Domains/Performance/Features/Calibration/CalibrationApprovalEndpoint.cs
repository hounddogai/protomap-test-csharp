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
using ApiBroadcasts = Acme.Broadcasts.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Performance;

/// <summary>
/// Routes calibrations through approval, with escalation and delegation.
/// </summary>
public sealed partial class CalibrationApprovalEndpoint : ApiPerformance.CalibrationApprovalService.CalibrationApprovalServiceBase
{
    private readonly ApiBroadcasts.BroadcastService.BroadcastServiceClient _broadcastsBroadcastService;
    private readonly ICalibrationApprovalEndpointSendSubmitCalibration1Port _calibrationApprovalEndpointSendSubmitCalibration1Port;
    private readonly ApiCorporateevents.VenueReportingService.VenueReportingServiceClient _corporateeventsVenueReportingService;
    private readonly ApiDocuments.DocumentExportService.DocumentExportServiceClient _documentsDocumentExportService;
    private readonly ApiIdentity.UserQueryService.UserQueryServiceClient _identityUserQueryService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.ChangeRequestReportingService.ChangeRequestReportingServiceClient _itsmChangeRequestReportingService;
    private readonly ILogger<CalibrationApprovalEndpoint> _logger;
    private readonly ApiPerformance.CalibrationLifecycleService.CalibrationLifecycleServiceClient _performanceCalibrationLifecycleService;
    private readonly ApiPerformance.FeedbackApprovalService.FeedbackApprovalServiceClient _performanceFeedbackApprovalService;
    private readonly ApiPerformance.GoalWorkflowService.GoalWorkflowServiceClient _performanceGoalWorkflowService;
    private readonly ApiPerformance.ReviewApprovalService.ReviewApprovalServiceClient _performanceReviewApprovalService;
    private readonly ApiRecruiting.CandidateCalculationService.CandidateCalculationServiceClient _recruitingCandidateCalculationService;
    private readonly ApiRecruiting.InterviewCalculationService.InterviewCalculationServiceClient _recruitingInterviewCalculationService;
    private readonly ApiRecruiting.JobOpeningQueryService.JobOpeningQueryServiceClient _recruitingJobOpeningQueryService;
    private readonly ApiRecruiting.OfferValidationService.OfferValidationServiceClient _recruitingOfferValidationService;
    private readonly IRecordRepository _repository;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiWorkplaces.BuildingWorkflowService.BuildingWorkflowServiceClient _workplacesBuildingWorkflowService;

    public CalibrationApprovalEndpoint(
        ApiBroadcasts.BroadcastService.BroadcastServiceClient broadcastsBroadcastService,
        ICalibrationApprovalEndpointSendSubmitCalibration1Port calibrationApprovalEndpointSendSubmitCalibration1PortAdapter,
        ApiCorporateevents.VenueReportingService.VenueReportingServiceClient corporateeventsVenueReportingService,
        ApiDocuments.DocumentExportService.DocumentExportServiceClient documentsDocumentExportService,
        ApiIdentity.UserQueryService.UserQueryServiceClient identityUserQueryService,
        CallInvoker invoker,
        ApiItsm.ChangeRequestReportingService.ChangeRequestReportingServiceClient itsmChangeRequestReportingService,
        ILogger<CalibrationApprovalEndpoint> logger,
        ApiPerformance.CalibrationLifecycleService.CalibrationLifecycleServiceClient performanceCalibrationLifecycleService,
        ApiPerformance.FeedbackApprovalService.FeedbackApprovalServiceClient performanceFeedbackApprovalService,
        ApiPerformance.GoalWorkflowService.GoalWorkflowServiceClient performanceGoalWorkflowService,
        ApiPerformance.ReviewApprovalService.ReviewApprovalServiceClient performanceReviewApprovalService,
        ApiRecruiting.CandidateCalculationService.CandidateCalculationServiceClient recruitingCandidateCalculationService,
        ApiRecruiting.InterviewCalculationService.InterviewCalculationServiceClient recruitingInterviewCalculationService,
        ApiRecruiting.JobOpeningQueryService.JobOpeningQueryServiceClient recruitingJobOpeningQueryService,
        ApiRecruiting.OfferValidationService.OfferValidationServiceClient recruitingOfferValidationService,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiWorkplaces.BuildingWorkflowService.BuildingWorkflowServiceClient workplacesBuildingWorkflowService)
    {
        _broadcastsBroadcastService = broadcastsBroadcastService;
        _calibrationApprovalEndpointSendSubmitCalibration1Port = calibrationApprovalEndpointSendSubmitCalibration1PortAdapter;
        _corporateeventsVenueReportingService = corporateeventsVenueReportingService;
        _documentsDocumentExportService = documentsDocumentExportService;
        _identityUserQueryService = identityUserQueryService;
        _invoker = invoker;
        _itsmChangeRequestReportingService = itsmChangeRequestReportingService;
        _logger = logger;
        _performanceCalibrationLifecycleService = performanceCalibrationLifecycleService;
        _performanceFeedbackApprovalService = performanceFeedbackApprovalService;
        _performanceGoalWorkflowService = performanceGoalWorkflowService;
        _performanceReviewApprovalService = performanceReviewApprovalService;
        _recruitingCandidateCalculationService = recruitingCandidateCalculationService;
        _recruitingInterviewCalculationService = recruitingInterviewCalculationService;
        _recruitingJobOpeningQueryService = recruitingJobOpeningQueryService;
        _recruitingOfferValidationService = recruitingOfferValidationService;
        _repository = repository;
        _settings = settings;
        _workplacesBuildingWorkflowService = workplacesBuildingWorkflowService;
    }
}
