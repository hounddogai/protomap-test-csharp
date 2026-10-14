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
using ApiCommon = Acme.Common.V1;
using ApiConfiguration = Acme.Configuration.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;

namespace Acme.Workforce.Itsm;

/// <summary>
/// Moves a problem between the states of its lifecycle.
/// </summary>
public sealed partial class ProblemLifecycleEndpoint : ApiItsm.ProblemLifecycleService.ProblemLifecycleServiceBase
{
    private readonly ApiAudit.AuditEventService.AuditEventServiceClient _auditAuditEventService;
    private readonly ApiConfiguration.ConfigSetService.ConfigSetServiceClient _configurationConfigSetService;
    private readonly ApiDevices.DevicePolicyWorkflowService.DevicePolicyWorkflowServiceClient _devicesDevicePolicyWorkflowService;
    private readonly ApiDevices.ManagedDeviceApprovalService.ManagedDeviceApprovalServiceClient _devicesManagedDeviceApprovalService;
    private readonly ApiDocuments.DocumentTemplateNotificationService.DocumentTemplateNotificationServiceClient _documentsDocumentTemplateNotificationService;
    private readonly ApiDocuments.SignatureCalculationService.SignatureCalculationServiceClient _documentsSignatureCalculationService;
    private readonly ApiDocuments.SignatureValidationService.SignatureValidationServiceClient _documentsSignatureValidationService;
    private readonly ApiEmployees.EmployeeService.EmployeeServiceClient _employeesEmployeeService;
    private readonly ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient _employeesEmploymentContractCalculationService;
    private readonly ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient _employeesEmploymentContractValidationService;
    private readonly ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient _employeesOrgUnitQueryService;
    private readonly ApiFeatureflags.FeatureFlagQueryService.FeatureFlagQueryServiceClient _featureflagsFeatureFlagQueryService;
    private readonly ApiIdentity.UserValidationService.UserValidationServiceClient _identityUserValidationService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.ChangeRequestReportingService.ChangeRequestReportingServiceClient _itsmChangeRequestReportingService;
    private readonly ApiItsm.ChangeRequestValidationService.ChangeRequestValidationServiceClient _itsmChangeRequestValidationService;
    private readonly ApiItsm.ProblemSyncService.ProblemSyncServiceClient _itsmProblemSyncService;
    private readonly ApiItsm.ProblemValidationService.ProblemValidationServiceClient _itsmProblemValidationService;
    private readonly ApiItsm.ServiceAssetCalculationService.ServiceAssetCalculationServiceClient _itsmServiceAssetCalculationService;
    private readonly ApiItsm.ServiceAssetReportingService.ServiceAssetReportingServiceClient _itsmServiceAssetReportingService;
    private readonly ApiLicensing.EntitlementWorkflowService.EntitlementWorkflowServiceClient _licensingEntitlementWorkflowService;
    private readonly ILogger<ProblemLifecycleEndpoint> _logger;
    private readonly IProblemLifecycleEndpointSendExpireProblem2Port _problemLifecycleEndpointSendExpireProblem2Port;
    private readonly IProblemLifecycleEndpointSendTerminateProblem1Port _problemLifecycleEndpointSendTerminateProblem1Port;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly IOptions<MeshOptions> _settings;

