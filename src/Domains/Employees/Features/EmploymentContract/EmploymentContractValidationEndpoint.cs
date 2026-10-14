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
using ApiAuthorization = Acme.Authorization.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Employees;

/// <summary>
/// Checks employment contracts against the rules before anything acts on them.
/// </summary>
public sealed partial class EmploymentContractValidationEndpoint : ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceBase
{
    private readonly ApiAudit.AuditEventQueryService.AuditEventQueryServiceClient _auditAuditEventQueryService;
    private readonly ApiAuthorization.AccessPolicyService.AccessPolicyServiceClient _authorizationAccessPolicyService;
    private readonly ApiCorporateevents.CorporateEventCalculationService.CorporateEventCalculationServiceClient _corporateeventsCorporateEventCalculationService;
    private readonly ApiDocuments.DocumentFolderNotificationService.DocumentFolderNotificationServiceClient _documentsDocumentFolderNotificationService;
    private readonly ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient _employeesEmploymentContractCalculationService;
    private readonly ApiEmployees.PositionValidationService.PositionValidationServiceClient _employeesPositionValidationService;
    private readonly IEmploymentContractValidationEndpointSendNormalizeEmploymentContract3Port _employmentContractValidationEndpointSendNormalizeEmploymentContract3Port;
    private readonly CallInvoker _invoker;
    private readonly ApiLedger.LedgerAccountQueryService.LedgerAccountQueryServiceClient _ledgerLedgerAccountQueryService;
    private readonly ILogger<EmploymentContractValidationEndpoint> _logger;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiScheduling.AvailabilityWindowCalculationService.AvailabilityWindowCalculationServiceClient _schedulingAvailabilityWindowCalculationService;
    private readonly Func<ApiEmployees.CheckPositionEligibilityRequest, CallOptions, AsyncUnaryCall<ApiEmployees.CheckPositionEligibilityResponse>> _sendLintEmploymentContracts0;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTimekeeping.OvertimeValidationService.OvertimeValidationServiceClient _timekeepingOvertimeValidationService;
    private readonly ApiTimekeeping.TimesheetQueryService.TimesheetQueryServiceClient _timekeepingTimesheetQueryService;
    private readonly ApiWorkplaces.BuildingValidationService.BuildingValidationServiceClient _workplacesBuildingValidationService;

    public EmploymentContractValidationEndpoint(
        ApiAudit.AuditEventQueryService.AuditEventQueryServiceClient auditAuditEventQueryService,
        ApiAuthorization.AccessPolicyService.AccessPolicyServiceClient authorizationAccessPolicyService,
        ApiCorporateevents.CorporateEventCalculationService.CorporateEventCalculationServiceClient corporateeventsCorporateEventCalculationService,
        ApiDocuments.DocumentFolderNotificationService.DocumentFolderNotificationServiceClient documentsDocumentFolderNotificationService,
        ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient employeesEmploymentContractCalculationService,
        ApiEmployees.PositionValidationService.PositionValidationServiceClient employeesPositionValidationService,
        IEmploymentContractValidationEndpointSendNormalizeEmploymentContract3Port employmentContractValidationEndpointSendNormalizeEmploymentContract3PortAdapter,
        CallInvoker invoker,
        ApiLedger.LedgerAccountQueryService.LedgerAccountQueryServiceClient ledgerLedgerAccountQueryService,
        ILogger<EmploymentContractValidationEndpoint> logger,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiScheduling.AvailabilityWindowCalculationService.AvailabilityWindowCalculationServiceClient schedulingAvailabilityWindowCalculationService,
        IOptions<MeshOptions> settings,
        ApiTimekeeping.OvertimeValidationService.OvertimeValidationServiceClient timekeepingOvertimeValidationService,
        ApiTimekeeping.TimesheetQueryService.TimesheetQueryServiceClient timekeepingTimesheetQueryService,
        ApiWorkplaces.BuildingValidationService.BuildingValidationServiceClient workplacesBuildingValidationService)
    {
        _auditAuditEventQueryService = auditAuditEventQueryService;
        _authorizationAccessPolicyService = authorizationAccessPolicyService;
        _corporateeventsCorporateEventCalculationService = corporateeventsCorporateEventCalculationService;
        _documentsDocumentFolderNotificationService = documentsDocumentFolderNotificationService;
        _employeesEmploymentContractCalculationService = employeesEmploymentContractCalculationService;
        _employeesPositionValidationService = employeesPositionValidationService;
        _employmentContractValidationEndpointSendNormalizeEmploymentContract3Port = employmentContractValidationEndpointSendNormalizeEmploymentContract3PortAdapter;
        _invoker = invoker;
        _ledgerLedgerAccountQueryService = ledgerLedgerAccountQueryService;
        _logger = logger;
        _registry = registry;
        _repository = repository;
        _schedulingAvailabilityWindowCalculationService = schedulingAvailabilityWindowCalculationService;
        _settings = settings;
        _timekeepingOvertimeValidationService = timekeepingOvertimeValidationService;
        _timekeepingTimesheetQueryService = timekeepingTimesheetQueryService;
        _workplacesBuildingValidationService = workplacesBuildingValidationService;
        _sendLintEmploymentContracts0 = _employeesPositionValidationService.CheckPositionEligibilityAsync;
    }
}
