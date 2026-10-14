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
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiNotifications = Acme.Notifications.V1;

namespace Acme.Workforce.Helpdesk;

/// <summary>
/// Routes escalations through approval, with escalation and delegation.
/// </summary>
public sealed partial class EscalationApprovalEndpoint : ApiHelpdesk.EscalationApprovalService.EscalationApprovalServiceBase
{
    private readonly NotificationBus _bus;
    private readonly ApiEmployees.EmployeeService.EmployeeServiceClient _employeesEmployeeService;
    private readonly ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient _employeesEmploymentContractCalculationService;
    private readonly ApiHelpdesk.EscalationLifecycleService.EscalationLifecycleServiceClient _helpdeskEscalationLifecycleService;
    private readonly ApiHelpdesk.EscalationReportingService.EscalationReportingServiceClient _helpdeskEscalationReportingService;
    private readonly ApiHelpdesk.KnowledgeArticleValidationService.KnowledgeArticleValidationServiceClient _helpdeskKnowledgeArticleValidationService;
    private readonly ApiHelpdesk.KnowledgeArticleWorkflowService.KnowledgeArticleWorkflowServiceClient _helpdeskKnowledgeArticleWorkflowService;
    private readonly ApiHelpdesk.SlaPolicyApprovalService.SlaPolicyApprovalServiceClient _helpdeskSlaPolicyApprovalService;
    private readonly ApiHelpdesk.TicketApprovalService.TicketApprovalServiceClient _helpdeskTicketApprovalService;
    private readonly ApiHelpdesk.TicketQueryService.TicketQueryServiceClient _helpdeskTicketQueryService;
    private readonly ApiHelpdesk.TicketWorkflowService.TicketWorkflowServiceClient _helpdeskTicketWorkflowService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.ServiceAssetReportingService.ServiceAssetReportingServiceClient _itsmServiceAssetReportingService;
    private readonly ApiLicensing.LicenseAssignmentWorkflowService.LicenseAssignmentWorkflowServiceClient _licensingLicenseAssignmentWorkflowService;
    private readonly ILogger<EscalationApprovalEndpoint> _logger;
    private readonly ApiNotifications.NotificationService.NotificationServiceClient _notificationsNotificationService;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiHelpdesk.CancelTicketWorkflowRequest, CallOptions, AsyncUnaryCall<ApiHelpdesk.CancelTicketWorkflowResponse>> _sendListPendingEscalations0;

    public EscalationApprovalEndpoint(
        NotificationBus bus,
        ApiEmployees.EmployeeService.EmployeeServiceClient employeesEmployeeService,
        ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient employeesEmploymentContractCalculationService,
        ApiHelpdesk.EscalationLifecycleService.EscalationLifecycleServiceClient helpdeskEscalationLifecycleService,
        ApiHelpdesk.EscalationReportingService.EscalationReportingServiceClient helpdeskEscalationReportingService,
        ApiHelpdesk.KnowledgeArticleValidationService.KnowledgeArticleValidationServiceClient helpdeskKnowledgeArticleValidationService,
        ApiHelpdesk.KnowledgeArticleWorkflowService.KnowledgeArticleWorkflowServiceClient helpdeskKnowledgeArticleWorkflowService,
        ApiHelpdesk.SlaPolicyApprovalService.SlaPolicyApprovalServiceClient helpdeskSlaPolicyApprovalService,
        ApiHelpdesk.TicketApprovalService.TicketApprovalServiceClient helpdeskTicketApprovalService,
        ApiHelpdesk.TicketQueryService.TicketQueryServiceClient helpdeskTicketQueryService,
        ApiHelpdesk.TicketWorkflowService.TicketWorkflowServiceClient helpdeskTicketWorkflowService,
        CallInvoker invoker,
        ApiItsm.ServiceAssetReportingService.ServiceAssetReportingServiceClient itsmServiceAssetReportingService,
        ApiLicensing.LicenseAssignmentWorkflowService.LicenseAssignmentWorkflowServiceClient licensingLicenseAssignmentWorkflowService,
        ILogger<EscalationApprovalEndpoint> logger,
        ApiNotifications.NotificationService.NotificationServiceClient notificationsNotificationService,
        IRecordRepository repository)
    {
        _bus = bus;
        _employeesEmployeeService = employeesEmployeeService;
        _employeesEmploymentContractCalculationService = employeesEmploymentContractCalculationService;
        _helpdeskEscalationLifecycleService = helpdeskEscalationLifecycleService;
        _helpdeskEscalationReportingService = helpdeskEscalationReportingService;
        _helpdeskKnowledgeArticleValidationService = helpdeskKnowledgeArticleValidationService;
        _helpdeskKnowledgeArticleWorkflowService = helpdeskKnowledgeArticleWorkflowService;
        _helpdeskSlaPolicyApprovalService = helpdeskSlaPolicyApprovalService;
        _helpdeskTicketApprovalService = helpdeskTicketApprovalService;
        _helpdeskTicketQueryService = helpdeskTicketQueryService;
        _helpdeskTicketWorkflowService = helpdeskTicketWorkflowService;
        _invoker = invoker;
        _itsmServiceAssetReportingService = itsmServiceAssetReportingService;
        _licensingLicenseAssignmentWorkflowService = licensingLicenseAssignmentWorkflowService;
        _logger = logger;
        _notificationsNotificationService = notificationsNotificationService;
        _repository = repository;
        _sendListPendingEscalations0 = _helpdeskTicketWorkflowService.CancelTicketWorkflowAsync;
    }
}
