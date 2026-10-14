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
using ApiCart = Acme.Cart.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiOrders = Acme.Orders.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Performance;

public sealed partial class ReviewWorkflowEndpoint
{
    /// <summary>
    /// Advances a review.
    /// </summary>
    public override async Task<ApiPerformance.AdvanceReviewWorkflowResponse> AdvanceReviewWorkflow(ApiPerformance.AdvanceReviewWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling AdvanceReviewWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Review is { } record)
        {
            var snapshot = ReviewMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiPerformance.NormalizeCalibrationRequest
        {
            Calibration = new ApiPerformance.Calibration
            {
                Customer = new ApiPerformance.ReviewRef
                {
                    DisplayName = (request.Review?.Counterparty?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiPerformance.NormalizeCalibrationRequest>();
        using var rpc0 = _performanceCalibrationValidationService.NormalizeCalibrationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiCorporateevents.ListVenueConflictsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiCorporateevents.ListVenueConflictsRequest.Types.Filter
            {
                Region = (request.Review?.BillingAddress?.Region ?? ""),
            },
        };
        var json1 = JsonFormatter.Default.Format(input1);
        input1 = JsonParser.Default.Parse<ApiCorporateevents.ListVenueConflictsRequest>(json1);
        using var rpc1 = _corporateeventsVenueSyncService.ListVenueConflictsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiPerformance.AdvanceReviewWorkflowResponse
        {
            Outcome = new ApiPerformance.AdvanceReviewWorkflowResponse.Types.Outcome
            {
                Message = (reply1.Result?.Message ?? ""),
            },
            Review = new ApiPerformance.Review
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Calibration?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Metadata = { (request.Review?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReviewId = (request.ReviewId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Review?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "review",
            "audit", "AdvanceReviewWorkflow", (request.Review?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Cancels a review.
    /// </summary>
    public override async Task<ApiPerformance.CancelReviewWorkflowResponse> CancelReviewWorkflow(ApiPerformance.CancelReviewWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CancelReviewWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Review is { } record)
        {
            var snapshot = ReviewMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiPerformance.PeekReviewRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.ReviewId = (request.Review?.ReviewId ?? "");
        using var rpc0 = _performanceReviewQueryService.PeekReviewAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiCart.ListCartsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiCart.ListCartsRequest.Types.Filter
            {
                Region = (reply0.Review?.BillingAddress?.Region ?? ""),
            },
        };
        using var rpc1 = _cartCartService.ListCartsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiPerformance.CancelReviewWorkflowResponse
        {
            Outcome = new ApiPerformance.CancelReviewWorkflowResponse.Types.Outcome
            {
                Status = (reply1.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Message = (reply0.Result?.Message ?? ""),
            },
            Review = new ApiPerformance.Review
            {
                Counterparty = new ApiPerformance.GoalRef
                {
                    GoalId = (reply0.Review?.Counterparty?.GoalId ?? ""),
                },
                Notes = (request.Review?.Notes ?? ""),
                ReviewId = (request.ReviewId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Review?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "review",
            "audit", "CancelReviewWorkflow", (request.Review?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a review.
    /// </summary>
    public override async Task<ApiPerformance.InspectReviewWorkflowResponse> InspectReviewWorkflow(ApiPerformance.InspectReviewWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ReviewId, "review_id");
        _logger.LogInformation("Handling InspectReviewWorkflow for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "review", request.ReviewId,
            "InspectReviewWorkflow", "accepted", token);
        var input0 = new ApiDocuments.ListDocumentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListDocumentsRequest.Types.Filter
            {
                OwnerId = (request.ReviewId ?? ""),
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _documentsDocumentService.ListDocumentsAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiPerformance.CompareFeedbackOptionsRequest
        {
            Annotations = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Feedback = new ApiPerformance.Feedback
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Source = (request.ReviewId ?? ""),
                Subject = new ApiRecruiting.CandidateRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _performanceFeedbackCalculationService.CompareFeedbackOptionsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiPerformance.InspectReviewWorkflowResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Feedback?.CorrelationId ?? ""),
                Message = (reply0.Result?.Message ?? ""),
            },
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
    public override async Task<ApiPerformance.ListReviewWorkflowsResponse> ListReviewWorkflows(ApiPerformance.ListReviewWorkflowsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListReviewWorkflows for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "review", "batch",
            "ListReviewWorkflows", "accepted", token);
        var input0 = new ApiRecruiting.SuggestJobOpeningsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiRecruiting.SuggestJobOpeningsRequest.Types.Filter
            {
                CreatedAfter = (request.Filter?.CreatedAfter ?? new Timestamp()),
                Region = (request.Filter?.Region ?? ""),
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _recruitingJobOpeningQueryService.SuggestJobOpeningsAsync);
        await _bus.PublishAsync(new TransportNotification<ApiRecruiting.SuggestJobOpeningsRequest>("acme.performance.v1.ReviewWorkflowService/ListReviewWorkflows/0", input0.Clone()), token);
        var input1 = new ApiRecruiting.ExplainInterviewCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Interview = new ApiRecruiting.Interview
            {
                Location = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
                Organizer = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _sendListReviewWorkflows1(input1, new CallOptions(cancellationToken: token));
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiPerformance.ListReviewWorkflowsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply1.Outcome?.Status ?? ""),
            },
            Reviews = { new ApiPerformance.Review
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply1.Interview?.Audit?.UpdatedBy ?? ""),
                },
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "review",
            "audit", "ListReviewWorkflows", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pauses a review.
    /// </summary>
    public override async Task<ApiPerformance.PauseReviewWorkflowResponse> PauseReviewWorkflow(ApiPerformance.PauseReviewWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PauseReviewWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Review is { } record)
        {
            var snapshot = ReviewMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.ShareSignatureExportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ShareSignatureExportRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                    Name = (request.Details?.Notify?.Name ?? new ApiCommon.PersonName()),
                    PreferredChannel = (request.Details?.Notify?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc0 = _documentsSignatureExportService.ShareSignatureExportAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiPerformance.PauseReviewWorkflowResponse
        {
            Outcome = new ApiPerformance.PauseReviewWorkflowResponse.Types.Outcome
            {
                NextSteps = { (reply0.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Outcome?.Status ?? ""),
            },
            Review = new ApiPerformance.Review
            {
                Number = (request.Review?.Number ?? ""),
                ReviewId = (request.ReviewId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Review?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "review",
            "audit", "PauseReviewWorkflow", (request.Review?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Resumes a review.
    /// </summary>
    public override async Task<ApiPerformance.ResumeReviewWorkflowResponse> ResumeReviewWorkflow(ApiPerformance.ResumeReviewWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ResumeReviewWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Review is { } record)
        {
            var snapshot = ReviewMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiPerformance.ScoreGoalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiPerformance.ScoreGoalRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Locality = (request.Review?.BillingAddress?.Locality ?? ""),
                },
            },
            Goal = new ApiPerformance.Goal
            {
                Requester = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Details?.Notify?.PreferredChannel ?? ""),
                },
            },
        };
        var descriptor0 = new Method<ApiPerformance.ScoreGoalRequest, ApiPerformance.ScoreGoalResponse>(
            MethodType.Unary, "acme.performance.v1" + ".GoalValidationService", "ScoreGoal",
            Wire.Marshaller<ApiPerformance.ScoreGoalRequest>(), Wire.Marshaller<ApiPerformance.ScoreGoalResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var reply1 = new ApiDocuments.PullDocumentTemplatesResponse();
        foreach (var item in (request.Review?.Attachments ?? new Google.Protobuf.Collections.RepeatedField<ApiPerformance.ReviewAttachment>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input1 = new ApiDocuments.PullDocumentTemplatesRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiDocuments.PullDocumentTemplatesRequest.Types.Details
                {
                    Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
                },
                DocumentTemplate = new ApiDocuments.DocumentTemplate
                {
                    Attachments = { new ApiDocuments.DocumentTemplateAttachment
                    {
                        ContentType = (item.ContentType ?? ""),
                        SizeBytes = item.SizeBytes,
                        Uri = (item.Uri ?? ""),
                    } },
                },
            };
            using var rpc1 = _documentsDocumentTemplateSyncService.PullDocumentTemplatesAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var response = new ApiPerformance.ResumeReviewWorkflowResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
            Review = new ApiPerformance.Review
            {
                IssuedAt = (request.Review?.IssuedAt ?? new Timestamp()),
                ReviewId = (request.ReviewId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Review?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "review",
            "audit", "ResumeReviewWorkflow", (request.Review?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Retrys a review.
    /// </summary>
    public override async Task<ApiPerformance.RetryReviewWorkflowResponse> RetryReviewWorkflow(ApiPerformance.RetryReviewWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RetryReviewWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Review is { } record)
        {
            var snapshot = ReviewMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var batch0 = (request.Review?.Attachments ?? new Google.Protobuf.Collections.RepeatedField<ApiPerformance.ReviewAttachment>()).Select(async item =>
        {
            var input0 = new ApiOrders.ExplainOrderCalculationRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Order = new ApiOrders.Order
                {
                    Attachments = { new ApiOrders.OrderAttachment
                    {
                        Uri = (item.Uri ?? ""),
                    } },
                },
            };
            using var rpc0 = _ordersOrderCalculationService.ExplainOrderCalculationAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            return reply0;
        }).ToArray();
        var replies0 = await Task.WhenAll(batch0);
        var reply0 = replies0.LastOrDefault() ?? new ApiOrders.ExplainOrderCalculationResponse();
        var response = new ApiPerformance.RetryReviewWorkflowResponse
        {
            Review = new ApiPerformance.Review
            {
                ReviewId = (request.ReviewId ?? ""),
                Total = (request.Review?.Total ?? new ApiCommon.Money()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Review?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "review",
            "audit", "RetryReviewWorkflow", (request.Review?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Signals a review.
    /// </summary>
    public override async Task<ApiPerformance.SignalReviewWorkflowResponse> SignalReviewWorkflow(ApiPerformance.SignalReviewWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SignalReviewWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Review is { } record)
        {
            var snapshot = ReviewMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiCorporateevents.GenerateRegistrationReportResponse();
        foreach (var item in (request.Review?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiPerformance.ReviewLine>()))
        {
            var input0 = new ApiCorporateevents.GenerateRegistrationReportRequest
            {
                Annotations = { (item.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Reason = (request.Reason ?? ""),
            };
            using var rpc0 = _corporateeventsRegistrationReportingService.GenerateRegistrationReportAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var response = new ApiPerformance.SignalReviewWorkflowResponse
        {
            Review = new ApiPerformance.Review
            {
                Notes = (request.Review?.Notes ?? ""),
                ReviewId = (request.ReviewId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Review?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "review",
            "audit", "SignalReviewWorkflow", (request.Review?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Starts a review.
    /// </summary>
    public override async Task<ApiPerformance.StartReviewWorkflowResponse> StartReviewWorkflow(ApiPerformance.StartReviewWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling StartReviewWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Review is { } record)
        {
            var snapshot = ReviewMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = request.StartReviewWorkflowToPreviewVenue0();
        using var rpc0 = _venuesVenueValidationService.PreviewVenueAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiPerformance.StartReviewWorkflowResponse
        {
            Review = new ApiPerformance.Review
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Venue?.Audit?.CreatedAt ?? new Timestamp()),
                },
                IssuedAt = (request.Review?.IssuedAt ?? new Timestamp()),
                ReviewId = (request.ReviewId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Review?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "review",
            "audit", "StartReviewWorkflow", (request.Review?.VoucherCode ?? ""), token);
        return response;
    }
}
