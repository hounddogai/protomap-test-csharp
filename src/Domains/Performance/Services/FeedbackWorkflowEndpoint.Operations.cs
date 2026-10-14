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
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiGiftcards = Acme.Giftcards.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Performance;

public sealed partial class FeedbackWorkflowEndpoint
{
    /// <summary>
    /// Advances a feedback.
    /// </summary>
    public override async Task<ApiPerformance.AdvanceFeedbackWorkflowResponse> AdvanceFeedbackWorkflow(ApiPerformance.AdvanceFeedbackWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling AdvanceFeedbackWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Feedback is { } record)
        {
            var snapshot = FeedbackMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.AdvanceManagedDeviceWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.AdvanceManagedDeviceWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                    Phone = (request.Details?.Notify?.Phone ?? ""),
                    PreferredChannel = (request.Details?.Notify?.PreferredChannel ?? ""),
                },
            },
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Feedback?.Subject?.Email ?? ""),
                },
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiDevices.AdvanceManagedDeviceWorkflowRequest>(json0);
        using var rpc0 = _devicesManagedDeviceWorkflowService.AdvanceManagedDeviceWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiPerformance.AdvanceFeedbackWorkflowResponse
        {
            Feedback = new ApiPerformance.Feedback
            {
                CorrelationId = (request.Feedback?.CorrelationId ?? ""),
                FeedbackId = (request.FeedbackId ?? ""),
                Measurements = { new ApiPerformance.FeedbackMeasurement
                {
                    Unit = (reply0.ManagedDevice?.Dimensions?.Unit ?? ""),
                } },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "feedback",
            "audit", "AdvanceFeedbackWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Cancels a feedback.
    /// </summary>
    public override async Task<ApiPerformance.CancelFeedbackWorkflowResponse> CancelFeedbackWorkflow(ApiPerformance.CancelFeedbackWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CancelFeedbackWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Feedback is { } record)
        {
            var snapshot = FeedbackMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.RecalculateCandidatesRequest();
        input0.Candidate = new ApiRecruiting.Candidate
        {
            RelationshipManager = new ApiIdentity.UserRef
            {
                Email = (request.Feedback?.Subject?.Email ?? ""),
            },
        };
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        using var rpc0 = _recruitingCandidateCalculationService.RecalculateCandidatesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.RecalculateInterviewsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Interview = new ApiRecruiting.Interview
            {
                Participants = { new ApiRecruiting.InterviewParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Name = (reply0.Candidate?.Contact?.Name ?? new ApiCommon.PersonName()),
                    },
                } },
            },
        };
        using var rpc1 = _recruitingInterviewCalculationService.RecalculateInterviewsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiRecruiting.ValidateJobOpeningRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.ValidateJobOpeningRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Feedback?.Subject?.Email ?? ""),
                },
            },
            JobOpening = new ApiRecruiting.JobOpening
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.Candidate?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (reply1.Interview?.Location?.Locality ?? ""),
                },
                Counterparty = new ApiRecruiting.InterviewRef
                {
                    DisplayName = (request.Feedback?.Subject?.DisplayName ?? ""),
                },
            },
        };
        var reply2 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _recruitingJobOpeningValidationService.ValidateJobOpeningAsync(input2, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input3 = new ApiIdentity.ScoreUserRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiIdentity.ScoreUserRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (reply2.JobOpening?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
            User = new ApiIdentity.User
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Feedback?.Subject?.Email ?? ""),
                },
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply2.JobOpening?.BillingAddress?.PostalCode ?? ""),
                },
            },
        };
        var reply3 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _identityUserValidationService.ScoreUserAsync(input3, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        await _bus.PublishAsync(new TransportNotification<ApiIdentity.ScoreUserRequest>("acme.performance.v1.FeedbackWorkflowService/CancelFeedbackWorkflow/3", input3.Clone()), token);
        var response = new ApiPerformance.CancelFeedbackWorkflowResponse
        {
            Feedback = new ApiPerformance.Feedback
            {
                FeedbackId = (request.FeedbackId ?? ""),
                Sequence = (request.Feedback?.Sequence ?? 0L),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (reply0.Candidate?.RelationshipManager?.DisplayName ?? ""),
                    Email = (reply0.Candidate?.RelationshipManager?.Email ?? ""),
                },
            },
            Outcome = new ApiPerformance.CancelFeedbackWorkflowResponse.Types.Outcome
            {
                NextSteps = { (reply2.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply2.Outcome?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "feedback",
            "audit", "CancelFeedbackWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a feedback.
    /// </summary>
    public override async Task<ApiPerformance.InspectFeedbackWorkflowResponse> InspectFeedbackWorkflow(ApiPerformance.InspectFeedbackWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.FeedbackId, "feedback_id");
        _logger.LogInformation("Handling InspectFeedbackWorkflow for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "feedback", request.FeedbackId,
            "InspectFeedbackWorkflow", "accepted", token);
        var response = new ApiPerformance.InspectFeedbackWorkflowResponse
        {
            Feedback = new ApiPerformance.Feedback
            {
                FeedbackId = (request.FeedbackId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists feedbacks a page at a time.
    /// </summary>
    public override async Task<ApiPerformance.ListFeedbackWorkflowsResponse> ListFeedbackWorkflows(ApiPerformance.ListFeedbackWorkflowsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListFeedbackWorkflows for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "feedback", "batch",
            "ListFeedbackWorkflows", "accepted", token);
        var response = new ApiPerformance.ListFeedbackWorkflowsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "feedback",
            "audit", "ListFeedbackWorkflows", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pauses a feedback.
    /// </summary>
    public override async Task<ApiPerformance.PauseFeedbackWorkflowResponse> PauseFeedbackWorkflow(ApiPerformance.PauseFeedbackWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PauseFeedbackWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Feedback is { } record)
        {
            var snapshot = FeedbackMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.ScoreJobOpeningRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            JobOpening = new ApiRecruiting.JobOpening
            {
                Counterparty = new ApiRecruiting.InterviewRef
                {
                    DisplayName = (request.Feedback?.Subject?.DisplayName ?? ""),
                },
                Lines = { new ApiRecruiting.JobOpeningLine
                {
                    Attributes = { (request.Feedback?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                } },
            },
            Reason = (request.Reason ?? ""),
        };
        using var rpc0 = _recruitingJobOpeningValidationService.ScoreJobOpeningAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.GetCandidateRatesRequest
        {
            CandidateId = (request.Feedback?.Subject?.CandidateId ?? ""),
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc1 = _recruitingCandidateCalculationService.GetCandidateRatesAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var response = new ApiPerformance.PauseFeedbackWorkflowResponse
        {
            Feedback = new ApiPerformance.Feedback
            {
                FeedbackId = (request.FeedbackId ?? ""),
                Kind = (request.Feedback?.Kind ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Feedback?.Subject?.Email ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "feedback",
            "audit", "PauseFeedbackWorkflow", (request.Feedback?.Subject?.Email ?? ""), token);
        return response;
    }
    /// <summary>
    /// Resumes a feedback.
    /// </summary>
    public override async Task<ApiPerformance.ResumeFeedbackWorkflowResponse> ResumeFeedbackWorkflow(ApiPerformance.ResumeFeedbackWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ResumeFeedbackWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Feedback is { } record)
        {
            var snapshot = FeedbackMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiPerformance.SummarizeReviewsRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiPerformance.SummarizeReviewsRequest.Types.Details
            {
                Filters = { (request.Feedback?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Reason = (request.Reason ?? ""),
        };
        var named0 = _registry.Get<ApiPerformance.ReviewQueryService.ReviewQueryServiceClient>("acme.performance.v1.ReviewQueryService");
        using var rpc0 = named0.SummarizeReviewsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiGiftcards.WatchGiftCardsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Subscriber = new ApiCommon.ContactInfo
            {
                Name = (request.Details?.Notify?.Name ?? new ApiCommon.PersonName()),
                Phone = (request.Details?.Notify?.Phone ?? ""),
            },
        };
        using var stream1 = _giftcardsGiftCardService.WatchGiftCards(input1, cancellationToken: token);
        await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received WatchGiftCards frame {Type}", change1.GetType().Name);
        }
        var input2 = new ApiRecruiting.CompareInterviewOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Interview = new ApiRecruiting.Interview
            {
                Participants = { new ApiRecruiting.InterviewParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Name = (request.Details?.Notify?.Name ?? new ApiCommon.PersonName()),
                        PreferredChannel = (request.Details?.Notify?.PreferredChannel ?? ""),
                    },
                } },
            },
        };
        var command2 = new DispatchCommand<ApiRecruiting.CompareInterviewOptionsRequest>(input2,
            "primary", requestContext.TenantId);
        var routed2 = command2 with { Region = _settings.Value.Region };
        using var rpc2 = _recruitingInterviewCalculationService.CompareInterviewOptionsAsync(routed2.Payload, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiPerformance.ResumeFeedbackWorkflowResponse
        {
            Feedback = new ApiPerformance.Feedback
            {
                FeedbackId = (request.FeedbackId ?? ""),
                Source = (request.Feedback?.Source ?? ""),
            },
            Outcome = new ApiPerformance.ResumeFeedbackWorkflowResponse.Types.Outcome
            {
                Message = (reply2.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Feedback?.Subject?.Email ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "feedback",
            "audit", "ResumeFeedbackWorkflow", (request.Feedback?.Subject?.Email ?? ""), token);
        return response;
    }
    /// <summary>
    /// Retrys a feedback.
    /// </summary>
    public override async Task<ApiPerformance.RetryFeedbackWorkflowResponse> RetryFeedbackWorkflow(ApiPerformance.RetryFeedbackWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RetryFeedbackWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Feedback is { } record)
        {
            var snapshot = FeedbackMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.RecordSignatureVersionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.RecordSignatureVersionRequest.Types.Details
            {
                Snapshot = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Signature = new ApiDocuments.Signature
            {
                Attributes = { (request.Feedback?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Kind = (request.Feedback?.Kind ?? ""),
                Subject = new ApiDocuments.DocumentRef
                {
                    DisplayName = (request.Feedback?.Subject?.DisplayName ?? ""),
                },
            },
        };
        using var rpc0 = _documentsSignatureHistoryService.RecordSignatureVersionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiPerformance.RetryFeedbackWorkflowResponse
        {
            Feedback = new ApiPerformance.Feedback
            {
                FeedbackId = (request.FeedbackId ?? ""),
                Sequence = (reply0.Signature?.Sequence ?? 0L),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (request.Feedback?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "feedback",
            "audit", "RetryFeedbackWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Signals a feedback.
    /// </summary>
    public override async Task<ApiPerformance.SignalFeedbackWorkflowResponse> SignalFeedbackWorkflow(ApiPerformance.SignalFeedbackWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SignalFeedbackWorkflow for {Tenant}", requestContext.TenantId);
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
                    Name = (request.Details?.Notify?.Name ?? new ApiCommon.PersonName()),
                },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _performanceReviewWorkflowService.ResumeReviewWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = request.SignalFeedbackWorkflowToPreviewCandidate1(reply0);
        using var rpc1 = _recruitingCandidateValidationService.PreviewCandidateAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiPerformance.SignalFeedbackWorkflowResponse
        {
            Feedback = new ApiPerformance.Feedback
            {
                FeedbackId = (request.FeedbackId ?? ""),
                Subject = (request.Feedback?.Subject ?? new ApiRecruiting.CandidateRef()),
            },
            Outcome = new ApiPerformance.SignalFeedbackWorkflowResponse.Types.Outcome
            {
                EffectiveAt = (reply1.Outcome?.EffectiveAt ?? new Timestamp()),
                Message = (reply0.Outcome?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "feedback",
            "audit", "SignalFeedbackWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Starts a feedback.
    /// </summary>
    public override async Task<ApiPerformance.StartFeedbackWorkflowResponse> StartFeedbackWorkflow(ApiPerformance.StartFeedbackWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling StartFeedbackWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Feedback is { } record)
        {
            var snapshot = FeedbackMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiPerformance.BrowseGoalsResponse();
        foreach (var item in (request.Feedback?.Measurements ?? new Google.Protobuf.Collections.RepeatedField<ApiPerformance.FeedbackMeasurement>()))
        {
            var input0 = new ApiPerformance.BrowseGoalsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiPerformance.BrowseGoalsRequest.Types.Filter
                {
                    CreatedAfter = (item.ObservedAt ?? new Timestamp()),
                    Labels = { (request.Feedback?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                    OwnerId = (request.FeedbackId ?? ""),
                },
            };
            using var rpc0 = _performanceGoalQueryService.BrowseGoalsAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var response = new ApiPerformance.StartFeedbackWorkflowResponse
        {
            Feedback = new ApiPerformance.Feedback
            {
                CorrelationId = (request.Feedback?.CorrelationId ?? ""),
                FeedbackId = (request.FeedbackId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "feedback",
            "audit", "StartFeedbackWorkflow", (request.Reason ?? ""), token);
        return response;
    }
}
