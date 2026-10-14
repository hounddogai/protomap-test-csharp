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
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;

namespace Acme.Workforce.Employeeportal;

/// <summary>
/// Manages portal homes: creates, reads, changes, and lists them.
/// </summary>
public sealed partial class PortalHomeEndpoint : ApiEmployeeportal.PortalHomeService.PortalHomeServiceBase
{
    private readonly ApiAuthorization.AccessPolicyValidationService.AccessPolicyValidationServiceClient _authorizationAccessPolicyValidationService;
    private readonly NotificationBus _bus;
    private readonly ApiDocuments.DocumentSyncService.DocumentSyncServiceClient _documentsDocumentSyncService;
    private readonly ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient _documentsDocumentTemplateSyncService;
    private readonly ApiDocuments.DocumentTemplateWorkflowService.DocumentTemplateWorkflowServiceClient _documentsDocumentTemplateWorkflowService;
    private readonly ApiDocuments.SignatureAdminService.SignatureAdminServiceClient _documentsSignatureAdminService;
    private readonly ApiDocuments.SignatureExportService.SignatureExportServiceClient _documentsSignatureExportService;
    private readonly ApiEmployeeportal.AnnouncementIntegrationService.AnnouncementIntegrationServiceClient _employeeportalAnnouncementIntegrationService;
    private readonly ApiEmployeeportal.AnnouncementNotificationService.AnnouncementNotificationServiceClient _employeeportalAnnouncementNotificationService;
    private readonly ApiEmployeeportal.AnnouncementWorkflowService.AnnouncementWorkflowServiceClient _employeeportalAnnouncementWorkflowService;
    private readonly ApiEmployeeportal.SelfServiceRequestIntegrationService.SelfServiceRequestIntegrationServiceClient _employeeportalSelfServiceRequestIntegrationService;
    private readonly ApiEmployeeportal.SelfServiceRequestNotificationService.SelfServiceRequestNotificationServiceClient _employeeportalSelfServiceRequestNotificationService;
    private readonly ApiEmployees.PositionApprovalService.PositionApprovalServiceClient _employeesPositionApprovalService;
    private readonly ApiEmployees.PositionWorkflowService.PositionWorkflowServiceClient _employeesPositionWorkflowService;
    private readonly ApiLicensing.SoftwareLicenseApprovalService.SoftwareLicenseApprovalServiceClient _licensingSoftwareLicenseApprovalService;
    private readonly ILogger<PortalHomeEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly IRecordRepository _repository;

    public PortalHomeEndpoint(
        ApiAuthorization.AccessPolicyValidationService.AccessPolicyValidationServiceClient authorizationAccessPolicyValidationService,
        NotificationBus bus,
        ApiDocuments.DocumentSyncService.DocumentSyncServiceClient documentsDocumentSyncService,
        ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient documentsDocumentTemplateSyncService,
        ApiDocuments.DocumentTemplateWorkflowService.DocumentTemplateWorkflowServiceClient documentsDocumentTemplateWorkflowService,
        ApiDocuments.SignatureAdminService.SignatureAdminServiceClient documentsSignatureAdminService,
        ApiDocuments.SignatureExportService.SignatureExportServiceClient documentsSignatureExportService,
        ApiEmployeeportal.AnnouncementIntegrationService.AnnouncementIntegrationServiceClient employeeportalAnnouncementIntegrationService,
        ApiEmployeeportal.AnnouncementNotificationService.AnnouncementNotificationServiceClient employeeportalAnnouncementNotificationService,
        ApiEmployeeportal.AnnouncementWorkflowService.AnnouncementWorkflowServiceClient employeeportalAnnouncementWorkflowService,
        ApiEmployeeportal.SelfServiceRequestIntegrationService.SelfServiceRequestIntegrationServiceClient employeeportalSelfServiceRequestIntegrationService,
        ApiEmployeeportal.SelfServiceRequestNotificationService.SelfServiceRequestNotificationServiceClient employeeportalSelfServiceRequestNotificationService,
        ApiEmployees.PositionApprovalService.PositionApprovalServiceClient employeesPositionApprovalService,
        ApiEmployees.PositionWorkflowService.PositionWorkflowServiceClient employeesPositionWorkflowService,
        ApiLicensing.SoftwareLicenseApprovalService.SoftwareLicenseApprovalServiceClient licensingSoftwareLicenseApprovalService,
        ILogger<PortalHomeEndpoint> logger,
        RetryPolicy policy,
        IRecordRepository repository)
    {
        _authorizationAccessPolicyValidationService = authorizationAccessPolicyValidationService;
        _bus = bus;
        _documentsDocumentSyncService = documentsDocumentSyncService;
        _documentsDocumentTemplateSyncService = documentsDocumentTemplateSyncService;
        _documentsDocumentTemplateWorkflowService = documentsDocumentTemplateWorkflowService;
        _documentsSignatureAdminService = documentsSignatureAdminService;
        _documentsSignatureExportService = documentsSignatureExportService;
        _employeeportalAnnouncementIntegrationService = employeeportalAnnouncementIntegrationService;
        _employeeportalAnnouncementNotificationService = employeeportalAnnouncementNotificationService;
        _employeeportalAnnouncementWorkflowService = employeeportalAnnouncementWorkflowService;
        _employeeportalSelfServiceRequestIntegrationService = employeeportalSelfServiceRequestIntegrationService;
        _employeeportalSelfServiceRequestNotificationService = employeeportalSelfServiceRequestNotificationService;
        _employeesPositionApprovalService = employeesPositionApprovalService;
        _employeesPositionWorkflowService = employeesPositionWorkflowService;
        _licensingSoftwareLicenseApprovalService = licensingSoftwareLicenseApprovalService;
        _logger = logger;
        _policy = policy;
        _repository = repository;
    }
}
