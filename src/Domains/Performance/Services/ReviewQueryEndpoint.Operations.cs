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
using ApiAnalytics = Acme.Analytics.V1;
using ApiAudit = Acme.Audit.V1;
using ApiCommon = Acme.Common.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Performance;

public sealed partial class ReviewQueryEndpoint
{
    /// <summary>
    /// Lists reviews a page at a time.
    /// </summary>
    public override async Task<ApiPerformance.BrowseReviewsResponse> BrowseReviews(ApiPerformance.BrowseReviewsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling BrowseReviews for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "review", "batch",
            "BrowseReviews", "accepted", token);
        var input0 = new ApiAudit.CloneAuditEventRequest
        {
            AuditEvent = new ApiAudit.AuditEvent
            {
                Attributes = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Measurements = { new ApiAudit.AuditEventMeasurement
                {
                    ObservedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                } },
                Source = (request.Filter?.OwnerId ?? ""),
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _auditAuditEventService.CloneAuditEventAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiAnalytics.SearchAnalyticsEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiAnalytics.SearchAnalyticsEventsRequest.Types.Filter
            {
                Labels = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Filter?.OwnerId ?? ""),
            },
            Page = new ApiCommon.PageRequest
            {
                PageSize = (request.Page?.PageSize ?? 0),
                PageToken = (request.Page?.PageToken ?? ""),
            },
        };
        var reply1 = await Workflow.SendAsync(input1, token, _analyticsAnalyticsEventService.SearchAnalyticsEventsAsync);
        var response = new ApiPerformance.BrowseReviewsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Reviews = { new ApiPerformance.Review
            {
                Counterparty = new ApiPerformance.GoalRef
                {
                    DisplayName = (reply0.AuditEvent?.Subject?.DisplayName ?? ""),
                },
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.OrderBy ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "review",
            "audit", "BrowseReviews", (request.OrderBy ?? ""), token);
        return response;
    }
    /// <summary>
    /// Counts reviews by status.
    /// </summary>
    public override async Task<ApiPerformance.CountReviewsResponse> CountReviews(ApiPerformance.CountReviewsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CountReviews for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "review", "batch",
            "CountReviews", "accepted", token);
        var input0 = new ApiItsm.NormalizeServiceAssetRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Address = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        using var rpc0 = _sendCountReviews0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiAudit.BatchGetAuditEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var named1 = _registry.Get<ApiAudit.AuditEventService.AuditEventServiceClient>("acme.audit.v1.AuditEventService");
        using var rpc1 = named1.BatchGetAuditEventsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiPerformance.CountReviewsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "review",
            "audit", "CountReviews", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Lists reviews a page at a time.
    /// </summary>
    public override async Task<ApiPerformance.FindReviewsResponse> FindReviews(ApiPerformance.FindReviewsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling FindReviews for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "review", "batch",
            "FindReviews", "accepted", token);
        var input0 = new ApiPerformance.QuoteCalibrationRequest
        {
            Calibration = new ApiPerformance.Calibration
            {
                Assignee = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiPerformance.QuoteCalibrationRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        var descriptor0 = new Method<ApiPerformance.QuoteCalibrationRequest, ApiPerformance.QuoteCalibrationResponse>(
            MethodType.Unary, "acme.performance.v1" + ".CalibrationCalculationService", "QuoteCalibration",
            Wire.Marshaller<ApiPerformance.QuoteCalibrationRequest>(), Wire.Marshaller<ApiPerformance.QuoteCalibrationResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiAudit.BatchCreateAuditEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Header = new ApiAudit.BatchCreateAuditEventsRequest.Types.StreamHeader
            {
                SubmittedBy = new ApiCommon.ContactInfo
                {
                    Email = (reply0.Calibration?.Requester?.Email ?? ""),
                    Name = (reply0.Calibration?.Requester?.Name ?? new ApiCommon.PersonName()),
                    Phone = (reply0.Calibration?.Requester?.Phone ?? ""),
                    PreferredChannel = (reply0.Calibration?.Requester?.PreferredChannel ?? ""),
                },
            },
        };
        using var stream1 = _auditAuditEventService.BatchCreateAuditEvents(cancellationToken: token);
        await stream1.RequestStream.WriteAsync(input1, token);
        await stream1.RequestStream.CompleteAsync();
        var reply1 = await stream1.ResponseAsync;
        var response = new ApiPerformance.FindReviewsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "review",
            "audit", "FindReviews", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a review.
    /// </summary>
    public override async Task<ApiPerformance.LookupReviewResponse> LookupReview(ApiPerformance.LookupReviewRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ReviewId, "review_id");
        _logger.LogInformation("Handling LookupReview for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "review", request.ReviewId,
            "LookupReview", "accepted", token);
        var response = new ApiPerformance.LookupReviewResponse
        {
            Review = new ApiPerformance.Review
            {
                ReviewId = (request.ReviewId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Returns a review.
    /// </summary>
    public override async Task<ApiPerformance.PeekReviewResponse> PeekReview(ApiPerformance.PeekReviewRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ReviewId, "review_id");
        _logger.LogInformation("Handling PeekReview for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "review", request.ReviewId,
            "PeekReview", "accepted", token);
        var response = new ApiPerformance.PeekReviewResponse
        {
            Review = new ApiPerformance.Review
            {
                ReviewId = (request.ReviewId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Returns several reviews at once.
    /// </summary>
    public override async Task<ApiPerformance.ResolveReviewsResponse> ResolveReviews(ApiPerformance.ResolveReviewsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ResolveReviews for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "review", "batch",
            "ResolveReviews", "accepted", token);
        var profile0 = new MappingProfile<ApiPerformance.ResolveReviewsRequest, ApiWorkplaces.CompareRoomBookingOptionsRequest>(value => new ApiWorkplaces.CompareRoomBookingOptionsRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                Participants = { new ApiWorkplaces.RoomBookingParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (value.Context?.Actor?.Email ?? ""),
                    },
                } },
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _workplacesRoomBookingCalculationService.CompareRoomBookingOptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiAudit.BatchGetAuditEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        input1 = PropertyMap.Copy(input1);
        using var rpc1 = _auditAuditEventService.BatchGetAuditEventsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiPerformance.ResolveReviewsResponse
        {
            PageInfo = new ApiCommon.PageInfo
            {
                TotalSize = (reply1.PageInfo?.TotalSize ?? 0L),
            },
            Reviews = { new ApiPerformance.Review
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.RoomBooking?.Audit?.Etag ?? ""),
                },
            } },
        };
        return response;
    }
    /// <summary>
    /// Summarizes a review.
    /// </summary>
    public override async Task<ApiPerformance.SummarizeReviewsResponse> SummarizeReviews(ApiPerformance.SummarizeReviewsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SummarizeReviews for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "review", request.ReviewId,
            "SummarizeReviews", "accepted", token);
        var input0 = request.SummarizeReviewsToCompareCalibrationOptions0();
        using var rpc0 = _performanceCalibrationCalculationService.CompareCalibrationOptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiPerformance.SimulateFeedbackRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiPerformance.SimulateFeedbackRequest.Types.Details
            {
                CurrencyCode = (reply0.Breakdown?.CurrencyCode ?? ""),
            },
            Feedback = new ApiPerformance.Feedback
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (reply0.Calibration?.Assignee?.DisplayName ?? ""),
                    Email = (reply0.Calibration?.Requester?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _performanceFeedbackCalculationService.SimulateFeedbackAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiItsm.PreviewServiceAssetRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.Calibration?.Audit?.Etag ?? ""),
                },
            },
        };
        var labels2 = input2.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input2.Context.Baggage.Clear();
        input2.Context.Baggage.Add(labels2);
        using var rpc2 = _itsmServiceAssetValidationService.PreviewServiceAssetAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiAudit.FindAuditEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiAudit.FindAuditEventsRequest.Types.Filter
            {
                CreatedAfter = (reply2.ServiceAsset?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Region = (reply2.ServiceAsset ?? new ApiItsm.ServiceAsset()).LocatorCase switch
                {
                    ApiItsm.ServiceAsset.LocatorOneofCase.Address => (reply2.ServiceAsset?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc3 = _auditAuditEventQueryService.FindAuditEventsAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiPerformance.SummarizeReviewsResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply3.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply1.Result?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.ReviewId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "review",
            "audit", "SummarizeReviews", (request.ReviewId ?? ""), token);
        return response;
    }
}
