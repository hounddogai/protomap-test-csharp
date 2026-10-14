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
using ApiDocuments = Acme.Documents.V1;
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Employeeportal;

/// <summary>
/// Answers read-only questions about self service requests, for screens and reports.
/// </summary>
public sealed partial class SelfServiceRequestQueryEndpoint : ApiEmployeeportal.SelfServiceRequestQueryService.SelfServiceRequestQueryServiceBase
{
    private readonly ApiAuthorization.AccessPolicyQueryService.AccessPolicyQueryServiceClient _authorizationAccessPolicyQueryService;
    private readonly ApiDocuments.DocumentFolderValidationService.DocumentFolderValidationServiceClient _documentsDocumentFolderValidationService;
    private readonly ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient _documentsDocumentTemplateSyncService;
    private readonly ApiEmployeeportal.AnnouncementNotificationService.AnnouncementNotificationServiceClient _employeeportalAnnouncementNotificationService;
    private readonly ApiEmployeeportal.AnnouncementWorkflowService.AnnouncementWorkflowServiceClient _employeeportalAnnouncementWorkflowService;
    private readonly ApiEmployeeportal.PortalHomeIntegrationService.PortalHomeIntegrationServiceClient _employeeportalPortalHomeIntegrationService;
    private readonly ApiEmployeeportal.SelfServiceRequestNotificationService.SelfServiceRequestNotificationServiceClient _employeeportalSelfServiceRequestNotificationService;
    private readonly CallInvoker _invoker;
    private readonly ILogger<SelfServiceRequestQueryEndpoint> _logger;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiWorkplaces.VisitorPassApprovalService.VisitorPassApprovalServiceClient _workplacesVisitorPassApprovalService;

    public SelfServiceRequestQueryEndpoint(
        ApiAuthorization.AccessPolicyQueryService.AccessPolicyQueryServiceClient authorizationAccessPolicyQueryService,
        ApiDocuments.DocumentFolderValidationService.DocumentFolderValidationServiceClient documentsDocumentFolderValidationService,
        ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient documentsDocumentTemplateSyncService,
        ApiEmployeeportal.AnnouncementNotificationService.AnnouncementNotificationServiceClient employeeportalAnnouncementNotificationService,
        ApiEmployeeportal.AnnouncementWorkflowService.AnnouncementWorkflowServiceClient employeeportalAnnouncementWorkflowService,
        ApiEmployeeportal.PortalHomeIntegrationService.PortalHomeIntegrationServiceClient employeeportalPortalHomeIntegrationService,
        ApiEmployeeportal.SelfServiceRequestNotificationService.SelfServiceRequestNotificationServiceClient employeeportalSelfServiceRequestNotificationService,
        CallInvoker invoker,
        ILogger<SelfServiceRequestQueryEndpoint> logger,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiWorkplaces.VisitorPassApprovalService.VisitorPassApprovalServiceClient workplacesVisitorPassApprovalService)
    {
        _authorizationAccessPolicyQueryService = authorizationAccessPolicyQueryService;
        _documentsDocumentFolderValidationService = documentsDocumentFolderValidationService;
        _documentsDocumentTemplateSyncService = documentsDocumentTemplateSyncService;
        _employeeportalAnnouncementNotificationService = employeeportalAnnouncementNotificationService;
        _employeeportalAnnouncementWorkflowService = employeeportalAnnouncementWorkflowService;
        _employeeportalPortalHomeIntegrationService = employeeportalPortalHomeIntegrationService;
        _employeeportalSelfServiceRequestNotificationService = employeeportalSelfServiceRequestNotificationService;
        _invoker = invoker;
        _logger = logger;
        _registry = registry;
        _repository = repository;
        _settings = settings;
        _workplacesVisitorPassApprovalService = workplacesVisitorPassApprovalService;
    }
}
