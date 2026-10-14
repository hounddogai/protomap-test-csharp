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
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSearch = Acme.Search.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Performance;

public sealed partial class ReviewApprovalEndpoint
{
    /// <summary>
    /// Approves a review.
    /// </summary>
    public override async Task<ApiPerformance.ApproveReviewResponse> ApproveReview(ApiPerformance.ApproveReviewRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ApproveReview for {Tenant}", requestContext.TenantId);
        if (request.Review is { } record)
        {
            var snapshot = ReviewMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var batch0 = (request.Review?.Attachments ?? new Google.Protobuf.Collections.RepeatedField<ApiPerformance.ReviewAttachment>()).Select(async item =>
        {
            var input0 = new ApiRecruiting.CheckOfferEligibilityRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Offer = new ApiRecruiting.Offer
                {
                    Attachments = { new ApiRecruiting.OfferAttachment
                    {
                        Checksum = (item.Checksum ?? ""),
                        FileName = (item.FileName ?? ""),
                    } },
                },
            };
            using var rpc0 = _recruitingOfferValidationService.CheckOfferEligibilityAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            return reply0;
        }).ToArray();
        var replies0 = await Task.WhenAll(batch0);
        var reply0 = replies0.LastOrDefault() ?? new ApiRecruiting.CheckOfferEligibilityResponse();
        var response = new ApiPerformance.ApproveReviewResponse
        {
            Review = new ApiPerformance.Review
            {
                ReviewId = (request.ReviewId ?? ""),
                Total = (request.Review?.Total ?? new ApiCommon.Money()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Review?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "review",
            "audit", "ApproveReview", (request.Review?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Comments a review.
    /// </summary>
    public override async Task<ApiPerformance.CommentReviewApprovalResponse> CommentReviewApproval(ApiPerformance.CommentReviewApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CommentReviewApproval for {Tenant}", requestContext.TenantId);
        if (request.Review is { } record)
        {
            var snapshot = ReviewMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiPerformance.PeekReviewRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ReviewId = (request.ReviewId ?? ""),
        };
        var descriptor0 = new Method<ApiPerformance.PeekReviewRequest, ApiPerformance.PeekReviewResponse>(
            MethodType.Unary, "acme.performance.v1" + ".ReviewQueryService", "PeekReview",
            Wire.Marshaller<ApiPerformance.PeekReviewRequest>(), Wire.Marshaller<ApiPerformance.PeekReviewResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiPerformance.CommentReviewApprovalResponse
        {
            Review = new ApiPerformance.Review
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.Review?.Audit?.Etag ?? ""),
                },
                ReviewId = (request.ReviewId ?? ""),
                Status = (request.Review?.Status ?? (ApiPerformance.ReviewStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Review?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "review",
            "audit", "CommentReviewApproval", (request.Review?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Escalates a review.
    /// </summary>
    public override async Task<ApiPerformance.EscalateReviewResponse> EscalateReview(ApiPerformance.EscalateReviewRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EscalateReview for {Tenant}", requestContext.TenantId);
        if (request.Review is { } record)
        {
            var snapshot = ReviewMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.CompareDocumentFolderVersionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.CompareDocumentFolderVersionsRequest.Types.Details
            {
                Snapshot = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                Children = { new ApiDocuments.DocumentFolder
                {
                    Children = { new ApiDocuments.DocumentFolder
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            UpdatedAt = (request.Review?.Audit?.UpdatedAt ?? new Timestamp()),
                        },
                        Location = new ApiCommon.PostalAddress
                        {
                            PostalCode = (request.Review?.BillingAddress?.PostalCode ?? ""),
                            Region = (request.Review?.BillingAddress?.Region ?? ""),
                        },
                        Manager = new ApiIdentity.UserRef
                        {
                            DisplayName = (request.Review?.Counterparty?.DisplayName ?? ""),
                        },
                    } },
                } },
            },
        };
        using var rpc0 = _documentsDocumentFolderHistoryService.CompareDocumentFolderVersionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var batch1 = (request.Review?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiPerformance.ReviewLine>()).Select(async item =>
        {
            var input1 = new ApiSearch.SuggestSearchIndexesRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiSearch.SuggestSearchIndexesRequest.Types.Filter
                {
                    Labels = { (item.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                    Region = (request.Review?.BillingAddress?.Region ?? ""),
                },
            };
            using var rpc1 = _searchSearchIndexQueryService.SuggestSearchIndexesAsync(input1, cancellationToken: token);
            var reply1 = await rpc1.ResponseAsync;
            return reply1;
        }).ToArray();
        var replies1 = await Task.WhenAll(batch1);
        var reply1 = replies1.LastOrDefault() ?? new ApiSearch.SuggestSearchIndexesResponse();
        var input2 = new ApiPerformance.BrowseReviewsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiPerformance.BrowseReviewsRequest.Types.Filter
            {
                CreatedAfter = (request.Review?.IssuedAt ?? new Timestamp()),
                Labels = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply1.Result?.CorrelationId ?? ""),
                Region = (reply0.DocumentFolder?.Location?.Region ?? ""),
            },
        };
        input2 = PropertyMap.Copy(input2);
        using var rpc2 = _performanceReviewQueryService.BrowseReviewsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = request.EscalateReviewToLookupReview3();
        using var rpc3 = _performanceReviewQueryService.LookupReviewAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiPerformance.EscalateReviewResponse
        {
            Outcome = new ApiPerformance.EscalateReviewResponse.Types.Outcome
            {
                Message = (reply3.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
            },
            Review = new ApiPerformance.Review
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.DocumentFolder?.Audit?.UpdatedBy ?? ""),
                },
                BillingAddress = (request.Review?.BillingAddress ?? new ApiCommon.PostalAddress()),
                Number = (reply3.Review?.Number ?? ""),
                ReviewId = (request.ReviewId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Review?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "review",
            "audit", "EscalateReview", (request.Review?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a review.
    /// </summary>
    public override async Task<ApiPerformance.GetReviewApprovalStatusResponse> GetReviewApprovalStatus(ApiPerformance.GetReviewApprovalStatusRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ReviewId, "review_id");
        _logger.LogInformation("Handling GetReviewApprovalStatus for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "review", request.ReviewId,
            "GetReviewApprovalStatus", "accepted", token);
        var input0 = new ApiPerformance.NormalizeGoalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Goal = new ApiPerformance.Goal
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Customer = new ApiPerformance.ReviewRef
                {
                    ReviewId = (request.ReviewId ?? ""),
                },
            },
        };
        var reply0 = await SendGetReviewApprovalStatus0Async(input0, token);
        var response = new ApiPerformance.GetReviewApprovalStatusResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Outcome?.Message ?? ""),
            },
            Review = new ApiPerformance.Review
            {
                ReviewId = (reply0.Goal?.Customer?.ReviewId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists reviews a page at a time.
    /// </summary>
    public override async Task<ApiPerformance.ListPendingReviewsResponse> ListPendingReviews(ApiPerformance.ListPendingReviewsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListPendingReviews for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "review", "batch",
            "ListPendingReviews", "accepted", token);
        var input0 = new ApiItsm.GenerateProblemReportRequest
        {
            Annotations = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.GenerateProblemReportRequest.Types.Details
            {
                Filters = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _itsmProblemReportingService.GenerateProblemReportAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiPerformance.ListPendingReviewsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "review",
            "audit", "ListPendingReviews", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Rejects a review.
    /// </summary>
    public override async Task<ApiPerformance.RejectReviewResponse> RejectReview(ApiPerformance.RejectReviewRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RejectReview for {Tenant}", requestContext.TenantId);
        if (request.Review is { } record)
        {
            var snapshot = ReviewMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiPerformance.ResolveReviewsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiPerformance.ResolveReviewsRequest>();
        using var rpc0 = _performanceReviewQueryService.ResolveReviewsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.AdvanceIncidentWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Incident = new ApiItsm.Incident
            {
                Customer = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                },
            },
        };
        var json1 = JsonFormatter.Default.Format(input1);
        input1 = JsonParser.Default.Parse<ApiItsm.AdvanceIncidentWorkflowRequest>(json1);
        using var rpc1 = _itsmIncidentWorkflowService.AdvanceIncidentWorkflowAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiCorporateevents.SyncVenuesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.SyncVenuesRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Venue = new ApiCorporateevents.Venue
            {
                Address = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Review?.BillingAddress?.CountryCode ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Review?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedBy = (reply1.Incident?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        using var rpc2 = _corporateeventsVenueSyncService.SyncVenuesAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiPerformance.RejectReviewResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                ReferenceIds = { (reply2.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Review = new ApiPerformance.Review
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply1.Incident?.Audit?.UpdatedBy ?? ""),
                },
                DueAt = (request.Review?.DueAt ?? new Timestamp()),
                ReviewId = (request.ReviewId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Review?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "review",
            "audit", "RejectReview", (request.Review?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Submits a review.
    /// </summary>
    public override async Task<ApiPerformance.SubmitReviewResponse> SubmitReview(ApiPerformance.SubmitReviewRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SubmitReview for {Tenant}", requestContext.TenantId);
        if (request.Review is { } record)
        {
            var snapshot = ReviewMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.ListItineraryReportsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiTravel.ListItineraryReportsRequest.Types.Filter
            {
                CreatedAfter = (request.Review?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (request.Review?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Details?.Approver?.UserId ?? ""),
                Region = (request.Review?.BillingAddress?.Region ?? ""),
            },
        };
        using var rpc0 = _travelItineraryReportingService.ListItineraryReportsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var batch1 = (request.Review?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiPerformance.ReviewLine>()).Select(async item =>
        {
            var input1 = new ApiPerformance.ScoreFeedbackRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiPerformance.ScoreFeedbackRequest.Types.Details
                {
                    Destination = new ApiCommon.PostalAddress
                    {
                        Lines = { (request.Review?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                        Locality = (request.Review?.BillingAddress?.Locality ?? ""),
                        Recipient = (request.Review?.BillingAddress?.Recipient ?? ""),
                    },
                },
                Feedback = new ApiPerformance.Feedback
                {
                    Attributes = { (item.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                    Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                },
            };
            using var rpc1 = _performanceFeedbackValidationService.ScoreFeedbackAsync(input1, cancellationToken: token);
            var reply1 = await rpc1.ResponseAsync;
            return reply1;
        }).ToArray();
        var replies1 = await Task.WhenAll(batch1);
        var reply1 = replies1.LastOrDefault() ?? new ApiPerformance.ScoreFeedbackResponse();
        var response = new ApiPerformance.SubmitReviewResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            Review = new ApiPerformance.Review
            {
                Number = (request.Review?.Number ?? ""),
                ReviewId = (request.ReviewId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Review?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "review",
            "audit", "SubmitReview", (request.Review?.VoucherCode ?? ""), token);
        return response;
    }
    private async Task<ApiPerformance.NormalizeGoalResponse> SendGetReviewApprovalStatus0Async(ApiPerformance.NormalizeGoalRequest request, CancellationToken token)
    {
        return await _reviewApprovalEndpointSendGetReviewApprovalStatus0Port.SendAsync(request, token);
    }
}
