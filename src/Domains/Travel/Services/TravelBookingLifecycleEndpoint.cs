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
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFeaturestore = Acme.Featurestore.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

/// <summary>
/// Moves a travel booking between the states of its lifecycle.
/// </summary>
public sealed partial class TravelBookingLifecycleEndpoint : ApiTravel.TravelBookingLifecycleService.TravelBookingLifecycleServiceBase
{
    private readonly ApiCorporateevents.VenueApprovalService.VenueApprovalServiceClient _corporateeventsVenueApprovalService;
    private readonly ApiDocuments.DocumentFolderNotificationService.DocumentFolderNotificationServiceClient _documentsDocumentFolderNotificationService;
    private readonly ApiDocuments.DocumentTemplateNotificationService.DocumentTemplateNotificationServiceClient _documentsDocumentTemplateNotificationService;
    private readonly ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient _documentsDocumentTemplateSyncService;
    private readonly ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient _employeesEmployeeQueryService;
    private readonly ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient _employeesEmploymentContractCalculationService;
    private readonly ApiFeaturestore.FeatureViewCalculationService.FeatureViewCalculationServiceClient _featurestoreFeatureViewCalculationService;
    private readonly ApiItsm.ServiceAssetWorkflowService.ServiceAssetWorkflowServiceClient _itsmServiceAssetWorkflowService;
    private readonly ApiLicensing.LicenseAssignmentApprovalService.LicenseAssignmentApprovalServiceClient _licensingLicenseAssignmentApprovalService;
    private readonly ILogger<TravelBookingLifecycleEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiFeaturestore.SimulateFeatureViewRequest, CallOptions, AsyncUnaryCall<ApiFeaturestore.SimulateFeatureViewResponse>> _sendTerminateTravelBooking0;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ITravelBookingLifecycleEndpointSendReinstateTravelBooking0Port _travelBookingLifecycleEndpointSendReinstateTravelBooking0Port;
    private readonly ApiTravel.TravelBookingReportingService.TravelBookingReportingServiceClient _travelTravelBookingReportingService;
    private readonly ApiTravel.TravelBookingValidationService.TravelBookingValidationServiceClient _travelTravelBookingValidationService;
    private readonly ApiTravel.TravelPolicyCalculationService.TravelPolicyCalculationServiceClient _travelTravelPolicyCalculationService;
    private readonly ApiTravel.TravelPolicyReportingService.TravelPolicyReportingServiceClient _travelTravelPolicyReportingService;
    private readonly ApiTravel.TravelPolicySyncService.TravelPolicySyncServiceClient _travelTravelPolicySyncService;
    private readonly ApiTravel.TripValidationService.TripValidationServiceClient _travelTripValidationService;

    public TravelBookingLifecycleEndpoint(
        ApiCorporateevents.VenueApprovalService.VenueApprovalServiceClient corporateeventsVenueApprovalService,
        ApiDocuments.DocumentFolderNotificationService.DocumentFolderNotificationServiceClient documentsDocumentFolderNotificationService,
        ApiDocuments.DocumentTemplateNotificationService.DocumentTemplateNotificationServiceClient documentsDocumentTemplateNotificationService,
        ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient documentsDocumentTemplateSyncService,
        ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient employeesEmployeeQueryService,
        ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient employeesEmploymentContractCalculationService,
        ApiFeaturestore.FeatureViewCalculationService.FeatureViewCalculationServiceClient featurestoreFeatureViewCalculationService,
        ApiItsm.ServiceAssetWorkflowService.ServiceAssetWorkflowServiceClient itsmServiceAssetWorkflowService,
        ApiLicensing.LicenseAssignmentApprovalService.LicenseAssignmentApprovalServiceClient licensingLicenseAssignmentApprovalService,
        ILogger<TravelBookingLifecycleEndpoint> logger,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ITravelBookingLifecycleEndpointSendReinstateTravelBooking0Port travelBookingLifecycleEndpointSendReinstateTravelBooking0PortAdapter,
        ApiTravel.TravelBookingReportingService.TravelBookingReportingServiceClient travelTravelBookingReportingService,
        ApiTravel.TravelBookingValidationService.TravelBookingValidationServiceClient travelTravelBookingValidationService,
        ApiTravel.TravelPolicyCalculationService.TravelPolicyCalculationServiceClient travelTravelPolicyCalculationService,
        ApiTravel.TravelPolicyReportingService.TravelPolicyReportingServiceClient travelTravelPolicyReportingService,
        ApiTravel.TravelPolicySyncService.TravelPolicySyncServiceClient travelTravelPolicySyncService,
        ApiTravel.TripValidationService.TripValidationServiceClient travelTripValidationService)
    {
        _corporateeventsVenueApprovalService = corporateeventsVenueApprovalService;
        _documentsDocumentFolderNotificationService = documentsDocumentFolderNotificationService;
        _documentsDocumentTemplateNotificationService = documentsDocumentTemplateNotificationService;
        _documentsDocumentTemplateSyncService = documentsDocumentTemplateSyncService;
        _employeesEmployeeQueryService = employeesEmployeeQueryService;
        _employeesEmploymentContractCalculationService = employeesEmploymentContractCalculationService;
        _featurestoreFeatureViewCalculationService = featurestoreFeatureViewCalculationService;
        _itsmServiceAssetWorkflowService = itsmServiceAssetWorkflowService;
        _licensingLicenseAssignmentApprovalService = licensingLicenseAssignmentApprovalService;
        _logger = logger;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _settings = settings;
        _travelBookingLifecycleEndpointSendReinstateTravelBooking0Port = travelBookingLifecycleEndpointSendReinstateTravelBooking0PortAdapter;
        _travelTravelBookingReportingService = travelTravelBookingReportingService;
        _travelTravelBookingValidationService = travelTravelBookingValidationService;
        _travelTravelPolicyCalculationService = travelTravelPolicyCalculationService;
        _travelTravelPolicyReportingService = travelTravelPolicyReportingService;
        _travelTravelPolicySyncService = travelTravelPolicySyncService;
        _travelTripValidationService = travelTripValidationService;
        _sendTerminateTravelBooking0 = _featurestoreFeatureViewCalculationService.SimulateFeatureViewAsync;
    }
}
