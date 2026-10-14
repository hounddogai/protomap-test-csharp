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
using ApiDocuments = Acme.Documents.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTravel = Acme.Travel.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Performance;

/// <summary>
/// Computes the figures of a calibration, such as totals, taxes, and estimates.
/// </summary>
public sealed partial class CalibrationCalculationEndpoint : ApiPerformance.CalibrationCalculationService.CalibrationCalculationServiceBase
{
    private readonly ApiDocuments.DocumentFolderValidationService.DocumentFolderValidationServiceClient _documentsDocumentFolderValidationService;
    private readonly ApiDocuments.DocumentTemplateHistoryService.DocumentTemplateHistoryServiceClient _documentsDocumentTemplateHistoryService;
    private readonly ApiFeatureflags.FeatureFlagQueryService.FeatureFlagQueryServiceClient _featureflagsFeatureFlagQueryService;
    private readonly ApiIdentity.UserService.UserServiceClient _identityUserService;
    private readonly CallInvoker _invoker;
    private readonly ILogger<CalibrationCalculationEndpoint> _logger;
    private readonly ApiMetrics.MetricSeriesValidationService.MetricSeriesValidationServiceClient _metricsMetricSeriesValidationService;
    private readonly ApiPerformance.FeedbackCalculationService.FeedbackCalculationServiceClient _performanceFeedbackCalculationService;
    private readonly ApiPerformance.GoalValidationService.GoalValidationServiceClient _performanceGoalValidationService;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiPerformance.PreviewGoalRequest, CallOptions, AsyncUnaryCall<ApiPerformance.PreviewGoalResponse>> _sendGetCalibrationRates1;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTravel.ItineraryValidationService.ItineraryValidationServiceClient _travelItineraryValidationService;
    private readonly ApiVenues.VenueService.VenueServiceClient _venuesVenueService;

    public CalibrationCalculationEndpoint(
        ApiDocuments.DocumentFolderValidationService.DocumentFolderValidationServiceClient documentsDocumentFolderValidationService,
        ApiDocuments.DocumentTemplateHistoryService.DocumentTemplateHistoryServiceClient documentsDocumentTemplateHistoryService,
        ApiFeatureflags.FeatureFlagQueryService.FeatureFlagQueryServiceClient featureflagsFeatureFlagQueryService,
        ApiIdentity.UserService.UserServiceClient identityUserService,
        CallInvoker invoker,
        ILogger<CalibrationCalculationEndpoint> logger,
        ApiMetrics.MetricSeriesValidationService.MetricSeriesValidationServiceClient metricsMetricSeriesValidationService,
        ApiPerformance.FeedbackCalculationService.FeedbackCalculationServiceClient performanceFeedbackCalculationService,
        ApiPerformance.GoalValidationService.GoalValidationServiceClient performanceGoalValidationService,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiTravel.ItineraryValidationService.ItineraryValidationServiceClient travelItineraryValidationService,
        ApiVenues.VenueService.VenueServiceClient venuesVenueService)
    {
        _documentsDocumentFolderValidationService = documentsDocumentFolderValidationService;
        _documentsDocumentTemplateHistoryService = documentsDocumentTemplateHistoryService;
        _featureflagsFeatureFlagQueryService = featureflagsFeatureFlagQueryService;
        _identityUserService = identityUserService;
        _invoker = invoker;
        _logger = logger;
        _metricsMetricSeriesValidationService = metricsMetricSeriesValidationService;
        _performanceFeedbackCalculationService = performanceFeedbackCalculationService;
        _performanceGoalValidationService = performanceGoalValidationService;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _settings = settings;
        _travelItineraryValidationService = travelItineraryValidationService;
        _venuesVenueService = venuesVenueService;
        _sendGetCalibrationRates1 = _performanceGoalValidationService.PreviewGoalAsync;
    }
}
