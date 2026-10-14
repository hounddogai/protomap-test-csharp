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
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLearning = Acme.Learning.V1;
using ApiMaintenance = Acme.Maintenance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSearch = Acme.Search.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Timekeeping;

public sealed partial class TimesheetQueryEndpoint
{
    /// <summary>
    /// Lists timesheets a page at a time.
    /// </summary>
    public override async Task<ApiTimekeeping.BrowseTimesheetsResponse> BrowseTimesheets(ApiTimekeeping.BrowseTimesheetsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling BrowseTimesheets for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "timesheet", "batch",
            "BrowseTimesheets", "accepted", token);
        var response = new ApiTimekeeping.BrowseTimesheetsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "timesheet",
            "audit", "BrowseTimesheets", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Counts timesheets by status.
    /// </summary>
    public override async Task<ApiTimekeeping.CountTimesheetsResponse> CountTimesheets(ApiTimekeeping.CountTimesheetsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CountTimesheets for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "timesheet", "batch",
            "CountTimesheets", "accepted", token);
        var input0 = new ApiTimekeeping.RecalculateOvertimesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTimekeeping.RecalculateOvertimesRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _timekeepingOvertimeCalculationService.RecalculateOvertimesAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiTimekeeping.CountTimesheetsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "timesheet",
            "audit", "CountTimesheets", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams every matching timesheet.
    /// </summary>
    public override async Task ExportTimesheets(ApiTimekeeping.ExportTimesheetsRequest request, IServerStreamWriter<ApiTimekeeping.ExportTimesheetsResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExportTimesheets for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "timesheet", "batch",
            "ExportTimesheets", "accepted", token);
        var input0 = new ApiTimekeeping.CalculateOvertimeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTimekeeping.CalculateOvertimeRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
            Overtime = new ApiTimekeeping.Overtime
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _timekeepingOvertimeCalculationService.CalculateOvertimeAsync);
        var input1 = new ApiMaintenance.ResolveWorkOrdersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc1 = _sendExportTimesheets1(input1, new CallOptions(cancellationToken: token));
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiSearch.SummarizeSearchIndexesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiSearch.SummarizeSearchIndexesRequest.Types.Details
            {
                Filters = { (reply0.Overtime?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Format = (request.Format ?? ""),
                Period = (reply0.Overtime?.Effective ?? new ApiCommon.TimeWindow()),
            },
            Reason = (reply0.Overtime ?? new ApiTimekeeping.Overtime()).ActionCase switch
            {
                ApiTimekeeping.Overtime.ActionOneofCase.Block => (reply0.Overtime?.Block?.Reason ?? ""),
                _ => ""
            },
        };
        using var rpc2 = _searchSearchIndexQueryService.SummarizeSearchIndexesAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiTimekeeping.ExportTimesheetsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
            Timesheet = new ApiTimekeeping.Timesheet
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
                Total = (reply0.Breakdown?.Total ?? new ApiCommon.Money()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Filter?.Query ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "timesheet",
            "audit", "ExportTimesheets", (request.Filter?.Query ?? ""), token);
        await responses.WriteAsync(response, token);
    }
    /// <summary>
    /// Lists timesheets a page at a time.
    /// </summary>
    public override async Task<ApiTimekeeping.FindTimesheetsResponse> FindTimesheets(ApiTimekeeping.FindTimesheetsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling FindTimesheets for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "timesheet", "batch",
            "FindTimesheets", "accepted", token);
        var input0 = new ApiTimekeeping.ExplainOvertimeDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Overtime = new ApiTimekeeping.Overtime
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        var descriptor0 = new Method<ApiTimekeeping.ExplainOvertimeDecisionRequest, ApiTimekeeping.ExplainOvertimeDecisionResponse>(
            MethodType.Unary, "acme.timekeeping.v1" + ".OvertimeValidationService", "ExplainOvertimeDecision",
            Wire.Marshaller<ApiTimekeeping.ExplainOvertimeDecisionRequest>(), Wire.Marshaller<ApiTimekeeping.ExplainOvertimeDecisionResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTimekeeping.FindTimesheetsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            Timesheets = { new ApiTimekeeping.Timesheet
            {
                Lines = { new ApiTimekeeping.TimesheetLine
                {
                    Tax = new ApiTimekeeping.TimesheetLineTax
                    {
                        Amount = (reply0.Overtime?.Discount?.Amount ?? new ApiCommon.Money()),
                    },
                } },
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "timesheet",
            "audit", "FindTimesheets", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a timesheet.
    /// </summary>
    public override async Task<ApiTimekeeping.LookupTimesheetResponse> LookupTimesheet(ApiTimekeeping.LookupTimesheetRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.TimesheetId, "timesheet_id");
        _logger.LogInformation("Handling LookupTimesheet for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "timesheet", request.TimesheetId,
            "LookupTimesheet", "accepted", token);
        var input0 = new ApiAuthorization.ExplainAccessPolicyDecisionRequest
        {
            AccessPolicy = new ApiAuthorization.AccessPolicy
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var command0 = new DispatchCommand<ApiAuthorization.ExplainAccessPolicyDecisionRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _authorizationAccessPolicyValidationService.ExplainAccessPolicyDecisionAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTimekeeping.LookupTimesheetResponse
        {
            Timesheet = new ApiTimekeeping.Timesheet
            {
                Lines = { new ApiTimekeeping.TimesheetLine
                {
                    Amount = (reply0.AccessPolicy?.Discount?.Amount ?? new ApiCommon.Money()),
                } },
                TimesheetId = (request.TimesheetId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists timesheets a page at a time.
    /// </summary>
    public override async Task<ApiTimekeeping.SuggestTimesheetsResponse> SuggestTimesheets(ApiTimekeeping.SuggestTimesheetsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SuggestTimesheets for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "timesheet", "batch",
            "SuggestTimesheets", "accepted", token);
        var profile0 = new MappingProfile<ApiTimekeeping.SuggestTimesheetsRequest, ApiItsm.ValidateChangeRequestRequest>(value => new ApiItsm.ValidateChangeRequestRequest
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Assignee = new ApiIdentity.UserRef
                {
                    UserId = (value.Context?.Actor?.UserId ?? ""),
                },
                Comments = { new ApiItsm.ChangeRequestComment
                {
                    CreatedAt = (value.Filter?.CreatedAfter ?? new Timestamp()),
                } },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (value.Context?.Actor?.Email ?? ""),
                },
            },
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.ValidateChangeRequestRequest.Types.Details
            {
                Locale = (value.Context?.Locale ?? ""),
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _itsmChangeRequestValidationService.ValidateChangeRequestAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTimekeeping.ValidateTimesheetRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Timesheet = new ApiTimekeeping.Timesheet
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.ChangeRequest?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedAt = (reply0.ChangeRequest?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        input1 = PropertyMap.Copy(input1);
        using var rpc1 = _timekeepingTimesheetValidationService.ValidateTimesheetAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTimekeeping.SuggestTimesheetsResponse
        {
            Timesheets = { new ApiTimekeeping.Timesheet
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (reply1.Timesheet?.BillingAddress?.Region ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (reply0.ChangeRequest?.Assignee?.DisplayName ?? ""),
                },
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "timesheet",
            "audit", "SuggestTimesheets", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Summarizes a timesheet.
    /// </summary>
    public override async Task<ApiTimekeeping.SummarizeTimesheetsResponse> SummarizeTimesheets(ApiTimekeeping.SummarizeTimesheetsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SummarizeTimesheets for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "timesheet", request.TimesheetId,
            "SummarizeTimesheets", "accepted", token);
        var input0 = new ApiTimekeeping.ScoreClockEventRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Measurements = { new ApiTimekeeping.ClockEventMeasurement
                {
                    ObservedAt = (request.Context?.Deadline ?? new Timestamp()),
                } },
                Subject = new ApiTimekeeping.TimesheetRef
                {
                    TimesheetId = (request.TimesheetId ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Reason ?? ""),
        };
        using var rpc0 = _timekeepingClockEventValidationService.ScoreClockEventAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiTimekeeping.ScoreClockEventRequest>("acme.timekeeping.v1.TimesheetQueryService/SummarizeTimesheets/0", input0.Clone()), token);
        var input1 = new ApiLearning.PreviewCourseEnrollmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CourseEnrollment = new ApiLearning.CourseEnrollment
            {
                CurrencyCode = (reply0.Breakdown?.CurrencyCode ?? ""),
                Holder = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (reply0.ClockEvent?.Subject?.DisplayName ?? ""),
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            Reason = (request.Reason ?? ""),
        };
        var reply1 = await SendSummarizeTimesheets1Async(input1, token);
        var input2 = new ApiTimekeeping.QuoteOvertimeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Overtime = new ApiTimekeeping.Overtime
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.CourseEnrollment?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedAt = (reply1.CourseEnrollment?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply1.CourseEnrollment?.Holder?.Email ?? ""),
                },
            },
        };
        var labels2 = input2.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input2.Context.Baggage.Clear();
        input2.Context.Baggage.Add(labels2);
        using var rpc2 = _timekeepingOvertimeCalculationService.QuoteOvertimeAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiTimekeeping.SummarizeTimesheetsResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.ClockEvent?.CorrelationId ?? ""),
                Details = { (reply2.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply2.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.Format ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "timesheet",
            "audit", "SummarizeTimesheets", (request.Details?.Format ?? ""), token);
        return response;
    }
    private async Task<ApiLearning.PreviewCourseEnrollmentResponse> SendSummarizeTimesheets1Async(ApiLearning.PreviewCourseEnrollmentRequest request, CancellationToken token)
    {
        return await _timesheetQueryEndpointSendSummarizeTimesheets1Port.SendAsync(request, token);
    }
}
