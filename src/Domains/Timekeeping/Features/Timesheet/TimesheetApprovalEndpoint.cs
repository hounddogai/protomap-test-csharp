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
using ApiCommon = Acme.Common.V1;
using ApiConfiguration = Acme.Configuration.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Timekeeping;

/// <summary>
/// Routes timesheets through approval, with escalation and delegation.
/// </summary>
public sealed partial class TimesheetApprovalEndpoint : ApiTimekeeping.TimesheetApprovalService.TimesheetApprovalServiceBase
{
    private readonly ApiAuthorization.AccessPolicyValidationService.AccessPolicyValidationServiceClient _authorizationAccessPolicyValidationService;
    private readonly ApiConfiguration.ConfigSetQueryService.ConfigSetQueryServiceClient _configurationConfigSetQueryService;
    private readonly ApiEmployees.EmployeeService.EmployeeServiceClient _employeesEmployeeService;
    private readonly ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient _employeesOrgUnitQueryService;
    private readonly CallInvoker _invoker;
    private readonly ILogger<TimesheetApprovalEndpoint> _logger;
    private readonly IRecordRepository _repository;
    private readonly ApiScheduling.ShiftApprovalService.ShiftApprovalServiceClient _schedulingShiftApprovalService;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTimekeeping.ClockEventCalculationService.ClockEventCalculationServiceClient _timekeepingClockEventCalculationService;
    private readonly ApiTimekeeping.TimesheetQueryService.TimesheetQueryServiceClient _timekeepingTimesheetQueryService;
    private readonly ApiWorkplaces.RoomBookingReportingService.RoomBookingReportingServiceClient _workplacesRoomBookingReportingService;

    public TimesheetApprovalEndpoint(
        ApiAuthorization.AccessPolicyValidationService.AccessPolicyValidationServiceClient authorizationAccessPolicyValidationService,
        ApiConfiguration.ConfigSetQueryService.ConfigSetQueryServiceClient configurationConfigSetQueryService,
        ApiEmployees.EmployeeService.EmployeeServiceClient employeesEmployeeService,
        ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient employeesOrgUnitQueryService,
        CallInvoker invoker,
        ILogger<TimesheetApprovalEndpoint> logger,
        IRecordRepository repository,
        ApiScheduling.ShiftApprovalService.ShiftApprovalServiceClient schedulingShiftApprovalService,
        IOptions<MeshOptions> settings,
        ApiTimekeeping.ClockEventCalculationService.ClockEventCalculationServiceClient timekeepingClockEventCalculationService,
        ApiTimekeeping.TimesheetQueryService.TimesheetQueryServiceClient timekeepingTimesheetQueryService,
        ApiWorkplaces.RoomBookingReportingService.RoomBookingReportingServiceClient workplacesRoomBookingReportingService)
    {
        _authorizationAccessPolicyValidationService = authorizationAccessPolicyValidationService;
        _configurationConfigSetQueryService = configurationConfigSetQueryService;
        _employeesEmployeeService = employeesEmployeeService;
        _employeesOrgUnitQueryService = employeesOrgUnitQueryService;
        _invoker = invoker;
        _logger = logger;
        _repository = repository;
        _schedulingShiftApprovalService = schedulingShiftApprovalService;
        _settings = settings;
        _timekeepingClockEventCalculationService = timekeepingClockEventCalculationService;
        _timekeepingTimesheetQueryService = timekeepingTimesheetQueryService;
        _workplacesRoomBookingReportingService = workplacesRoomBookingReportingService;
    }
}
