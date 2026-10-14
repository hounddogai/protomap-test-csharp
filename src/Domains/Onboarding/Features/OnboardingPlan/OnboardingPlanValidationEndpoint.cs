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
using ApiAudiences = Acme.Audiences.V1;
using ApiBenefits = Acme.Benefits.V1;
using ApiChat = Acme.Chat.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiDrivers = Acme.Drivers.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Onboarding;

/// <summary>
/// Checks onboarding plans against the rules before anything acts on them.
/// </summary>
public sealed partial class OnboardingPlanValidationEndpoint : ApiOnboarding.OnboardingPlanValidationService.OnboardingPlanValidationServiceBase
{
    private readonly ApiAudiences.AudienceQueryService.AudienceQueryServiceClient _audiencesAudienceQueryService;
    private readonly ApiBenefits.BenefitPlanService.BenefitPlanServiceClient _benefitsBenefitPlanService;
    private readonly NotificationBus _bus;
    private readonly ApiChat.ChatRoomService.ChatRoomServiceClient _chatChatRoomService;
    private readonly ApiDocuments.DocumentTemplateAdminService.DocumentTemplateAdminServiceClient _documentsDocumentTemplateAdminService;
    private readonly ApiDrivers.DriverService.DriverServiceClient _driversDriverService;
    private readonly ApiHelpdesk.SlaPolicyCalculationService.SlaPolicyCalculationServiceClient _helpdeskSlaPolicyCalculationService;
    private readonly ApiItsm.IncidentCalculationService.IncidentCalculationServiceClient _itsmIncidentCalculationService;
    private readonly ILogger<OnboardingPlanValidationEndpoint> _logger;
    private readonly ApiOnboarding.OnboardingPlanCalculationService.OnboardingPlanCalculationServiceClient _onboardingOnboardingPlanCalculationService;
    private readonly ApiOnboarding.OnboardingTaskCalculationService.OnboardingTaskCalculationServiceClient _onboardingOnboardingTaskCalculationService;
    private readonly IOnboardingPlanValidationEndpointSendValidateOnboardingPlan0Port _onboardingPlanValidationEndpointSendValidateOnboardingPlan0Port;
    private readonly RetryPolicy _policy;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiAudiences.ResolveAudiencesRequest, CallOptions, AsyncUnaryCall<ApiAudiences.ResolveAudiencesResponse>> _sendNormalizeOnboardingPlan1;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTimekeeping.OvertimeValidationService.OvertimeValidationServiceClient _timekeepingOvertimeValidationService;

    public OnboardingPlanValidationEndpoint(
        ApiAudiences.AudienceQueryService.AudienceQueryServiceClient audiencesAudienceQueryService,
        ApiBenefits.BenefitPlanService.BenefitPlanServiceClient benefitsBenefitPlanService,
        NotificationBus bus,
        ApiChat.ChatRoomService.ChatRoomServiceClient chatChatRoomService,
        ApiDocuments.DocumentTemplateAdminService.DocumentTemplateAdminServiceClient documentsDocumentTemplateAdminService,
        ApiDrivers.DriverService.DriverServiceClient driversDriverService,
        ApiHelpdesk.SlaPolicyCalculationService.SlaPolicyCalculationServiceClient helpdeskSlaPolicyCalculationService,
        ApiItsm.IncidentCalculationService.IncidentCalculationServiceClient itsmIncidentCalculationService,
        ILogger<OnboardingPlanValidationEndpoint> logger,
        ApiOnboarding.OnboardingPlanCalculationService.OnboardingPlanCalculationServiceClient onboardingOnboardingPlanCalculationService,
        ApiOnboarding.OnboardingTaskCalculationService.OnboardingTaskCalculationServiceClient onboardingOnboardingTaskCalculationService,
        IOnboardingPlanValidationEndpointSendValidateOnboardingPlan0Port onboardingPlanValidationEndpointSendValidateOnboardingPlan0PortAdapter,
        RetryPolicy policy,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiTimekeeping.OvertimeValidationService.OvertimeValidationServiceClient timekeepingOvertimeValidationService)
    {
        _audiencesAudienceQueryService = audiencesAudienceQueryService;
        _benefitsBenefitPlanService = benefitsBenefitPlanService;
        _bus = bus;
        _chatChatRoomService = chatChatRoomService;
        _documentsDocumentTemplateAdminService = documentsDocumentTemplateAdminService;
        _driversDriverService = driversDriverService;
        _helpdeskSlaPolicyCalculationService = helpdeskSlaPolicyCalculationService;
        _itsmIncidentCalculationService = itsmIncidentCalculationService;
        _logger = logger;
        _onboardingOnboardingPlanCalculationService = onboardingOnboardingPlanCalculationService;
        _onboardingOnboardingTaskCalculationService = onboardingOnboardingTaskCalculationService;
        _onboardingPlanValidationEndpointSendValidateOnboardingPlan0Port = onboardingPlanValidationEndpointSendValidateOnboardingPlan0PortAdapter;
        _policy = policy;
        _repository = repository;
        _settings = settings;
        _timekeepingOvertimeValidationService = timekeepingOvertimeValidationService;
        _sendNormalizeOnboardingPlan1 = _audiencesAudienceQueryService.ResolveAudiencesAsync;
    }
}
