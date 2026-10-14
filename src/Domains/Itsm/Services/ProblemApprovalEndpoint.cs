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
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;

namespace Acme.Workforce.Itsm;

/// <summary>
/// Routes problems through approval, with escalation and delegation.
/// </summary>
public sealed partial class ProblemApprovalEndpoint : ApiItsm.ProblemApprovalService.ProblemApprovalServiceBase
{
    private readonly NotificationBus _bus;
    private readonly ApiDevices.ComplianceScanWorkflowService.ComplianceScanWorkflowServiceClient _devicesComplianceScanWorkflowService;
    private readonly ApiDevices.DevicePolicyApprovalService.DevicePolicyApprovalServiceClient _devicesDevicePolicyApprovalService;
    private readonly ApiDocuments.SignatureValidationService.SignatureValidationServiceClient _documentsSignatureValidationService;
    private readonly ApiEmployees.PositionValidationService.PositionValidationServiceClient _employeesPositionValidationService;
    private readonly ApiIdentity.UserQueryService.UserQueryServiceClient _identityUserQueryService;
    private readonly ApiIdentity.UserValidationService.UserValidationServiceClient _identityUserValidationService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.ChangeRequestApprovalService.ChangeRequestApprovalServiceClient _itsmChangeRequestApprovalService;
    private readonly ApiItsm.IncidentApprovalService.IncidentApprovalServiceClient _itsmIncidentApprovalService;
    private readonly ApiItsm.IncidentCalculationService.IncidentCalculationServiceClient _itsmIncidentCalculationService;
    private readonly ApiItsm.ProblemLifecycleService.ProblemLifecycleServiceClient _itsmProblemLifecycleService;
    private readonly ApiItsm.ProblemReportingService.ProblemReportingServiceClient _itsmProblemReportingService;
    private readonly ApiItsm.ServiceAssetApprovalService.ServiceAssetApprovalServiceClient _itsmServiceAssetApprovalService;
    private readonly ILogger<ProblemApprovalEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiEmployees.CheckPositionEligibilityRequest, CallOptions, AsyncUnaryCall<ApiEmployees.CheckPositionEligibilityResponse>> _sendSubmitProblem1;
    private readonly IOptions<MeshOptions> _settings;

    public ProblemApprovalEndpoint(
        NotificationBus bus,
        ApiDevices.ComplianceScanWorkflowService.ComplianceScanWorkflowServiceClient devicesComplianceScanWorkflowService,
        ApiDevices.DevicePolicyApprovalService.DevicePolicyApprovalServiceClient devicesDevicePolicyApprovalService,
        ApiDocuments.SignatureValidationService.SignatureValidationServiceClient documentsSignatureValidationService,
        ApiEmployees.PositionValidationService.PositionValidationServiceClient employeesPositionValidationService,
        ApiIdentity.UserQueryService.UserQueryServiceClient identityUserQueryService,
        ApiIdentity.UserValidationService.UserValidationServiceClient identityUserValidationService,
        CallInvoker invoker,
        ApiItsm.ChangeRequestApprovalService.ChangeRequestApprovalServiceClient itsmChangeRequestApprovalService,
        ApiItsm.IncidentApprovalService.IncidentApprovalServiceClient itsmIncidentApprovalService,
        ApiItsm.IncidentCalculationService.IncidentCalculationServiceClient itsmIncidentCalculationService,
        ApiItsm.ProblemLifecycleService.ProblemLifecycleServiceClient itsmProblemLifecycleService,
        ApiItsm.ProblemReportingService.ProblemReportingServiceClient itsmProblemReportingService,
        ApiItsm.ServiceAssetApprovalService.ServiceAssetApprovalServiceClient itsmServiceAssetApprovalService,
        ILogger<ProblemApprovalEndpoint> logger,
        RetryPolicy policy,
        IRecordRepository repository,
        IOptions<MeshOptions> settings)
    {
        _bus = bus;
        _devicesComplianceScanWorkflowService = devicesComplianceScanWorkflowService;
        _devicesDevicePolicyApprovalService = devicesDevicePolicyApprovalService;
        _documentsSignatureValidationService = documentsSignatureValidationService;
        _employeesPositionValidationService = employeesPositionValidationService;
        _identityUserQueryService = identityUserQueryService;
        _identityUserValidationService = identityUserValidationService;
        _invoker = invoker;
        _itsmChangeRequestApprovalService = itsmChangeRequestApprovalService;
        _itsmIncidentApprovalService = itsmIncidentApprovalService;
        _itsmIncidentCalculationService = itsmIncidentCalculationService;
        _itsmProblemLifecycleService = itsmProblemLifecycleService;
        _itsmProblemReportingService = itsmProblemReportingService;
        _itsmServiceAssetApprovalService = itsmServiceAssetApprovalService;
        _logger = logger;
        _policy = policy;
        _repository = repository;
        _settings = settings;
        _sendSubmitProblem1 = _employeesPositionValidationService.CheckPositionEligibilityAsync;
    }
}
