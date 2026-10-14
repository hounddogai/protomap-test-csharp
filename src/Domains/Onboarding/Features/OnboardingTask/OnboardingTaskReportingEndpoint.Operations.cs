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
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiMedia = Acme.Media.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiReviews = Acme.Reviews.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Onboarding;

public sealed partial class OnboardingTaskReportingEndpoint
{
    /// <summary>
    /// Counts onboarding tasks by status.
    /// </summary>
    public override async Task<ApiOnboarding.CountOnboardingTaskTotalsResponse> CountOnboardingTaskTotals(ApiOnboarding.CountOnboardingTaskTotalsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CountOnboardingTaskTotals for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task", "batch",
            "CountOnboardingTaskTotals", "accepted", token);
        var response = new ApiOnboarding.CountOnboardingTaskTotalsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task",
            "audit", "CountOnboardingTaskTotals", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Deletes an onboarding task.
    /// </summary>
    public override async Task<ApiOnboarding.DeleteOnboardingTaskReportResponse> DeleteOnboardingTaskReport(ApiOnboarding.DeleteOnboardingTaskReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.OnboardingTaskId, "onboarding_task_id");
        _logger.LogInformation("Handling DeleteOnboardingTaskReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task", request.OnboardingTaskId,
            "DeleteOnboardingTaskReport", "accepted", token);
        var input0 = new ApiDocuments.ResolveDocumentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var command0 = new DispatchCommand<ApiDocuments.ResolveDocumentsRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _documentsDocumentQueryService.ResolveDocumentsAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.CalculateSignatureRequest
        {
            Annotations = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Reason ?? ""),
            Signature = new ApiDocuments.Signature
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Source = (request.OnboardingTaskId ?? ""),
            },
        };
        using var rpc1 = _documentsSignatureCalculationService.CalculateSignatureAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiOnboarding.DeleteOnboardingTaskReportResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Etag ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task",
            "audit", "DeleteOnboardingTaskReport", (request.Etag ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an onboarding task.
    /// </summary>
    public override async Task<ApiOnboarding.FetchOnboardingTaskReportResponse> FetchOnboardingTaskReport(ApiOnboarding.FetchOnboardingTaskReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.OnboardingTaskId, "onboarding_task_id");
        _logger.LogInformation("Handling FetchOnboardingTaskReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task", request.OnboardingTaskId,
            "FetchOnboardingTaskReport", "accepted", token);
        var response = new ApiOnboarding.FetchOnboardingTaskReportResponse
        {
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                OnboardingTaskId = (request.OnboardingTaskId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Generates an onboarding task.
    /// </summary>
    public override async Task<ApiOnboarding.GenerateOnboardingTaskReportResponse> GenerateOnboardingTaskReport(ApiOnboarding.GenerateOnboardingTaskReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling GenerateOnboardingTaskReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task", request.OnboardingTaskId,
            "GenerateOnboardingTaskReport", "accepted", token);
        var input0 = new ApiLearning.RetryLearningPathWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LearningPath = new ApiLearning.LearningPath
            {
                Children = { new ApiLearning.LearningPath
                {
                    Children = { new ApiLearning.LearningPath
                    {
                        Manager = new ApiIdentity.UserRef
                        {
                            UserId = (request.Context?.Actor?.UserId ?? ""),
                        },
                    } },
                    Manager = new ApiIdentity.UserRef
                    {
                        Email = (request.Context?.Actor?.Email ?? ""),
                    },
                } },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _learningLearningPathWorkflowService.RetryLearningPathWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = request.GenerateOnboardingTaskReportToAdvanceLearningPathWorkflow1(reply0);
        using var rpc1 = _learningLearningPathWorkflowService.AdvanceLearningPathWorkflowAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiWorkplaces.SignalBuildingWorkflowRequest
        {
            Building = new ApiWorkplaces.Building
            {
                Children = { new ApiWorkplaces.Building
                {
                    Children = { new ApiWorkplaces.Building
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            Etag = (reply1.LearningPath?.Audit?.Etag ?? ""),
                        },
                        Location = new ApiCommon.PostalAddress
                        {
                            Locality = (reply1.LearningPath?.Location?.Locality ?? ""),
                            Recipient = (reply0.LearningPath?.Location?.Recipient ?? ""),
                        },
                        Manager = new ApiIdentity.UserRef
                        {
                            Email = (reply0.LearningPath?.Manager?.Email ?? ""),
                        },
                    } },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply2 = await SendGenerateOnboardingTaskReport2Async(input2, token);
        var response = new ApiOnboarding.GenerateOnboardingTaskReportResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply2.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task",
            "audit", "GenerateOnboardingTaskReport", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Lists onboarding tasks a page at a time.
    /// </summary>
    public override async Task<ApiOnboarding.ListOnboardingTaskReportsResponse> ListOnboardingTaskReports(ApiOnboarding.ListOnboardingTaskReportsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListOnboardingTaskReports for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task", "batch",
            "ListOnboardingTaskReports", "accepted", token);
        var input0 = new ApiReviews.SummarizeProductReviewsRequest
        {
            Annotations = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiReviews.SummarizeProductReviewsRequest.Types.Details
            {
                Filters = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _reviewsProductReviewQueryService.SummarizeProductReviewsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiOnboarding.ListOnboardingTaskReportsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task",
            "audit", "ListOnboardingTaskReports", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Schedules an onboarding task.
    /// </summary>
    public override async Task<ApiOnboarding.ScheduleOnboardingTaskReportResponse> ScheduleOnboardingTaskReport(ApiOnboarding.ScheduleOnboardingTaskReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScheduleOnboardingTaskReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task", request.OnboardingTaskId,
            "ScheduleOnboardingTaskReport", "accepted", token);
        var input0 = new ApiOnboarding.QuoteOnboardingPlanRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiOnboarding.QuoteOnboardingPlanRequest>();
        using var rpc0 = _onboardingOnboardingPlanCalculationService.QuoteOnboardingPlanAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiOnboarding.CompareOnboardingPlanOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.CompareOnboardingPlanOptionsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Recipient = (reply0.OnboardingPlan?.BillingAddress?.Recipient ?? ""),
                },
            },
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.OnboardingPlan?.Audit?.UpdatedBy ?? ""),
                },
                Bank = new ApiOnboarding.OnboardingPlanBankSettlement
                {
                    Iban = (reply0.OnboardingPlan ?? new ApiOnboarding.OnboardingPlan()).PaymentCase switch
                    {
                        ApiOnboarding.OnboardingPlan.SettlementOneofCase.Bank => (reply0.OnboardingPlan?.Bank?.Iban ?? ""),
                        _ => ""
                    },
                },
                Counterparty = new ApiOnboarding.EquipmentRef
                {
                    DisplayName = (reply0.OnboardingPlan?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        using var rpc1 = _onboardingOnboardingPlanCalculationService.CompareOnboardingPlanOptionsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiOnboarding.EstimateOnboardingPlanRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply1.OnboardingPlan?.Audit?.UpdatedBy ?? ""),
                },
                Card = new ApiOnboarding.OnboardingPlanCardSettlement
                {
                    Last4 = (reply1.OnboardingPlan ?? new ApiOnboarding.OnboardingPlan()).PaymentCase switch
                    {
                        ApiOnboarding.OnboardingPlan.SettlementOneofCase.Card => (reply1.OnboardingPlan?.Card?.Last4 ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc2 = _onboardingOnboardingPlanCalculationService.EstimateOnboardingPlanAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiDocuments.SuggestDocumentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.SuggestDocumentsRequest.Types.Filter
            {
                Region = (reply1.OnboardingPlan?.BillingAddress?.Region ?? ""),
            },
        };
        using var rpc3 = _documentsDocumentQueryService.SuggestDocumentsAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiOnboarding.ScheduleOnboardingTaskReportResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply3.Result?.CorrelationId ?? ""),
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Result?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task",
            "audit", "ScheduleOnboardingTaskReport", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Shares an onboarding task.
    /// </summary>
    public override async Task<ApiOnboarding.ShareOnboardingTaskReportResponse> ShareOnboardingTaskReport(ApiOnboarding.ShareOnboardingTaskReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ShareOnboardingTaskReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task", request.OnboardingTaskId,
            "ShareOnboardingTaskReport", "accepted", token);
        var input0 = new ApiHelpdesk.CancelSlaPolicyWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.CancelSlaPolicyWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Recipient?.Email ?? ""),
                    Name = (request.Details?.Recipient?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.Details?.Recipient?.Phone ?? ""),
                },
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _helpdeskSlaPolicyWorkflowService.CancelSlaPolicyWorkflowAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiWorkplaces.SignalBuildingWorkflowRequest
        {
            Building = new ApiWorkplaces.Building
            {
                Children = { new ApiWorkplaces.Building
                {
                    Children = { new ApiWorkplaces.Building
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            CreatedAt = (reply0.SlaPolicy?.Audit?.CreatedAt ?? new Timestamp()),
                        },
                    } },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply1 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _workplacesBuildingWorkflowService.SignalBuildingWorkflowAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input2 = new ApiOnboarding.ValidateOnboardingTaskRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Recipient?.Email ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.Building?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
        };
        var reply2 = await Workflow.SendAsync(input2, token, _onboardingOnboardingTaskValidationService.ValidateOnboardingTaskAsync);
        var response = new ApiOnboarding.ShareOnboardingTaskReportResponse
        {
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.SlaPolicy?.Owner?.DisplayName ?? ""),
                    UserId = (reply1.Building?.Manager?.UserId ?? ""),
                },
                OnboardingTaskId = (request.OnboardingTaskId ?? ""),
                Requester = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Recipient?.Name ?? new ApiCommon.PersonName()),
                },
            },
            Outcome = new ApiOnboarding.ShareOnboardingTaskReportResponse.Types.Outcome
            {
                NextSteps = { (reply2.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task",
            "audit", "ShareOnboardingTaskReport", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams the changes of onboarding tasks as they happen.
    /// </summary>
    public override async Task StreamOnboardingTaskMetrics(ApiOnboarding.StreamOnboardingTaskMetricsRequest request, IServerStreamWriter<ApiOnboarding.StreamOnboardingTaskMetricsResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling StreamOnboardingTaskMetrics for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task", "batch",
            "StreamOnboardingTaskMetrics", "accepted", token);
        var input0 = new ApiWorkplaces.ResumeBuildingWorkflowRequest
        {
            Building = new ApiWorkplaces.Building
            {
                Children = { new ApiWorkplaces.Building
                {
                    Children = { new ApiWorkplaces.Building
                    {
                        Location = new ApiCommon.PostalAddress
                        {
                            Region = (request.Filter?.Region ?? ""),
                        },
                        Manager = new ApiIdentity.UserRef
                        {
                            Email = (request.Subscriber?.Email ?? ""),
                        },
                    } },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _sendStreamOnboardingTaskMetrics0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiMedia.FindMediaAssetsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiMedia.FindMediaAssetsRequest.Types.Filter
            {
                Region = (request.Filter?.Region ?? ""),
            },
        };
        var named1 = _registry.Get<ApiMedia.MediaAssetQueryService.MediaAssetQueryServiceClient>("acme.media.v1.MediaAssetQueryService");
        using var rpc1 = named1.FindMediaAssetsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiOnboarding.StreamOnboardingTaskMetricsResponse
        {
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                Requester = new ApiCommon.ContactInfo
                {
                    Name = (request.Subscriber?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.Subscriber?.Phone ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Subscriber?.PreferredChannel ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task",
            "audit", "StreamOnboardingTaskMetrics", (request.Subscriber?.PreferredChannel ?? ""), token);
        await responses.WriteAsync(response, token);
    }
    private async Task<ApiWorkplaces.SignalBuildingWorkflowResponse> SendGenerateOnboardingTaskReport2Async(ApiWorkplaces.SignalBuildingWorkflowRequest request, CancellationToken token)
    {
        return await _onboardingTaskReportingEndpointSendGenerateOnboardingTaskReport2Port.SendAsync(request, token);
    }
}
