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
using ApiAuthorization = Acme.Authorization.V1;
using ApiBenefits = Acme.Benefits.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiPerformance = Acme.Performance.V1;

namespace Acme.Workforce.Onboarding;

/// <summary>
/// Checks onboarding tasks against the rules before anything acts on them.
/// </summary>
public sealed partial class OnboardingTaskValidationEndpoint : ApiOnboarding.OnboardingTaskValidationService.OnboardingTaskValidationServiceBase
{
    private readonly ApiAuthorization.AccessPolicyService.AccessPolicyServiceClient _authorizationAccessPolicyService;
    private readonly ApiBenefits.BenefitPlanQueryService.BenefitPlanQueryServiceClient _benefitsBenefitPlanQueryService;
    private readonly ApiCorporateevents.CorporateEventQueryService.CorporateEventQueryServiceClient _corporateeventsCorporateEventQueryService;
    private readonly ApiCorporateevents.RegistrationCalculationService.RegistrationCalculationServiceClient _corporateeventsRegistrationCalculationService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.IncidentQueryService.IncidentQueryServiceClient _itsmIncidentQueryService;
    private readonly ApiItsm.ProblemValidationService.ProblemValidationServiceClient _itsmProblemValidationService;
    private readonly ILogger<OnboardingTaskValidationEndpoint> _logger;
    private readonly ApiOnboarding.EquipmentCalculationService.EquipmentCalculationServiceClient _onboardingEquipmentCalculationService;
    private readonly ApiOnboarding.OnboardingPlanCalculationService.OnboardingPlanCalculationServiceClient _onboardingOnboardingPlanCalculationService;
    private readonly ApiOnboarding.OnboardingPlanValidationService.OnboardingPlanValidationServiceClient _onboardingOnboardingPlanValidationService;
    private readonly IOnboardingTaskValidationEndpointSendGetOnboardingTaskRules0Port _onboardingTaskValidationEndpointSendGetOnboardingTaskRules0Port;
    private readonly ApiPerformance.GoalValidationService.GoalValidationServiceClient _performanceGoalValidationService;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiItsm.LintProblemsRequest, CallOptions, AsyncUnaryCall<ApiItsm.LintProblemsResponse>> _sendValidateOnboardingTask1;
    private readonly IOptions<MeshOptions> _settings;

    public OnboardingTaskValidationEndpoint(
        ApiAuthorization.AccessPolicyService.AccessPolicyServiceClient authorizationAccessPolicyService,
        ApiBenefits.BenefitPlanQueryService.BenefitPlanQueryServiceClient benefitsBenefitPlanQueryService,
        ApiCorporateevents.CorporateEventQueryService.CorporateEventQueryServiceClient corporateeventsCorporateEventQueryService,
        ApiCorporateevents.RegistrationCalculationService.RegistrationCalculationServiceClient corporateeventsRegistrationCalculationService,
        CallInvoker invoker,
        ApiItsm.IncidentQueryService.IncidentQueryServiceClient itsmIncidentQueryService,
        ApiItsm.ProblemValidationService.ProblemValidationServiceClient itsmProblemValidationService,
        ILogger<OnboardingTaskValidationEndpoint> logger,
        ApiOnboarding.EquipmentCalculationService.EquipmentCalculationServiceClient onboardingEquipmentCalculationService,
        ApiOnboarding.OnboardingPlanCalculationService.OnboardingPlanCalculationServiceClient onboardingOnboardingPlanCalculationService,
        ApiOnboarding.OnboardingPlanValidationService.OnboardingPlanValidationServiceClient onboardingOnboardingPlanValidationService,
        IOnboardingTaskValidationEndpointSendGetOnboardingTaskRules0Port onboardingTaskValidationEndpointSendGetOnboardingTaskRules0PortAdapter,
        ApiPerformance.GoalValidationService.GoalValidationServiceClient performanceGoalValidationService,
        IRecordRepository repository,
        IOptions<MeshOptions> settings)
    {
        _authorizationAccessPolicyService = authorizationAccessPolicyService;
        _benefitsBenefitPlanQueryService = benefitsBenefitPlanQueryService;
        _corporateeventsCorporateEventQueryService = corporateeventsCorporateEventQueryService;
        _corporateeventsRegistrationCalculationService = corporateeventsRegistrationCalculationService;
        _invoker = invoker;
        _itsmIncidentQueryService = itsmIncidentQueryService;
        _itsmProblemValidationService = itsmProblemValidationService;
        _logger = logger;
        _onboardingEquipmentCalculationService = onboardingEquipmentCalculationService;
        _onboardingOnboardingPlanCalculationService = onboardingOnboardingPlanCalculationService;
        _onboardingOnboardingPlanValidationService = onboardingOnboardingPlanValidationService;
        _onboardingTaskValidationEndpointSendGetOnboardingTaskRules0Port = onboardingTaskValidationEndpointSendGetOnboardingTaskRules0PortAdapter;
        _performanceGoalValidationService = performanceGoalValidationService;
        _repository = repository;
        _settings = settings;
        _sendValidateOnboardingTask1 = _itsmProblemValidationService.LintProblemsAsync;
    }
}
