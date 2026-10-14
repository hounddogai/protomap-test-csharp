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
using ApiAudit = Acme.Audit.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Timekeeping;

/// <summary>
/// Runs the long-lived workflow that takes a clock event from start to finish.
/// </summary>
public sealed partial class ClockEventWorkflowEndpoint : ApiTimekeeping.ClockEventWorkflowService.ClockEventWorkflowServiceBase
{
    private readonly ApiAudit.AuditEventValidationService.AuditEventValidationServiceClient _auditAuditEventValidationService;
    private readonly IClockEventWorkflowEndpointSendSignalClockEventWorkflow0Port _clockEventWorkflowEndpointSendSignalClockEventWorkflow0Port;
    private readonly ApiDocuments.DocumentFolderExportService.DocumentFolderExportServiceClient _documentsDocumentFolderExportService;
    private readonly ApiDocuments.SignatureNotificationService.SignatureNotificationServiceClient _documentsSignatureNotificationService;
    private readonly ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient _employeesEmployeeQueryService;
    private readonly ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient _employeesEmploymentContractCalculationService;
    private readonly ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient _employeesOrgUnitQueryService;
    private readonly ApiEmployees.PositionValidationService.PositionValidationServiceClient _employeesPositionValidationService;
    private readonly ILogger<ClockEventWorkflowEndpoint> _logger;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiEmployees.EstimateEmploymentContractRequest, CallOptions, AsyncUnaryCall<ApiEmployees.EstimateEmploymentContractResponse>> _sendInspectClockEventWorkflow0;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTimekeeping.ClockEventApprovalService.ClockEventApprovalServiceClient _timekeepingClockEventApprovalService;
    private readonly ApiTimekeeping.ClockEventCalculationService.ClockEventCalculationServiceClient _timekeepingClockEventCalculationService;
    private readonly ApiTimekeeping.OvertimeValidationService.OvertimeValidationServiceClient _timekeepingOvertimeValidationService;
    private readonly ApiTimekeeping.TimesheetApprovalService.TimesheetApprovalServiceClient _timekeepingTimesheetApprovalService;
    private readonly ApiTimekeeping.TimesheetService.TimesheetServiceClient _timekeepingTimesheetService;
    private readonly ApiTimekeeping.TimesheetWorkflowService.TimesheetWorkflowServiceClient _timekeepingTimesheetWorkflowService;

    public ClockEventWorkflowEndpoint(
        ApiAudit.AuditEventValidationService.AuditEventValidationServiceClient auditAuditEventValidationService,
        IClockEventWorkflowEndpointSendSignalClockEventWorkflow0Port clockEventWorkflowEndpointSendSignalClockEventWorkflow0PortAdapter,
        ApiDocuments.DocumentFolderExportService.DocumentFolderExportServiceClient documentsDocumentFolderExportService,
        ApiDocuments.SignatureNotificationService.SignatureNotificationServiceClient documentsSignatureNotificationService,
        ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient employeesEmployeeQueryService,
        ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient employeesEmploymentContractCalculationService,
        ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient employeesOrgUnitQueryService,
        ApiEmployees.PositionValidationService.PositionValidationServiceClient employeesPositionValidationService,
        ILogger<ClockEventWorkflowEndpoint> logger,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiTimekeeping.ClockEventApprovalService.ClockEventApprovalServiceClient timekeepingClockEventApprovalService,
        ApiTimekeeping.ClockEventCalculationService.ClockEventCalculationServiceClient timekeepingClockEventCalculationService,
        ApiTimekeeping.OvertimeValidationService.OvertimeValidationServiceClient timekeepingOvertimeValidationService,
        ApiTimekeeping.TimesheetApprovalService.TimesheetApprovalServiceClient timekeepingTimesheetApprovalService,
        ApiTimekeeping.TimesheetService.TimesheetServiceClient timekeepingTimesheetService,
        ApiTimekeeping.TimesheetWorkflowService.TimesheetWorkflowServiceClient timekeepingTimesheetWorkflowService)
    {
        _auditAuditEventValidationService = auditAuditEventValidationService;
        _clockEventWorkflowEndpointSendSignalClockEventWorkflow0Port = clockEventWorkflowEndpointSendSignalClockEventWorkflow0PortAdapter;
        _documentsDocumentFolderExportService = documentsDocumentFolderExportService;
        _documentsSignatureNotificationService = documentsSignatureNotificationService;
        _employeesEmployeeQueryService = employeesEmployeeQueryService;
        _employeesEmploymentContractCalculationService = employeesEmploymentContractCalculationService;
        _employeesOrgUnitQueryService = employeesOrgUnitQueryService;
        _employeesPositionValidationService = employeesPositionValidationService;
        _logger = logger;
        _repository = repository;
        _settings = settings;
        _timekeepingClockEventApprovalService = timekeepingClockEventApprovalService;
        _timekeepingClockEventCalculationService = timekeepingClockEventCalculationService;
        _timekeepingOvertimeValidationService = timekeepingOvertimeValidationService;
        _timekeepingTimesheetApprovalService = timekeepingTimesheetApprovalService;
        _timekeepingTimesheetService = timekeepingTimesheetService;
        _timekeepingTimesheetWorkflowService = timekeepingTimesheetWorkflowService;
        _sendInspectClockEventWorkflow0 = _employeesEmploymentContractCalculationService.EstimateEmploymentContractAsync;
    }
}
