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
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiMaintenance = Acme.Maintenance.V1;
using ApiPromotions = Acme.Promotions.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Employees;

/// <summary>
/// Routes employment contracts through approval, with escalation and delegation.
/// </summary>
public sealed partial class EmploymentContractApprovalEndpoint : ApiEmployees.EmploymentContractApprovalService.EmploymentContractApprovalServiceBase
{
    private readonly ApiAuthorization.AccessPolicyQueryService.AccessPolicyQueryServiceClient _authorizationAccessPolicyQueryService;
    private readonly ApiAuthorization.AccessPolicyService.AccessPolicyServiceClient _authorizationAccessPolicyService;
    private readonly NotificationBus _bus;
    private readonly ApiCorporateevents.RegistrationLifecycleService.RegistrationLifecycleServiceClient _corporateeventsRegistrationLifecycleService;
    private readonly ApiDocuments.DocumentValidationService.DocumentValidationServiceClient _documentsDocumentValidationService;
    private readonly ApiEmployees.EmployeeApprovalService.EmployeeApprovalServiceClient _employeesEmployeeApprovalService;
    private readonly ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient _employeesEmployeeQueryService;
    private readonly ApiEmployees.EmployeeService.EmployeeServiceClient _employeesEmployeeService;
    private readonly ApiEmployees.EmployeeWorkflowService.EmployeeWorkflowServiceClient _employeesEmployeeWorkflowService;
    private readonly ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient _employeesEmploymentContractCalculationService;
    private readonly ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient _employeesEmploymentContractValidationService;
    private readonly ApiEmployees.OrgUnitWorkflowService.OrgUnitWorkflowServiceClient _employeesOrgUnitWorkflowService;
    private readonly ApiEmployees.PositionValidationService.PositionValidationServiceClient _employeesPositionValidationService;
    private readonly IEmploymentContractApprovalEndpointSendDelegateEmploymentContractApproval2Port _employmentContractApprovalEndpointSendDelegateEmploymentContractApproval2Port;
    private readonly ApiFeatureflags.FeatureFlagQueryService.FeatureFlagQueryServiceClient _featureflagsFeatureFlagQueryService;
    private readonly ApiLedger.LedgerAccountValidationService.LedgerAccountValidationServiceClient _ledgerLedgerAccountValidationService;
    private readonly ApiLicensing.LicenseAssignmentApprovalService.LicenseAssignmentApprovalServiceClient _licensingLicenseAssignmentApprovalService;
    private readonly ILogger<EmploymentContractApprovalEndpoint> _logger;
    private readonly ApiMaintenance.WorkOrderCalculationService.WorkOrderCalculationServiceClient _maintenanceWorkOrderCalculationService;
    private readonly ApiPromotions.PromotionService.PromotionServiceClient _promotionsPromotionService;
    private readonly ApiRecruiting.JobOpeningWorkflowService.JobOpeningWorkflowServiceClient _recruitingJobOpeningWorkflowService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiEmployees.CountEmployeesRequest, CallOptions, AsyncUnaryCall<ApiEmployees.CountEmployeesResponse>> _sendRecallEmploymentContract3;
    private readonly IOptions<MeshOptions> _settings;

    public EmploymentContractApprovalEndpoint(
        ApiAuthorization.AccessPolicyQueryService.AccessPolicyQueryServiceClient authorizationAccessPolicyQueryService,
        ApiAuthorization.AccessPolicyService.AccessPolicyServiceClient authorizationAccessPolicyService,
        NotificationBus bus,
        ApiCorporateevents.RegistrationLifecycleService.RegistrationLifecycleServiceClient corporateeventsRegistrationLifecycleService,
        ApiDocuments.DocumentValidationService.DocumentValidationServiceClient documentsDocumentValidationService,
        ApiEmployees.EmployeeApprovalService.EmployeeApprovalServiceClient employeesEmployeeApprovalService,
        ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient employeesEmployeeQueryService,
        ApiEmployees.EmployeeService.EmployeeServiceClient employeesEmployeeService,
        ApiEmployees.EmployeeWorkflowService.EmployeeWorkflowServiceClient employeesEmployeeWorkflowService,
        ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient employeesEmploymentContractCalculationService,
        ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient employeesEmploymentContractValidationService,
        ApiEmployees.OrgUnitWorkflowService.OrgUnitWorkflowServiceClient employeesOrgUnitWorkflowService,
        ApiEmployees.PositionValidationService.PositionValidationServiceClient employeesPositionValidationService,
        IEmploymentContractApprovalEndpointSendDelegateEmploymentContractApproval2Port employmentContractApprovalEndpointSendDelegateEmploymentContractApproval2PortAdapter,
        ApiFeatureflags.FeatureFlagQueryService.FeatureFlagQueryServiceClient featureflagsFeatureFlagQueryService,
        ApiLedger.LedgerAccountValidationService.LedgerAccountValidationServiceClient ledgerLedgerAccountValidationService,
        ApiLicensing.LicenseAssignmentApprovalService.LicenseAssignmentApprovalServiceClient licensingLicenseAssignmentApprovalService,
        ILogger<EmploymentContractApprovalEndpoint> logger,
        ApiMaintenance.WorkOrderCalculationService.WorkOrderCalculationServiceClient maintenanceWorkOrderCalculationService,
        ApiPromotions.PromotionService.PromotionServiceClient promotionsPromotionService,
        ApiRecruiting.JobOpeningWorkflowService.JobOpeningWorkflowServiceClient recruitingJobOpeningWorkflowService,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings)
    {
        _authorizationAccessPolicyQueryService = authorizationAccessPolicyQueryService;
        _authorizationAccessPolicyService = authorizationAccessPolicyService;
        _bus = bus;
        _corporateeventsRegistrationLifecycleService = corporateeventsRegistrationLifecycleService;
        _documentsDocumentValidationService = documentsDocumentValidationService;
        _employeesEmployeeApprovalService = employeesEmployeeApprovalService;
        _employeesEmployeeQueryService = employeesEmployeeQueryService;
        _employeesEmployeeService = employeesEmployeeService;
        _employeesEmployeeWorkflowService = employeesEmployeeWorkflowService;
        _employeesEmploymentContractCalculationService = employeesEmploymentContractCalculationService;
        _employeesEmploymentContractValidationService = employeesEmploymentContractValidationService;
        _employeesOrgUnitWorkflowService = employeesOrgUnitWorkflowService;
        _employeesPositionValidationService = employeesPositionValidationService;
        _employmentContractApprovalEndpointSendDelegateEmploymentContractApproval2Port = employmentContractApprovalEndpointSendDelegateEmploymentContractApproval2PortAdapter;
        _featureflagsFeatureFlagQueryService = featureflagsFeatureFlagQueryService;
        _ledgerLedgerAccountValidationService = ledgerLedgerAccountValidationService;
        _licensingLicenseAssignmentApprovalService = licensingLicenseAssignmentApprovalService;
        _logger = logger;
        _maintenanceWorkOrderCalculationService = maintenanceWorkOrderCalculationService;
        _promotionsPromotionService = promotionsPromotionService;
        _recruitingJobOpeningWorkflowService = recruitingJobOpeningWorkflowService;
        _registry = registry;
        _repository = repository;
        _settings = settings;
        _sendRecallEmploymentContract3 = _employeesEmployeeQueryService.CountEmployeesAsync;
    }
}
