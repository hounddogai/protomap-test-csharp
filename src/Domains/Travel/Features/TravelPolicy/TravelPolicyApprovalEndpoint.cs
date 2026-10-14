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
using ApiBoxoffice = Acme.Boxoffice.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiRatelimiting = Acme.Ratelimiting.V1;
using ApiSearch = Acme.Search.V1;
using ApiTravel = Acme.Travel.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Travel;

/// <summary>
/// Routes travel policies through approval, with escalation and delegation.
/// </summary>
public sealed partial class TravelPolicyApprovalEndpoint : ApiTravel.TravelPolicyApprovalService.TravelPolicyApprovalServiceBase
{
    private readonly ApiBoxoffice.BoxOfficeSaleQueryService.BoxOfficeSaleQueryServiceClient _boxofficeBoxOfficeSaleQueryService;
    private readonly ApiCorporateevents.VenueWorkflowService.VenueWorkflowServiceClient _corporateeventsVenueWorkflowService;
    private readonly ApiDocuments.DocumentFolderHistoryService.DocumentFolderHistoryServiceClient _documentsDocumentFolderHistoryService;
    private readonly ApiDocuments.DocumentNotificationService.DocumentNotificationServiceClient _documentsDocumentNotificationService;
    private readonly ApiEmployees.EmployeeService.EmployeeServiceClient _employeesEmployeeService;
    private readonly ApiEmployees.PositionValidationService.PositionValidationServiceClient _employeesPositionValidationService;
    private readonly ApiHelpdesk.SlaPolicyLifecycleService.SlaPolicyLifecycleServiceClient _helpdeskSlaPolicyLifecycleService;
    private readonly ApiIdentity.UserQueryService.UserQueryServiceClient _identityUserQueryService;
    private readonly ApiIdentity.UserService.UserServiceClient _identityUserService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.ProblemLifecycleService.ProblemLifecycleServiceClient _itsmProblemLifecycleService;
    private readonly ILogger<TravelPolicyApprovalEndpoint> _logger;
    private readonly ApiRatelimiting.RateLimitCalculationService.RateLimitCalculationServiceClient _ratelimitingRateLimitCalculationService;
    private readonly IRecordRepository _repository;
    private readonly ApiSearch.SearchIndexValidationService.SearchIndexValidationServiceClient _searchSearchIndexValidationService;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTravel.ItinerarySyncService.ItinerarySyncServiceClient _travelItinerarySyncService;
    private readonly ApiTravel.TravelBookingApprovalService.TravelBookingApprovalServiceClient _travelTravelBookingApprovalService;
    private readonly ApiTravel.TravelPolicyLifecycleService.TravelPolicyLifecycleServiceClient _travelTravelPolicyLifecycleService;
    private readonly ApiTravel.TravelPolicyReportingService.TravelPolicyReportingServiceClient _travelTravelPolicyReportingService;
    private readonly ApiTravel.TripApprovalService.TripApprovalServiceClient _travelTripApprovalService;
    private readonly ApiTravel.TripQueryService.TripQueryServiceClient _travelTripQueryService;
    private readonly ApiTravel.TripWorkflowService.TripWorkflowServiceClient _travelTripWorkflowService;
    private readonly ApiVenues.VenueQueryService.VenueQueryServiceClient _venuesVenueQueryService;

    public TravelPolicyApprovalEndpoint(
        ApiBoxoffice.BoxOfficeSaleQueryService.BoxOfficeSaleQueryServiceClient boxofficeBoxOfficeSaleQueryService,
        ApiCorporateevents.VenueWorkflowService.VenueWorkflowServiceClient corporateeventsVenueWorkflowService,
        ApiDocuments.DocumentFolderHistoryService.DocumentFolderHistoryServiceClient documentsDocumentFolderHistoryService,
        ApiDocuments.DocumentNotificationService.DocumentNotificationServiceClient documentsDocumentNotificationService,
        ApiEmployees.EmployeeService.EmployeeServiceClient employeesEmployeeService,
        ApiEmployees.PositionValidationService.PositionValidationServiceClient employeesPositionValidationService,
        ApiHelpdesk.SlaPolicyLifecycleService.SlaPolicyLifecycleServiceClient helpdeskSlaPolicyLifecycleService,
        ApiIdentity.UserQueryService.UserQueryServiceClient identityUserQueryService,
        ApiIdentity.UserService.UserServiceClient identityUserService,
        CallInvoker invoker,
        ApiItsm.ProblemLifecycleService.ProblemLifecycleServiceClient itsmProblemLifecycleService,
        ILogger<TravelPolicyApprovalEndpoint> logger,
        ApiRatelimiting.RateLimitCalculationService.RateLimitCalculationServiceClient ratelimitingRateLimitCalculationService,
        IRecordRepository repository,
        ApiSearch.SearchIndexValidationService.SearchIndexValidationServiceClient searchSearchIndexValidationService,
        IOptions<MeshOptions> settings,
        ApiTravel.ItinerarySyncService.ItinerarySyncServiceClient travelItinerarySyncService,
        ApiTravel.TravelBookingApprovalService.TravelBookingApprovalServiceClient travelTravelBookingApprovalService,
        ApiTravel.TravelPolicyLifecycleService.TravelPolicyLifecycleServiceClient travelTravelPolicyLifecycleService,
        ApiTravel.TravelPolicyReportingService.TravelPolicyReportingServiceClient travelTravelPolicyReportingService,
        ApiTravel.TripApprovalService.TripApprovalServiceClient travelTripApprovalService,
        ApiTravel.TripQueryService.TripQueryServiceClient travelTripQueryService,
        ApiTravel.TripWorkflowService.TripWorkflowServiceClient travelTripWorkflowService,
        ApiVenues.VenueQueryService.VenueQueryServiceClient venuesVenueQueryService)
    {
        _boxofficeBoxOfficeSaleQueryService = boxofficeBoxOfficeSaleQueryService;
        _corporateeventsVenueWorkflowService = corporateeventsVenueWorkflowService;
        _documentsDocumentFolderHistoryService = documentsDocumentFolderHistoryService;
        _documentsDocumentNotificationService = documentsDocumentNotificationService;
        _employeesEmployeeService = employeesEmployeeService;
        _employeesPositionValidationService = employeesPositionValidationService;
        _helpdeskSlaPolicyLifecycleService = helpdeskSlaPolicyLifecycleService;
        _identityUserQueryService = identityUserQueryService;
        _identityUserService = identityUserService;
        _invoker = invoker;
        _itsmProblemLifecycleService = itsmProblemLifecycleService;
        _logger = logger;
        _ratelimitingRateLimitCalculationService = ratelimitingRateLimitCalculationService;
        _repository = repository;
        _searchSearchIndexValidationService = searchSearchIndexValidationService;
        _settings = settings;
        _travelItinerarySyncService = travelItinerarySyncService;
        _travelTravelBookingApprovalService = travelTravelBookingApprovalService;
        _travelTravelPolicyLifecycleService = travelTravelPolicyLifecycleService;
        _travelTravelPolicyReportingService = travelTravelPolicyReportingService;
        _travelTripApprovalService = travelTripApprovalService;
        _travelTripQueryService = travelTripQueryService;
        _travelTripWorkflowService = travelTripWorkflowService;
        _venuesVenueQueryService = venuesVenueQueryService;
    }
}
