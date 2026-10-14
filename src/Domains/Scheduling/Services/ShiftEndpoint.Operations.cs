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
using ApiIdentity = Acme.Identity.V1;
using ApiScheduling = Acme.Scheduling.V1;

namespace Acme.Workforce.Scheduling;

public sealed partial class ShiftEndpoint
{
    /// <summary>
    /// Takes a stream of shifts and answers once at the end.
    /// </summary>
    public override async Task<ApiScheduling.BatchCreateShiftsResponse> BatchCreateShifts(IAsyncStreamReader<ApiScheduling.BatchCreateShiftsRequest> requests, ServerCallContext context)
    {
        var token = context.CancellationToken;
        var result = new ApiScheduling.BatchCreateShiftsResponse();
        await foreach (var request in requests.ReadAllAsync(token))
        {
            RequestGuard.Tenant(request.Context);
            var requestContext = request.Context;
            _logger.LogInformation("Handling BatchCreateShifts for {Tenant}", requestContext.TenantId);
            if (request.Record is { } record)
            {
                var snapshot = ShiftMapper.ToRecord(requestContext.TenantId, record);
                await _repository.SaveAsync(snapshot, token);
            }
            var batch0 = (request.Record?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiScheduling.ShiftParticipant>()).Select(async item =>
            {
                var input0 = new ApiScheduling.ValidateAvailabilityWindowRequest
                {
                    AvailabilityWindow = new ApiScheduling.AvailabilityWindow
                    {
                        Participants = { new ApiScheduling.AvailabilityWindowParticipant
                        {
                            Contact = new ApiCommon.ContactInfo
                            {
                                Name = (item.Contact?.Name ?? new ApiCommon.PersonName()),
                                Phone = (item.Contact?.Phone ?? ""),
                            },
                        } },
                    },
                    Context = (request.Context ?? new ApiCommon.RequestContext()),
                };
                using var rpc0 = _schedulingAvailabilityWindowValidationService.ValidateAvailabilityWindowAsync(input0, cancellationToken: token);
                var reply0 = await rpc0.ResponseAsync;
                return reply0;
            }).ToArray();
            var replies0 = await Task.WhenAll(batch0);
            var reply0 = replies0.LastOrDefault() ?? new ApiScheduling.ValidateAvailabilityWindowResponse();
            var response = new ApiScheduling.BatchCreateShiftsResponse();
            _logger.LogDebug("Local audit detail {Detail}", (request.Record?.Title ?? ""));
            await _repository.RecordAsync(requestContext.TenantId, "shift",
                "audit", "BatchCreateShifts", (request.Record?.Title ?? ""), token);
            result = response;
        }
        return result;
    }
    /// <summary>
    /// Creates a shift.
    /// </summary>
    public override async Task<ApiScheduling.CreateShiftResponse> CreateShift(ApiScheduling.CreateShiftRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CreateShift for {Tenant}", requestContext.TenantId);
        if (request.Shift is { } record)
        {
            var snapshot = ShiftMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiScheduling.ExplainSwapRequestDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SwapRequest = new ApiScheduling.SwapRequest
            {
                Comments = { new ApiScheduling.SwapRequestComment
                {
                    CreatedAt = (request.Shift?.Audit?.CreatedAt ?? new Timestamp()),
                } },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _schedulingSwapRequestValidationService.ExplainSwapRequestDecisionAsync);
        var input1 = new ApiDocuments.SubscribeDocumentFolderUpdatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.SubscribeDocumentFolderUpdatesRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (reply0.SwapRequest?.Requester?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _documentsDocumentFolderNotificationService.SubscribeDocumentFolderUpdatesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiScheduling.CreateShiftResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            Shift = new ApiScheduling.Shift
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply1.DocumentFolder?.Audit?.UpdatedBy ?? ""),
                },
                Organizer = new ApiIdentity.UserRef
                {
                    Email = (reply0.SwapRequest?.Requester?.Email ?? ""),
                },
                ShiftId = (request.Shift?.ShiftId ?? ""),
                Title = (request.Shift?.Title ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Shift?.Organizer?.UserId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "shift",
            "audit", "CreateShift", (request.Shift?.Organizer?.UserId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Deletes a shift.
    /// </summary>
    public override async Task<ApiScheduling.DeleteShiftResponse> DeleteShift(ApiScheduling.DeleteShiftRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ShiftId, "shift_id");
        _logger.LogInformation("Handling DeleteShift for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "shift", request.ShiftId,
            "DeleteShift", "accepted", token);
        var input0 = new ApiScheduling.BrowseRostersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiScheduling.BrowseRostersRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
                Labels = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.ShiftId ?? ""),
            },
        };
        var named0 = _registry.Get<ApiScheduling.RosterQueryService.RosterQueryServiceClient>("acme.scheduling.v1.RosterQueryService");
        using var rpc0 = named0.BrowseRostersAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.UnsubscribeDocumentTemplateUpdatesRequest
        {
            Annotations = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.UnsubscribeDocumentTemplateUpdatesRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            Reason = (request.Reason ?? ""),
        };
        using var rpc1 = _documentsDocumentTemplateNotificationService.UnsubscribeDocumentTemplateUpdatesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiScheduling.DeleteShiftResponse
        {
            EffectiveAt = (reply1.Outcome?.EffectiveAt ?? new Timestamp()),
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Etag ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "shift",
            "audit", "DeleteShift", (request.Etag ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a shift.
    /// </summary>
    public override async Task<ApiScheduling.GetShiftResponse> GetShift(ApiScheduling.GetShiftRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ShiftId, "shift_id");
        _logger.LogInformation("Handling GetShift for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "shift", request.ShiftId,
            "GetShift", "accepted", token);
        var input0 = new ApiScheduling.LintShiftsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Shift = new ApiScheduling.Shift
            {
                Participants = { new ApiScheduling.ShiftParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Context?.Actor?.Email ?? ""),
                    },
                } },
            },
            ShiftId = (request.ShiftId ?? ""),
        };
        var command0 = new DispatchCommand<ApiScheduling.LintShiftsRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _schedulingShiftValidationService.LintShiftsAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiScheduling.GetShiftResponse
        {
            Shift = new ApiScheduling.Shift
            {
                Location = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply0.Shift?.Location?.CountryCode ?? ""),
                },
                ShiftId = (request.ShiftId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists shifts a page at a time.
    /// </summary>
    public override async Task<ApiScheduling.ListShiftsResponse> ListShifts(ApiScheduling.ListShiftsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListShifts for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "shift", "batch",
            "ListShifts", "accepted", token);
        var response = new ApiScheduling.ListShiftsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "shift",
            "audit", "ListShifts", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Lists shifts a page at a time.
    /// </summary>
    public override async Task<ApiScheduling.SearchShiftsResponse> SearchShifts(ApiScheduling.SearchShiftsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SearchShifts for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "shift", "batch",
            "SearchShifts", "accepted", token);
        var response = new ApiScheduling.SearchShiftsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "shift",
            "audit", "SearchShifts", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Changes a shift.
    /// </summary>
    public override async Task<ApiScheduling.UpdateShiftResponse> UpdateShift(ApiScheduling.UpdateShiftRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling UpdateShift for {Tenant}", requestContext.TenantId);
        if (request.Shift is { } record)
        {
            var snapshot = ShiftMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var batch0 = (request.Shift?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiScheduling.ShiftParticipant>()).Select(async item =>
        {
            var input0 = new ApiScheduling.ScoreAvailabilityWindowRequest
            {
                AvailabilityWindow = new ApiScheduling.AvailabilityWindow
                {
                    Participants = { new ApiScheduling.AvailabilityWindowParticipant
                    {
                        Contact = new ApiCommon.ContactInfo
                        {
                            Email = (item.Contact?.Email ?? ""),
                        },
                        Required = item.Required,
                    } },
                },
                Context = (request.Context ?? new ApiCommon.RequestContext()),
            };
            using var rpc0 = _schedulingAvailabilityWindowValidationService.ScoreAvailabilityWindowAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            return reply0;
        }).ToArray();
        var replies0 = await Task.WhenAll(batch0);
        var reply0 = replies0.LastOrDefault() ?? new ApiScheduling.ScoreAvailabilityWindowResponse();
        var response = new ApiScheduling.UpdateShiftResponse
        {
            Shift = new ApiScheduling.Shift
            {
                Status = (request.Shift?.Status ?? (ApiScheduling.ShiftStatus)0),
                Title = (request.Shift?.Title ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Shift?.ShiftId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "shift",
            "audit", "UpdateShift", (request.Shift?.ShiftId ?? ""), token);
        return response;
    }
}
