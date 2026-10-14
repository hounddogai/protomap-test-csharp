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
using ApiAuthorization = Acme.Authorization.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

/// <summary>
/// Checks itineraries against the rules before anything acts on them.
/// </summary>
public sealed partial class ItineraryValidationEndpoint : ApiTravel.ItineraryValidationService.ItineraryValidationServiceBase
{
    private readonly ApiAuthorization.AccessPolicyValidationService.AccessPolicyValidationServiceClient _authorizationAccessPolicyValidationService;
    private readonly ApiDocuments.DocumentFolderExportService.DocumentFolderExportServiceClient _documentsDocumentFolderExportService;
    private readonly ApiDocuments.DocumentHistoryService.DocumentHistoryServiceClient _documentsDocumentHistoryService;
    private readonly ApiDocuments.DocumentService.DocumentServiceClient _documentsDocumentService;
    private readonly ApiIdentity.UserValidationService.UserValidationServiceClient _identityUserValidationService;
    private readonly ApiItsm.ServiceAssetCalculationService.ServiceAssetCalculationServiceClient _itsmServiceAssetCalculationService;
    private readonly ILogger<ItineraryValidationEndpoint> _logger;
    private readonly IRecordRepository _repository;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTravel.TravelBookingCalculationService.TravelBookingCalculationServiceClient _travelTravelBookingCalculationService;
    private readonly ApiTravel.TravelBookingValidationService.TravelBookingValidationServiceClient _travelTravelBookingValidationService;
    private readonly ApiTravel.TravelPolicyValidationService.TravelPolicyValidationServiceClient _travelTravelPolicyValidationService;
    private readonly ApiTravel.TripCalculationService.TripCalculationServiceClient _travelTripCalculationService;

    public ItineraryValidationEndpoint(
        ApiAuthorization.AccessPolicyValidationService.AccessPolicyValidationServiceClient authorizationAccessPolicyValidationService,
        ApiDocuments.DocumentFolderExportService.DocumentFolderExportServiceClient documentsDocumentFolderExportService,
        ApiDocuments.DocumentHistoryService.DocumentHistoryServiceClient documentsDocumentHistoryService,
        ApiDocuments.DocumentService.DocumentServiceClient documentsDocumentService,
        ApiIdentity.UserValidationService.UserValidationServiceClient identityUserValidationService,
        ApiItsm.ServiceAssetCalculationService.ServiceAssetCalculationServiceClient itsmServiceAssetCalculationService,
        ILogger<ItineraryValidationEndpoint> logger,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiTravel.TravelBookingCalculationService.TravelBookingCalculationServiceClient travelTravelBookingCalculationService,
        ApiTravel.TravelBookingValidationService.TravelBookingValidationServiceClient travelTravelBookingValidationService,
        ApiTravel.TravelPolicyValidationService.TravelPolicyValidationServiceClient travelTravelPolicyValidationService,
        ApiTravel.TripCalculationService.TripCalculationServiceClient travelTripCalculationService)
    {
        _authorizationAccessPolicyValidationService = authorizationAccessPolicyValidationService;
        _documentsDocumentFolderExportService = documentsDocumentFolderExportService;
        _documentsDocumentHistoryService = documentsDocumentHistoryService;
        _documentsDocumentService = documentsDocumentService;
        _identityUserValidationService = identityUserValidationService;
        _itsmServiceAssetCalculationService = itsmServiceAssetCalculationService;
        _logger = logger;
        _repository = repository;
        _settings = settings;
        _travelTravelBookingCalculationService = travelTravelBookingCalculationService;
        _travelTravelBookingValidationService = travelTravelBookingValidationService;
        _travelTravelPolicyValidationService = travelTravelPolicyValidationService;
        _travelTripCalculationService = travelTripCalculationService;
    }
}
