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
using ApiItsm = Acme.Itsm.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiSearch = Acme.Search.V1;

namespace Acme.Workforce.Itsm;

/// <summary>
/// Runs the long-lived workflow that takes an incident from start to finish.
/// </summary>
public sealed partial class IncidentWorkflowEndpoint : ApiItsm.IncidentWorkflowService.IncidentWorkflowServiceBase
{
    private readonly ApiDevices.DevicePolicyWorkflowService.DevicePolicyWorkflowServiceClient _devicesDevicePolicyWorkflowService;
    private readonly ApiDocuments.DocumentExportService.DocumentExportServiceClient _documentsDocumentExportService;
    private readonly ApiDocuments.DocumentFolderHistoryService.DocumentFolderHistoryServiceClient _documentsDocumentFolderHistoryService;
    private readonly ApiDocuments.DocumentValidationService.DocumentValidationServiceClient _documentsDocumentValidationService;
    private readonly ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient _employeesEmployeeQueryService;
    private readonly ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient _employeesOrgUnitQueryService;
    private readonly IIncidentWorkflowEndpointSendPauseIncidentWorkflow0Port _incidentWorkflowEndpointSendPauseIncidentWorkflow0Port;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.ChangeRequestApprovalService.ChangeRequestApprovalServiceClient _itsmChangeRequestApprovalService;
    private readonly ApiItsm.ChangeRequestValidationService.ChangeRequestValidationServiceClient _itsmChangeRequestValidationService;
    private readonly ApiItsm.IncidentLifecycleService.IncidentLifecycleServiceClient _itsmIncidentLifecycleService;
    private readonly ApiItsm.ProblemCalculationService.ProblemCalculationServiceClient _itsmProblemCalculationService;
    private readonly ApiItsm.ProblemSyncService.ProblemSyncServiceClient _itsmProblemSyncService;
    private readonly ApiLedger.LedgerAccountValidationService.LedgerAccountValidationServiceClient _ledgerLedgerAccountValidationService;
    private readonly ApiLicensing.LicenseAssignmentApprovalService.LicenseAssignmentApprovalServiceClient _licensingLicenseAssignmentApprovalService;
    private readonly ApiLicensing.LicenseAssignmentWorkflowService.LicenseAssignmentWorkflowServiceClient _licensingLicenseAssignmentWorkflowService;
    private readonly ILogger<IncidentWorkflowEndpoint> _logger;
    private readonly IRecordRepository _repository;
    private readonly ApiSearch.SearchIndexQueryService.SearchIndexQueryServiceClient _searchSearchIndexQueryService;
    private readonly Func<ApiLedger.PreviewLedgerAccountRequest, CallOptions, AsyncUnaryCall<ApiLedger.PreviewLedgerAccountResponse>> _sendStartIncidentWorkflow0;

    public IncidentWorkflowEndpoint(
        ApiDevices.DevicePolicyWorkflowService.DevicePolicyWorkflowServiceClient devicesDevicePolicyWorkflowService,
        ApiDocuments.DocumentExportService.DocumentExportServiceClient documentsDocumentExportService,
        ApiDocuments.DocumentFolderHistoryService.DocumentFolderHistoryServiceClient documentsDocumentFolderHistoryService,
        ApiDocuments.DocumentValidationService.DocumentValidationServiceClient documentsDocumentValidationService,
        ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient employeesEmployeeQueryService,
        ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient employeesOrgUnitQueryService,
        IIncidentWorkflowEndpointSendPauseIncidentWorkflow0Port incidentWorkflowEndpointSendPauseIncidentWorkflow0PortAdapter,
        CallInvoker invoker,
        ApiItsm.ChangeRequestApprovalService.ChangeRequestApprovalServiceClient itsmChangeRequestApprovalService,
        ApiItsm.ChangeRequestValidationService.ChangeRequestValidationServiceClient itsmChangeRequestValidationService,
        ApiItsm.IncidentLifecycleService.IncidentLifecycleServiceClient itsmIncidentLifecycleService,
        ApiItsm.ProblemCalculationService.ProblemCalculationServiceClient itsmProblemCalculationService,
        ApiItsm.ProblemSyncService.ProblemSyncServiceClient itsmProblemSyncService,
        ApiLedger.LedgerAccountValidationService.LedgerAccountValidationServiceClient ledgerLedgerAccountValidationService,
        ApiLicensing.LicenseAssignmentApprovalService.LicenseAssignmentApprovalServiceClient licensingLicenseAssignmentApprovalService,
        ApiLicensing.LicenseAssignmentWorkflowService.LicenseAssignmentWorkflowServiceClient licensingLicenseAssignmentWorkflowService,
        ILogger<IncidentWorkflowEndpoint> logger,
        IRecordRepository repository,
        ApiSearch.SearchIndexQueryService.SearchIndexQueryServiceClient searchSearchIndexQueryService)
    {
        _devicesDevicePolicyWorkflowService = devicesDevicePolicyWorkflowService;
        _documentsDocumentExportService = documentsDocumentExportService;
        _documentsDocumentFolderHistoryService = documentsDocumentFolderHistoryService;
        _documentsDocumentValidationService = documentsDocumentValidationService;
        _employeesEmployeeQueryService = employeesEmployeeQueryService;
        _employeesOrgUnitQueryService = employeesOrgUnitQueryService;
        _incidentWorkflowEndpointSendPauseIncidentWorkflow0Port = incidentWorkflowEndpointSendPauseIncidentWorkflow0PortAdapter;
        _invoker = invoker;
        _itsmChangeRequestApprovalService = itsmChangeRequestApprovalService;
        _itsmChangeRequestValidationService = itsmChangeRequestValidationService;
        _itsmIncidentLifecycleService = itsmIncidentLifecycleService;
        _itsmProblemCalculationService = itsmProblemCalculationService;
        _itsmProblemSyncService = itsmProblemSyncService;
        _ledgerLedgerAccountValidationService = ledgerLedgerAccountValidationService;
        _licensingLicenseAssignmentApprovalService = licensingLicenseAssignmentApprovalService;
        _licensingLicenseAssignmentWorkflowService = licensingLicenseAssignmentWorkflowService;
        _logger = logger;
        _repository = repository;
        _searchSearchIndexQueryService = searchSearchIndexQueryService;
        _sendStartIncidentWorkflow0 = _ledgerLedgerAccountValidationService.PreviewLedgerAccountAsync;
    }
}
