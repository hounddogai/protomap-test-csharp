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
using ApiDatahub = Acme.Datahub.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEdgegateway = Acme.Edgegateway.V1;
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFanportal = Acme.Fanportal.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLearning = Acme.Learning.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiScheduling = Acme.Scheduling.V1;

namespace Acme.Workforce.Employeeportal;

/// <summary>
/// Connects external systems that send portal home events.
/// </summary>
public sealed partial class PortalHomeIntegrationEndpoint : ApiEmployeeportal.PortalHomeIntegrationService.PortalHomeIntegrationServiceBase
{
    private readonly ApiAuthorization.AccessPolicyService.AccessPolicyServiceClient _authorizationAccessPolicyService;
    private readonly ApiDatahub.WorkspaceQueryService.WorkspaceQueryServiceClient _datahubWorkspaceQueryService;
    private readonly ApiDevices.ManagedDeviceWorkflowService.ManagedDeviceWorkflowServiceClient _devicesManagedDeviceWorkflowService;
    private readonly ApiDocuments.DocumentSyncService.DocumentSyncServiceClient _documentsDocumentSyncService;
    private readonly ApiDocuments.SignatureAdminService.SignatureAdminServiceClient _documentsSignatureAdminService;
    private readonly ApiDocuments.SignatureHistoryService.SignatureHistoryServiceClient _documentsSignatureHistoryService;
    private readonly ApiEdgegateway.RouteBindingService.RouteBindingServiceClient _edgegatewayRouteBindingService;
    private readonly ApiEmployees.EmploymentContractApprovalService.EmploymentContractApprovalServiceClient _employeesEmploymentContractApprovalService;
    private readonly ApiFanportal.FanSessionService.FanSessionServiceClient _fanportalFanSessionService;
    private readonly ApiFeatureflags.FeatureFlagQueryService.FeatureFlagQueryServiceClient _featureflagsFeatureFlagQueryService;
    private readonly ApiItsm.ChangeRequestReportingService.ChangeRequestReportingServiceClient _itsmChangeRequestReportingService;
    private readonly ApiLearning.CourseEnrollmentApprovalService.CourseEnrollmentApprovalServiceClient _learningCourseEnrollmentApprovalService;
    private readonly ILogger<PortalHomeIntegrationEndpoint> _logger;
    private readonly ApiNotifications.NotificationService.NotificationServiceClient _notificationsNotificationService;
    private readonly IPortalHomeIntegrationEndpointSendReplayPortalHomeEvents0Port _portalHomeIntegrationEndpointSendReplayPortalHomeEvents0Port;
    private readonly ApiRecruiting.CandidateWorkflowService.CandidateWorkflowServiceClient _recruitingCandidateWorkflowService;
    private readonly ApiRecruiting.OfferReportingService.OfferReportingServiceClient _recruitingOfferReportingService;
    private readonly IRecordRepository _repository;
    private readonly ApiScheduling.RosterWorkflowService.RosterWorkflowServiceClient _schedulingRosterWorkflowService;
    private readonly Func<ApiEdgegateway.CreateRouteBindingRequest, CallOptions, AsyncUnaryCall<ApiEdgegateway.CreateRouteBindingResponse>> _sendDisconnectPortalHomeSource3;
    private readonly IOptions<MeshOptions> _settings;

    public PortalHomeIntegrationEndpoint(
        ApiAuthorization.AccessPolicyService.AccessPolicyServiceClient authorizationAccessPolicyService,
        ApiDatahub.WorkspaceQueryService.WorkspaceQueryServiceClient datahubWorkspaceQueryService,
        ApiDevices.ManagedDeviceWorkflowService.ManagedDeviceWorkflowServiceClient devicesManagedDeviceWorkflowService,
        ApiDocuments.DocumentSyncService.DocumentSyncServiceClient documentsDocumentSyncService,
        ApiDocuments.SignatureAdminService.SignatureAdminServiceClient documentsSignatureAdminService,
        ApiDocuments.SignatureHistoryService.SignatureHistoryServiceClient documentsSignatureHistoryService,
        ApiEdgegateway.RouteBindingService.RouteBindingServiceClient edgegatewayRouteBindingService,
        ApiEmployees.EmploymentContractApprovalService.EmploymentContractApprovalServiceClient employeesEmploymentContractApprovalService,
        ApiFanportal.FanSessionService.FanSessionServiceClient fanportalFanSessionService,
        ApiFeatureflags.FeatureFlagQueryService.FeatureFlagQueryServiceClient featureflagsFeatureFlagQueryService,
        ApiItsm.ChangeRequestReportingService.ChangeRequestReportingServiceClient itsmChangeRequestReportingService,
        ApiLearning.CourseEnrollmentApprovalService.CourseEnrollmentApprovalServiceClient learningCourseEnrollmentApprovalService,
        ILogger<PortalHomeIntegrationEndpoint> logger,
        ApiNotifications.NotificationService.NotificationServiceClient notificationsNotificationService,
        IPortalHomeIntegrationEndpointSendReplayPortalHomeEvents0Port portalHomeIntegrationEndpointSendReplayPortalHomeEvents0PortAdapter,
        ApiRecruiting.CandidateWorkflowService.CandidateWorkflowServiceClient recruitingCandidateWorkflowService,
        ApiRecruiting.OfferReportingService.OfferReportingServiceClient recruitingOfferReportingService,
        IRecordRepository repository,
        ApiScheduling.RosterWorkflowService.RosterWorkflowServiceClient schedulingRosterWorkflowService,
        IOptions<MeshOptions> settings)
    {
        _authorizationAccessPolicyService = authorizationAccessPolicyService;
        _datahubWorkspaceQueryService = datahubWorkspaceQueryService;
        _devicesManagedDeviceWorkflowService = devicesManagedDeviceWorkflowService;
        _documentsDocumentSyncService = documentsDocumentSyncService;
        _documentsSignatureAdminService = documentsSignatureAdminService;
        _documentsSignatureHistoryService = documentsSignatureHistoryService;
        _edgegatewayRouteBindingService = edgegatewayRouteBindingService;
        _employeesEmploymentContractApprovalService = employeesEmploymentContractApprovalService;
        _fanportalFanSessionService = fanportalFanSessionService;
        _featureflagsFeatureFlagQueryService = featureflagsFeatureFlagQueryService;
        _itsmChangeRequestReportingService = itsmChangeRequestReportingService;
        _learningCourseEnrollmentApprovalService = learningCourseEnrollmentApprovalService;
        _logger = logger;
        _notificationsNotificationService = notificationsNotificationService;
        _portalHomeIntegrationEndpointSendReplayPortalHomeEvents0Port = portalHomeIntegrationEndpointSendReplayPortalHomeEvents0PortAdapter;
        _recruitingCandidateWorkflowService = recruitingCandidateWorkflowService;
        _recruitingOfferReportingService = recruitingOfferReportingService;
        _repository = repository;
        _schedulingRosterWorkflowService = schedulingRosterWorkflowService;
        _settings = settings;
        _sendDisconnectPortalHomeSource3 = _edgegatewayRouteBindingService.CreateRouteBindingAsync;
    }
}
