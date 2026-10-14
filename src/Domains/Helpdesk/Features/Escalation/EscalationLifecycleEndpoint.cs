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
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiTreasury = Acme.Treasury.V1;

namespace Acme.Workforce.Helpdesk;

/// <summary>
/// Moves an escalation between the states of its lifecycle.
/// </summary>
public sealed partial class EscalationLifecycleEndpoint : ApiHelpdesk.EscalationLifecycleService.EscalationLifecycleServiceBase
{
    private readonly ApiAudit.AuditEventValidationService.AuditEventValidationServiceClient _auditAuditEventValidationService;
    private readonly NotificationBus _bus;
    private readonly ApiEmployees.EmployeeService.EmployeeServiceClient _employeesEmployeeService;
    private readonly ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient _employeesEmploymentContractCalculationService;
    private readonly ApiEmployees.PositionValidationService.PositionValidationServiceClient _employeesPositionValidationService;
    private readonly IEscalationLifecycleEndpointSendTerminateEscalation0Port _escalationLifecycleEndpointSendTerminateEscalation0Port;
    private readonly ApiHelpdesk.EscalationReportingService.EscalationReportingServiceClient _helpdeskEscalationReportingService;
    private readonly ApiHelpdesk.SlaPolicyCalculationService.SlaPolicyCalculationServiceClient _helpdeskSlaPolicyCalculationService;
    private readonly ApiItsm.ProblemSyncService.ProblemSyncServiceClient _itsmProblemSyncService;
    private readonly ApiLicensing.LicenseAssignmentWorkflowService.LicenseAssignmentWorkflowServiceClient _licensingLicenseAssignmentWorkflowService;
    private readonly ApiLicensing.SoftwareLicenseApprovalService.SoftwareLicenseApprovalServiceClient _licensingSoftwareLicenseApprovalService;
    private readonly ApiLicensing.SoftwareLicenseWorkflowService.SoftwareLicenseWorkflowServiceClient _licensingSoftwareLicenseWorkflowService;
    private readonly ILogger<EscalationLifecycleEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTreasury.CashPositionService.CashPositionServiceClient _treasuryCashPositionService;

    public EscalationLifecycleEndpoint(
        ApiAudit.AuditEventValidationService.AuditEventValidationServiceClient auditAuditEventValidationService,
        NotificationBus bus,
        ApiEmployees.EmployeeService.EmployeeServiceClient employeesEmployeeService,
        ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient employeesEmploymentContractCalculationService,
        ApiEmployees.PositionValidationService.PositionValidationServiceClient employeesPositionValidationService,
        IEscalationLifecycleEndpointSendTerminateEscalation0Port escalationLifecycleEndpointSendTerminateEscalation0PortAdapter,
        ApiHelpdesk.EscalationReportingService.EscalationReportingServiceClient helpdeskEscalationReportingService,
        ApiHelpdesk.SlaPolicyCalculationService.SlaPolicyCalculationServiceClient helpdeskSlaPolicyCalculationService,
        ApiItsm.ProblemSyncService.ProblemSyncServiceClient itsmProblemSyncService,
        ApiLicensing.LicenseAssignmentWorkflowService.LicenseAssignmentWorkflowServiceClient licensingLicenseAssignmentWorkflowService,
        ApiLicensing.SoftwareLicenseApprovalService.SoftwareLicenseApprovalServiceClient licensingSoftwareLicenseApprovalService,
        ApiLicensing.SoftwareLicenseWorkflowService.SoftwareLicenseWorkflowServiceClient licensingSoftwareLicenseWorkflowService,
        ILogger<EscalationLifecycleEndpoint> logger,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiTreasury.CashPositionService.CashPositionServiceClient treasuryCashPositionService)
    {
        _auditAuditEventValidationService = auditAuditEventValidationService;
        _bus = bus;
        _employeesEmployeeService = employeesEmployeeService;
        _employeesEmploymentContractCalculationService = employeesEmploymentContractCalculationService;
        _employeesPositionValidationService = employeesPositionValidationService;
        _escalationLifecycleEndpointSendTerminateEscalation0Port = escalationLifecycleEndpointSendTerminateEscalation0PortAdapter;
        _helpdeskEscalationReportingService = helpdeskEscalationReportingService;
        _helpdeskSlaPolicyCalculationService = helpdeskSlaPolicyCalculationService;
        _itsmProblemSyncService = itsmProblemSyncService;
        _licensingLicenseAssignmentWorkflowService = licensingLicenseAssignmentWorkflowService;
        _licensingSoftwareLicenseApprovalService = licensingSoftwareLicenseApprovalService;
        _licensingSoftwareLicenseWorkflowService = licensingSoftwareLicenseWorkflowService;
        _logger = logger;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _settings = settings;
        _treasuryCashPositionService = treasuryCashPositionService;
    }
}
