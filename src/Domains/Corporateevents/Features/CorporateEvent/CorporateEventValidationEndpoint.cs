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
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDeliveries = Acme.Deliveries.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDispatch = Acme.Dispatch.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSearch = Acme.Search.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Corporateevents;

/// <summary>
/// Checks corporate events against the rules before anything acts on them.
/// </summary>
public sealed partial class CorporateEventValidationEndpoint : ApiCorporateevents.CorporateEventValidationService.CorporateEventValidationServiceBase
{
    private readonly ICorporateEventValidationEndpointSendGetCorporateEventRules1Port _corporateEventValidationEndpointSendGetCorporateEventRules1Port;
    private readonly ApiCorporateevents.CorporateEventCalculationService.CorporateEventCalculationServiceClient _corporateeventsCorporateEventCalculationService;
    private readonly ApiCorporateevents.RegistrationCalculationService.RegistrationCalculationServiceClient _corporateeventsRegistrationCalculationService;
    private readonly ApiDeliveries.DeliveryQueryService.DeliveryQueryServiceClient _deliveriesDeliveryQueryService;
    private readonly ApiDevices.ComplianceScanValidationService.ComplianceScanValidationServiceClient _devicesComplianceScanValidationService;
    private readonly ApiDispatch.DispatchOrderQueryService.DispatchOrderQueryServiceClient _dispatchDispatchOrderQueryService;
    private readonly ApiDocuments.SignatureSyncService.SignatureSyncServiceClient _documentsSignatureSyncService;
    private readonly ApiHelpdesk.TicketQueryService.TicketQueryServiceClient _helpdeskTicketQueryService;
    private readonly CallInvoker _invoker;
    private readonly ILogger<CorporateEventValidationEndpoint> _logger;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiSearch.SearchIndexQueryService.SearchIndexQueryServiceClient _searchSearchIndexQueryService;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiVenues.VenueValidationService.VenueValidationServiceClient _venuesVenueValidationService;

    public CorporateEventValidationEndpoint(
        ICorporateEventValidationEndpointSendGetCorporateEventRules1Port corporateEventValidationEndpointSendGetCorporateEventRules1PortAdapter,
        ApiCorporateevents.CorporateEventCalculationService.CorporateEventCalculationServiceClient corporateeventsCorporateEventCalculationService,
        ApiCorporateevents.RegistrationCalculationService.RegistrationCalculationServiceClient corporateeventsRegistrationCalculationService,
        ApiDeliveries.DeliveryQueryService.DeliveryQueryServiceClient deliveriesDeliveryQueryService,
        ApiDevices.ComplianceScanValidationService.ComplianceScanValidationServiceClient devicesComplianceScanValidationService,
        ApiDispatch.DispatchOrderQueryService.DispatchOrderQueryServiceClient dispatchDispatchOrderQueryService,
        ApiDocuments.SignatureSyncService.SignatureSyncServiceClient documentsSignatureSyncService,
        ApiHelpdesk.TicketQueryService.TicketQueryServiceClient helpdeskTicketQueryService,
        CallInvoker invoker,
        ILogger<CorporateEventValidationEndpoint> logger,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiSearch.SearchIndexQueryService.SearchIndexQueryServiceClient searchSearchIndexQueryService,
        IOptions<MeshOptions> settings,
        ApiVenues.VenueValidationService.VenueValidationServiceClient venuesVenueValidationService)
    {
        _corporateEventValidationEndpointSendGetCorporateEventRules1Port = corporateEventValidationEndpointSendGetCorporateEventRules1PortAdapter;
        _corporateeventsCorporateEventCalculationService = corporateeventsCorporateEventCalculationService;
        _corporateeventsRegistrationCalculationService = corporateeventsRegistrationCalculationService;
        _deliveriesDeliveryQueryService = deliveriesDeliveryQueryService;
        _devicesComplianceScanValidationService = devicesComplianceScanValidationService;
        _dispatchDispatchOrderQueryService = dispatchDispatchOrderQueryService;
        _documentsSignatureSyncService = documentsSignatureSyncService;
        _helpdeskTicketQueryService = helpdeskTicketQueryService;
        _invoker = invoker;
        _logger = logger;
        _registry = registry;
        _repository = repository;
        _searchSearchIndexQueryService = searchSearchIndexQueryService;
        _settings = settings;
        _venuesVenueValidationService = venuesVenueValidationService;
    }
}
