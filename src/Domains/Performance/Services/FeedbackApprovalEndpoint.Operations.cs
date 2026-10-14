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
using ApiDrivers = Acme.Drivers.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiRouting = Acme.Routing.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Performance;

public sealed partial class FeedbackApprovalEndpoint
{
    /// <summary>
    /// Approves a feedback.
    /// </summary>
    public override async Task<ApiPerformance.ApproveFeedbackResponse> ApproveFeedback(ApiPerformance.ApproveFeedbackRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ApproveFeedback for {Tenant}", requestContext.TenantId);
        if (request.Feedback is { } record)
        {
            var snapshot = FeedbackMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiPerformance.ResumeReviewWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiPerformance.ResumeReviewWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
            Review = new ApiPerformance.Review
            {
                Counterparty = new ApiPerformance.GoalRef
                {
                    DisplayName = (request.Feedback?.Subject?.DisplayName ?? ""),
                },
                Lines = { new ApiPerformance.ReviewLine
                {
                    Attributes = { (request.Feedback?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                } },
            },
        };
        var command0 = new DispatchCommand<ApiPerformance.ResumeReviewWorkflowRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _performanceReviewWorkflowService.ResumeReviewWorkflowAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiPerformance.ApproveFeedbackResponse
        {
            Feedback = new ApiPerformance.Feedback
            {
                FeedbackId = (request.FeedbackId ?? ""),
                Kind = (request.Feedback?.Kind ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "feedback",
            "audit", "ApproveFeedback", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Comments a feedback.
    /// </summary>
    public override async Task<ApiPerformance.CommentFeedbackApprovalResponse> CommentFeedbackApproval(ApiPerformance.CommentFeedbackApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CommentFeedbackApproval for {Tenant}", requestContext.TenantId);
        if (request.Feedback is { } record)
        {
            var snapshot = FeedbackMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiPerformance.PreviewFeedbackRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiPerformance.PreviewFeedbackRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
            Feedback = new ApiPerformance.Feedback
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = (request.Feedback?.Subject ?? new ApiRecruiting.CandidateRef()),
            },
        };
        using var rpc0 = _performanceFeedbackValidationService.PreviewFeedbackAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiWorkplaces.CountRoomBookingTotalsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiWorkplaces.CountRoomBookingTotalsRequest.Types.Filter
            {
                CreatedAfter = (reply0.Outcome?.EffectiveAt ?? new Timestamp()),
                Labels = { (request.Feedback?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Feedback?.Subject?.CandidateId ?? ""),
            },
        };
        input1 = PropertyMap.Copy(input1);
        using var rpc1 = _workplacesRoomBookingReportingService.CountRoomBookingTotalsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiPerformance.CommentFeedbackApprovalResponse
        {
            Feedback = new ApiPerformance.Feedback
            {
                FeedbackId = (request.FeedbackId ?? ""),
                OccurredAt = (request.Feedback?.OccurredAt ?? new Timestamp()),
            },
            Outcome = new ApiPerformance.CommentFeedbackApprovalResponse.Types.Outcome
            {
                Status = (reply0.Outcome?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Outcome?.Message ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "feedback",
            "audit", "CommentFeedbackApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Delegates a feedback.
    /// </summary>
    public override async Task<ApiPerformance.DelegateFeedbackApprovalResponse> DelegateFeedbackApproval(ApiPerformance.DelegateFeedbackApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling DelegateFeedbackApproval for {Tenant}", requestContext.TenantId);
        if (request.Feedback is { } record)
        {
            var snapshot = FeedbackMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = request.DelegateFeedbackApprovalToPreviewCandidate0();
        using var rpc0 = _recruitingCandidateValidationService.PreviewCandidateAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiIdentity.CloneUserRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            User = new ApiIdentity.User
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Feedback?.Subject?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _identityUserService.CloneUserAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        var input2 = new ApiRecruiting.QuoteOfferRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.QuoteOfferRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Recipient = (reply0.Candidate?.PrimaryAddress?.Recipient ?? ""),
                },
            },
        };
        using var rpc2 = _recruitingOfferCalculationService.QuoteOfferAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        await Task.WhenAll(pending1, pending2);
        var reply1 = await pending1;
        var reply2 = await pending2;
        var input3 = new ApiTravel.CountTravelPolicyTotalsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiTravel.CountTravelPolicyTotalsRequest.Types.Filter
            {
                CreatedAfter = (reply2.Offer?.Audit?.UpdatedAt ?? new Timestamp()),
                OwnerId = (reply2.Result?.CorrelationId ?? ""),
                Region = (reply0.Candidate?.PrimaryAddress?.Region ?? ""),
            },
        };
        var packed3 = Any.Pack(input3);
        input3 = packed3.Unpack<ApiTravel.CountTravelPolicyTotalsRequest>();
        using var rpc3 = _travelTravelPolicyReportingService.CountTravelPolicyTotalsAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiPerformance.DelegateFeedbackApprovalResponse
        {
            Feedback = new ApiPerformance.Feedback
            {
                FeedbackId = (request.FeedbackId ?? ""),
                Sequence = (request.Feedback?.Sequence ?? 0L),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Details = { (reply3.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Outcome?.Message ?? ""),
                ReferenceIds = { (reply2.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "feedback",
            "audit", "DelegateFeedbackApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Escalates a feedback.
    /// </summary>
    public override async Task<ApiPerformance.EscalateFeedbackResponse> EscalateFeedback(ApiPerformance.EscalateFeedbackRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EscalateFeedback for {Tenant}", requestContext.TenantId);
        if (request.Feedback is { } record)
        {
            var snapshot = FeedbackMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var batch0 = (request.Feedback?.Measurements ?? new Google.Protobuf.Collections.RepeatedField<ApiPerformance.FeedbackMeasurement>()).Select(async item =>
        {
            var input0 = new ApiRecruiting.SimulateOfferRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Offer = new ApiRecruiting.Offer
                {
                    Counterparty = new ApiEmployees.EmployeeRef
                    {
                        Email = (request.Details?.Approver?.Email ?? ""),
                    },
                    IssuedAt = (item.ObservedAt ?? new Timestamp()),
                },
            };
            using var rpc0 = _recruitingOfferCalculationService.SimulateOfferAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            return reply0;
        }).ToArray();
        var replies0 = await Task.WhenAll(batch0);
        var reply0 = replies0.LastOrDefault() ?? new ApiRecruiting.SimulateOfferResponse();
        var response = new ApiPerformance.EscalateFeedbackResponse
        {
            Feedback = new ApiPerformance.Feedback
            {
                FeedbackId = (request.FeedbackId ?? ""),
            },
            Outcome = new ApiPerformance.EscalateFeedbackResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "feedback",
            "audit", "EscalateFeedback", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a feedback.
    /// </summary>
    public override async Task<ApiPerformance.GetFeedbackApprovalStatusResponse> GetFeedbackApprovalStatus(ApiPerformance.GetFeedbackApprovalStatusRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.FeedbackId, "feedback_id");
        _logger.LogInformation("Handling GetFeedbackApprovalStatus for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "feedback", request.FeedbackId,
            "GetFeedbackApprovalStatus", "accepted", token);
        var input0 = new ApiRecruiting.PreviewOfferRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Offer = new ApiRecruiting.Offer
        {
            Counterparty = new ApiEmployees.EmployeeRef
            {
                Email = (request.Context?.Actor?.Email ?? ""),
            },
        };
        using var rpc0 = _recruitingOfferValidationService.PreviewOfferAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiPerformance.GetFeedbackApprovalStatusResponse
        {
            Feedback = new ApiPerformance.Feedback
            {
                FeedbackId = (request.FeedbackId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists feedbacks a page at a time.
    /// </summary>
    public override async Task<ApiPerformance.ListPendingFeedbacksResponse> ListPendingFeedbacks(ApiPerformance.ListPendingFeedbacksRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListPendingFeedbacks for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "feedback", "batch",
            "ListPendingFeedbacks", "accepted", token);
        var input0 = new ApiPerformance.CreateReviewRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Review = new ApiPerformance.Review
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        using var rpc0 = _performanceReviewService.CreateReviewAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiPerformance.ListPendingFeedbacksResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "feedback",
            "audit", "ListPendingFeedbacks", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalls a feedback.
    /// </summary>
    public override async Task<ApiPerformance.RecallFeedbackResponse> RecallFeedback(ApiPerformance.RecallFeedbackRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecallFeedback for {Tenant}", requestContext.TenantId);
        if (request.Feedback is { } record)
        {
            var snapshot = FeedbackMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.ResolveDocumentFoldersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _documentsDocumentFolderQueryService.ResolveDocumentFoldersAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiPerformance.ApproveGoalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiPerformance.ApproveGoalRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    Email = (request.Feedback?.Subject?.Email ?? ""),
                },
            },
            Goal = new ApiPerformance.Goal
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
        };
        var reply1 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _performanceGoalApprovalService.ApproveGoalAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiPerformance.RecallFeedbackResponse
        {
            Feedback = new ApiPerformance.Feedback
            {
                Attributes = { (request.Feedback?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                FeedbackId = (request.FeedbackId ?? ""),
                Subject = new ApiRecruiting.CandidateRef
                {
                    Email = (reply1.Goal?.Requester?.Email ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "feedback",
            "audit", "RecallFeedback", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Rejects a feedback.
    /// </summary>
    public override async Task<ApiPerformance.RejectFeedbackResponse> RejectFeedback(ApiPerformance.RejectFeedbackRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RejectFeedback for {Tenant}", requestContext.TenantId);
        if (request.Feedback is { } record)
        {
            var snapshot = FeedbackMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRouting.CompareRouteOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Route = new ApiRouting.Route
            {
                Counterparty = new ApiDrivers.DriverRef
                {
                    DisplayName = (request.Feedback?.Subject?.DisplayName ?? ""),
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
                Lines = { new ApiRouting.RouteLine
                {
                    Attributes = { (request.Feedback?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                } },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _routingRouteCalculationService.CompareRouteOptionsAsync);
        var batch1 = (request.Feedback?.Measurements ?? new Google.Protobuf.Collections.RepeatedField<ApiPerformance.FeedbackMeasurement>()).Select(async item =>
        {
            var input1 = new ApiRecruiting.SearchJobOpeningsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiRecruiting.SearchJobOpeningsRequest.Types.Filter
                {
                    CreatedAfter = (item.ObservedAt ?? new Timestamp()),
                    OwnerId = (reply0.Route?.Counterparty?.DriverId ?? ""),
                    Region = (reply0.Route?.BillingAddress?.Region ?? ""),
                },
            };
            using var rpc1 = _recruitingJobOpeningService.SearchJobOpeningsAsync(input1, cancellationToken: token);
            var reply1 = await rpc1.ResponseAsync;
            return reply1;
        }).ToArray();
        var replies1 = await Task.WhenAll(batch1);
        var reply1 = replies1.LastOrDefault() ?? new ApiRecruiting.SearchJobOpeningsResponse();
        var input2 = new ApiPerformance.FindGoalsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiPerformance.FindGoalsRequest.Types.Filter
            {
                CreatedAfter = (reply0.Route?.Audit?.CreatedAt ?? new Timestamp()),
                OwnerId = (request.Details?.Approver?.UserId ?? ""),
                Region = (reply0.Route?.BillingAddress?.Region ?? ""),
            },
        };
        var named2 = _registry.Get<ApiPerformance.GoalQueryService.GoalQueryServiceClient>("acme.performance.v1.GoalQueryService");
        using var rpc2 = named2.FindGoalsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiPerformance.RejectFeedbackResponse
        {
            Feedback = new ApiPerformance.Feedback
            {
                CorrelationId = (request.Feedback?.CorrelationId ?? ""),
                FeedbackId = (request.FeedbackId ?? ""),
                Subject = new ApiRecruiting.CandidateRef
                {
                    Email = (reply0.Route?.Counterparty?.Email ?? ""),
                },
            },
            Outcome = new ApiPerformance.RejectFeedbackResponse.Types.Outcome
            {
                Message = (reply2.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply2.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "feedback",
            "audit", "RejectFeedback", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Submits a feedback.
    /// </summary>
    public override async Task<ApiPerformance.SubmitFeedbackResponse> SubmitFeedback(ApiPerformance.SubmitFeedbackRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SubmitFeedback for {Tenant}", requestContext.TenantId);
        if (request.Feedback is { } record)
        {
            var snapshot = FeedbackMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiPerformance.SubmitReviewRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiPerformance.SubmitReviewRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Feedback?.Subject?.DisplayName ?? ""),
                    Email = (request.Feedback?.Subject?.Email ?? ""),
                },
            },
        };
        var descriptor0 = new Method<ApiPerformance.SubmitReviewRequest, ApiPerformance.SubmitReviewResponse>(
            MethodType.Unary, "acme.performance.v1" + ".ReviewApprovalService", "SubmitReview",
            Wire.Marshaller<ApiPerformance.SubmitReviewRequest>(), Wire.Marshaller<ApiPerformance.SubmitReviewResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiPerformance.SubmitFeedbackResponse
        {
            Feedback = new ApiPerformance.Feedback
            {
                FeedbackId = (request.FeedbackId ?? ""),
                OccurredAt = (request.Feedback?.OccurredAt ?? new Timestamp()),
            },
            Outcome = new ApiPerformance.SubmitFeedbackResponse.Types.Outcome
            {
                Message = (reply0.Outcome?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "feedback",
            "audit", "SubmitFeedback", (request.Reason ?? ""), token);
        return response;
    }
}
