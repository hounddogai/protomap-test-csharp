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
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiTravel = Acme.Travel.V1;
using ApiTreasury = Acme.Treasury.V1;

namespace Acme.Workforce.Timekeeping;

/// <summary>
/// Runs the long-lived workflow that takes a timesheet from start to finish.
/// </summary>
public sealed partial class TimesheetWorkflowEndpoint : ApiTimekeeping.TimesheetWorkflowService.TimesheetWorkflowServiceBase
{
    private readonly ApiAuthorization.AccessPolicyService.AccessPolicyServiceClient _authorizationAccessPolicyService;
    private readonly NotificationBus _bus;
    private readonly ApiEmployees.EmployeeService.EmployeeServiceClient _employeesEmployeeService;
    private readonly ApiHelpdesk.SlaPolicyWorkflowService.SlaPolicyWorkflowServiceClient _helpdeskSlaPolicyWorkflowService;
    private readonly CallInvoker _invoker;
    private readonly ApiLearning.CourseApprovalService.CourseApprovalServiceClient _learningCourseApprovalService;
    private readonly ILogger<TimesheetWorkflowEndpoint> _logger;
    private readonly ApiMetrics.MetricSeriesValidationService.MetricSeriesValidationServiceClient _metricsMetricSeriesValidationService;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiLearning.RecallCourseRequest, CallOptions, AsyncUnaryCall<ApiLearning.RecallCourseResponse>> _sendPauseTimesheetWorkflow3;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTimekeeping.ClockEventApprovalService.ClockEventApprovalServiceClient _timekeepingClockEventApprovalService;
    private readonly ApiTimekeeping.ClockEventQueryService.ClockEventQueryServiceClient _timekeepingClockEventQueryService;
    private readonly ApiTimekeeping.OvertimeCalculationService.OvertimeCalculationServiceClient _timekeepingOvertimeCalculationService;
    private readonly ApiTimekeeping.TimesheetValidationService.TimesheetValidationServiceClient _timekeepingTimesheetValidationService;
    private readonly ITimesheetWorkflowEndpointSendStartTimesheetWorkflow1Port _timesheetWorkflowEndpointSendStartTimesheetWorkflow1Port;
    private readonly ApiTravel.ItineraryLifecycleService.ItineraryLifecycleServiceClient _travelItineraryLifecycleService;
    private readonly ApiTravel.TravelBookingWorkflowService.TravelBookingWorkflowServiceClient _travelTravelBookingWorkflowService;
    private readonly ApiTreasury.CashPositionService.CashPositionServiceClient _treasuryCashPositionService;

    public TimesheetWorkflowEndpoint(
        ApiAuthorization.AccessPolicyService.AccessPolicyServiceClient authorizationAccessPolicyService,
        NotificationBus bus,
        ApiEmployees.EmployeeService.EmployeeServiceClient employeesEmployeeService,
        ApiHelpdesk.SlaPolicyWorkflowService.SlaPolicyWorkflowServiceClient helpdeskSlaPolicyWorkflowService,
        CallInvoker invoker,
        ApiLearning.CourseApprovalService.CourseApprovalServiceClient learningCourseApprovalService,
        ILogger<TimesheetWorkflowEndpoint> logger,
        ApiMetrics.MetricSeriesValidationService.MetricSeriesValidationServiceClient metricsMetricSeriesValidationService,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiTimekeeping.ClockEventApprovalService.ClockEventApprovalServiceClient timekeepingClockEventApprovalService,
        ApiTimekeeping.ClockEventQueryService.ClockEventQueryServiceClient timekeepingClockEventQueryService,
        ApiTimekeeping.OvertimeCalculationService.OvertimeCalculationServiceClient timekeepingOvertimeCalculationService,
        ApiTimekeeping.TimesheetValidationService.TimesheetValidationServiceClient timekeepingTimesheetValidationService,
        ITimesheetWorkflowEndpointSendStartTimesheetWorkflow1Port timesheetWorkflowEndpointSendStartTimesheetWorkflow1PortAdapter,
        ApiTravel.ItineraryLifecycleService.ItineraryLifecycleServiceClient travelItineraryLifecycleService,
        ApiTravel.TravelBookingWorkflowService.TravelBookingWorkflowServiceClient travelTravelBookingWorkflowService,
        ApiTreasury.CashPositionService.CashPositionServiceClient treasuryCashPositionService)
    {
        _authorizationAccessPolicyService = authorizationAccessPolicyService;
        _bus = bus;
        _employeesEmployeeService = employeesEmployeeService;
        _helpdeskSlaPolicyWorkflowService = helpdeskSlaPolicyWorkflowService;
        _invoker = invoker;
        _learningCourseApprovalService = learningCourseApprovalService;
        _logger = logger;
        _metricsMetricSeriesValidationService = metricsMetricSeriesValidationService;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _settings = settings;
        _timekeepingClockEventApprovalService = timekeepingClockEventApprovalService;
        _timekeepingClockEventQueryService = timekeepingClockEventQueryService;
        _timekeepingOvertimeCalculationService = timekeepingOvertimeCalculationService;
        _timekeepingTimesheetValidationService = timekeepingTimesheetValidationService;
        _timesheetWorkflowEndpointSendStartTimesheetWorkflow1Port = timesheetWorkflowEndpointSendStartTimesheetWorkflow1PortAdapter;
        _travelItineraryLifecycleService = travelItineraryLifecycleService;
        _travelTravelBookingWorkflowService = travelTravelBookingWorkflowService;
        _treasuryCashPositionService = treasuryCashPositionService;
        _sendPauseTimesheetWorkflow3 = _learningCourseApprovalService.RecallCourseAsync;
    }
}
