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
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiKeymanagement = Acme.Keymanagement.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Performance;

public sealed partial class FeedbackValidationEndpoint
{
    /// <summary>
    /// Checks a feedback.
    /// </summary>
    public override async Task<ApiPerformance.CheckFeedbackEligibilityResponse> CheckFeedbackEligibility(ApiPerformance.CheckFeedbackEligibilityRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CheckFeedbackEligibility for {Tenant}", requestContext.TenantId);
        if (request.Feedback is { } record)
        {
            var snapshot = FeedbackMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiMetrics.CheckMetricSeriesEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            MetricSeries = new ApiMetrics.MetricSeries
            {
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Feedback?.Subject?.DisplayName ?? ""),
                },
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiMetrics.CheckMetricSeriesEligibilityRequest>(json0);
        using var rpc0 = _metricsMetricSeriesValidationService.CheckMetricSeriesEligibilityAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiPerformance.SimulateCalibrationRequest();
        input1.Calibration = new ApiPerformance.Calibration
        {
            Assignee = new ApiIdentity.UserRef
            {
                DisplayName = (reply0.MetricSeries?.Owner?.DisplayName ?? ""),
            },
        };
        input1.Context = (request.Context ?? new ApiCommon.RequestContext());
        using var rpc1 = _performanceCalibrationCalculationService.SimulateCalibrationAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiKeymanagement.CompareCryptoKeyOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CryptoKey = new ApiKeymanagement.CryptoKey
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (reply1.Calibration?.Requester?.Name ?? new ApiCommon.PersonName()),
                    PreferredChannel = (reply1.Calibration?.Requester?.PreferredChannel ?? ""),
                },
                Holder = new ApiKeymanagement.KeyRingRef
                {
                    DisplayName = (reply0.MetricSeries?.Owner?.DisplayName ?? ""),
                },
            },
        };
        using var rpc2 = _keymanagementCryptoKeyCalculationService.CompareCryptoKeyOptionsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiAuthorization.ExplainAccessPolicyDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiAuthorization.ExplainAccessPolicyDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
            },
        };
        var reply3 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _authorizationAccessPolicyValidationService.ExplainAccessPolicyDecisionAsync(input3, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiPerformance.CheckFeedbackEligibilityResponse
        {
            Feedback = new ApiPerformance.Feedback
            {
                FeedbackId = (request.FeedbackId ?? ""),
                OccurredAt = (request.Feedback?.OccurredAt ?? new Timestamp()),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (reply2.CryptoKey?.Holder?.DisplayName ?? ""),
                },
            },
            Outcome = new ApiPerformance.CheckFeedbackEligibilityResponse.Types.Outcome
            {
                Message = (reply3.Outcome?.Message ?? ""),
                NextSteps = { (reply0.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply3.Outcome?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "feedback",
            "audit", "CheckFeedbackEligibility", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a feedback.
    /// </summary>
    public override async Task<ApiPerformance.ExplainFeedbackDecisionResponse> ExplainFeedbackDecision(ApiPerformance.ExplainFeedbackDecisionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExplainFeedbackDecision for {Tenant}", requestContext.TenantId);
        if (request.Feedback is { } record)
        {
            var snapshot = FeedbackMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.FindChangeRequestsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiItsm.FindChangeRequestsRequest.Types.Filter
            {
                CreatedAfter = (request.Feedback?.OccurredAt ?? new Timestamp()),
                Labels = { (request.Feedback?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Feedback?.FeedbackId ?? ""),
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _itsmChangeRequestQueryService.FindChangeRequestsAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiPerformance.CalculateCalibrationRequest
        {
            Calibration = new ApiPerformance.Calibration
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply1 = await Workflow.SendAsync(input1, token, _performanceCalibrationCalculationService.CalculateCalibrationAsync);
        var response = new ApiPerformance.ExplainFeedbackDecisionResponse
        {
            Feedback = new ApiPerformance.Feedback
            {
                FeedbackId = (request.FeedbackId ?? ""),
                Kind = (request.Feedback?.Kind ?? ""),
                Subject = new ApiRecruiting.CandidateRef
                {
                    Email = (reply1.Calibration?.Assignee?.Email ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "feedback",
            "audit", "ExplainFeedbackDecision", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a feedback.
    /// </summary>
    public override async Task<ApiPerformance.GetFeedbackRulesResponse> GetFeedbackRules(ApiPerformance.GetFeedbackRulesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.FeedbackId, "feedback_id");
        _logger.LogInformation("Handling GetFeedbackRules for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "feedback", request.FeedbackId,
            "GetFeedbackRules", "accepted", token);
        var response = new ApiPerformance.GetFeedbackRulesResponse
        {
            Feedback = new ApiPerformance.Feedback
            {
                FeedbackId = (request.FeedbackId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lints a feedback.
    /// </summary>
    public override async Task<ApiPerformance.LintFeedbacksResponse> LintFeedbacks(ApiPerformance.LintFeedbacksRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling LintFeedbacks for {Tenant}", requestContext.TenantId);
        if (request.Feedback is { } record)
        {
            var snapshot = FeedbackMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiPerformance.EstimateFeedbackResponse();
        foreach (var item in (request.Feedback?.Measurements ?? new Google.Protobuf.Collections.RepeatedField<ApiPerformance.FeedbackMeasurement>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input0 = new ApiPerformance.EstimateFeedbackRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Feedback = new ApiPerformance.Feedback
                {
                    Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                    Measurements = { new ApiPerformance.FeedbackMeasurement
                    {
                        Value = item.Value,
                    } },
                },
            };
            using var rpc0 = _performanceFeedbackCalculationService.EstimateFeedbackAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var response = new ApiPerformance.LintFeedbacksResponse
        {
            Feedback = new ApiPerformance.Feedback
            {
                FeedbackId = (request.FeedbackId ?? ""),
                Subject = (request.Feedback?.Subject ?? new ApiRecruiting.CandidateRef()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "feedback",
            "audit", "LintFeedbacks", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Normalizes a feedback.
    /// </summary>
    public override async Task<ApiPerformance.NormalizeFeedbackResponse> NormalizeFeedback(ApiPerformance.NormalizeFeedbackRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling NormalizeFeedback for {Tenant}", requestContext.TenantId);
        if (request.Feedback is { } record)
        {
            var snapshot = FeedbackMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiPerformance.CalculateCalibrationRequest
        {
            Calibration = new ApiPerformance.Calibration
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Feedback?.Subject?.DisplayName ?? ""),
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
                Comments = { new ApiPerformance.CalibrationComment
                {
                    AuthorEmail = (request.Feedback?.Subject?.Email ?? ""),
                } },
                Requester = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var named0 = _registry.Get<ApiPerformance.CalibrationCalculationService.CalibrationCalculationServiceClient>("acme.performance.v1.CalibrationCalculationService");
        using var rpc0 = named0.CalculateCalibrationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiPerformance.NormalizeFeedbackResponse
        {
            Feedback = new ApiPerformance.Feedback
            {
                FeedbackId = (request.FeedbackId ?? ""),
                OccurredAt = (request.Feedback?.OccurredAt ?? new Timestamp()),
                Subject = new ApiRecruiting.CandidateRef
                {
                    Email = (reply0.Calibration?.Assignee?.Email ?? ""),
                },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "feedback",
            "audit", "NormalizeFeedback", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Previews a feedback.
    /// </summary>
    public override async Task<ApiPerformance.PreviewFeedbackResponse> PreviewFeedback(ApiPerformance.PreviewFeedbackRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PreviewFeedback for {Tenant}", requestContext.TenantId);
        if (request.Feedback is { } record)
        {
            var snapshot = FeedbackMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiPerformance.SimulateCalibrationRequest
        {
            Calibration = new ApiPerformance.Calibration
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Feedback?.Subject?.DisplayName ?? ""),
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.Contact?.Phone ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var descriptor0 = new Method<ApiPerformance.SimulateCalibrationRequest, ApiPerformance.SimulateCalibrationResponse>(
            MethodType.Unary, "acme.performance.v1" + ".CalibrationCalculationService", "SimulateCalibration",
            Wire.Marshaller<ApiPerformance.SimulateCalibrationRequest>(), Wire.Marshaller<ApiPerformance.SimulateCalibrationResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiPerformance.NormalizeGoalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Goal = new ApiPerformance.Goal
            {
                Customer = new ApiPerformance.ReviewRef
                {
                    DisplayName = (reply0.Calibration ?? new ApiPerformance.Calibration()).OriginCase switch
                    {
                        ApiPerformance.Calibration.OriginOneofCase.Customer => (reply0.Calibration?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
                Sla = new ApiPerformance.GoalSla
                {
                    DueAt = (reply0.Calibration?.Sla?.DueAt ?? new Timestamp()),
                },
            },
        };
        using var rpc1 = _performanceGoalValidationService.NormalizeGoalAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiPerformance.PreviewFeedbackResponse
        {
            Feedback = new ApiPerformance.Feedback
            {
                FeedbackId = (request.FeedbackId ?? ""),
                Source = (request.Feedback?.Source ?? ""),
            },
            Outcome = new ApiPerformance.PreviewFeedbackResponse.Types.Outcome
            {
                Status = (reply1.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "feedback",
            "audit", "PreviewFeedback", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Scores a feedback.
    /// </summary>
    public override async Task<ApiPerformance.ScoreFeedbackResponse> ScoreFeedback(ApiPerformance.ScoreFeedbackRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScoreFeedback for {Tenant}", requestContext.TenantId);
        if (request.Feedback is { } record)
        {
            var snapshot = FeedbackMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var batch0 = (request.Feedback?.Measurements ?? new Google.Protobuf.Collections.RepeatedField<ApiPerformance.FeedbackMeasurement>()).Select(async item =>
        {
            var input0 = new ApiPerformance.CalculateFeedbackRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Feedback = new ApiPerformance.Feedback
                {
                    Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                    Measurements = { new ApiPerformance.FeedbackMeasurement
                    {
                        Metric = (item.Metric ?? ""),
                        ObservedAt = (item.ObservedAt ?? new Timestamp()),
                        Unit = (item.Unit ?? ""),
                        Value = item.Value,
                    } },
                },
            };
            using var rpc0 = _performanceFeedbackCalculationService.CalculateFeedbackAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            return reply0;
        }).ToArray();
        var replies0 = await Task.WhenAll(batch0);
        var reply0 = replies0.LastOrDefault() ?? new ApiPerformance.CalculateFeedbackResponse();
        var reply1 = new ApiDocuments.ListDocumentTemplateConflictsResponse();
        foreach (var item in (request.Feedback?.Measurements ?? new Google.Protobuf.Collections.RepeatedField<ApiPerformance.FeedbackMeasurement>()))
        {
            var input1 = new ApiDocuments.ListDocumentTemplateConflictsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiDocuments.ListDocumentTemplateConflictsRequest.Types.Filter
                {
                    CreatedAfter = (item.ObservedAt ?? new Timestamp()),
                },
            };
            using var rpc1 = _documentsDocumentTemplateSyncService.ListDocumentTemplateConflictsAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var response = new ApiPerformance.ScoreFeedbackResponse
        {
            Feedback = new ApiPerformance.Feedback
            {
                FeedbackId = (request.FeedbackId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (request.Feedback?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "feedback",
            "audit", "ScoreFeedback", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Validates a feedback.
    /// </summary>
    public override async Task<ApiPerformance.ValidateFeedbackResponse> ValidateFeedback(ApiPerformance.ValidateFeedbackRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ValidateFeedback for {Tenant}", requestContext.TenantId);
        if (request.Feedback is { } record)
        {
            var snapshot = FeedbackMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = request.ValidateFeedbackToExplainCryptoKeyCalculation0();
        using var rpc0 = _keymanagementCryptoKeyCalculationService.ExplainCryptoKeyCalculationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiNotifications.ExportNotificationsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiNotifications.ExportNotificationsRequest.Types.Filter
            {
                Labels = { (reply0.CryptoKey?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        using var stream1 = _notificationsNotificationQueryService.ExportNotifications(input1, cancellationToken: token);
        await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportNotifications frame {Type}", change1.GetType().Name);
        }
        var input2 = new ApiPerformance.EstimateFeedbackRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Feedback = new ApiPerformance.Feedback
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (reply0.CryptoKey?.Holder?.DisplayName ?? ""),
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
            },
        };
        using var rpc2 = _performanceFeedbackCalculationService.EstimateFeedbackAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiDocuments.ScoreSignatureRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ScoreSignatureRequest.Types.Details
            {
                CurrencyCode = (reply2.Breakdown?.CurrencyCode ?? ""),
            },
            Signature = new ApiDocuments.Signature
            {
                Attributes = { (request.Feedback?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiDocuments.DocumentRef
                {
                    DisplayName = (request.Feedback?.Subject?.DisplayName ?? ""),
                },
            },
        };
        using var rpc3 = _documentsSignatureValidationService.ScoreSignatureAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiPerformance.ValidateFeedbackResponse
        {
            Feedback = new ApiPerformance.Feedback
            {
                Attributes = { (reply3.Signature?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
                FeedbackId = (request.FeedbackId ?? ""),
                Kind = (request.Feedback?.Kind ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
                Message = (reply0.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "feedback",
            "audit", "ValidateFeedback", (request.Reason ?? ""), token);
        return response;
    }
}
