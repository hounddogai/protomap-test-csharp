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
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiSurveys = Acme.Surveys.V1;

namespace Acme.Workforce.Scheduling;

/// <summary>
/// Routes rosters through approval, with escalation and delegation.
/// </summary>
public sealed partial class RosterApprovalEndpoint : ApiScheduling.RosterApprovalService.RosterApprovalServiceBase
{
    private readonly ApiBenefits.DependentApprovalService.DependentApprovalServiceClient _benefitsDependentApprovalService;
    private readonly ApiDocuments.DocumentFolderSyncService.DocumentFolderSyncServiceClient _documentsDocumentFolderSyncService;
    private readonly ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient _employeesEmployeeQueryService;
    private readonly ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient _employeesEmploymentContractCalculationService;
    private readonly ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient _employeesOrgUnitQueryService;
    private readonly ApiHelpdesk.EscalationReportingService.EscalationReportingServiceClient _helpdeskEscalationReportingService;
    private readonly ApiIdentity.UserQueryService.UserQueryServiceClient _identityUserQueryService;
    private readonly CallInvoker _invoker;
    private readonly ApiLicensing.LicenseAssignmentWorkflowService.LicenseAssignmentWorkflowServiceClient _licensingLicenseAssignmentWorkflowService;
    private readonly ILogger<RosterApprovalEndpoint> _logger;
    private readonly ApiMetrics.MetricSeriesValidationService.MetricSeriesValidationServiceClient _metricsMetricSeriesValidationService;
    private readonly ApiNotifications.NotificationService.NotificationServiceClient _notificationsNotificationService;
    private readonly ApiPerformance.CalibrationApprovalService.CalibrationApprovalServiceClient _performanceCalibrationApprovalService;
    private readonly IRecordRepository _repository;
    private readonly IRosterApprovalEndpointSendSubmitRoster3Port _rosterApprovalEndpointSendSubmitRoster3Port;
    private readonly ApiScheduling.AvailabilityWindowValidationService.AvailabilityWindowValidationServiceClient _schedulingAvailabilityWindowValidationService;
    private readonly ApiScheduling.RosterCalculationService.RosterCalculationServiceClient _schedulingRosterCalculationService;
    private readonly ApiScheduling.RosterQueryService.RosterQueryServiceClient _schedulingRosterQueryService;
    private readonly ApiScheduling.RosterValidationService.RosterValidationServiceClient _schedulingRosterValidationService;
    private readonly ApiScheduling.ShiftValidationService.ShiftValidationServiceClient _schedulingShiftValidationService;
    private readonly ApiScheduling.SwapRequestCalculationService.SwapRequestCalculationServiceClient _schedulingSwapRequestCalculationService;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiSurveys.SurveyService.SurveyServiceClient _surveysSurveyService;

    public RosterApprovalEndpoint(
        ApiBenefits.DependentApprovalService.DependentApprovalServiceClient benefitsDependentApprovalService,
        ApiDocuments.DocumentFolderSyncService.DocumentFolderSyncServiceClient documentsDocumentFolderSyncService,
        ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient employeesEmployeeQueryService,
        ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient employeesEmploymentContractCalculationService,
        ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient employeesOrgUnitQueryService,
        ApiHelpdesk.EscalationReportingService.EscalationReportingServiceClient helpdeskEscalationReportingService,
        ApiIdentity.UserQueryService.UserQueryServiceClient identityUserQueryService,
        CallInvoker invoker,
        ApiLicensing.LicenseAssignmentWorkflowService.LicenseAssignmentWorkflowServiceClient licensingLicenseAssignmentWorkflowService,
        ILogger<RosterApprovalEndpoint> logger,
        ApiMetrics.MetricSeriesValidationService.MetricSeriesValidationServiceClient metricsMetricSeriesValidationService,
        ApiNotifications.NotificationService.NotificationServiceClient notificationsNotificationService,
        ApiPerformance.CalibrationApprovalService.CalibrationApprovalServiceClient performanceCalibrationApprovalService,
        IRecordRepository repository,
        IRosterApprovalEndpointSendSubmitRoster3Port rosterApprovalEndpointSendSubmitRoster3PortAdapter,
        ApiScheduling.AvailabilityWindowValidationService.AvailabilityWindowValidationServiceClient schedulingAvailabilityWindowValidationService,
        ApiScheduling.RosterCalculationService.RosterCalculationServiceClient schedulingRosterCalculationService,
        ApiScheduling.RosterQueryService.RosterQueryServiceClient schedulingRosterQueryService,
        ApiScheduling.RosterValidationService.RosterValidationServiceClient schedulingRosterValidationService,
        ApiScheduling.ShiftValidationService.ShiftValidationServiceClient schedulingShiftValidationService,
        ApiScheduling.SwapRequestCalculationService.SwapRequestCalculationServiceClient schedulingSwapRequestCalculationService,
        IOptions<MeshOptions> settings,
        ApiSurveys.SurveyService.SurveyServiceClient surveysSurveyService)
    {
        _benefitsDependentApprovalService = benefitsDependentApprovalService;
        _documentsDocumentFolderSyncService = documentsDocumentFolderSyncService;
        _employeesEmployeeQueryService = employeesEmployeeQueryService;
        _employeesEmploymentContractCalculationService = employeesEmploymentContractCalculationService;
        _employeesOrgUnitQueryService = employeesOrgUnitQueryService;
        _helpdeskEscalationReportingService = helpdeskEscalationReportingService;
        _identityUserQueryService = identityUserQueryService;
        _invoker = invoker;
        _licensingLicenseAssignmentWorkflowService = licensingLicenseAssignmentWorkflowService;
        _logger = logger;
        _metricsMetricSeriesValidationService = metricsMetricSeriesValidationService;
        _notificationsNotificationService = notificationsNotificationService;
        _performanceCalibrationApprovalService = performanceCalibrationApprovalService;
        _repository = repository;
        _rosterApprovalEndpointSendSubmitRoster3Port = rosterApprovalEndpointSendSubmitRoster3PortAdapter;
        _schedulingAvailabilityWindowValidationService = schedulingAvailabilityWindowValidationService;
        _schedulingRosterCalculationService = schedulingRosterCalculationService;
        _schedulingRosterQueryService = schedulingRosterQueryService;
        _schedulingRosterValidationService = schedulingRosterValidationService;
        _schedulingShiftValidationService = schedulingShiftValidationService;
        _schedulingSwapRequestCalculationService = schedulingSwapRequestCalculationService;
        _settings = settings;
        _surveysSurveyService = surveysSurveyService;
    }
}
