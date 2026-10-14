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
using ApiCustomers = Acme.Customers.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFeaturestore = Acme.Featurestore.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLabeling = Acme.Labeling.V1;
using ApiPayroll = Acme.Payroll.V1;
using ApiRatelimiting = Acme.Ratelimiting.V1;
using ApiScheduling = Acme.Scheduling.V1;

namespace Acme.Workforce.Scheduling;

/// <summary>
/// Runs the long-lived workflow that takes a roster from start to finish.
/// </summary>
public sealed partial class RosterWorkflowEndpoint : ApiScheduling.RosterWorkflowService.RosterWorkflowServiceBase
{
    private readonly ApiBenefits.DependentWorkflowService.DependentWorkflowServiceClient _benefitsDependentWorkflowService;
    private readonly ApiCustomers.CustomerCalculationService.CustomerCalculationServiceClient _customersCustomerCalculationService;
    private readonly ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient _employeesEmploymentContractValidationService;
    private readonly ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient _employeesOrgUnitQueryService;
    private readonly ApiEmployees.PositionValidationService.PositionValidationServiceClient _employeesPositionValidationService;
    private readonly ApiFeaturestore.FeatureViewCalculationService.FeatureViewCalculationServiceClient _featurestoreFeatureViewCalculationService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.ChangeRequestWorkflowService.ChangeRequestWorkflowServiceClient _itsmChangeRequestWorkflowService;
    private readonly ApiLabeling.LabelingTaskCalculationService.LabelingTaskCalculationServiceClient _labelingLabelingTaskCalculationService;
    private readonly ILogger<RosterWorkflowEndpoint> _logger;
    private readonly ApiPayroll.PayRunService.PayRunServiceClient _payrollPayRunService;
    private readonly ApiRatelimiting.RateLimitCalculationService.RateLimitCalculationServiceClient _ratelimitingRateLimitCalculationService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly IRosterWorkflowEndpointSendRetryRosterWorkflow2Port _rosterWorkflowEndpointSendRetryRosterWorkflow2Port;
    private readonly ApiScheduling.AvailabilityWindowLifecycleService.AvailabilityWindowLifecycleServiceClient _schedulingAvailabilityWindowLifecycleService;
    private readonly ApiScheduling.AvailabilityWindowValidationService.AvailabilityWindowValidationServiceClient _schedulingAvailabilityWindowValidationService;
    private readonly ApiScheduling.RosterQueryService.RosterQueryServiceClient _schedulingRosterQueryService;
    private readonly ApiScheduling.ShiftApprovalService.ShiftApprovalServiceClient _schedulingShiftApprovalService;
    private readonly ApiScheduling.ShiftValidationService.ShiftValidationServiceClient _schedulingShiftValidationService;
    private readonly ApiScheduling.ShiftWorkflowService.ShiftWorkflowServiceClient _schedulingShiftWorkflowService;
    private readonly Func<ApiScheduling.ListShiftWorkflowsRequest, CallOptions, AsyncUnaryCall<ApiScheduling.ListShiftWorkflowsResponse>> _sendListRosterWorkflows0;

    public RosterWorkflowEndpoint(
        ApiBenefits.DependentWorkflowService.DependentWorkflowServiceClient benefitsDependentWorkflowService,
        ApiCustomers.CustomerCalculationService.CustomerCalculationServiceClient customersCustomerCalculationService,
        ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient employeesEmploymentContractValidationService,
        ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient employeesOrgUnitQueryService,
        ApiEmployees.PositionValidationService.PositionValidationServiceClient employeesPositionValidationService,
        ApiFeaturestore.FeatureViewCalculationService.FeatureViewCalculationServiceClient featurestoreFeatureViewCalculationService,
        CallInvoker invoker,
        ApiItsm.ChangeRequestWorkflowService.ChangeRequestWorkflowServiceClient itsmChangeRequestWorkflowService,
        ApiLabeling.LabelingTaskCalculationService.LabelingTaskCalculationServiceClient labelingLabelingTaskCalculationService,
        ILogger<RosterWorkflowEndpoint> logger,
        ApiPayroll.PayRunService.PayRunServiceClient payrollPayRunService,
        ApiRatelimiting.RateLimitCalculationService.RateLimitCalculationServiceClient ratelimitingRateLimitCalculationService,
        ClientRegistry registry,
        IRecordRepository repository,
        IRosterWorkflowEndpointSendRetryRosterWorkflow2Port rosterWorkflowEndpointSendRetryRosterWorkflow2PortAdapter,
        ApiScheduling.AvailabilityWindowLifecycleService.AvailabilityWindowLifecycleServiceClient schedulingAvailabilityWindowLifecycleService,
        ApiScheduling.AvailabilityWindowValidationService.AvailabilityWindowValidationServiceClient schedulingAvailabilityWindowValidationService,
        ApiScheduling.RosterQueryService.RosterQueryServiceClient schedulingRosterQueryService,
        ApiScheduling.ShiftApprovalService.ShiftApprovalServiceClient schedulingShiftApprovalService,
        ApiScheduling.ShiftValidationService.ShiftValidationServiceClient schedulingShiftValidationService,
        ApiScheduling.ShiftWorkflowService.ShiftWorkflowServiceClient schedulingShiftWorkflowService)
    {
        _benefitsDependentWorkflowService = benefitsDependentWorkflowService;
        _customersCustomerCalculationService = customersCustomerCalculationService;
        _employeesEmploymentContractValidationService = employeesEmploymentContractValidationService;
        _employeesOrgUnitQueryService = employeesOrgUnitQueryService;
        _employeesPositionValidationService = employeesPositionValidationService;
        _featurestoreFeatureViewCalculationService = featurestoreFeatureViewCalculationService;
        _invoker = invoker;
        _itsmChangeRequestWorkflowService = itsmChangeRequestWorkflowService;
        _labelingLabelingTaskCalculationService = labelingLabelingTaskCalculationService;
        _logger = logger;
        _payrollPayRunService = payrollPayRunService;
        _ratelimitingRateLimitCalculationService = ratelimitingRateLimitCalculationService;
        _registry = registry;
        _repository = repository;
        _rosterWorkflowEndpointSendRetryRosterWorkflow2Port = rosterWorkflowEndpointSendRetryRosterWorkflow2PortAdapter;
        _schedulingAvailabilityWindowLifecycleService = schedulingAvailabilityWindowLifecycleService;
        _schedulingAvailabilityWindowValidationService = schedulingAvailabilityWindowValidationService;
        _schedulingRosterQueryService = schedulingRosterQueryService;
        _schedulingShiftApprovalService = schedulingShiftApprovalService;
        _schedulingShiftValidationService = schedulingShiftValidationService;
        _schedulingShiftWorkflowService = schedulingShiftWorkflowService;
        _sendListRosterWorkflows0 = _schedulingShiftWorkflowService.ListShiftWorkflowsAsync;
    }
}
