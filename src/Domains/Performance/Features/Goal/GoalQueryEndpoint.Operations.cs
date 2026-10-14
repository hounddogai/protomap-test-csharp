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
using ApiContent = Acme.Content.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLineage = Acme.Lineage.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Performance;

public sealed partial class GoalQueryEndpoint
{
    /// <summary>
    /// Lists goals a page at a time.
    /// </summary>
    public override async Task<ApiPerformance.BrowseGoalsResponse> BrowseGoals(ApiPerformance.BrowseGoalsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling BrowseGoals for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "goal", "batch",
            "BrowseGoals", "accepted", token);
        var input0 = new ApiPerformance.NormalizeCalibrationRequest
        {
            Calibration = new ApiPerformance.Calibration
            {
                Sla = new ApiPerformance.CalibrationSla
                {
                    DueAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiPerformance.NormalizeCalibrationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _sendBrowseGoals0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiPerformance.BrowseGoalsResponse
        {
            Goals = { new ApiPerformance.Goal
            {
                Requester = (reply0.Calibration?.Requester ?? new ApiCommon.ContactInfo()),
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "goal",
            "audit", "BrowseGoals", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Counts goals by status.
    /// </summary>
    public override async Task<ApiPerformance.CountGoalsResponse> CountGoals(ApiPerformance.CountGoalsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CountGoals for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "goal", "batch",
            "CountGoals", "accepted", token);
        var input0 = new ApiLineage.BrowseLineageNodesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Page = new ApiCommon.PageRequest
            {
                PageToken = (request.Page?.PageToken ?? ""),
            },
        };
        var named0 = _registry.Get<ApiLineage.LineageNodeQueryService.LineageNodeQueryServiceClient>("acme.lineage.v1.LineageNodeQueryService");
        using var rpc0 = named0.BrowseLineageNodesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiContent.CountArticlesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiContent.CountArticlesRequest.Types.Filter
            {
                OwnerId = (request.Filter?.OwnerId ?? ""),
            },
            Page = new ApiCommon.PageRequest
            {
                PageSize = (request.Page?.PageSize ?? 0),
            },
        };
        var descriptor1 = new Method<ApiContent.CountArticlesRequest, ApiContent.CountArticlesResponse>(
            MethodType.Unary, "acme.content.v1" + ".ArticleQueryService", "CountArticles",
            Wire.Marshaller<ApiContent.CountArticlesRequest>(), Wire.Marshaller<ApiContent.CountArticlesResponse>());
        using var rpc1 = _invoker.AsyncUnaryCall(descriptor1, null,
            new CallOptions(cancellationToken: token), input1);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiPerformance.CountGoalsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.OrderBy ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "goal",
            "audit", "CountGoals", (request.OrderBy ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams every matching goal.
    /// </summary>
    public override async Task ExportGoals(ApiPerformance.ExportGoalsRequest request, IServerStreamWriter<ApiPerformance.ExportGoalsResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExportGoals for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "goal", "batch",
            "ExportGoals", "accepted", token);
        var input0 = new ApiPerformance.ExplainFeedbackDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiPerformance.ExplainFeedbackDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Locale = (request.Context?.Locale ?? ""),
            },
            Feedback = new ApiPerformance.Feedback
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Measurements = { new ApiPerformance.FeedbackMeasurement
                {
                    ObservedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                } },
            },
        };
        using var rpc0 = _performanceFeedbackValidationService.ExplainFeedbackDecisionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiPerformance.CalculateFeedbackRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Feedback = new ApiPerformance.Feedback
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    CandidateId = (reply0.Feedback?.Subject?.CandidateId ?? ""),
                    DisplayName = (reply0.Feedback?.Subject?.DisplayName ?? ""),
                    Email = (reply0.Feedback?.Subject?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _performanceFeedbackCalculationService.CalculateFeedbackAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiPerformance.ValidateFeedbackRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiPerformance.ValidateFeedbackRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply0.Feedback?.Subject?.Email ?? ""),
                },
            },
            Feedback = new ApiPerformance.Feedback
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    CandidateId = (reply1.Feedback?.Subject?.CandidateId ?? ""),
                    DisplayName = (reply1.Feedback?.Subject?.DisplayName ?? ""),
                },
            },
        };
        using var rpc2 = _performanceFeedbackValidationService.ValidateFeedbackAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiDocuments.SendDocumentTemplateTestNoticeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.SendDocumentTemplateTestNoticeRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (reply0.Feedback?.Subject?.Email ?? ""),
                },
                Variables = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        using var rpc3 = _documentsDocumentTemplateNotificationService.SendDocumentTemplateTestNoticeAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiPerformance.ExportGoalsResponse
        {
            Goal = new ApiPerformance.Goal
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (reply3.DocumentTemplate?.Counterparty?.DisplayName ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply3.DocumentTemplate?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Format ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "goal",
            "audit", "ExportGoals", (request.Format ?? ""), token);
        await responses.WriteAsync(response, token);
    }
    /// <summary>
    /// Lists goals a page at a time.
    /// </summary>
    public override async Task<ApiPerformance.FindGoalsResponse> FindGoals(ApiPerformance.FindGoalsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling FindGoals for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "goal", "batch",
            "FindGoals", "accepted", token);
        var response = new ApiPerformance.FindGoalsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "goal",
            "audit", "FindGoals", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a goal.
    /// </summary>
    public override async Task<ApiPerformance.LookupGoalResponse> LookupGoal(ApiPerformance.LookupGoalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.GoalId, "goal_id");
        _logger.LogInformation("Handling LookupGoal for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "goal", request.GoalId,
            "LookupGoal", "accepted", token);
        var input0 = new ApiWorkplaces.CalculateVisitorPassRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        var reply0 = await SendLookupGoal0Async(input0, token);
        var response = new ApiPerformance.LookupGoalResponse
        {
            Goal = new ApiPerformance.Goal
            {
                GoalId = (request.GoalId ?? ""),
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (reply0.VisitorPass?.Counterparty?.Email ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Returns a goal.
    /// </summary>
    public override async Task<ApiPerformance.PeekGoalResponse> PeekGoal(ApiPerformance.PeekGoalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.GoalId, "goal_id");
        _logger.LogInformation("Handling PeekGoal for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "goal", request.GoalId,
            "PeekGoal", "accepted", token);
        var input0 = new ApiDocuments.ShareSignatureExportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ShareSignatureExportRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _documentsSignatureExportService.ShareSignatureExportAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiPerformance.PeekGoalResponse
        {
            Goal = new ApiPerformance.Goal
            {
                GoalId = (request.GoalId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        return response;
    }
    /// <summary>
    /// Returns several goals at once.
    /// </summary>
    public override async Task<ApiPerformance.ResolveGoalsResponse> ResolveGoals(ApiPerformance.ResolveGoalsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ResolveGoals for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "goal", "batch",
            "ResolveGoals", "accepted", token);
        var response = new ApiPerformance.ResolveGoalsResponse();
        return response;
    }
    /// <summary>
    /// Lists goals a page at a time.
    /// </summary>
    public override async Task<ApiPerformance.SuggestGoalsResponse> SuggestGoals(ApiPerformance.SuggestGoalsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SuggestGoals for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "goal", "batch",
            "SuggestGoals", "accepted", token);
        var response = new ApiPerformance.SuggestGoalsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "goal",
            "audit", "SuggestGoals", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Summarizes a goal.
    /// </summary>
    public override async Task<ApiPerformance.SummarizeGoalsResponse> SummarizeGoals(ApiPerformance.SummarizeGoalsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SummarizeGoals for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "goal", request.GoalId,
            "SummarizeGoals", "accepted", token);
        var input0 = new ApiPerformance.FindReviewsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiPerformance.FindReviewsRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
                Labels = { (request.Details?.Filters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.GoalId ?? ""),
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiPerformance.FindReviewsRequest>();
        using var rpc0 = _performanceReviewQueryService.FindReviewsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiPerformance.FindReviewsRequest>("acme.performance.v1.GoalQueryService/SummarizeGoals/0", input0.Clone()), token);
        var input1 = new ApiItsm.FindChangeRequestsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiItsm.FindChangeRequestsRequest.Types.Filter
            {
                Labels = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        var json1 = JsonFormatter.Default.Format(input1);
        input1 = JsonParser.Default.Parse<ApiItsm.FindChangeRequestsRequest>(json1);
        using var rpc1 = _itsmChangeRequestQueryService.FindChangeRequestsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiPerformance.SummarizeGoalsResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "goal",
            "audit", "SummarizeGoals", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiWorkplaces.CalculateVisitorPassResponse> SendLookupGoal0Async(ApiWorkplaces.CalculateVisitorPassRequest request, CancellationToken token)
    {
        return await _goalQueryEndpointSendLookupGoal0Port.SendAsync(request, token);
    }
}
