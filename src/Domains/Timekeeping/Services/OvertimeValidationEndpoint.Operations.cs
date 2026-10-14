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
using ApiIdentity = Acme.Identity.V1;
using ApiMessaging = Acme.Messaging.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Timekeeping;

public sealed partial class OvertimeValidationEndpoint
{
    /// <summary>
    /// Checks an overtime.
    /// </summary>
    public override async Task<ApiTimekeeping.CheckOvertimeEligibilityResponse> CheckOvertimeEligibility(ApiTimekeeping.CheckOvertimeEligibilityRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CheckOvertimeEligibility for {Tenant}", requestContext.TenantId);
        if (request.Overtime is { } record)
        {
            var snapshot = OvertimeMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTimekeeping.LintClockEventsRequest
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTimekeeping.LintClockEventsRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.Details?.Contact?.Phone ?? ""),
                    PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
                },
                Locale = (request.Details?.Locale ?? ""),
            },
        };
        using var rpc0 = _timekeepingClockEventValidationService.LintClockEventsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTimekeeping.CheckOvertimeEligibilityResponse
        {
            Outcome = new ApiTimekeeping.CheckOvertimeEligibilityResponse.Types.Outcome
            {
                ReferenceId = (reply0.Outcome?.ReferenceId ?? ""),
            },
            Overtime = new ApiTimekeeping.Overtime
            {
                OvertimeId = (request.OvertimeId ?? ""),
                Status = (request.Overtime?.Status ?? (ApiTimekeeping.OvertimeStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "overtime",
            "audit", "CheckOvertimeEligibility", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains an overtime.
    /// </summary>
    public override async Task<ApiTimekeeping.ExplainOvertimeDecisionResponse> ExplainOvertimeDecision(ApiTimekeeping.ExplainOvertimeDecisionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExplainOvertimeDecision for {Tenant}", requestContext.TenantId);
        if (request.Overtime is { } record)
        {
            var snapshot = OvertimeMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTimekeeping.ExplainClockEventDecisionRequest
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTimekeeping.ExplainClockEventDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                    Name = (request.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
                },
            },
        };
        using var rpc0 = _timekeepingClockEventValidationService.ExplainClockEventDecisionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTimekeeping.ExplainOvertimeDecisionResponse
        {
            Overtime = new ApiTimekeeping.Overtime
            {
                Effective = (request.Overtime?.Effective ?? new ApiCommon.TimeWindow()),
                OvertimeId = (request.OvertimeId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.ClockEvent?.CorrelationId ?? ""),
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "overtime",
            "audit", "ExplainOvertimeDecision", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an overtime.
    /// </summary>
    public override async Task<ApiTimekeeping.GetOvertimeRulesResponse> GetOvertimeRules(ApiTimekeeping.GetOvertimeRulesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.OvertimeId, "overtime_id");
        _logger.LogInformation("Handling GetOvertimeRules for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "overtime", request.OvertimeId,
            "GetOvertimeRules", "accepted", token);
        var response = new ApiTimekeeping.GetOvertimeRulesResponse
        {
            Overtime = new ApiTimekeeping.Overtime
            {
                OvertimeId = (request.OvertimeId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lints an overtime.
    /// </summary>
    public override async Task<ApiTimekeeping.LintOvertimesResponse> LintOvertimes(ApiTimekeeping.LintOvertimesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling LintOvertimes for {Tenant}", requestContext.TenantId);
        if (request.Overtime is { } record)
        {
            var snapshot = OvertimeMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiNotifications.ResolveNotificationsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _notificationsNotificationQueryService.ResolveNotificationsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTimekeeping.LintOvertimesResponse
        {
            Overtime = new ApiTimekeeping.Overtime
            {
                OvertimeId = (request.OvertimeId ?? ""),
                Priority = (request.Overtime?.Priority ?? 0),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "overtime",
            "audit", "LintOvertimes", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Previews an overtime.
    /// </summary>
    public override async Task<ApiTimekeeping.PreviewOvertimeResponse> PreviewOvertime(ApiTimekeeping.PreviewOvertimeRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PreviewOvertime for {Tenant}", requestContext.TenantId);
        if (request.Overtime is { } record)
        {
            var snapshot = OvertimeMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiBenefits.CountBenefitPlansRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiBenefits.CountBenefitPlansRequest.Types.Filter
            {
                CreatedAfter = (request.Overtime?.Audit?.CreatedAt ?? new Timestamp()),
                OwnerId = (request.Overtime?.OvertimeId ?? ""),
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiBenefits.CountBenefitPlansRequest>();
        using var rpc0 = _benefitsBenefitPlanQueryService.CountBenefitPlansAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTimekeeping.PreviewClockEventRequest
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTimekeeping.PreviewClockEventRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.Contact?.Phone ?? ""),
                    PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc1 = _timekeepingClockEventValidationService.PreviewClockEventAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        var input2 = new ApiTravel.EstimateTravelPolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.Overtime?.Audit?.UpdatedBy ?? ""),
                },
                Block = new ApiTravel.TravelPolicyBlockAction
                {
                    NotifyEmail = (request.Overtime?.Block?.NotifyEmail ?? ""),
                    Reason = (request.Overtime?.Block?.Reason ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Overtime?.Owner?.DisplayName ?? ""),
                },
            },
        };
        using var rpc2 = _travelTravelPolicyCalculationService.EstimateTravelPolicyAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        await Task.WhenAll(pending1, pending2);
        var reply1 = await pending1;
        var reply2 = await pending2;
        var input3 = new ApiTimekeeping.GetOvertimeRatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            OvertimeId = (request.OvertimeId ?? ""),
        };
        using var rpc3 = _timekeepingOvertimeCalculationService.GetOvertimeRatesAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiTimekeeping.PreviewOvertimeResponse
        {
            Overtime = new ApiTimekeeping.Overtime
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply2.TravelPolicy?.Audit?.CreatedBy ?? ""),
                    Etag = (reply3.Overtime?.Audit?.Etag ?? ""),
                },
                OvertimeId = (request.OvertimeId ?? ""),
                Owner = (request.Overtime?.Owner ?? new ApiIdentity.UserRef()),
                Parameters = { (reply2.TravelPolicy?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Message = (reply1.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "overtime",
            "audit", "PreviewOvertime", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Scores an overtime.
    /// </summary>
    public override async Task<ApiTimekeeping.ScoreOvertimeResponse> ScoreOvertime(ApiTimekeeping.ScoreOvertimeRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScoreOvertime for {Tenant}", requestContext.TenantId);
        if (request.Overtime is { } record)
        {
            var snapshot = OvertimeMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiNotifications.NormalizeNotificationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiNotifications.NormalizeNotificationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Overtime?.Owner?.Email ?? ""),
                },
            },
            Notification = new ApiNotifications.Notification
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Overtime?.Owner?.DisplayName ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Overtime?.Audit?.Etag ?? ""),
                },
                Comments = { new ApiNotifications.NotificationComment
                {
                    CreatedAt = (request.Overtime?.Audit?.CreatedAt ?? new Timestamp()),
                } },
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _notificationsNotificationValidationService.NormalizeNotificationAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiTimekeeping.ScoreClockEventRequest
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTimekeeping.ScoreClockEventRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Details?.Destination?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
        };
        using var rpc1 = _timekeepingClockEventValidationService.ScoreClockEventAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTimekeeping.ScoreOvertimeResponse
        {
            Overtime = new ApiTimekeeping.Overtime
            {
                OvertimeId = (request.OvertimeId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply0.Notification?.Requester?.Email ?? ""),
                },
                Status = (request.Overtime?.Status ?? (ApiTimekeeping.OvertimeStatus)0),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.ClockEvent?.CorrelationId ?? ""),
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "overtime",
            "audit", "ScoreOvertime", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Validates an overtime.
    /// </summary>
    public override async Task<ApiTimekeeping.ValidateOvertimeResponse> ValidateOvertime(ApiTimekeeping.ValidateOvertimeRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ValidateOvertime for {Tenant}", requestContext.TenantId);
        if (request.Overtime is { } record)
        {
            var snapshot = OvertimeMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiMessaging.SimulateConversationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Conversation = new ApiMessaging.Conversation
            {
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                    PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _messagingConversationCalculationService.SimulateConversationAsync);
        var input1 = new ApiTimekeeping.ExplainTimesheetDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTimekeeping.ExplainTimesheetDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _sendValidateOvertime1(input1, new CallOptions(cancellationToken: token));
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiTimekeeping.EstimateClockEventRequest
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTimekeeping.EstimateClockEventRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Locality = (reply1.Timesheet?.BillingAddress?.Locality ?? ""),
                    PostalCode = (reply1.Timesheet?.BillingAddress?.PostalCode ?? ""),
                },
            },
        };
        using var rpc2 = _timekeepingClockEventCalculationService.EstimateClockEventAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiTimekeeping.ValidateOvertimeResponse
        {
            Outcome = new ApiTimekeeping.ValidateOvertimeResponse.Types.Outcome
            {
                ReferenceId = (reply1.Outcome?.ReferenceId ?? ""),
            },
            Overtime = new ApiTimekeeping.Overtime
            {
                Name = (request.Overtime?.Name ?? ""),
                OvertimeId = (request.OvertimeId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply2.ClockEvent?.Subject?.DisplayName ?? ""),
                    Email = (reply0.Conversation?.Assignee?.Email ?? ""),
                },
                Priority = (reply0.Conversation?.Priority ?? 0),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "overtime",
            "audit", "ValidateOvertime", (request.Reason ?? ""), token);
        return response;
    }
}
