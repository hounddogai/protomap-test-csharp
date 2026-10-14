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
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiScheduling = Acme.Scheduling.V1;

namespace Acme.Workforce.Scheduling;

public sealed partial class AvailabilityWindowValidationEndpoint
{
    /// <summary>
    /// Checks an availability window.
    /// </summary>
    public override async Task<ApiScheduling.CheckAvailabilityWindowEligibilityResponse> CheckAvailabilityWindowEligibility(ApiScheduling.CheckAvailabilityWindowEligibilityRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CheckAvailabilityWindowEligibility for {Tenant}", requestContext.TenantId);
        if (request.AvailabilityWindow is { } record)
        {
            var snapshot = AvailabilityWindowMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiPerformance.ValidateGoalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Goal = new ApiPerformance.Goal
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.AvailabilityWindow?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Comments = { new ApiPerformance.GoalComment
                {
                    CreatedAt = (request.AvailabilityWindow?.Audit?.CreatedAt ?? new Timestamp()),
                } },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                    Name = (request.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _performanceGoalValidationService.ValidateGoalAsync);
        var input1 = new ApiScheduling.RecalculateAvailabilityWindowsRequest
        {
            AvailabilityWindow = new ApiScheduling.AvailabilityWindow
            {
                Organizer = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Goal ?? new ApiPerformance.Goal()).OriginCase switch
                    {
                        ApiPerformance.Goal.OriginOneofCase.Customer => (reply0.Goal?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                    UserId = (request.AvailabilityWindow?.Organizer?.UserId ?? ""),
                },
                Participants = { new ApiScheduling.AvailabilityWindowParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Phone = (request.Details?.Contact?.Phone ?? ""),
                    },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc1 = _schedulingAvailabilityWindowCalculationService.RecalculateAvailabilityWindowsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiScheduling.CheckAvailabilityWindowEligibilityResponse
        {
            AvailabilityWindow = new ApiScheduling.AvailabilityWindow
            {
                AvailabilityWindowId = (request.AvailabilityWindowId ?? ""),
                Metadata = { (request.AvailabilityWindow?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Organizer = new ApiIdentity.UserRef
                {
                    Email = (reply0.Goal?.Assignee?.Email ?? ""),
                    UserId = (reply1.AvailabilityWindow?.Organizer?.UserId ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "availability_window",
            "audit", "CheckAvailabilityWindowEligibility", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains an availability window.
    /// </summary>
    public override async Task<ApiScheduling.ExplainAvailabilityWindowDecisionResponse> ExplainAvailabilityWindowDecision(ApiScheduling.ExplainAvailabilityWindowDecisionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExplainAvailabilityWindowDecision for {Tenant}", requestContext.TenantId);
        if (request.AvailabilityWindow is { } record)
        {
            var snapshot = AvailabilityWindowMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiMetrics.SuggestMetricSeriesesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiMetrics.SuggestMetricSeriesesRequest.Types.Filter
            {
                Region = (request.AvailabilityWindow?.Location?.Region ?? ""),
            },
        };
        var named0 = _registry.Get<ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient>("acme.metrics.v1.MetricSeriesQueryService");
        using var rpc0 = named0.SuggestMetricSeriesesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiScheduling.RecalculateSwapRequestsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SwapRequest = new ApiScheduling.SwapRequest
            {
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.AvailabilityWindow?.Organizer?.Email ?? ""),
                },
            },
        };
        var descriptor1 = new Method<ApiScheduling.RecalculateSwapRequestsRequest, ApiScheduling.RecalculateSwapRequestsResponse>(
            MethodType.Unary, "acme.scheduling.v1" + ".SwapRequestCalculationService", "RecalculateSwapRequests",
            Wire.Marshaller<ApiScheduling.RecalculateSwapRequestsRequest>(), Wire.Marshaller<ApiScheduling.RecalculateSwapRequestsResponse>());
        using var rpc1 = _invoker.AsyncUnaryCall(descriptor1, null,
            new CallOptions(cancellationToken: token), input1);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiScheduling.ExplainAvailabilityWindowDecisionResponse
        {
            AvailabilityWindow = new ApiScheduling.AvailabilityWindow
            {
                AvailabilityWindowId = (request.AvailabilityWindowId ?? ""),
                Organizer = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.SwapRequest?.Assignee?.DisplayName ?? ""),
                },
                Status = (request.AvailabilityWindow?.Status ?? (ApiScheduling.AvailabilityWindowStatus)0),
            },
            Outcome = new ApiScheduling.ExplainAvailabilityWindowDecisionResponse.Types.Outcome
            {
                Status = (reply1.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "availability_window",
            "audit", "ExplainAvailabilityWindowDecision", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an availability window.
    /// </summary>
    public override async Task<ApiScheduling.GetAvailabilityWindowRulesResponse> GetAvailabilityWindowRules(ApiScheduling.GetAvailabilityWindowRulesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.AvailabilityWindowId, "availability_window_id");
        _logger.LogInformation("Handling GetAvailabilityWindowRules for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "availability_window", request.AvailabilityWindowId,
            "GetAvailabilityWindowRules", "accepted", token);
        var input0 = new ApiDocuments.ValidateDocumentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ValidateDocumentRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Locale = (request.Context?.Locale ?? ""),
            },
            Document = new ApiDocuments.Document
            {
                IssuedAt = (request.Context?.Deadline ?? new Timestamp()),
            },
        };
        var command0 = new DispatchCommand<ApiDocuments.ValidateDocumentRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _documentsDocumentValidationService.ValidateDocumentAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiScheduling.GetAvailabilityWindowRulesResponse
        {
            AvailabilityWindow = new ApiScheduling.AvailabilityWindow
            {
                AvailabilityWindowId = (request.AvailabilityWindowId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lints an availability window.
    /// </summary>
    public override async Task<ApiScheduling.LintAvailabilityWindowsResponse> LintAvailabilityWindows(ApiScheduling.LintAvailabilityWindowsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling LintAvailabilityWindows for {Tenant}", requestContext.TenantId);
        if (request.AvailabilityWindow is { } record)
        {
            var snapshot = AvailabilityWindowMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var profile0 = new MappingProfile<ApiScheduling.LintAvailabilityWindowsRequest, ApiScheduling.SimulateAvailabilityWindowRequest>(value => new ApiScheduling.SimulateAvailabilityWindowRequest
        {
            AvailabilityWindow = new ApiScheduling.AvailabilityWindow
            {
                Participants = { new ApiScheduling.AvailabilityWindowParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Phone = (value.Details?.Contact?.Phone ?? ""),
                    },
                } },
            },
            Context = (value.Context ?? new ApiCommon.RequestContext()),
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _schedulingAvailabilityWindowCalculationService.SimulateAvailabilityWindowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLedger.PreviewLedgerAccountRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLedger.PreviewLedgerAccountRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                    Name = (request.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
                },
            },
            LedgerAccount = new ApiLedger.LedgerAccount
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.AvailabilityWindow?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        input1 = PropertyMap.Copy(input1);
        using var rpc1 = _ledgerLedgerAccountValidationService.PreviewLedgerAccountAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiScheduling.LintAvailabilityWindowsResponse
        {
            AvailabilityWindow = new ApiScheduling.AvailabilityWindow
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.AvailabilityWindow?.Audit?.Etag ?? ""),
                    UpdatedBy = (reply1.LedgerAccount?.Audit?.UpdatedBy ?? ""),
                },
                AvailabilityWindowId = (request.AvailabilityWindowId ?? ""),
                Location = (reply0.AvailabilityWindow?.Location ?? new ApiCommon.PostalAddress()),
                Organizer = (request.AvailabilityWindow?.Organizer ?? new ApiIdentity.UserRef()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "availability_window",
            "audit", "LintAvailabilityWindows", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Previews an availability window.
    /// </summary>
    public override async Task<ApiScheduling.PreviewAvailabilityWindowResponse> PreviewAvailabilityWindow(ApiScheduling.PreviewAvailabilityWindowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PreviewAvailabilityWindow for {Tenant}", requestContext.TenantId);
        if (request.AvailabilityWindow is { } record)
        {
            var snapshot = AvailabilityWindowMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = request.PreviewAvailabilityWindowToGetAvailabilityWindowRates0();
        using var rpc0 = _schedulingAvailabilityWindowCalculationService.GetAvailabilityWindowRatesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiScheduling.ExplainRosterDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiScheduling.ExplainRosterDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.Contact?.Phone ?? ""),
                },
            },
            Roster = new ApiScheduling.Roster
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.AvailabilityWindow?.Audit?.UpdatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.AvailabilityWindow?.Location?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (reply0.AvailabilityWindow?.Organizer?.Email ?? ""),
                },
            },
        };
        var reply1 = await SendPreviewAvailabilityWindow1Async(input1, token);
        var input2 = new ApiScheduling.ValidateShiftRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Shift = new ApiScheduling.Shift
            {
                Participants = { new ApiScheduling.ShiftParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Name = (request.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
                        Phone = (request.Details?.Contact?.Phone ?? ""),
                    },
                } },
            },
        };
        var labels2 = input2.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input2.Context.Baggage.Clear();
        input2.Context.Baggage.Add(labels2);
        using var rpc2 = _schedulingShiftValidationService.ValidateShiftAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiScheduling.ValidateShiftRequest>("acme.scheduling.v1.AvailabilityWindowValidationService/PreviewAvailabilityWindow/2", input2.Clone()), token);
        var reply3 = new ApiScheduling.ExplainAvailabilityWindowCalculationResponse();
        foreach (var item in (request.AvailabilityWindow?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiScheduling.AvailabilityWindowParticipant>()))
        {
            var input3 = new ApiScheduling.ExplainAvailabilityWindowCalculationRequest
            {
                AvailabilityWindow = new ApiScheduling.AvailabilityWindow
                {
                    Organizer = new ApiIdentity.UserRef
                    {
                        Email = (item.Contact?.Email ?? ""),
                    },
                    Participants = { new ApiScheduling.AvailabilityWindowParticipant
                    {
                        Contact = new ApiCommon.ContactInfo
                        {
                            Phone = (item.Contact?.Phone ?? ""),
                        },
                    } },
                },
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiScheduling.ExplainAvailabilityWindowCalculationRequest.Types.Details
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        PreferredChannel = (item.Contact?.PreferredChannel ?? ""),
                    },
                },
            };
            using var rpc3 = _schedulingAvailabilityWindowCalculationService.ExplainAvailabilityWindowCalculationAsync(input3, cancellationToken: token);
            reply3 = await rpc3.ResponseAsync;
        }
        var response = new ApiScheduling.PreviewAvailabilityWindowResponse
        {
            AvailabilityWindow = new ApiScheduling.AvailabilityWindow
            {
                AvailabilityWindowId = (request.AvailabilityWindowId ?? ""),
                Location = new ApiCommon.PostalAddress
                {
                    Region = (reply0.AvailabilityWindow?.Location?.Region ?? ""),
                },
                Organizer = (request.AvailabilityWindow?.Organizer ?? new ApiIdentity.UserRef()),
                Participants = { new ApiScheduling.AvailabilityWindowParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (reply1.Roster?.Counterparty?.Email ?? ""),
                    },
                } },
                Title = (reply0.AvailabilityWindow?.Title ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply2.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "availability_window",
            "audit", "PreviewAvailabilityWindow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Scores an availability window.
    /// </summary>
    public override async Task<ApiScheduling.ScoreAvailabilityWindowResponse> ScoreAvailabilityWindow(ApiScheduling.ScoreAvailabilityWindowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScoreAvailabilityWindow for {Tenant}", requestContext.TenantId);
        if (request.AvailabilityWindow is { } record)
        {
            var snapshot = AvailabilityWindowMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiScheduling.CompareAvailabilityWindowOptionsResponse();
        foreach (var item in (request.AvailabilityWindow?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiScheduling.AvailabilityWindowParticipant>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input0 = new ApiScheduling.CompareAvailabilityWindowOptionsRequest
            {
                AvailabilityWindow = new ApiScheduling.AvailabilityWindow
                {
                    Participants = { new ApiScheduling.AvailabilityWindowParticipant
                    {
                        Contact = new ApiCommon.ContactInfo
                        {
                            Phone = (item.Contact?.Phone ?? ""),
                        },
                        ParticipantId = (item.ParticipantId ?? ""),
                        Required = item.Required,
                    } },
                },
                Context = (request.Context ?? new ApiCommon.RequestContext()),
            };
            using var rpc0 = _schedulingAvailabilityWindowCalculationService.CompareAvailabilityWindowOptionsAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var input1 = new ApiScheduling.QuoteRosterRequest();
        input1.Context = (request.Context ?? new ApiCommon.RequestContext());
        input1.Roster = new ApiScheduling.Roster
        {
            Audit = new ApiCommon.AuditInfo
            {
                UpdatedBy = (request.AvailabilityWindow?.Audit?.UpdatedBy ?? ""),
            },
            BillingAddress = new ApiCommon.PostalAddress
            {
                Locality = (reply0.AvailabilityWindow?.Location?.Locality ?? ""),
                Region = (request.Details?.Destination?.Region ?? ""),
            },
        };
        using var rpc1 = _schedulingRosterCalculationService.QuoteRosterAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDevices.EstimateDevicePolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.EstimateDevicePolicyRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.AvailabilityWindow?.Location?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Recipient = (request.Details?.Destination?.Recipient ?? ""),
                },
            },
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.Roster?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (reply0.AvailabilityWindow?.Organizer?.UserId ?? ""),
                },
            },
        };
        using var rpc2 = _devicesDevicePolicyCalculationService.EstimateDevicePolicyAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiScheduling.GetAvailabilityWindowRatesRequest
        {
            AvailabilityWindowId = (request.AvailabilityWindow?.AvailabilityWindowId ?? ""),
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply3 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _schedulingAvailabilityWindowCalculationService.GetAvailabilityWindowRatesAsync(input3, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiScheduling.ScoreAvailabilityWindowResponse
        {
            AvailabilityWindow = new ApiScheduling.AvailabilityWindow
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply2.DevicePolicy?.Audit?.CreatedAt ?? new Timestamp()),
                },
                AvailabilityWindowId = (request.AvailabilityWindowId ?? ""),
                Location = (request.AvailabilityWindow?.Location ?? new ApiCommon.PostalAddress()),
                Organizer = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.Roster?.Counterparty?.DisplayName ?? ""),
                },
            },
            Breakdown = new ApiScheduling.ScoreAvailabilityWindowResponse.Types.Breakdown
            {
                Total = (reply1.Breakdown?.Total ?? new ApiCommon.Money()),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply3.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "availability_window",
            "audit", "ScoreAvailabilityWindow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Validates an availability window.
    /// </summary>
    public override async Task<ApiScheduling.ValidateAvailabilityWindowResponse> ValidateAvailabilityWindow(ApiScheduling.ValidateAvailabilityWindowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ValidateAvailabilityWindow for {Tenant}", requestContext.TenantId);
        if (request.AvailabilityWindow is { } record)
        {
            var snapshot = AvailabilityWindowMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiScheduling.CalculateSwapRequestRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiScheduling.CalculateSwapRequestRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (request.AvailabilityWindow?.Location?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Recipient = (request.AvailabilityWindow?.Location?.Recipient ?? ""),
                    Region = (request.AvailabilityWindow?.Location?.Region ?? ""),
                },
            },
            SwapRequest = new ApiScheduling.SwapRequest
            {
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.AvailabilityWindow?.Organizer?.Email ?? ""),
                },
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _schedulingSwapRequestCalculationService.CalculateSwapRequestAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiScheduling.ValidateAvailabilityWindowResponse
        {
            AvailabilityWindow = new ApiScheduling.AvailabilityWindow
            {
                AvailabilityWindowId = (request.AvailabilityWindowId ?? ""),
                Participants = { new ApiScheduling.AvailabilityWindowParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Name = (reply0.SwapRequest?.Requester?.Name ?? new ApiCommon.PersonName()),
                    },
                } },
                Status = (request.AvailabilityWindow?.Status ?? (ApiScheduling.AvailabilityWindowStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "availability_window",
            "audit", "ValidateAvailabilityWindow", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiScheduling.ExplainRosterDecisionResponse> SendPreviewAvailabilityWindow1Async(ApiScheduling.ExplainRosterDecisionRequest request, CancellationToken token)
    {
        return await _availabilityWindowValidationEndpointSendPreviewAvailabilityWindow1Port.SendAsync(request, token);
    }
}
