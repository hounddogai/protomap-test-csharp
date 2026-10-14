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
using ApiContent = Acme.Content.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Timekeeping;

public sealed partial class OvertimeApprovalEndpoint
{
    /// <summary>
    /// Approves an overtime.
    /// </summary>
    public override async Task<ApiTimekeeping.ApproveOvertimeResponse> ApproveOvertime(ApiTimekeeping.ApproveOvertimeRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ApproveOvertime for {Tenant}", requestContext.TenantId);
        if (request.Overtime is { } record)
        {
            var snapshot = OvertimeMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTimekeeping.UpdateTimesheetRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Timesheet = new ApiTimekeeping.Timesheet
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Overtime?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedAt = (request.Overtime?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Lines = { new ApiTimekeeping.TimesheetLine
                {
                    Tax = new ApiTimekeeping.TimesheetLineTax
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
        using var rpc0 = _timekeepingTimesheetService.UpdateTimesheetAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTimekeeping.SuspendOvertimeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTimekeeping.SuspendOvertimeRequest.Types.Details
            {
                NotifyContact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
            OvertimeId = (request.Overtime?.OvertimeId ?? ""),
        };
        var reply1 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _timekeepingOvertimeLifecycleService.SuspendOvertimeAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var batch2 = (request.Overtime?.Conditions ?? new Google.Protobuf.Collections.RepeatedField<ApiTimekeeping.OvertimeCondition>()).Select(async item =>
        {
            var input2 = new ApiEmployees.ScorePositionRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Position = new ApiEmployees.Position
                {
                    Conditions = { new ApiEmployees.PositionCondition
                    {
                        Attribute = (item.Attribute ?? ""),
                        Negate = item.Negate,
                        Operator = (item.Operator ?? ""),
                    } },
                },
            };
            using var rpc2 = _employeesPositionValidationService.ScorePositionAsync(input2, cancellationToken: token);
            var reply2 = await rpc2.ResponseAsync;
            return reply2;
        }).ToArray();
        var replies2 = await Task.WhenAll(batch2);
        var reply2 = replies2.LastOrDefault() ?? new ApiEmployees.ScorePositionResponse();
        var input3 = new ApiContent.SearchArticlesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiContent.SearchArticlesRequest.Types.Filter
            {
                CreatedAfter = (reply1.Overtime?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (request.Overtime?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply1.Overtime?.Owner?.UserId ?? ""),
                Region = (reply0.Timesheet?.BillingAddress?.Region ?? ""),
            },
        };
        using var rpc3 = _sendApproveOvertime3(input3, new CallOptions(cancellationToken: token));
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiTimekeeping.ApproveOvertimeResponse
        {
            Outcome = new ApiTimekeeping.ApproveOvertimeResponse.Types.Outcome
            {
                Message = (reply1.Outcome?.Message ?? ""),
            },
            Overtime = new ApiTimekeeping.Overtime
            {
                Name = (request.Overtime?.Name ?? ""),
                OvertimeId = (request.OvertimeId ?? ""),
                Scopes = { (reply1.Overtime?.Scopes ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply3.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply3.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "overtime",
            "audit", "ApproveOvertime", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Comments an overtime.
    /// </summary>
    public override async Task<ApiTimekeeping.CommentOvertimeApprovalResponse> CommentOvertimeApproval(ApiTimekeeping.CommentOvertimeApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CommentOvertimeApproval for {Tenant}", requestContext.TenantId);
        if (request.Overtime is { } record)
        {
            var snapshot = OvertimeMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTimekeeping.CompareOvertimeOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Overtime = new ApiTimekeeping.Overtime
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Overtime?.Owner?.Email ?? ""),
                },
            },
        };
        var named0 = _registry.Get<ApiTimekeeping.OvertimeCalculationService.OvertimeCalculationServiceClient>("acme.timekeeping.v1.OvertimeCalculationService");
        using var rpc0 = named0.CompareOvertimeOptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLedger.WatchLedgerAccountsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Subscriber = new ApiCommon.ContactInfo
            {
                Email = (request.Overtime?.Owner?.Email ?? ""),
            },
        };
        using var stream1 = _ledgerLedgerAccountService.WatchLedgerAccounts(input1, cancellationToken: token);
        await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received WatchLedgerAccounts frame {Type}", change1.GetType().Name);
        }
        var response = new ApiTimekeeping.CommentOvertimeApprovalResponse
        {
            Overtime = new ApiTimekeeping.Overtime
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.Overtime?.Audit?.Etag ?? ""),
                },
                OvertimeId = (request.OvertimeId ?? ""),
                Status = (request.Overtime?.Status ?? (ApiTimekeeping.OvertimeStatus)0),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "overtime",
            "audit", "CommentOvertimeApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Delegates an overtime.
    /// </summary>
    public override async Task<ApiTimekeeping.DelegateOvertimeApprovalResponse> DelegateOvertimeApproval(ApiTimekeeping.DelegateOvertimeApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling DelegateOvertimeApproval for {Tenant}", requestContext.TenantId);
        if (request.Overtime is { } record)
        {
            var snapshot = OvertimeMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTimekeeping.ScoreClockEventRequest
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiTimekeeping.TimesheetRef
                {
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _timekeepingClockEventValidationService.ScoreClockEventAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var profile1 = new MappingProfile<ApiTimekeeping.DelegateOvertimeApprovalRequest, ApiTimekeeping.ListPendingTimesheetsRequest>(value => new ApiTimekeeping.ListPendingTimesheetsRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiTimekeeping.ListPendingTimesheetsRequest.Types.Filter
            {
                CreatedAfter = (value.Overtime?.Audit?.CreatedAt ?? new Timestamp()),
            },
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _timekeepingTimesheetApprovalService.ListPendingTimesheetsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiEmployees.LintPositionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Position = new ApiEmployees.Position
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
        };
        input2 = PropertyMap.Copy(input2);
        using var rpc2 = _employeesPositionValidationService.LintPositionsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiTimekeeping.DelegateOvertimeApprovalResponse
        {
            Outcome = new ApiTimekeeping.DelegateOvertimeApprovalResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
            Overtime = new ApiTimekeeping.Overtime
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply2.Position?.Audit?.Etag ?? ""),
                },
                Effective = (reply2.Position?.Effective ?? new ApiCommon.TimeWindow()),
                OvertimeId = (request.OvertimeId ?? ""),
                Owner = (request.Overtime?.Owner ?? new ApiIdentity.UserRef()),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "overtime",
            "audit", "DelegateOvertimeApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Escalates an overtime.
    /// </summary>
    public override async Task<ApiTimekeeping.EscalateOvertimeResponse> EscalateOvertime(ApiTimekeeping.EscalateOvertimeRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EscalateOvertime for {Tenant}", requestContext.TenantId);
        if (request.Overtime is { } record)
        {
            var snapshot = OvertimeMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiTimekeeping.RecalculateOvertimesResponse();
        foreach (var item in (request.Overtime?.Conditions ?? new Google.Protobuf.Collections.RepeatedField<ApiTimekeeping.OvertimeCondition>()))
        {
            var input0 = new ApiTimekeeping.RecalculateOvertimesRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Overtime = new ApiTimekeeping.Overtime
                {
                    Conditions = { new ApiTimekeeping.OvertimeCondition
                    {
                        Attribute = (item.Attribute ?? ""),
                        Negate = item.Negate,
                        Operator = (item.Operator ?? ""),
                        Value = (item.Value ?? ""),
                    } },
                },
            };
            using var rpc0 = _timekeepingOvertimeCalculationService.RecalculateOvertimesAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var input1 = new ApiTimekeeping.ExplainClockEventDecisionRequest
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiTimekeeping.TimesheetRef
                {
                    DisplayName = (reply0.Overtime?.Owner?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTimekeeping.ExplainClockEventDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Overtime?.Owner?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _timekeepingClockEventValidationService.ExplainClockEventDecisionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTimekeeping.EscalateOvertimeResponse
        {
            Outcome = new ApiTimekeeping.EscalateOvertimeResponse.Types.Outcome
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
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "overtime",
            "audit", "EscalateOvertime", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an overtime.
    /// </summary>
    public override async Task<ApiTimekeeping.GetOvertimeApprovalStatusResponse> GetOvertimeApprovalStatus(ApiTimekeeping.GetOvertimeApprovalStatusRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.OvertimeId, "overtime_id");
        _logger.LogInformation("Handling GetOvertimeApprovalStatus for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "overtime", request.OvertimeId,
            "GetOvertimeApprovalStatus", "accepted", token);
        var input0 = new ApiEmployees.ScorePositionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Position = new ApiEmployees.Position
            {
                Owner = new ApiIdentity.UserRef
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
        using var rpc0 = _employeesPositionValidationService.ScorePositionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTimekeeping.ReinstateOvertimeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTimekeeping.ReinstateOvertimeRequest.Types.Details
            {
                NewOwner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Position?.Owner?.DisplayName ?? ""),
                    Email = (reply0.Position ?? new ApiEmployees.Position()).ActionCase switch
                    {
                        ApiEmployees.Position.ActionOneofCase.Block => (reply0.Position?.Block?.NotifyEmail ?? ""),
                        _ => ""
                    },
                    UserId = (reply0.Position?.Owner?.UserId ?? ""),
                },
                NotifyContact = new ApiCommon.ContactInfo
                {
                    Email = (reply0.Position?.Owner?.Email ?? ""),
                },
            },
            OvertimeId = (request.OvertimeId ?? ""),
        };
        using var rpc1 = _timekeepingOvertimeLifecycleService.ReinstateOvertimeAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTimekeeping.GetOvertimeApprovalStatusResponse
        {
            Overtime = new ApiTimekeeping.Overtime
            {
                Name = (reply0.Position?.Name ?? ""),
                OvertimeId = (request.OvertimeId ?? ""),
                Owner = (reply1.Overtime?.Owner ?? new ApiIdentity.UserRef()),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        return response;
    }
    /// <summary>
    /// Lists overtimes a page at a time.
    /// </summary>
    public override async Task<ApiTimekeeping.ListPendingOvertimesResponse> ListPendingOvertimes(ApiTimekeeping.ListPendingOvertimesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListPendingOvertimes for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "overtime", "batch",
            "ListPendingOvertimes", "accepted", token);
        var response = new ApiTimekeeping.ListPendingOvertimesResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "overtime",
            "audit", "ListPendingOvertimes", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalls an overtime.
    /// </summary>
    public override async Task<ApiTimekeeping.RecallOvertimeResponse> RecallOvertime(ApiTimekeeping.RecallOvertimeRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecallOvertime for {Tenant}", requestContext.TenantId);
        if (request.Overtime is { } record)
        {
            var snapshot = OvertimeMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTimekeeping.ApproveTimesheetRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTimekeeping.ApproveTimesheetRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
            Timesheet = new ApiTimekeeping.Timesheet
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Overtime?.Audit?.CreatedBy ?? ""),
                    UpdatedBy = (request.Overtime?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiTimekeeping.ApproveTimesheetRequest>(json0);
        using var rpc0 = _timekeepingTimesheetApprovalService.ApproveTimesheetAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTravel.ReconcileTravelPoliciesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.ReconcileTravelPoliciesRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Overtime?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (reply0.Timesheet?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        using var rpc1 = _travelTravelPolicySyncService.ReconcileTravelPoliciesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiEmployees.BatchGetEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc2 = _employeesEmployeeService.BatchGetEmployeesAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiTimekeeping.RecallOvertimeResponse
        {
            Outcome = new ApiTimekeeping.RecallOvertimeResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Overtime = new ApiTimekeeping.Overtime
            {
                OvertimeId = (request.OvertimeId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply1.TravelPolicy?.Owner?.Email ?? ""),
                    UserId = (reply1.TravelPolicy?.Owner?.UserId ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Result?.Message ?? ""),
                ReferenceIds = { (reply2.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "overtime",
            "audit", "RecallOvertime", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Rejects an overtime.
    /// </summary>
    public override async Task<ApiTimekeeping.RejectOvertimeResponse> RejectOvertime(ApiTimekeeping.RejectOvertimeRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RejectOvertime for {Tenant}", requestContext.TenantId);
        if (request.Overtime is { } record)
        {
            var snapshot = OvertimeMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiHelpdesk.RecallSlaPolicyResponse();
        foreach (var item in (request.Overtime?.Conditions ?? new Google.Protobuf.Collections.RepeatedField<ApiTimekeeping.OvertimeCondition>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input0 = new ApiHelpdesk.RecallSlaPolicyRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                SlaPolicy = new ApiHelpdesk.SlaPolicy
                {
                    Conditions = { new ApiHelpdesk.SlaPolicyCondition
                    {
                        Value = (item.Value ?? ""),
                    } },
                },
            };
            using var rpc0 = _helpdeskSlaPolicyApprovalService.RecallSlaPolicyAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var input1 = new ApiEmployees.ResolveOrgUnitsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply1 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _employeesOrgUnitQueryService.ResolveOrgUnitsAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiTimekeeping.RejectOvertimeResponse
        {
            Outcome = new ApiTimekeeping.RejectOvertimeResponse.Types.Outcome
            {
                Status = (reply1.Result?.Status ?? ""),
            },
            Overtime = new ApiTimekeeping.Overtime
            {
                Description = (request.Overtime?.Description ?? ""),
                OvertimeId = (request.OvertimeId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "overtime",
            "audit", "RejectOvertime", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Submits an overtime.
    /// </summary>
    public override async Task<ApiTimekeeping.SubmitOvertimeResponse> SubmitOvertime(ApiTimekeeping.SubmitOvertimeRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SubmitOvertime for {Tenant}", requestContext.TenantId);
        if (request.Overtime is { } record)
        {
            var snapshot = OvertimeMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.EscalateSlaPolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.EscalateSlaPolicyRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Overtime?.Owner?.DisplayName ?? ""),
                },
            },
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.Overtime?.Owner?.UserId ?? ""),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _helpdeskSlaPolicyApprovalService.EscalateSlaPolicyAsync);
        var input1 = new ApiTimekeeping.SubmitClockEventRequest
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiTimekeeping.TimesheetRef
                {
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTimekeeping.SubmitClockEventRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    UserId = (request.Overtime?.Owner?.UserId ?? ""),
                },
            },
        };
        using var rpc1 = _timekeepingClockEventApprovalService.SubmitClockEventAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiTimekeeping.SubmitClockEventRequest>("acme.timekeeping.v1.OvertimeApprovalService/SubmitOvertime/1", input1.Clone()), token);
        var response = new ApiTimekeeping.SubmitOvertimeResponse
        {
            Outcome = new ApiTimekeeping.SubmitOvertimeResponse.Types.Outcome
            {
                Message = (reply1.Outcome?.Message ?? ""),
                NextSteps = { (reply1.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                ReferenceId = (reply0.Outcome?.ReferenceId ?? ""),
            },
            Overtime = new ApiTimekeeping.Overtime
            {
                Effective = (request.Overtime?.Effective ?? new ApiCommon.TimeWindow()),
                Name = (reply0.SlaPolicy?.Name ?? ""),
                OvertimeId = (request.OvertimeId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "overtime",
            "audit", "SubmitOvertime", (request.Reason ?? ""), token);
        return response;
    }
}
