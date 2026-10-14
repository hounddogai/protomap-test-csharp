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
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiExperiments = Acme.Experiments.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLabeling = Acme.Labeling.V1;
using ApiLocalization = Acme.Localization.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Performance;

/// <summary>
/// Manages reviews: creates, reads, changes, and lists them.
/// </summary>
public sealed class ReviewEndpoint : ApiPerformance.ReviewService.ReviewServiceBase
{
    private readonly ApiAuthorization.AccessPolicyValidationService.AccessPolicyValidationServiceClient _authorizationAccessPolicyValidationService;
    private readonly ApiDevices.DevicePolicyValidationService.DevicePolicyValidationServiceClient _devicesDevicePolicyValidationService;
    private readonly ApiDocuments.SignatureNotificationService.SignatureNotificationServiceClient _documentsSignatureNotificationService;
    private readonly ApiExperiments.ExperimentQueryService.ExperimentQueryServiceClient _experimentsExperimentQueryService;
    private readonly CallInvoker _invoker;
    private readonly ApiLabeling.LabelingTaskService.LabelingTaskServiceClient _labelingLabelingTaskService;
    private readonly ApiLocalization.TranslationKeyQueryService.TranslationKeyQueryServiceClient _localizationTranslationKeyQueryService;
    private readonly ILogger<ReviewEndpoint> _logger;
    private readonly ApiPerformance.CalibrationCalculationService.CalibrationCalculationServiceClient _performanceCalibrationCalculationService;
    private readonly ApiPerformance.CalibrationValidationService.CalibrationValidationServiceClient _performanceCalibrationValidationService;
    private readonly ApiPerformance.FeedbackCalculationService.FeedbackCalculationServiceClient _performanceFeedbackCalculationService;
    private readonly ApiPerformance.FeedbackValidationService.FeedbackValidationServiceClient _performanceFeedbackValidationService;
    private readonly ApiPerformance.GoalQueryService.GoalQueryServiceClient _performanceGoalQueryService;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTravel.TravelPolicyValidationService.TravelPolicyValidationServiceClient _travelTravelPolicyValidationService;

