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
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiPatientapp = Acme.Patientapp.V1;

namespace Acme.Workforce.Employeeportal;

/// <summary>
/// Tells the people who follow an announcement what changed.
/// </summary>
public sealed partial class AnnouncementNotificationEndpoint : ApiEmployeeportal.AnnouncementNotificationService.AnnouncementNotificationServiceBase
{
    private readonly IAnnouncementNotificationEndpointSendSendAnnouncementTestNotice0Port _announcementNotificationEndpointSendSendAnnouncementTestNotice0Port;
    private readonly ApiDevices.DevicePolicyWorkflowService.DevicePolicyWorkflowServiceClient _devicesDevicePolicyWorkflowService;
    private readonly ApiDocuments.DocumentFolderNotificationService.DocumentFolderNotificationServiceClient _documentsDocumentFolderNotificationService;
    private readonly ApiDocuments.SignatureSyncService.SignatureSyncServiceClient _documentsSignatureSyncService;
    private readonly ApiEmployeeportal.AnnouncementIntegrationService.AnnouncementIntegrationServiceClient _employeeportalAnnouncementIntegrationService;
    private readonly ApiEmployeeportal.AnnouncementWorkflowService.AnnouncementWorkflowServiceClient _employeeportalAnnouncementWorkflowService;
    private readonly ApiEmployeeportal.PortalHomeNotificationService.PortalHomeNotificationServiceClient _employeeportalPortalHomeNotificationService;
    private readonly ApiEmployeeportal.SelfServiceRequestIntegrationService.SelfServiceRequestIntegrationServiceClient _employeeportalSelfServiceRequestIntegrationService;
    private readonly ApiEmployeeportal.SelfServiceRequestNotificationService.SelfServiceRequestNotificationServiceClient _employeeportalSelfServiceRequestNotificationService;
    private readonly ApiHelpdesk.EscalationReportingService.EscalationReportingServiceClient _helpdeskEscalationReportingService;
    private readonly CallInvoker _invoker;
    private readonly ILogger<AnnouncementNotificationEndpoint> _logger;
    private readonly ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient _metricsMetricSeriesQueryService;
    private readonly ApiNotifications.NotificationValidationService.NotificationValidationServiceClient _notificationsNotificationValidationService;
    private readonly ApiPatientapp.PatientHomeQueryService.PatientHomeQueryServiceClient _patientappPatientHomeQueryService;
    private readonly ApiPatientapp.PatientHomeService.PatientHomeServiceClient _patientappPatientHomeService;
    private readonly RetryPolicy _policy;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiEmployeeportal.SignalAnnouncementWorkflowRequest, CallOptions, AsyncUnaryCall<ApiEmployeeportal.SignalAnnouncementWorkflowResponse>> _sendUnsubscribeAnnouncementUpdates0;
    private readonly IOptions<MeshOptions> _settings;

    public AnnouncementNotificationEndpoint(
        IAnnouncementNotificationEndpointSendSendAnnouncementTestNotice0Port announcementNotificationEndpointSendSendAnnouncementTestNotice0PortAdapter,
        ApiDevices.DevicePolicyWorkflowService.DevicePolicyWorkflowServiceClient devicesDevicePolicyWorkflowService,
        ApiDocuments.DocumentFolderNotificationService.DocumentFolderNotificationServiceClient documentsDocumentFolderNotificationService,
        ApiDocuments.SignatureSyncService.SignatureSyncServiceClient documentsSignatureSyncService,
        ApiEmployeeportal.AnnouncementIntegrationService.AnnouncementIntegrationServiceClient employeeportalAnnouncementIntegrationService,
        ApiEmployeeportal.AnnouncementWorkflowService.AnnouncementWorkflowServiceClient employeeportalAnnouncementWorkflowService,
        ApiEmployeeportal.PortalHomeNotificationService.PortalHomeNotificationServiceClient employeeportalPortalHomeNotificationService,
        ApiEmployeeportal.SelfServiceRequestIntegrationService.SelfServiceRequestIntegrationServiceClient employeeportalSelfServiceRequestIntegrationService,
        ApiEmployeeportal.SelfServiceRequestNotificationService.SelfServiceRequestNotificationServiceClient employeeportalSelfServiceRequestNotificationService,
        ApiHelpdesk.EscalationReportingService.EscalationReportingServiceClient helpdeskEscalationReportingService,
        CallInvoker invoker,
        ILogger<AnnouncementNotificationEndpoint> logger,
        ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient metricsMetricSeriesQueryService,
        ApiNotifications.NotificationValidationService.NotificationValidationServiceClient notificationsNotificationValidationService,
        ApiPatientapp.PatientHomeQueryService.PatientHomeQueryServiceClient patientappPatientHomeQueryService,
        ApiPatientapp.PatientHomeService.PatientHomeServiceClient patientappPatientHomeService,
        RetryPolicy policy,
        IRecordRepository repository,
        IOptions<MeshOptions> settings)
    {
        _announcementNotificationEndpointSendSendAnnouncementTestNotice0Port = announcementNotificationEndpointSendSendAnnouncementTestNotice0PortAdapter;
        _devicesDevicePolicyWorkflowService = devicesDevicePolicyWorkflowService;
        _documentsDocumentFolderNotificationService = documentsDocumentFolderNotificationService;
        _documentsSignatureSyncService = documentsSignatureSyncService;
        _employeeportalAnnouncementIntegrationService = employeeportalAnnouncementIntegrationService;
        _employeeportalAnnouncementWorkflowService = employeeportalAnnouncementWorkflowService;
        _employeeportalPortalHomeNotificationService = employeeportalPortalHomeNotificationService;
        _employeeportalSelfServiceRequestIntegrationService = employeeportalSelfServiceRequestIntegrationService;
        _employeeportalSelfServiceRequestNotificationService = employeeportalSelfServiceRequestNotificationService;
        _helpdeskEscalationReportingService = helpdeskEscalationReportingService;
        _invoker = invoker;
        _logger = logger;
        _metricsMetricSeriesQueryService = metricsMetricSeriesQueryService;
        _notificationsNotificationValidationService = notificationsNotificationValidationService;
        _patientappPatientHomeQueryService = patientappPatientHomeQueryService;
        _patientappPatientHomeService = patientappPatientHomeService;
        _policy = policy;
        _repository = repository;
        _settings = settings;
        _sendUnsubscribeAnnouncementUpdates0 = _employeeportalAnnouncementWorkflowService.SignalAnnouncementWorkflowAsync;
    }
}
