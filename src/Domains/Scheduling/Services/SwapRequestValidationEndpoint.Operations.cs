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
using ApiConfiguration = Acme.Configuration.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiScheduling = Acme.Scheduling.V1;

namespace Acme.Workforce.Scheduling;

public sealed partial class SwapRequestValidationEndpoint
{
    /// <summary>
    /// Checks a swap request.
    /// </summary>
    public override async Task<ApiScheduling.CheckSwapRequestEligibilityResponse> CheckSwapRequestEligibility(ApiScheduling.CheckSwapRequestEligibilityRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CheckSwapRequestEligibility for {Tenant}", requestContext.TenantId);
        if (request.SwapRequest is { } record)
        {
            var snapshot = SwapRequestMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = request.CheckSwapRequestEligibilityToResolveLearningPaths0();
        using var rpc0 = _learningLearningPathQueryService.ResolveLearningPathsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiScheduling.CheckSwapRequestEligibilityResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            SwapRequest = new ApiScheduling.SwapRequest
            {
                Subject = (request.SwapRequest?.Subject ?? ""),
                SwapRequestId = (request.SwapRequestId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SwapRequest?.SwapRequestId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "swap_request",
            "audit", "CheckSwapRequestEligibility", (request.SwapRequest?.SwapRequestId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a swap request.
    /// </summary>
    public override async Task<ApiScheduling.ExplainSwapRequestDecisionResponse> ExplainSwapRequestDecision(ApiScheduling.ExplainSwapRequestDecisionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExplainSwapRequestDecision for {Tenant}", requestContext.TenantId);
        if (request.SwapRequest is { } record)
        {
            var snapshot = SwapRequestMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiScheduling.CheckShiftEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Shift = new ApiScheduling.Shift
            {
                Participants = { new ApiScheduling.ShiftParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.SwapRequest?.Requester?.Email ?? ""),
                    },
                } },
            },
        };
        using var rpc0 = _schedulingShiftValidationService.CheckShiftEligibilityAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiConfiguration.SummarizeConfigSetsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Reason ?? ""),
        };
        using var rpc1 = _configurationConfigSetQueryService.SummarizeConfigSetsAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var response = new ApiScheduling.ExplainSwapRequestDecisionResponse
        {
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            SwapRequest = new ApiScheduling.SwapRequest
            {
                Assignee = (request.SwapRequest?.Assignee ?? new ApiIdentity.UserRef()),
                SwapRequestId = (request.SwapRequestId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SwapRequest?.SwapRequestId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "swap_request",
            "audit", "ExplainSwapRequestDecision", (request.SwapRequest?.SwapRequestId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a swap request.
    /// </summary>
    public override async Task<ApiScheduling.GetSwapRequestRulesResponse> GetSwapRequestRules(ApiScheduling.GetSwapRequestRulesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.SwapRequestId, "swap_request_id");
        _logger.LogInformation("Handling GetSwapRequestRules for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "swap_request", request.SwapRequestId,
            "GetSwapRequestRules", "accepted", token);
        var response = new ApiScheduling.GetSwapRequestRulesResponse
        {
            SwapRequest = new ApiScheduling.SwapRequest
            {
                SwapRequestId = (request.SwapRequestId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Normalizes a swap request.
    /// </summary>
    public override async Task<ApiScheduling.NormalizeSwapRequestResponse> NormalizeSwapRequest(ApiScheduling.NormalizeSwapRequestRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling NormalizeSwapRequest for {Tenant}", requestContext.TenantId);
        if (request.SwapRequest is { } record)
        {
            var snapshot = SwapRequestMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiScheduling.CompareRosterOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Roster = new ApiScheduling.Roster
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.SwapRequest?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.SwapRequest ?? new ApiScheduling.SwapRequest()).OriginCase switch
                    {
                        ApiScheduling.SwapRequest.OriginOneofCase.Customer => (request.SwapRequest?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                    Email = (request.SwapRequest ?? new ApiScheduling.SwapRequest()).OriginCase switch
                    {
                        ApiScheduling.SwapRequest.OriginOneofCase.Customer => (request.SwapRequest?.Customer?.Email ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc0 = _schedulingRosterCalculationService.CompareRosterOptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiScheduling.ExplainSwapRequestCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SwapRequest = new ApiScheduling.SwapRequest
            {
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.SwapRequest?.Assignee?.Email ?? ""),
                },
            },
        };
        var json1 = JsonFormatter.Default.Format(input1);
        input1 = JsonParser.Default.Parse<ApiScheduling.ExplainSwapRequestCalculationRequest>(json1);
        using var rpc1 = _schedulingSwapRequestCalculationService.ExplainSwapRequestCalculationAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var batch2 = (request.SwapRequest?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiScheduling.SwapRequestComment>()).Select(async item =>
        {
            var input2 = new ApiScheduling.NormalizeShiftRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiScheduling.NormalizeShiftRequest.Types.Details
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (item.AuthorEmail ?? ""),
                    },
                },
                Shift = new ApiScheduling.Shift
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedAt = (item.CreatedAt ?? new Timestamp()),
                    },
                },
            };
            using var rpc2 = _schedulingShiftValidationService.NormalizeShiftAsync(input2, cancellationToken: token);
            var reply2 = await rpc2.ResponseAsync;
            return reply2;
        }).ToArray();
        var replies2 = await Task.WhenAll(batch2);
        var reply2 = replies2.LastOrDefault() ?? new ApiScheduling.NormalizeShiftResponse();
        var reply3 = new ApiPerformance.ListReviewsResponse();
        foreach (var item in (request.SwapRequest?.History ?? new Google.Protobuf.Collections.RepeatedField<ApiScheduling.SwapRequestStatusChange>()))
        {
            var input3 = new ApiPerformance.ListReviewsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiPerformance.ListReviewsRequest.Types.Filter
                {
                    CreatedAfter = (item.ChangedAt ?? new Timestamp()),
                    Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                    OwnerId = (reply1.SwapRequest?.Assignee?.UserId ?? ""),
                    Region = (reply0.Roster?.BillingAddress?.Region ?? ""),
                },
            };
            using var rpc3 = _performanceReviewService.ListReviewsAsync(input3, cancellationToken: token);
            reply3 = await rpc3.ResponseAsync;
        }
        var response = new ApiScheduling.NormalizeSwapRequestResponse
        {
            SwapRequest = new ApiScheduling.SwapRequest
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (reply1.SwapRequest?.Customer?.Email ?? ""),
                },
                Comments = { new ApiScheduling.SwapRequestComment
                {
                    CreatedAt = (reply0.Roster?.Audit?.CreatedAt ?? new Timestamp()),
                } },
                Requester = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (reply1.SwapRequest?.Requester?.PreferredChannel ?? ""),
                },
                Status = (request.SwapRequest?.Status ?? (ApiScheduling.SwapRequestStatus)0),
                SwapRequestId = (request.SwapRequestId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SwapRequest?.SwapRequestId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "swap_request",
            "audit", "NormalizeSwapRequest", (request.SwapRequest?.SwapRequestId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Previews a swap request.
    /// </summary>
    public override async Task<ApiScheduling.PreviewSwapRequestResponse> PreviewSwapRequest(ApiScheduling.PreviewSwapRequestRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PreviewSwapRequest for {Tenant}", requestContext.TenantId);
        if (request.SwapRequest is { } record)
        {
            var snapshot = SwapRequestMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiScheduling.CheckShiftEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Shift = new ApiScheduling.Shift
            {
                Organizer = new ApiIdentity.UserRef
                {
                    DisplayName = (request.SwapRequest ?? new ApiScheduling.SwapRequest()).OriginCase switch
                    {
                        ApiScheduling.SwapRequest.OriginOneofCase.Customer => (request.SwapRequest?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                },
                Participants = { new ApiScheduling.ShiftParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
                    },
                } },
            },
        };
        using var rpc0 = _schedulingShiftValidationService.CheckShiftEligibilityAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiScheduling.NormalizeRosterRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Roster = new ApiScheduling.Roster
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (reply0.Shift?.Location?.Locality ?? ""),
                },
            },
        };
        var reply1 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _schedulingRosterValidationService.NormalizeRosterAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input2 = new ApiScheduling.ExplainAvailabilityWindowDecisionRequest
        {
            AvailabilityWindow = new ApiScheduling.AvailabilityWindow
            {
                Participants = { new ApiScheduling.AvailabilityWindowParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (reply1.Roster?.Counterparty?.Email ?? ""),
                        Phone = (request.Details?.Contact?.Phone ?? ""),
                        PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
                    },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply2 = await Workflow.SendAsync(input2, token, _schedulingAvailabilityWindowValidationService.ExplainAvailabilityWindowDecisionAsync);
        var input3 = new ApiLedger.ValidateLedgerAccountRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LedgerAccount = new ApiLedger.LedgerAccount
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply2.AvailabilityWindow?.Organizer?.Email ?? ""),
                    Name = (request.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
                },
            },
        };
        using var rpc3 = _sendPreviewSwapRequest3(input3, new CallOptions(cancellationToken: token));
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiScheduling.PreviewSwapRequestResponse
        {
            Outcome = new ApiScheduling.PreviewSwapRequestResponse.Types.Outcome
            {
                Message = (reply2.Outcome?.Message ?? ""),
                NextSteps = { (reply1.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply3.Result?.CorrelationId ?? ""),
            },
            SwapRequest = new ApiScheduling.SwapRequest
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.Roster?.Audit?.CreatedAt ?? new Timestamp()),
                    CreatedBy = (reply0.Shift?.Audit?.CreatedBy ?? ""),
                },
                Comments = { new ApiScheduling.SwapRequestComment
                {
                    CreatedAt = (reply3.LedgerAccount?.Audit?.CreatedAt ?? new Timestamp()),
                } },
                Subject = (request.SwapRequest?.Subject ?? ""),
                SwapRequestId = (request.SwapRequestId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SwapRequest?.SwapRequestId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "swap_request",
            "audit", "PreviewSwapRequest", (request.SwapRequest?.SwapRequestId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Scores a swap request.
    /// </summary>
    public override async Task<ApiScheduling.ScoreSwapRequestResponse> ScoreSwapRequest(ApiScheduling.ScoreSwapRequestRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScoreSwapRequest for {Tenant}", requestContext.TenantId);
        if (request.SwapRequest is { } record)
        {
            var snapshot = SwapRequestMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiScheduling.CalculateRosterRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiScheduling.CalculateRosterRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Details?.Destination?.CountryCode ?? ""),
                },
            },
            Roster = new ApiScheduling.Roster
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Details?.Destination?.PostalCode ?? ""),
                },
            },
        };
        var named0 = _registry.Get<ApiScheduling.RosterCalculationService.RosterCalculationServiceClient>("acme.scheduling.v1.RosterCalculationService");
        using var rpc0 = named0.CalculateRosterAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var reply1 = new ApiScheduling.SimulateRosterResponse();
        foreach (var item in (request.SwapRequest?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiScheduling.SwapRequestComment>()))
        {
            var input1 = new ApiScheduling.SimulateRosterRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiScheduling.SimulateRosterRequest.Types.Details
                {
                    Destination = new ApiCommon.PostalAddress
                    {
                        Locality = (request.Details?.Destination?.Locality ?? ""),
                    },
                },
                Roster = new ApiScheduling.Roster
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedAt = (item.CreatedAt ?? new Timestamp()),
                    },
                    BillingAddress = new ApiCommon.PostalAddress
                    {
                        CountryCode = (request.Details?.Destination?.CountryCode ?? ""),
                    },
                    Counterparty = new ApiEmployees.EmployeeRef
                    {
                        Email = (item.AuthorEmail ?? ""),
                    },
                },
            };
            using var rpc1 = _schedulingRosterCalculationService.SimulateRosterAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var input2 = new ApiScheduling.ScoreShiftRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiScheduling.ScoreShiftRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.Roster?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
            Shift = new ApiScheduling.Shift
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.Roster?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Location = new ApiCommon.PostalAddress
                {
                    Recipient = (reply1.Roster?.BillingAddress?.Recipient ?? ""),
                },
                Participants = { new ApiScheduling.ShiftParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (reply1.Roster?.Counterparty?.Email ?? ""),
                    },
                } },
            },
        };
        var command2 = new DispatchCommand<ApiScheduling.ScoreShiftRequest>(input2,
            "primary", requestContext.TenantId);
        var routed2 = command2 with { Region = _settings.Value.Region };
        using var rpc2 = _schedulingShiftValidationService.ScoreShiftAsync(routed2.Payload, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiScheduling.ScoreSwapRequestResponse
        {
            Breakdown = new ApiScheduling.ScoreSwapRequestResponse.Types.Breakdown
            {
                CurrencyCode = (reply2.Breakdown?.CurrencyCode ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            SwapRequest = new ApiScheduling.SwapRequest
            {
                Requester = (request.SwapRequest?.Requester ?? new ApiCommon.ContactInfo()),
                SwapRequestId = (request.SwapRequestId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SwapRequest?.SwapRequestId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "swap_request",
            "audit", "ScoreSwapRequest", (request.SwapRequest?.SwapRequestId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Validates a swap request.
    /// </summary>
    public override async Task<ApiScheduling.ValidateSwapRequestResponse> ValidateSwapRequest(ApiScheduling.ValidateSwapRequestRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ValidateSwapRequest for {Tenant}", requestContext.TenantId);
        if (request.SwapRequest is { } record)
        {
            var snapshot = SwapRequestMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiMetrics.NormalizeMetricSeriesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiMetrics.NormalizeMetricSeriesRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.SwapRequest ?? new ApiScheduling.SwapRequest()).OriginCase switch
                    {
                        ApiScheduling.SwapRequest.OriginOneofCase.Customer => (request.SwapRequest?.Customer?.Email ?? ""),
                        _ => ""
                    },
                },
            },
            MetricSeries = new ApiMetrics.MetricSeries
            {
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.SwapRequest?.Assignee?.UserId ?? ""),
                },
            },
        };
        using var rpc0 = _metricsMetricSeriesValidationService.NormalizeMetricSeriesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiScheduling.LintShiftsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiScheduling.LintShiftsRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.SwapRequest?.Requester?.Email ?? ""),
                },
            },
            Shift = new ApiScheduling.Shift
            {
                Participants = { new ApiScheduling.ShiftParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Details?.Contact?.Email ?? ""),
                        Phone = (request.SwapRequest?.Requester?.Phone ?? ""),
                    },
                } },
            },
        };
        input1 = PropertyMap.Copy(input1);
        using var rpc1 = _schedulingShiftValidationService.LintShiftsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiScheduling.ValidateSwapRequestResponse
        {
            SwapRequest = new ApiScheduling.SwapRequest
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.MetricSeries?.Owner?.DisplayName ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.Shift?.Audit?.Etag ?? ""),
                },
                Status = (request.SwapRequest?.Status ?? (ApiScheduling.SwapRequestStatus)0),
                SwapRequestId = (request.SwapRequestId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SwapRequest?.SwapRequestId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "swap_request",
            "audit", "ValidateSwapRequest", (request.SwapRequest?.SwapRequestId ?? ""), token);
        return response;
    }
}
