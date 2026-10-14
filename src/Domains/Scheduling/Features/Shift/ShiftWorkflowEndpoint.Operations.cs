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
using ApiModelregistry = Acme.Modelregistry.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiScheduling = Acme.Scheduling.V1;

namespace Acme.Workforce.Scheduling;

public sealed partial class ShiftWorkflowEndpoint
{
    /// <summary>
    /// Advances a shift.
    /// </summary>
    public override async Task<ApiScheduling.AdvanceShiftWorkflowResponse> AdvanceShiftWorkflow(ApiScheduling.AdvanceShiftWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling AdvanceShiftWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Shift is { } record)
        {
            var snapshot = ShiftMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiModelregistry.SimulateRegisteredModelRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiModelregistry.SimulateRegisteredModelRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Locality = (request.Shift?.Location?.Locality ?? ""),
                },
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _modelregistryRegisteredModelCalculationService.SimulateRegisteredModelAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var batch1 = (request.Shift?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiScheduling.ShiftParticipant>()).Select(async item =>
        {
            var input1 = new ApiScheduling.UpdateShiftRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Shift = new ApiScheduling.Shift
                {
                    Participants = { new ApiScheduling.ShiftParticipant
                    {
                        Contact = new ApiCommon.ContactInfo
                        {
                            Email = (item.Contact?.Email ?? ""),
                            Name = (item.Contact?.Name ?? new ApiCommon.PersonName()),
                        },
                        Required = item.Required,
                        Role = (item.Role ?? ""),
                    } },
                },
            };
            using var rpc1 = _schedulingShiftService.UpdateShiftAsync(input1, cancellationToken: token);
            var reply1 = await rpc1.ResponseAsync;
            return reply1;
        }).ToArray();
        var replies1 = await Task.WhenAll(batch1);
        var reply1 = replies1.LastOrDefault() ?? new ApiScheduling.UpdateShiftResponse();
        var input2 = new ApiScheduling.ResolveRostersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var json2 = JsonFormatter.Default.Format(input2);
        input2 = JsonParser.Default.Parse<ApiScheduling.ResolveRostersRequest>(json2);
        using var rpc2 = _schedulingRosterQueryService.ResolveRostersAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiScheduling.AdvanceShiftWorkflowResponse
        {
            Outcome = new ApiScheduling.AdvanceShiftWorkflowResponse.Types.Outcome
            {
                Message = (reply2.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
            Shift = new ApiScheduling.Shift
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.RegisteredModel?.Audit?.UpdatedBy ?? ""),
                },
                Location = (request.Shift?.Location ?? new ApiCommon.PostalAddress()),
                ShiftId = (request.ShiftId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Shift?.Title ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "shift",
            "audit", "AdvanceShiftWorkflow", (request.Shift?.Title ?? ""), token);
        return response;
    }
    /// <summary>
    /// Cancels a shift.
    /// </summary>
    public override async Task<ApiScheduling.CancelShiftWorkflowResponse> CancelShiftWorkflow(ApiScheduling.CancelShiftWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CancelShiftWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Shift is { } record)
        {
            var snapshot = ShiftMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiScheduling.ScheduleAvailabilityWindowExpiryRequest
        {
            AvailabilityWindow = new ApiScheduling.AvailabilityWindow
            {
                Location = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Shift?.Location?.CountryCode ?? ""),
                },
                Participants = { new ApiScheduling.AvailabilityWindowParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Details?.Notify?.Email ?? ""),
                    },
                } },
                Weekly = new ApiScheduling.AvailabilityWindowWeeklyRule
                {
                    Days = { (request.Shift ?? new ApiScheduling.Shift()).RecurrenceCase switch
                    {
                        ApiScheduling.Shift.RecurrenceOneofCase.Weekly => (request.Shift?.Weekly?.Days ?? new Google.Protobuf.Collections.RepeatedField<string>()),
                        _ => new Google.Protobuf.Collections.RepeatedField<string>()
                    } },
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _schedulingAvailabilityWindowLifecycleService.ScheduleAvailabilityWindowExpiryAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiModelregistry.CloneRegisteredModelRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            RegisteredModel = new ApiModelregistry.RegisteredModel
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.AvailabilityWindow?.Audit?.CreatedBy ?? ""),
                },
            },
        };
        using var rpc1 = _modelregistryRegisteredModelService.CloneRegisteredModelAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiEmployees.ExportOrgUnitsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.ExportOrgUnitsRequest.Types.Filter
            {
                CreatedAfter = (reply0.AvailabilityWindow?.Audit?.UpdatedAt ?? new Timestamp()),
                OwnerId = (reply1.RegisteredModel?.Owner?.UserId ?? ""),
                Region = (reply1.RegisteredModel?.Address?.Region ?? ""),
            },
        };
        using var stream2 = _employeesOrgUnitQueryService.ExportOrgUnits(input2, cancellationToken: token);
        await foreach (var change2 in stream2.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportOrgUnits frame {Type}", change2.GetType().Name);
        }
        var response = new ApiScheduling.CancelShiftWorkflowResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Outcome?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Shift = new ApiScheduling.Shift
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.RegisteredModel?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Location = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply1.RegisteredModel?.Address?.PostalCode ?? ""),
                },
                ShiftId = (request.ShiftId ?? ""),
                Window = (request.Shift?.Window ?? new ApiCommon.TimeWindow()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Shift?.Title ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "shift",
            "audit", "CancelShiftWorkflow", (request.Shift?.Title ?? ""), token);
        return response;
    }
    /// <summary>
    /// Lists shifts a page at a time.
    /// </summary>
    public override async Task<ApiScheduling.ListShiftWorkflowsResponse> ListShiftWorkflows(ApiScheduling.ListShiftWorkflowsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListShiftWorkflows for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "shift", "batch",
            "ListShiftWorkflows", "accepted", token);
        var response = new ApiScheduling.ListShiftWorkflowsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "shift",
            "audit", "ListShiftWorkflows", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Retrys a shift.
    /// </summary>
    public override async Task<ApiScheduling.RetryShiftWorkflowResponse> RetryShiftWorkflow(ApiScheduling.RetryShiftWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RetryShiftWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Shift is { } record)
        {
            var snapshot = ShiftMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiScheduling.FindRostersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiScheduling.FindRostersRequest.Types.Filter
            {
                CreatedAfter = (request.Shift?.Audit?.CreatedAt ?? new Timestamp()),
                OwnerId = (request.Shift?.Organizer?.UserId ?? ""),
                Region = (request.Shift?.Location?.Region ?? ""),
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _schedulingRosterQueryService.FindRostersAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiScheduling.CommentRosterApprovalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Roster = new ApiScheduling.Roster
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Shift?.Location?.CountryCode ?? ""),
                },
            },
        };
        var reply1 = await Workflow.SendAsync(input1, token, _schedulingRosterApprovalService.CommentRosterApprovalAsync);
        var input2 = new ApiPerformance.SignalGoalWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Goal = new ApiPerformance.Goal
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply1.Roster?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        using var rpc2 = _sendRetryShiftWorkflow2(input2, new CallOptions(cancellationToken: token));
        var reply2 = await rpc2.ResponseAsync;
        var reply3 = new ApiPerformance.ApproveReviewResponse();
        foreach (var item in (request.Shift?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiScheduling.ShiftParticipant>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input3 = new ApiPerformance.ApproveReviewRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiPerformance.ApproveReviewRequest.Types.Details
                {
                    Approver = new ApiIdentity.UserRef
                    {
                        Email = (item.Contact?.Email ?? ""),
                    },
                },
                Review = new ApiPerformance.Review
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        UpdatedBy = (request.Shift?.Audit?.UpdatedBy ?? ""),
                    },
                    Card = new ApiPerformance.ReviewCardSettlement
                    {
                        Network = (reply1.Roster?.Card?.Network ?? ""),
                    },
                },
            };
            using var rpc3 = _performanceReviewApprovalService.ApproveReviewAsync(input3, cancellationToken: token);
            reply3 = await rpc3.ResponseAsync;
        }
        var response = new ApiScheduling.RetryShiftWorkflowResponse
        {
            Outcome = new ApiScheduling.RetryShiftWorkflowResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            Shift = new ApiScheduling.Shift
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply2.Goal?.Audit?.Etag ?? ""),
                },
                Location = new ApiCommon.PostalAddress
                {
                    Lines = { (reply1.Roster?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
                ShiftId = (request.ShiftId ?? ""),
                Window = (request.Shift?.Window ?? new ApiCommon.TimeWindow()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Shift?.Title ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "shift",
            "audit", "RetryShiftWorkflow", (request.Shift?.Title ?? ""), token);
        return response;
    }
    /// <summary>
    /// Signals a shift.
    /// </summary>
    public override async Task<ApiScheduling.SignalShiftWorkflowResponse> SignalShiftWorkflow(ApiScheduling.SignalShiftWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SignalShiftWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Shift is { } record)
        {
            var snapshot = ShiftMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiScheduling.EscalateShiftRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Shift = new ApiScheduling.Shift
            {
                Participants = { new ApiScheduling.ShiftParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Name = (request.Details?.Notify?.Name ?? new ApiCommon.PersonName()),
                    },
                } },
            },
        };
        var descriptor0 = new Method<ApiScheduling.EscalateShiftRequest, ApiScheduling.EscalateShiftResponse>(
            MethodType.Unary, "acme.scheduling.v1" + ".ShiftApprovalService", "EscalateShift",
            Wire.Marshaller<ApiScheduling.EscalateShiftRequest>(), Wire.Marshaller<ApiScheduling.EscalateShiftResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiScheduling.ListShiftsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiScheduling.ListShiftsRequest.Types.Filter
            {
                CreatedAfter = (reply0.Shift?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (request.Shift?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply0.Shift?.Organizer?.UserId ?? ""),
                Region = (request.Shift?.Location?.Region ?? ""),
            },
        };
        var command1 = new DispatchCommand<ApiScheduling.ListShiftsRequest>(input1,
            "primary", requestContext.TenantId);
        var routed1 = command1 with { Region = _settings.Value.Region };
        using var rpc1 = _schedulingShiftService.ListShiftsAsync(routed1.Payload, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiScheduling.SignalShiftWorkflowResponse
        {
            Outcome = new ApiScheduling.SignalShiftWorkflowResponse.Types.Outcome
            {
                Message = (reply1.Result?.Message ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
            Shift = new ApiScheduling.Shift
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Shift?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Location = new ApiCommon.PostalAddress
                {
                    Recipient = (reply0.Shift?.Location?.Recipient ?? ""),
                },
                ShiftId = (request.ShiftId ?? ""),
                Window = (request.Shift?.Window ?? new ApiCommon.TimeWindow()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Shift?.Title ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "shift",
            "audit", "SignalShiftWorkflow", (request.Shift?.Title ?? ""), token);
        return response;
    }
    /// <summary>
    /// Starts a shift.
    /// </summary>
    public override async Task<ApiScheduling.StartShiftWorkflowResponse> StartShiftWorkflow(ApiScheduling.StartShiftWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling StartShiftWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Shift is { } record)
        {
            var snapshot = ShiftMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var profile0 = new MappingProfile<ApiScheduling.StartShiftWorkflowRequest, ApiPerformance.SubmitReviewRequest>(value => new ApiPerformance.SubmitReviewRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Review = new ApiPerformance.Review
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (value.Shift?.Location?.CountryCode ?? ""),
                    Region = (value.Shift?.Location?.Region ?? ""),
                },
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _performanceReviewApprovalService.SubmitReviewAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.ExplainEmploymentContractCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Shift?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (request.Shift?.Location?.Locality ?? ""),
                },
            },
        };
        input1 = PropertyMap.Copy(input1);
        using var rpc1 = _employeesEmploymentContractCalculationService.ExplainEmploymentContractCalculationAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        foreach (var item in (request.Shift?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiScheduling.ShiftParticipant>()))
        {
            var input2 = new ApiEmployees.WatchEmployeesRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Subscriber = new ApiCommon.ContactInfo
                {
                    Email = (item.Contact?.Email ?? ""),
                },
            };
            using var stream2 = _employeesEmployeeService.WatchEmployees(input2, cancellationToken: token);
            await foreach (var change2 in stream2.ResponseStream.ReadAllAsync(token))
            {
                _logger.LogDebug("Received WatchEmployees frame {Type}", change2.GetType().Name);
            }
        }
        var response = new ApiScheduling.StartShiftWorkflowResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
            Shift = new ApiScheduling.Shift
            {
                Metadata = { (reply1.EmploymentContract?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ShiftId = (request.ShiftId ?? ""),
                Status = (request.Shift?.Status ?? (ApiScheduling.ShiftStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Shift?.Title ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "shift",
            "audit", "StartShiftWorkflow", (request.Shift?.Title ?? ""), token);
        return response;
    }
}
