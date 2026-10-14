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
using ApiChat = Acme.Chat.V1;
using ApiCommon = Acme.Common.V1;
using ApiConcessions = Acme.Concessions.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiTravel = Acme.Travel.V1;
using ApiTreasury = Acme.Treasury.V1;

namespace Acme.Workforce.Travel;

/// <summary>
/// Runs the long-lived workflow that takes a trip from start to finish.
/// </summary>
public sealed partial class TripWorkflowEndpoint : ApiTravel.TripWorkflowService.TripWorkflowServiceBase
{
    private readonly ApiChat.ChatRoomCalculationService.ChatRoomCalculationServiceClient _chatChatRoomCalculationService;
    private readonly ApiConcessions.ConcessionStandQueryService.ConcessionStandQueryServiceClient _concessionsConcessionStandQueryService;
    private readonly ApiCorporateevents.RegistrationWorkflowService.RegistrationWorkflowServiceClient _corporateeventsRegistrationWorkflowService;
    private readonly ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient _employeesEmploymentContractValidationService;
    private readonly ApiHelpdesk.TicketApprovalService.TicketApprovalServiceClient _helpdeskTicketApprovalService;
    private readonly ILogger<TripWorkflowEndpoint> _logger;
    private readonly ApiNotifications.NotificationValidationService.NotificationValidationServiceClient _notificationsNotificationValidationService;
    private readonly IRecordRepository _repository;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTravel.ItineraryReportingService.ItineraryReportingServiceClient _travelItineraryReportingService;
    private readonly ApiTravel.ItinerarySyncService.ItinerarySyncServiceClient _travelItinerarySyncService;
    private readonly ApiTravel.TravelPolicyReportingService.TravelPolicyReportingServiceClient _travelTravelPolicyReportingService;
    private readonly ApiTravel.TravelPolicySyncService.TravelPolicySyncServiceClient _travelTravelPolicySyncService;
    private readonly ApiTreasury.CashPositionService.CashPositionServiceClient _treasuryCashPositionService;
    private readonly ITripWorkflowEndpointSendResumeTripWorkflow1Port _tripWorkflowEndpointSendResumeTripWorkflow1Port;

    public TripWorkflowEndpoint(
        ApiChat.ChatRoomCalculationService.ChatRoomCalculationServiceClient chatChatRoomCalculationService,
        ApiConcessions.ConcessionStandQueryService.ConcessionStandQueryServiceClient concessionsConcessionStandQueryService,
        ApiCorporateevents.RegistrationWorkflowService.RegistrationWorkflowServiceClient corporateeventsRegistrationWorkflowService,
        ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient employeesEmploymentContractValidationService,
        ApiHelpdesk.TicketApprovalService.TicketApprovalServiceClient helpdeskTicketApprovalService,
        ILogger<TripWorkflowEndpoint> logger,
        ApiNotifications.NotificationValidationService.NotificationValidationServiceClient notificationsNotificationValidationService,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiTravel.ItineraryReportingService.ItineraryReportingServiceClient travelItineraryReportingService,
        ApiTravel.ItinerarySyncService.ItinerarySyncServiceClient travelItinerarySyncService,
        ApiTravel.TravelPolicyReportingService.TravelPolicyReportingServiceClient travelTravelPolicyReportingService,
        ApiTravel.TravelPolicySyncService.TravelPolicySyncServiceClient travelTravelPolicySyncService,
        ApiTreasury.CashPositionService.CashPositionServiceClient treasuryCashPositionService,
        ITripWorkflowEndpointSendResumeTripWorkflow1Port tripWorkflowEndpointSendResumeTripWorkflow1PortAdapter)
    {
        _chatChatRoomCalculationService = chatChatRoomCalculationService;
        _concessionsConcessionStandQueryService = concessionsConcessionStandQueryService;
        _corporateeventsRegistrationWorkflowService = corporateeventsRegistrationWorkflowService;
        _employeesEmploymentContractValidationService = employeesEmploymentContractValidationService;
        _helpdeskTicketApprovalService = helpdeskTicketApprovalService;
        _logger = logger;
        _notificationsNotificationValidationService = notificationsNotificationValidationService;
        _repository = repository;
        _settings = settings;
        _travelItineraryReportingService = travelItineraryReportingService;
        _travelItinerarySyncService = travelItinerarySyncService;
        _travelTravelPolicyReportingService = travelTravelPolicyReportingService;
        _travelTravelPolicySyncService = travelTravelPolicySyncService;
        _treasuryCashPositionService = treasuryCashPositionService;
        _tripWorkflowEndpointSendResumeTripWorkflow1Port = tripWorkflowEndpointSendResumeTripWorkflow1PortAdapter;
    }
}
