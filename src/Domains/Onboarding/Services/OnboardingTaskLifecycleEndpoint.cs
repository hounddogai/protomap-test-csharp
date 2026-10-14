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
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiSearch = Acme.Search.V1;
using ApiShipping = Acme.Shipping.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Onboarding;

/// <summary>
/// Moves an onboarding task between the states of its lifecycle.
/// </summary>
public sealed partial class OnboardingTaskLifecycleEndpoint : ApiOnboarding.OnboardingTaskLifecycleService.OnboardingTaskLifecycleServiceBase
{
    private readonly ApiBenefits.BenefitPlanWorkflowService.BenefitPlanWorkflowServiceClient _benefitsBenefitPlanWorkflowService;
    private readonly ApiHelpdesk.SlaPolicyApprovalService.SlaPolicyApprovalServiceClient _helpdeskSlaPolicyApprovalService;
    private readonly CallInvoker _invoker;
    private readonly ILogger<OnboardingTaskLifecycleEndpoint> _logger;
    private readonly ApiOnboarding.EquipmentValidationService.EquipmentValidationServiceClient _onboardingEquipmentValidationService;
    private readonly ApiOnboarding.OnboardingPlanQueryService.OnboardingPlanQueryServiceClient _onboardingOnboardingPlanQueryService;
    private readonly ApiOnboarding.OnboardingPlanValidationService.OnboardingPlanValidationServiceClient _onboardingOnboardingPlanValidationService;
    private readonly ApiOnboarding.OnboardingTaskCalculationService.OnboardingTaskCalculationServiceClient _onboardingOnboardingTaskCalculationService;
    private readonly IOnboardingTaskLifecycleEndpointSendSuspendOnboardingTask1Port _onboardingTaskLifecycleEndpointSendSuspendOnboardingTask1Port;
    private readonly ApiPerformance.ReviewWorkflowService.ReviewWorkflowServiceClient _performanceReviewWorkflowService;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiScheduling.ShiftApprovalService.ShiftApprovalServiceClient _schedulingShiftApprovalService;
    private readonly ApiSearch.SearchIndexValidationService.SearchIndexValidationServiceClient _searchSearchIndexValidationService;
    private readonly Func<ApiHelpdesk.ApproveSlaPolicyRequest, CallOptions, AsyncUnaryCall<ApiHelpdesk.ApproveSlaPolicyResponse>> _sendExpireOnboardingTask0;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiShipping.ShipmentQueryService.ShipmentQueryServiceClient _shippingShipmentQueryService;
    private readonly ApiTimekeeping.TimesheetWorkflowService.TimesheetWorkflowServiceClient _timekeepingTimesheetWorkflowService;
    private readonly ApiTravel.TravelBookingWorkflowService.TravelBookingWorkflowServiceClient _travelTravelBookingWorkflowService;

    public OnboardingTaskLifecycleEndpoint(
        ApiBenefits.BenefitPlanWorkflowService.BenefitPlanWorkflowServiceClient benefitsBenefitPlanWorkflowService,
        ApiHelpdesk.SlaPolicyApprovalService.SlaPolicyApprovalServiceClient helpdeskSlaPolicyApprovalService,
        CallInvoker invoker,
        ILogger<OnboardingTaskLifecycleEndpoint> logger,
        ApiOnboarding.EquipmentValidationService.EquipmentValidationServiceClient onboardingEquipmentValidationService,
        ApiOnboarding.OnboardingPlanQueryService.OnboardingPlanQueryServiceClient onboardingOnboardingPlanQueryService,
        ApiOnboarding.OnboardingPlanValidationService.OnboardingPlanValidationServiceClient onboardingOnboardingPlanValidationService,
        ApiOnboarding.OnboardingTaskCalculationService.OnboardingTaskCalculationServiceClient onboardingOnboardingTaskCalculationService,
        IOnboardingTaskLifecycleEndpointSendSuspendOnboardingTask1Port onboardingTaskLifecycleEndpointSendSuspendOnboardingTask1PortAdapter,
        ApiPerformance.ReviewWorkflowService.ReviewWorkflowServiceClient performanceReviewWorkflowService,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiScheduling.ShiftApprovalService.ShiftApprovalServiceClient schedulingShiftApprovalService,
        ApiSearch.SearchIndexValidationService.SearchIndexValidationServiceClient searchSearchIndexValidationService,
        IOptions<MeshOptions> settings,
        ApiShipping.ShipmentQueryService.ShipmentQueryServiceClient shippingShipmentQueryService,
        ApiTimekeeping.TimesheetWorkflowService.TimesheetWorkflowServiceClient timekeepingTimesheetWorkflowService,
        ApiTravel.TravelBookingWorkflowService.TravelBookingWorkflowServiceClient travelTravelBookingWorkflowService)
    {
        _benefitsBenefitPlanWorkflowService = benefitsBenefitPlanWorkflowService;
        _helpdeskSlaPolicyApprovalService = helpdeskSlaPolicyApprovalService;
        _invoker = invoker;
        _logger = logger;
        _onboardingEquipmentValidationService = onboardingEquipmentValidationService;
        _onboardingOnboardingPlanQueryService = onboardingOnboardingPlanQueryService;
        _onboardingOnboardingPlanValidationService = onboardingOnboardingPlanValidationService;
        _onboardingOnboardingTaskCalculationService = onboardingOnboardingTaskCalculationService;
        _onboardingTaskLifecycleEndpointSendSuspendOnboardingTask1Port = onboardingTaskLifecycleEndpointSendSuspendOnboardingTask1PortAdapter;
        _performanceReviewWorkflowService = performanceReviewWorkflowService;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _schedulingShiftApprovalService = schedulingShiftApprovalService;
        _searchSearchIndexValidationService = searchSearchIndexValidationService;
        _settings = settings;
        _shippingShipmentQueryService = shippingShipmentQueryService;
        _timekeepingTimesheetWorkflowService = timekeepingTimesheetWorkflowService;
        _travelTravelBookingWorkflowService = travelTravelBookingWorkflowService;
        _sendExpireOnboardingTask0 = _helpdeskSlaPolicyApprovalService.ApproveSlaPolicyAsync;
    }
}