    public ProblemLifecycleEndpoint(
        ApiAudit.AuditEventService.AuditEventServiceClient auditAuditEventService,
        ApiConfiguration.ConfigSetService.ConfigSetServiceClient configurationConfigSetService,
        ApiDevices.DevicePolicyWorkflowService.DevicePolicyWorkflowServiceClient devicesDevicePolicyWorkflowService,
        ApiDevices.ManagedDeviceApprovalService.ManagedDeviceApprovalServiceClient devicesManagedDeviceApprovalService,
        ApiDocuments.DocumentTemplateNotificationService.DocumentTemplateNotificationServiceClient documentsDocumentTemplateNotificationService,
        ApiDocuments.SignatureCalculationService.SignatureCalculationServiceClient documentsSignatureCalculationService,
        ApiDocuments.SignatureValidationService.SignatureValidationServiceClient documentsSignatureValidationService,
        ApiEmployees.EmployeeService.EmployeeServiceClient employeesEmployeeService,
        ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient employeesEmploymentContractCalculationService,
        ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient employeesEmploymentContractValidationService,
        ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient employeesOrgUnitQueryService,
        ApiFeatureflags.FeatureFlagQueryService.FeatureFlagQueryServiceClient featureflagsFeatureFlagQueryService,
        ApiIdentity.UserValidationService.UserValidationServiceClient identityUserValidationService,
        CallInvoker invoker,
        ApiItsm.ChangeRequestReportingService.ChangeRequestReportingServiceClient itsmChangeRequestReportingService,
        ApiItsm.ChangeRequestValidationService.ChangeRequestValidationServiceClient itsmChangeRequestValidationService,
        ApiItsm.ProblemSyncService.ProblemSyncServiceClient itsmProblemSyncService,
        ApiItsm.ProblemValidationService.ProblemValidationServiceClient itsmProblemValidationService,
        ApiItsm.ServiceAssetCalculationService.ServiceAssetCalculationServiceClient itsmServiceAssetCalculationService,
        ApiItsm.ServiceAssetReportingService.ServiceAssetReportingServiceClient itsmServiceAssetReportingService,
        ApiLicensing.EntitlementWorkflowService.EntitlementWorkflowServiceClient licensingEntitlementWorkflowService,
        ILogger<ProblemLifecycleEndpoint> logger,
        IProblemLifecycleEndpointSendExpireProblem2Port problemLifecycleEndpointSendExpireProblem2PortAdapter,
        IProblemLifecycleEndpointSendTerminateProblem1Port problemLifecycleEndpointSendTerminateProblem1PortAdapter,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings)
    {
        _auditAuditEventService = auditAuditEventService;
        _configurationConfigSetService = configurationConfigSetService;
        _devicesDevicePolicyWorkflowService = devicesDevicePolicyWorkflowService;
        _devicesManagedDeviceApprovalService = devicesManagedDeviceApprovalService;
        _documentsDocumentTemplateNotificationService = documentsDocumentTemplateNotificationService;
        _documentsSignatureCalculationService = documentsSignatureCalculationService;
        _documentsSignatureValidationService = documentsSignatureValidationService;
        _employeesEmployeeService = employeesEmployeeService;
        _employeesEmploymentContractCalculationService = employeesEmploymentContractCalculationService;
        _employeesEmploymentContractValidationService = employeesEmploymentContractValidationService;
        _employeesOrgUnitQueryService = employeesOrgUnitQueryService;
        _featureflagsFeatureFlagQueryService = featureflagsFeatureFlagQueryService;
        _identityUserValidationService = identityUserValidationService;
        _invoker = invoker;
        _itsmChangeRequestReportingService = itsmChangeRequestReportingService;
        _itsmChangeRequestValidationService = itsmChangeRequestValidationService;
        _itsmProblemSyncService = itsmProblemSyncService;
        _itsmProblemValidationService = itsmProblemValidationService;
        _itsmServiceAssetCalculationService = itsmServiceAssetCalculationService;
        _itsmServiceAssetReportingService = itsmServiceAssetReportingService;
        _licensingEntitlementWorkflowService = licensingEntitlementWorkflowService;
        _logger = logger;
        _problemLifecycleEndpointSendExpireProblem2Port = problemLifecycleEndpointSendExpireProblem2PortAdapter;
        _problemLifecycleEndpointSendTerminateProblem1Port = problemLifecycleEndpointSendTerminateProblem1PortAdapter;
        _registry = registry;
        _repository = repository;
        _settings = settings;
    }
}
