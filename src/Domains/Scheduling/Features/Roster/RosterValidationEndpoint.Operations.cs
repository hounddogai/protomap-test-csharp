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
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Scheduling;

public sealed partial class RosterValidationEndpoint
{
    /// <summary>
    /// Checks a roster.
    /// </summary>
    public override async Task<ApiScheduling.CheckRosterEligibilityResponse> CheckRosterEligibility(ApiScheduling.CheckRosterEligibilityRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CheckRosterEligibility for {Tenant}", requestContext.TenantId);
        if (request.Roster is { } record)
        {
            var snapshot = RosterMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiScheduling.CheckShiftEligibilityRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Shift = new ApiScheduling.Shift
        {
            Audit = new ApiCommon.AuditInfo
            {
                CreatedAt = (request.Roster?.Audit?.CreatedAt ?? new Timestamp()),
            },
        };
        using var rpc0 = _schedulingShiftValidationService.CheckShiftEligibilityAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTravel.ScoreTripRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.ScoreTripRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Roster?.BillingAddress?.CountryCode ?? ""),
                    Lines = { (request.Roster?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
            Trip = new ApiTravel.Trip
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Roster?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (reply0.Shift?.Location?.Locality ?? ""),
                },
            },
        };
        using var rpc1 = _travelTripValidationService.ScoreTripAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiWorkplaces.QuoteDeskRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Desk = new ApiWorkplaces.Desk
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Shift?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
            Details = new ApiWorkplaces.QuoteDeskRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Recipient = (reply1.Trip?.BillingAddress?.Recipient ?? ""),
                },
            },
        };
        using var rpc2 = _workplacesDeskCalculationService.QuoteDeskAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        var input3 = new ApiScheduling.CompareRosterOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Roster = new ApiScheduling.Roster
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.Trip?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        using var rpc3 = _schedulingRosterCalculationService.CompareRosterOptionsAsync(input3, cancellationToken: token);
        var pending3 = rpc3.ResponseAsync;
        await Task.WhenAll(pending2, pending3);
        var reply2 = await pending2;
        var reply3 = await pending3;
        var response = new ApiScheduling.CheckRosterEligibilityResponse
        {
            Outcome = new ApiScheduling.CheckRosterEligibilityResponse.Types.Outcome
            {
                Status = (reply0.Outcome?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
            Roster = new ApiScheduling.Roster
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply2.Desk?.Address?.PostalCode ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (reply1.Trip?.Counterparty?.DisplayName ?? ""),
                },
                Metadata = { (reply3.Roster?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Notes = (request.Roster?.Notes ?? ""),
                RosterId = (request.RosterId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Roster?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "roster",
            "audit", "CheckRosterEligibility", (request.Roster?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a roster.
    /// </summary>
    public override async Task<ApiScheduling.ExplainRosterDecisionResponse> ExplainRosterDecision(ApiScheduling.ExplainRosterDecisionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExplainRosterDecision for {Tenant}", requestContext.TenantId);
        if (request.Roster is { } record)
        {
            var snapshot = RosterMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiScheduling.CompareRosterOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Roster = new ApiScheduling.Roster
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Roster?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _schedulingRosterCalculationService.CompareRosterOptionsAsync);
        var response = new ApiScheduling.ExplainRosterDecisionResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            Roster = new ApiScheduling.Roster
            {
                Number = (request.Roster?.Number ?? ""),
                RosterId = (request.RosterId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Roster?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "roster",
            "audit", "ExplainRosterDecision", (request.Roster?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a roster.
    /// </summary>
    public override async Task<ApiScheduling.GetRosterRulesResponse> GetRosterRules(ApiScheduling.GetRosterRulesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.RosterId, "roster_id");
        _logger.LogInformation("Handling GetRosterRules for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "roster", request.RosterId,
            "GetRosterRules", "accepted", token);
        var input0 = new ApiScheduling.CalculateRosterRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Roster = new ApiScheduling.Roster
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                RosterId = (request.RosterId ?? ""),
            },
        };
        using var rpc0 = _sendGetRosterRules0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiScheduling.GetRosterRulesResponse
        {
            Roster = new ApiScheduling.Roster
            {
                BillingAddress = (reply0.Roster?.BillingAddress ?? new ApiCommon.PostalAddress()),
                DueAt = (reply0.Roster?.DueAt ?? new Timestamp()),
                RosterId = (request.RosterId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lints a roster.
    /// </summary>
    public override async Task<ApiScheduling.LintRostersResponse> LintRosters(ApiScheduling.LintRostersRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling LintRosters for {Tenant}", requestContext.TenantId);
        if (request.Roster is { } record)
        {
            var snapshot = RosterMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiScheduling.SimulateRosterRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiScheduling.SimulateRosterRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Roster?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    PostalCode = (request.Roster?.BillingAddress?.PostalCode ?? ""),
                },
            },
            Roster = new ApiScheduling.Roster
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Roster?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (request.Roster?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        var named0 = _registry.Get<ApiScheduling.RosterCalculationService.RosterCalculationServiceClient>("acme.scheduling.v1.RosterCalculationService");
        using var rpc0 = named0.SimulateRosterAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var batch1 = (request.Roster?.Attachments ?? new Google.Protobuf.Collections.RepeatedField<ApiScheduling.RosterAttachment>()).Select(async item =>
        {
            var input1 = new ApiScheduling.CompareRosterOptionsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Roster = new ApiScheduling.Roster
                {
                    Attachments = { new ApiScheduling.RosterAttachment
                    {
                        Checksum = (item.Checksum ?? ""),
                        FileName = (item.FileName ?? ""),
                        SizeBytes = item.SizeBytes,
                        Uri = (item.Uri ?? ""),
                    } },
                },
            };
            using var rpc1 = _schedulingRosterCalculationService.CompareRosterOptionsAsync(input1, cancellationToken: token);
            var reply1 = await rpc1.ResponseAsync;
            return reply1;
        }).ToArray();
        var replies1 = await Task.WhenAll(batch1);
        var reply1 = replies1.LastOrDefault() ?? new ApiScheduling.CompareRosterOptionsResponse();
        var input2 = new ApiScheduling.ExplainAvailabilityWindowCalculationRequest
        {
            AvailabilityWindow = new ApiScheduling.AvailabilityWindow
            {
                Organizer = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Roster?.Counterparty?.DisplayName ?? ""),
                },
                Participants = { new ApiScheduling.AvailabilityWindowParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Roster?.Counterparty?.Email ?? ""),
                        Phone = (request.Details?.Contact?.Phone ?? ""),
                    },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var command2 = new DispatchCommand<ApiScheduling.ExplainAvailabilityWindowCalculationRequest>(input2,
            "primary", requestContext.TenantId);
        var routed2 = command2 with { Region = _settings.Value.Region };
        using var rpc2 = _schedulingAvailabilityWindowCalculationService.ExplainAvailabilityWindowCalculationAsync(routed2.Payload, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiScheduling.LintRostersResponse
        {
            Outcome = new ApiScheduling.LintRostersResponse.Types.Outcome
            {
                Message = (reply2.Outcome?.Message ?? ""),
            },
            Roster = new ApiScheduling.Roster
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply2.AvailabilityWindow?.Location?.PostalCode ?? ""),
                },
                Notes = (reply0.Roster?.Notes ?? ""),
                RosterId = (request.RosterId ?? ""),
                Status = (request.Roster?.Status ?? (ApiScheduling.RosterStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Roster?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "roster",
            "audit", "LintRosters", (request.Roster?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Normalizes a roster.
    /// </summary>
    public override async Task<ApiScheduling.NormalizeRosterResponse> NormalizeRoster(ApiScheduling.NormalizeRosterRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling NormalizeRoster for {Tenant}", requestContext.TenantId);
        if (request.Roster is { } record)
        {
            var snapshot = RosterMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var profile0 = new MappingProfile<ApiScheduling.NormalizeRosterRequest, ApiScheduling.ExplainRosterCalculationRequest>(value => new ApiScheduling.ExplainRosterCalculationRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Roster = new ApiScheduling.Roster
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    EmployeeId = (value.Roster?.Counterparty?.EmployeeId ?? ""),
                },
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _schedulingRosterCalculationService.ExplainRosterCalculationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiScheduling.QuoteRosterRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Roster = new ApiScheduling.Roster
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Roster?.BillingAddress?.PostalCode ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (reply0.Roster?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        input1 = PropertyMap.Copy(input1);
        using var rpc1 = _schedulingRosterCalculationService.QuoteRosterAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiScheduling.NormalizeRosterResponse
        {
            Outcome = new ApiScheduling.NormalizeRosterResponse.Types.Outcome
            {
                Message = (reply0.Outcome?.Message ?? ""),
            },
            Roster = new ApiScheduling.Roster
            {
                Counterparty = (request.Roster?.Counterparty ?? new ApiEmployees.EmployeeRef()),
                DueAt = (reply1.Roster?.DueAt ?? new Timestamp()),
                IssuedAt = (reply1.Roster?.IssuedAt ?? new Timestamp()),
                RosterId = (request.RosterId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Roster?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "roster",
            "audit", "NormalizeRoster", (request.Roster?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Previews a roster.
    /// </summary>
    public override async Task<ApiScheduling.PreviewRosterResponse> PreviewRoster(ApiScheduling.PreviewRosterRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PreviewRoster for {Tenant}", requestContext.TenantId);
        if (request.Roster is { } record)
        {
            var snapshot = RosterMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiScheduling.SimulateRosterResponse();
        foreach (var item in (request.Roster?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiScheduling.RosterLine>()))
        {
            var input0 = new ApiScheduling.SimulateRosterRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Roster = new ApiScheduling.Roster
                {
                    Lines = { new ApiScheduling.RosterLine
                    {
                        Description = (item.Description ?? ""),
                        Tax = new ApiScheduling.RosterLineTax
                        {
                            Amount = (item.Tax?.Amount ?? new ApiCommon.Money()),
                            Rate = (item.Tax?.Rate ?? 0d),
                        },
                    } },
                },
            };
            using var rpc0 = _schedulingRosterCalculationService.SimulateRosterAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var input1 = new ApiScheduling.ExplainRosterCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Roster = new ApiScheduling.Roster
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (reply0.Roster?.BillingAddress?.Region ?? ""),
                },
            },
        };
        var reply1 = await SendPreviewRoster1Async(input1, token);
        var response = new ApiScheduling.PreviewRosterResponse
        {
            Roster = new ApiScheduling.Roster
            {
                BillingAddress = (request.Roster?.BillingAddress ?? new ApiCommon.PostalAddress()),
                Notes = (reply1.Roster?.Notes ?? ""),
                RosterId = (request.RosterId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Roster?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "roster",
            "audit", "PreviewRoster", (request.Roster?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Scores a roster.
    /// </summary>
    public override async Task<ApiScheduling.ScoreRosterResponse> ScoreRoster(ApiScheduling.ScoreRosterRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScoreRoster for {Tenant}", requestContext.TenantId);
        if (request.Roster is { } record)
        {
            var snapshot = RosterMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLearning.SuggestCoursesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLearning.SuggestCoursesRequest.Types.Filter
            {
                Region = (request.Details?.Destination?.Region ?? ""),
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _learningCourseQueryService.SuggestCoursesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiScheduling.CompareRosterOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Roster = new ApiScheduling.Roster
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Roster?.Audit?.CreatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Details?.Destination?.PostalCode ?? ""),
                },
            },
        };
        var packed1 = Any.Pack(input1);
        input1 = packed1.Unpack<ApiScheduling.CompareRosterOptionsRequest>();
        using var rpc1 = _schedulingRosterCalculationService.CompareRosterOptionsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiScheduling.ScoreRosterResponse
        {
            Breakdown = new ApiScheduling.ScoreRosterResponse.Types.Breakdown
            {
                Total = (reply1.Roster?.Total ?? new ApiCommon.Money()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
            Roster = new ApiScheduling.Roster
            {
                Metadata = { (request.Roster?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                RosterId = (request.RosterId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Roster?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "roster",
            "audit", "ScoreRoster", (request.Roster?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Validates a roster.
    /// </summary>
    public override async Task<ApiScheduling.ValidateRosterResponse> ValidateRoster(ApiScheduling.ValidateRosterRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ValidateRoster for {Tenant}", requestContext.TenantId);
        if (request.Roster is { } record)
        {
            var snapshot = RosterMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiScheduling.RecalculateAvailabilityWindowsRequest
        {
            AvailabilityWindow = new ApiScheduling.AvailabilityWindow
            {
                Participants = { new ApiScheduling.AvailabilityWindowParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
                    },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiScheduling.RecalculateAvailabilityWindowsRequest>(json0);
        using var rpc0 = _schedulingAvailabilityWindowCalculationService.RecalculateAvailabilityWindowsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiScheduling.CompareAvailabilityWindowOptionsRequest
        {
            AvailabilityWindow = new ApiScheduling.AvailabilityWindow
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.Roster?.Audit?.UpdatedBy ?? ""),
                },
                Daily = new ApiScheduling.AvailabilityWindowDailyRule
                {
                    Interval = (reply0.AvailabilityWindow ?? new ApiScheduling.AvailabilityWindow()).RecurrenceCase switch
                    {
                        ApiScheduling.AvailabilityWindow.RecurrenceOneofCase.Weekly => (reply0.AvailabilityWindow?.Weekly?.Interval ?? 0),
                        _ => 0
                    },
                },
                Participants = { new ApiScheduling.AvailabilityWindowParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Details?.Contact?.Email ?? ""),
                    },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc1 = _schedulingAvailabilityWindowCalculationService.CompareAvailabilityWindowOptionsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiScheduling.ValidateRosterResponse
        {
            Roster = new ApiScheduling.Roster
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.AvailabilityWindow?.Audit?.CreatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply1.AvailabilityWindow?.Location?.PostalCode ?? ""),
                },
                IssuedAt = (request.Roster?.IssuedAt ?? new Timestamp()),
                Metadata = { (reply0.AvailabilityWindow?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                RosterId = (request.RosterId ?? ""),
                Total = (reply1.Breakdown?.Total ?? new ApiCommon.Money()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Roster?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "roster",
            "audit", "ValidateRoster", (request.Roster?.VoucherCode ?? ""), token);
        return response;
    }
    private async Task<ApiScheduling.ExplainRosterCalculationResponse> SendPreviewRoster1Async(ApiScheduling.ExplainRosterCalculationRequest request, CancellationToken token)
    {
        return await _rosterValidationEndpointSendPreviewRoster1Port.SendAsync(request, token);
    }
}
