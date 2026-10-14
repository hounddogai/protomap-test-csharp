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
using ApiCommon = Acme.Common.V1;
using ApiConfiguration = Acme.Configuration.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiPromotions = Acme.Promotions.V1;
using ApiSurveys = Acme.Surveys.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Onboarding;

/// <summary>
/// Routes equipments through approval, with escalation and delegation.
/// </summary>
public sealed partial class EquipmentApprovalEndpoint : ApiOnboarding.EquipmentApprovalService.EquipmentApprovalServiceBase
{
    private readonly ApiAudit.AuditEventValidationService.AuditEventValidationServiceClient _auditAuditEventValidationService;
    private readonly NotificationBus _bus;
    private readonly ApiConfiguration.ConfigSetCalculationService.ConfigSetCalculationServiceClient _configurationConfigSetCalculationService;
    private readonly ApiDocuments.SignatureSyncService.SignatureSyncServiceClient _documentsSignatureSyncService;
    private readonly IEquipmentApprovalEndpointSendApproveEquipment0Port _equipmentApprovalEndpointSendApproveEquipment0Port;
    private readonly ApiHelpdesk.EscalationReportingService.EscalationReportingServiceClient _helpdeskEscalationReportingService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.ServiceAssetWorkflowService.ServiceAssetWorkflowServiceClient _itsmServiceAssetWorkflowService;
    private readonly ApiLicensing.LicenseAssignmentLifecycleService.LicenseAssignmentLifecycleServiceClient _licensingLicenseAssignmentLifecycleService;
    private readonly ILogger<EquipmentApprovalEndpoint> _logger;
    private readonly ApiOnboarding.EquipmentLifecycleService.EquipmentLifecycleServiceClient _onboardingEquipmentLifecycleService;
    private readonly ApiOnboarding.OnboardingPlanApprovalService.OnboardingPlanApprovalServiceClient _onboardingOnboardingPlanApprovalService;
    private readonly ApiOnboarding.OnboardingPlanWorkflowService.OnboardingPlanWorkflowServiceClient _onboardingOnboardingPlanWorkflowService;
    private readonly ApiPerformance.ReviewApprovalService.ReviewApprovalServiceClient _performanceReviewApprovalService;
    private readonly ApiPromotions.PromotionCalculationService.PromotionCalculationServiceClient _promotionsPromotionCalculationService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiSurveys.SurveyQueryService.SurveyQueryServiceClient _surveysSurveyQueryService;
    private readonly ApiTravel.TravelBookingReportingService.TravelBookingReportingServiceClient _travelTravelBookingReportingService;

    public EquipmentApprovalEndpoint(
        ApiAudit.AuditEventValidationService.AuditEventValidationServiceClient auditAuditEventValidationService,
        NotificationBus bus,
        ApiConfiguration.ConfigSetCalculationService.ConfigSetCalculationServiceClient configurationConfigSetCalculationService,
        ApiDocuments.SignatureSyncService.SignatureSyncServiceClient documentsSignatureSyncService,
        IEquipmentApprovalEndpointSendApproveEquipment0Port equipmentApprovalEndpointSendApproveEquipment0PortAdapter,
        ApiHelpdesk.EscalationReportingService.EscalationReportingServiceClient helpdeskEscalationReportingService,
        CallInvoker invoker,
        ApiItsm.ServiceAssetWorkflowService.ServiceAssetWorkflowServiceClient itsmServiceAssetWorkflowService,
        ApiLicensing.LicenseAssignmentLifecycleService.LicenseAssignmentLifecycleServiceClient licensingLicenseAssignmentLifecycleService,
        ILogger<EquipmentApprovalEndpoint> logger,
        ApiOnboarding.EquipmentLifecycleService.EquipmentLifecycleServiceClient onboardingEquipmentLifecycleService,
        ApiOnboarding.OnboardingPlanApprovalService.OnboardingPlanApprovalServiceClient onboardingOnboardingPlanApprovalService,
        ApiOnboarding.OnboardingPlanWorkflowService.OnboardingPlanWorkflowServiceClient onboardingOnboardingPlanWorkflowService,
        ApiPerformance.ReviewApprovalService.ReviewApprovalServiceClient performanceReviewApprovalService,
        ApiPromotions.PromotionCalculationService.PromotionCalculationServiceClient promotionsPromotionCalculationService,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiSurveys.SurveyQueryService.SurveyQueryServiceClient surveysSurveyQueryService,
        ApiTravel.TravelBookingReportingService.TravelBookingReportingServiceClient travelTravelBookingReportingService)
    {
        _auditAuditEventValidationService = auditAuditEventValidationService;
        _bus = bus;
        _configurationConfigSetCalculationService = configurationConfigSetCalculationService;
        _documentsSignatureSyncService = documentsSignatureSyncService;
        _equipmentApprovalEndpointSendApproveEquipment0Port = equipmentApprovalEndpointSendApproveEquipment0PortAdapter;
        _helpdeskEscalationReportingService = helpdeskEscalationReportingService;
        _invoker = invoker;
        _itsmServiceAssetWorkflowService = itsmServiceAssetWorkflowService;
        _licensingLicenseAssignmentLifecycleService = licensingLicenseAssignmentLifecycleService;
        _logger = logger;
        _onboardingEquipmentLifecycleService = onboardingEquipmentLifecycleService;
        _onboardingOnboardingPlanApprovalService = onboardingOnboardingPlanApprovalService;
        _onboardingOnboardingPlanWorkflowService = onboardingOnboardingPlanWorkflowService;
        _performanceReviewApprovalService = performanceReviewApprovalService;
        _promotionsPromotionCalculationService = promotionsPromotionCalculationService;
        _registry = registry;
        _repository = repository;
        _settings = settings;
        _surveysSurveyQueryService = surveysSurveyQueryService;
        _travelTravelBookingReportingService = travelTravelBookingReportingService;
    }
}
