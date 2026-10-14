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
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiPersonalization = Acme.Personalization.V1;
using ApiProcurement = Acme.Procurement.V1;
using ApiSecrets = Acme.Secrets.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

/// <summary>
/// Builds and schedules reports about visitor passes.
/// </summary>
public sealed partial class VisitorPassReportingEndpoint : ApiWorkplaces.VisitorPassReportingService.VisitorPassReportingServiceBase
{
    private readonly ApiDocuments.DocumentHistoryService.DocumentHistoryServiceClient _documentsDocumentHistoryService;
    private readonly ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient _employeesEmployeeQueryService;
    private readonly ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient _employeesEmploymentContractValidationService;
    private readonly ApiEmployees.PositionValidationService.PositionValidationServiceClient _employeesPositionValidationService;
    private readonly ILogger<VisitorPassReportingEndpoint> _logger;
    private readonly ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient _metricsMetricSeriesQueryService;
    private readonly ApiPersonalization.ProfileQueryService.ProfileQueryServiceClient _personalizationProfileQueryService;
    private readonly RetryPolicy _policy;
    private readonly ApiProcurement.PurchaseRequisitionCalculationService.PurchaseRequisitionCalculationServiceClient _procurementPurchaseRequisitionCalculationService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiSecrets.SecretQueryService.SecretQueryServiceClient _secretsSecretQueryService;
    private readonly Func<ApiEmployees.ScoreEmploymentContractRequest, CallOptions, AsyncUnaryCall<ApiEmployees.ScoreEmploymentContractResponse>> _sendDeleteVisitorPassReport1;
    private readonly Func<ApiWorkplaces.ScoreDeskRequest, CallOptions, AsyncUnaryCall<ApiWorkplaces.ScoreDeskResponse>> _sendStreamVisitorPassMetrics3;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTravel.TravelPolicyReportingService.TravelPolicyReportingServiceClient _travelTravelPolicyReportingService;
    private readonly ApiTravel.TravelPolicySyncService.TravelPolicySyncServiceClient _travelTravelPolicySyncService;
    private readonly ApiWorkplaces.BuildingQueryService.BuildingQueryServiceClient _workplacesBuildingQueryService;
    private readonly ApiWorkplaces.DeskCalculationService.DeskCalculationServiceClient _workplacesDeskCalculationService;
    private readonly ApiWorkplaces.DeskValidationService.DeskValidationServiceClient _workplacesDeskValidationService;
    private readonly ApiWorkplaces.RoomBookingReportingService.RoomBookingReportingServiceClient _workplacesRoomBookingReportingService;
    private readonly ApiWorkplaces.VisitorPassSyncService.VisitorPassSyncServiceClient _workplacesVisitorPassSyncService;
    private readonly ApiWorkplaces.VisitorPassValidationService.VisitorPassValidationServiceClient _workplacesVisitorPassValidationService;

    public VisitorPassReportingEndpoint(
        ApiDocuments.DocumentHistoryService.DocumentHistoryServiceClient documentsDocumentHistoryService,
        ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient employeesEmployeeQueryService,
        ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient employeesEmploymentContractValidationService,
        ApiEmployees.PositionValidationService.PositionValidationServiceClient employeesPositionValidationService,
        ILogger<VisitorPassReportingEndpoint> logger,
        ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient metricsMetricSeriesQueryService,
        ApiPersonalization.ProfileQueryService.ProfileQueryServiceClient personalizationProfileQueryService,
        RetryPolicy policy,
        ApiProcurement.PurchaseRequisitionCalculationService.PurchaseRequisitionCalculationServiceClient procurementPurchaseRequisitionCalculationService,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiSecrets.SecretQueryService.SecretQueryServiceClient secretsSecretQueryService,
        IOptions<MeshOptions> settings,
        ApiTravel.TravelPolicyReportingService.TravelPolicyReportingServiceClient travelTravelPolicyReportingService,
        ApiTravel.TravelPolicySyncService.TravelPolicySyncServiceClient travelTravelPolicySyncService,
        ApiWorkplaces.BuildingQueryService.BuildingQueryServiceClient workplacesBuildingQueryService,
        ApiWorkplaces.DeskCalculationService.DeskCalculationServiceClient workplacesDeskCalculationService,
        ApiWorkplaces.DeskValidationService.DeskValidationServiceClient workplacesDeskValidationService,
        ApiWorkplaces.RoomBookingReportingService.RoomBookingReportingServiceClient workplacesRoomBookingReportingService,
        ApiWorkplaces.VisitorPassSyncService.VisitorPassSyncServiceClient workplacesVisitorPassSyncService,
        ApiWorkplaces.VisitorPassValidationService.VisitorPassValidationServiceClient workplacesVisitorPassValidationService)
    {
        _documentsDocumentHistoryService = documentsDocumentHistoryService;
        _employeesEmployeeQueryService = employeesEmployeeQueryService;
        _employeesEmploymentContractValidationService = employeesEmploymentContractValidationService;
        _employeesPositionValidationService = employeesPositionValidationService;
        _logger = logger;
        _metricsMetricSeriesQueryService = metricsMetricSeriesQueryService;
        _personalizationProfileQueryService = personalizationProfileQueryService;
        _policy = policy;
        _procurementPurchaseRequisitionCalculationService = procurementPurchaseRequisitionCalculationService;
        _registry = registry;
        _repository = repository;
        _secretsSecretQueryService = secretsSecretQueryService;
        _settings = settings;
        _travelTravelPolicyReportingService = travelTravelPolicyReportingService;
        _travelTravelPolicySyncService = travelTravelPolicySyncService;
        _workplacesBuildingQueryService = workplacesBuildingQueryService;
        _workplacesDeskCalculationService = workplacesDeskCalculationService;
        _workplacesDeskValidationService = workplacesDeskValidationService;
        _workplacesRoomBookingReportingService = workplacesRoomBookingReportingService;
        _workplacesVisitorPassSyncService = workplacesVisitorPassSyncService;
        _workplacesVisitorPassValidationService = workplacesVisitorPassValidationService;
        _sendDeleteVisitorPassReport1 = _employeesEmploymentContractValidationService.ScoreEmploymentContractAsync;
        _sendStreamVisitorPassMetrics3 = _workplacesDeskValidationService.ScoreDeskAsync;
    }
}
