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
using ApiBoxoffice = Acme.Boxoffice.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFraud = Acme.Fraud.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiInsurance = Acme.Insurance.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

/// <summary>
/// Keeps visitor passes in step with the systems of record in other regions.
/// </summary>
public sealed partial class VisitorPassSyncEndpoint : ApiWorkplaces.VisitorPassSyncService.VisitorPassSyncServiceBase
{
    private readonly ApiBoxoffice.BoxOfficeSaleQueryService.BoxOfficeSaleQueryServiceClient _boxofficeBoxOfficeSaleQueryService;
    private readonly ApiCorporateevents.RegistrationApprovalService.RegistrationApprovalServiceClient _corporateeventsRegistrationApprovalService;
    private readonly ApiDocuments.DocumentTemplateExportService.DocumentTemplateExportServiceClient _documentsDocumentTemplateExportService;
    private readonly ApiDocuments.SignatureHistoryService.SignatureHistoryServiceClient _documentsSignatureHistoryService;
    private readonly ApiDocuments.SignatureNotificationService.SignatureNotificationServiceClient _documentsSignatureNotificationService;
    private readonly ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient _employeesEmploymentContractValidationService;
    private readonly ApiFraud.RiskAssessmentQueryService.RiskAssessmentQueryServiceClient _fraudRiskAssessmentQueryService;
    private readonly ApiIdentity.UserValidationService.UserValidationServiceClient _identityUserValidationService;
    private readonly ApiInsurance.PolicyService.PolicyServiceClient _insurancePolicyService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.ChangeRequestReportingService.ChangeRequestReportingServiceClient _itsmChangeRequestReportingService;
    private readonly ApiLicensing.SoftwareLicenseWorkflowService.SoftwareLicenseWorkflowServiceClient _licensingSoftwareLicenseWorkflowService;
    private readonly ILogger<VisitorPassSyncEndpoint> _logger;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiWorkplaces.BuildingValidationService.BuildingValidationServiceClient _workplacesBuildingValidationService;
    private readonly ApiWorkplaces.VisitorPassValidationService.VisitorPassValidationServiceClient _workplacesVisitorPassValidationService;

    public VisitorPassSyncEndpoint(
        ApiBoxoffice.BoxOfficeSaleQueryService.BoxOfficeSaleQueryServiceClient boxofficeBoxOfficeSaleQueryService,
        ApiCorporateevents.RegistrationApprovalService.RegistrationApprovalServiceClient corporateeventsRegistrationApprovalService,
        ApiDocuments.DocumentTemplateExportService.DocumentTemplateExportServiceClient documentsDocumentTemplateExportService,
        ApiDocuments.SignatureHistoryService.SignatureHistoryServiceClient documentsSignatureHistoryService,
        ApiDocuments.SignatureNotificationService.SignatureNotificationServiceClient documentsSignatureNotificationService,
        ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient employeesEmploymentContractValidationService,
        ApiFraud.RiskAssessmentQueryService.RiskAssessmentQueryServiceClient fraudRiskAssessmentQueryService,
        ApiIdentity.UserValidationService.UserValidationServiceClient identityUserValidationService,
        ApiInsurance.PolicyService.PolicyServiceClient insurancePolicyService,
        CallInvoker invoker,
        ApiItsm.ChangeRequestReportingService.ChangeRequestReportingServiceClient itsmChangeRequestReportingService,
        ApiLicensing.SoftwareLicenseWorkflowService.SoftwareLicenseWorkflowServiceClient licensingSoftwareLicenseWorkflowService,
        ILogger<VisitorPassSyncEndpoint> logger,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiWorkplaces.BuildingValidationService.BuildingValidationServiceClient workplacesBuildingValidationService,
        ApiWorkplaces.VisitorPassValidationService.VisitorPassValidationServiceClient workplacesVisitorPassValidationService)
    {
        _boxofficeBoxOfficeSaleQueryService = boxofficeBoxOfficeSaleQueryService;
        _corporateeventsRegistrationApprovalService = corporateeventsRegistrationApprovalService;
        _documentsDocumentTemplateExportService = documentsDocumentTemplateExportService;
        _documentsSignatureHistoryService = documentsSignatureHistoryService;
        _documentsSignatureNotificationService = documentsSignatureNotificationService;
        _employeesEmploymentContractValidationService = employeesEmploymentContractValidationService;
        _fraudRiskAssessmentQueryService = fraudRiskAssessmentQueryService;
        _identityUserValidationService = identityUserValidationService;
        _insurancePolicyService = insurancePolicyService;
        _invoker = invoker;
        _itsmChangeRequestReportingService = itsmChangeRequestReportingService;
        _licensingSoftwareLicenseWorkflowService = licensingSoftwareLicenseWorkflowService;
        _logger = logger;
        _registry = registry;
        _repository = repository;
        _workplacesBuildingValidationService = workplacesBuildingValidationService;
        _workplacesVisitorPassValidationService = workplacesVisitorPassValidationService;
    }
}
