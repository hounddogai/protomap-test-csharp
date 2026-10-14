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
using ApiAdmissions = Acme.Admissions.V1;
using ApiBenefits = Acme.Benefits.V1;
using ApiCommon = Acme.Common.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiSellers = Acme.Sellers.V1;

namespace Acme.Workforce.Scheduling;

public sealed partial class SwapRequestLifecycleEndpoint
{
    /// <summary>
    /// Activates a swap request.
    /// </summary>
    public override async Task<ApiScheduling.ActivateSwapRequestResponse> ActivateSwapRequest(ApiScheduling.ActivateSwapRequestRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.SwapRequestId, "swap_request_id");
        _logger.LogInformation("Handling ActivateSwapRequest for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "swap_request", request.SwapRequestId,
            "ActivateSwapRequest", "accepted", token);
        var input0 = new ApiBenefits.RetryEnrollmentWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiBenefits.RetryEnrollmentWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                    Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                    PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                },
            },
            Enrollment = new ApiBenefits.Enrollment
            {
                Holder = new ApiRecruiting.CandidateRef
                {
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiBenefits.RetryEnrollmentWorkflowRequest>();
        using var rpc0 = _benefitsEnrollmentWorkflowService.RetryEnrollmentWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiSellers.ListSellersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiSellers.ListSellersRequest.Types.Filter
            {
                Labels = { (reply0.Enrollment?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        var json1 = JsonFormatter.Default.Format(input1);
        input1 = JsonParser.Default.Parse<ApiSellers.ListSellersRequest>(json1);
        using var rpc1 = _sellersSellerService.ListSellersAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiScheduling.EstimateAvailabilityWindowRequest();
        input2.AvailabilityWindow = new ApiScheduling.AvailabilityWindow
        {
            Participants = { new ApiScheduling.AvailabilityWindowParticipant
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                },
            } },
        };
        input2.Context = (request.Context ?? new ApiCommon.RequestContext());
        using var rpc2 = _schedulingAvailabilityWindowCalculationService.EstimateAvailabilityWindowAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiScheduling.ActivateSwapRequestResponse
        {
            Outcome = new ApiScheduling.ActivateSwapRequestResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Message = (reply0.Result?.Message ?? ""),
            },
            SwapRequest = new ApiScheduling.SwapRequest
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply2.AvailabilityWindow?.Audit?.UpdatedBy ?? ""),
                },
                SwapRequestId = (request.SwapRequestId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "swap_request",
            "audit", "ActivateSwapRequest", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a swap request.
    /// </summary>
    public override async Task<ApiScheduling.GetSwapRequestLifecycleResponse> GetSwapRequestLifecycle(ApiScheduling.GetSwapRequestLifecycleRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.SwapRequestId, "swap_request_id");
        _logger.LogInformation("Handling GetSwapRequestLifecycle for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "swap_request", request.SwapRequestId,
            "GetSwapRequestLifecycle", "accepted", token);
        var input0 = new ApiScheduling.ResolveShiftsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _schedulingShiftQueryService.ResolveShiftsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiScheduling.GetSwapRequestLifecycleResponse
        {
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            SwapRequest = new ApiScheduling.SwapRequest
            {
                SwapRequestId = (request.SwapRequestId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Reinstates a swap request.
    /// </summary>
    public override async Task<ApiScheduling.ReinstateSwapRequestResponse> ReinstateSwapRequest(ApiScheduling.ReinstateSwapRequestRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.SwapRequestId, "swap_request_id");
        _logger.LogInformation("Handling ReinstateSwapRequest for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "swap_request", request.SwapRequestId,
            "ReinstateSwapRequest", "accepted", token);
        var input0 = new ApiScheduling.ListShiftsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiScheduling.ListShiftsRequest.Types.Filter
            {
                OwnerId = (request.SwapRequestId ?? ""),
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _schedulingShiftService.ListShiftsAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiScheduling.CountShiftsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiScheduling.CountShiftsRequest.Types.Filter
            {
                CreatedAfter = (request.Details?.EffectiveAt ?? new Timestamp()),
                OwnerId = (request.Details?.NewOwner?.UserId ?? ""),
            },
        };
        var reply1 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _schedulingShiftQueryService.CountShiftsAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiScheduling.ReinstateSwapRequestResponse
        {
            Outcome = new ApiScheduling.ReinstateSwapRequestResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            SwapRequest = new ApiScheduling.SwapRequest
            {
                SwapRequestId = (request.SwapRequestId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "swap_request",
            "audit", "ReinstateSwapRequest", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Schedules a swap request.
    /// </summary>
    public override async Task<ApiScheduling.ScheduleSwapRequestExpiryResponse> ScheduleSwapRequestExpiry(ApiScheduling.ScheduleSwapRequestExpiryRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScheduleSwapRequestExpiry for {Tenant}", requestContext.TenantId);
        if (request.SwapRequest is { } record)
        {
            var snapshot = SwapRequestMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiScheduling.SearchShiftsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiScheduling.SearchShiftsRequest.Types.Filter
            {
                CreatedAfter = (request.SwapRequest?.Audit?.CreatedAt ?? new Timestamp()),
                OwnerId = (request.SwapRequest?.Sla?.PolicyId ?? ""),
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _schedulingShiftService.SearchShiftsAsync);
        var input1 = new ApiLedger.CloneLedgerAccountRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LedgerAccount = new ApiLedger.LedgerAccount
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.SwapRequest?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Notify?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.SwapRequest?.Requester?.Phone ?? ""),
                },
                Holder = new ApiLedger.PostingBatchRef
                {
                    DisplayName = (request.SwapRequest?.Assignee?.DisplayName ?? ""),
                },
            },
        };
        using var rpc1 = _sendScheduleSwapRequestExpiry1(input1, new CallOptions(cancellationToken: token));
        var reply1 = await rpc1.ResponseAsync;
        var reply2 = new ApiAdmissions.CloneAdmissionResponse();
        foreach (var item in (request.SwapRequest?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiScheduling.SwapRequestComment>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input2 = new ApiAdmissions.CloneAdmissionRequest
            {
                Admission = new ApiAdmissions.Admission
                {
                    CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                    Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                    OccurredAt = (item.CreatedAt ?? new Timestamp()),
                    Subject = new ApiAdmissions.GateScanRef
                    {
                        DisplayName = (reply1.LedgerAccount?.Holder?.DisplayName ?? ""),
                    },
                },
                Context = (request.Context ?? new ApiCommon.RequestContext()),
            };
            using var rpc2 = _admissionsAdmissionService.CloneAdmissionAsync(input2, cancellationToken: token);
            reply2 = await rpc2.ResponseAsync;
        }
        var response = new ApiScheduling.ScheduleSwapRequestExpiryResponse
        {
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
            SwapRequest = new ApiScheduling.SwapRequest
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply1.LedgerAccount?.Audit?.UpdatedBy ?? ""),
                },
                Priority = (request.SwapRequest?.Priority ?? 0),
                SwapRequestId = (request.SwapRequestId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SwapRequest?.SwapRequestId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "swap_request",
            "audit", "ScheduleSwapRequestExpiry", (request.SwapRequest?.SwapRequestId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Suspends a swap request.
    /// </summary>
    public override async Task<ApiScheduling.SuspendSwapRequestResponse> SuspendSwapRequest(ApiScheduling.SuspendSwapRequestRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.SwapRequestId, "swap_request_id");
        _logger.LogInformation("Handling SuspendSwapRequest for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "swap_request", request.SwapRequestId,
            "SuspendSwapRequest", "accepted", token);
        var input0 = new ApiScheduling.FindShiftsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiScheduling.FindShiftsRequest.Types.Filter
            {
                CreatedAfter = (request.Details?.EffectiveAt ?? new Timestamp()),
                Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Details?.NewOwner?.UserId ?? ""),
            },
        };
        var descriptor0 = new Method<ApiScheduling.FindShiftsRequest, ApiScheduling.FindShiftsResponse>(
            MethodType.Unary, "acme.scheduling.v1" + ".ShiftQueryService", "FindShifts",
            Wire.Marshaller<ApiScheduling.FindShiftsRequest>(), Wire.Marshaller<ApiScheduling.FindShiftsResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.RecalculateEmploymentContractsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                },
            },
        };
        var command1 = new DispatchCommand<ApiEmployees.RecalculateEmploymentContractsRequest>(input1,
            "primary", requestContext.TenantId);
        var routed1 = command1 with { Region = _settings.Value.Region };
        using var rpc1 = _employeesEmploymentContractCalculationService.RecalculateEmploymentContractsAsync(routed1.Payload, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiScheduling.SuspendSwapRequestResponse
        {
            Outcome = new ApiScheduling.SuspendSwapRequestResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            SwapRequest = new ApiScheduling.SwapRequest
            {
                SwapRequestId = (request.SwapRequestId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "swap_request",
            "audit", "SuspendSwapRequest", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Terminates a swap request.
    /// </summary>
    public override async Task<ApiScheduling.TerminateSwapRequestResponse> TerminateSwapRequest(ApiScheduling.TerminateSwapRequestRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.SwapRequestId, "swap_request_id");
        _logger.LogInformation("Handling TerminateSwapRequest for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "swap_request", request.SwapRequestId,
            "TerminateSwapRequest", "accepted", token);
        var profile0 = new MappingProfile<ApiScheduling.TerminateSwapRequestRequest, ApiIdentity.CheckUserEligibilityRequest>(value => new ApiIdentity.CheckUserEligibilityRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiIdentity.CheckUserEligibilityRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (value.Details?.NotifyContact?.Email ?? ""),
                    PreferredChannel = (value.Details?.NotifyContact?.PreferredChannel ?? ""),
                },
            },
            User = new ApiIdentity.User
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (value.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                },
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _identityUserValidationService.CheckUserEligibilityAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLedger.ListLedgerAccountsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLedger.ListLedgerAccountsRequest.Types.Filter
            {
                CreatedAfter = (reply0.User?.Audit?.UpdatedAt ?? new Timestamp()),
                OwnerId = (request.Details?.NewOwner?.UserId ?? ""),
                Region = (reply0.User?.PrimaryAddress?.Region ?? ""),
            },
        };
        input1 = PropertyMap.Copy(input1);
        using var rpc1 = _ledgerLedgerAccountService.ListLedgerAccountsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiScheduling.TerminateSwapRequestResponse
        {
            Outcome = new ApiScheduling.TerminateSwapRequestResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Message = (reply1.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply1.Result?.Status ?? ""),
            },
            SwapRequest = new ApiScheduling.SwapRequest
            {
                Requester = new ApiCommon.ContactInfo
                {
                    Name = (reply0.User?.Contact?.Name ?? new ApiCommon.PersonName()),
                },
                SwapRequestId = (request.SwapRequestId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "swap_request",
            "audit", "TerminateSwapRequest", (request.Reason ?? ""), token);
        return response;
    }
}
