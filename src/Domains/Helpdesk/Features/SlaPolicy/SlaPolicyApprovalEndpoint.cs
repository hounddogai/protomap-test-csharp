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
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiDrivers = Acme.Drivers.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiMaintenance = Acme.Maintenance.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Helpdesk;

/// <summary>
/// Routes sla policies through approval, with escalation and delegation.
/// </summary>
public sealed partial class SlaPolicyApprovalEndpoint : ApiHelpdesk.SlaPolicyApprovalService.SlaPolicyApprovalServiceBase
{
    private readonly ApiAuthorization.AccessPolicyValidationService.AccessPolicyValidationServiceClient _authorizationAccessPolicyValidationService;
    private readonly NotificationBus _bus;
    private readonly ApiDevices.ComplianceScanApprovalService.ComplianceScanApprovalServiceClient _devicesComplianceScanApprovalService;
    private readonly ApiDevices.ManagedDeviceWorkflowService.ManagedDeviceWorkflowServiceClient _devicesManagedDeviceWorkflowService;
    private readonly ApiDocuments.DocumentValidationService.DocumentValidationServiceClient _documentsDocumentValidationService;
    private readonly ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient _employeesEmploymentContractCalculationService;
    private readonly ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient _employeesEmploymentContractValidationService;
    private readonly ApiHelpdesk.EscalationReportingService.EscalationReportingServiceClient _helpdeskEscalationReportingService;
    private readonly ApiHelpdesk.EscalationValidationService.EscalationValidationServiceClient _helpdeskEscalationValidationService;
    private readonly ApiHelpdesk.KnowledgeArticleLifecycleService.KnowledgeArticleLifecycleServiceClient _helpdeskKnowledgeArticleLifecycleService;
    private readonly ApiHelpdesk.SlaPolicyValidationService.SlaPolicyValidationServiceClient _helpdeskSlaPolicyValidationService;
    private readonly ApiHelpdesk.TicketApprovalService.TicketApprovalServiceClient _helpdeskTicketApprovalService;
    private readonly ApiHelpdesk.TicketWorkflowService.TicketWorkflowServiceClient _helpdeskTicketWorkflowService;
    private readonly ApiItsm.IncidentWorkflowService.IncidentWorkflowServiceClient _itsmIncidentWorkflowService;
    private readonly ILogger<SlaPolicyApprovalEndpoint> _logger;
    private readonly ApiMaintenance.WorkOrderCalculationService.WorkOrderCalculationServiceClient _maintenanceWorkOrderCalculationService;
    private readonly RetryPolicy _policy;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiHelpdesk.RejectTicketRequest, CallOptions, AsyncUnaryCall<ApiHelpdesk.RejectTicketResponse>> _sendCommentSlaPolicyApproval0;

    public SlaPolicyApprovalEndpoint(
        ApiAuthorization.AccessPolicyValidationService.AccessPolicyValidationServiceClient authorizationAccessPolicyValidationService,
        NotificationBus bus,
        ApiDevices.ComplianceScanApprovalService.ComplianceScanApprovalServiceClient devicesComplianceScanApprovalService,
        ApiDevices.ManagedDeviceWorkflowService.ManagedDeviceWorkflowServiceClient devicesManagedDeviceWorkflowService,
        ApiDocuments.DocumentValidationService.DocumentValidationServiceClient documentsDocumentValidationService,
        ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient employeesEmploymentContractCalculationService,
        ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient employeesEmploymentContractValidationService,
        ApiHelpdesk.EscalationReportingService.EscalationReportingServiceClient helpdeskEscalationReportingService,
        ApiHelpdesk.EscalationValidationService.EscalationValidationServiceClient helpdeskEscalationValidationService,
        ApiHelpdesk.KnowledgeArticleLifecycleService.KnowledgeArticleLifecycleServiceClient helpdeskKnowledgeArticleLifecycleService,
        ApiHelpdesk.SlaPolicyValidationService.SlaPolicyValidationServiceClient helpdeskSlaPolicyValidationService,
        ApiHelpdesk.TicketApprovalService.TicketApprovalServiceClient helpdeskTicketApprovalService,
        ApiHelpdesk.TicketWorkflowService.TicketWorkflowServiceClient helpdeskTicketWorkflowService,
        ApiItsm.IncidentWorkflowService.IncidentWorkflowServiceClient itsmIncidentWorkflowService,
        ILogger<SlaPolicyApprovalEndpoint> logger,
        ApiMaintenance.WorkOrderCalculationService.WorkOrderCalculationServiceClient maintenanceWorkOrderCalculationService,
        RetryPolicy policy,
        IRecordRepository repository)
    {
        _authorizationAccessPolicyValidationService = authorizationAccessPolicyValidationService;
        _bus = bus;
        _devicesComplianceScanApprovalService = devicesComplianceScanApprovalService;
        _devicesManagedDeviceWorkflowService = devicesManagedDeviceWorkflowService;
        _documentsDocumentValidationService = documentsDocumentValidationService;
        _employeesEmploymentContractCalculationService = employeesEmploymentContractCalculationService;
        _employeesEmploymentContractValidationService = employeesEmploymentContractValidationService;
        _helpdeskEscalationReportingService = helpdeskEscalationReportingService;
        _helpdeskEscalationValidationService = helpdeskEscalationValidationService;
        _helpdeskKnowledgeArticleLifecycleService = helpdeskKnowledgeArticleLifecycleService;
        _helpdeskSlaPolicyValidationService = helpdeskSlaPolicyValidationService;
        _helpdeskTicketApprovalService = helpdeskTicketApprovalService;
        _helpdeskTicketWorkflowService = helpdeskTicketWorkflowService;
        _itsmIncidentWorkflowService = itsmIncidentWorkflowService;
        _logger = logger;
        _maintenanceWorkOrderCalculationService = maintenanceWorkOrderCalculationService;
        _policy = policy;
        _repository = repository;
        _sendCommentSlaPolicyApproval0 = _helpdeskTicketApprovalService.RejectTicketAsync;
    }
}
