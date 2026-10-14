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
using ApiConfiguration = Acme.Configuration.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Timekeeping;

public sealed partial class TimesheetApprovalEndpoint
{
    /// <summary>
    /// Approves a timesheet.
    /// </summary>
    public override async Task<ApiTimekeeping.ApproveTimesheetResponse> ApproveTimesheet(ApiTimekeeping.ApproveTimesheetRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ApproveTimesheet for {Tenant}", requestContext.TenantId);
        if (request.Timesheet is { } record)
        {
            var snapshot = TimesheetMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.SearchEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.SearchEmployeesRequest.Types.Filter
            {
                CreatedAfter = (request.Details?.EffectiveAt ?? new Timestamp()),
                Labels = { (request.Timesheet?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Region = (request.Timesheet?.BillingAddress?.Region ?? ""),
            },
        };
        var descriptor0 = new Method<ApiEmployees.SearchEmployeesRequest, ApiEmployees.SearchEmployeesResponse>(
            MethodType.Unary, "acme.employees.v1" + ".EmployeeService", "SearchEmployees",
            Wire.Marshaller<ApiEmployees.SearchEmployeesRequest>(), Wire.Marshaller<ApiEmployees.SearchEmployeesResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTimekeeping.LookupTimesheetRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            TimesheetId = (request.Timesheet?.TimesheetId ?? ""),
        };
        var command1 = new DispatchCommand<ApiTimekeeping.LookupTimesheetRequest>(input1,
            "primary", requestContext.TenantId);
        var routed1 = command1 with { Region = _settings.Value.Region };
        using var rpc1 = _timekeepingTimesheetQueryService.LookupTimesheetAsync(routed1.Payload, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTimekeeping.ApproveTimesheetResponse
        {
            Outcome = new ApiTimekeeping.ApproveTimesheetResponse.Types.Outcome
            {
                Status = (reply1.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
            Timesheet = new ApiTimekeeping.Timesheet
            {
                Counterparty = (reply1.Timesheet?.Counterparty ?? new ApiEmployees.EmployeeRef()),
                DueAt = (request.Timesheet?.DueAt ?? new Timestamp()),
                TimesheetId = (request.TimesheetId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Timesheet?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "timesheet",
            "audit", "ApproveTimesheet", (request.Timesheet?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Comments a timesheet.
    /// </summary>
    public override async Task<ApiTimekeeping.CommentTimesheetApprovalResponse> CommentTimesheetApproval(ApiTimekeeping.CommentTimesheetApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CommentTimesheetApproval for {Tenant}", requestContext.TenantId);
        if (request.Timesheet is { } record)
        {
            var snapshot = TimesheetMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var profile0 = new MappingProfile<ApiTimekeeping.CommentTimesheetApprovalRequest, ApiTimekeeping.CountTimesheetsRequest>(value => new ApiTimekeeping.CountTimesheetsRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiTimekeeping.CountTimesheetsRequest.Types.Filter
            {
                OwnerId = (value.Details?.Approver?.UserId ?? ""),
                Region = (value.Timesheet?.BillingAddress?.Region ?? ""),
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _timekeepingTimesheetQueryService.CountTimesheetsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiConfiguration.SummarizeConfigSetsRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        input1 = PropertyMap.Copy(input1);
        using var rpc1 = _configurationConfigSetQueryService.SummarizeConfigSetsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTimekeeping.CommentTimesheetApprovalResponse
        {
            Outcome = new ApiTimekeeping.CommentTimesheetApprovalResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
            Timesheet = new ApiTimekeeping.Timesheet
            {
                TimesheetId = (request.TimesheetId ?? ""),
                Total = (request.Timesheet?.Total ?? new ApiCommon.Money()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Timesheet?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "timesheet",
            "audit", "CommentTimesheetApproval", (request.Timesheet?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Escalates a timesheet.
    /// </summary>
    public override async Task<ApiTimekeeping.EscalateTimesheetResponse> EscalateTimesheet(ApiTimekeeping.EscalateTimesheetRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EscalateTimesheet for {Tenant}", requestContext.TenantId);
        if (request.Timesheet is { } record)
        {
            var snapshot = TimesheetMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiScheduling.ListPendingShiftsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiScheduling.ListPendingShiftsRequest.Types.Filter
            {
                Region = (request.Timesheet?.BillingAddress?.Region ?? ""),
            },
        };
        using var rpc0 = _schedulingShiftApprovalService.ListPendingShiftsAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiAuthorization.LintAccessPoliciesRequest
        {
            AccessPolicy = new ApiAuthorization.AccessPolicy
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc1 = _authorizationAccessPolicyValidationService.LintAccessPoliciesAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var response = new ApiTimekeeping.EscalateTimesheetResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Timesheet = new ApiTimekeeping.Timesheet
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (reply1.AccessPolicy?.Owner?.Email ?? ""),
                },
                Notes = (request.Timesheet?.Notes ?? ""),
                TimesheetId = (request.TimesheetId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Timesheet?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "timesheet",
            "audit", "EscalateTimesheet", (request.Timesheet?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a timesheet.
    /// </summary>
    public override async Task<ApiTimekeeping.GetTimesheetApprovalStatusResponse> GetTimesheetApprovalStatus(ApiTimekeeping.GetTimesheetApprovalStatusRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.TimesheetId, "timesheet_id");
        _logger.LogInformation("Handling GetTimesheetApprovalStatus for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "timesheet", request.TimesheetId,
            "GetTimesheetApprovalStatus", "accepted", token);
        var input0 = new ApiEmployees.CreateEmployeeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Employee = new ApiEmployees.Employee
            {
                RelationshipManager = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _employeesEmployeeService.CreateEmployeeAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTimekeeping.GetTimesheetApprovalStatusResponse
        {
            Timesheet = new ApiTimekeeping.Timesheet
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (reply0.Employee?.PrimaryAddress?.Recipient ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (reply0.Employee?.RelationshipManager?.DisplayName ?? ""),
                },
                TimesheetId = (request.TimesheetId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists timesheets a page at a time.
    /// </summary>
    public override async Task<ApiTimekeeping.ListPendingTimesheetsResponse> ListPendingTimesheets(ApiTimekeeping.ListPendingTimesheetsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListPendingTimesheets for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "timesheet", "batch",
            "ListPendingTimesheets", "accepted", token);
        var input0 = new ApiEmployees.CreateEmployeeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Employee = new ApiEmployees.Employee
            {
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiEmployees.CreateEmployeeRequest>();
        using var rpc0 = _employeesEmployeeService.CreateEmployeeAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTimekeeping.ListPendingTimesheetsResponse
        {
            Timesheets = { new ApiTimekeeping.Timesheet
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.Employee?.Audit?.Etag ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (reply0.Employee?.RelationshipManager?.Email ?? ""),
                },
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "timesheet",
            "audit", "ListPendingTimesheets", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalls a timesheet.
    /// </summary>
    public override async Task<ApiTimekeeping.RecallTimesheetResponse> RecallTimesheet(ApiTimekeeping.RecallTimesheetRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecallTimesheet for {Tenant}", requestContext.TenantId);
        if (request.Timesheet is { } record)
        {
            var snapshot = TimesheetMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTimekeeping.CalculateClockEventRequest
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiTimekeeping.TimesheetRef
                {
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                    Number = (request.Timesheet?.Number ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTimekeeping.CalculateClockEventRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Locality = (request.Timesheet?.BillingAddress?.Locality ?? ""),
                    Recipient = (request.Timesheet?.BillingAddress?.Recipient ?? ""),
                },
            },
        };
        using var rpc0 = _timekeepingClockEventCalculationService.CalculateClockEventAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiWorkplaces.CountRoomBookingTotalsRequest();
        input1.Context = (request.Context ?? new ApiCommon.RequestContext());
        input1.Filter = new ApiWorkplaces.CountRoomBookingTotalsRequest.Types.Filter
        {
            CreatedAfter = (reply0.ClockEvent?.OccurredAt ?? new Timestamp()),
            Labels = { (reply0.ClockEvent?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            OwnerId = (request.Details?.Approver?.UserId ?? ""),
            Region = (request.Timesheet?.BillingAddress?.Region ?? ""),
        };
        using var rpc1 = _workplacesRoomBookingReportingService.CountRoomBookingTotalsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTimekeeping.RecallTimesheetResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Timesheet = new ApiTimekeeping.Timesheet
            {
                Notes = (request.Timesheet?.Notes ?? ""),
                Number = (reply0.ClockEvent?.Subject?.Number ?? ""),
                TimesheetId = (request.TimesheetId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Timesheet?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "timesheet",
            "audit", "RecallTimesheet", (request.Timesheet?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Rejects a timesheet.
    /// </summary>
    public override async Task<ApiTimekeeping.RejectTimesheetResponse> RejectTimesheet(ApiTimekeeping.RejectTimesheetRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RejectTimesheet for {Tenant}", requestContext.TenantId);
        if (request.Timesheet is { } record)
        {
            var snapshot = TimesheetMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.ExportOrgUnitsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.ExportOrgUnitsRequest.Types.Filter
            {
                CreatedAfter = (request.Timesheet?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (request.Timesheet?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Timesheet?.Counterparty?.EmployeeId ?? ""),
                Region = (request.Timesheet?.BillingAddress?.Region ?? ""),
            },
        };
        using var stream0 = _employeesOrgUnitQueryService.ExportOrgUnits(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportOrgUnits frame {Type}", change0.GetType().Name);
        }
        var response = new ApiTimekeeping.RejectTimesheetResponse
        {
            Timesheet = new ApiTimekeeping.Timesheet
            {
                Notes = (request.Timesheet?.Notes ?? ""),
                TimesheetId = (request.TimesheetId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Timesheet?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "timesheet",
            "audit", "RejectTimesheet", (request.Timesheet?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Submits a timesheet.
    /// </summary>
    public override async Task<ApiTimekeeping.SubmitTimesheetResponse> SubmitTimesheet(ApiTimekeeping.SubmitTimesheetRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SubmitTimesheet for {Tenant}", requestContext.TenantId);
        if (request.Timesheet is { } record)
        {
            var snapshot = TimesheetMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.ListEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.ListEmployeesRequest.Types.Filter
            {
                Region = (request.Timesheet?.BillingAddress?.Region ?? ""),
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _employeesEmployeeService.ListEmployeesAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiTimekeeping.SubmitTimesheetResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
            Timesheet = new ApiTimekeeping.Timesheet
            {
                DueAt = (request.Timesheet?.DueAt ?? new Timestamp()),
                TimesheetId = (request.TimesheetId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Timesheet?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "timesheet",
            "audit", "SubmitTimesheet", (request.Timesheet?.VoucherCode ?? ""), token);
        return response;
    }
}
