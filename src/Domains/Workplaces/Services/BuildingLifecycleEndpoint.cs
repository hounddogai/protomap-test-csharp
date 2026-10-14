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
using ApiCustomers = Acme.Customers.V1;
using ApiDevices = Acme.Devices.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFraud = Acme.Fraud.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiKeymanagement = Acme.Keymanagement.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

/// <summary>
/// Moves a building between the states of its lifecycle.
/// </summary>
public sealed partial class BuildingLifecycleEndpoint : ApiWorkplaces.BuildingLifecycleService.BuildingLifecycleServiceBase
{
    private readonly ApiAuthorization.AccessPolicyService.AccessPolicyServiceClient _authorizationAccessPolicyService;
    private readonly IBuildingLifecycleEndpointSendSuspendBuilding1Port _buildingLifecycleEndpointSendSuspendBuilding1Port;
    private readonly ApiDevices.DevicePolicyWorkflowService.DevicePolicyWorkflowServiceClient _devicesDevicePolicyWorkflowService;
    private readonly ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient _employeesEmploymentContractCalculationService;
    private readonly ApiEmployees.PositionValidationService.PositionValidationServiceClient _employeesPositionValidationService;
    private readonly ApiFraud.RiskAssessmentCalculationService.RiskAssessmentCalculationServiceClient _fraudRiskAssessmentCalculationService;
    private readonly CallInvoker _invoker;
    private readonly ApiKeymanagement.CryptoKeyQueryService.CryptoKeyQueryServiceClient _keymanagementCryptoKeyQueryService;
    private readonly ILogger<BuildingLifecycleEndpoint> _logger;
    private readonly ApiMetrics.MetricSeriesValidationService.MetricSeriesValidationServiceClient _metricsMetricSeriesValidationService;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiEmployees.CalculateEmploymentContractRequest, CallOptions, AsyncUnaryCall<ApiEmployees.CalculateEmploymentContractResponse>> _sendReinstateBuilding1;
    private readonly ApiTravel.TravelBookingApprovalService.TravelBookingApprovalServiceClient _travelTravelBookingApprovalService;
    private readonly ApiTravel.TravelBookingWorkflowService.TravelBookingWorkflowServiceClient _travelTravelBookingWorkflowService;
    private readonly ApiWorkplaces.BuildingCalculationService.BuildingCalculationServiceClient _workplacesBuildingCalculationService;
    private readonly ApiWorkplaces.BuildingQueryService.BuildingQueryServiceClient _workplacesBuildingQueryService;
    private readonly ApiWorkplaces.BuildingValidationService.BuildingValidationServiceClient _workplacesBuildingValidationService;
    private readonly ApiWorkplaces.DeskQueryService.DeskQueryServiceClient _workplacesDeskQueryService;
    private readonly ApiWorkplaces.VisitorPassCalculationService.VisitorPassCalculationServiceClient _workplacesVisitorPassCalculationService;

    public BuildingLifecycleEndpoint(
        ApiAuthorization.AccessPolicyService.AccessPolicyServiceClient authorizationAccessPolicyService,
        IBuildingLifecycleEndpointSendSuspendBuilding1Port buildingLifecycleEndpointSendSuspendBuilding1PortAdapter,
        ApiDevices.DevicePolicyWorkflowService.DevicePolicyWorkflowServiceClient devicesDevicePolicyWorkflowService,
        ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient employeesEmploymentContractCalculationService,
        ApiEmployees.PositionValidationService.PositionValidationServiceClient employeesPositionValidationService,
        ApiFraud.RiskAssessmentCalculationService.RiskAssessmentCalculationServiceClient fraudRiskAssessmentCalculationService,
        CallInvoker invoker,
        ApiKeymanagement.CryptoKeyQueryService.CryptoKeyQueryServiceClient keymanagementCryptoKeyQueryService,
        ILogger<BuildingLifecycleEndpoint> logger,
        ApiMetrics.MetricSeriesValidationService.MetricSeriesValidationServiceClient metricsMetricSeriesValidationService,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiTravel.TravelBookingApprovalService.TravelBookingApprovalServiceClient travelTravelBookingApprovalService,
        ApiTravel.TravelBookingWorkflowService.TravelBookingWorkflowServiceClient travelTravelBookingWorkflowService,
        ApiWorkplaces.BuildingCalculationService.BuildingCalculationServiceClient workplacesBuildingCalculationService,
        ApiWorkplaces.BuildingQueryService.BuildingQueryServiceClient workplacesBuildingQueryService,
        ApiWorkplaces.BuildingValidationService.BuildingValidationServiceClient workplacesBuildingValidationService,
        ApiWorkplaces.DeskQueryService.DeskQueryServiceClient workplacesDeskQueryService,
        ApiWorkplaces.VisitorPassCalculationService.VisitorPassCalculationServiceClient workplacesVisitorPassCalculationService)
    {
        _authorizationAccessPolicyService = authorizationAccessPolicyService;
        _buildingLifecycleEndpointSendSuspendBuilding1Port = buildingLifecycleEndpointSendSuspendBuilding1PortAdapter;
        _devicesDevicePolicyWorkflowService = devicesDevicePolicyWorkflowService;
        _employeesEmploymentContractCalculationService = employeesEmploymentContractCalculationService;
        _employeesPositionValidationService = employeesPositionValidationService;
        _fraudRiskAssessmentCalculationService = fraudRiskAssessmentCalculationService;
        _invoker = invoker;
        _keymanagementCryptoKeyQueryService = keymanagementCryptoKeyQueryService;
        _logger = logger;
        _metricsMetricSeriesValidationService = metricsMetricSeriesValidationService;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _travelTravelBookingApprovalService = travelTravelBookingApprovalService;
        _travelTravelBookingWorkflowService = travelTravelBookingWorkflowService;
        _workplacesBuildingCalculationService = workplacesBuildingCalculationService;
        _workplacesBuildingQueryService = workplacesBuildingQueryService;
        _workplacesBuildingValidationService = workplacesBuildingValidationService;
        _workplacesDeskQueryService = workplacesDeskQueryService;
        _workplacesVisitorPassCalculationService = workplacesVisitorPassCalculationService;
        _sendReinstateBuilding1 = _employeesEmploymentContractCalculationService.CalculateEmploymentContractAsync;
    }
}
