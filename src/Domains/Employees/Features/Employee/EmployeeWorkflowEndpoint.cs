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
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiKeymanagement = Acme.Keymanagement.V1;
using ApiLabeling = Acme.Labeling.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiProcurement = Acme.Procurement.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiWebhooks = Acme.Webhooks.V1;

namespace Acme.Workforce.Employees;

/// <summary>
/// Runs the long-lived workflow that takes an employee from start to finish.
/// </summary>
public sealed partial class EmployeeWorkflowEndpoint : ApiEmployees.EmployeeWorkflowService.EmployeeWorkflowServiceBase
{
    private readonly NotificationBus _bus;
    private readonly ApiDevices.ComplianceScanWorkflowService.ComplianceScanWorkflowServiceClient _devicesComplianceScanWorkflowService;
    private readonly ApiDevices.DevicePolicyLifecycleService.DevicePolicyLifecycleServiceClient _devicesDevicePolicyLifecycleService;
    private readonly ApiDevices.ManagedDeviceApprovalService.ManagedDeviceApprovalServiceClient _devicesManagedDeviceApprovalService;
    private readonly ApiDocuments.DocumentExportService.DocumentExportServiceClient _documentsDocumentExportService;
    private readonly ApiDocuments.DocumentFolderNotificationService.DocumentFolderNotificationServiceClient _documentsDocumentFolderNotificationService;
    private readonly ApiDocuments.DocumentSyncService.DocumentSyncServiceClient _documentsDocumentSyncService;
    private readonly ApiDocuments.DocumentTemplateWorkflowService.DocumentTemplateWorkflowServiceClient _documentsDocumentTemplateWorkflowService;
    private readonly ApiDocuments.SignatureSyncService.SignatureSyncServiceClient _documentsSignatureSyncService;
    private readonly IEmployeeWorkflowEndpointSendAdvanceEmployeeWorkflow0Port _employeeWorkflowEndpointSendAdvanceEmployeeWorkflow0Port;
    private readonly ApiEmployees.EmployeeApprovalService.EmployeeApprovalServiceClient _employeesEmployeeApprovalService;
    private readonly ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient _employeesEmployeeQueryService;
    private readonly ApiEmployees.OrgUnitApprovalService.OrgUnitApprovalServiceClient _employeesOrgUnitApprovalService;
    private readonly ApiEmployees.PositionValidationService.PositionValidationServiceClient _employeesPositionValidationService;
    private readonly ApiIdentity.UserValidationService.UserValidationServiceClient _identityUserValidationService;
    private readonly CallInvoker _invoker;
    private readonly ApiKeymanagement.CryptoKeyCalculationService.CryptoKeyCalculationServiceClient _keymanagementCryptoKeyCalculationService;
    private readonly ApiLabeling.LabelingTaskCalculationService.LabelingTaskCalculationServiceClient _labelingLabelingTaskCalculationService;
    private readonly ILogger<EmployeeWorkflowEndpoint> _logger;
    private readonly ApiMetrics.MetricSeriesService.MetricSeriesServiceClient _metricsMetricSeriesService;
    private readonly RetryPolicy _policy;
    private readonly ApiProcurement.PurchaseRequisitionCalculationService.PurchaseRequisitionCalculationServiceClient _procurementPurchaseRequisitionCalculationService;
    private readonly IRecordRepository _repository;
    private readonly ApiScheduling.RosterApprovalService.RosterApprovalServiceClient _schedulingRosterApprovalService;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiWebhooks.WebhookEndpointQueryService.WebhookEndpointQueryServiceClient _webhooksWebhookEndpointQueryService;

