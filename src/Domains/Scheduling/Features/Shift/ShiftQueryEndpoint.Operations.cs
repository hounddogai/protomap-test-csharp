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
using ApiCareplans = Acme.Careplans.V1;
using ApiCommon = Acme.Common.V1;
using ApiDispatch = Acme.Dispatch.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiScheduling = Acme.Scheduling.V1;

namespace Acme.Workforce.Scheduling;

public sealed partial class ShiftQueryEndpoint
{
    /// <summary>
    /// Lists shifts a page at a time.
    /// </summary>
    public override async Task<ApiScheduling.BrowseShiftsResponse> BrowseShifts(ApiScheduling.BrowseShiftsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling BrowseShifts for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "shift", "batch",
            "BrowseShifts", "accepted", token);
        var input0 = new ApiCareplans.RecalculateCarePlansRequest
        {
            CarePlan = new ApiCareplans.CarePlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var descriptor0 = new Method<ApiCareplans.RecalculateCarePlansRequest, ApiCareplans.RecalculateCarePlansResponse>(
            MethodType.Unary, "acme.careplans.v1" + ".CarePlanCalculationService", "RecalculateCarePlans",
            Wire.Marshaller<ApiCareplans.RecalculateCarePlansRequest>(), Wire.Marshaller<ApiCareplans.RecalculateCarePlansResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.AnnotateDocumentVersionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.AnnotateDocumentVersionRequest.Types.Details
            {
                Snapshot = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Document = new ApiDocuments.Document
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.CarePlan?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (reply0.CarePlan?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Card = new ApiDocuments.DocumentCardSettlement
                {
                    HolderName = (reply0.CarePlan ?? new ApiCareplans.CarePlan()).PaymentCase switch
                    {
                        ApiCareplans.CarePlan.SettlementOneofCase.Card => (reply0.CarePlan?.Card?.HolderName ?? ""),
                        _ => ""
                    },
                    Last4 = (reply0.CarePlan ?? new ApiCareplans.CarePlan()).PaymentCase switch
                    {
                        ApiCareplans.CarePlan.SettlementOneofCase.Card => (reply0.CarePlan?.Card?.Last4 ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _documentsDocumentHistoryService.AnnotateDocumentVersionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiScheduling.BrowseShiftsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Status = (reply1.Outcome?.Status ?? ""),
            },
            Shifts = { new ApiScheduling.Shift
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.Document?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Location = new ApiCommon.PostalAddress
                {
                    Locality = (reply0.CarePlan?.BillingAddress?.Locality ?? ""),
                },
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "shift",
            "audit", "BrowseShifts", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Counts shifts by status.
    /// </summary>
    public override async Task<ApiScheduling.CountShiftsResponse> CountShifts(ApiScheduling.CountShiftsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CountShifts for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "shift", "batch",
            "CountShifts", "accepted", token);
        var profile0 = new MappingProfile<ApiScheduling.CountShiftsRequest, ApiScheduling.LintAvailabilityWindowsRequest>(value => new ApiScheduling.LintAvailabilityWindowsRequest
        {
            AvailabilityWindow = new ApiScheduling.AvailabilityWindow
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (value.Filter?.CreatedAfter ?? new Timestamp()),
                },
                Location = new ApiCommon.PostalAddress
                {
                    Region = (value.Filter?.Region ?? ""),
                },
                Participants = { new ApiScheduling.AvailabilityWindowParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (value.Context?.Actor?.Email ?? ""),
                    },
                } },
            },
            Context = (value.Context ?? new ApiCommon.RequestContext()),
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _schedulingAvailabilityWindowValidationService.LintAvailabilityWindowsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiScheduling.CountShiftsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "shift",
            "audit", "CountShifts", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams every matching shift.
    /// </summary>
    public override async Task ExportShifts(ApiScheduling.ExportShiftsRequest request, IServerStreamWriter<ApiScheduling.ExportShiftsResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExportShifts for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "shift", "batch",
            "ExportShifts", "accepted", token);
        var input0 = new ApiAudit.ValidateAuditEventRequest
        {
            AuditEvent = new ApiAudit.AuditEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiAudit.ValidateAuditEventRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _auditAuditEventValidationService.ValidateAuditEventAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = request.ExportShiftsToSimulateSwapRequest1(reply0);
        using var rpc1 = _schedulingSwapRequestCalculationService.SimulateSwapRequestAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiScheduling.CompareSwapRequestOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SwapRequest = new ApiScheduling.SwapRequest
            {
                Channel = new ApiScheduling.SwapRequestChannelOrigin
                {
                    Channel = (reply1.SwapRequest ?? new ApiScheduling.SwapRequest()).OriginCase switch
                    {
                        ApiScheduling.SwapRequest.OriginOneofCase.Channel => (reply1.SwapRequest?.Channel?.Channel ?? ""),
                        _ => ""
                    },
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Phone = (reply1.SwapRequest?.Requester?.Phone ?? ""),
                },
                Sla = new ApiScheduling.SwapRequestSla
                {
                    PausedFor = (reply1.SwapRequest?.Sla?.PausedFor ?? new Duration()),
                },
            },
        };
        using var rpc2 = _schedulingSwapRequestCalculationService.CompareSwapRequestOptionsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiDocuments.SendSignatureTestNoticeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.SendSignatureTestNoticeRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (reply1.SwapRequest?.Assignee?.Email ?? ""),
                    Name = (reply2.SwapRequest?.Requester?.Name ?? new ApiCommon.PersonName()),
                    Phone = (reply1.SwapRequest?.Requester?.Phone ?? ""),
                    PreferredChannel = (reply2.SwapRequest?.Requester?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc3 = _documentsSignatureNotificationService.SendSignatureTestNoticeAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiScheduling.ExportShiftsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.AuditEvent?.CorrelationId ?? ""),
            },
            Sequence = (reply3.Signature?.Sequence ?? 0L),
            Shift = new ApiScheduling.Shift
            {
                Location = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
                Participants = { new ApiScheduling.ShiftParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Phone = (reply2.SwapRequest?.Requester?.Phone ?? ""),
                        PreferredChannel = (reply1.SwapRequest?.Requester?.PreferredChannel ?? ""),
                    },
                } },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Format ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "shift",
            "audit", "ExportShifts", (request.Format ?? ""), token);
        await responses.WriteAsync(response, token);
    }
    /// <summary>
    /// Lists shifts a page at a time.
    /// </summary>
    public override async Task<ApiScheduling.FindShiftsResponse> FindShifts(ApiScheduling.FindShiftsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling FindShifts for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "shift", "batch",
            "FindShifts", "accepted", token);
        var response = new ApiScheduling.FindShiftsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "shift",
            "audit", "FindShifts", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a shift.
    /// </summary>
    public override async Task<ApiScheduling.LookupShiftResponse> LookupShift(ApiScheduling.LookupShiftRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ShiftId, "shift_id");
        _logger.LogInformation("Handling LookupShift for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "shift", request.ShiftId,
            "LookupShift", "accepted", token);
        var response = new ApiScheduling.LookupShiftResponse
        {
            Shift = new ApiScheduling.Shift
            {
                ShiftId = (request.ShiftId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Returns a shift.
    /// </summary>
    public override async Task<ApiScheduling.PeekShiftResponse> PeekShift(ApiScheduling.PeekShiftRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ShiftId, "shift_id");
        _logger.LogInformation("Handling PeekShift for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "shift", request.ShiftId,
            "PeekShift", "accepted", token);
        var input0 = new ApiItsm.RecalculateIncidentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Incident = new ApiItsm.Incident
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
                Subject = (request.ShiftId ?? ""),
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiItsm.RecalculateIncidentsRequest>();
        using var rpc0 = _itsmIncidentCalculationService.RecalculateIncidentsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiScheduling.PeekShiftResponse
        {
            Shift = new ApiScheduling.Shift
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.Incident?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (reply0.Incident?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                ShiftId = (request.ShiftId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Returns several shifts at once.
    /// </summary>
    public override async Task<ApiScheduling.ResolveShiftsResponse> ResolveShifts(ApiScheduling.ResolveShiftsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ResolveShifts for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "shift", "batch",
            "ResolveShifts", "accepted", token);
        var response = new ApiScheduling.ResolveShiftsResponse();
        return response;
    }
    /// <summary>
    /// Lists shifts a page at a time.
    /// </summary>
    public override async Task<ApiScheduling.SuggestShiftsResponse> SuggestShifts(ApiScheduling.SuggestShiftsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SuggestShifts for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "shift", "batch",
            "SuggestShifts", "accepted", token);
        var response = new ApiScheduling.SuggestShiftsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "shift",
            "audit", "SuggestShifts", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Summarizes a shift.
    /// </summary>
    public override async Task<ApiScheduling.SummarizeShiftsResponse> SummarizeShifts(ApiScheduling.SummarizeShiftsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SummarizeShifts for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "shift", request.ShiftId,
            "SummarizeShifts", "accepted", token);
        var input0 = new ApiAudit.SearchAuditEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiAudit.SearchAuditEventsRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
                Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.ShiftId ?? ""),
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiAudit.SearchAuditEventsRequest>(json0);
        using var rpc0 = _auditAuditEventService.SearchAuditEventsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiMetrics.CountMetricSeriesesRequest();
        input1.Context = (request.Context ?? new ApiCommon.RequestContext());
        input1.Filter = new ApiMetrics.CountMetricSeriesesRequest.Types.Filter
        {
            Labels = { (request.Details?.Filters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
        };
        using var rpc1 = _metricsMetricSeriesQueryService.CountMetricSeriesesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiScheduling.ExplainAvailabilityWindowDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiScheduling.ExplainAvailabilityWindowDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        using var rpc2 = _schedulingAvailabilityWindowValidationService.ExplainAvailabilityWindowDecisionAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiDispatch.BrowseDispatchOrdersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDispatch.BrowseDispatchOrdersRequest.Types.Filter
            {
                CreatedAfter = (reply2.Outcome?.EffectiveAt ?? new Timestamp()),
                Region = (reply2.AvailabilityWindow?.Location?.Region ?? ""),
            },
        };
        var reply3 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _dispatchDispatchOrderQueryService.BrowseDispatchOrdersAsync(input3, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiScheduling.SummarizeShiftsResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply2.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply1.Result?.Message ?? ""),
                ReferenceIds = { (reply2.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "shift",
            "audit", "SummarizeShifts", (request.Reason ?? ""), token);
        return response;
    }
}
