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
using ApiCheckout = Acme.Checkout.V1;
using ApiCommon = Acme.Common.V1;
using ApiDatasets = Acme.Datasets.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTravel = Acme.Travel.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Onboarding;

/// <summary>
/// Runs the long-lived workflow that takes an equipment from start to finish.
/// </summary>
public sealed partial class EquipmentWorkflowEndpoint : ApiOnboarding.EquipmentWorkflowService.EquipmentWorkflowServiceBase
{
    private readonly NotificationBus _bus;
    private readonly ApiCheckout.CheckoutSessionQueryService.CheckoutSessionQueryServiceClient _checkoutCheckoutSessionQueryService;
    private readonly ApiDatasets.DatasetQueryService.DatasetQueryServiceClient _datasetsDatasetQueryService;
    private readonly ApiDocuments.SignatureExportService.SignatureExportServiceClient _documentsSignatureExportService;
    private readonly IEquipmentWorkflowEndpointSendCancelEquipmentWorkflow0Port _equipmentWorkflowEndpointSendCancelEquipmentWorkflow0Port;
    private readonly ApiFeatureflags.FeatureFlagService.FeatureFlagServiceClient _featureflagsFeatureFlagService;
    private readonly ApiHelpdesk.EscalationLifecycleService.EscalationLifecycleServiceClient _helpdeskEscalationLifecycleService;
    private readonly ApiHelpdesk.KnowledgeArticleApprovalService.KnowledgeArticleApprovalServiceClient _helpdeskKnowledgeArticleApprovalService;
    private readonly ApiLearning.CourseEnrollmentApprovalService.CourseEnrollmentApprovalServiceClient _learningCourseEnrollmentApprovalService;
    private readonly ApiLearning.CourseWorkflowService.CourseWorkflowServiceClient _learningCourseWorkflowService;
    private readonly ApiLedger.LedgerAccountValidationService.LedgerAccountValidationServiceClient _ledgerLedgerAccountValidationService;
    private readonly ApiLicensing.LicenseAssignmentApprovalService.LicenseAssignmentApprovalServiceClient _licensingLicenseAssignmentApprovalService;
    private readonly ApiLicensing.LicenseAssignmentReportingService.LicenseAssignmentReportingServiceClient _licensingLicenseAssignmentReportingService;
    private readonly ILogger<EquipmentWorkflowEndpoint> _logger;
    private readonly ApiOnboarding.EquipmentApprovalService.EquipmentApprovalServiceClient _onboardingEquipmentApprovalService;
    private readonly ApiOnboarding.EquipmentLifecycleService.EquipmentLifecycleServiceClient _onboardingEquipmentLifecycleService;
    private readonly ApiOnboarding.EquipmentReportingService.EquipmentReportingServiceClient _onboardingEquipmentReportingService;
    private readonly ApiOnboarding.OnboardingPlanApprovalService.OnboardingPlanApprovalServiceClient _onboardingOnboardingPlanApprovalService;
    private readonly ApiOnboarding.OnboardingPlanQueryService.OnboardingPlanQueryServiceClient _onboardingOnboardingPlanQueryService;
    private readonly ApiOnboarding.OnboardingTaskApprovalService.OnboardingTaskApprovalServiceClient _onboardingOnboardingTaskApprovalService;
    private readonly ApiOnboarding.OnboardingTaskReportingService.OnboardingTaskReportingServiceClient _onboardingOnboardingTaskReportingService;
    private readonly RetryPolicy _policy;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiLearning.ResumeCourseWorkflowRequest, CallOptions, AsyncUnaryCall<ApiLearning.ResumeCourseWorkflowResponse>> _sendPauseEquipmentWorkflow2;
    private readonly ApiTravel.ItinerarySyncService.ItinerarySyncServiceClient _travelItinerarySyncService;
    private readonly ApiVenues.VenueValidationService.VenueValidationServiceClient _venuesVenueValidationService;

