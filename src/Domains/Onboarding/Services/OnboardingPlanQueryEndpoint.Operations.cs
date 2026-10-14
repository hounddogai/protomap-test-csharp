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
using ApiNotifications = Acme.Notifications.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Onboarding;

public sealed partial class OnboardingPlanQueryEndpoint
{
    /// <summary>
    /// Lists onboarding plans a page at a time.
    /// </summary>
    public override async Task<ApiOnboarding.BrowseOnboardingPlansResponse> BrowseOnboardingPlans(ApiOnboarding.BrowseOnboardingPlansRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling BrowseOnboardingPlans for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan", "batch",
            "BrowseOnboardingPlans", "accepted", token);
        var input0 = new ApiOnboarding.QuoteOnboardingTaskRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.QuoteOnboardingTaskRequest.Types.Details
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
        var descriptor0 = new Method<ApiOnboarding.QuoteOnboardingTaskRequest, ApiOnboarding.QuoteOnboardingTaskResponse>(
            MethodType.Unary, "acme.onboarding.v1" + ".OnboardingTaskCalculationService", "QuoteOnboardingTask",
            Wire.Marshaller<ApiOnboarding.QuoteOnboardingTaskRequest>(), Wire.Marshaller<ApiOnboarding.QuoteOnboardingTaskResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiOnboarding.BrowseOnboardingPlansResponse
        {
            OnboardingPlans = { new ApiOnboarding.OnboardingPlan
            {
                Counterparty = new ApiOnboarding.EquipmentRef
                {
                    DisplayName = (reply0.OnboardingTask?.Assignee?.DisplayName ?? ""),
                },
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan",
            "audit", "BrowseOnboardingPlans", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Counts onboarding plans by status.
    /// </summary>
    public override async Task<ApiOnboarding.CountOnboardingPlansResponse> CountOnboardingPlans(ApiOnboarding.CountOnboardingPlansRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CountOnboardingPlans for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan", "batch",
            "CountOnboardingPlans", "accepted", token);
        var response = new ApiOnboarding.CountOnboardingPlansResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan",
            "audit", "CountOnboardingPlans", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams every matching onboarding plan.
    /// </summary>
    public override async Task ExportOnboardingPlans(ApiOnboarding.ExportOnboardingPlansRequest request, IServerStreamWriter<ApiOnboarding.ExportOnboardingPlansResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExportOnboardingPlans for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan", "batch",
            "ExportOnboardingPlans", "accepted", token);
        var input0 = new ApiOnboarding.ExplainOnboardingTaskCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.ExplainOnboardingTaskCalculationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        var command0 = new DispatchCommand<ApiOnboarding.ExplainOnboardingTaskCalculationRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _onboardingOnboardingTaskCalculationService.ExplainOnboardingTaskCalculationAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiOnboarding.ExportOnboardingPlansResponse
        {
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
                Counterparty = new ApiOnboarding.EquipmentRef
                {
                    Code = (reply0.OnboardingTask?.Customer?.Code ?? ""),
                    DisplayName = (reply0.OnboardingTask?.Customer?.DisplayName ?? ""),
                },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Format ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan",
            "audit", "ExportOnboardingPlans", (request.Format ?? ""), token);
        await responses.WriteAsync(response, token);
    }
    /// <summary>
    /// Lists onboarding plans a page at a time.
    /// </summary>
    public override async Task<ApiOnboarding.FindOnboardingPlansResponse> FindOnboardingPlans(ApiOnboarding.FindOnboardingPlansRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling FindOnboardingPlans for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan", "batch",
            "FindOnboardingPlans", "accepted", token);
        var profile0 = new MappingProfile<ApiOnboarding.FindOnboardingPlansRequest, ApiWorkplaces.ValidateRoomBookingRequest>(value => new ApiWorkplaces.ValidateRoomBookingRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiWorkplaces.ValidateRoomBookingRequest.Types.Details
            {
                Locale = (value.Context?.Locale ?? ""),
            },
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                Location = new ApiCommon.PostalAddress
                {
                    Region = (value.Filter?.Region ?? ""),
                },
                Organizer = new ApiIdentity.UserRef
                {
                    UserId = (value.Context?.Actor?.UserId ?? ""),
                },
                Participants = { new ApiWorkplaces.RoomBookingParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (value.Context?.Actor?.Email ?? ""),
                    },
                } },
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _workplacesRoomBookingValidationService.ValidateRoomBookingAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiOnboarding.FindOnboardingPlansResponse
        {
            OnboardingPlans = { new ApiOnboarding.OnboardingPlan
            {
                Counterparty = new ApiOnboarding.EquipmentRef
                {
                    DisplayName = (reply0.RoomBooking?.Organizer?.DisplayName ?? ""),
                },
            } },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan",
            "audit", "FindOnboardingPlans", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an onboarding plan.
    /// </summary>
    public override async Task<ApiOnboarding.LookupOnboardingPlanResponse> LookupOnboardingPlan(ApiOnboarding.LookupOnboardingPlanRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.OnboardingPlanId, "onboarding_plan_id");
        _logger.LogInformation("Handling LookupOnboardingPlan for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan", request.OnboardingPlanId,
            "LookupOnboardingPlan", "accepted", token);
        var response = new ApiOnboarding.LookupOnboardingPlanResponse
        {
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                OnboardingPlanId = (request.OnboardingPlanId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Returns an onboarding plan.
    /// </summary>
    public override async Task<ApiOnboarding.PeekOnboardingPlanResponse> PeekOnboardingPlan(ApiOnboarding.PeekOnboardingPlanRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.OnboardingPlanId, "onboarding_plan_id");
        _logger.LogInformation("Handling PeekOnboardingPlan for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan", request.OnboardingPlanId,
            "PeekOnboardingPlan", "accepted", token);
        var response = new ApiOnboarding.PeekOnboardingPlanResponse
        {
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                OnboardingPlanId = (request.OnboardingPlanId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Returns several onboarding plans at once.
    /// </summary>
    public override async Task<ApiOnboarding.ResolveOnboardingPlansResponse> ResolveOnboardingPlans(ApiOnboarding.ResolveOnboardingPlansRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ResolveOnboardingPlans for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan", "batch",
            "ResolveOnboardingPlans", "accepted", token);
        var input0 = new ApiNotifications.NormalizeNotificationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Notification = new ApiNotifications.Notification
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
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _notificationsNotificationValidationService.NormalizeNotificationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiOnboarding.ResolveOnboardingPlansResponse
        {
            OnboardingPlans = { new ApiOnboarding.OnboardingPlan
            {
                Counterparty = new ApiOnboarding.EquipmentRef
                {
                    DisplayName = (reply0.Notification?.Assignee?.DisplayName ?? ""),
                },
                DueAt = (reply0.Notification?.Sla?.DueAt ?? new Timestamp()),
            } },
        };
        return response;
    }
    /// <summary>
    /// Lists onboarding plans a page at a time.
    /// </summary>
    public override async Task<ApiOnboarding.SuggestOnboardingPlansResponse> SuggestOnboardingPlans(ApiOnboarding.SuggestOnboardingPlansRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SuggestOnboardingPlans for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan", "batch",
            "SuggestOnboardingPlans", "accepted", token);
        var response = new ApiOnboarding.SuggestOnboardingPlansResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan",
            "audit", "SuggestOnboardingPlans", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Summarizes an onboarding plan.
    /// </summary>
    public override async Task<ApiOnboarding.SummarizeOnboardingPlansResponse> SummarizeOnboardingPlans(ApiOnboarding.SummarizeOnboardingPlansRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SummarizeOnboardingPlans for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan", request.OnboardingPlanId,
            "SummarizeOnboardingPlans", "accepted", token);
        var input0 = request.SummarizeOnboardingPlansToCheckKnowledgeArticleEligibility0();
        using var rpc0 = _helpdeskKnowledgeArticleValidationService.CheckKnowledgeArticleEligibilityAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.PushDocumentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.PushDocumentRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Document = new ApiDocuments.Document
            {
                Card = new ApiDocuments.DocumentCardSettlement
                {
                    Network = (reply0.KnowledgeArticle ?? new ApiHelpdesk.KnowledgeArticle()).PaymentCase switch
                    {
                        ApiHelpdesk.KnowledgeArticle.SettlementOneofCase.Card => (reply0.KnowledgeArticle?.Card?.Network ?? ""),
                        _ => ""
                    },
                    Token = (reply0.KnowledgeArticle ?? new ApiHelpdesk.KnowledgeArticle()).PaymentCase switch
                    {
                        ApiHelpdesk.KnowledgeArticle.SettlementOneofCase.Card => (reply0.KnowledgeArticle?.Card?.Token ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _documentsDocumentSyncService.PushDocumentAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiOnboarding.SummarizeOnboardingPlansResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Message = (reply0.Result?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.OnboardingPlanId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan",
            "audit", "SummarizeOnboardingPlans", (request.OnboardingPlanId ?? ""), token);
        return response;
    }
}
