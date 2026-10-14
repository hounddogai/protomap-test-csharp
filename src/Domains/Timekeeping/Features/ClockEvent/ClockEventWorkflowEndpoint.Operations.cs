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
using ApiAudit = Acme.Audit.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Timekeeping;

public sealed partial class ClockEventWorkflowEndpoint
{
    /// <summary>
    /// Advances a clock event.
    /// </summary>
    public override async Task<ApiTimekeeping.AdvanceClockEventWorkflowResponse> AdvanceClockEventWorkflow(ApiTimekeeping.AdvanceClockEventWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling AdvanceClockEventWorkflow for {Tenant}", requestContext.TenantId);
        if (request.ClockEvent is { } record)
        {
            var snapshot = ClockEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiAudit.ExplainAuditEventDecisionRequest
        {
            AuditEvent = new ApiAudit.AuditEvent
            {
                CorrelationId = (request.ClockEvent?.CorrelationId ?? ""),
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiAudit.AuditTrailRef
                {
                    Number = (request.ClockEvent?.Subject?.Number ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiAudit.ExplainAuditEventDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _auditAuditEventValidationService.ExplainAuditEventDecisionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTimekeeping.AdvanceClockEventWorkflowResponse
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                ClockEventId = (request.ClockEventId ?? ""),
                Kind = (request.ClockEvent?.Kind ?? ""),
                Subject = new ApiTimekeeping.TimesheetRef
                {
                    Number = (reply0.AuditEvent?.Subject?.Number ?? ""),
                },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "clock_event",
            "audit", "AdvanceClockEventWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Cancels a clock event.
    /// </summary>
    public override async Task<ApiTimekeeping.CancelClockEventWorkflowResponse> CancelClockEventWorkflow(ApiTimekeeping.CancelClockEventWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CancelClockEventWorkflow for {Tenant}", requestContext.TenantId);
        if (request.ClockEvent is { } record)
        {
            var snapshot = ClockEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.ShareDocumentFolderExportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ShareDocumentFolderExportRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Notify?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.Details?.Notify?.Phone ?? ""),
                    PreferredChannel = (request.Details?.Notify?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc0 = _documentsDocumentFolderExportService.ShareDocumentFolderExportAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTimekeeping.CancelClockEventWorkflowResponse
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                ClockEventId = (request.ClockEventId ?? ""),
                CorrelationId = (request.ClockEvent?.CorrelationId ?? ""),
            },
            Outcome = new ApiTimekeeping.CancelClockEventWorkflowResponse.Types.Outcome
            {
                NextSteps = { (reply0.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "clock_event",
            "audit", "CancelClockEventWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a clock event.
    /// </summary>
    public override async Task<ApiTimekeeping.InspectClockEventWorkflowResponse> InspectClockEventWorkflow(ApiTimekeeping.InspectClockEventWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ClockEventId, "clock_event_id");
        _logger.LogInformation("Handling InspectClockEventWorkflow for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "clock_event", request.ClockEventId,
            "InspectClockEventWorkflow", "accepted", token);
        var input0 = new ApiEmployees.EstimateEmploymentContractRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _sendInspectClockEventWorkflow0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTimekeeping.InspectClockEventWorkflowResponse
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                ClockEventId = (request.ClockEventId ?? ""),
                Subject = new ApiTimekeeping.TimesheetRef
                {
                    DisplayName = (reply0.EmploymentContract?.Counterparty?.DisplayName ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        return response;
    }
    /// <summary>
    /// Lists clock events a page at a time.
    /// </summary>
    public override async Task<ApiTimekeeping.ListClockEventWorkflowsResponse> ListClockEventWorkflows(ApiTimekeeping.ListClockEventWorkflowsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListClockEventWorkflows for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "clock_event", "batch",
            "ListClockEventWorkflows", "accepted", token);
        var input0 = new ApiDocuments.NotifySignatureWatchersRequest
        {
            Annotations = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.NotifySignatureWatchersRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Variables = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        using var rpc0 = _documentsSignatureNotificationService.NotifySignatureWatchersAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.ExportOrgUnitsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.ExportOrgUnitsRequest.Types.Filter
            {
                CreatedAfter = (request.Filter?.CreatedAfter ?? new Timestamp()),
                Labels = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Filter?.OwnerId ?? ""),
                Region = (request.Filter?.Region ?? ""),
            },
        };
        using var stream1 = _employeesOrgUnitQueryService.ExportOrgUnits(input1, cancellationToken: token);
        await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportOrgUnits frame {Type}", change1.GetType().Name);
        }
        var response = new ApiTimekeeping.ListClockEventWorkflowsResponse
        {
            ClockEvents = { new ApiTimekeeping.ClockEvent
            {
                Subject = new ApiTimekeeping.TimesheetRef
                {
                    Number = (reply0.Signature?.Subject?.Number ?? ""),
                },
            } },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Signature?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "clock_event",
            "audit", "ListClockEventWorkflows", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pauses a clock event.
    /// </summary>
    public override async Task<ApiTimekeeping.PauseClockEventWorkflowResponse> PauseClockEventWorkflow(ApiTimekeeping.PauseClockEventWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PauseClockEventWorkflow for {Tenant}", requestContext.TenantId);
        if (request.ClockEvent is { } record)
        {
            var snapshot = ClockEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTimekeeping.StartTimesheetWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTimekeeping.StartTimesheetWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                },
            },
        };
        var command0 = new DispatchCommand<ApiTimekeeping.StartTimesheetWorkflowRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _timekeepingTimesheetWorkflowService.StartTimesheetWorkflowAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTimekeeping.PauseClockEventWorkflowResponse
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                Attributes = { (request.ClockEvent?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ClockEventId = (request.ClockEventId ?? ""),
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Subject = new ApiTimekeeping.TimesheetRef
                {
                    Number = (reply0.Timesheet?.Number ?? ""),
                },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "clock_event",
            "audit", "PauseClockEventWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Resumes a clock event.
    /// </summary>
    public override async Task<ApiTimekeeping.ResumeClockEventWorkflowResponse> ResumeClockEventWorkflow(ApiTimekeeping.ResumeClockEventWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ResumeClockEventWorkflow for {Tenant}", requestContext.TenantId);
        if (request.ClockEvent is { } record)
        {
            var snapshot = ClockEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var profile0 = new MappingProfile<ApiTimekeeping.ResumeClockEventWorkflowRequest, ApiTimekeeping.UpdateTimesheetRequest>(value => new ApiTimekeeping.UpdateTimesheetRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Timesheet = new ApiTimekeeping.Timesheet
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (value.ClockEvent?.Subject?.DisplayName ?? ""),
                },
                TimesheetId = (value.ClockEvent?.Subject?.TimesheetId ?? ""),
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _timekeepingTimesheetService.UpdateTimesheetAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTimekeeping.SubmitClockEventRequest
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiTimekeeping.TimesheetRef
                {
                    DisplayName = (reply0.Timesheet?.Counterparty?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTimekeeping.SubmitClockEventRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    DisplayName = (request.ClockEvent?.Subject?.DisplayName ?? ""),
                    Email = (request.Details?.Notify?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _timekeepingClockEventApprovalService.SubmitClockEventAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTimekeeping.ResumeClockEventWorkflowResponse
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                ClockEventId = (reply1.ClockEvent?.ClockEventId ?? ""),
                Kind = (request.ClockEvent?.Kind ?? ""),
                Source = (request.ClockEvent?.Source ?? ""),
            },
            Outcome = new ApiTimekeeping.ResumeClockEventWorkflowResponse.Types.Outcome
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "clock_event",
            "audit", "ResumeClockEventWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Retrys a clock event.
    /// </summary>
    public override async Task<ApiTimekeeping.RetryClockEventWorkflowResponse> RetryClockEventWorkflow(ApiTimekeeping.RetryClockEventWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RetryClockEventWorkflow for {Tenant}", requestContext.TenantId);
        if (request.ClockEvent is { } record)
        {
            var snapshot = ClockEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = request.RetryClockEventWorkflowToFindEmployees0();
        using var rpc0 = _employeesEmployeeQueryService.FindEmployeesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTimekeeping.RetryClockEventWorkflowResponse
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                ClockEventId = (request.ClockEventId ?? ""),
                Subject = (request.ClockEvent?.Subject ?? new ApiTimekeeping.TimesheetRef()),
            },
            Outcome = new ApiTimekeeping.RetryClockEventWorkflowResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "clock_event",
            "audit", "RetryClockEventWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Signals a clock event.
    /// </summary>
    public override async Task<ApiTimekeeping.SignalClockEventWorkflowResponse> SignalClockEventWorkflow(ApiTimekeeping.SignalClockEventWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SignalClockEventWorkflow for {Tenant}", requestContext.TenantId);
        if (request.ClockEvent is { } record)
        {
            var snapshot = ClockEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTimekeeping.CheckOvertimeEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTimekeeping.CheckOvertimeEligibilityRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                },
            },
            Overtime = new ApiTimekeeping.Overtime
            {
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.ClockEvent?.Subject?.DisplayName ?? ""),
                },
            },
        };
        var reply0 = await SendSignalClockEventWorkflow0Async(input0, token);
        var input1 = new ApiEmployees.ValidatePositionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.ValidatePositionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply0.Overtime?.Owner?.Email ?? ""),
                },
            },
            Position = new ApiEmployees.Position
            {
                Discount = new ApiEmployees.PositionDiscountAction
                {
                    Amount = (reply0.Overtime ?? new ApiTimekeeping.Overtime()).ActionCase switch
                    {
                        ApiTimekeeping.Overtime.ActionOneofCase.Discount => (reply0.Overtime?.Discount?.Amount ?? new ApiCommon.Money()),
                        _ => new ApiCommon.Money()
                    },
                },
            },
        };
        using var rpc1 = _employeesPositionValidationService.ValidatePositionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTimekeeping.SignalClockEventWorkflowResponse
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                ClockEventId = (request.ClockEventId ?? ""),
                Kind = (request.ClockEvent?.Kind ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "clock_event",
            "audit", "SignalClockEventWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Starts a clock event.
    /// </summary>
    public override async Task<ApiTimekeeping.StartClockEventWorkflowResponse> StartClockEventWorkflow(ApiTimekeeping.StartClockEventWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling StartClockEventWorkflow for {Tenant}", requestContext.TenantId);
        if (request.ClockEvent is { } record)
        {
            var snapshot = ClockEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.ResolveEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiEmployees.ResolveEmployeesRequest>();
        using var rpc0 = _employeesEmployeeQueryService.ResolveEmployeesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTimekeeping.EstimateClockEventRequest
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiTimekeeping.TimesheetRef
                {
                    Number = (request.ClockEvent?.Subject?.Number ?? ""),
                    TimesheetId = (request.ClockEvent?.Subject?.TimesheetId ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc1 = _timekeepingClockEventCalculationService.EstimateClockEventAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiTimekeeping.ListPendingTimesheetsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiTimekeeping.ListPendingTimesheetsRequest.Types.Filter
            {
                CreatedAfter = (request.ClockEvent?.OccurredAt ?? new Timestamp()),
                Labels = { (request.Details?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        using var rpc2 = _timekeepingTimesheetApprovalService.ListPendingTimesheetsAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        var input3 = new ApiTimekeeping.CalculateClockEventRequest
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiTimekeeping.TimesheetRef
                {
                    DisplayName = (reply1.ClockEvent?.Subject?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc3 = _timekeepingClockEventCalculationService.CalculateClockEventAsync(input3, cancellationToken: token);
        var pending3 = rpc3.ResponseAsync;
        await Task.WhenAll(pending2, pending3);
        var reply2 = await pending2;
        var reply3 = await pending3;
        var response = new ApiTimekeeping.StartClockEventWorkflowResponse
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                ClockEventId = (reply3.ClockEvent?.ClockEventId ?? ""),
                CorrelationId = (request.ClockEvent?.CorrelationId ?? ""),
                OccurredAt = (reply1.ClockEvent?.OccurredAt ?? new Timestamp()),
                Source = (request.ClockEvent?.Source ?? ""),
            },
            Outcome = new ApiTimekeeping.StartClockEventWorkflowResponse.Types.Outcome
            {
                Status = (reply2.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "clock_event",
            "audit", "StartClockEventWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiTimekeeping.CheckOvertimeEligibilityResponse> SendSignalClockEventWorkflow0Async(ApiTimekeeping.CheckOvertimeEligibilityRequest request, CancellationToken token)
    {
        return await _clockEventWorkflowEndpointSendSignalClockEventWorkflow0Port.SendAsync(request, token);
    }
}
