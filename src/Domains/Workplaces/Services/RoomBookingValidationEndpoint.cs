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
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiSearch = Acme.Search.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

/// <summary>
/// Checks room bookings against the rules before anything acts on them.
/// </summary>
public sealed partial class RoomBookingValidationEndpoint : ApiWorkplaces.RoomBookingValidationService.RoomBookingValidationServiceBase
{
    private readonly ApiDocuments.DocumentValidationService.DocumentValidationServiceClient _documentsDocumentValidationService;
    private readonly ApiHelpdesk.KnowledgeArticleCalculationService.KnowledgeArticleCalculationServiceClient _helpdeskKnowledgeArticleCalculationService;
    private readonly ApiHelpdesk.SlaPolicyValidationService.SlaPolicyValidationServiceClient _helpdeskSlaPolicyValidationService;
    private readonly ApiHelpdesk.TicketCalculationService.TicketCalculationServiceClient _helpdeskTicketCalculationService;
    private readonly ApiIdentity.UserQueryService.UserQueryServiceClient _identityUserQueryService;
    private readonly ApiItsm.ChangeRequestValidationService.ChangeRequestValidationServiceClient _itsmChangeRequestValidationService;
    private readonly ApiLicensing.SoftwareLicenseCalculationService.SoftwareLicenseCalculationServiceClient _licensingSoftwareLicenseCalculationService;
    private readonly ILogger<RoomBookingValidationEndpoint> _logger;
    private readonly ApiNotifications.NotificationValidationService.NotificationValidationServiceClient _notificationsNotificationValidationService;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly IRoomBookingValidationEndpointSendCheckRoomBookingEligibility2Port _roomBookingValidationEndpointSendCheckRoomBookingEligibility2Port;
    private readonly IRoomBookingValidationEndpointSendValidateRoomBooking2Port _roomBookingValidationEndpointSendValidateRoomBooking2Port;
    private readonly ApiSearch.SearchIndexService.SearchIndexServiceClient _searchSearchIndexService;
    private readonly Func<ApiWorkplaces.ExplainBuildingDecisionRequest, CallOptions, AsyncUnaryCall<ApiWorkplaces.ExplainBuildingDecisionResponse>> _sendPreviewRoomBooking1;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiWorkplaces.BuildingValidationService.BuildingValidationServiceClient _workplacesBuildingValidationService;
    private readonly ApiWorkplaces.DeskValidationService.DeskValidationServiceClient _workplacesDeskValidationService;
    private readonly ApiWorkplaces.RoomBookingCalculationService.RoomBookingCalculationServiceClient _workplacesRoomBookingCalculationService;
    private readonly ApiWorkplaces.VisitorPassCalculationService.VisitorPassCalculationServiceClient _workplacesVisitorPassCalculationService;

    public RoomBookingValidationEndpoint(
        ApiDocuments.DocumentValidationService.DocumentValidationServiceClient documentsDocumentValidationService,
        ApiHelpdesk.KnowledgeArticleCalculationService.KnowledgeArticleCalculationServiceClient helpdeskKnowledgeArticleCalculationService,
        ApiHelpdesk.SlaPolicyValidationService.SlaPolicyValidationServiceClient helpdeskSlaPolicyValidationService,
        ApiHelpdesk.TicketCalculationService.TicketCalculationServiceClient helpdeskTicketCalculationService,
        ApiIdentity.UserQueryService.UserQueryServiceClient identityUserQueryService,
        ApiItsm.ChangeRequestValidationService.ChangeRequestValidationServiceClient itsmChangeRequestValidationService,
        ApiLicensing.SoftwareLicenseCalculationService.SoftwareLicenseCalculationServiceClient licensingSoftwareLicenseCalculationService,
        ILogger<RoomBookingValidationEndpoint> logger,
        ApiNotifications.NotificationValidationService.NotificationValidationServiceClient notificationsNotificationValidationService,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository,
        IRoomBookingValidationEndpointSendCheckRoomBookingEligibility2Port roomBookingValidationEndpointSendCheckRoomBookingEligibility2PortAdapter,
        IRoomBookingValidationEndpointSendValidateRoomBooking2Port roomBookingValidationEndpointSendValidateRoomBooking2PortAdapter,
        ApiSearch.SearchIndexService.SearchIndexServiceClient searchSearchIndexService,
        IOptions<MeshOptions> settings,
        ApiWorkplaces.BuildingValidationService.BuildingValidationServiceClient workplacesBuildingValidationService,
        ApiWorkplaces.DeskValidationService.DeskValidationServiceClient workplacesDeskValidationService,
        ApiWorkplaces.RoomBookingCalculationService.RoomBookingCalculationServiceClient workplacesRoomBookingCalculationService,
        ApiWorkplaces.VisitorPassCalculationService.VisitorPassCalculationServiceClient workplacesVisitorPassCalculationService)
    {
        _documentsDocumentValidationService = documentsDocumentValidationService;
        _helpdeskKnowledgeArticleCalculationService = helpdeskKnowledgeArticleCalculationService;
        _helpdeskSlaPolicyValidationService = helpdeskSlaPolicyValidationService;
        _helpdeskTicketCalculationService = helpdeskTicketCalculationService;
        _identityUserQueryService = identityUserQueryService;
        _itsmChangeRequestValidationService = itsmChangeRequestValidationService;
        _licensingSoftwareLicenseCalculationService = licensingSoftwareLicenseCalculationService;
        _logger = logger;
        _notificationsNotificationValidationService = notificationsNotificationValidationService;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _roomBookingValidationEndpointSendCheckRoomBookingEligibility2Port = roomBookingValidationEndpointSendCheckRoomBookingEligibility2PortAdapter;
        _roomBookingValidationEndpointSendValidateRoomBooking2Port = roomBookingValidationEndpointSendValidateRoomBooking2PortAdapter;
        _searchSearchIndexService = searchSearchIndexService;
        _settings = settings;
        _workplacesBuildingValidationService = workplacesBuildingValidationService;
        _workplacesDeskValidationService = workplacesDeskValidationService;
        _workplacesRoomBookingCalculationService = workplacesRoomBookingCalculationService;
        _workplacesVisitorPassCalculationService = workplacesVisitorPassCalculationService;
        _sendPreviewRoomBooking1 = _workplacesBuildingValidationService.ExplainBuildingDecisionAsync;
    }
}
