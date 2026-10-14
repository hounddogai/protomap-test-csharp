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
using ApiBroadcasts = Acme.Broadcasts.V1;
using ApiCart = Acme.Cart.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiExperiments = Acme.Experiments.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

/// <summary>
/// Runs the long-lived workflow that takes a travel policy from start to finish.
/// </summary>
public sealed partial class TravelPolicyWorkflowEndpoint : ApiTravel.TravelPolicyWorkflowService.TravelPolicyWorkflowServiceBase
{
    private readonly ApiBroadcasts.BroadcastService.BroadcastServiceClient _broadcastsBroadcastService;
    private readonly NotificationBus _bus;
    private readonly ApiCart.CartService.CartServiceClient _cartCartService;
    private readonly ApiCorporateevents.VenueWorkflowService.VenueWorkflowServiceClient _corporateeventsVenueWorkflowService;
    private readonly ApiDocuments.DocumentExportService.DocumentExportServiceClient _documentsDocumentExportService;
    private readonly ApiDocuments.DocumentFolderValidationService.DocumentFolderValidationServiceClient _documentsDocumentFolderValidationService;
    private readonly ApiDocuments.SignatureSyncService.SignatureSyncServiceClient _documentsSignatureSyncService;
    private readonly ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient _employeesEmployeeQueryService;
    private readonly ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient _employeesOrgUnitQueryService;
    private readonly ApiExperiments.ExperimentQueryService.ExperimentQueryServiceClient _experimentsExperimentQueryService;
    private readonly ApiHelpdesk.EscalationApprovalService.EscalationApprovalServiceClient _helpdeskEscalationApprovalService;
    private readonly ILogger<TravelPolicyWorkflowEndpoint> _logger;
    private readonly ApiNotifications.NotificationService.NotificationServiceClient _notificationsNotificationService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiTravel.CommentTravelPolicyApprovalRequest, CallOptions, AsyncUnaryCall<ApiTravel.CommentTravelPolicyApprovalResponse>> _sendListTravelPolicyWorkflows0;
    private readonly ApiTravel.ItineraryReportingService.ItineraryReportingServiceClient _travelItineraryReportingService;
    private readonly ApiTravel.TravelBookingReportingService.TravelBookingReportingServiceClient _travelTravelBookingReportingService;
    private readonly ApiTravel.TravelPolicyApprovalService.TravelPolicyApprovalServiceClient _travelTravelPolicyApprovalService;
    private readonly ApiTravel.TravelPolicyLifecycleService.TravelPolicyLifecycleServiceClient _travelTravelPolicyLifecycleService;
    private readonly ApiTravel.TravelPolicyReportingService.TravelPolicyReportingServiceClient _travelTravelPolicyReportingService;

    public TravelPolicyWorkflowEndpoint(
        ApiBroadcasts.BroadcastService.BroadcastServiceClient broadcastsBroadcastService,
        NotificationBus bus,
        ApiCart.CartService.CartServiceClient cartCartService,
        ApiCorporateevents.VenueWorkflowService.VenueWorkflowServiceClient corporateeventsVenueWorkflowService,
        ApiDocuments.DocumentExportService.DocumentExportServiceClient documentsDocumentExportService,
        ApiDocuments.DocumentFolderValidationService.DocumentFolderValidationServiceClient documentsDocumentFolderValidationService,
        ApiDocuments.SignatureSyncService.SignatureSyncServiceClient documentsSignatureSyncService,
        ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient employeesEmployeeQueryService,
        ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient employeesOrgUnitQueryService,
        ApiExperiments.ExperimentQueryService.ExperimentQueryServiceClient experimentsExperimentQueryService,
        ApiHelpdesk.EscalationApprovalService.EscalationApprovalServiceClient helpdeskEscalationApprovalService,
        ILogger<TravelPolicyWorkflowEndpoint> logger,
        ApiNotifications.NotificationService.NotificationServiceClient notificationsNotificationService,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiTravel.ItineraryReportingService.ItineraryReportingServiceClient travelItineraryReportingService,
        ApiTravel.TravelBookingReportingService.TravelBookingReportingServiceClient travelTravelBookingReportingService,
        ApiTravel.TravelPolicyApprovalService.TravelPolicyApprovalServiceClient travelTravelPolicyApprovalService,
        ApiTravel.TravelPolicyLifecycleService.TravelPolicyLifecycleServiceClient travelTravelPolicyLifecycleService,
        ApiTravel.TravelPolicyReportingService.TravelPolicyReportingServiceClient travelTravelPolicyReportingService)
    {
        _broadcastsBroadcastService = broadcastsBroadcastService;
        _bus = bus;
        _cartCartService = cartCartService;
        _corporateeventsVenueWorkflowService = corporateeventsVenueWorkflowService;
        _documentsDocumentExportService = documentsDocumentExportService;
        _documentsDocumentFolderValidationService = documentsDocumentFolderValidationService;
        _documentsSignatureSyncService = documentsSignatureSyncService;
        _employeesEmployeeQueryService = employeesEmployeeQueryService;
        _employeesOrgUnitQueryService = employeesOrgUnitQueryService;
        _experimentsExperimentQueryService = experimentsExperimentQueryService;
        _helpdeskEscalationApprovalService = helpdeskEscalationApprovalService;
        _logger = logger;
        _notificationsNotificationService = notificationsNotificationService;
        _registry = registry;
        _repository = repository;
        _travelItineraryReportingService = travelItineraryReportingService;
        _travelTravelBookingReportingService = travelTravelBookingReportingService;
        _travelTravelPolicyApprovalService = travelTravelPolicyApprovalService;
        _travelTravelPolicyLifecycleService = travelTravelPolicyLifecycleService;
        _travelTravelPolicyReportingService = travelTravelPolicyReportingService;
        _sendListTravelPolicyWorkflows0 = _travelTravelPolicyApprovalService.CommentTravelPolicyApprovalAsync;
    }
}
