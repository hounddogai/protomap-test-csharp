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
using ApiIdentity = Acme.Identity.V1;
using ApiMedia = Acme.Media.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiRatelimiting = Acme.Ratelimiting.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

/// <summary>
/// Answers read-only questions about desks, for screens and reports.
/// </summary>
public sealed partial class DeskQueryEndpoint : ApiWorkplaces.DeskQueryService.DeskQueryServiceBase
{
    private readonly ApiDocuments.DocumentTemplateCalculationService.DocumentTemplateCalculationServiceClient _documentsDocumentTemplateCalculationService;
    private readonly ApiDocuments.DocumentValidationService.DocumentValidationServiceClient _documentsDocumentValidationService;
    private readonly ApiIdentity.UserService.UserServiceClient _identityUserService;
    private readonly ApiIdentity.UserValidationService.UserValidationServiceClient _identityUserValidationService;
    private readonly CallInvoker _invoker;
    private readonly ILogger<DeskQueryEndpoint> _logger;
    private readonly ApiMedia.MediaAssetService.MediaAssetServiceClient _mediaMediaAssetService;
    private readonly ApiMetrics.MetricSeriesValidationService.MetricSeriesValidationServiceClient _metricsMetricSeriesValidationService;
    private readonly RetryPolicy _policy;
    private readonly ApiRatelimiting.RateLimitQueryService.RateLimitQueryServiceClient _ratelimitingRateLimitQueryService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiMedia.UpdateMediaAssetRequest, CallOptions, AsyncUnaryCall<ApiMedia.UpdateMediaAssetResponse>> _sendResolveDesks0;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTravel.TripQueryService.TripQueryServiceClient _travelTripQueryService;
    private readonly ApiWorkplaces.BuildingCalculationService.BuildingCalculationServiceClient _workplacesBuildingCalculationService;
    private readonly ApiWorkplaces.BuildingValidationService.BuildingValidationServiceClient _workplacesBuildingValidationService;
    private readonly ApiWorkplaces.VisitorPassValidationService.VisitorPassValidationServiceClient _workplacesVisitorPassValidationService;

    public DeskQueryEndpoint(
        ApiDocuments.DocumentTemplateCalculationService.DocumentTemplateCalculationServiceClient documentsDocumentTemplateCalculationService,
        ApiDocuments.DocumentValidationService.DocumentValidationServiceClient documentsDocumentValidationService,
        ApiIdentity.UserService.UserServiceClient identityUserService,
        ApiIdentity.UserValidationService.UserValidationServiceClient identityUserValidationService,
        CallInvoker invoker,
        ILogger<DeskQueryEndpoint> logger,
        ApiMedia.MediaAssetService.MediaAssetServiceClient mediaMediaAssetService,
        ApiMetrics.MetricSeriesValidationService.MetricSeriesValidationServiceClient metricsMetricSeriesValidationService,
        RetryPolicy policy,
        ApiRatelimiting.RateLimitQueryService.RateLimitQueryServiceClient ratelimitingRateLimitQueryService,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiTravel.TripQueryService.TripQueryServiceClient travelTripQueryService,
        ApiWorkplaces.BuildingCalculationService.BuildingCalculationServiceClient workplacesBuildingCalculationService,
        ApiWorkplaces.BuildingValidationService.BuildingValidationServiceClient workplacesBuildingValidationService,
        ApiWorkplaces.VisitorPassValidationService.VisitorPassValidationServiceClient workplacesVisitorPassValidationService)
    {
        _documentsDocumentTemplateCalculationService = documentsDocumentTemplateCalculationService;
        _documentsDocumentValidationService = documentsDocumentValidationService;
        _identityUserService = identityUserService;
        _identityUserValidationService = identityUserValidationService;
        _invoker = invoker;
        _logger = logger;
        _mediaMediaAssetService = mediaMediaAssetService;
        _metricsMetricSeriesValidationService = metricsMetricSeriesValidationService;
        _policy = policy;
        _ratelimitingRateLimitQueryService = ratelimitingRateLimitQueryService;
        _registry = registry;
        _repository = repository;
        _settings = settings;
        _travelTripQueryService = travelTripQueryService;
        _workplacesBuildingCalculationService = workplacesBuildingCalculationService;
        _workplacesBuildingValidationService = workplacesBuildingValidationService;
        _workplacesVisitorPassValidationService = workplacesVisitorPassValidationService;
        _sendResolveDesks0 = _mediaMediaAssetService.UpdateMediaAssetAsync;
    }
}
