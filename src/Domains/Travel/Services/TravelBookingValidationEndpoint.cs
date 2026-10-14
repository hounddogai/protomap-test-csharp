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
using ApiAppointments = Acme.Appointments.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiSearch = Acme.Search.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

/// <summary>
/// Checks travel bookings against the rules before anything acts on them.
/// </summary>
public sealed partial class TravelBookingValidationEndpoint : ApiTravel.TravelBookingValidationService.TravelBookingValidationServiceBase
{
    private readonly ApiAppointments.AppointmentService.AppointmentServiceClient _appointmentsAppointmentService;
    private readonly ApiCorporateevents.RegistrationCalculationService.RegistrationCalculationServiceClient _corporateeventsRegistrationCalculationService;
    private readonly ApiHelpdesk.KnowledgeArticleQueryService.KnowledgeArticleQueryServiceClient _helpdeskKnowledgeArticleQueryService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.IncidentQueryService.IncidentQueryServiceClient _itsmIncidentQueryService;
    private readonly ApiLedger.LedgerAccountQueryService.LedgerAccountQueryServiceClient _ledgerLedgerAccountQueryService;
    private readonly ApiLedger.LedgerAccountService.LedgerAccountServiceClient _ledgerLedgerAccountService;
    private readonly ApiLicensing.SoftwareLicenseValidationService.SoftwareLicenseValidationServiceClient _licensingSoftwareLicenseValidationService;
    private readonly ILogger<TravelBookingValidationEndpoint> _logger;
    private readonly ApiNotifications.NotificationValidationService.NotificationValidationServiceClient _notificationsNotificationValidationService;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiSearch.SearchIndexQueryService.SearchIndexQueryServiceClient _searchSearchIndexQueryService;
    private readonly Func<ApiItsm.SuggestIncidentsRequest, CallOptions, AsyncUnaryCall<ApiItsm.SuggestIncidentsResponse>> _sendGetTravelBookingRules0;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ITravelBookingValidationEndpointSendScoreTravelBooking0Port _travelBookingValidationEndpointSendScoreTravelBooking0Port;
    private readonly ApiTravel.TravelPolicyCalculationService.TravelPolicyCalculationServiceClient _travelTravelPolicyCalculationService;
    private readonly ApiTravel.TripCalculationService.TripCalculationServiceClient _travelTripCalculationService;

    public TravelBookingValidationEndpoint(
        ApiAppointments.AppointmentService.AppointmentServiceClient appointmentsAppointmentService,
        ApiCorporateevents.RegistrationCalculationService.RegistrationCalculationServiceClient corporateeventsRegistrationCalculationService,
        ApiHelpdesk.KnowledgeArticleQueryService.KnowledgeArticleQueryServiceClient helpdeskKnowledgeArticleQueryService,
        CallInvoker invoker,
        ApiItsm.IncidentQueryService.IncidentQueryServiceClient itsmIncidentQueryService,
        ApiLedger.LedgerAccountQueryService.LedgerAccountQueryServiceClient ledgerLedgerAccountQueryService,
        ApiLedger.LedgerAccountService.LedgerAccountServiceClient ledgerLedgerAccountService,
        ApiLicensing.SoftwareLicenseValidationService.SoftwareLicenseValidationServiceClient licensingSoftwareLicenseValidationService,
        ILogger<TravelBookingValidationEndpoint> logger,
        ApiNotifications.NotificationValidationService.NotificationValidationServiceClient notificationsNotificationValidationService,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiSearch.SearchIndexQueryService.SearchIndexQueryServiceClient searchSearchIndexQueryService,
        IOptions<MeshOptions> settings,
        ITravelBookingValidationEndpointSendScoreTravelBooking0Port travelBookingValidationEndpointSendScoreTravelBooking0PortAdapter,
        ApiTravel.TravelPolicyCalculationService.TravelPolicyCalculationServiceClient travelTravelPolicyCalculationService,
        ApiTravel.TripCalculationService.TripCalculationServiceClient travelTripCalculationService)
    {
        _appointmentsAppointmentService = appointmentsAppointmentService;
        _corporateeventsRegistrationCalculationService = corporateeventsRegistrationCalculationService;
        _helpdeskKnowledgeArticleQueryService = helpdeskKnowledgeArticleQueryService;
        _invoker = invoker;
        _itsmIncidentQueryService = itsmIncidentQueryService;
        _ledgerLedgerAccountQueryService = ledgerLedgerAccountQueryService;
        _ledgerLedgerAccountService = ledgerLedgerAccountService;
        _licensingSoftwareLicenseValidationService = licensingSoftwareLicenseValidationService;
        _logger = logger;
        _notificationsNotificationValidationService = notificationsNotificationValidationService;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _searchSearchIndexQueryService = searchSearchIndexQueryService;
        _settings = settings;
        _travelBookingValidationEndpointSendScoreTravelBooking0Port = travelBookingValidationEndpointSendScoreTravelBooking0PortAdapter;
        _travelTravelPolicyCalculationService = travelTravelPolicyCalculationService;
        _travelTripCalculationService = travelTripCalculationService;
        _sendGetTravelBookingRules0 = _itsmIncidentQueryService.SuggestIncidentsAsync;
    }
}
