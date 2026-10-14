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
using ApiAudit = Acme.Audit.V1;
using ApiAuthorization = Acme.Authorization.V1;
using ApiBenefits = Acme.Benefits.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiWebhooks = Acme.Webhooks.V1;

namespace Acme.Workforce.Benefits;

public sealed partial class DependentValidationEndpoint
{
    /// <summary>
    /// Checks a dependent.
    /// </summary>
    public override async Task<ApiBenefits.CheckDependentEligibilityResponse> CheckDependentEligibility(ApiBenefits.CheckDependentEligibilityRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CheckDependentEligibility for {Tenant}", requestContext.TenantId);
        if (request.Dependent is { } record)
        {
            var snapshot = DependentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiBenefits.QuoteLifeEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiBenefits.QuoteLifeEventRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Dependent?.PrimaryAddress?.PostalCode ?? ""),
                    Region = (request.Dependent?.PrimaryAddress?.Region ?? ""),
                },
            },
            LifeEvent = new ApiBenefits.LifeEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _benefitsLifeEventCalculationService.QuoteLifeEventAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiBenefits.SimulateLifeEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiBenefits.SimulateLifeEventRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Locality = (request.Dependent?.PrimaryAddress?.Locality ?? ""),
                },
            },
            LifeEvent = new ApiBenefits.LifeEvent
            {
                Attributes = { (request.Dependent?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (request.Dependent?.RelationshipManager?.DisplayName ?? ""),
                    Email = (request.Dependent?.RelationshipManager?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _benefitsLifeEventCalculationService.SimulateLifeEventAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiBenefits.CompareLifeEventOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LifeEvent = new ApiBenefits.LifeEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (reply1.LifeEvent?.Subject?.DisplayName ?? ""),
                },
            },
        };
        using var rpc2 = _benefitsLifeEventCalculationService.CompareLifeEventOptionsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiBenefits.CheckDependentEligibilityResponse
        {
            Dependent = new ApiBenefits.Dependent
            {
                Attributes = { (reply2.LifeEvent?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                DependentId = (request.DependentId ?? ""),
                RelationshipManager = new ApiIdentity.UserRef
                {
                    DisplayName = (reply2.LifeEvent?.Subject?.DisplayName ?? ""),
                },
                Status = (request.Dependent?.Status ?? (ApiBenefits.DependentStatus)0),
            },
            Outcome = new ApiBenefits.CheckDependentEligibilityResponse.Types.Outcome
            {
                Message = (reply1.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.LifeEvent?.CorrelationId ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "dependent",
            "audit", "CheckDependentEligibility", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a dependent.
    /// </summary>
    public override async Task<ApiBenefits.ExplainDependentDecisionResponse> ExplainDependentDecision(ApiBenefits.ExplainDependentDecisionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExplainDependentDecision for {Tenant}", requestContext.TenantId);
        if (request.Dependent is { } record)
        {
            var snapshot = DependentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLedger.ExportLedgerAccountsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLedger.ExportLedgerAccountsRequest.Types.Filter
            {
                CreatedAfter = (request.Dependent?.Audit?.UpdatedAt ?? new Timestamp()),
                Region = (request.Dependent?.PrimaryAddress?.Region ?? ""),
            },
        };
        using var stream0 = _ledgerLedgerAccountQueryService.ExportLedgerAccounts(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportLedgerAccounts frame {Type}", change0.GetType().Name);
        }
        var input1 = new ApiLearning.SuggestCoursesRequest();
        input1.Context = (request.Context ?? new ApiCommon.RequestContext());
        input1.Filter = new ApiLearning.SuggestCoursesRequest.Types.Filter
        {
            Region = (request.Dependent?.PrimaryAddress?.Region ?? ""),
        };
        using var rpc1 = _learningCourseQueryService.SuggestCoursesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiLedger.SummarizeLedgerAccountsRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLedger.SummarizeLedgerAccountsRequest.Types.Details
            {
                Filters = { (request.Dependent?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Period = (request.Dependent?.Preferences?.QuietHours ?? new ApiCommon.TimeWindow()),
            },
            Reason = (request.Reason ?? ""),
        };
        using var rpc2 = _ledgerLedgerAccountQueryService.SummarizeLedgerAccountsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiPerformance.EstimateCalibrationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiPerformance.EstimateCalibrationRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Recipient = (request.Dependent?.PrimaryAddress?.Recipient ?? ""),
                },
            },
        };
        var reply3 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _performanceCalibrationCalculationService.EstimateCalibrationAsync(input3, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiBenefits.ExplainDependentDecisionResponse
        {
            Dependent = new ApiBenefits.Dependent
            {
                Contact = (request.Dependent?.Contact ?? new ApiCommon.ContactInfo()),
                DependentId = (request.DependentId ?? ""),
                RelationshipManager = new ApiIdentity.UserRef
                {
                    Email = (reply3.Calibration?.Assignee?.Email ?? ""),
                },
            },
            Outcome = new ApiBenefits.ExplainDependentDecisionResponse.Types.Outcome
            {
                Status = (reply2.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.Locale ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "dependent",
            "audit", "ExplainDependentDecision", (request.Details?.Locale ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a dependent.
    /// </summary>
    public override async Task<ApiBenefits.GetDependentRulesResponse> GetDependentRules(ApiBenefits.GetDependentRulesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DependentId, "dependent_id");
        _logger.LogInformation("Handling GetDependentRules for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "dependent", request.DependentId,
            "GetDependentRules", "accepted", token);
        var input0 = new ApiAuthorization.CloneAccessPolicyRequest
        {
            AccessPolicy = new ApiAuthorization.AccessPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _authorizationAccessPolicyService.CloneAccessPolicyAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiBenefits.GetDependentRulesResponse
        {
            Dependent = new ApiBenefits.Dependent
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.AccessPolicy?.Audit?.Etag ?? ""),
                },
                DependentId = (request.DependentId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lints a dependent.
    /// </summary>
    public override async Task<ApiBenefits.LintDependentsResponse> LintDependents(ApiBenefits.LintDependentsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling LintDependents for {Tenant}", requestContext.TenantId);
        if (request.Dependent is { } record)
        {
            var snapshot = DependentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiBenefits.ExplainLifeEventCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiBenefits.ExplainLifeEventCalculationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.Contact?.Phone ?? ""),
                },
            },
            LifeEvent = new ApiBenefits.LifeEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    Email = (request.Dependent?.RelationshipManager?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _benefitsLifeEventCalculationService.ExplainLifeEventCalculationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiBenefits.EstimateLifeEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiBenefits.EstimateLifeEventRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Dependent?.PrimaryAddress?.CountryCode ?? ""),
                    Lines = { (request.Dependent?.PrimaryAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Region = (request.Dependent?.PrimaryAddress?.Region ?? ""),
                },
            },
            LifeEvent = new ApiBenefits.LifeEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (request.Dependent?.RelationshipManager?.DisplayName ?? ""),
                },
            },
        };
        using var rpc1 = _benefitsLifeEventCalculationService.EstimateLifeEventAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiBenefits.LintDependentsResponse
        {
            Dependent = new ApiBenefits.Dependent
            {
                Contact = (request.Details?.Contact ?? new ApiCommon.ContactInfo()),
                DependentId = (request.DependentId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.LifeEvent?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "dependent",
            "audit", "LintDependents", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Normalizes a dependent.
    /// </summary>
    public override async Task<ApiBenefits.NormalizeDependentResponse> NormalizeDependent(ApiBenefits.NormalizeDependentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling NormalizeDependent for {Tenant}", requestContext.TenantId);
        if (request.Dependent is { } record)
        {
            var snapshot = DependentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.BrowseDocumentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.BrowseDocumentsRequest.Types.Filter
            {
                CreatedAfter = (request.Dependent?.Passport?.ExpiresAt ?? new Timestamp()),
                Region = (request.Dependent?.PrimaryAddress?.Region ?? ""),
            },
        };
        using var rpc0 = _documentsDocumentQueryService.BrowseDocumentsAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiBenefits.CalculateLifeEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiBenefits.CalculateLifeEventRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Dependent?.PrimaryAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Region = (request.Dependent?.PrimaryAddress?.Region ?? ""),
                },
            },
            LifeEvent = new ApiBenefits.LifeEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
        };
        using var rpc1 = _benefitsLifeEventCalculationService.CalculateLifeEventAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var response = new ApiBenefits.NormalizeDependentResponse
        {
            Dependent = new ApiBenefits.Dependent
            {
                DependentId = (request.DependentId ?? ""),
                RelationshipManager = (request.Dependent?.RelationshipManager ?? new ApiIdentity.UserRef()),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "dependent",
            "audit", "NormalizeDependent", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Previews a dependent.
    /// </summary>
    public override async Task<ApiBenefits.PreviewDependentResponse> PreviewDependent(ApiBenefits.PreviewDependentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PreviewDependent for {Tenant}", requestContext.TenantId);
        if (request.Dependent is { } record)
        {
            var snapshot = DependentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiAudit.SearchAuditEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiAudit.SearchAuditEventsRequest.Types.Filter
            {
                CreatedAfter = (request.Dependent?.Audit?.UpdatedAt ?? new Timestamp()),
                Region = (request.Dependent?.PrimaryAddress?.Region ?? ""),
            },
        };
        var command0 = new DispatchCommand<ApiAudit.SearchAuditEventsRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _auditAuditEventService.SearchAuditEventsAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiBenefits.EstimateLifeEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiBenefits.EstimateLifeEventRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Dependent?.PrimaryAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Region = (request.Dependent?.PrimaryAddress?.Region ?? ""),
                },
            },
            LifeEvent = new ApiBenefits.LifeEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
        };
        using var rpc1 = _benefitsLifeEventCalculationService.EstimateLifeEventAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiBenefits.SimulateLifeEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiBenefits.SimulateLifeEventRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Locality = (request.Dependent?.PrimaryAddress?.Locality ?? ""),
                },
            },
            LifeEvent = new ApiBenefits.LifeEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    CandidateId = (reply1.LifeEvent?.Subject?.CandidateId ?? ""),
                    Email = (request.Dependent?.Contact?.Email ?? ""),
                },
            },
        };
        using var rpc2 = _benefitsLifeEventCalculationService.SimulateLifeEventAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiBenefits.RecalculateLifeEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LifeEvent = new ApiBenefits.LifeEvent
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                LifeEventId = (reply1.LifeEvent?.LifeEventId ?? ""),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (reply2.LifeEvent?.Subject?.DisplayName ?? ""),
                    Email = (reply2.LifeEvent?.Subject?.Email ?? ""),
                },
            },
        };
        using var rpc3 = _benefitsLifeEventCalculationService.RecalculateLifeEventsAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiBenefits.PreviewDependentResponse
        {
            Dependent = new ApiBenefits.Dependent
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply1.LifeEvent?.Subject?.Email ?? ""),
                    Name = (request.Dependent?.Name ?? new ApiCommon.PersonName()),
                },
                DependentId = (request.DependentId ?? ""),
                RelationshipManager = new ApiIdentity.UserRef
                {
                    DisplayName = (reply2.LifeEvent?.Subject?.DisplayName ?? ""),
                },
            },
            Outcome = new ApiBenefits.PreviewDependentResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "dependent",
            "audit", "PreviewDependent", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Scores a dependent.
    /// </summary>
    public override async Task<ApiBenefits.ScoreDependentResponse> ScoreDependent(ApiBenefits.ScoreDependentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScoreDependent for {Tenant}", requestContext.TenantId);
        if (request.Dependent is { } record)
        {
            var snapshot = DependentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiWebhooks.SummarizeWebhookEndpointsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Reason ?? ""),
        };
        var reply0 = await SendScoreDependent0Async(input0, token);
        var input1 = new ApiMetrics.SummarizeMetricSeriesesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiMetrics.SummarizeMetricSeriesesRequest.Types.Details
            {
                Period = (reply0.Period ?? new ApiCommon.TimeWindow()),
            },
        };
        using var rpc1 = _metricsMetricSeriesQueryService.SummarizeMetricSeriesesAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        var input2 = new ApiBenefits.ExplainLifeEventCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiBenefits.ExplainLifeEventCalculationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Dependent?.Contact?.Email ?? ""),
                    Name = (request.Dependent?.Contact?.Name ?? new ApiCommon.PersonName()),
                },
            },
            LifeEvent = new ApiBenefits.LifeEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    Email = (request.Dependent?.RelationshipManager?.Email ?? ""),
                },
            },
        };
        using var rpc2 = _benefitsLifeEventCalculationService.ExplainLifeEventCalculationAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        await Task.WhenAll(pending1, pending2);
        var reply1 = await pending1;
        var reply2 = await pending2;
        var input3 = new ApiBenefits.CalculateLifeEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiBenefits.CalculateLifeEventRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Details?.Destination?.PostalCode ?? ""),
                },
            },
            LifeEvent = new ApiBenefits.LifeEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    CandidateId = (reply2.LifeEvent?.Subject?.CandidateId ?? ""),
                    DisplayName = (request.Dependent?.RelationshipManager?.DisplayName ?? ""),
                    Email = (reply2.LifeEvent?.Subject?.Email ?? ""),
                },
            },
        };
        using var rpc3 = _benefitsLifeEventCalculationService.CalculateLifeEventAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiBenefits.ScoreDependentResponse
        {
            Dependent = new ApiBenefits.Dependent
            {
                DependentId = (request.DependentId ?? ""),
                Name = (request.Dependent?.Name ?? new ApiCommon.PersonName()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply1.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.Destination?.Region ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "dependent",
            "audit", "ScoreDependent", (request.Details?.Destination?.Region ?? ""), token);
        return response;
    }
    /// <summary>
    /// Validates a dependent.
    /// </summary>
    public override async Task<ApiBenefits.ValidateDependentResponse> ValidateDependent(ApiBenefits.ValidateDependentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ValidateDependent for {Tenant}", requestContext.TenantId);
        if (request.Dependent is { } record)
        {
            var snapshot = DependentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.QuoteKnowledgeArticleRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
        {
            BillingAddress = new ApiCommon.PostalAddress
            {
                Locality = (request.Dependent?.PrimaryAddress?.Locality ?? ""),
            },
        };
        using var rpc0 = _helpdeskKnowledgeArticleCalculationService.QuoteKnowledgeArticleAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiBenefits.QuoteLifeEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiBenefits.QuoteLifeEventRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Dependent?.PrimaryAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
            LifeEvent = new ApiBenefits.LifeEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    Email = (reply0.KnowledgeArticle?.Counterparty?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _benefitsLifeEventCalculationService.QuoteLifeEventAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiBenefits.ValidateDependentResponse
        {
            Dependent = new ApiBenefits.Dependent
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.KnowledgeArticle?.Audit?.UpdatedBy ?? ""),
                },
                Contact = (request.Details?.Contact ?? new ApiCommon.ContactInfo()),
                DependentId = (request.DependentId ?? ""),
                RelationshipManager = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.LifeEvent?.Subject?.DisplayName ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "dependent",
            "audit", "ValidateDependent", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiWebhooks.SummarizeWebhookEndpointsResponse> SendScoreDependent0Async(ApiWebhooks.SummarizeWebhookEndpointsRequest request, CancellationToken token)
    {
        return await _dependentValidationEndpointSendScoreDependent0Port.SendAsync(request, token);
    }
}
