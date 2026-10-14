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
using ApiLearning = Acme.Learning.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSearch = Acme.Search.V1;

namespace Acme.Workforce.Recruiting;

public sealed partial class CandidateLifecycleEndpoint
{
    /// <summary>
    /// Activates a candidate.
    /// </summary>
    public override async Task<ApiRecruiting.ActivateCandidateResponse> ActivateCandidate(ApiRecruiting.ActivateCandidateRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.CandidateId, "candidate_id");
        _logger.LogInformation("Handling ActivateCandidate for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "candidate", request.CandidateId,
            "ActivateCandidate", "accepted", token);
        var input0 = new ApiRecruiting.ListInterviewReportsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiRecruiting.ListInterviewReportsRequest.Types.Filter
            {
                CreatedAfter = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
        };
        using var rpc0 = _recruitingInterviewReportingService.ListInterviewReportsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiRecruiting.ActivateCandidateResponse
        {
            Candidate = new ApiRecruiting.Candidate
            {
                CandidateId = (request.CandidateId ?? ""),
            },
            Outcome = new ApiRecruiting.ActivateCandidateResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "candidate",
            "audit", "ActivateCandidate", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Expires a candidate.
    /// </summary>
    public override async Task<ApiRecruiting.ExpireCandidateResponse> ExpireCandidate(ApiRecruiting.ExpireCandidateRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.CandidateId, "candidate_id");
        _logger.LogInformation("Handling ExpireCandidate for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "candidate", request.CandidateId,
            "ExpireCandidate", "accepted", token);
        var input0 = new ApiRecruiting.GetCandidateRatesRequest
        {
            CandidateId = (request.CandidateId ?? ""),
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _recruitingCandidateCalculationService.GetCandidateRatesAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiRecruiting.ExpireCandidateResponse
        {
            Candidate = new ApiRecruiting.Candidate
            {
                CandidateId = (request.CandidateId ?? ""),
                RelationshipManager = new ApiIdentity.UserRef
                {
                    Email = (reply0.Candidate?.RelationshipManager?.Email ?? ""),
                },
            },
            Outcome = new ApiRecruiting.ExpireCandidateResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "candidate",
            "audit", "ExpireCandidate", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Reinstates a candidate.
    /// </summary>
    public override async Task<ApiRecruiting.ReinstateCandidateResponse> ReinstateCandidate(ApiRecruiting.ReinstateCandidateRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.CandidateId, "candidate_id");
        _logger.LogInformation("Handling ReinstateCandidate for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "candidate", request.CandidateId,
            "ReinstateCandidate", "accepted", token);
        var input0 = new ApiLedger.SearchLedgerAccountsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLedger.SearchLedgerAccountsRequest.Types.Filter
            {
                CreatedAfter = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _ledgerLedgerAccountService.SearchLedgerAccountsAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiRecruiting.ListJobOpeningsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiRecruiting.ListJobOpeningsRequest.Types.Filter
            {
                CreatedAfter = (request.Details?.EffectiveAt ?? new Timestamp()),
                Labels = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Details?.NewOwner?.UserId ?? ""),
            },
        };
        var reply1 = await Workflow.SendAsync(input1, token, _recruitingJobOpeningService.ListJobOpeningsAsync);
        var response = new ApiRecruiting.ReinstateCandidateResponse
        {
            Candidate = new ApiRecruiting.Candidate
            {
                CandidateId = (request.CandidateId ?? ""),
            },
            Outcome = new ApiRecruiting.ReinstateCandidateResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "candidate",
            "audit", "ReinstateCandidate", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Renews a candidate.
    /// </summary>
    public override async Task<ApiRecruiting.RenewCandidateResponse> RenewCandidate(ApiRecruiting.RenewCandidateRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.CandidateId, "candidate_id");
        _logger.LogInformation("Handling RenewCandidate for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "candidate", request.CandidateId,
            "RenewCandidate", "accepted", token);
        var input0 = new ApiNotifications.ResolveNotificationsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _sendRenewCandidate0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.RetryDocumentTemplateExportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.RetryDocumentTemplateExportRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                    Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                    PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                },
            },
        };
        var named1 = _registry.Get<ApiDocuments.DocumentTemplateExportService.DocumentTemplateExportServiceClient>("acme.documents.v1.DocumentTemplateExportService");
        using var rpc1 = named1.RetryDocumentTemplateExportAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiLearning.RecallCertificationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLearning.RecallCertificationRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                },
            },
        };
        var descriptor2 = new Method<ApiLearning.RecallCertificationRequest, ApiLearning.RecallCertificationResponse>(
            MethodType.Unary, "acme.learning.v1" + ".CertificationApprovalService", "RecallCertification",
            Wire.Marshaller<ApiLearning.RecallCertificationRequest>(), Wire.Marshaller<ApiLearning.RecallCertificationResponse>());
        using var rpc2 = _invoker.AsyncUnaryCall(descriptor2, null,
            new CallOptions(cancellationToken: token), input2);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiRecruiting.RenewCandidateResponse
        {
            Candidate = new ApiRecruiting.Candidate
            {
                CandidateId = (request.CandidateId ?? ""),
                RelationshipManager = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.DocumentTemplate?.Counterparty?.DisplayName ?? ""),
                },
            },
            Outcome = new ApiRecruiting.RenewCandidateResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply1.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "candidate",
            "audit", "RenewCandidate", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Schedules a candidate.
    /// </summary>
    public override async Task<ApiRecruiting.ScheduleCandidateExpiryResponse> ScheduleCandidateExpiry(ApiRecruiting.ScheduleCandidateExpiryRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScheduleCandidateExpiry for {Tenant}", requestContext.TenantId);
        if (request.Candidate is { } record)
        {
            var snapshot = CandidateMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.ShareCandidateReportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.ShareCandidateReportRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                    Name = (request.Candidate?.Contact?.Name ?? new ApiCommon.PersonName()),
                },
            },
        };
        using var rpc0 = _recruitingCandidateReportingService.ShareCandidateReportAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var profile1 = new MappingProfile<ApiRecruiting.ScheduleCandidateExpiryRequest, ApiSearch.BatchGetSearchIndexesRequest>(value => new ApiSearch.BatchGetSearchIndexesRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _searchSearchIndexService.BatchGetSearchIndexesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDocuments.ExplainDocumentTemplateCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                Counterparty = new ApiDocuments.DocumentFolderRef
                {
                    DisplayName = (reply0.Candidate?.RelationshipManager?.DisplayName ?? ""),
                },
            },
        };
        using var rpc2 = _documentsDocumentTemplateCalculationService.ExplainDocumentTemplateCalculationAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        var input3 = new ApiOnboarding.SubmitOnboardingTaskRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Candidate?.RelationshipManager?.DisplayName ?? ""),
                    Email = (reply0.Candidate?.RelationshipManager?.Email ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Details?.Notify?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc3 = _onboardingOnboardingTaskApprovalService.SubmitOnboardingTaskAsync(input3, cancellationToken: token);
        var pending3 = rpc3.ResponseAsync;
        await Task.WhenAll(pending2, pending3);
        var reply2 = await pending2;
        var reply3 = await pending3;
        var response = new ApiRecruiting.ScheduleCandidateExpiryResponse
        {
            Candidate = new ApiRecruiting.Candidate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply3.OnboardingTask?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedAt = (reply0.Candidate?.Audit?.UpdatedAt ?? new Timestamp()),
                    UpdatedBy = (reply2.DocumentTemplate?.Audit?.UpdatedBy ?? ""),
                },
                CandidateId = (request.CandidateId ?? ""),
                Contact = (request.Candidate?.Contact ?? new ApiCommon.ContactInfo()),
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (reply0.Candidate?.PrimaryAddress?.Recipient ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply3.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "candidate",
            "audit", "ScheduleCandidateExpiry", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Suspends a candidate.
    /// </summary>
    public override async Task<ApiRecruiting.SuspendCandidateResponse> SuspendCandidate(ApiRecruiting.SuspendCandidateRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.CandidateId, "candidate_id");
        _logger.LogInformation("Handling SuspendCandidate for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "candidate", request.CandidateId,
            "SuspendCandidate", "accepted", token);
        var input0 = new ApiRecruiting.GenerateInterviewReportRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Reason ?? ""),
        };
        var reply0 = await SendSuspendCandidate0Async(input0, token);
        var input1 = new ApiDocuments.PullDocumentTemplatesRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.PullDocumentTemplatesRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                Counterparty = new ApiDocuments.DocumentFolderRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                },
            },
        };
        using var rpc1 = _documentsDocumentTemplateSyncService.PullDocumentTemplatesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.SuspendCandidateResponse
        {
            Candidate = new ApiRecruiting.Candidate
            {
                CandidateId = (request.CandidateId ?? ""),
            },
            Outcome = new ApiRecruiting.SuspendCandidateResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                NextSteps = { (reply1.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.ReasonCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "candidate",
            "audit", "SuspendCandidate", (request.Details?.ReasonCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Terminates a candidate.
    /// </summary>
    public override async Task<ApiRecruiting.TerminateCandidateResponse> TerminateCandidate(ApiRecruiting.TerminateCandidateRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.CandidateId, "candidate_id");
        _logger.LogInformation("Handling TerminateCandidate for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "candidate", request.CandidateId,
            "TerminateCandidate", "accepted", token);
        var input0 = new ApiRecruiting.ScheduleJobOpeningExpiryRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.ScheduleJobOpeningExpiryRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                    Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                    PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                },
            },
            JobOpening = new ApiRecruiting.JobOpening
            {
                Counterparty = new ApiRecruiting.InterviewRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiRecruiting.ScheduleJobOpeningExpiryRequest>();
        using var rpc0 = _recruitingJobOpeningLifecycleService.ScheduleJobOpeningExpiryAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiMetrics.ScoreMetricSeriesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiMetrics.ScoreMetricSeriesRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (reply0.JobOpening?.BillingAddress?.Region ?? ""),
                },
            },
        };
        var json1 = JsonFormatter.Default.Format(input1);
        input1 = JsonParser.Default.Parse<ApiMetrics.ScoreMetricSeriesRequest>(json1);
        using var rpc1 = _metricsMetricSeriesValidationService.ScoreMetricSeriesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.TerminateCandidateResponse
        {
            Candidate = new ApiRecruiting.Candidate
            {
                CandidateId = (request.CandidateId ?? ""),
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (reply1.MetricSeries?.Address?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    PostalCode = (reply0.JobOpening?.BillingAddress?.PostalCode ?? ""),
                },
            },
            Outcome = new ApiRecruiting.TerminateCandidateResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "candidate",
            "audit", "TerminateCandidate", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiRecruiting.GenerateInterviewReportResponse> SendSuspendCandidate0Async(ApiRecruiting.GenerateInterviewReportRequest request, CancellationToken token)
    {
        return await _candidateLifecycleEndpointSendSuspendCandidate0Port.SendAsync(request, token);
    }
}
