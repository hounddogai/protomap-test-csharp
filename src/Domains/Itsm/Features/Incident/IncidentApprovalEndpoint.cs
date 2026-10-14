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
using ApiBudgeting = Acme.Budgeting.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiNotifications = Acme.Notifications.V1;

namespace Acme.Workforce.Itsm;

/// <summary>
/// Routes incidents through approval, with escalation and delegation.
/// </summary>
public sealed partial class IncidentApprovalEndpoint : ApiItsm.IncidentApprovalService.IncidentApprovalServiceBase
{
    private readonly ApiBudgeting.BudgetCalculationService.BudgetCalculationServiceClient _budgetingBudgetCalculationService;
    private readonly ApiCorporateevents.CorporateEventCalculationService.CorporateEventCalculationServiceClient _corporateeventsCorporateEventCalculationService;
    private readonly ApiDocuments.DocumentFolderNotificationService.DocumentFolderNotificationServiceClient _documentsDocumentFolderNotificationService;
    private readonly ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient _employeesEmployeeQueryService;
    private readonly ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient _employeesOrgUnitQueryService;
    private readonly ApiEmployees.PositionValidationService.PositionValidationServiceClient _employeesPositionValidationService;
    private readonly IIncidentApprovalEndpointSendRecallIncident0Port _incidentApprovalEndpointSendRecallIncident0Port;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.IncidentService.IncidentServiceClient _itsmIncidentService;
    private readonly ApiItsm.ProblemReportingService.ProblemReportingServiceClient _itsmProblemReportingService;
    private readonly ApiItsm.ProblemSyncService.ProblemSyncServiceClient _itsmProblemSyncService;
    private readonly ApiItsm.ServiceAssetCalculationService.ServiceAssetCalculationServiceClient _itsmServiceAssetCalculationService;
    private readonly ApiItsm.ServiceAssetLifecycleService.ServiceAssetLifecycleServiceClient _itsmServiceAssetLifecycleService;
    private readonly ApiItsm.ServiceAssetReportingService.ServiceAssetReportingServiceClient _itsmServiceAssetReportingService;
    private readonly ILogger<IncidentApprovalEndpoint> _logger;
    private readonly ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient _metricsMetricSeriesQueryService;
    private readonly ApiNotifications.NotificationValidationService.NotificationValidationServiceClient _notificationsNotificationValidationService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;

    public IncidentApprovalEndpoint(
        ApiBudgeting.BudgetCalculationService.BudgetCalculationServiceClient budgetingBudgetCalculationService,
        ApiCorporateevents.CorporateEventCalculationService.CorporateEventCalculationServiceClient corporateeventsCorporateEventCalculationService,
        ApiDocuments.DocumentFolderNotificationService.DocumentFolderNotificationServiceClient documentsDocumentFolderNotificationService,
        ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient employeesEmployeeQueryService,
        ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient employeesOrgUnitQueryService,
        ApiEmployees.PositionValidationService.PositionValidationServiceClient employeesPositionValidationService,
        IIncidentApprovalEndpointSendRecallIncident0Port incidentApprovalEndpointSendRecallIncident0PortAdapter,
        CallInvoker invoker,
        ApiItsm.IncidentService.IncidentServiceClient itsmIncidentService,
        ApiItsm.ProblemReportingService.ProblemReportingServiceClient itsmProblemReportingService,
        ApiItsm.ProblemSyncService.ProblemSyncServiceClient itsmProblemSyncService,
        ApiItsm.ServiceAssetCalculationService.ServiceAssetCalculationServiceClient itsmServiceAssetCalculationService,
        ApiItsm.ServiceAssetLifecycleService.ServiceAssetLifecycleServiceClient itsmServiceAssetLifecycleService,
        ApiItsm.ServiceAssetReportingService.ServiceAssetReportingServiceClient itsmServiceAssetReportingService,
        ILogger<IncidentApprovalEndpoint> logger,
        ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient metricsMetricSeriesQueryService,
        ApiNotifications.NotificationValidationService.NotificationValidationServiceClient notificationsNotificationValidationService,
        ClientRegistry registry,
        IRecordRepository repository)
    {
        _budgetingBudgetCalculationService = budgetingBudgetCalculationService;
        _corporateeventsCorporateEventCalculationService = corporateeventsCorporateEventCalculationService;
        _documentsDocumentFolderNotificationService = documentsDocumentFolderNotificationService;
        _employeesEmployeeQueryService = employeesEmployeeQueryService;
        _employeesOrgUnitQueryService = employeesOrgUnitQueryService;
        _employeesPositionValidationService = employeesPositionValidationService;
        _incidentApprovalEndpointSendRecallIncident0Port = incidentApprovalEndpointSendRecallIncident0PortAdapter;
        _invoker = invoker;
        _itsmIncidentService = itsmIncidentService;
        _itsmProblemReportingService = itsmProblemReportingService;
        _itsmProblemSyncService = itsmProblemSyncService;
        _itsmServiceAssetCalculationService = itsmServiceAssetCalculationService;
        _itsmServiceAssetLifecycleService = itsmServiceAssetLifecycleService;
        _itsmServiceAssetReportingService = itsmServiceAssetReportingService;
        _logger = logger;
        _metricsMetricSeriesQueryService = metricsMetricSeriesQueryService;
        _notificationsNotificationValidationService = notificationsNotificationValidationService;
        _registry = registry;
        _repository = repository;
    }
}
