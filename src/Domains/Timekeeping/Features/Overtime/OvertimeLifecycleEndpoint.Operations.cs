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
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiSearch = Acme.Search.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Timekeeping;

public sealed partial class OvertimeLifecycleEndpoint
{
    /// <summary>
    /// Activates an overtime.
    /// </summary>
    public override async Task<ApiTimekeeping.ActivateOvertimeResponse> ActivateOvertime(ApiTimekeeping.ActivateOvertimeRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.OvertimeId, "overtime_id");
        _logger.LogInformation("Handling ActivateOvertime for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "overtime", request.OvertimeId,
            "ActivateOvertime", "accepted", token);
        var input0 = new ApiTimekeeping.LintOvertimesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTimekeeping.LintOvertimesRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                    Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                    PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                },
            },
            Overtime = new ApiTimekeeping.Overtime
            {
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.Details?.NewOwner?.UserId ?? ""),
                },
            },
        };
        using var rpc0 = _sendActivateOvertime0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTimekeeping.FindTimesheetsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiTimekeeping.FindTimesheetsRequest.Types.Filter
            {
                CreatedAfter = (reply0.Outcome?.EffectiveAt ?? new Timestamp()),
                Labels = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply0.Overtime?.Owner?.UserId ?? ""),
            },
        };
        var named1 = _registry.Get<ApiTimekeeping.TimesheetQueryService.TimesheetQueryServiceClient>("acme.timekeeping.v1.TimesheetQueryService");
        using var rpc1 = named1.FindTimesheetsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiScheduling.ListPendingRostersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiScheduling.ListPendingRostersRequest.Types.Filter
            {
                CreatedAfter = (reply0.Overtime?.Audit?.CreatedAt ?? new Timestamp()),
            },
        };
        var descriptor2 = new Method<ApiScheduling.ListPendingRostersRequest, ApiScheduling.ListPendingRostersResponse>(
            MethodType.Unary, "acme.scheduling.v1" + ".RosterApprovalService", "ListPendingRosters",
            Wire.Marshaller<ApiScheduling.ListPendingRostersRequest>(), Wire.Marshaller<ApiScheduling.ListPendingRostersResponse>());
        using var rpc2 = _invoker.AsyncUnaryCall(descriptor2, null,
            new CallOptions(cancellationToken: token), input2);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiScheduling.AdvanceShiftWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Shift = new ApiScheduling.Shift
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.Overtime?.Audit?.UpdatedBy ?? ""),
                },
                Organizer = new ApiIdentity.UserRef
                {
                    UserId = (request.Details?.NewOwner?.UserId ?? ""),
                },
                Participants = { new ApiScheduling.ShiftParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Details?.NotifyContact?.Email ?? ""),
                        PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                    },
                } },
            },
        };
        var command3 = new DispatchCommand<ApiScheduling.AdvanceShiftWorkflowRequest>(input3,
            "primary", requestContext.TenantId);
        var routed3 = command3 with { Region = _settings.Value.Region };
        using var rpc3 = _schedulingShiftWorkflowService.AdvanceShiftWorkflowAsync(routed3.Payload, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiTimekeeping.ActivateOvertimeResponse
        {
            Outcome = new ApiTimekeeping.ActivateOvertimeResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Message = (reply2.Result?.Message ?? ""),
            },
            Overtime = new ApiTimekeeping.Overtime
            {
                OvertimeId = (request.OvertimeId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply0.Overtime?.Owner?.Email ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply2.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply3.Result?.Message ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "overtime",
            "audit", "ActivateOvertime", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Expires an overtime.
    /// </summary>
    public override async Task<ApiTimekeeping.ExpireOvertimeResponse> ExpireOvertime(ApiTimekeeping.ExpireOvertimeRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.OvertimeId, "overtime_id");
        _logger.LogInformation("Handling ExpireOvertime for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "overtime", request.OvertimeId,
            "ExpireOvertime", "accepted", token);
        var profile0 = new MappingProfile<ApiTimekeeping.ExpireOvertimeRequest, ApiEmployees.QuoteEmploymentContractRequest>(value => new ApiEmployees.QuoteEmploymentContractRequest
        {
            Annotations = { (value.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (value.Details?.NewOwner?.DisplayName ?? ""),
                    Email = (value.Details?.NewOwner?.Email ?? ""),
                },
            },
            Reason = (value.Reason ?? ""),
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _employeesEmploymentContractCalculationService.QuoteEmploymentContractAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTimekeeping.CheckClockEventEligibilityRequest
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiTimekeeping.TimesheetRef
                {
                    DisplayName = (reply0.EmploymentContract?.Counterparty?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTimekeeping.CheckClockEventEligibilityRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                    PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc1 = _timekeepingClockEventValidationService.CheckClockEventEligibilityAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTimekeeping.ExpireOvertimeResponse
        {
            Outcome = new ApiTimekeeping.ExpireOvertimeResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                ReferenceId = (reply1.Outcome?.ReferenceId ?? ""),
            },
            Overtime = new ApiTimekeeping.Overtime
            {
                OvertimeId = (request.OvertimeId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.ReasonCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "overtime",
            "audit", "ExpireOvertime", (request.Details?.ReasonCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an overtime.
    /// </summary>
    public override async Task<ApiTimekeeping.GetOvertimeLifecycleResponse> GetOvertimeLifecycle(ApiTimekeeping.GetOvertimeLifecycleRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.OvertimeId, "overtime_id");
        _logger.LogInformation("Handling GetOvertimeLifecycle for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "overtime", request.OvertimeId,
            "GetOvertimeLifecycle", "accepted", token);
        var response = new ApiTimekeeping.GetOvertimeLifecycleResponse
        {
            Overtime = new ApiTimekeeping.Overtime
            {
                OvertimeId = (request.OvertimeId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Reinstates an overtime.
    /// </summary>
    public override async Task<ApiTimekeeping.ReinstateOvertimeResponse> ReinstateOvertime(ApiTimekeeping.ReinstateOvertimeRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.OvertimeId, "overtime_id");
        _logger.LogInformation("Handling ReinstateOvertime for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "overtime", request.OvertimeId,
            "ReinstateOvertime", "accepted", token);
        var input0 = request.ReinstateOvertimeToQuoteEmploymentContract0();
        using var rpc0 = _employeesEmploymentContractCalculationService.QuoteEmploymentContractAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.UpdateEmployeeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Employee = new ApiEmployees.Employee
            {
                RelationshipManager = new ApiIdentity.UserRef
                {
                    Email = (reply0.EmploymentContract?.Counterparty?.Email ?? ""),
                },
            },
        };
        var reply1 = await SendReinstateOvertime1Async(input1, token);
        var input2 = new ApiDocuments.LintDocumentFoldersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                Children = { new ApiDocuments.DocumentFolder
                {
                    Children = { new ApiDocuments.DocumentFolder
                    {
                        Location = new ApiCommon.PostalAddress
                        {
                            PostalCode = (reply0.EmploymentContract?.BillingAddress?.PostalCode ?? ""),
                        },
                        Manager = new ApiIdentity.UserRef
                        {
                            DisplayName = (reply1.Employee?.RelationshipManager?.DisplayName ?? ""),
                        },
                    } },
                } },
            },
        };
        var labels2 = input2.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input2.Context.Baggage.Clear();
        input2.Context.Baggage.Add(labels2);
        using var rpc2 = _documentsDocumentFolderValidationService.LintDocumentFoldersAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiTimekeeping.ReinstateOvertimeResponse
        {
            Outcome = new ApiTimekeeping.ReinstateOvertimeResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Status = (reply1.Result?.Status ?? ""),
            },
            Overtime = new ApiTimekeeping.Overtime
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.EmploymentContract?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (reply1.Employee?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                OvertimeId = (request.OvertimeId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply2.DocumentFolder?.Manager?.Email ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "overtime",
            "audit", "ReinstateOvertime", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Renews an overtime.
    /// </summary>
    public override async Task<ApiTimekeeping.RenewOvertimeResponse> RenewOvertime(ApiTimekeeping.RenewOvertimeRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.OvertimeId, "overtime_id");
        _logger.LogInformation("Handling RenewOvertime for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "overtime", request.OvertimeId,
            "RenewOvertime", "accepted", token);
        var input0 = new ApiTimekeeping.CalculateClockEventRequest
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiTimekeeping.TimesheetRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _timekeepingClockEventCalculationService.CalculateClockEventAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiSearch.CountSearchIndexesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiSearch.CountSearchIndexesRequest.Types.Filter
            {
                CreatedAfter = (request.Details?.EffectiveAt ?? new Timestamp()),
                Labels = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply0.ClockEvent?.ClockEventId ?? ""),
            },
        };
        var json1 = JsonFormatter.Default.Format(input1);
        input1 = JsonParser.Default.Parse<ApiSearch.CountSearchIndexesRequest>(json1);
        using var rpc1 = _searchSearchIndexQueryService.CountSearchIndexesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiScheduling.RetryShiftWorkflowRequest();
        input2.Context = (request.Context ?? new ApiCommon.RequestContext());
        input2.Details = new ApiScheduling.RetryShiftWorkflowRequest.Types.Details
        {
            Notify = new ApiCommon.ContactInfo
            {
                Email = (request.Details?.NotifyContact?.Email ?? ""),
            },
        };
        input2.Shift = new ApiScheduling.Shift
        {
            Organizer = new ApiIdentity.UserRef
            {
                UserId = (request.Details?.NewOwner?.UserId ?? ""),
            },
            Participants = { new ApiScheduling.ShiftParticipant
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                },
            } },
        };
        using var rpc2 = _schedulingShiftWorkflowService.RetryShiftWorkflowAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiTimekeeping.ExplainOvertimeCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Overtime = new ApiTimekeeping.Overtime
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply2.Shift?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply2.Shift?.Organizer?.DisplayName ?? ""),
                },
            },
        };
        using var rpc3 = _timekeepingOvertimeCalculationService.ExplainOvertimeCalculationAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiTimekeeping.RenewOvertimeResponse
        {
            Outcome = new ApiTimekeeping.RenewOvertimeResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
            Overtime = new ApiTimekeeping.Overtime
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply3.Overtime?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                OvertimeId = (request.OvertimeId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.ClockEvent?.CorrelationId ?? ""),
                ReferenceIds = { (reply2.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "overtime",
            "audit", "RenewOvertime", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Schedules an overtime.
    /// </summary>
    public override async Task<ApiTimekeeping.ScheduleOvertimeExpiryResponse> ScheduleOvertimeExpiry(ApiTimekeeping.ScheduleOvertimeExpiryRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScheduleOvertimeExpiry for {Tenant}", requestContext.TenantId);
        if (request.Overtime is { } record)
        {
            var snapshot = OvertimeMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTimekeeping.QuoteClockEventRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiTimekeeping.TimesheetRef
                {
                    DisplayName = (request.Overtime?.Owner?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Reason ?? ""),
        };
        using var rpc0 = _timekeepingClockEventCalculationService.QuoteClockEventAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.CompareEmploymentContractOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Lines = { new ApiEmployees.EmploymentContractLine
                {
                    Tax = new ApiEmployees.EmploymentContractLineTax
                    {
                        Amount = (request.Overtime ?? new ApiTimekeeping.Overtime()).ActionCase switch
                        {
                            ApiTimekeeping.Overtime.ActionOneofCase.Discount => (request.Overtime?.Discount?.Amount ?? new ApiCommon.Money()),
                            _ => new ApiCommon.Money()
                        },
                    },
                } },
            },
        };
        using var rpc1 = _employeesEmploymentContractCalculationService.CompareEmploymentContractOptionsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiEmployees.PreviewPositionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Position = new ApiEmployees.Position
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.EmploymentContract?.Audit?.Etag ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                },
            },
        };
        var reply2 = await Workflow.SendAsync(input2, token, _employeesPositionValidationService.PreviewPositionAsync);
        var input3 = new ApiTimekeeping.BrowseTimesheetsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiTimekeeping.BrowseTimesheetsRequest.Types.Filter
            {
                CreatedAfter = (reply2.Position?.Audit?.CreatedAt ?? new Timestamp()),
                OwnerId = (reply2.Outcome?.ReferenceId ?? ""),
                Region = (reply1.EmploymentContract?.BillingAddress?.Region ?? ""),
            },
        };
        using var rpc3 = _sendScheduleOvertimeExpiry3(input3, new CallOptions(cancellationToken: token));
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiTimekeeping.ScheduleOvertimeExpiryResponse
        {
            Outcome = new ApiTimekeeping.ScheduleOvertimeExpiryResponse.Types.Outcome
            {
                Status = (reply2.Result?.Status ?? ""),
            },
            Overtime = new ApiTimekeeping.Overtime
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply1.EmploymentContract?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (reply1.EmploymentContract?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                OvertimeId = (request.OvertimeId ?? ""),
                Parameters = { (request.Overtime?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply3.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Overtime?.Route?.Target ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "overtime",
            "audit", "ScheduleOvertimeExpiry", (request.Overtime?.Route?.Target ?? ""), token);
        return response;
    }
    /// <summary>
    /// Suspends an overtime.
    /// </summary>
    public override async Task<ApiTimekeeping.SuspendOvertimeResponse> SuspendOvertime(ApiTimekeeping.SuspendOvertimeRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.OvertimeId, "overtime_id");
        _logger.LogInformation("Handling SuspendOvertime for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "overtime", request.OvertimeId,
            "SuspendOvertime", "accepted", token);
        var input0 = new ApiWorkplaces.CancelRoomBookingWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiWorkplaces.CancelRoomBookingWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                },
            },
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                Organizer = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                    UserId = (request.Details?.NewOwner?.UserId ?? ""),
                },
                Participants = { new ApiWorkplaces.RoomBookingParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                    },
                } },
            },
        };
        var named0 = _registry.Get<ApiWorkplaces.RoomBookingWorkflowService.RoomBookingWorkflowServiceClient>("acme.workplaces.v1.RoomBookingWorkflowService");
        using var rpc0 = named0.CancelRoomBookingWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.NormalizeEmploymentContractRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.NormalizeEmploymentContractRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                },
            },
        };
        var descriptor1 = new Method<ApiEmployees.NormalizeEmploymentContractRequest, ApiEmployees.NormalizeEmploymentContractResponse>(
            MethodType.Unary, "acme.employees.v1" + ".EmploymentContractValidationService", "NormalizeEmploymentContract",
            Wire.Marshaller<ApiEmployees.NormalizeEmploymentContractRequest>(), Wire.Marshaller<ApiEmployees.NormalizeEmploymentContractResponse>());
        using var rpc1 = _invoker.AsyncUnaryCall(descriptor1, null,
            new CallOptions(cancellationToken: token), input1);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTimekeeping.SuspendOvertimeResponse
        {
            Outcome = new ApiTimekeeping.SuspendOvertimeResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Overtime = new ApiTimekeeping.Overtime
            {
                OvertimeId = (request.OvertimeId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "overtime",
            "audit", "SuspendOvertime", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Terminates an overtime.
    /// </summary>
    public override async Task<ApiTimekeeping.TerminateOvertimeResponse> TerminateOvertime(ApiTimekeeping.TerminateOvertimeRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.OvertimeId, "overtime_id");
        _logger.LogInformation("Handling TerminateOvertime for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "overtime", request.OvertimeId,
            "TerminateOvertime", "accepted", token);
        var input0 = new ApiTimekeeping.FindTimesheetsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiTimekeeping.FindTimesheetsRequest.Types.Filter
            {
                CreatedAfter = (request.Details?.EffectiveAt ?? new Timestamp()),
                OwnerId = (request.Details?.NewOwner?.UserId ?? ""),
            },
        };
        using var rpc0 = _timekeepingTimesheetQueryService.FindTimesheetsAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiTimekeeping.ExplainClockEventDecisionRequest
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiTimekeeping.TimesheetRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTimekeeping.ExplainClockEventDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                    Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                },
            },
        };
        using var rpc1 = _timekeepingClockEventValidationService.ExplainClockEventDecisionAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var input2 = new ApiEmployees.CreateEmployeeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Employee = new ApiEmployees.Employee
            {
                RelationshipManager = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.ClockEvent?.Subject?.DisplayName ?? ""),
                },
            },
        };
        input2 = PropertyMap.Copy(input2);
        using var rpc2 = _employeesEmployeeService.CreateEmployeeAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiTimekeeping.TerminateOvertimeResponse
        {
            Outcome = new ApiTimekeeping.TerminateOvertimeResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
            Overtime = new ApiTimekeeping.Overtime
            {
                OvertimeId = (request.OvertimeId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "overtime",
            "audit", "TerminateOvertime", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiEmployees.UpdateEmployeeResponse> SendReinstateOvertime1Async(ApiEmployees.UpdateEmployeeRequest request, CancellationToken token)
    {
        return await _overtimeLifecycleEndpointSendReinstateOvertime1Port.SendAsync(request, token);
    }
}