    public EquipmentWorkflowEndpoint(
        NotificationBus bus,
        ApiCheckout.CheckoutSessionQueryService.CheckoutSessionQueryServiceClient checkoutCheckoutSessionQueryService,
        ApiDatasets.DatasetQueryService.DatasetQueryServiceClient datasetsDatasetQueryService,
        ApiDocuments.SignatureExportService.SignatureExportServiceClient documentsSignatureExportService,
        IEquipmentWorkflowEndpointSendCancelEquipmentWorkflow0Port equipmentWorkflowEndpointSendCancelEquipmentWorkflow0PortAdapter,
        ApiFeatureflags.FeatureFlagService.FeatureFlagServiceClient featureflagsFeatureFlagService,
        ApiHelpdesk.EscalationLifecycleService.EscalationLifecycleServiceClient helpdeskEscalationLifecycleService,
        ApiHelpdesk.KnowledgeArticleApprovalService.KnowledgeArticleApprovalServiceClient helpdeskKnowledgeArticleApprovalService,
        ApiLearning.CourseEnrollmentApprovalService.CourseEnrollmentApprovalServiceClient learningCourseEnrollmentApprovalService,
        ApiLearning.CourseWorkflowService.CourseWorkflowServiceClient learningCourseWorkflowService,
        ApiLedger.LedgerAccountValidationService.LedgerAccountValidationServiceClient ledgerLedgerAccountValidationService,
        ApiLicensing.LicenseAssignmentApprovalService.LicenseAssignmentApprovalServiceClient licensingLicenseAssignmentApprovalService,
        ApiLicensing.LicenseAssignmentReportingService.LicenseAssignmentReportingServiceClient licensingLicenseAssignmentReportingService,
        ILogger<EquipmentWorkflowEndpoint> logger,
        ApiOnboarding.EquipmentApprovalService.EquipmentApprovalServiceClient onboardingEquipmentApprovalService,
        ApiOnboarding.EquipmentLifecycleService.EquipmentLifecycleServiceClient onboardingEquipmentLifecycleService,
        ApiOnboarding.EquipmentReportingService.EquipmentReportingServiceClient onboardingEquipmentReportingService,
        ApiOnboarding.OnboardingPlanApprovalService.OnboardingPlanApprovalServiceClient onboardingOnboardingPlanApprovalService,
        ApiOnboarding.OnboardingPlanQueryService.OnboardingPlanQueryServiceClient onboardingOnboardingPlanQueryService,
        ApiOnboarding.OnboardingTaskApprovalService.OnboardingTaskApprovalServiceClient onboardingOnboardingTaskApprovalService,
        ApiOnboarding.OnboardingTaskReportingService.OnboardingTaskReportingServiceClient onboardingOnboardingTaskReportingService,
        RetryPolicy policy,
        IRecordRepository repository,
        ApiTravel.ItinerarySyncService.ItinerarySyncServiceClient travelItinerarySyncService,
        ApiVenues.VenueValidationService.VenueValidationServiceClient venuesVenueValidationService)
    {
        _bus = bus;
        _checkoutCheckoutSessionQueryService = checkoutCheckoutSessionQueryService;
        _datasetsDatasetQueryService = datasetsDatasetQueryService;
        _documentsSignatureExportService = documentsSignatureExportService;
        _equipmentWorkflowEndpointSendCancelEquipmentWorkflow0Port = equipmentWorkflowEndpointSendCancelEquipmentWorkflow0PortAdapter;
        _featureflagsFeatureFlagService = featureflagsFeatureFlagService;
        _helpdeskEscalationLifecycleService = helpdeskEscalationLifecycleService;
        _helpdeskKnowledgeArticleApprovalService = helpdeskKnowledgeArticleApprovalService;
        _learningCourseEnrollmentApprovalService = learningCourseEnrollmentApprovalService;
        _learningCourseWorkflowService = learningCourseWorkflowService;
        _ledgerLedgerAccountValidationService = ledgerLedgerAccountValidationService;
        _licensingLicenseAssignmentApprovalService = licensingLicenseAssignmentApprovalService;
        _licensingLicenseAssignmentReportingService = licensingLicenseAssignmentReportingService;
        _logger = logger;
        _onboardingEquipmentApprovalService = onboardingEquipmentApprovalService;
        _onboardingEquipmentLifecycleService = onboardingEquipmentLifecycleService;
        _onboardingEquipmentReportingService = onboardingEquipmentReportingService;
        _onboardingOnboardingPlanApprovalService = onboardingOnboardingPlanApprovalService;
        _onboardingOnboardingPlanQueryService = onboardingOnboardingPlanQueryService;
        _onboardingOnboardingTaskApprovalService = onboardingOnboardingTaskApprovalService;
        _onboardingOnboardingTaskReportingService = onboardingOnboardingTaskReportingService;
        _policy = policy;
        _repository = repository;
        _travelItinerarySyncService = travelItinerarySyncService;
        _venuesVenueValidationService = venuesVenueValidationService;
        _sendPauseEquipmentWorkflow2 = _learningCourseWorkflowService.ResumeCourseWorkflowAsync;
    }
}
