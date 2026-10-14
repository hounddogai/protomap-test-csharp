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
using ApiCart = Acme.Cart.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiOrders = Acme.Orders.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Performance;

/// <summary>
/// Runs the long-lived workflow that takes a review from start to finish.
/// </summary>
public sealed partial class ReviewWorkflowEndpoint : ApiPerformance.ReviewWorkflowService.ReviewWorkflowServiceBase
{
    private readonly NotificationBus _bus;
    private readonly ApiCart.CartService.CartServiceClient _cartCartService;
    private readonly ApiCorporateevents.RegistrationReportingService.RegistrationReportingServiceClient _corporateeventsRegistrationReportingService;
    private readonly ApiCorporateevents.VenueSyncService.VenueSyncServiceClient _corporateeventsVenueSyncService;
    private readonly ApiDocuments.DocumentService.DocumentServiceClient _documentsDocumentService;
    private readonly ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient _documentsDocumentTemplateSyncService;
    private readonly ApiDocuments.SignatureExportService.SignatureExportServiceClient _documentsSignatureExportService;
    private readonly CallInvoker _invoker;
    private readonly ILogger<ReviewWorkflowEndpoint> _logger;
    private readonly ApiOrders.OrderCalculationService.OrderCalculationServiceClient _ordersOrderCalculationService;
    private readonly ApiPerformance.CalibrationValidationService.CalibrationValidationServiceClient _performanceCalibrationValidationService;
    private readonly ApiPerformance.FeedbackCalculationService.FeedbackCalculationServiceClient _performanceFeedbackCalculationService;
    private readonly ApiPerformance.GoalValidationService.GoalValidationServiceClient _performanceGoalValidationService;
    private readonly ApiPerformance.ReviewQueryService.ReviewQueryServiceClient _performanceReviewQueryService;
    private readonly ApiRecruiting.InterviewCalculationService.InterviewCalculationServiceClient _recruitingInterviewCalculationService;
    private readonly ApiRecruiting.JobOpeningQueryService.JobOpeningQueryServiceClient _recruitingJobOpeningQueryService;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiRecruiting.ExplainInterviewCalculationRequest, CallOptions, AsyncUnaryCall<ApiRecruiting.ExplainInterviewCalculationResponse>> _sendListReviewWorkflows1;
    private readonly ApiVenues.VenueValidationService.VenueValidationServiceClient _venuesVenueValidationService;

    public ReviewWorkflowEndpoint(
        NotificationBus bus,
        ApiCart.CartService.CartServiceClient cartCartService,
        ApiCorporateevents.RegistrationReportingService.RegistrationReportingServiceClient corporateeventsRegistrationReportingService,
        ApiCorporateevents.VenueSyncService.VenueSyncServiceClient corporateeventsVenueSyncService,
        ApiDocuments.DocumentService.DocumentServiceClient documentsDocumentService,
        ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient documentsDocumentTemplateSyncService,
        ApiDocuments.SignatureExportService.SignatureExportServiceClient documentsSignatureExportService,
        CallInvoker invoker,
        ILogger<ReviewWorkflowEndpoint> logger,
        ApiOrders.OrderCalculationService.OrderCalculationServiceClient ordersOrderCalculationService,
        ApiPerformance.CalibrationValidationService.CalibrationValidationServiceClient performanceCalibrationValidationService,
        ApiPerformance.FeedbackCalculationService.FeedbackCalculationServiceClient performanceFeedbackCalculationService,
        ApiPerformance.GoalValidationService.GoalValidationServiceClient performanceGoalValidationService,
        ApiPerformance.ReviewQueryService.ReviewQueryServiceClient performanceReviewQueryService,
        ApiRecruiting.InterviewCalculationService.InterviewCalculationServiceClient recruitingInterviewCalculationService,
        ApiRecruiting.JobOpeningQueryService.JobOpeningQueryServiceClient recruitingJobOpeningQueryService,
        IRecordRepository repository,
        ApiVenues.VenueValidationService.VenueValidationServiceClient venuesVenueValidationService)
    {
        _bus = bus;
        _cartCartService = cartCartService;
        _corporateeventsRegistrationReportingService = corporateeventsRegistrationReportingService;
        _corporateeventsVenueSyncService = corporateeventsVenueSyncService;
        _documentsDocumentService = documentsDocumentService;
        _documentsDocumentTemplateSyncService = documentsDocumentTemplateSyncService;
        _documentsSignatureExportService = documentsSignatureExportService;
        _invoker = invoker;
        _logger = logger;
        _ordersOrderCalculationService = ordersOrderCalculationService;
        _performanceCalibrationValidationService = performanceCalibrationValidationService;
        _performanceFeedbackCalculationService = performanceFeedbackCalculationService;
        _performanceGoalValidationService = performanceGoalValidationService;
        _performanceReviewQueryService = performanceReviewQueryService;
        _recruitingInterviewCalculationService = recruitingInterviewCalculationService;
        _recruitingJobOpeningQueryService = recruitingJobOpeningQueryService;
        _repository = repository;
        _venuesVenueValidationService = venuesVenueValidationService;
        _sendListReviewWorkflows1 = _recruitingInterviewCalculationService.ExplainInterviewCalculationAsync;
    }
}
