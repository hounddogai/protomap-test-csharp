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
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiPerformers = Acme.Performers.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Onboarding;

/// <summary>
/// Moves an onboarding plan between the states of its lifecycle.
/// </summary>
public sealed partial class OnboardingPlanLifecycleEndpoint : ApiOnboarding.OnboardingPlanLifecycleService.OnboardingPlanLifecycleServiceBase
{
    private readonly ApiBenefits.EnrollmentApprovalService.EnrollmentApprovalServiceClient _benefitsEnrollmentApprovalService;
    private readonly NotificationBus _bus;
    private readonly ApiDocuments.DocumentFolderQueryService.DocumentFolderQueryServiceClient _documentsDocumentFolderQueryService;
    private readonly ApiDocuments.DocumentNotificationService.DocumentNotificationServiceClient _documentsDocumentNotificationService;
    private readonly ApiDocuments.DocumentService.DocumentServiceClient _documentsDocumentService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.IncidentApprovalService.IncidentApprovalServiceClient _itsmIncidentApprovalService;
    private readonly ILogger<OnboardingPlanLifecycleEndpoint> _logger;
    private readonly ApiOnboarding.EquipmentCalculationService.EquipmentCalculationServiceClient _onboardingEquipmentCalculationService;
    private readonly ApiOnboarding.OnboardingPlanCalculationService.OnboardingPlanCalculationServiceClient _onboardingOnboardingPlanCalculationService;
    private readonly ApiOnboarding.OnboardingPlanQueryService.OnboardingPlanQueryServiceClient _onboardingOnboardingPlanQueryService;
    private readonly ApiOnboarding.OnboardingPlanValidationService.OnboardingPlanValidationServiceClient _onboardingOnboardingPlanValidationService;
    private readonly ApiOnboarding.OnboardingTaskCalculationService.OnboardingTaskCalculationServiceClient _onboardingOnboardingTaskCalculationService;
    private readonly ApiOnboarding.OnboardingTaskReportingService.OnboardingTaskReportingServiceClient _onboardingOnboardingTaskReportingService;
    private readonly ApiOnboarding.OnboardingTaskValidationService.OnboardingTaskValidationServiceClient _onboardingOnboardingTaskValidationService;
    private readonly IOnboardingPlanLifecycleEndpointSendRenewOnboardingPlan0Port _onboardingPlanLifecycleEndpointSendRenewOnboardingPlan0Port;
    private readonly ApiPerformers.PerformerQueryService.PerformerQueryServiceClient _performersPerformerQueryService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiOnboarding.PreviewOnboardingPlanRequest, CallOptions, AsyncUnaryCall<ApiOnboarding.PreviewOnboardingPlanResponse>> _sendTerminateOnboardingPlan0;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiVenues.VenueService.VenueServiceClient _venuesVenueService;

    public OnboardingPlanLifecycleEndpoint(
        ApiBenefits.EnrollmentApprovalService.EnrollmentApprovalServiceClient benefitsEnrollmentApprovalService,
        NotificationBus bus,
        ApiDocuments.DocumentFolderQueryService.DocumentFolderQueryServiceClient documentsDocumentFolderQueryService,
        ApiDocuments.DocumentNotificationService.DocumentNotificationServiceClient documentsDocumentNotificationService,
        ApiDocuments.DocumentService.DocumentServiceClient documentsDocumentService,
        CallInvoker invoker,
        ApiItsm.IncidentApprovalService.IncidentApprovalServiceClient itsmIncidentApprovalService,
        ILogger<OnboardingPlanLifecycleEndpoint> logger,
        ApiOnboarding.EquipmentCalculationService.EquipmentCalculationServiceClient onboardingEquipmentCalculationService,
        ApiOnboarding.OnboardingPlanCalculationService.OnboardingPlanCalculationServiceClient onboardingOnboardingPlanCalculationService,
        ApiOnboarding.OnboardingPlanQueryService.OnboardingPlanQueryServiceClient onboardingOnboardingPlanQueryService,
        ApiOnboarding.OnboardingPlanValidationService.OnboardingPlanValidationServiceClient onboardingOnboardingPlanValidationService,
        ApiOnboarding.OnboardingTaskCalculationService.OnboardingTaskCalculationServiceClient onboardingOnboardingTaskCalculationService,
        ApiOnboarding.OnboardingTaskReportingService.OnboardingTaskReportingServiceClient onboardingOnboardingTaskReportingService,
        ApiOnboarding.OnboardingTaskValidationService.OnboardingTaskValidationServiceClient onboardingOnboardingTaskValidationService,
        IOnboardingPlanLifecycleEndpointSendRenewOnboardingPlan0Port onboardingPlanLifecycleEndpointSendRenewOnboardingPlan0PortAdapter,
        ApiPerformers.PerformerQueryService.PerformerQueryServiceClient performersPerformerQueryService,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiVenues.VenueService.VenueServiceClient venuesVenueService)
    {
        _benefitsEnrollmentApprovalService = benefitsEnrollmentApprovalService;
        _bus = bus;
        _documentsDocumentFolderQueryService = documentsDocumentFolderQueryService;
        _documentsDocumentNotificationService = documentsDocumentNotificationService;
        _documentsDocumentService = documentsDocumentService;
        _invoker = invoker;
        _itsmIncidentApprovalService = itsmIncidentApprovalService;
        _logger = logger;
        _onboardingEquipmentCalculationService = onboardingEquipmentCalculationService;
        _onboardingOnboardingPlanCalculationService = onboardingOnboardingPlanCalculationService;
        _onboardingOnboardingPlanQueryService = onboardingOnboardingPlanQueryService;
        _onboardingOnboardingPlanValidationService = onboardingOnboardingPlanValidationService;
        _onboardingOnboardingTaskCalculationService = onboardingOnboardingTaskCalculationService;
        _onboardingOnboardingTaskReportingService = onboardingOnboardingTaskReportingService;
        _onboardingOnboardingTaskValidationService = onboardingOnboardingTaskValidationService;
        _onboardingPlanLifecycleEndpointSendRenewOnboardingPlan0Port = onboardingPlanLifecycleEndpointSendRenewOnboardingPlan0PortAdapter;
        _performersPerformerQueryService = performersPerformerQueryService;
        _registry = registry;
        _repository = repository;
        _settings = settings;
        _venuesVenueService = venuesVenueService;
        _sendTerminateOnboardingPlan0 = _onboardingOnboardingPlanValidationService.PreviewOnboardingPlanAsync;
    }
}
