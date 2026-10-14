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
using ApiCareplans = Acme.Careplans.V1;
using ApiCommon = Acme.Common.V1;
using ApiCustomers = Acme.Customers.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiReturns = Acme.Returns.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

/// <summary>
/// Keeps travel policies in step with the systems of record in other regions.
/// </summary>
public sealed partial class TravelPolicySyncEndpoint : ApiTravel.TravelPolicySyncService.TravelPolicySyncServiceBase
{
    private readonly ApiCareplans.CarePlanCalculationService.CarePlanCalculationServiceClient _careplansCarePlanCalculationService;
    private readonly ApiDocuments.SignatureHistoryService.SignatureHistoryServiceClient _documentsSignatureHistoryService;
    private readonly ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient _employeesEmploymentContractCalculationService;
    private readonly ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient _employeesOrgUnitQueryService;
    private readonly ApiHelpdesk.SlaPolicyWorkflowService.SlaPolicyWorkflowServiceClient _helpdeskSlaPolicyWorkflowService;
    private readonly ApiIdentity.UserValidationService.UserValidationServiceClient _identityUserValidationService;
    private readonly ApiLicensing.LicenseAssignmentReportingService.LicenseAssignmentReportingServiceClient _licensingLicenseAssignmentReportingService;
    private readonly ILogger<TravelPolicySyncEndpoint> _logger;
    private readonly ApiNotifications.NotificationValidationService.NotificationValidationServiceClient _notificationsNotificationValidationService;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiReturns.ReturnRequestCalculationService.ReturnRequestCalculationServiceClient _returnsReturnRequestCalculationService;
    private readonly Func<ApiCareplans.CompareCarePlanOptionsRequest, CallOptions, AsyncUnaryCall<ApiCareplans.CompareCarePlanOptionsResponse>> _sendListTravelPolicyConflicts1;
    private readonly ApiTravel.ItineraryCalculationService.ItineraryCalculationServiceClient _travelItineraryCalculationService;
    private readonly ITravelPolicySyncEndpointSendReconcileTravelPolicies0Port _travelPolicySyncEndpointSendReconcileTravelPolicies0Port;
    private readonly ApiTravel.TravelBookingQueryService.TravelBookingQueryServiceClient _travelTravelBookingQueryService;
    private readonly ApiTravel.TripCalculationService.TripCalculationServiceClient _travelTripCalculationService;
    private readonly ApiTravel.TripValidationService.TripValidationServiceClient _travelTripValidationService;

    public TravelPolicySyncEndpoint(
        ApiCareplans.CarePlanCalculationService.CarePlanCalculationServiceClient careplansCarePlanCalculationService,
        ApiDocuments.SignatureHistoryService.SignatureHistoryServiceClient documentsSignatureHistoryService,
        ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient employeesEmploymentContractCalculationService,
        ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient employeesOrgUnitQueryService,
        ApiHelpdesk.SlaPolicyWorkflowService.SlaPolicyWorkflowServiceClient helpdeskSlaPolicyWorkflowService,
        ApiIdentity.UserValidationService.UserValidationServiceClient identityUserValidationService,
        ApiLicensing.LicenseAssignmentReportingService.LicenseAssignmentReportingServiceClient licensingLicenseAssignmentReportingService,
        ILogger<TravelPolicySyncEndpoint> logger,
        ApiNotifications.NotificationValidationService.NotificationValidationServiceClient notificationsNotificationValidationService,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiReturns.ReturnRequestCalculationService.ReturnRequestCalculationServiceClient returnsReturnRequestCalculationService,
        ApiTravel.ItineraryCalculationService.ItineraryCalculationServiceClient travelItineraryCalculationService,
        ITravelPolicySyncEndpointSendReconcileTravelPolicies0Port travelPolicySyncEndpointSendReconcileTravelPolicies0PortAdapter,
        ApiTravel.TravelBookingQueryService.TravelBookingQueryServiceClient travelTravelBookingQueryService,
        ApiTravel.TripCalculationService.TripCalculationServiceClient travelTripCalculationService,
        ApiTravel.TripValidationService.TripValidationServiceClient travelTripValidationService)
    {
        _careplansCarePlanCalculationService = careplansCarePlanCalculationService;
        _documentsSignatureHistoryService = documentsSignatureHistoryService;
        _employeesEmploymentContractCalculationService = employeesEmploymentContractCalculationService;
        _employeesOrgUnitQueryService = employeesOrgUnitQueryService;
        _helpdeskSlaPolicyWorkflowService = helpdeskSlaPolicyWorkflowService;
        _identityUserValidationService = identityUserValidationService;
        _licensingLicenseAssignmentReportingService = licensingLicenseAssignmentReportingService;
        _logger = logger;
        _notificationsNotificationValidationService = notificationsNotificationValidationService;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _returnsReturnRequestCalculationService = returnsReturnRequestCalculationService;
        _travelItineraryCalculationService = travelItineraryCalculationService;
        _travelPolicySyncEndpointSendReconcileTravelPolicies0Port = travelPolicySyncEndpointSendReconcileTravelPolicies0PortAdapter;
        _travelTravelBookingQueryService = travelTravelBookingQueryService;
        _travelTripCalculationService = travelTripCalculationService;
        _travelTripValidationService = travelTripValidationService;
        _sendListTravelPolicyConflicts1 = _careplansCarePlanCalculationService.CompareCarePlanOptionsAsync;
    }
}
