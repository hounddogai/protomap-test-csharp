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
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSearch = Acme.Search.V1;
using ApiSellers = Acme.Sellers.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Licensing;

/// <summary>
/// Routes license assignments through approval, with escalation and delegation.
/// </summary>
public sealed partial class LicenseAssignmentApprovalEndpoint : ApiLicensing.LicenseAssignmentApprovalService.LicenseAssignmentApprovalServiceBase
{
    private readonly NotificationBus _bus;
    private readonly ApiFeatureflags.FeatureFlagValidationService.FeatureFlagValidationServiceClient _featureflagsFeatureFlagValidationService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.ServiceAssetValidationService.ServiceAssetValidationServiceClient _itsmServiceAssetValidationService;
    private readonly ILicenseAssignmentApprovalEndpointSendEscalateLicenseAssignment3Port _licenseAssignmentApprovalEndpointSendEscalateLicenseAssignment3Port;
    private readonly ApiLicensing.EntitlementApprovalService.EntitlementApprovalServiceClient _licensingEntitlementApprovalService;
    private readonly ApiLicensing.EntitlementLifecycleService.EntitlementLifecycleServiceClient _licensingEntitlementLifecycleService;
    private readonly ApiLicensing.EntitlementQueryService.EntitlementQueryServiceClient _licensingEntitlementQueryService;
    private readonly ApiLicensing.EntitlementValidationService.EntitlementValidationServiceClient _licensingEntitlementValidationService;
    private readonly ApiLicensing.LicenseAssignmentCalculationService.LicenseAssignmentCalculationServiceClient _licensingLicenseAssignmentCalculationService;
    private readonly ApiLicensing.LicenseAssignmentReportingService.LicenseAssignmentReportingServiceClient _licensingLicenseAssignmentReportingService;
    private readonly ApiLicensing.SoftwareLicenseQueryService.SoftwareLicenseQueryServiceClient _licensingSoftwareLicenseQueryService;
    private readonly ApiLicensing.SoftwareLicenseService.SoftwareLicenseServiceClient _licensingSoftwareLicenseService;
    private readonly ApiLicensing.SoftwareLicenseValidationService.SoftwareLicenseValidationServiceClient _licensingSoftwareLicenseValidationService;
    private readonly ILogger<LicenseAssignmentApprovalEndpoint> _logger;
    private readonly ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient _metricsMetricSeriesQueryService;
    private readonly ApiPerformance.GoalValidationService.GoalValidationServiceClient _performanceGoalValidationService;
    private readonly RetryPolicy _policy;
    private readonly ApiRecruiting.InterviewCalculationService.InterviewCalculationServiceClient _recruitingInterviewCalculationService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiSearch.SearchIndexValidationService.SearchIndexValidationServiceClient _searchSearchIndexValidationService;
    private readonly ApiSellers.SellerService.SellerServiceClient _sellersSellerService;
    private readonly Func<ApiMetrics.CountMetricSeriesesRequest, CallOptions, AsyncUnaryCall<ApiMetrics.CountMetricSeriesesResponse>> _sendCommentLicenseAssignmentApproval0;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiWorkplaces.RoomBookingValidationService.RoomBookingValidationServiceClient _workplacesRoomBookingValidationService;

    public LicenseAssignmentApprovalEndpoint(
        NotificationBus bus,
        ApiFeatureflags.FeatureFlagValidationService.FeatureFlagValidationServiceClient featureflagsFeatureFlagValidationService,
        CallInvoker invoker,
        ApiItsm.ServiceAssetValidationService.ServiceAssetValidationServiceClient itsmServiceAssetValidationService,
        ILicenseAssignmentApprovalEndpointSendEscalateLicenseAssignment3Port licenseAssignmentApprovalEndpointSendEscalateLicenseAssignment3PortAdapter,
        ApiLicensing.EntitlementApprovalService.EntitlementApprovalServiceClient licensingEntitlementApprovalService,
        ApiLicensing.EntitlementLifecycleService.EntitlementLifecycleServiceClient licensingEntitlementLifecycleService,
        ApiLicensing.EntitlementQueryService.EntitlementQueryServiceClient licensingEntitlementQueryService,
        ApiLicensing.EntitlementValidationService.EntitlementValidationServiceClient licensingEntitlementValidationService,
        ApiLicensing.LicenseAssignmentCalculationService.LicenseAssignmentCalculationServiceClient licensingLicenseAssignmentCalculationService,
        ApiLicensing.LicenseAssignmentReportingService.LicenseAssignmentReportingServiceClient licensingLicenseAssignmentReportingService,
        ApiLicensing.SoftwareLicenseQueryService.SoftwareLicenseQueryServiceClient licensingSoftwareLicenseQueryService,
        ApiLicensing.SoftwareLicenseService.SoftwareLicenseServiceClient licensingSoftwareLicenseService,
        ApiLicensing.SoftwareLicenseValidationService.SoftwareLicenseValidationServiceClient licensingSoftwareLicenseValidationService,
        ILogger<LicenseAssignmentApprovalEndpoint> logger,
        ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient metricsMetricSeriesQueryService,
        ApiPerformance.GoalValidationService.GoalValidationServiceClient performanceGoalValidationService,
        RetryPolicy policy,
        ApiRecruiting.InterviewCalculationService.InterviewCalculationServiceClient recruitingInterviewCalculationService,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiSearch.SearchIndexValidationService.SearchIndexValidationServiceClient searchSearchIndexValidationService,
        ApiSellers.SellerService.SellerServiceClient sellersSellerService,
        IOptions<MeshOptions> settings,
        ApiWorkplaces.RoomBookingValidationService.RoomBookingValidationServiceClient workplacesRoomBookingValidationService)
    {
        _bus = bus;
        _featureflagsFeatureFlagValidationService = featureflagsFeatureFlagValidationService;
        _invoker = invoker;
        _itsmServiceAssetValidationService = itsmServiceAssetValidationService;
        _licenseAssignmentApprovalEndpointSendEscalateLicenseAssignment3Port = licenseAssignmentApprovalEndpointSendEscalateLicenseAssignment3PortAdapter;
        _licensingEntitlementApprovalService = licensingEntitlementApprovalService;
        _licensingEntitlementLifecycleService = licensingEntitlementLifecycleService;
        _licensingEntitlementQueryService = licensingEntitlementQueryService;
        _licensingEntitlementValidationService = licensingEntitlementValidationService;
        _licensingLicenseAssignmentCalculationService = licensingLicenseAssignmentCalculationService;
        _licensingLicenseAssignmentReportingService = licensingLicenseAssignmentReportingService;
        _licensingSoftwareLicenseQueryService = licensingSoftwareLicenseQueryService;
        _licensingSoftwareLicenseService = licensingSoftwareLicenseService;
        _licensingSoftwareLicenseValidationService = licensingSoftwareLicenseValidationService;
        _logger = logger;
        _metricsMetricSeriesQueryService = metricsMetricSeriesQueryService;
        _performanceGoalValidationService = performanceGoalValidationService;
        _policy = policy;
        _recruitingInterviewCalculationService = recruitingInterviewCalculationService;
        _registry = registry;
        _repository = repository;
        _searchSearchIndexValidationService = searchSearchIndexValidationService;
        _sellersSellerService = sellersSellerService;
        _settings = settings;
        _workplacesRoomBookingValidationService = workplacesRoomBookingValidationService;
        _sendCommentLicenseAssignmentApproval0 = _metricsMetricSeriesQueryService.CountMetricSeriesesAsync;
    }
}
