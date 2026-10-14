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
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiOnboarding = Acme.Onboarding.V1;

namespace Acme.Workforce.Licensing;

/// <summary>
/// Runs the long-lived workflow that takes a license assignment from start to finish.
/// </summary>
public sealed partial class LicenseAssignmentWorkflowEndpoint : ApiLicensing.LicenseAssignmentWorkflowService.LicenseAssignmentWorkflowServiceBase
{
    private readonly ApiDocuments.DocumentTemplateNotificationService.DocumentTemplateNotificationServiceClient _documentsDocumentTemplateNotificationService;
    private readonly ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient _documentsDocumentTemplateSyncService;
    private readonly ApiHelpdesk.EscalationCalculationService.EscalationCalculationServiceClient _helpdeskEscalationCalculationService;
    private readonly ApiIdentity.UserValidationService.UserValidationServiceClient _identityUserValidationService;
    private readonly ILicenseAssignmentWorkflowEndpointSendCancelLicenseAssignmentWorkflow3Port _licenseAssignmentWorkflowEndpointSendCancelLicenseAssignmentWorkflow3Port;
    private readonly ApiLicensing.EntitlementLifecycleService.EntitlementLifecycleServiceClient _licensingEntitlementLifecycleService;
    private readonly ApiLicensing.EntitlementWorkflowService.EntitlementWorkflowServiceClient _licensingEntitlementWorkflowService;
    private readonly ApiLicensing.LicenseAssignmentApprovalService.LicenseAssignmentApprovalServiceClient _licensingLicenseAssignmentApprovalService;
    private readonly ApiLicensing.LicenseAssignmentLifecycleService.LicenseAssignmentLifecycleServiceClient _licensingLicenseAssignmentLifecycleService;
    private readonly ApiLicensing.LicenseAssignmentReportingService.LicenseAssignmentReportingServiceClient _licensingLicenseAssignmentReportingService;
    private readonly ApiLicensing.LicenseAssignmentValidationService.LicenseAssignmentValidationServiceClient _licensingLicenseAssignmentValidationService;
    private readonly ApiLicensing.SoftwareLicenseApprovalService.SoftwareLicenseApprovalServiceClient _licensingSoftwareLicenseApprovalService;
    private readonly ILogger<LicenseAssignmentWorkflowEndpoint> _logger;
    private readonly ApiOnboarding.OnboardingPlanQueryService.OnboardingPlanQueryServiceClient _onboardingOnboardingPlanQueryService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiLicensing.ListLicenseAssignmentReportsRequest, CallOptions, AsyncUnaryCall<ApiLicensing.ListLicenseAssignmentReportsResponse>> _sendSignalLicenseAssignmentWorkflow1;
    private readonly IOptions<MeshOptions> _settings;

    public LicenseAssignmentWorkflowEndpoint(
        ApiDocuments.DocumentTemplateNotificationService.DocumentTemplateNotificationServiceClient documentsDocumentTemplateNotificationService,
        ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient documentsDocumentTemplateSyncService,
        ApiHelpdesk.EscalationCalculationService.EscalationCalculationServiceClient helpdeskEscalationCalculationService,
        ApiIdentity.UserValidationService.UserValidationServiceClient identityUserValidationService,
        ILicenseAssignmentWorkflowEndpointSendCancelLicenseAssignmentWorkflow3Port licenseAssignmentWorkflowEndpointSendCancelLicenseAssignmentWorkflow3PortAdapter,
        ApiLicensing.EntitlementLifecycleService.EntitlementLifecycleServiceClient licensingEntitlementLifecycleService,
        ApiLicensing.EntitlementWorkflowService.EntitlementWorkflowServiceClient licensingEntitlementWorkflowService,
        ApiLicensing.LicenseAssignmentApprovalService.LicenseAssignmentApprovalServiceClient licensingLicenseAssignmentApprovalService,
        ApiLicensing.LicenseAssignmentLifecycleService.LicenseAssignmentLifecycleServiceClient licensingLicenseAssignmentLifecycleService,
        ApiLicensing.LicenseAssignmentReportingService.LicenseAssignmentReportingServiceClient licensingLicenseAssignmentReportingService,
        ApiLicensing.LicenseAssignmentValidationService.LicenseAssignmentValidationServiceClient licensingLicenseAssignmentValidationService,
        ApiLicensing.SoftwareLicenseApprovalService.SoftwareLicenseApprovalServiceClient licensingSoftwareLicenseApprovalService,
        ILogger<LicenseAssignmentWorkflowEndpoint> logger,
        ApiOnboarding.OnboardingPlanQueryService.OnboardingPlanQueryServiceClient onboardingOnboardingPlanQueryService,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings)
    {
        _documentsDocumentTemplateNotificationService = documentsDocumentTemplateNotificationService;
        _documentsDocumentTemplateSyncService = documentsDocumentTemplateSyncService;
        _helpdeskEscalationCalculationService = helpdeskEscalationCalculationService;
        _identityUserValidationService = identityUserValidationService;
        _licenseAssignmentWorkflowEndpointSendCancelLicenseAssignmentWorkflow3Port = licenseAssignmentWorkflowEndpointSendCancelLicenseAssignmentWorkflow3PortAdapter;
        _licensingEntitlementLifecycleService = licensingEntitlementLifecycleService;
        _licensingEntitlementWorkflowService = licensingEntitlementWorkflowService;
        _licensingLicenseAssignmentApprovalService = licensingLicenseAssignmentApprovalService;
        _licensingLicenseAssignmentLifecycleService = licensingLicenseAssignmentLifecycleService;
        _licensingLicenseAssignmentReportingService = licensingLicenseAssignmentReportingService;
        _licensingLicenseAssignmentValidationService = licensingLicenseAssignmentValidationService;
        _licensingSoftwareLicenseApprovalService = licensingSoftwareLicenseApprovalService;
        _logger = logger;
        _onboardingOnboardingPlanQueryService = onboardingOnboardingPlanQueryService;
        _registry = registry;
        _repository = repository;
        _settings = settings;
        _sendSignalLicenseAssignmentWorkflow1 = _licensingLicenseAssignmentReportingService.ListLicenseAssignmentReportsAsync;
    }
}
