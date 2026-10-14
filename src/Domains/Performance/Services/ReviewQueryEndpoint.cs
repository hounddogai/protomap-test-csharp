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
using ApiAnalytics = Acme.Analytics.V1;
using ApiAudit = Acme.Audit.V1;
using ApiCommon = Acme.Common.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Performance;

/// <summary>
/// Answers read-only questions about reviews, for screens and reports.
/// </summary>
public sealed partial class ReviewQueryEndpoint : ApiPerformance.ReviewQueryService.ReviewQueryServiceBase
{
    private readonly ApiAnalytics.AnalyticsEventService.AnalyticsEventServiceClient _analyticsAnalyticsEventService;
    private readonly ApiAudit.AuditEventQueryService.AuditEventQueryServiceClient _auditAuditEventQueryService;
    private readonly ApiAudit.AuditEventService.AuditEventServiceClient _auditAuditEventService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.ServiceAssetValidationService.ServiceAssetValidationServiceClient _itsmServiceAssetValidationService;
    private readonly ILogger<ReviewQueryEndpoint> _logger;
    private readonly ApiPerformance.CalibrationCalculationService.CalibrationCalculationServiceClient _performanceCalibrationCalculationService;
    private readonly ApiPerformance.FeedbackCalculationService.FeedbackCalculationServiceClient _performanceFeedbackCalculationService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiItsm.NormalizeServiceAssetRequest, CallOptions, AsyncUnaryCall<ApiItsm.NormalizeServiceAssetResponse>> _sendCountReviews0;
    private readonly ApiWorkplaces.RoomBookingCalculationService.RoomBookingCalculationServiceClient _workplacesRoomBookingCalculationService;

    public ReviewQueryEndpoint(
        ApiAnalytics.AnalyticsEventService.AnalyticsEventServiceClient analyticsAnalyticsEventService,
        ApiAudit.AuditEventQueryService.AuditEventQueryServiceClient auditAuditEventQueryService,
        ApiAudit.AuditEventService.AuditEventServiceClient auditAuditEventService,
        CallInvoker invoker,
        ApiItsm.ServiceAssetValidationService.ServiceAssetValidationServiceClient itsmServiceAssetValidationService,
        ILogger<ReviewQueryEndpoint> logger,
        ApiPerformance.CalibrationCalculationService.CalibrationCalculationServiceClient performanceCalibrationCalculationService,
        ApiPerformance.FeedbackCalculationService.FeedbackCalculationServiceClient performanceFeedbackCalculationService,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiWorkplaces.RoomBookingCalculationService.RoomBookingCalculationServiceClient workplacesRoomBookingCalculationService)
    {
        _analyticsAnalyticsEventService = analyticsAnalyticsEventService;
        _auditAuditEventQueryService = auditAuditEventQueryService;
        _auditAuditEventService = auditAuditEventService;
        _invoker = invoker;
        _itsmServiceAssetValidationService = itsmServiceAssetValidationService;
        _logger = logger;
        _performanceCalibrationCalculationService = performanceCalibrationCalculationService;
        _performanceFeedbackCalculationService = performanceFeedbackCalculationService;
        _registry = registry;
        _repository = repository;
        _workplacesRoomBookingCalculationService = workplacesRoomBookingCalculationService;
        _sendCountReviews0 = _itsmServiceAssetValidationService.NormalizeServiceAssetAsync;
    }
}
