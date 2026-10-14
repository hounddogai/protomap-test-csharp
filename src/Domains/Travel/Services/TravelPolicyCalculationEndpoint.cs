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
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiFeaturestore = Acme.Featurestore.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

/// <summary>
/// Computes the figures of a travel policy, such as totals, taxes, and estimates.
/// </summary>
public sealed partial class TravelPolicyCalculationEndpoint : ApiTravel.TravelPolicyCalculationService.TravelPolicyCalculationServiceBase
{
    private readonly ApiCorporateevents.RegistrationQueryService.RegistrationQueryServiceClient _corporateeventsRegistrationQueryService;
    private readonly ApiDocuments.DocumentHistoryService.DocumentHistoryServiceClient _documentsDocumentHistoryService;
    private readonly ApiFeatureflags.FeatureFlagQueryService.FeatureFlagQueryServiceClient _featureflagsFeatureFlagQueryService;
    private readonly ApiFeaturestore.FeatureViewQueryService.FeatureViewQueryServiceClient _featurestoreFeatureViewQueryService;
    private readonly ApiItsm.IncidentService.IncidentServiceClient _itsmIncidentService;
    private readonly ApiItsm.ProblemValidationService.ProblemValidationServiceClient _itsmProblemValidationService;
    private readonly ILogger<TravelPolicyCalculationEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiCorporateevents.BrowseRegistrationsRequest, CallOptions, AsyncUnaryCall<ApiCorporateevents.BrowseRegistrationsResponse>> _sendSimulateTravelPolicy0;
    private readonly ApiTravel.TravelBookingCalculationService.TravelBookingCalculationServiceClient _travelTravelBookingCalculationService;
    private readonly ApiTravel.TripCalculationService.TripCalculationServiceClient _travelTripCalculationService;

    public TravelPolicyCalculationEndpoint(
        ApiCorporateevents.RegistrationQueryService.RegistrationQueryServiceClient corporateeventsRegistrationQueryService,
        ApiDocuments.DocumentHistoryService.DocumentHistoryServiceClient documentsDocumentHistoryService,
        ApiFeatureflags.FeatureFlagQueryService.FeatureFlagQueryServiceClient featureflagsFeatureFlagQueryService,
        ApiFeaturestore.FeatureViewQueryService.FeatureViewQueryServiceClient featurestoreFeatureViewQueryService,
        ApiItsm.IncidentService.IncidentServiceClient itsmIncidentService,
        ApiItsm.ProblemValidationService.ProblemValidationServiceClient itsmProblemValidationService,
        ILogger<TravelPolicyCalculationEndpoint> logger,
        RetryPolicy policy,
        IRecordRepository repository,
        ApiTravel.TravelBookingCalculationService.TravelBookingCalculationServiceClient travelTravelBookingCalculationService,
        ApiTravel.TripCalculationService.TripCalculationServiceClient travelTripCalculationService)
    {
        _corporateeventsRegistrationQueryService = corporateeventsRegistrationQueryService;
        _documentsDocumentHistoryService = documentsDocumentHistoryService;
        _featureflagsFeatureFlagQueryService = featureflagsFeatureFlagQueryService;
        _featurestoreFeatureViewQueryService = featurestoreFeatureViewQueryService;
        _itsmIncidentService = itsmIncidentService;
        _itsmProblemValidationService = itsmProblemValidationService;
        _logger = logger;
        _policy = policy;
        _repository = repository;
        _travelTravelBookingCalculationService = travelTravelBookingCalculationService;
        _travelTripCalculationService = travelTripCalculationService;
        _sendSimulateTravelPolicy0 = _corporateeventsRegistrationQueryService.BrowseRegistrationsAsync;
    }
}
