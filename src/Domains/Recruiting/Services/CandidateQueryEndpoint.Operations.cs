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
using ApiCareplans = Acme.Careplans.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Recruiting;

public sealed partial class CandidateQueryEndpoint
{
    /// <summary>
    /// Lists candidates a page at a time.
    /// </summary>
    public override async Task<ApiRecruiting.BrowseCandidatesResponse> BrowseCandidates(ApiRecruiting.BrowseCandidatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling BrowseCandidates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "candidate", "batch",
            "BrowseCandidates", "accepted", token);
        var input0 = new ApiDocuments.CancelDocumentTemplateWorkflowRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Details = new ApiDocuments.CancelDocumentTemplateWorkflowRequest.Types.Details
        {
            Notify = new ApiCommon.ContactInfo
            {
                Email = (request.Context?.Actor?.Email ?? ""),
            },
        };
        input0.DocumentTemplate = new ApiDocuments.DocumentTemplate
        {
            Audit = new ApiCommon.AuditInfo
            {
                UpdatedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
            },
            BillingAddress = new ApiCommon.PostalAddress
            {
                Region = (request.Filter?.Region ?? ""),
            },
            Lines = { new ApiDocuments.DocumentTemplateLine
            {
                Attributes = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            } },
        };
        using var rpc0 = _documentsDocumentTemplateWorkflowService.CancelDocumentTemplateWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiRecruiting.BrowseCandidatesResponse
        {
            Candidates = { new ApiRecruiting.Candidate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.DocumentTemplate?.Audit?.Etag ?? ""),
                },
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (reply0.DocumentTemplate?.BillingAddress?.Recipient ?? ""),
                },
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "candidate",
            "audit", "BrowseCandidates", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Counts candidates by status.
    /// </summary>
    public override async Task<ApiRecruiting.CountCandidatesResponse> CountCandidates(ApiRecruiting.CountCandidatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CountCandidates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "candidate", "batch",
            "CountCandidates", "accepted", token);
        var response = new ApiRecruiting.CountCandidatesResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "candidate",
            "audit", "CountCandidates", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams every matching candidate.
    /// </summary>
    public override async Task ExportCandidates(ApiRecruiting.ExportCandidatesRequest request, IServerStreamWriter<ApiRecruiting.ExportCandidatesResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExportCandidates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "candidate", "batch",
            "ExportCandidates", "accepted", token);
        var input0 = new ApiOnboarding.RecalculateOnboardingTasksRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.RecalculateOnboardingTasksRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                Assignee = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _onboardingOnboardingTaskCalculationService.RecalculateOnboardingTasksAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.CheckJobOpeningEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.CheckJobOpeningEligibilityRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (reply0.OnboardingTask?.Requester?.PreferredChannel ?? ""),
                },
            },
            JobOpening = new ApiRecruiting.JobOpening
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.OnboardingTask?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
        };
        var reply1 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _recruitingJobOpeningValidationService.CheckJobOpeningEligibilityAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input2 = new ApiRecruiting.ExplainInterviewCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Interview = new ApiRecruiting.Interview
            {
                Participants = { new ApiRecruiting.InterviewParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (reply0.OnboardingTask?.Requester?.Email ?? ""),
                    },
                } },
            },
        };
        var reply2 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _recruitingInterviewCalculationService.ExplainInterviewCalculationAsync(input2, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiRecruiting.ExportCandidatesResponse
        {
            Candidate = new ApiRecruiting.Candidate
            {
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    Email = (reply0.OnboardingTask?.Requester?.Email ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
                Message = (reply2.Outcome?.Message ?? ""),
                Status = (reply1.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Format ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "candidate",
            "audit", "ExportCandidates", (request.Format ?? ""), token);
        await responses.WriteAsync(response, token);
    }
    /// <summary>
    /// Lists candidates a page at a time.
    /// </summary>
    public override async Task<ApiRecruiting.FindCandidatesResponse> FindCandidates(ApiRecruiting.FindCandidatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling FindCandidates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "candidate", "batch",
            "FindCandidates", "accepted", token);
        var response = new ApiRecruiting.FindCandidatesResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "candidate",
            "audit", "FindCandidates", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a candidate.
    /// </summary>
    public override async Task<ApiRecruiting.LookupCandidateResponse> LookupCandidate(ApiRecruiting.LookupCandidateRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.CandidateId, "candidate_id");
        _logger.LogInformation("Handling LookupCandidate for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "candidate", request.CandidateId,
            "LookupCandidate", "accepted", token);
        var input0 = new ApiLearning.SummarizeCoursesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLearning.SummarizeCoursesRequest.Types.Details
            {
                Filters = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _learningCourseQueryService.SummarizeCoursesAsync);
        var input1 = new ApiCareplans.ListCarePlansRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiCareplans.ListCarePlansRequest.Types.Filter
            {
                OwnerId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        using var rpc1 = _sendLookupCandidate1(input1, new CallOptions(cancellationToken: token));
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.LookupCandidateResponse
        {
            Candidate = new ApiRecruiting.Candidate
            {
                CandidateId = (request.CandidateId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        return response;
    }
    /// <summary>
    /// Returns a candidate.
    /// </summary>
    public override async Task<ApiRecruiting.PeekCandidateResponse> PeekCandidate(ApiRecruiting.PeekCandidateRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.CandidateId, "candidate_id");
        _logger.LogInformation("Handling PeekCandidate for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "candidate", request.CandidateId,
            "PeekCandidate", "accepted", token);
        var input0 = new ApiMetrics.SuggestMetricSeriesesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiMetrics.SuggestMetricSeriesesRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
                OwnerId = (request.CandidateId ?? ""),
            },
        };
        var named0 = _registry.Get<ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient>("acme.metrics.v1.MetricSeriesQueryService");
        using var rpc0 = named0.SuggestMetricSeriesesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiFeatureflags.LintFeatureFlagsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiFeatureflags.LintFeatureFlagsRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            FeatureFlag = new ApiFeatureflags.FeatureFlag
            {
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        var descriptor1 = new Method<ApiFeatureflags.LintFeatureFlagsRequest, ApiFeatureflags.LintFeatureFlagsResponse>(
            MethodType.Unary, "acme.featureflags.v1" + ".FeatureFlagValidationService", "LintFeatureFlags",
            Wire.Marshaller<ApiFeatureflags.LintFeatureFlagsRequest>(), Wire.Marshaller<ApiFeatureflags.LintFeatureFlagsResponse>());
        using var rpc1 = _invoker.AsyncUnaryCall(descriptor1, null,
            new CallOptions(cancellationToken: token), input1);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.PeekCandidateResponse
        {
            Candidate = new ApiRecruiting.Candidate
            {
                CandidateId = (request.CandidateId ?? ""),
                RelationshipManager = new ApiIdentity.UserRef
                {
                    UserId = (reply1.FeatureFlag?.Owner?.UserId ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Returns several candidates at once.
    /// </summary>
    public override async Task<ApiRecruiting.ResolveCandidatesResponse> ResolveCandidates(ApiRecruiting.ResolveCandidatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ResolveCandidates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "candidate", "batch",
            "ResolveCandidates", "accepted", token);
        var input0 = new ApiRecruiting.ExplainCandidateCalculationRequest
        {
            Candidate = new ApiRecruiting.Candidate
            {
                RelationshipManager = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var command0 = new DispatchCommand<ApiRecruiting.ExplainCandidateCalculationRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _recruitingCandidateCalculationService.ExplainCandidateCalculationAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiRecruiting.ResolveCandidatesResponse
        {
            Candidates = { new ApiRecruiting.Candidate
            {
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply0.Candidate?.PrimaryAddress?.PostalCode ?? ""),
                },
            } },
        };
        return response;
    }
    /// <summary>
    /// Lists candidates a page at a time.
    /// </summary>
    public override async Task<ApiRecruiting.SuggestCandidatesResponse> SuggestCandidates(ApiRecruiting.SuggestCandidatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SuggestCandidates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "candidate", "batch",
            "SuggestCandidates", "accepted", token);
        var profile0 = new MappingProfile<ApiRecruiting.SuggestCandidatesRequest, ApiRecruiting.EstimateCandidateRequest>(value => new ApiRecruiting.EstimateCandidateRequest
        {
            Candidate = new ApiRecruiting.Candidate
            {
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    Region = (value.Filter?.Region ?? ""),
                },
            },
            Context = (value.Context ?? new ApiCommon.RequestContext()),
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _recruitingCandidateCalculationService.EstimateCandidateAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiIdentity.ExportUsersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiIdentity.ExportUsersRequest.Types.Filter
            {
                CreatedAfter = (request.Filter?.CreatedAfter ?? new Timestamp()),
            },
        };
        using var stream1 = _identityUserQueryService.ExportUsers(input1, cancellationToken: token);
        await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportUsers frame {Type}", change1.GetType().Name);
        }
        var response = new ApiRecruiting.SuggestCandidatesResponse
        {
            Candidates = { new ApiRecruiting.Candidate
            {
                RelationshipManager = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Candidate?.RelationshipManager?.DisplayName ?? ""),
                },
                Segments = { (reply0.Candidate?.Segments ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "candidate",
            "audit", "SuggestCandidates", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Summarizes a candidate.
    /// </summary>
    public override async Task<ApiRecruiting.SummarizeCandidatesResponse> SummarizeCandidates(ApiRecruiting.SummarizeCandidatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SummarizeCandidates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "candidate", request.CandidateId,
            "SummarizeCandidates", "accepted", token);
        var input0 = request.SummarizeCandidatesToGetCandidateRules0();
        using var rpc0 = _recruitingCandidateValidationService.GetCandidateRulesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiRecruiting.SummarizeCandidatesResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "candidate",
            "audit", "SummarizeCandidates", (request.Reason ?? ""), token);
        return response;
    }
}
