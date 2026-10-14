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
using ApiAudiences = Acme.Audiences.V1;
using ApiAudit = Acme.Audit.V1;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiMaintenance = Acme.Maintenance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTravel = Acme.Travel.V1;
using ApiVenues = Acme.Venues.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

/// <summary>
/// Computes the figures of a building, such as totals, taxes, and estimates.
/// </summary>
public sealed partial class BuildingCalculationEndpoint : ApiWorkplaces.BuildingCalculationService.BuildingCalculationServiceBase
{
    private readonly ApiAudiences.AudienceQueryService.AudienceQueryServiceClient _audiencesAudienceQueryService;
    private readonly ApiAudit.AuditEventValidationService.AuditEventValidationServiceClient _auditAuditEventValidationService;
    private readonly IBuildingCalculationEndpointSendSimulateBuilding1Port _buildingCalculationEndpointSendSimulateBuilding1Port;
    private readonly ApiDevices.ComplianceScanCalculationService.ComplianceScanCalculationServiceClient _devicesComplianceScanCalculationService;
    private readonly ApiDocuments.DocumentFolderHistoryService.DocumentFolderHistoryServiceClient _documentsDocumentFolderHistoryService;
    private readonly ApiDocuments.DocumentSyncService.DocumentSyncServiceClient _documentsDocumentSyncService;
    private readonly ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient _documentsDocumentTemplateSyncService;
    private readonly ApiHelpdesk.EscalationValidationService.EscalationValidationServiceClient _helpdeskEscalationValidationService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.ChangeRequestQueryService.ChangeRequestQueryServiceClient _itsmChangeRequestQueryService;
    private readonly ApiItsm.IncidentQueryService.IncidentQueryServiceClient _itsmIncidentQueryService;
    private readonly ILogger<BuildingCalculationEndpoint> _logger;
    private readonly ApiMaintenance.WorkOrderQueryService.WorkOrderQueryServiceClient _maintenanceWorkOrderQueryService;
    private readonly RetryPolicy _policy;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiMaintenance.FindWorkOrdersRequest, CallOptions, AsyncUnaryCall<ApiMaintenance.FindWorkOrdersResponse>> _sendQuoteBuilding0;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTravel.ItineraryValidationService.ItineraryValidationServiceClient _travelItineraryValidationService;
    private readonly ApiTravel.TripService.TripServiceClient _travelTripService;
    private readonly ApiVenues.VenueQueryService.VenueQueryServiceClient _venuesVenueQueryService;

    public BuildingCalculationEndpoint(
        ApiAudiences.AudienceQueryService.AudienceQueryServiceClient audiencesAudienceQueryService,
        ApiAudit.AuditEventValidationService.AuditEventValidationServiceClient auditAuditEventValidationService,
        IBuildingCalculationEndpointSendSimulateBuilding1Port buildingCalculationEndpointSendSimulateBuilding1PortAdapter,
        ApiDevices.ComplianceScanCalculationService.ComplianceScanCalculationServiceClient devicesComplianceScanCalculationService,
        ApiDocuments.DocumentFolderHistoryService.DocumentFolderHistoryServiceClient documentsDocumentFolderHistoryService,
        ApiDocuments.DocumentSyncService.DocumentSyncServiceClient documentsDocumentSyncService,
        ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient documentsDocumentTemplateSyncService,
        ApiHelpdesk.EscalationValidationService.EscalationValidationServiceClient helpdeskEscalationValidationService,
        CallInvoker invoker,
        ApiItsm.ChangeRequestQueryService.ChangeRequestQueryServiceClient itsmChangeRequestQueryService,
        ApiItsm.IncidentQueryService.IncidentQueryServiceClient itsmIncidentQueryService,
        ILogger<BuildingCalculationEndpoint> logger,
        ApiMaintenance.WorkOrderQueryService.WorkOrderQueryServiceClient maintenanceWorkOrderQueryService,
        RetryPolicy policy,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiTravel.ItineraryValidationService.ItineraryValidationServiceClient travelItineraryValidationService,
        ApiTravel.TripService.TripServiceClient travelTripService,
        ApiVenues.VenueQueryService.VenueQueryServiceClient venuesVenueQueryService)
    {
        _audiencesAudienceQueryService = audiencesAudienceQueryService;
        _auditAuditEventValidationService = auditAuditEventValidationService;
        _buildingCalculationEndpointSendSimulateBuilding1Port = buildingCalculationEndpointSendSimulateBuilding1PortAdapter;
        _devicesComplianceScanCalculationService = devicesComplianceScanCalculationService;
        _documentsDocumentFolderHistoryService = documentsDocumentFolderHistoryService;
        _documentsDocumentSyncService = documentsDocumentSyncService;
        _documentsDocumentTemplateSyncService = documentsDocumentTemplateSyncService;
        _helpdeskEscalationValidationService = helpdeskEscalationValidationService;
        _invoker = invoker;
        _itsmChangeRequestQueryService = itsmChangeRequestQueryService;
        _itsmIncidentQueryService = itsmIncidentQueryService;
        _logger = logger;
        _maintenanceWorkOrderQueryService = maintenanceWorkOrderQueryService;
        _policy = policy;
        _repository = repository;
        _settings = settings;
        _travelItineraryValidationService = travelItineraryValidationService;
        _travelTripService = travelTripService;
        _venuesVenueQueryService = venuesVenueQueryService;
        _sendQuoteBuilding0 = _maintenanceWorkOrderQueryService.FindWorkOrdersAsync;
    }
}
