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
using ApiCampaigns = Acme.Campaigns.V1;
using ApiCommon = Acme.Common.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLearning = Acme.Learning.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Learning;

/// <summary>
/// Checks certifications against the rules before anything acts on them.
/// </summary>
public sealed partial class CertificationValidationEndpoint : ApiLearning.CertificationValidationService.CertificationValidationServiceBase
{
    private readonly ApiCampaigns.MarketingCampaignService.MarketingCampaignServiceClient _campaignsMarketingCampaignService;
    private readonly ApiFeatureflags.FeatureFlagValidationService.FeatureFlagValidationServiceClient _featureflagsFeatureFlagValidationService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.IncidentService.IncidentServiceClient _itsmIncidentService;
    private readonly ApiLearning.CertificationCalculationService.CertificationCalculationServiceClient _learningCertificationCalculationService;
    private readonly ApiLearning.CourseValidationService.CourseValidationServiceClient _learningCourseValidationService;
    private readonly ApiLearning.LearningPathCalculationService.LearningPathCalculationServiceClient _learningLearningPathCalculationService;
    private readonly ApiLearning.LearningPathValidationService.LearningPathValidationServiceClient _learningLearningPathValidationService;
    private readonly ApiLicensing.SoftwareLicenseCalculationService.SoftwareLicenseCalculationServiceClient _licensingSoftwareLicenseCalculationService;
    private readonly ILogger<CertificationValidationEndpoint> _logger;
    private readonly ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient _metricsMetricSeriesQueryService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiCampaigns.UpdateMarketingCampaignRequest, CallOptions, AsyncUnaryCall<ApiCampaigns.UpdateMarketingCampaignResponse>> _sendLintCertifications0;
    private readonly ApiTravel.TravelBookingCalculationService.TravelBookingCalculationServiceClient _travelTravelBookingCalculationService;

    public CertificationValidationEndpoint(
        ApiCampaigns.MarketingCampaignService.MarketingCampaignServiceClient campaignsMarketingCampaignService,
        ApiFeatureflags.FeatureFlagValidationService.FeatureFlagValidationServiceClient featureflagsFeatureFlagValidationService,
        CallInvoker invoker,
        ApiItsm.IncidentService.IncidentServiceClient itsmIncidentService,
        ApiLearning.CertificationCalculationService.CertificationCalculationServiceClient learningCertificationCalculationService,
        ApiLearning.CourseValidationService.CourseValidationServiceClient learningCourseValidationService,
        ApiLearning.LearningPathCalculationService.LearningPathCalculationServiceClient learningLearningPathCalculationService,
        ApiLearning.LearningPathValidationService.LearningPathValidationServiceClient learningLearningPathValidationService,
        ApiLicensing.SoftwareLicenseCalculationService.SoftwareLicenseCalculationServiceClient licensingSoftwareLicenseCalculationService,
        ILogger<CertificationValidationEndpoint> logger,
        ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient metricsMetricSeriesQueryService,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiTravel.TravelBookingCalculationService.TravelBookingCalculationServiceClient travelTravelBookingCalculationService)
    {
        _campaignsMarketingCampaignService = campaignsMarketingCampaignService;
        _featureflagsFeatureFlagValidationService = featureflagsFeatureFlagValidationService;
        _invoker = invoker;
        _itsmIncidentService = itsmIncidentService;
        _learningCertificationCalculationService = learningCertificationCalculationService;
        _learningCourseValidationService = learningCourseValidationService;
        _learningLearningPathCalculationService = learningLearningPathCalculationService;
        _learningLearningPathValidationService = learningLearningPathValidationService;
        _licensingSoftwareLicenseCalculationService = licensingSoftwareLicenseCalculationService;
        _logger = logger;
        _metricsMetricSeriesQueryService = metricsMetricSeriesQueryService;
        _registry = registry;
        _repository = repository;
        _travelTravelBookingCalculationService = travelTravelBookingCalculationService;
        _sendLintCertifications0 = _campaignsMarketingCampaignService.UpdateMarketingCampaignAsync;
    }
}
