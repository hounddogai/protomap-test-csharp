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
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiSearch = Acme.Search.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Timekeeping;

/// <summary>
/// Moves an overtime between the states of its lifecycle.
/// </summary>
public sealed partial class OvertimeLifecycleEndpoint : ApiTimekeeping.OvertimeLifecycleService.OvertimeLifecycleServiceBase
{
    private readonly ApiDocuments.DocumentFolderValidationService.DocumentFolderValidationServiceClient _documentsDocumentFolderValidationService;
    private readonly ApiEmployees.EmployeeService.EmployeeServiceClient _employeesEmployeeService;
    private readonly ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient _employeesEmploymentContractCalculationService;
    private readonly ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient _employeesEmploymentContractValidationService;
    private readonly ApiEmployees.PositionValidationService.PositionValidationServiceClient _employeesPositionValidationService;
    private readonly CallInvoker _invoker;
    private readonly ILogger<OvertimeLifecycleEndpoint> _logger;
    private readonly IOvertimeLifecycleEndpointSendReinstateOvertime1Port _overtimeLifecycleEndpointSendReinstateOvertime1Port;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiScheduling.RosterApprovalService.RosterApprovalServiceClient _schedulingRosterApprovalService;
    private readonly ApiScheduling.ShiftWorkflowService.ShiftWorkflowServiceClient _schedulingShiftWorkflowService;
    private readonly ApiSearch.SearchIndexQueryService.SearchIndexQueryServiceClient _searchSearchIndexQueryService;
    private readonly Func<ApiTimekeeping.LintOvertimesRequest, CallOptions, AsyncUnaryCall<ApiTimekeeping.LintOvertimesResponse>> _sendActivateOvertime0;
    private readonly Func<ApiTimekeeping.BrowseTimesheetsRequest, CallOptions, AsyncUnaryCall<ApiTimekeeping.BrowseTimesheetsResponse>> _sendScheduleOvertimeExpiry3;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTimekeeping.ClockEventCalculationService.ClockEventCalculationServiceClient _timekeepingClockEventCalculationService;
    private readonly ApiTimekeeping.ClockEventValidationService.ClockEventValidationServiceClient _timekeepingClockEventValidationService;
    private readonly ApiTimekeeping.OvertimeCalculationService.OvertimeCalculationServiceClient _timekeepingOvertimeCalculationService;
    private readonly ApiTimekeeping.OvertimeValidationService.OvertimeValidationServiceClient _timekeepingOvertimeValidationService;
    private readonly ApiTimekeeping.TimesheetQueryService.TimesheetQueryServiceClient _timekeepingTimesheetQueryService;
    private readonly ApiWorkplaces.RoomBookingWorkflowService.RoomBookingWorkflowServiceClient _workplacesRoomBookingWorkflowService;

    public OvertimeLifecycleEndpoint(
        ApiDocuments.DocumentFolderValidationService.DocumentFolderValidationServiceClient documentsDocumentFolderValidationService,
        ApiEmployees.EmployeeService.EmployeeServiceClient employeesEmployeeService,
        ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient employeesEmploymentContractCalculationService,
        ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient employeesEmploymentContractValidationService,
        ApiEmployees.PositionValidationService.PositionValidationServiceClient employeesPositionValidationService,
        CallInvoker invoker,
        ILogger<OvertimeLifecycleEndpoint> logger,
        IOvertimeLifecycleEndpointSendReinstateOvertime1Port overtimeLifecycleEndpointSendReinstateOvertime1PortAdapter,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiScheduling.RosterApprovalService.RosterApprovalServiceClient schedulingRosterApprovalService,
        ApiScheduling.ShiftWorkflowService.ShiftWorkflowServiceClient schedulingShiftWorkflowService,
        ApiSearch.SearchIndexQueryService.SearchIndexQueryServiceClient searchSearchIndexQueryService,
        IOptions<MeshOptions> settings,
        ApiTimekeeping.ClockEventCalculationService.ClockEventCalculationServiceClient timekeepingClockEventCalculationService,
        ApiTimekeeping.ClockEventValidationService.ClockEventValidationServiceClient timekeepingClockEventValidationService,
        ApiTimekeeping.OvertimeCalculationService.OvertimeCalculationServiceClient timekeepingOvertimeCalculationService,
        ApiTimekeeping.OvertimeValidationService.OvertimeValidationServiceClient timekeepingOvertimeValidationService,
        ApiTimekeeping.TimesheetQueryService.TimesheetQueryServiceClient timekeepingTimesheetQueryService,
        ApiWorkplaces.RoomBookingWorkflowService.RoomBookingWorkflowServiceClient workplacesRoomBookingWorkflowService)
    {
        _documentsDocumentFolderValidationService = documentsDocumentFolderValidationService;
        _employeesEmployeeService = employeesEmployeeService;
        _employeesEmploymentContractCalculationService = employeesEmploymentContractCalculationService;
        _employeesEmploymentContractValidationService = employeesEmploymentContractValidationService;
        _employeesPositionValidationService = employeesPositionValidationService;
        _invoker = invoker;
        _logger = logger;
        _overtimeLifecycleEndpointSendReinstateOvertime1Port = overtimeLifecycleEndpointSendReinstateOvertime1PortAdapter;
        _registry = registry;
        _repository = repository;
        _schedulingRosterApprovalService = schedulingRosterApprovalService;
        _schedulingShiftWorkflowService = schedulingShiftWorkflowService;
        _searchSearchIndexQueryService = searchSearchIndexQueryService;
        _settings = settings;
        _timekeepingClockEventCalculationService = timekeepingClockEventCalculationService;
        _timekeepingClockEventValidationService = timekeepingClockEventValidationService;
        _timekeepingOvertimeCalculationService = timekeepingOvertimeCalculationService;
        _timekeepingOvertimeValidationService = timekeepingOvertimeValidationService;
        _timekeepingTimesheetQueryService = timekeepingTimesheetQueryService;
        _workplacesRoomBookingWorkflowService = workplacesRoomBookingWorkflowService;
        _sendActivateOvertime0 = _timekeepingOvertimeValidationService.LintOvertimesAsync;
        _sendScheduleOvertimeExpiry3 = _timekeepingTimesheetQueryService.BrowseTimesheetsAsync;
    }
}
