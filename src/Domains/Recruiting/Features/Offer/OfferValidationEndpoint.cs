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
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiMedia = Acme.Media.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Recruiting;

/// <summary>
/// Checks offers against the rules before anything acts on them.
/// </summary>
public sealed partial class OfferValidationEndpoint : ApiRecruiting.OfferValidationService.OfferValidationServiceBase
{
    private readonly ApiAuthorization.AccessPolicyQueryService.AccessPolicyQueryServiceClient _authorizationAccessPolicyQueryService;
    private readonly ApiCorporateevents.RegistrationValidationService.RegistrationValidationServiceClient _corporateeventsRegistrationValidationService;
    private readonly ApiDocuments.DocumentNotificationService.DocumentNotificationServiceClient _documentsDocumentNotificationService;
    private readonly ApiDocuments.DocumentValidationService.DocumentValidationServiceClient _documentsDocumentValidationService;
    private readonly ApiFeatureflags.FeatureFlagService.FeatureFlagServiceClient _featureflagsFeatureFlagService;
    private readonly ApiLearning.LearningPathCalculationService.LearningPathCalculationServiceClient _learningLearningPathCalculationService;
    private readonly ILogger<OfferValidationEndpoint> _logger;
    private readonly ApiMedia.MediaAssetQueryService.MediaAssetQueryServiceClient _mediaMediaAssetQueryService;
    private readonly IOfferValidationEndpointSendNormalizeOffer1Port _offerValidationEndpointSendNormalizeOffer1Port;
    private readonly RetryPolicy _policy;
    private readonly ApiRecruiting.CandidateCalculationService.CandidateCalculationServiceClient _recruitingCandidateCalculationService;
    private readonly ApiRecruiting.InterviewValidationService.InterviewValidationServiceClient _recruitingInterviewValidationService;
    private readonly ApiRecruiting.OfferCalculationService.OfferCalculationServiceClient _recruitingOfferCalculationService;
    private readonly IRecordRepository _repository;
    private readonly IOptions<MeshOptions> _settings;

    public OfferValidationEndpoint(
        ApiAuthorization.AccessPolicyQueryService.AccessPolicyQueryServiceClient authorizationAccessPolicyQueryService,
        ApiCorporateevents.RegistrationValidationService.RegistrationValidationServiceClient corporateeventsRegistrationValidationService,
        ApiDocuments.DocumentNotificationService.DocumentNotificationServiceClient documentsDocumentNotificationService,
        ApiDocuments.DocumentValidationService.DocumentValidationServiceClient documentsDocumentValidationService,
        ApiFeatureflags.FeatureFlagService.FeatureFlagServiceClient featureflagsFeatureFlagService,
        ApiLearning.LearningPathCalculationService.LearningPathCalculationServiceClient learningLearningPathCalculationService,
        ILogger<OfferValidationEndpoint> logger,
        ApiMedia.MediaAssetQueryService.MediaAssetQueryServiceClient mediaMediaAssetQueryService,
        IOfferValidationEndpointSendNormalizeOffer1Port offerValidationEndpointSendNormalizeOffer1PortAdapter,
        RetryPolicy policy,
        ApiRecruiting.CandidateCalculationService.CandidateCalculationServiceClient recruitingCandidateCalculationService,
        ApiRecruiting.InterviewValidationService.InterviewValidationServiceClient recruitingInterviewValidationService,
        ApiRecruiting.OfferCalculationService.OfferCalculationServiceClient recruitingOfferCalculationService,
        IRecordRepository repository,
        IOptions<MeshOptions> settings)
    {
        _authorizationAccessPolicyQueryService = authorizationAccessPolicyQueryService;
        _corporateeventsRegistrationValidationService = corporateeventsRegistrationValidationService;
        _documentsDocumentNotificationService = documentsDocumentNotificationService;
        _documentsDocumentValidationService = documentsDocumentValidationService;
        _featureflagsFeatureFlagService = featureflagsFeatureFlagService;
        _learningLearningPathCalculationService = learningLearningPathCalculationService;
        _logger = logger;
        _mediaMediaAssetQueryService = mediaMediaAssetQueryService;
        _offerValidationEndpointSendNormalizeOffer1Port = offerValidationEndpointSendNormalizeOffer1PortAdapter;
        _policy = policy;
        _recruitingCandidateCalculationService = recruitingCandidateCalculationService;
        _recruitingInterviewValidationService = recruitingInterviewValidationService;
        _recruitingOfferCalculationService = recruitingOfferCalculationService;
        _repository = repository;
        _settings = settings;
    }
}
