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
using ApiLicensing = Acme.Licensing.V1;
using ApiSellers = Acme.Sellers.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

/// <summary>
/// Builds and schedules reports about travel policies.
/// </summary>
public sealed partial class TravelPolicyReportingEndpoint : ApiTravel.TravelPolicyReportingService.TravelPolicyReportingServiceBase
{
    private readonly NotificationBus _bus;
    private readonly ApiDocuments.DocumentTemplateWorkflowService.DocumentTemplateWorkflowServiceClient _documentsDocumentTemplateWorkflowService;
    private readonly ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient _employeesEmploymentContractValidationService;
    private readonly ApiEmployees.PositionValidationService.PositionValidationServiceClient _employeesPositionValidationService;
    private readonly CallInvoker _invoker;
    private readonly ApiLicensing.EntitlementWorkflowService.EntitlementWorkflowServiceClient _licensingEntitlementWorkflowService;
    private readonly ILogger<TravelPolicyReportingEndpoint> _logger;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiSellers.SellerCalculationService.SellerCalculationServiceClient _sellersSellerCalculationService;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTravel.ItineraryValidationService.ItineraryValidationServiceClient _travelItineraryValidationService;
    private readonly ApiTravel.TravelPolicyCalculationService.TravelPolicyCalculationServiceClient _travelTravelPolicyCalculationService;
    private readonly ApiTravel.TravelPolicySyncService.TravelPolicySyncServiceClient _travelTravelPolicySyncService;
    private readonly ApiTravel.TripCalculationService.TripCalculationServiceClient _travelTripCalculationService;

    public TravelPolicyReportingEndpoint(
        NotificationBus bus,
        ApiDocuments.DocumentTemplateWorkflowService.DocumentTemplateWorkflowServiceClient documentsDocumentTemplateWorkflowService,
        ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient employeesEmploymentContractValidationService,
        ApiEmployees.PositionValidationService.PositionValidationServiceClient employeesPositionValidationService,
        CallInvoker invoker,
        ApiLicensing.EntitlementWorkflowService.EntitlementWorkflowServiceClient licensingEntitlementWorkflowService,
        ILogger<TravelPolicyReportingEndpoint> logger,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiSellers.SellerCalculationService.SellerCalculationServiceClient sellersSellerCalculationService,
        IOptions<MeshOptions> settings,
        ApiTravel.ItineraryValidationService.ItineraryValidationServiceClient travelItineraryValidationService,
        ApiTravel.TravelPolicyCalculationService.TravelPolicyCalculationServiceClient travelTravelPolicyCalculationService,
        ApiTravel.TravelPolicySyncService.TravelPolicySyncServiceClient travelTravelPolicySyncService,
        ApiTravel.TripCalculationService.TripCalculationServiceClient travelTripCalculationService)
    {
        _bus = bus;
        _documentsDocumentTemplateWorkflowService = documentsDocumentTemplateWorkflowService;
        _employeesEmploymentContractValidationService = employeesEmploymentContractValidationService;
        _employeesPositionValidationService = employeesPositionValidationService;
        _invoker = invoker;
        _licensingEntitlementWorkflowService = licensingEntitlementWorkflowService;
        _logger = logger;
        _registry = registry;
        _repository = repository;
        _sellersSellerCalculationService = sellersSellerCalculationService;
        _settings = settings;
        _travelItineraryValidationService = travelItineraryValidationService;
        _travelTravelPolicyCalculationService = travelTravelPolicyCalculationService;
        _travelTravelPolicySyncService = travelTravelPolicySyncService;
        _travelTripCalculationService = travelTripCalculationService;
    }
}
