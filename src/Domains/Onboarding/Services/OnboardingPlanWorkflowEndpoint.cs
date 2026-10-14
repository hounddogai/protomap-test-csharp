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
using ApiBenefits = Acme.Benefits.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiLineage = Acme.Lineage.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiRouting = Acme.Routing.V1;
using ApiSearch = Acme.Search.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Onboarding;

/// <summary>
/// Runs the long-lived workflow that takes an onboarding plan from start to finish.
/// </summary>
public sealed partial class OnboardingPlanWorkflowEndpoint : ApiOnboarding.OnboardingPlanWorkflowService.OnboardingPlanWorkflowServiceBase
{
    private readonly ApiBenefits.EnrollmentApprovalService.EnrollmentApprovalServiceClient _benefitsEnrollmentApprovalService;
    private readonly ApiDocuments.DocumentTemplateCalculationService.DocumentTemplateCalculationServiceClient _documentsDocumentTemplateCalculationService;
    private readonly ApiFeatureflags.FeatureFlagQueryService.FeatureFlagQueryServiceClient _featureflagsFeatureFlagQueryService;
    private readonly ApiHelpdesk.EscalationApprovalService.EscalationApprovalServiceClient _helpdeskEscalationApprovalService;
    private readonly ApiHelpdesk.SlaPolicyWorkflowService.SlaPolicyWorkflowServiceClient _helpdeskSlaPolicyWorkflowService;
    private readonly ApiLicensing.SoftwareLicenseWorkflowService.SoftwareLicenseWorkflowServiceClient _licensingSoftwareLicenseWorkflowService;
    private readonly ApiLineage.LineageNodeQueryService.LineageNodeQueryServiceClient _lineageLineageNodeQueryService;
    private readonly ILogger<OnboardingPlanWorkflowEndpoint> _logger;
    private readonly ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient _metricsMetricSeriesQueryService;
    private readonly ApiOnboarding.EquipmentLifecycleService.EquipmentLifecycleServiceClient _onboardingEquipmentLifecycleService;
    private readonly ApiOnboarding.OnboardingPlanApprovalService.OnboardingPlanApprovalServiceClient _onboardingOnboardingPlanApprovalService;
    private readonly ApiOnboarding.OnboardingTaskCalculationService.OnboardingTaskCalculationServiceClient _onboardingOnboardingTaskCalculationService;
    private readonly IOnboardingPlanWorkflowEndpointSendRetryOnboardingPlanWorkflow1Port _onboardingPlanWorkflowEndpointSendRetryOnboardingPlanWorkflow1Port;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiRouting.RouteCalculationService.RouteCalculationServiceClient _routingRouteCalculationService;
    private readonly ApiSearch.SearchIndexQueryService.SearchIndexQueryServiceClient _searchSearchIndexQueryService;
    private readonly Func<ApiLicensing.CancelSoftwareLicenseWorkflowRequest, CallOptions, AsyncUnaryCall<ApiLicensing.CancelSoftwareLicenseWorkflowResponse>> _sendPauseOnboardingPlanWorkflow1;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiVenues.VenueQueryService.VenueQueryServiceClient _venuesVenueQueryService;

    public OnboardingPlanWorkflowEndpoint(
        ApiBenefits.EnrollmentApprovalService.EnrollmentApprovalServiceClient benefitsEnrollmentApprovalService,
        ApiDocuments.DocumentTemplateCalculationService.DocumentTemplateCalculationServiceClient documentsDocumentTemplateCalculationService,
        ApiFeatureflags.FeatureFlagQueryService.FeatureFlagQueryServiceClient featureflagsFeatureFlagQueryService,
        ApiHelpdesk.EscalationApprovalService.EscalationApprovalServiceClient helpdeskEscalationApprovalService,
        ApiHelpdesk.SlaPolicyWorkflowService.SlaPolicyWorkflowServiceClient helpdeskSlaPolicyWorkflowService,
        ApiLicensing.SoftwareLicenseWorkflowService.SoftwareLicenseWorkflowServiceClient licensingSoftwareLicenseWorkflowService,
        ApiLineage.LineageNodeQueryService.LineageNodeQueryServiceClient lineageLineageNodeQueryService,
        ILogger<OnboardingPlanWorkflowEndpoint> logger,
        ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient metricsMetricSeriesQueryService,
        ApiOnboarding.EquipmentLifecycleService.EquipmentLifecycleServiceClient onboardingEquipmentLifecycleService,
        ApiOnboarding.OnboardingPlanApprovalService.OnboardingPlanApprovalServiceClient onboardingOnboardingPlanApprovalService,
        ApiOnboarding.OnboardingTaskCalculationService.OnboardingTaskCalculationServiceClient onboardingOnboardingTaskCalculationService,
        IOnboardingPlanWorkflowEndpointSendRetryOnboardingPlanWorkflow1Port onboardingPlanWorkflowEndpointSendRetryOnboardingPlanWorkflow1PortAdapter,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiRouting.RouteCalculationService.RouteCalculationServiceClient routingRouteCalculationService,
        ApiSearch.SearchIndexQueryService.SearchIndexQueryServiceClient searchSearchIndexQueryService,
        IOptions<MeshOptions> settings,
        ApiVenues.VenueQueryService.VenueQueryServiceClient venuesVenueQueryService)
    {
        _benefitsEnrollmentApprovalService = benefitsEnrollmentApprovalService;
        _documentsDocumentTemplateCalculationService = documentsDocumentTemplateCalculationService;
        _featureflagsFeatureFlagQueryService = featureflagsFeatureFlagQueryService;
        _helpdeskEscalationApprovalService = helpdeskEscalationApprovalService;
        _helpdeskSlaPolicyWorkflowService = helpdeskSlaPolicyWorkflowService;
        _licensingSoftwareLicenseWorkflowService = licensingSoftwareLicenseWorkflowService;
        _lineageLineageNodeQueryService = lineageLineageNodeQueryService;
        _logger = logger;
        _metricsMetricSeriesQueryService = metricsMetricSeriesQueryService;
        _onboardingEquipmentLifecycleService = onboardingEquipmentLifecycleService;
        _onboardingOnboardingPlanApprovalService = onboardingOnboardingPlanApprovalService;
        _onboardingOnboardingTaskCalculationService = onboardingOnboardingTaskCalculationService;
        _onboardingPlanWorkflowEndpointSendRetryOnboardingPlanWorkflow1Port = onboardingPlanWorkflowEndpointSendRetryOnboardingPlanWorkflow1PortAdapter;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _routingRouteCalculationService = routingRouteCalculationService;
        _searchSearchIndexQueryService = searchSearchIndexQueryService;
        _settings = settings;
        _venuesVenueQueryService = venuesVenueQueryService;
        _sendPauseOnboardingPlanWorkflow1 = _licensingSoftwareLicenseWorkflowService.CancelSoftwareLicenseWorkflowAsync;
    }
}
