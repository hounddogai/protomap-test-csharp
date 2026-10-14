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
using ApiBenefits = Acme.Benefits.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;

namespace Acme.Workforce.Employeeportal;

/// <summary>
/// Answers read-only questions about portal homes, for screens and reports.
/// </summary>
public sealed partial class PortalHomeQueryEndpoint : ApiEmployeeportal.PortalHomeQueryService.PortalHomeQueryServiceBase
{
    private readonly ApiAudit.AuditEventQueryService.AuditEventQueryServiceClient _auditAuditEventQueryService;
    private readonly ApiBenefits.BenefitPlanWorkflowService.BenefitPlanWorkflowServiceClient _benefitsBenefitPlanWorkflowService;
    private readonly ApiDocuments.SignatureExportService.SignatureExportServiceClient _documentsSignatureExportService;
    private readonly ApiEmployeeportal.AnnouncementWorkflowService.AnnouncementWorkflowServiceClient _employeeportalAnnouncementWorkflowService;
    private readonly ApiEmployeeportal.PortalHomeIntegrationService.PortalHomeIntegrationServiceClient _employeeportalPortalHomeIntegrationService;
    private readonly ApiEmployeeportal.SelfServiceRequestIntegrationService.SelfServiceRequestIntegrationServiceClient _employeeportalSelfServiceRequestIntegrationService;
    private readonly ApiEmployeeportal.SelfServiceRequestNotificationService.SelfServiceRequestNotificationServiceClient _employeeportalSelfServiceRequestNotificationService;
    private readonly ApiEmployees.EmploymentContractApprovalService.EmploymentContractApprovalServiceClient _employeesEmploymentContractApprovalService;
    private readonly ILogger<PortalHomeQueryEndpoint> _logger;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiEmployeeportal.ListPortalHomeSourcesRequest, CallOptions, AsyncUnaryCall<ApiEmployeeportal.ListPortalHomeSourcesResponse>> _sendFindPortalHomes0;
    private readonly IOptions<MeshOptions> _settings;

    public PortalHomeQueryEndpoint(
        ApiAudit.AuditEventQueryService.AuditEventQueryServiceClient auditAuditEventQueryService,
        ApiBenefits.BenefitPlanWorkflowService.BenefitPlanWorkflowServiceClient benefitsBenefitPlanWorkflowService,
        ApiDocuments.SignatureExportService.SignatureExportServiceClient documentsSignatureExportService,
        ApiEmployeeportal.AnnouncementWorkflowService.AnnouncementWorkflowServiceClient employeeportalAnnouncementWorkflowService,
        ApiEmployeeportal.PortalHomeIntegrationService.PortalHomeIntegrationServiceClient employeeportalPortalHomeIntegrationService,
        ApiEmployeeportal.SelfServiceRequestIntegrationService.SelfServiceRequestIntegrationServiceClient employeeportalSelfServiceRequestIntegrationService,
        ApiEmployeeportal.SelfServiceRequestNotificationService.SelfServiceRequestNotificationServiceClient employeeportalSelfServiceRequestNotificationService,
        ApiEmployees.EmploymentContractApprovalService.EmploymentContractApprovalServiceClient employeesEmploymentContractApprovalService,
        ILogger<PortalHomeQueryEndpoint> logger,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings)
    {
        _auditAuditEventQueryService = auditAuditEventQueryService;
        _benefitsBenefitPlanWorkflowService = benefitsBenefitPlanWorkflowService;
        _documentsSignatureExportService = documentsSignatureExportService;
        _employeeportalAnnouncementWorkflowService = employeeportalAnnouncementWorkflowService;
        _employeeportalPortalHomeIntegrationService = employeeportalPortalHomeIntegrationService;
        _employeeportalSelfServiceRequestIntegrationService = employeeportalSelfServiceRequestIntegrationService;
        _employeeportalSelfServiceRequestNotificationService = employeeportalSelfServiceRequestNotificationService;
        _employeesEmploymentContractApprovalService = employeesEmploymentContractApprovalService;
        _logger = logger;
        _registry = registry;
        _repository = repository;
        _settings = settings;
        _sendFindPortalHomes0 = _employeeportalPortalHomeIntegrationService.ListPortalHomeSourcesAsync;
    }
}
