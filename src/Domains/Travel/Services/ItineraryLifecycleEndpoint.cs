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
using ApiEmployees = Acme.Employees.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiTravel = Acme.Travel.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Travel;

/// <summary>
/// Moves an itinerary between the states of its lifecycle.
/// </summary>
public sealed partial class ItineraryLifecycleEndpoint : ApiTravel.ItineraryLifecycleService.ItineraryLifecycleServiceBase
{
    private readonly ApiEmployees.EmployeeService.EmployeeServiceClient _employeesEmployeeService;
    private readonly ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient _employeesOrgUnitQueryService;
    private readonly ApiFeatureflags.FeatureFlagValidationService.FeatureFlagValidationServiceClient _featureflagsFeatureFlagValidationService;
    private readonly ApiHelpdesk.KnowledgeArticleApprovalService.KnowledgeArticleApprovalServiceClient _helpdeskKnowledgeArticleApprovalService;
    private readonly ApiIdentity.UserValidationService.UserValidationServiceClient _identityUserValidationService;
    private readonly CallInvoker _invoker;
    private readonly IItineraryLifecycleEndpointSendSuspendItinerary0Port _itineraryLifecycleEndpointSendSuspendItinerary0Port;
    private readonly ApiLicensing.LicenseAssignmentApprovalService.LicenseAssignmentApprovalServiceClient _licensingLicenseAssignmentApprovalService;
    private readonly ILogger<ItineraryLifecycleEndpoint> _logger;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiLicensing.SubmitLicenseAssignmentRequest, CallOptions, AsyncUnaryCall<ApiLicensing.SubmitLicenseAssignmentResponse>> _sendActivateItinerary0;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTravel.ItineraryReportingService.ItineraryReportingServiceClient _travelItineraryReportingService;
    private readonly ApiTravel.TravelBookingValidationService.TravelBookingValidationServiceClient _travelTravelBookingValidationService;
    private readonly ApiTravel.TripLifecycleService.TripLifecycleServiceClient _travelTripLifecycleService;
    private readonly ApiVenues.VenueQueryService.VenueQueryServiceClient _venuesVenueQueryService;

    public ItineraryLifecycleEndpoint(
        ApiEmployees.EmployeeService.EmployeeServiceClient employeesEmployeeService,
        ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient employeesOrgUnitQueryService,
        ApiFeatureflags.FeatureFlagValidationService.FeatureFlagValidationServiceClient featureflagsFeatureFlagValidationService,
        ApiHelpdesk.KnowledgeArticleApprovalService.KnowledgeArticleApprovalServiceClient helpdeskKnowledgeArticleApprovalService,
        ApiIdentity.UserValidationService.UserValidationServiceClient identityUserValidationService,
        CallInvoker invoker,
        IItineraryLifecycleEndpointSendSuspendItinerary0Port itineraryLifecycleEndpointSendSuspendItinerary0PortAdapter,
        ApiLicensing.LicenseAssignmentApprovalService.LicenseAssignmentApprovalServiceClient licensingLicenseAssignmentApprovalService,
        ILogger<ItineraryLifecycleEndpoint> logger,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiTravel.ItineraryReportingService.ItineraryReportingServiceClient travelItineraryReportingService,
        ApiTravel.TravelBookingValidationService.TravelBookingValidationServiceClient travelTravelBookingValidationService,
        ApiTravel.TripLifecycleService.TripLifecycleServiceClient travelTripLifecycleService,
        ApiVenues.VenueQueryService.VenueQueryServiceClient venuesVenueQueryService)
    {
        _employeesEmployeeService = employeesEmployeeService;
        _employeesOrgUnitQueryService = employeesOrgUnitQueryService;
        _featureflagsFeatureFlagValidationService = featureflagsFeatureFlagValidationService;
        _helpdeskKnowledgeArticleApprovalService = helpdeskKnowledgeArticleApprovalService;
        _identityUserValidationService = identityUserValidationService;
        _invoker = invoker;
        _itineraryLifecycleEndpointSendSuspendItinerary0Port = itineraryLifecycleEndpointSendSuspendItinerary0PortAdapter;
        _licensingLicenseAssignmentApprovalService = licensingLicenseAssignmentApprovalService;
        _logger = logger;
        _registry = registry;
        _repository = repository;
        _settings = settings;
        _travelItineraryReportingService = travelItineraryReportingService;
        _travelTravelBookingValidationService = travelTravelBookingValidationService;
        _travelTripLifecycleService = travelTripLifecycleService;
        _venuesVenueQueryService = venuesVenueQueryService;
        _sendActivateItinerary0 = _licensingLicenseAssignmentApprovalService.SubmitLicenseAssignmentAsync;
    }
}