    public EmployeeWorkflowEndpoint(
        NotificationBus bus,
        ApiDevices.ComplianceScanWorkflowService.ComplianceScanWorkflowServiceClient devicesComplianceScanWorkflowService,
        ApiDevices.DevicePolicyLifecycleService.DevicePolicyLifecycleServiceClient devicesDevicePolicyLifecycleService,
        ApiDevices.ManagedDeviceApprovalService.ManagedDeviceApprovalServiceClient devicesManagedDeviceApprovalService,
        ApiDocuments.DocumentExportService.DocumentExportServiceClient documentsDocumentExportService,
        ApiDocuments.DocumentFolderNotificationService.DocumentFolderNotificationServiceClient documentsDocumentFolderNotificationService,
        ApiDocuments.DocumentSyncService.DocumentSyncServiceClient documentsDocumentSyncService,
        ApiDocuments.DocumentTemplateWorkflowService.DocumentTemplateWorkflowServiceClient documentsDocumentTemplateWorkflowService,
        ApiDocuments.SignatureSyncService.SignatureSyncServiceClient documentsSignatureSyncService,
        IEmployeeWorkflowEndpointSendAdvanceEmployeeWorkflow0Port employeeWorkflowEndpointSendAdvanceEmployeeWorkflow0PortAdapter,
        ApiEmployees.EmployeeApprovalService.EmployeeApprovalServiceClient employeesEmployeeApprovalService,
        ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient employeesEmployeeQueryService,
        ApiEmployees.OrgUnitApprovalService.OrgUnitApprovalServiceClient employeesOrgUnitApprovalService,
        ApiEmployees.PositionValidationService.PositionValidationServiceClient employeesPositionValidationService,
        ApiIdentity.UserValidationService.UserValidationServiceClient identityUserValidationService,
        CallInvoker invoker,
        ApiKeymanagement.CryptoKeyCalculationService.CryptoKeyCalculationServiceClient keymanagementCryptoKeyCalculationService,
        ApiLabeling.LabelingTaskCalculationService.LabelingTaskCalculationServiceClient labelingLabelingTaskCalculationService,
        ILogger<EmployeeWorkflowEndpoint> logger,
        ApiMetrics.MetricSeriesService.MetricSeriesServiceClient metricsMetricSeriesService,
        RetryPolicy policy,
        ApiProcurement.PurchaseRequisitionCalculationService.PurchaseRequisitionCalculationServiceClient procurementPurchaseRequisitionCalculationService,
        IRecordRepository repository,
        ApiScheduling.RosterApprovalService.RosterApprovalServiceClient schedulingRosterApprovalService,
        IOptions<MeshOptions> settings,
        ApiWebhooks.WebhookEndpointQueryService.WebhookEndpointQueryServiceClient webhooksWebhookEndpointQueryService)
    {
        _bus = bus;
        _devicesComplianceScanWorkflowService = devicesComplianceScanWorkflowService;
        _devicesDevicePolicyLifecycleService = devicesDevicePolicyLifecycleService;
        _devicesManagedDeviceApprovalService = devicesManagedDeviceApprovalService;
        _documentsDocumentExportService = documentsDocumentExportService;
        _documentsDocumentFolderNotificationService = documentsDocumentFolderNotificationService;
        _documentsDocumentSyncService = documentsDocumentSyncService;
        _documentsDocumentTemplateWorkflowService = documentsDocumentTemplateWorkflowService;
        _documentsSignatureSyncService = documentsSignatureSyncService;
        _employeeWorkflowEndpointSendAdvanceEmployeeWorkflow0Port = employeeWorkflowEndpointSendAdvanceEmployeeWorkflow0PortAdapter;
        _employeesEmployeeApprovalService = employeesEmployeeApprovalService;
        _employeesEmployeeQueryService = employeesEmployeeQueryService;
        _employeesOrgUnitApprovalService = employeesOrgUnitApprovalService;
        _employeesPositionValidationService = employeesPositionValidationService;
        _identityUserValidationService = identityUserValidationService;
        _invoker = invoker;
        _keymanagementCryptoKeyCalculationService = keymanagementCryptoKeyCalculationService;
        _labelingLabelingTaskCalculationService = labelingLabelingTaskCalculationService;
        _logger = logger;
        _metricsMetricSeriesService = metricsMetricSeriesService;
        _policy = policy;
        _procurementPurchaseRequisitionCalculationService = procurementPurchaseRequisitionCalculationService;
        _repository = repository;
        _schedulingRosterApprovalService = schedulingRosterApprovalService;
        _settings = settings;
        _webhooksWebhookEndpointQueryService = webhooksWebhookEndpointQueryService;
    }
}
