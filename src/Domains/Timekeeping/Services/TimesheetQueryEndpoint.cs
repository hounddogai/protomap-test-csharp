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
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLearning = Acme.Learning.V1;
using ApiMaintenance = Acme.Maintenance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSearch = Acme.Search.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Timekeeping;

/// <summary>
/// Answers read-only questions about timesheets, for screens and reports.
/// </summary>
public sealed partial class TimesheetQueryEndpoint : ApiTimekeeping.TimesheetQueryService.TimesheetQueryServiceBase
{
    private readonly ApiAuthorization.AccessPolicyValidationService.AccessPolicyValidationServiceClient _authorizationAccessPolicyValidationService;
    private readonly NotificationBus _bus;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.ChangeRequestValidationService.ChangeRequestValidationServiceClient _itsmChangeRequestValidationService;
    private readonly ApiLearning.CourseEnrollmentValidationService.CourseEnrollmentValidationServiceClient _learningCourseEnrollmentValidationService;
    private readonly ILogger<TimesheetQueryEndpoint> _logger;
    private readonly ApiMaintenance.WorkOrderQueryService.WorkOrderQueryServiceClient _maintenanceWorkOrderQueryService;
    private readonly RetryPolicy _policy;
    private readonly IRecordRepository _repository;
    private readonly ApiSearch.SearchIndexQueryService.SearchIndexQueryServiceClient _searchSearchIndexQueryService;
    private readonly Func<ApiMaintenance.ResolveWorkOrdersRequest, CallOptions, AsyncUnaryCall<ApiMaintenance.ResolveWorkOrdersResponse>> _sendExportTimesheets1;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTimekeeping.ClockEventValidationService.ClockEventValidationServiceClient _timekeepingClockEventValidationService;
    private readonly ApiTimekeeping.OvertimeCalculationService.OvertimeCalculationServiceClient _timekeepingOvertimeCalculationService;
    private readonly ApiTimekeeping.OvertimeValidationService.OvertimeValidationServiceClient _timekeepingOvertimeValidationService;
    private readonly ApiTimekeeping.TimesheetValidationService.TimesheetValidationServiceClient _timekeepingTimesheetValidationService;
    private readonly ITimesheetQueryEndpointSendSummarizeTimesheets1Port _timesheetQueryEndpointSendSummarizeTimesheets1Port;

    public TimesheetQueryEndpoint(
        ApiAuthorization.AccessPolicyValidationService.AccessPolicyValidationServiceClient authorizationAccessPolicyValidationService,
        NotificationBus bus,
        CallInvoker invoker,
        ApiItsm.ChangeRequestValidationService.ChangeRequestValidationServiceClient itsmChangeRequestValidationService,
        ApiLearning.CourseEnrollmentValidationService.CourseEnrollmentValidationServiceClient learningCourseEnrollmentValidationService,
        ILogger<TimesheetQueryEndpoint> logger,
        ApiMaintenance.WorkOrderQueryService.WorkOrderQueryServiceClient maintenanceWorkOrderQueryService,
        RetryPolicy policy,
        IRecordRepository repository,
        ApiSearch.SearchIndexQueryService.SearchIndexQueryServiceClient searchSearchIndexQueryService,
        IOptions<MeshOptions> settings,
        ApiTimekeeping.ClockEventValidationService.ClockEventValidationServiceClient timekeepingClockEventValidationService,
        ApiTimekeeping.OvertimeCalculationService.OvertimeCalculationServiceClient timekeepingOvertimeCalculationService,
        ApiTimekeeping.OvertimeValidationService.OvertimeValidationServiceClient timekeepingOvertimeValidationService,
        ApiTimekeeping.TimesheetValidationService.TimesheetValidationServiceClient timekeepingTimesheetValidationService,
        ITimesheetQueryEndpointSendSummarizeTimesheets1Port timesheetQueryEndpointSendSummarizeTimesheets1PortAdapter)
    {
        _authorizationAccessPolicyValidationService = authorizationAccessPolicyValidationService;
        _bus = bus;
        _invoker = invoker;
        _itsmChangeRequestValidationService = itsmChangeRequestValidationService;
        _learningCourseEnrollmentValidationService = learningCourseEnrollmentValidationService;
        _logger = logger;
        _maintenanceWorkOrderQueryService = maintenanceWorkOrderQueryService;
        _policy = policy;
        _repository = repository;
        _searchSearchIndexQueryService = searchSearchIndexQueryService;
        _settings = settings;
        _timekeepingClockEventValidationService = timekeepingClockEventValidationService;
        _timekeepingOvertimeCalculationService = timekeepingOvertimeCalculationService;
        _timekeepingOvertimeValidationService = timekeepingOvertimeValidationService;
        _timekeepingTimesheetValidationService = timekeepingTimesheetValidationService;
        _timesheetQueryEndpointSendSummarizeTimesheets1Port = timesheetQueryEndpointSendSummarizeTimesheets1PortAdapter;
        _sendExportTimesheets1 = _maintenanceWorkOrderQueryService.ResolveWorkOrdersAsync;
    }
}
