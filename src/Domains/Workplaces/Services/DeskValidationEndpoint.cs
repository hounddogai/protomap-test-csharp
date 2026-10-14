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
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

/// <summary>
/// Checks desks against the rules before anything acts on them.
/// </summary>
public sealed partial class DeskValidationEndpoint : ApiWorkplaces.DeskValidationService.DeskValidationServiceBase
{
    private readonly NotificationBus _bus;
    private readonly ApiCorporateevents.VenueCalculationService.VenueCalculationServiceClient _corporateeventsVenueCalculationService;
    private readonly IDeskValidationEndpointSendCheckDeskEligibility3Port _deskValidationEndpointSendCheckDeskEligibility3Port;
    private readonly ApiDevices.ComplianceScanCalculationService.ComplianceScanCalculationServiceClient _devicesComplianceScanCalculationService;
    private readonly ApiDocuments.DocumentFolderSyncService.DocumentFolderSyncServiceClient _documentsDocumentFolderSyncService;
    private readonly ApiHelpdesk.KnowledgeArticleCalculationService.KnowledgeArticleCalculationServiceClient _helpdeskKnowledgeArticleCalculationService;
    private readonly ILogger<DeskValidationEndpoint> _logger;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiTravel.TravelBookingValidationService.TravelBookingValidationServiceClient _travelTravelBookingValidationService;
    private readonly ApiTravel.TripQueryService.TripQueryServiceClient _travelTripQueryService;
    private readonly ApiWorkplaces.BuildingValidationService.BuildingValidationServiceClient _workplacesBuildingValidationService;
    private readonly ApiWorkplaces.DeskCalculationService.DeskCalculationServiceClient _workplacesDeskCalculationService;
    private readonly ApiWorkplaces.RoomBookingCalculationService.RoomBookingCalculationServiceClient _workplacesRoomBookingCalculationService;

    public DeskValidationEndpoint(
        NotificationBus bus,
        ApiCorporateevents.VenueCalculationService.VenueCalculationServiceClient corporateeventsVenueCalculationService,
        IDeskValidationEndpointSendCheckDeskEligibility3Port deskValidationEndpointSendCheckDeskEligibility3PortAdapter,
        ApiDevices.ComplianceScanCalculationService.ComplianceScanCalculationServiceClient devicesComplianceScanCalculationService,
        ApiDocuments.DocumentFolderSyncService.DocumentFolderSyncServiceClient documentsDocumentFolderSyncService,
        ApiHelpdesk.KnowledgeArticleCalculationService.KnowledgeArticleCalculationServiceClient helpdeskKnowledgeArticleCalculationService,
        ILogger<DeskValidationEndpoint> logger,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiTravel.TravelBookingValidationService.TravelBookingValidationServiceClient travelTravelBookingValidationService,
        ApiTravel.TripQueryService.TripQueryServiceClient travelTripQueryService,
        ApiWorkplaces.BuildingValidationService.BuildingValidationServiceClient workplacesBuildingValidationService,
        ApiWorkplaces.DeskCalculationService.DeskCalculationServiceClient workplacesDeskCalculationService,
        ApiWorkplaces.RoomBookingCalculationService.RoomBookingCalculationServiceClient workplacesRoomBookingCalculationService)
    {
        _bus = bus;
        _corporateeventsVenueCalculationService = corporateeventsVenueCalculationService;
        _deskValidationEndpointSendCheckDeskEligibility3Port = deskValidationEndpointSendCheckDeskEligibility3PortAdapter;
        _devicesComplianceScanCalculationService = devicesComplianceScanCalculationService;
        _documentsDocumentFolderSyncService = documentsDocumentFolderSyncService;
        _helpdeskKnowledgeArticleCalculationService = helpdeskKnowledgeArticleCalculationService;
        _logger = logger;
        _registry = registry;
        _repository = repository;
        _travelTravelBookingValidationService = travelTravelBookingValidationService;
        _travelTripQueryService = travelTripQueryService;
        _workplacesBuildingValidationService = workplacesBuildingValidationService;
        _workplacesDeskCalculationService = workplacesDeskCalculationService;
        _workplacesRoomBookingCalculationService = workplacesRoomBookingCalculationService;
    }
}
