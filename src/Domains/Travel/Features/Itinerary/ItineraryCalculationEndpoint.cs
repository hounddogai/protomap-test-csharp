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
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiTours = Acme.Tours.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

/// <summary>
/// Computes the figures of an itinerary, such as totals, taxes, and estimates.
/// </summary>
public sealed partial class ItineraryCalculationEndpoint : ApiTravel.ItineraryCalculationService.ItineraryCalculationServiceBase
{
    private readonly ApiAudit.AuditEventQueryService.AuditEventQueryServiceClient _auditAuditEventQueryService;
    private readonly NotificationBus _bus;
    private readonly ApiCorporateevents.CorporateEventQueryService.CorporateEventQueryServiceClient _corporateeventsCorporateEventQueryService;
    private readonly ApiCorporateevents.VenueCalculationService.VenueCalculationServiceClient _corporateeventsVenueCalculationService;
    private readonly ApiDevices.DevicePolicyQueryService.DevicePolicyQueryServiceClient _devicesDevicePolicyQueryService;
    private readonly ApiDocuments.DocumentNotificationService.DocumentNotificationServiceClient _documentsDocumentNotificationService;
    private readonly ApiIdentity.UserValidationService.UserValidationServiceClient _identityUserValidationService;
    private readonly CallInvoker _invoker;
    private readonly IItineraryCalculationEndpointSendGetItineraryRates0Port _itineraryCalculationEndpointSendGetItineraryRates0Port;
    private readonly ApiItsm.IncidentValidationService.IncidentValidationServiceClient _itsmIncidentValidationService;
    private readonly ApiLedger.LedgerAccountQueryService.LedgerAccountQueryServiceClient _ledgerLedgerAccountQueryService;
    private readonly ILogger<ItineraryCalculationEndpoint> _logger;
    private readonly ApiMetrics.MetricSeriesValidationService.MetricSeriesValidationServiceClient _metricsMetricSeriesValidationService;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiTravel.GetTravelBookingRulesRequest, CallOptions, AsyncUnaryCall<ApiTravel.GetTravelBookingRulesResponse>> _sendCompareItineraryOptions1;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTours.TourQueryService.TourQueryServiceClient _toursTourQueryService;
    private readonly ApiTravel.TravelBookingValidationService.TravelBookingValidationServiceClient _travelTravelBookingValidationService;
    private readonly ApiTravel.TravelPolicyCalculationService.TravelPolicyCalculationServiceClient _travelTravelPolicyCalculationService;
    private readonly ApiTravel.TripCalculationService.TripCalculationServiceClient _travelTripCalculationService;
    private readonly ApiTravel.TripValidationService.TripValidationServiceClient _travelTripValidationService;

    public ItineraryCalculationEndpoint(
        ApiAudit.AuditEventQueryService.AuditEventQueryServiceClient auditAuditEventQueryService,
        NotificationBus bus,
        ApiCorporateevents.CorporateEventQueryService.CorporateEventQueryServiceClient corporateeventsCorporateEventQueryService,
        ApiCorporateevents.VenueCalculationService.VenueCalculationServiceClient corporateeventsVenueCalculationService,
        ApiDevices.DevicePolicyQueryService.DevicePolicyQueryServiceClient devicesDevicePolicyQueryService,
        ApiDocuments.DocumentNotificationService.DocumentNotificationServiceClient documentsDocumentNotificationService,
        ApiIdentity.UserValidationService.UserValidationServiceClient identityUserValidationService,
        CallInvoker invoker,
        IItineraryCalculationEndpointSendGetItineraryRates0Port itineraryCalculationEndpointSendGetItineraryRates0PortAdapter,
        ApiItsm.IncidentValidationService.IncidentValidationServiceClient itsmIncidentValidationService,
        ApiLedger.LedgerAccountQueryService.LedgerAccountQueryServiceClient ledgerLedgerAccountQueryService,
        ILogger<ItineraryCalculationEndpoint> logger,
        ApiMetrics.MetricSeriesValidationService.MetricSeriesValidationServiceClient metricsMetricSeriesValidationService,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiTours.TourQueryService.TourQueryServiceClient toursTourQueryService,
        ApiTravel.TravelBookingValidationService.TravelBookingValidationServiceClient travelTravelBookingValidationService,
        ApiTravel.TravelPolicyCalculationService.TravelPolicyCalculationServiceClient travelTravelPolicyCalculationService,
        ApiTravel.TripCalculationService.TripCalculationServiceClient travelTripCalculationService,
        ApiTravel.TripValidationService.TripValidationServiceClient travelTripValidationService)
    {
        _auditAuditEventQueryService = auditAuditEventQueryService;
        _bus = bus;
        _corporateeventsCorporateEventQueryService = corporateeventsCorporateEventQueryService;
        _corporateeventsVenueCalculationService = corporateeventsVenueCalculationService;
        _devicesDevicePolicyQueryService = devicesDevicePolicyQueryService;
        _documentsDocumentNotificationService = documentsDocumentNotificationService;
        _identityUserValidationService = identityUserValidationService;
        _invoker = invoker;
        _itineraryCalculationEndpointSendGetItineraryRates0Port = itineraryCalculationEndpointSendGetItineraryRates0PortAdapter;
        _itsmIncidentValidationService = itsmIncidentValidationService;
        _ledgerLedgerAccountQueryService = ledgerLedgerAccountQueryService;
        _logger = logger;
        _metricsMetricSeriesValidationService = metricsMetricSeriesValidationService;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _settings = settings;
        _toursTourQueryService = toursTourQueryService;
        _travelTravelBookingValidationService = travelTravelBookingValidationService;
        _travelTravelPolicyCalculationService = travelTravelPolicyCalculationService;
        _travelTripCalculationService = travelTripCalculationService;
        _travelTripValidationService = travelTripValidationService;
        _sendCompareItineraryOptions1 = _travelTravelBookingValidationService.GetTravelBookingRulesAsync;
    }
}
