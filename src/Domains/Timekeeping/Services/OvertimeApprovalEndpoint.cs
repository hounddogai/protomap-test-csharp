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
using ApiContent = Acme.Content.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Timekeeping;

/// <summary>
/// Routes overtimes through approval, with escalation and delegation.
/// </summary>
public sealed partial class OvertimeApprovalEndpoint : ApiTimekeeping.OvertimeApprovalService.OvertimeApprovalServiceBase
{
    private readonly NotificationBus _bus;
    private readonly ApiContent.ArticleService.ArticleServiceClient _contentArticleService;
    private readonly ApiEmployees.EmployeeService.EmployeeServiceClient _employeesEmployeeService;
    private readonly ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient _employeesOrgUnitQueryService;
    private readonly ApiEmployees.PositionValidationService.PositionValidationServiceClient _employeesPositionValidationService;
    private readonly ApiHelpdesk.SlaPolicyApprovalService.SlaPolicyApprovalServiceClient _helpdeskSlaPolicyApprovalService;
    private readonly ApiLedger.LedgerAccountService.LedgerAccountServiceClient _ledgerLedgerAccountService;
    private readonly ILogger<OvertimeApprovalEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiContent.SearchArticlesRequest, CallOptions, AsyncUnaryCall<ApiContent.SearchArticlesResponse>> _sendApproveOvertime3;
    private readonly ApiTimekeeping.ClockEventApprovalService.ClockEventApprovalServiceClient _timekeepingClockEventApprovalService;
    private readonly ApiTimekeeping.ClockEventValidationService.ClockEventValidationServiceClient _timekeepingClockEventValidationService;
    private readonly ApiTimekeeping.OvertimeCalculationService.OvertimeCalculationServiceClient _timekeepingOvertimeCalculationService;
    private readonly ApiTimekeeping.OvertimeLifecycleService.OvertimeLifecycleServiceClient _timekeepingOvertimeLifecycleService;
    private readonly ApiTimekeeping.TimesheetApprovalService.TimesheetApprovalServiceClient _timekeepingTimesheetApprovalService;
    private readonly ApiTimekeeping.TimesheetService.TimesheetServiceClient _timekeepingTimesheetService;
    private readonly ApiTravel.TravelPolicySyncService.TravelPolicySyncServiceClient _travelTravelPolicySyncService;

    public OvertimeApprovalEndpoint(
        NotificationBus bus,
        ApiContent.ArticleService.ArticleServiceClient contentArticleService,
        ApiEmployees.EmployeeService.EmployeeServiceClient employeesEmployeeService,
        ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient employeesOrgUnitQueryService,
        ApiEmployees.PositionValidationService.PositionValidationServiceClient employeesPositionValidationService,
        ApiHelpdesk.SlaPolicyApprovalService.SlaPolicyApprovalServiceClient helpdeskSlaPolicyApprovalService,
        ApiLedger.LedgerAccountService.LedgerAccountServiceClient ledgerLedgerAccountService,
        ILogger<OvertimeApprovalEndpoint> logger,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiTimekeeping.ClockEventApprovalService.ClockEventApprovalServiceClient timekeepingClockEventApprovalService,
        ApiTimekeeping.ClockEventValidationService.ClockEventValidationServiceClient timekeepingClockEventValidationService,
        ApiTimekeeping.OvertimeCalculationService.OvertimeCalculationServiceClient timekeepingOvertimeCalculationService,
        ApiTimekeeping.OvertimeLifecycleService.OvertimeLifecycleServiceClient timekeepingOvertimeLifecycleService,
        ApiTimekeeping.TimesheetApprovalService.TimesheetApprovalServiceClient timekeepingTimesheetApprovalService,
        ApiTimekeeping.TimesheetService.TimesheetServiceClient timekeepingTimesheetService,
        ApiTravel.TravelPolicySyncService.TravelPolicySyncServiceClient travelTravelPolicySyncService)
    {
        _bus = bus;
        _contentArticleService = contentArticleService;
        _employeesEmployeeService = employeesEmployeeService;
        _employeesOrgUnitQueryService = employeesOrgUnitQueryService;
        _employeesPositionValidationService = employeesPositionValidationService;
        _helpdeskSlaPolicyApprovalService = helpdeskSlaPolicyApprovalService;
        _ledgerLedgerAccountService = ledgerLedgerAccountService;
        _logger = logger;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _timekeepingClockEventApprovalService = timekeepingClockEventApprovalService;
        _timekeepingClockEventValidationService = timekeepingClockEventValidationService;
        _timekeepingOvertimeCalculationService = timekeepingOvertimeCalculationService;
        _timekeepingOvertimeLifecycleService = timekeepingOvertimeLifecycleService;
        _timekeepingTimesheetApprovalService = timekeepingTimesheetApprovalService;
        _timekeepingTimesheetService = timekeepingTimesheetService;
        _travelTravelPolicySyncService = travelTravelPolicySyncService;
        _sendApproveOvertime3 = _contentArticleService.SearchArticlesAsync;
    }
}
