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
using ApiAudit = Acme.Audit.V1;
using ApiBenefits = Acme.Benefits.V1;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSeating = Acme.Seating.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Onboarding;

/// <summary>
/// Computes the figures of an onboarding task, such as totals, taxes, and estimates.
/// </summary>
public sealed partial class OnboardingTaskCalculationEndpoint : ApiOnboarding.OnboardingTaskCalculationService.OnboardingTaskCalculationServiceBase
{
    private readonly ApiAudit.AuditEventValidationService.AuditEventValidationServiceClient _auditAuditEventValidationService;
    private readonly ApiBenefits.LifeEventValidationService.LifeEventValidationServiceClient _benefitsLifeEventValidationService;
    private readonly ApiDevices.DevicePolicyValidationService.DevicePolicyValidationServiceClient _devicesDevicePolicyValidationService;
    private readonly ApiDocuments.DocumentHistoryService.DocumentHistoryServiceClient _documentsDocumentHistoryService;
    private readonly ApiDocuments.SignatureCalculationService.SignatureCalculationServiceClient _documentsSignatureCalculationService;
    private readonly ApiDocuments.SignatureHistoryService.SignatureHistoryServiceClient _documentsSignatureHistoryService;
    private readonly ILogger<OnboardingTaskCalculationEndpoint> _logger;
    private readonly ApiMetrics.MetricSeriesService.MetricSeriesServiceClient _metricsMetricSeriesService;
    private readonly ApiOnboarding.OnboardingPlanCalculationService.OnboardingPlanCalculationServiceClient _onboardingOnboardingPlanCalculationService;
    private readonly IRecordRepository _repository;
    private readonly ApiSeating.SeatMapQueryService.SeatMapQueryServiceClient _seatingSeatMapQueryService;
    private readonly Func<ApiOnboarding.ExplainOnboardingPlanCalculationRequest, CallOptions, AsyncUnaryCall<ApiOnboarding.ExplainOnboardingPlanCalculationResponse>> _sendCompareOnboardingTaskOptions3;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTimekeeping.ClockEventValidationService.ClockEventValidationServiceClient _timekeepingClockEventValidationService;
    private readonly ApiVenues.VenueService.VenueServiceClient _venuesVenueService;

    public OnboardingTaskCalculationEndpoint(
        ApiAudit.AuditEventValidationService.AuditEventValidationServiceClient auditAuditEventValidationService,
        ApiBenefits.LifeEventValidationService.LifeEventValidationServiceClient benefitsLifeEventValidationService,
        ApiDevices.DevicePolicyValidationService.DevicePolicyValidationServiceClient devicesDevicePolicyValidationService,
        ApiDocuments.DocumentHistoryService.DocumentHistoryServiceClient documentsDocumentHistoryService,
        ApiDocuments.SignatureCalculationService.SignatureCalculationServiceClient documentsSignatureCalculationService,
        ApiDocuments.SignatureHistoryService.SignatureHistoryServiceClient documentsSignatureHistoryService,
        ILogger<OnboardingTaskCalculationEndpoint> logger,
        ApiMetrics.MetricSeriesService.MetricSeriesServiceClient metricsMetricSeriesService,
        ApiOnboarding.OnboardingPlanCalculationService.OnboardingPlanCalculationServiceClient onboardingOnboardingPlanCalculationService,
        IRecordRepository repository,
        ApiSeating.SeatMapQueryService.SeatMapQueryServiceClient seatingSeatMapQueryService,
        IOptions<MeshOptions> settings,
        ApiTimekeeping.ClockEventValidationService.ClockEventValidationServiceClient timekeepingClockEventValidationService,
        ApiVenues.VenueService.VenueServiceClient venuesVenueService)
    {
        _auditAuditEventValidationService = auditAuditEventValidationService;
        _benefitsLifeEventValidationService = benefitsLifeEventValidationService;
        _devicesDevicePolicyValidationService = devicesDevicePolicyValidationService;
        _documentsDocumentHistoryService = documentsDocumentHistoryService;
        _documentsSignatureCalculationService = documentsSignatureCalculationService;
        _documentsSignatureHistoryService = documentsSignatureHistoryService;
        _logger = logger;
        _metricsMetricSeriesService = metricsMetricSeriesService;
        _onboardingOnboardingPlanCalculationService = onboardingOnboardingPlanCalculationService;
        _repository = repository;
        _seatingSeatMapQueryService = seatingSeatMapQueryService;
        _settings = settings;
        _timekeepingClockEventValidationService = timekeepingClockEventValidationService;
        _venuesVenueService = venuesVenueService;
        _sendCompareOnboardingTaskOptions3 = _onboardingOnboardingPlanCalculationService.ExplainOnboardingPlanCalculationAsync;
    }
}
