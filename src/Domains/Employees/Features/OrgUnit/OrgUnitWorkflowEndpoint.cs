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
using ApiAssets = Acme.Assets.V1;
using ApiAuthorization = Acme.Authorization.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Employees;

/// <summary>
/// Runs the long-lived workflow that takes an org unit from start to finish.
/// </summary>
public sealed partial class OrgUnitWorkflowEndpoint : ApiEmployees.OrgUnitWorkflowService.OrgUnitWorkflowServiceBase
{
    private readonly ApiAssets.FixedAssetCalculationService.FixedAssetCalculationServiceClient _assetsFixedAssetCalculationService;
    private readonly ApiAuthorization.AccessPolicyValidationService.AccessPolicyValidationServiceClient _authorizationAccessPolicyValidationService;
    private readonly ApiCorporateevents.CorporateEventApprovalService.CorporateEventApprovalServiceClient _corporateeventsCorporateEventApprovalService;
    private readonly ApiDocuments.DocumentFolderQueryService.DocumentFolderQueryServiceClient _documentsDocumentFolderQueryService;
    private readonly ApiDocuments.DocumentTemplateExportService.DocumentTemplateExportServiceClient _documentsDocumentTemplateExportService;
    private readonly ApiDocuments.DocumentValidationService.DocumentValidationServiceClient _documentsDocumentValidationService;
    private readonly ApiEmployees.EmployeeApprovalService.EmployeeApprovalServiceClient _employeesEmployeeApprovalService;
    private readonly ApiEmployees.EmployeeWorkflowService.EmployeeWorkflowServiceClient _employeesEmployeeWorkflowService;
    private readonly ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient _employeesEmploymentContractCalculationService;
    private readonly ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient _employeesEmploymentContractValidationService;
    private readonly ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient _employeesOrgUnitQueryService;
    private readonly ApiEmployees.PositionApprovalService.PositionApprovalServiceClient _employeesPositionApprovalService;
    private readonly ApiFeatureflags.FeatureFlagService.FeatureFlagServiceClient _featureflagsFeatureFlagService;
    private readonly ApiHelpdesk.EscalationReportingService.EscalationReportingServiceClient _helpdeskEscalationReportingService;
    private readonly ILogger<OrgUnitWorkflowEndpoint> _logger;
    private readonly IOrgUnitWorkflowEndpointSendRetryOrgUnitWorkflow1Port _orgUnitWorkflowEndpointSendRetryOrgUnitWorkflow1Port;
    private readonly RetryPolicy _policy;
    private readonly ApiRecruiting.OfferReportingService.OfferReportingServiceClient _recruitingOfferReportingService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiHelpdesk.GenerateEscalationReportRequest, CallOptions, AsyncUnaryCall<ApiHelpdesk.GenerateEscalationReportResponse>> _sendListOrgUnitWorkflows0;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTimekeeping.ClockEventApprovalService.ClockEventApprovalServiceClient _timekeepingClockEventApprovalService;

    public OrgUnitWorkflowEndpoint(
        ApiAssets.FixedAssetCalculationService.FixedAssetCalculationServiceClient assetsFixedAssetCalculationService,
        ApiAuthorization.AccessPolicyValidationService.AccessPolicyValidationServiceClient authorizationAccessPolicyValidationService,
        ApiCorporateevents.CorporateEventApprovalService.CorporateEventApprovalServiceClient corporateeventsCorporateEventApprovalService,
        ApiDocuments.DocumentFolderQueryService.DocumentFolderQueryServiceClient documentsDocumentFolderQueryService,
        ApiDocuments.DocumentTemplateExportService.DocumentTemplateExportServiceClient documentsDocumentTemplateExportService,
        ApiDocuments.DocumentValidationService.DocumentValidationServiceClient documentsDocumentValidationService,
        ApiEmployees.EmployeeApprovalService.EmployeeApprovalServiceClient employeesEmployeeApprovalService,
        ApiEmployees.EmployeeWorkflowService.EmployeeWorkflowServiceClient employeesEmployeeWorkflowService,
        ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient employeesEmploymentContractCalculationService,
        ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient employeesEmploymentContractValidationService,
        ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient employeesOrgUnitQueryService,
        ApiEmployees.PositionApprovalService.PositionApprovalServiceClient employeesPositionApprovalService,
        ApiFeatureflags.FeatureFlagService.FeatureFlagServiceClient featureflagsFeatureFlagService,
        ApiHelpdesk.EscalationReportingService.EscalationReportingServiceClient helpdeskEscalationReportingService,
        ILogger<OrgUnitWorkflowEndpoint> logger,
        IOrgUnitWorkflowEndpointSendRetryOrgUnitWorkflow1Port orgUnitWorkflowEndpointSendRetryOrgUnitWorkflow1PortAdapter,
        RetryPolicy policy,
        ApiRecruiting.OfferReportingService.OfferReportingServiceClient recruitingOfferReportingService,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiTimekeeping.ClockEventApprovalService.ClockEventApprovalServiceClient timekeepingClockEventApprovalService)
    {
        _assetsFixedAssetCalculationService = assetsFixedAssetCalculationService;
        _authorizationAccessPolicyValidationService = authorizationAccessPolicyValidationService;
        _corporateeventsCorporateEventApprovalService = corporateeventsCorporateEventApprovalService;
        _documentsDocumentFolderQueryService = documentsDocumentFolderQueryService;
        _documentsDocumentTemplateExportService = documentsDocumentTemplateExportService;
        _documentsDocumentValidationService = documentsDocumentValidationService;
        _employeesEmployeeApprovalService = employeesEmployeeApprovalService;
        _employeesEmployeeWorkflowService = employeesEmployeeWorkflowService;
        _employeesEmploymentContractCalculationService = employeesEmploymentContractCalculationService;
        _employeesEmploymentContractValidationService = employeesEmploymentContractValidationService;
        _employeesOrgUnitQueryService = employeesOrgUnitQueryService;
        _employeesPositionApprovalService = employeesPositionApprovalService;
        _featureflagsFeatureFlagService = featureflagsFeatureFlagService;
        _helpdeskEscalationReportingService = helpdeskEscalationReportingService;
        _logger = logger;
        _orgUnitWorkflowEndpointSendRetryOrgUnitWorkflow1Port = orgUnitWorkflowEndpointSendRetryOrgUnitWorkflow1PortAdapter;
        _policy = policy;
        _recruitingOfferReportingService = recruitingOfferReportingService;
        _registry = registry;
        _repository = repository;
        _settings = settings;
        _timekeepingClockEventApprovalService = timekeepingClockEventApprovalService;
        _sendListOrgUnitWorkflows0 = _helpdeskEscalationReportingService.GenerateEscalationReportAsync;
    }
}
