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
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

/// <summary>
/// Computes the figures of a visitor pass, such as totals, taxes, and estimates.
/// </summary>
public sealed partial class VisitorPassCalculationEndpoint : ApiWorkplaces.VisitorPassCalculationService.VisitorPassCalculationServiceBase
{
    private readonly ApiAppointments.AppointmentService.AppointmentServiceClient _appointmentsAppointmentService;
    private readonly ApiCorporateevents.VenueCalculationService.VenueCalculationServiceClient _corporateeventsVenueCalculationService;
    private readonly ApiDocuments.DocumentTemplateWorkflowService.DocumentTemplateWorkflowServiceClient _documentsDocumentTemplateWorkflowService;
    private readonly ApiFeatureflags.FeatureFlagService.FeatureFlagServiceClient _featureflagsFeatureFlagService;
    private readonly CallInvoker _invoker;
    private readonly ApiLedger.LedgerAccountService.LedgerAccountServiceClient _ledgerLedgerAccountService;
    private readonly ILogger<VisitorPassCalculationEndpoint> _logger;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiWorkplaces.ExplainDeskDecisionRequest, CallOptions, AsyncUnaryCall<ApiWorkplaces.ExplainDeskDecisionResponse>> _sendExplainVisitorPassCalculation1;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTravel.TripQueryService.TripQueryServiceClient _travelTripQueryService;
    private readonly IVisitorPassCalculationEndpointSendCalculateVisitorPass2Port _visitorPassCalculationEndpointSendCalculateVisitorPass2Port;
    private readonly IVisitorPassCalculationEndpointSendRecalculateVisitorPasses2Port _visitorPassCalculationEndpointSendRecalculateVisitorPasses2Port;
    private readonly ApiWorkplaces.BuildingCalculationService.BuildingCalculationServiceClient _workplacesBuildingCalculationService;
    private readonly ApiWorkplaces.BuildingValidationService.BuildingValidationServiceClient _workplacesBuildingValidationService;
    private readonly ApiWorkplaces.DeskCalculationService.DeskCalculationServiceClient _workplacesDeskCalculationService;
    private readonly ApiWorkplaces.DeskValidationService.DeskValidationServiceClient _workplacesDeskValidationService;
    private readonly ApiWorkplaces.RoomBookingCalculationService.RoomBookingCalculationServiceClient _workplacesRoomBookingCalculationService;

    public VisitorPassCalculationEndpoint(
        ApiAppointments.AppointmentService.AppointmentServiceClient appointmentsAppointmentService,
        ApiCorporateevents.VenueCalculationService.VenueCalculationServiceClient corporateeventsVenueCalculationService,
        ApiDocuments.DocumentTemplateWorkflowService.DocumentTemplateWorkflowServiceClient documentsDocumentTemplateWorkflowService,
        ApiFeatureflags.FeatureFlagService.FeatureFlagServiceClient featureflagsFeatureFlagService,
        CallInvoker invoker,
        ApiLedger.LedgerAccountService.LedgerAccountServiceClient ledgerLedgerAccountService,
        ILogger<VisitorPassCalculationEndpoint> logger,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiTravel.TripQueryService.TripQueryServiceClient travelTripQueryService,
        IVisitorPassCalculationEndpointSendCalculateVisitorPass2Port visitorPassCalculationEndpointSendCalculateVisitorPass2PortAdapter,
        IVisitorPassCalculationEndpointSendRecalculateVisitorPasses2Port visitorPassCalculationEndpointSendRecalculateVisitorPasses2PortAdapter,
        ApiWorkplaces.BuildingCalculationService.BuildingCalculationServiceClient workplacesBuildingCalculationService,
        ApiWorkplaces.BuildingValidationService.BuildingValidationServiceClient workplacesBuildingValidationService,
        ApiWorkplaces.DeskCalculationService.DeskCalculationServiceClient workplacesDeskCalculationService,
        ApiWorkplaces.DeskValidationService.DeskValidationServiceClient workplacesDeskValidationService,
        ApiWorkplaces.RoomBookingCalculationService.RoomBookingCalculationServiceClient workplacesRoomBookingCalculationService)
    {
        _appointmentsAppointmentService = appointmentsAppointmentService;
        _corporateeventsVenueCalculationService = corporateeventsVenueCalculationService;
        _documentsDocumentTemplateWorkflowService = documentsDocumentTemplateWorkflowService;
        _featureflagsFeatureFlagService = featureflagsFeatureFlagService;
        _invoker = invoker;
        _ledgerLedgerAccountService = ledgerLedgerAccountService;
        _logger = logger;
        _repository = repository;
        _settings = settings;
        _travelTripQueryService = travelTripQueryService;
        _visitorPassCalculationEndpointSendCalculateVisitorPass2Port = visitorPassCalculationEndpointSendCalculateVisitorPass2PortAdapter;
        _visitorPassCalculationEndpointSendRecalculateVisitorPasses2Port = visitorPassCalculationEndpointSendRecalculateVisitorPasses2PortAdapter;
        _workplacesBuildingCalculationService = workplacesBuildingCalculationService;
        _workplacesBuildingValidationService = workplacesBuildingValidationService;
        _workplacesDeskCalculationService = workplacesDeskCalculationService;
        _workplacesDeskValidationService = workplacesDeskValidationService;
        _workplacesRoomBookingCalculationService = workplacesRoomBookingCalculationService;
        _sendExplainVisitorPassCalculation1 = _workplacesDeskValidationService.ExplainDeskDecisionAsync;
    }
}
