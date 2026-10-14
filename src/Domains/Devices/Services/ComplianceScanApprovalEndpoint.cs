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
using ApiDevices = Acme.Devices.V1;
using ApiDrivers = Acme.Drivers.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Devices;

/// <summary>
/// Routes compliance scans through approval, with escalation and delegation.
/// </summary>
public sealed partial class ComplianceScanApprovalEndpoint : ApiDevices.ComplianceScanApprovalService.ComplianceScanApprovalServiceBase
{
    private readonly ApiAuthorization.AccessPolicyQueryService.AccessPolicyQueryServiceClient _authorizationAccessPolicyQueryService;
    private readonly NotificationBus _bus;
    private readonly ApiDevices.ComplianceScanCalculationService.ComplianceScanCalculationServiceClient _devicesComplianceScanCalculationService;
    private readonly ApiDevices.ComplianceScanLifecycleService.ComplianceScanLifecycleServiceClient _devicesComplianceScanLifecycleService;
    private readonly ApiDevices.DevicePolicyApprovalService.DevicePolicyApprovalServiceClient _devicesDevicePolicyApprovalService;
    private readonly ApiDevices.ManagedDeviceService.ManagedDeviceServiceClient _devicesManagedDeviceService;
    private readonly ApiDevices.ManagedDeviceWorkflowService.ManagedDeviceWorkflowServiceClient _devicesManagedDeviceWorkflowService;
    private readonly ApiDrivers.DriverService.DriverServiceClient _driversDriverService;
    private readonly CallInvoker _invoker;
    private readonly ILogger<ComplianceScanApprovalEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly ApiRecruiting.CandidateCalculationService.CandidateCalculationServiceClient _recruitingCandidateCalculationService;
    private readonly ApiRecruiting.CandidateValidationService.CandidateValidationServiceClient _recruitingCandidateValidationService;
    private readonly ApiRecruiting.InterviewValidationService.InterviewValidationServiceClient _recruitingInterviewValidationService;
    private readonly ApiRecruiting.OfferCalculationService.OfferCalculationServiceClient _recruitingOfferCalculationService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiDevices.SubmitDevicePolicyRequest, CallOptions, AsyncUnaryCall<ApiDevices.SubmitDevicePolicyResponse>> _sendEscalateComplianceScan0;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiWorkplaces.DeskQueryService.DeskQueryServiceClient _workplacesDeskQueryService;

    public ComplianceScanApprovalEndpoint(
        ApiAuthorization.AccessPolicyQueryService.AccessPolicyQueryServiceClient authorizationAccessPolicyQueryService,
        NotificationBus bus,
        ApiDevices.ComplianceScanCalculationService.ComplianceScanCalculationServiceClient devicesComplianceScanCalculationService,
        ApiDevices.ComplianceScanLifecycleService.ComplianceScanLifecycleServiceClient devicesComplianceScanLifecycleService,
        ApiDevices.DevicePolicyApprovalService.DevicePolicyApprovalServiceClient devicesDevicePolicyApprovalService,
        ApiDevices.ManagedDeviceService.ManagedDeviceServiceClient devicesManagedDeviceService,
        ApiDevices.ManagedDeviceWorkflowService.ManagedDeviceWorkflowServiceClient devicesManagedDeviceWorkflowService,
        ApiDrivers.DriverService.DriverServiceClient driversDriverService,
        CallInvoker invoker,
        ILogger<ComplianceScanApprovalEndpoint> logger,
        RetryPolicy policy,
        ApiRecruiting.CandidateCalculationService.CandidateCalculationServiceClient recruitingCandidateCalculationService,
        ApiRecruiting.CandidateValidationService.CandidateValidationServiceClient recruitingCandidateValidationService,
        ApiRecruiting.InterviewValidationService.InterviewValidationServiceClient recruitingInterviewValidationService,
        ApiRecruiting.OfferCalculationService.OfferCalculationServiceClient recruitingOfferCalculationService,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiWorkplaces.DeskQueryService.DeskQueryServiceClient workplacesDeskQueryService)
    {
        _authorizationAccessPolicyQueryService = authorizationAccessPolicyQueryService;
        _bus = bus;
        _devicesComplianceScanCalculationService = devicesComplianceScanCalculationService;
        _devicesComplianceScanLifecycleService = devicesComplianceScanLifecycleService;
        _devicesDevicePolicyApprovalService = devicesDevicePolicyApprovalService;
        _devicesManagedDeviceService = devicesManagedDeviceService;
        _devicesManagedDeviceWorkflowService = devicesManagedDeviceWorkflowService;
        _driversDriverService = driversDriverService;
        _invoker = invoker;
        _logger = logger;
        _policy = policy;
        _recruitingCandidateCalculationService = recruitingCandidateCalculationService;
        _recruitingCandidateValidationService = recruitingCandidateValidationService;
        _recruitingInterviewValidationService = recruitingInterviewValidationService;
        _recruitingOfferCalculationService = recruitingOfferCalculationService;
        _registry = registry;
        _repository = repository;
        _settings = settings;
        _workplacesDeskQueryService = workplacesDeskQueryService;
        _sendEscalateComplianceScan0 = _devicesDevicePolicyApprovalService.SubmitDevicePolicyAsync;
    }
}
