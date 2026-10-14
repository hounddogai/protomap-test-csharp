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
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDevices = Acme.Devices.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Timekeeping;

public sealed partial class ClockEventQueryEndpoint
{
    /// <summary>
    /// Lists clock events a page at a time.
    /// </summary>
    public override async Task<ApiTimekeeping.BrowseClockEventsResponse> BrowseClockEvents(ApiTimekeeping.BrowseClockEventsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling BrowseClockEvents for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "clock_event", "batch",
            "BrowseClockEvents", "accepted", token);
        var response = new ApiTimekeeping.BrowseClockEventsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "clock_event",
            "audit", "BrowseClockEvents", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Counts clock events by status.
    /// </summary>
    public override async Task<ApiTimekeeping.CountClockEventsResponse> CountClockEvents(ApiTimekeeping.CountClockEventsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CountClockEvents for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "clock_event", "batch",
            "CountClockEvents", "accepted", token);
        var input0 = new ApiCorporateevents.ListCorporateEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiCorporateevents.ListCorporateEventsRequest.Types.Filter
            {
                Region = (request.Filter?.Region ?? ""),
            },
            Page = new ApiCommon.PageRequest
            {
                PageToken = (request.Page?.PageToken ?? ""),
            },
        };
        var command0 = new DispatchCommand<ApiCorporateevents.ListCorporateEventsRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _corporateeventsCorporateEventService.ListCorporateEventsAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var profile1 = new MappingProfile<ApiTimekeeping.CountClockEventsRequest, ApiScheduling.LintShiftsRequest>(value => new ApiScheduling.LintShiftsRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Shift = new ApiScheduling.Shift
            {
                Location = new ApiCommon.PostalAddress
                {
                    Region = (value.Filter?.Region ?? ""),
                },
            },
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _schedulingShiftValidationService.LintShiftsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTimekeeping.CountClockEventsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.OrderBy ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "clock_event",
            "audit", "CountClockEvents", (request.OrderBy ?? ""), token);
        return response;
    }
    /// <summary>
    /// Lists clock events a page at a time.
    /// </summary>
    public override async Task<ApiTimekeeping.FindClockEventsResponse> FindClockEvents(ApiTimekeeping.FindClockEventsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling FindClockEvents for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "clock_event", "batch",
            "FindClockEvents", "accepted", token);
        var response = new ApiTimekeeping.FindClockEventsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "clock_event",
            "audit", "FindClockEvents", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a clock event.
    /// </summary>
    public override async Task<ApiTimekeeping.LookupClockEventResponse> LookupClockEvent(ApiTimekeeping.LookupClockEventRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ClockEventId, "clock_event_id");
        _logger.LogInformation("Handling LookupClockEvent for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "clock_event", request.ClockEventId,
            "LookupClockEvent", "accepted", token);
        var response = new ApiTimekeeping.LookupClockEventResponse
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                ClockEventId = (request.ClockEventId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Returns a clock event.
    /// </summary>
    public override async Task<ApiTimekeeping.PeekClockEventResponse> PeekClockEvent(ApiTimekeeping.PeekClockEventRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ClockEventId, "clock_event_id");
        _logger.LogInformation("Handling PeekClockEvent for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "clock_event", request.ClockEventId,
            "PeekClockEvent", "accepted", token);
        var response = new ApiTimekeeping.PeekClockEventResponse
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                ClockEventId = (request.ClockEventId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Returns several clock events at once.
    /// </summary>
    public override async Task<ApiTimekeeping.ResolveClockEventsResponse> ResolveClockEvents(ApiTimekeeping.ResolveClockEventsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ResolveClockEvents for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "clock_event", "batch",
            "ResolveClockEvents", "accepted", token);
        var input0 = new ApiLicensing.ValidateLicenseAssignmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.ValidateLicenseAssignmentRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Locale = (request.Context?.Locale ?? ""),
            },
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
                Metadata = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _licensingLicenseAssignmentValidationService.ValidateLicenseAssignmentAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDevices.EstimateComplianceScanRequest
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.EstimateComplianceScanRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply0.LicenseAssignment?.BillingAddress?.CountryCode ?? ""),
                    PostalCode = (reply0.LicenseAssignment?.BillingAddress?.PostalCode ?? ""),
                },
            },
        };
        using var rpc1 = _devicesComplianceScanCalculationService.EstimateComplianceScanAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTimekeeping.ResolveClockEventsResponse
        {
            ClockEvents = { new ApiTimekeeping.ClockEvent
            {
                OccurredAt = (reply1.ComplianceScan?.OccurredAt ?? new Timestamp()),
            } },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Outcome?.Message ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists clock events a page at a time.
    /// </summary>
    public override async Task<ApiTimekeeping.SuggestClockEventsResponse> SuggestClockEvents(ApiTimekeeping.SuggestClockEventsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SuggestClockEvents for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "clock_event", "batch",
            "SuggestClockEvents", "accepted", token);
        var input0 = new ApiTimekeeping.ExportTimesheetsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiTimekeeping.ExportTimesheetsRequest.Types.Filter
            {
                CreatedAfter = (request.Filter?.CreatedAfter ?? new Timestamp()),
                OwnerId = (request.Filter?.OwnerId ?? ""),
                Query = (request.Filter?.Query ?? ""),
                Region = (request.Filter?.Region ?? ""),
            },
        };
        using var stream0 = _timekeepingTimesheetQueryService.ExportTimesheets(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportTimesheets frame {Type}", change0.GetType().Name);
        }
        var response = new ApiTimekeeping.SuggestClockEventsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "clock_event",
            "audit", "SuggestClockEvents", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Summarizes a clock event.
    /// </summary>
    public override async Task<ApiTimekeeping.SummarizeClockEventsResponse> SummarizeClockEvents(ApiTimekeeping.SummarizeClockEventsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SummarizeClockEvents for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "clock_event", request.ClockEventId,
            "SummarizeClockEvents", "accepted", token);
        var input0 = new ApiTimekeeping.CalculateOvertimeRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Overtime = new ApiTimekeeping.Overtime
            {
                Block = new ApiTimekeeping.OvertimeBlockAction
                {
                    NotifyEmail = (request.Context?.Actor?.Email ?? ""),
                    Reason = (request.Reason ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _timekeepingOvertimeCalculationService.CalculateOvertimeAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTimekeeping.ExplainOvertimeDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Overtime = new ApiTimekeeping.Overtime
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.Overtime?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Block = new ApiTimekeeping.OvertimeBlockAction
                {
                    Reason = (reply0.Overtime ?? new ApiTimekeeping.Overtime()).ActionCase switch
                    {
                        ApiTimekeeping.Overtime.ActionOneofCase.Block => (reply0.Overtime?.Block?.Reason ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _timekeepingOvertimeValidationService.ExplainOvertimeDecisionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiPerformance.FindReviewsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiPerformance.FindReviewsRequest.Types.Filter
            {
                CreatedAfter = (reply0.Overtime?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (reply0.Overtime?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply1.Result?.CorrelationId ?? ""),
            },
        };
        var json2 = JsonFormatter.Default.Format(input2);
        input2 = JsonParser.Default.Parse<ApiPerformance.FindReviewsRequest>(json2);
        using var rpc2 = _performanceReviewQueryService.FindReviewsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiTimekeeping.CalculateClockEventRequest
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiTimekeeping.TimesheetRef
                {
                    DisplayName = (reply1.Overtime?.Owner?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTimekeeping.CalculateClockEventRequest.Types.Details
            {
                CurrencyCode = (reply0.Breakdown?.CurrencyCode ?? ""),
            },
        };
        using var rpc3 = _timekeepingClockEventCalculationService.CalculateClockEventAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiTimekeeping.SummarizeClockEventsResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply2.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply1.Outcome?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply2.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.Format ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "clock_event",
            "audit", "SummarizeClockEvents", (request.Details?.Format ?? ""), token);
        return response;
    }
}
