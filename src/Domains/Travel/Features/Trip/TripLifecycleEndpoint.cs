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
using ApiDrivers = Acme.Drivers.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

/// <summary>
/// Moves a trip between the states of its lifecycle.
/// </summary>
public sealed partial class TripLifecycleEndpoint : ApiTravel.TripLifecycleService.TripLifecycleServiceBase
{
    private readonly ApiDrivers.DriverService.DriverServiceClient _driversDriverService;
    private readonly ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient _employeesEmploymentContractCalculationService;
    private readonly ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient _employeesEmploymentContractValidationService;
    private readonly ApiEmployees.PositionValidationService.PositionValidationServiceClient _employeesPositionValidationService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.ChangeRequestApprovalService.ChangeRequestApprovalServiceClient _itsmChangeRequestApprovalService;
    private readonly ApiItsm.ProblemApprovalService.ProblemApprovalServiceClient _itsmProblemApprovalService;
    private readonly ApiLicensing.EntitlementApprovalService.EntitlementApprovalServiceClient _licensingEntitlementApprovalService;
    private readonly ILogger<TripLifecycleEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiItsm.ApproveChangeRequestRequest, CallOptions, AsyncUnaryCall<ApiItsm.ApproveChangeRequestResponse>> _sendActivateTrip0;
    private readonly Func<ApiTravel.EstimateTripRequest, CallOptions, AsyncUnaryCall<ApiTravel.EstimateTripResponse>> _sendTerminateTrip1;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTravel.ItineraryValidationService.ItineraryValidationServiceClient _travelItineraryValidationService;
    private readonly ApiTravel.TravelBookingValidationService.TravelBookingValidationServiceClient _travelTravelBookingValidationService;
    private readonly ApiTravel.TravelPolicyCalculationService.TravelPolicyCalculationServiceClient _travelTravelPolicyCalculationService;
    private readonly ApiTravel.TripCalculationService.TripCalculationServiceClient _travelTripCalculationService;
    private readonly ApiTravel.TripValidationService.TripValidationServiceClient _travelTripValidationService;

    public TripLifecycleEndpoint(
        ApiDrivers.DriverService.DriverServiceClient driversDriverService,
        ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient employeesEmploymentContractCalculationService,
        ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient employeesEmploymentContractValidationService,
        ApiEmployees.PositionValidationService.PositionValidationServiceClient employeesPositionValidationService,
        CallInvoker invoker,
        ApiItsm.ChangeRequestApprovalService.ChangeRequestApprovalServiceClient itsmChangeRequestApprovalService,
        ApiItsm.ProblemApprovalService.ProblemApprovalServiceClient itsmProblemApprovalService,
        ApiLicensing.EntitlementApprovalService.EntitlementApprovalServiceClient licensingEntitlementApprovalService,
        ILogger<TripLifecycleEndpoint> logger,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiTravel.ItineraryValidationService.ItineraryValidationServiceClient travelItineraryValidationService,
        ApiTravel.TravelBookingValidationService.TravelBookingValidationServiceClient travelTravelBookingValidationService,
        ApiTravel.TravelPolicyCalculationService.TravelPolicyCalculationServiceClient travelTravelPolicyCalculationService,
        ApiTravel.TripCalculationService.TripCalculationServiceClient travelTripCalculationService,
        ApiTravel.TripValidationService.TripValidationServiceClient travelTripValidationService)
    {
        _driversDriverService = driversDriverService;
        _employeesEmploymentContractCalculationService = employeesEmploymentContractCalculationService;
        _employeesEmploymentContractValidationService = employeesEmploymentContractValidationService;
        _employeesPositionValidationService = employeesPositionValidationService;
        _invoker = invoker;
        _itsmChangeRequestApprovalService = itsmChangeRequestApprovalService;
        _itsmProblemApprovalService = itsmProblemApprovalService;
        _licensingEntitlementApprovalService = licensingEntitlementApprovalService;
        _logger = logger;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _settings = settings;
        _travelItineraryValidationService = travelItineraryValidationService;
        _travelTravelBookingValidationService = travelTravelBookingValidationService;
        _travelTravelPolicyCalculationService = travelTravelPolicyCalculationService;
        _travelTripCalculationService = travelTripCalculationService;
        _travelTripValidationService = travelTripValidationService;
        _sendActivateTrip0 = _itsmChangeRequestApprovalService.ApproveChangeRequestAsync;
        _sendTerminateTrip1 = _travelTripCalculationService.EstimateTripAsync;
    }
}
