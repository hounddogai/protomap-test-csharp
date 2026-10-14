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
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiModelregistry = Acme.Modelregistry.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiScheduling = Acme.Scheduling.V1;

namespace Acme.Workforce.Scheduling;

/// <summary>
/// Runs the long-lived workflow that takes a shift from start to finish.
/// </summary>
public sealed partial class ShiftWorkflowEndpoint : ApiScheduling.ShiftWorkflowService.ShiftWorkflowServiceBase
{
    private readonly ApiEmployees.EmployeeService.EmployeeServiceClient _employeesEmployeeService;
    private readonly ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient _employeesEmploymentContractCalculationService;
    private readonly ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient _employeesOrgUnitQueryService;
    private readonly CallInvoker _invoker;
    private readonly ILogger<ShiftWorkflowEndpoint> _logger;
    private readonly ApiModelregistry.RegisteredModelCalculationService.RegisteredModelCalculationServiceClient _modelregistryRegisteredModelCalculationService;
    private readonly ApiModelregistry.RegisteredModelService.RegisteredModelServiceClient _modelregistryRegisteredModelService;
    private readonly ApiPerformance.GoalWorkflowService.GoalWorkflowServiceClient _performanceGoalWorkflowService;
    private readonly ApiPerformance.ReviewApprovalService.ReviewApprovalServiceClient _performanceReviewApprovalService;
    private readonly RetryPolicy _policy;
    private readonly IRecordRepository _repository;
    private readonly ApiScheduling.AvailabilityWindowLifecycleService.AvailabilityWindowLifecycleServiceClient _schedulingAvailabilityWindowLifecycleService;
    private readonly ApiScheduling.RosterApprovalService.RosterApprovalServiceClient _schedulingRosterApprovalService;
    private readonly ApiScheduling.RosterQueryService.RosterQueryServiceClient _schedulingRosterQueryService;
    private readonly ApiScheduling.ShiftApprovalService.ShiftApprovalServiceClient _schedulingShiftApprovalService;
    private readonly ApiScheduling.ShiftService.ShiftServiceClient _schedulingShiftService;
    private readonly Func<ApiPerformance.SignalGoalWorkflowRequest, CallOptions, AsyncUnaryCall<ApiPerformance.SignalGoalWorkflowResponse>> _sendRetryShiftWorkflow2;
    private readonly IOptions<MeshOptions> _settings;

    public ShiftWorkflowEndpoint(
        ApiEmployees.EmployeeService.EmployeeServiceClient employeesEmployeeService,
        ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient employeesEmploymentContractCalculationService,
        ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient employeesOrgUnitQueryService,
        CallInvoker invoker,
        ILogger<ShiftWorkflowEndpoint> logger,
        ApiModelregistry.RegisteredModelCalculationService.RegisteredModelCalculationServiceClient modelregistryRegisteredModelCalculationService,
        ApiModelregistry.RegisteredModelService.RegisteredModelServiceClient modelregistryRegisteredModelService,
        ApiPerformance.GoalWorkflowService.GoalWorkflowServiceClient performanceGoalWorkflowService,
        ApiPerformance.ReviewApprovalService.ReviewApprovalServiceClient performanceReviewApprovalService,
        RetryPolicy policy,
        IRecordRepository repository,
        ApiScheduling.AvailabilityWindowLifecycleService.AvailabilityWindowLifecycleServiceClient schedulingAvailabilityWindowLifecycleService,
        ApiScheduling.RosterApprovalService.RosterApprovalServiceClient schedulingRosterApprovalService,
        ApiScheduling.RosterQueryService.RosterQueryServiceClient schedulingRosterQueryService,
        ApiScheduling.ShiftApprovalService.ShiftApprovalServiceClient schedulingShiftApprovalService,
        ApiScheduling.ShiftService.ShiftServiceClient schedulingShiftService,
        IOptions<MeshOptions> settings)
    {
        _employeesEmployeeService = employeesEmployeeService;
        _employeesEmploymentContractCalculationService = employeesEmploymentContractCalculationService;
        _employeesOrgUnitQueryService = employeesOrgUnitQueryService;
        _invoker = invoker;
        _logger = logger;
        _modelregistryRegisteredModelCalculationService = modelregistryRegisteredModelCalculationService;
        _modelregistryRegisteredModelService = modelregistryRegisteredModelService;
        _performanceGoalWorkflowService = performanceGoalWorkflowService;
        _performanceReviewApprovalService = performanceReviewApprovalService;
        _policy = policy;
        _repository = repository;
        _schedulingAvailabilityWindowLifecycleService = schedulingAvailabilityWindowLifecycleService;
        _schedulingRosterApprovalService = schedulingRosterApprovalService;
        _schedulingRosterQueryService = schedulingRosterQueryService;
        _schedulingShiftApprovalService = schedulingShiftApprovalService;
        _schedulingShiftService = schedulingShiftService;
        _settings = settings;
        _sendRetryShiftWorkflow2 = _performanceGoalWorkflowService.SignalGoalWorkflowAsync;
    }
}