    public ReviewEndpoint(
        ApiAuthorization.AccessPolicyValidationService.AccessPolicyValidationServiceClient authorizationAccessPolicyValidationService,
        ApiDevices.DevicePolicyValidationService.DevicePolicyValidationServiceClient devicesDevicePolicyValidationService,
        ApiDocuments.SignatureNotificationService.SignatureNotificationServiceClient documentsSignatureNotificationService,
        ApiExperiments.ExperimentQueryService.ExperimentQueryServiceClient experimentsExperimentQueryService,
        CallInvoker invoker,
        ApiLabeling.LabelingTaskService.LabelingTaskServiceClient labelingLabelingTaskService,
        ApiLocalization.TranslationKeyQueryService.TranslationKeyQueryServiceClient localizationTranslationKeyQueryService,
        ILogger<ReviewEndpoint> logger,
        ApiPerformance.CalibrationCalculationService.CalibrationCalculationServiceClient performanceCalibrationCalculationService,
        ApiPerformance.CalibrationValidationService.CalibrationValidationServiceClient performanceCalibrationValidationService,
        ApiPerformance.FeedbackCalculationService.FeedbackCalculationServiceClient performanceFeedbackCalculationService,
        ApiPerformance.FeedbackValidationService.FeedbackValidationServiceClient performanceFeedbackValidationService,
        ApiPerformance.GoalQueryService.GoalQueryServiceClient performanceGoalQueryService,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiTravel.TravelPolicyValidationService.TravelPolicyValidationServiceClient travelTravelPolicyValidationService)
    {
        _authorizationAccessPolicyValidationService = authorizationAccessPolicyValidationService;
        _devicesDevicePolicyValidationService = devicesDevicePolicyValidationService;
        _documentsSignatureNotificationService = documentsSignatureNotificationService;
        _experimentsExperimentQueryService = experimentsExperimentQueryService;
        _invoker = invoker;
        _labelingLabelingTaskService = labelingLabelingTaskService;
        _localizationTranslationKeyQueryService = localizationTranslationKeyQueryService;
        _logger = logger;
        _performanceCalibrationCalculationService = performanceCalibrationCalculationService;
        _performanceCalibrationValidationService = performanceCalibrationValidationService;
        _performanceFeedbackCalculationService = performanceFeedbackCalculationService;
        _performanceFeedbackValidationService = performanceFeedbackValidationService;
        _performanceGoalQueryService = performanceGoalQueryService;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _settings = settings;
        _travelTravelPolicyValidationService = travelTravelPolicyValidationService;
    }
    /// <summary>
    /// Creates a review.
    /// </summary>
    public override async Task<ApiPerformance.CloneReviewResponse> CloneReview(ApiPerformance.CloneReviewRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CloneReview for {Tenant}", requestContext.TenantId);
        if (request.Review is { } record)
        {
            var snapshot = ReviewMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiAuthorization.ExplainAccessPolicyDecisionRequest
        {
            AccessPolicy = new ApiAuthorization.AccessPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Review?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (request.Review?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Review?.Counterparty?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiAuthorization.ExplainAccessPolicyDecisionRequest>(json0);
        using var rpc0 = _authorizationAccessPolicyValidationService.ExplainAccessPolicyDecisionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiPerformance.CompareFeedbackOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiPerformance.CompareFeedbackOptionsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Recipient = (request.Review?.BillingAddress?.Recipient ?? ""),
                },
            },
            Feedback = new ApiPerformance.Feedback
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
        };
        using var rpc1 = _performanceFeedbackCalculationService.CompareFeedbackOptionsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiPerformance.CloneReviewResponse
        {
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Review = new ApiPerformance.Review
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.AccessPolicy?.Audit?.Etag ?? ""),
                    UpdatedBy = (reply0.AccessPolicy?.Audit?.UpdatedBy ?? ""),
                },
                Counterparty = new ApiPerformance.GoalRef
                {
                    DisplayName = (reply1.Feedback?.Subject?.DisplayName ?? ""),
                },
                DueAt = (request.Review?.DueAt ?? new Timestamp()),
                IssuedAt = (request.Review?.IssuedAt ?? new Timestamp()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Review?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "review",
            "audit", "CloneReview", (request.Review?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Creates a review.
    /// </summary>
    public override async Task<ApiPerformance.CreateReviewResponse> CreateReview(ApiPerformance.CreateReviewRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CreateReview for {Tenant}", requestContext.TenantId);
        if (request.Review is { } record)
        {
            var snapshot = ReviewMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiPerformance.ExplainFeedbackCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Feedback = new ApiPerformance.Feedback
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (request.Review?.Counterparty?.DisplayName ?? ""),
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _performanceFeedbackCalculationService.ExplainFeedbackCalculationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTravel.ScoreTravelPolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.ScoreTravelPolicyRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Locality = (request.Review?.BillingAddress?.Locality ?? ""),
                },
            },
        };
        var reply1 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _travelTravelPolicyValidationService.ScoreTravelPolicyAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input2 = new ApiPerformance.CheckCalibrationEligibilityRequest
        {
            Calibration = new ApiPerformance.Calibration
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Review?.Audit?.CreatedBy ?? ""),
                    Etag = (request.Review?.Audit?.Etag ?? ""),
                },
                Customer = new ApiPerformance.ReviewRef
                {
                    DisplayName = (request.Review?.Counterparty?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply2 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _performanceCalibrationValidationService.CheckCalibrationEligibilityAsync(input2, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input3 = new ApiPerformance.SimulateCalibrationRequest
        {
            Calibration = new ApiPerformance.Calibration
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (reply2.Calibration ?? new ApiPerformance.Calibration()).OriginCase switch
                    {
                        ApiPerformance.Calibration.OriginOneofCase.Customer => (reply2.Calibration?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Phone = (reply2.Calibration?.Requester?.Phone ?? ""),
                    PreferredChannel = (reply2.Calibration?.Requester?.PreferredChannel ?? ""),
                },
                Sla = new ApiPerformance.CalibrationSla
                {
                    PolicyId = (reply2.Calibration?.Sla?.PolicyId ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc3 = _performanceCalibrationCalculationService.SimulateCalibrationAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiPerformance.CreateReviewResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Feedback?.CorrelationId ?? ""),
                Details = { (reply2.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Result?.Message ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Review = new ApiPerformance.Review
            {
                Counterparty = (request.Review?.Counterparty ?? new ApiPerformance.GoalRef()),
                DueAt = (request.Review?.DueAt ?? new Timestamp()),
                Total = (reply3.Breakdown?.Total ?? new ApiCommon.Money()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Review?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "review",
            "audit", "CreateReview", (request.Review?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Deletes a review.
    /// </summary>
    public override async Task<ApiPerformance.DeleteReviewResponse> DeleteReview(ApiPerformance.DeleteReviewRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ReviewId, "review_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling DeleteReview for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "review", request.ReviewId,
            "DeleteReview", "accepted", token);
        var input0 = new ApiPerformance.PreviewFeedbackRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiPerformance.PreviewFeedbackRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            Feedback = new ApiPerformance.Feedback
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
            Reason = (request.Reason ?? ""),
        };
        using var rpc0 = _performanceFeedbackValidationService.PreviewFeedbackAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLocalization.FindTranslationKeysRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLocalization.FindTranslationKeysRequest.Types.Filter
            {
                CreatedAfter = (reply0.Outcome?.EffectiveAt ?? new Timestamp()),
                Labels = { (reply0.Feedback?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply0.Feedback?.Subject?.CandidateId ?? ""),
            },
        };
        var named1 = _registry.Get<ApiLocalization.TranslationKeyQueryService.TranslationKeyQueryServiceClient>("acme.localization.v1.TranslationKeyQueryService");
        using var rpc1 = named1.FindTranslationKeysAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiPerformance.DeleteReviewResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply1.Result?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.ReviewId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "review",
            "audit", "DeleteReview", (request.ReviewId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a review.
    /// </summary>
    public override async Task<ApiPerformance.GetReviewResponse> GetReview(ApiPerformance.GetReviewRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ReviewId, "review_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetReview for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "review", request.ReviewId,
            "GetReview", "accepted", token);
        var response = new ApiPerformance.GetReviewResponse
        {
            Review = new ApiPerformance.Review
            {
                ReviewId = (request.ReviewId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists reviews a page at a time.
    /// </summary>
    public override async Task<ApiPerformance.ListReviewsResponse> ListReviews(ApiPerformance.ListReviewsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ListReviews for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "review", "batch",
            "ListReviews", "accepted", token);
        var input0 = new ApiDevices.PreviewDevicePolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.PreviewDevicePolicyRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        var descriptor0 = new Method<ApiDevices.PreviewDevicePolicyRequest, ApiDevices.PreviewDevicePolicyResponse>(
            MethodType.Unary, "acme.devices.v1" + ".DevicePolicyValidationService", "PreviewDevicePolicy",
            Wire.Marshaller<ApiDevices.PreviewDevicePolicyRequest>(), Wire.Marshaller<ApiDevices.PreviewDevicePolicyResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiPerformance.ListReviewsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "review",
            "audit", "ListReviews", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Lists reviews a page at a time.
    /// </summary>
    public override async Task<ApiPerformance.SearchReviewsResponse> SearchReviews(ApiPerformance.SearchReviewsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SearchReviews for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "review", "batch",
            "SearchReviews", "accepted", token);
        var input0 = new ApiPerformance.BrowseGoalsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiPerformance.BrowseGoalsRequest.Types.Filter
            {
                CreatedAfter = (request.Filter?.CreatedAfter ?? new Timestamp()),
                Labels = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Query = (request.Filter?.Query ?? ""),
            },
            Page = new ApiCommon.PageRequest
            {
                PageToken = (request.Page?.PageToken ?? ""),
            },
        };
        var command0 = new DispatchCommand<ApiPerformance.BrowseGoalsRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _performanceGoalQueryService.BrowseGoalsAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiPerformance.SearchReviewsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.OrderBy ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "review",
            "audit", "SearchReviews", (request.OrderBy ?? ""), token);
        return response;
    }
    /// <summary>
    /// Changes a review.
    /// </summary>
    public override async Task<ApiPerformance.UpdateReviewResponse> UpdateReview(ApiPerformance.UpdateReviewRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling UpdateReview for {Tenant}", requestContext.TenantId);
        if (request.Review is { } record)
        {
            var snapshot = ReviewMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var profile0 = new MappingProfile<ApiPerformance.UpdateReviewRequest, ApiDocuments.ListSignatureSubscriptionsRequest>(value => new ApiDocuments.ListSignatureSubscriptionsRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListSignatureSubscriptionsRequest.Types.Filter
            {
                CreatedAfter = (value.Review?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (value.Review?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (value.Review?.Counterparty?.GoalId ?? ""),
                Region = (value.Review?.BillingAddress?.Region ?? ""),
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _documentsSignatureNotificationService.ListSignatureSubscriptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiPerformance.NormalizeCalibrationRequest
        {
            Calibration = new ApiPerformance.Calibration
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Review?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Customer = new ApiPerformance.ReviewRef
                {
                    DisplayName = (request.Review?.Counterparty?.DisplayName ?? ""),
                },
                Priority = (request.Review?.Counterparty?.Priority ?? 0),
                Sla = new ApiPerformance.CalibrationSla
                {
                    DueAt = (request.Review?.DueAt ?? new Timestamp()),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        input1 = PropertyMap.Copy(input1);
        using var rpc1 = _performanceCalibrationValidationService.NormalizeCalibrationAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiPerformance.UpdateReviewResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Review = new ApiPerformance.Review
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.Calibration?.Audit?.CreatedAt ?? new Timestamp()),
                },
                ReviewId = (request.Review?.ReviewId ?? ""),
                Status = (request.Review?.Status ?? (ApiPerformance.ReviewStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Review?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "review",
            "audit", "UpdateReview", (request.Review?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams the changes of reviews as they happen.
    /// </summary>
    public override async Task WatchReviews(ApiPerformance.WatchReviewsRequest request, IServerStreamWriter<ApiPerformance.WatchReviewsResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling WatchReviews for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "review", "batch",
            "WatchReviews", "accepted", token);
        var input0 = new ApiLabeling.ListLabelingTasksRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLabeling.ListLabelingTasksRequest.Types.Filter
            {
                CreatedAfter = (request.Filter?.CreatedAfter ?? new Timestamp()),
            },
        };
        using var rpc0 = _labelingLabelingTaskService.ListLabelingTasksAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiExperiments.SuggestExperimentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiExperiments.SuggestExperimentsRequest.Types.Filter
            {
                CreatedAfter = (request.Filter?.CreatedAfter ?? new Timestamp()),
                Labels = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Filter?.OwnerId ?? ""),
                Query = (request.Filter?.Query ?? ""),
            },
        };
        using var rpc1 = _experimentsExperimentQueryService.SuggestExperimentsAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var input2 = new ApiPerformance.CompareFeedbackOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiPerformance.CompareFeedbackOptionsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
            Feedback = new ApiPerformance.Feedback
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    Email = (request.Subscriber?.Email ?? ""),
                },
            },
        };
        using var rpc2 = _performanceFeedbackCalculationService.CompareFeedbackOptionsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiPerformance.WatchReviewsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Result?.Message ?? ""),
            },
            Review = new ApiPerformance.Review
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Subscriber?.PreferredChannel ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "review",
            "audit", "WatchReviews", (request.Subscriber?.PreferredChannel ?? ""), token);
        await responses.WriteAsync(response, token);
    }
}
