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
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiNotifications = Acme.Notifications.V1;

namespace Acme.Workforce.Helpdesk;

/// <summary>
/// Runs the long-lived workflow that takes a sla policy from start to finish.
/// </summary>
public sealed partial class SlaPolicyWorkflowEndpoint : ApiHelpdesk.SlaPolicyWorkflowService.SlaPolicyWorkflowServiceBase
{
    private readonly ApiDevices.DevicePolicyWorkflowService.DevicePolicyWorkflowServiceClient _devicesDevicePolicyWorkflowService;
    private readonly ApiDevices.ManagedDeviceWorkflowService.ManagedDeviceWorkflowServiceClient _devicesManagedDeviceWorkflowService;
    private readonly ApiDocuments.SignatureNotificationService.SignatureNotificationServiceClient _documentsSignatureNotificationService;
    private readonly ApiEmployees.EmployeeService.EmployeeServiceClient _employeesEmployeeService;
    private readonly ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient _employeesOrgUnitQueryService;
    private readonly ApiEmployees.PositionValidationService.PositionValidationServiceClient _employeesPositionValidationService;
    private readonly ApiHelpdesk.EscalationApprovalService.EscalationApprovalServiceClient _helpdeskEscalationApprovalService;
    private readonly ApiHelpdesk.KnowledgeArticleValidationService.KnowledgeArticleValidationServiceClient _helpdeskKnowledgeArticleValidationService;
    private readonly ApiHelpdesk.SlaPolicyLifecycleService.SlaPolicyLifecycleServiceClient _helpdeskSlaPolicyLifecycleService;
    private readonly ApiHelpdesk.TicketWorkflowService.TicketWorkflowServiceClient _helpdeskTicketWorkflowService;
    private readonly ApiItsm.ChangeRequestWorkflowService.ChangeRequestWorkflowServiceClient _itsmChangeRequestWorkflowService;
    private readonly ApiItsm.ProblemSyncService.ProblemSyncServiceClient _itsmProblemSyncService;
    private readonly ApiLicensing.EntitlementApprovalService.EntitlementApprovalServiceClient _licensingEntitlementApprovalService;
    private readonly ApiLicensing.LicenseAssignmentWorkflowService.LicenseAssignmentWorkflowServiceClient _licensingLicenseAssignmentWorkflowService;
    private readonly ILogger<SlaPolicyWorkflowEndpoint> _logger;
    private readonly ApiNotifications.NotificationService.NotificationServiceClient _notificationsNotificationService;
    private readonly RetryPolicy _policy;
    private readonly IRecordRepository _repository;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ISlaPolicyWorkflowEndpointSendResumeSlaPolicyWorkflow1Port _slaPolicyWorkflowEndpointSendResumeSlaPolicyWorkflow1Port;

    public SlaPolicyWorkflowEndpoint(
        ApiDevices.DevicePolicyWorkflowService.DevicePolicyWorkflowServiceClient devicesDevicePolicyWorkflowService,
        ApiDevices.ManagedDeviceWorkflowService.ManagedDeviceWorkflowServiceClient devicesManagedDeviceWorkflowService,
        ApiDocuments.SignatureNotificationService.SignatureNotificationServiceClient documentsSignatureNotificationService,
        ApiEmployees.EmployeeService.EmployeeServiceClient employeesEmployeeService,
        ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient employeesOrgUnitQueryService,
        ApiEmployees.PositionValidationService.PositionValidationServiceClient employeesPositionValidationService,
        ApiHelpdesk.EscalationApprovalService.EscalationApprovalServiceClient helpdeskEscalationApprovalService,
        ApiHelpdesk.KnowledgeArticleValidationService.KnowledgeArticleValidationServiceClient helpdeskKnowledgeArticleValidationService,
        ApiHelpdesk.SlaPolicyLifecycleService.SlaPolicyLifecycleServiceClient helpdeskSlaPolicyLifecycleService,
        ApiHelpdesk.TicketWorkflowService.TicketWorkflowServiceClient helpdeskTicketWorkflowService,
        ApiItsm.ChangeRequestWorkflowService.ChangeRequestWorkflowServiceClient itsmChangeRequestWorkflowService,
        ApiItsm.ProblemSyncService.ProblemSyncServiceClient itsmProblemSyncService,
        ApiLicensing.EntitlementApprovalService.EntitlementApprovalServiceClient licensingEntitlementApprovalService,
        ApiLicensing.LicenseAssignmentWorkflowService.LicenseAssignmentWorkflowServiceClient licensingLicenseAssignmentWorkflowService,
        ILogger<SlaPolicyWorkflowEndpoint> logger,
        ApiNotifications.NotificationService.NotificationServiceClient notificationsNotificationService,
        RetryPolicy policy,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ISlaPolicyWorkflowEndpointSendResumeSlaPolicyWorkflow1Port slaPolicyWorkflowEndpointSendResumeSlaPolicyWorkflow1PortAdapter)
    {
        _devicesDevicePolicyWorkflowService = devicesDevicePolicyWorkflowService;
        _devicesManagedDeviceWorkflowService = devicesManagedDeviceWorkflowService;
        _documentsSignatureNotificationService = documentsSignatureNotificationService;
        _employeesEmployeeService = employeesEmployeeService;
        _employeesOrgUnitQueryService = employeesOrgUnitQueryService;
        _employeesPositionValidationService = employeesPositionValidationService;
        _helpdeskEscalationApprovalService = helpdeskEscalationApprovalService;
        _helpdeskKnowledgeArticleValidationService = helpdeskKnowledgeArticleValidationService;
        _helpdeskSlaPolicyLifecycleService = helpdeskSlaPolicyLifecycleService;
        _helpdeskTicketWorkflowService = helpdeskTicketWorkflowService;
        _itsmChangeRequestWorkflowService = itsmChangeRequestWorkflowService;
        _itsmProblemSyncService = itsmProblemSyncService;
        _licensingEntitlementApprovalService = licensingEntitlementApprovalService;
        _licensingLicenseAssignmentWorkflowService = licensingLicenseAssignmentWorkflowService;
        _logger = logger;
        _notificationsNotificationService = notificationsNotificationService;
        _policy = policy;
        _repository = repository;
        _settings = settings;
        _slaPolicyWorkflowEndpointSendResumeSlaPolicyWorkflow1Port = slaPolicyWorkflowEndpointSendResumeSlaPolicyWorkflow1PortAdapter;
    }
}
