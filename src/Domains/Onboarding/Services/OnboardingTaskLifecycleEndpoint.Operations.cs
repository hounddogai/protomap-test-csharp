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
using ApiBenefits = Acme.Benefits.V1;
using ApiCommon = Acme.Common.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiSearch = Acme.Search.V1;
using ApiShipping = Acme.Shipping.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Onboarding;

public sealed partial class OnboardingTaskLifecycleEndpoint
{
    /// <summary>
    /// Activates an onboarding task.
    /// </summary>
    public override async Task<ApiOnboarding.ActivateOnboardingTaskResponse> ActivateOnboardingTask(ApiOnboarding.ActivateOnboardingTaskRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.OnboardingTaskId, "onboarding_task_id");
        _logger.LogInformation("Handling ActivateOnboardingTask for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task", request.OnboardingTaskId,
            "ActivateOnboardingTask", "accepted", token);
        var input0 = new ApiScheduling.ListPendingShiftsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiScheduling.ListPendingShiftsRequest.Types.Filter
            {
                CreatedAfter = (request.Details?.EffectiveAt ?? new Timestamp()),
                Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        using var rpc0 = _schedulingShiftApprovalService.ListPendingShiftsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiSearch.NormalizeSearchIndexRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiSearch.NormalizeSearchIndexRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                },
            },
            SearchIndex = new ApiSearch.SearchIndex
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                },
            },
        };
        var reply1 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _searchSearchIndexValidationService.NormalizeSearchIndexAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input2 = new ApiOnboarding.RecalculateOnboardingTasksRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                Assignee = new ApiIdentity.UserRef
                {
                    UserId = (request.Details?.NewOwner?.UserId ?? ""),
                },
            },
        };
        var reply2 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _onboardingOnboardingTaskCalculationService.RecalculateOnboardingTasksAsync(input2, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input3 = new ApiBenefits.ResumeBenefitPlanWorkflowRequest
        {
            BenefitPlan = new ApiBenefits.BenefitPlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply2.OnboardingTask?.Audit?.CreatedBy ?? ""),
                    UpdatedBy = (reply1.SearchIndex?.Audit?.UpdatedBy ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiBenefits.ResumeBenefitPlanWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (reply2.OnboardingTask?.Requester?.Email ?? ""),
                },
            },
        };
        var reply3 = await Workflow.SendAsync(input3, token, _benefitsBenefitPlanWorkflowService.ResumeBenefitPlanWorkflowAsync);
        var response = new ApiOnboarding.ActivateOnboardingTaskResponse
        {
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                Comments = { new ApiOnboarding.OnboardingTaskComment
                {
                    CreatedAt = (reply1.SearchIndex?.Audit?.CreatedAt ?? new Timestamp()),
                } },
                OnboardingTaskId = (request.OnboardingTaskId ?? ""),
                Priority = (reply2.OnboardingTask?.Priority ?? 0),
                Requester = new ApiCommon.ContactInfo
                {
                    Name = (reply2.OnboardingTask?.Requester?.Name ?? new ApiCommon.PersonName()),
                },
            },
            Outcome = new ApiOnboarding.ActivateOnboardingTaskResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Result?.Message ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task",
            "audit", "ActivateOnboardingTask", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Expires an onboarding task.
    /// </summary>
    public override async Task<ApiOnboarding.ExpireOnboardingTaskResponse> ExpireOnboardingTask(ApiOnboarding.ExpireOnboardingTaskRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.OnboardingTaskId, "onboarding_task_id");
        _logger.LogInformation("Handling ExpireOnboardingTask for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task", request.OnboardingTaskId,
            "ExpireOnboardingTask", "accepted", token);
        var input0 = new ApiHelpdesk.ApproveSlaPolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.ApproveSlaPolicyRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                    UserId = (request.Details?.NewOwner?.UserId ?? ""),
                },
            },
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Block = new ApiHelpdesk.SlaPolicyBlockAction
                {
                    Reason = (request.Reason ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _sendExpireOnboardingTask0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiOnboarding.ValidateEquipmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Equipment = new ApiOnboarding.Equipment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.SlaPolicy?.Audit?.CreatedBy ?? ""),
                },
            },
        };
        var named1 = _registry.Get<ApiOnboarding.EquipmentValidationService.EquipmentValidationServiceClient>("acme.onboarding.v1.EquipmentValidationService");
        using var rpc1 = named1.ValidateEquipmentAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiOnboarding.ExpireOnboardingTaskResponse
        {
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.SlaPolicy?.Audit?.CreatedAt ?? new Timestamp()),
                },
                OnboardingTaskId = (request.OnboardingTaskId ?? ""),
            },
            Outcome = new ApiOnboarding.ExpireOnboardingTaskResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.ReasonCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task",
            "audit", "ExpireOnboardingTask", (request.Details?.ReasonCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an onboarding task.
    /// </summary>
    public override async Task<ApiOnboarding.GetOnboardingTaskLifecycleResponse> GetOnboardingTaskLifecycle(ApiOnboarding.GetOnboardingTaskLifecycleRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.OnboardingTaskId, "onboarding_task_id");
        _logger.LogInformation("Handling GetOnboardingTaskLifecycle for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task", request.OnboardingTaskId,
            "GetOnboardingTaskLifecycle", "accepted", token);
        var input0 = new ApiPerformance.ResumeReviewWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiPerformance.ResumeReviewWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        var descriptor0 = new Method<ApiPerformance.ResumeReviewWorkflowRequest, ApiPerformance.ResumeReviewWorkflowResponse>(
            MethodType.Unary, "acme.performance.v1" + ".ReviewWorkflowService", "ResumeReviewWorkflow",
            Wire.Marshaller<ApiPerformance.ResumeReviewWorkflowRequest>(), Wire.Marshaller<ApiPerformance.ResumeReviewWorkflowResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiOnboarding.GetOnboardingTaskLifecycleResponse
        {
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                OnboardingTaskId = (request.OnboardingTaskId ?? ""),
                Priority = (reply0.Review?.Counterparty?.Priority ?? 0),
            },
        };
        return response;
    }
    /// <summary>
    /// Renews an onboarding task.
    /// </summary>
    public override async Task<ApiOnboarding.RenewOnboardingTaskResponse> RenewOnboardingTask(ApiOnboarding.RenewOnboardingTaskRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.OnboardingTaskId, "onboarding_task_id");
        _logger.LogInformation("Handling RenewOnboardingTask for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task", request.OnboardingTaskId,
            "RenewOnboardingTask", "accepted", token);
        var input0 = new ApiShipping.FindShipmentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiShipping.FindShipmentsRequest.Types.Filter
            {
                CreatedAfter = (request.Details?.EffectiveAt ?? new Timestamp()),
                OwnerId = (request.Details?.NewOwner?.UserId ?? ""),
            },
        };
        var command0 = new DispatchCommand<ApiShipping.FindShipmentsRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _shippingShipmentQueryService.FindShipmentsAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var profile1 = new MappingProfile<ApiOnboarding.RenewOnboardingTaskRequest, ApiOnboarding.SummarizeOnboardingPlansRequest>(value => new ApiOnboarding.SummarizeOnboardingPlansRequest
        {
            Annotations = { (value.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.SummarizeOnboardingPlansRequest.Types.Details
            {
                Filters = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Reason = (value.Reason ?? ""),
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _onboardingOnboardingPlanQueryService.SummarizeOnboardingPlansAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiOnboarding.ValidateOnboardingPlanRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.ValidateOnboardingPlanRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                },
            },
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Counterparty = new ApiOnboarding.EquipmentRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                },
            },
        };
        input2 = PropertyMap.Copy(input2);
        using var rpc2 = _onboardingOnboardingPlanValidationService.ValidateOnboardingPlanAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiOnboarding.RenewOnboardingTaskResponse
        {
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply2.OnboardingPlan?.Audit?.Etag ?? ""),
                },
                OnboardingTaskId = (request.OnboardingTaskId ?? ""),
            },
            Outcome = new ApiOnboarding.RenewOnboardingTaskResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Status = (reply1.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply2.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.ReasonCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task",
            "audit", "RenewOnboardingTask", (request.Details?.ReasonCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Suspends an onboarding task.
    /// </summary>
    public override async Task<ApiOnboarding.SuspendOnboardingTaskResponse> SuspendOnboardingTask(ApiOnboarding.SuspendOnboardingTaskRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.OnboardingTaskId, "onboarding_task_id");
        _logger.LogInformation("Handling SuspendOnboardingTask for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task", request.OnboardingTaskId,
            "SuspendOnboardingTask", "accepted", token);
        var input0 = request.SuspendOnboardingTaskToRetryTravelBookingWorkflow0();
        using var rpc0 = _travelTravelBookingWorkflowService.RetryTravelBookingWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiOnboarding.CalculateOnboardingTaskRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.CalculateOnboardingTaskRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Locality = (reply0.TravelBooking?.BillingAddress?.Locality ?? ""),
                },
            },
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (reply0.TravelBooking?.Counterparty?.Email ?? ""),
                    UserId = (request.Details?.NewOwner?.UserId ?? ""),
                },
            },
        };
        var reply1 = await SendSuspendOnboardingTask1Async(input1, token);
        var response = new ApiOnboarding.SuspendOnboardingTaskResponse
        {
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                OnboardingTaskId = (request.OnboardingTaskId ?? ""),
            },
            Outcome = new ApiOnboarding.SuspendOnboardingTaskResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Status = (reply0.Outcome?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task",
            "audit", "SuspendOnboardingTask", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Terminates an onboarding task.
    /// </summary>
    public override async Task<ApiOnboarding.TerminateOnboardingTaskResponse> TerminateOnboardingTask(ApiOnboarding.TerminateOnboardingTaskRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.OnboardingTaskId, "onboarding_task_id");
        _logger.LogInformation("Handling TerminateOnboardingTask for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task", request.OnboardingTaskId,
            "TerminateOnboardingTask", "accepted", token);
        var input0 = new ApiOnboarding.ValidateOnboardingPlanRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.ValidateOnboardingPlanRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                },
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _onboardingOnboardingPlanValidationService.ValidateOnboardingPlanAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTimekeeping.ResumeTimesheetWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Timesheet = new ApiTimekeeping.Timesheet
            {
                Card = new ApiTimekeeping.TimesheetCardSettlement
                {
                    HolderName = (reply0.OnboardingPlan ?? new ApiOnboarding.OnboardingPlan()).PaymentCase switch
                    {
                        ApiOnboarding.OnboardingPlan.SettlementOneofCase.Card => (reply0.OnboardingPlan?.Card?.HolderName ?? ""),
                        _ => ""
                    },
                    Last4 = (reply0.OnboardingPlan ?? new ApiOnboarding.OnboardingPlan()).PaymentCase switch
                    {
                        ApiOnboarding.OnboardingPlan.SettlementOneofCase.Card => (reply0.OnboardingPlan?.Card?.Last4 ?? ""),
                        _ => ""
                    },
                    Network = (reply0.OnboardingPlan ?? new ApiOnboarding.OnboardingPlan()).PaymentCase switch
                    {
                        ApiOnboarding.OnboardingPlan.SettlementOneofCase.Card => (reply0.OnboardingPlan?.Card?.Network ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _timekeepingTimesheetWorkflowService.ResumeTimesheetWorkflowAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiOnboarding.ExplainOnboardingPlanDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.ExplainOnboardingPlanDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                },
            },
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (reply1.Timesheet?.BillingAddress?.Recipient ?? ""),
                },
            },
        };
        var json2 = JsonFormatter.Default.Format(input2);
        input2 = JsonParser.Default.Parse<ApiOnboarding.ExplainOnboardingPlanDecisionRequest>(json2);
        using var rpc2 = _onboardingOnboardingPlanValidationService.ExplainOnboardingPlanDecisionAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiOnboarding.TerminateOnboardingTaskResponse
        {
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.Timesheet?.Counterparty?.DisplayName ?? ""),
                },
                OnboardingTaskId = (request.OnboardingTaskId ?? ""),
            },
            Outcome = new ApiOnboarding.TerminateOnboardingTaskResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                ReferenceIds = { (reply2.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply2.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_task",
            "audit", "TerminateOnboardingTask", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiOnboarding.CalculateOnboardingTaskResponse> SendSuspendOnboardingTask1Async(ApiOnboarding.CalculateOnboardingTaskRequest request, CancellationToken token)
    {
        return await _onboardingTaskLifecycleEndpointSendSuspendOnboardingTask1Port.SendAsync(request, token);
    }
}
