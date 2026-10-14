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
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

/// <summary>
/// Checks trips against the rules before anything acts on them.
/// </summary>
public sealed partial class TripValidationEndpoint : ApiTravel.TripValidationService.TripValidationServiceBase
{
    private readonly ApiCorporateevents.CorporateEventQueryService.CorporateEventQueryServiceClient _corporateeventsCorporateEventQueryService;
    private readonly ApiDevices.ManagedDeviceCalculationService.ManagedDeviceCalculationServiceClient _devicesManagedDeviceCalculationService;
    private readonly ApiDocuments.DocumentFolderSyncService.DocumentFolderSyncServiceClient _documentsDocumentFolderSyncService;
    private readonly ApiDocuments.SignatureNotificationService.SignatureNotificationServiceClient _documentsSignatureNotificationService;
    private readonly ApiItsm.ProblemCalculationService.ProblemCalculationServiceClient _itsmProblemCalculationService;
    private readonly ApiItsm.ProblemValidationService.ProblemValidationServiceClient _itsmProblemValidationService;
    private readonly ILogger<TripValidationEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiTravel.RecalculateTripsRequest, CallOptions, AsyncUnaryCall<ApiTravel.RecalculateTripsResponse>> _sendValidateTrip0;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTravel.TripCalculationService.TripCalculationServiceClient _travelTripCalculationService;
    private readonly ITripValidationEndpointSendNormalizeTrip0Port _tripValidationEndpointSendNormalizeTrip0Port;

    public TripValidationEndpoint(
        ApiCorporateevents.CorporateEventQueryService.CorporateEventQueryServiceClient corporateeventsCorporateEventQueryService,
        ApiDevices.ManagedDeviceCalculationService.ManagedDeviceCalculationServiceClient devicesManagedDeviceCalculationService,
        ApiDocuments.DocumentFolderSyncService.DocumentFolderSyncServiceClient documentsDocumentFolderSyncService,
        ApiDocuments.SignatureNotificationService.SignatureNotificationServiceClient documentsSignatureNotificationService,
        ApiItsm.ProblemCalculationService.ProblemCalculationServiceClient itsmProblemCalculationService,
        ApiItsm.ProblemValidationService.ProblemValidationServiceClient itsmProblemValidationService,
        ILogger<TripValidationEndpoint> logger,
        RetryPolicy policy,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiTravel.TripCalculationService.TripCalculationServiceClient travelTripCalculationService,
        ITripValidationEndpointSendNormalizeTrip0Port tripValidationEndpointSendNormalizeTrip0PortAdapter)
    {
        _corporateeventsCorporateEventQueryService = corporateeventsCorporateEventQueryService;
        _devicesManagedDeviceCalculationService = devicesManagedDeviceCalculationService;
        _documentsDocumentFolderSyncService = documentsDocumentFolderSyncService;
        _documentsSignatureNotificationService = documentsSignatureNotificationService;
        _itsmProblemCalculationService = itsmProblemCalculationService;
        _itsmProblemValidationService = itsmProblemValidationService;
        _logger = logger;
        _policy = policy;
        _repository = repository;
        _settings = settings;
        _travelTripCalculationService = travelTripCalculationService;
        _tripValidationEndpointSendNormalizeTrip0Port = tripValidationEndpointSendNormalizeTrip0PortAdapter;
        _sendValidateTrip0 = _travelTripCalculationService.RecalculateTripsAsync;
    }
}
