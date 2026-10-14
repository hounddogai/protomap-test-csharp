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
using ApiAudiences = Acme.Audiences.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiScheduling = Acme.Scheduling.V1;

namespace Acme.Workforce.Scheduling;

/// <summary>
/// Answers read-only questions about rosters, for screens and reports.
/// </summary>
public sealed class RosterQueryEndpoint : ApiScheduling.RosterQueryService.RosterQueryServiceBase
{
    private readonly ApiAudiences.AudienceQueryService.AudienceQueryServiceClient _audiencesAudienceQueryService;
    private readonly ApiCorporateevents.CorporateEventQueryService.CorporateEventQueryServiceClient _corporateeventsCorporateEventQueryService;
    private readonly CallInvoker _invoker;
    private readonly ApiLedger.LedgerAccountValidationService.LedgerAccountValidationServiceClient _ledgerLedgerAccountValidationService;
    private readonly ILogger<RosterQueryEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly IRosterQueryEndpointSendSuggestRosters1Port _rosterQueryEndpointSendSuggestRosters1Port;
    private readonly ApiScheduling.RosterCalculationService.RosterCalculationServiceClient _schedulingRosterCalculationService;
    private readonly ApiScheduling.RosterValidationService.RosterValidationServiceClient _schedulingRosterValidationService;
    private readonly ApiScheduling.ShiftQueryService.ShiftQueryServiceClient _schedulingShiftQueryService;
    private readonly ApiScheduling.ShiftValidationService.ShiftValidationServiceClient _schedulingShiftValidationService;
    private readonly ApiScheduling.SwapRequestValidationService.SwapRequestValidationServiceClient _schedulingSwapRequestValidationService;
    private readonly Func<ApiScheduling.CheckRosterEligibilityRequest, CallOptions, AsyncUnaryCall<ApiScheduling.CheckRosterEligibilityResponse>> _sendExportRosters0;
    private readonly IOptions<MeshOptions> _settings;

    public RosterQueryEndpoint(
        ApiAudiences.AudienceQueryService.AudienceQueryServiceClient audiencesAudienceQueryService,
        ApiCorporateevents.CorporateEventQueryService.CorporateEventQueryServiceClient corporateeventsCorporateEventQueryService,
        CallInvoker invoker,
        ApiLedger.LedgerAccountValidationService.LedgerAccountValidationServiceClient ledgerLedgerAccountValidationService,
        ILogger<RosterQueryEndpoint> logger,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository,
        IRosterQueryEndpointSendSuggestRosters1Port rosterQueryEndpointSendSuggestRosters1PortAdapter,
        ApiScheduling.RosterCalculationService.RosterCalculationServiceClient schedulingRosterCalculationService,
        ApiScheduling.RosterValidationService.RosterValidationServiceClient schedulingRosterValidationService,
        ApiScheduling.ShiftQueryService.ShiftQueryServiceClient schedulingShiftQueryService,
        ApiScheduling.ShiftValidationService.ShiftValidationServiceClient schedulingShiftValidationService,
        ApiScheduling.SwapRequestValidationService.SwapRequestValidationServiceClient schedulingSwapRequestValidationService,
        IOptions<MeshOptions> settings)
    {
        _audiencesAudienceQueryService = audiencesAudienceQueryService;
        _corporateeventsCorporateEventQueryService = corporateeventsCorporateEventQueryService;
        _invoker = invoker;
        _ledgerLedgerAccountValidationService = ledgerLedgerAccountValidationService;
        _logger = logger;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _rosterQueryEndpointSendSuggestRosters1Port = rosterQueryEndpointSendSuggestRosters1PortAdapter;
        _schedulingRosterCalculationService = schedulingRosterCalculationService;
        _schedulingRosterValidationService = schedulingRosterValidationService;
        _schedulingShiftQueryService = schedulingShiftQueryService;
        _schedulingShiftValidationService = schedulingShiftValidationService;
        _schedulingSwapRequestValidationService = schedulingSwapRequestValidationService;
        _settings = settings;
        _sendExportRosters0 = _schedulingRosterValidationService.CheckRosterEligibilityAsync;
    }
    /// <summary>
    /// Lists rosters a page at a time.
    /// </summary>
    public override async Task<ApiScheduling.BrowseRostersResponse> BrowseRosters(ApiScheduling.BrowseRostersRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling BrowseRosters for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "roster", "batch",
            "BrowseRosters", "accepted", token);
        var input0 = new ApiScheduling.LintShiftsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Shift = new ApiScheduling.Shift
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                },
                Location = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
                Organizer = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
                Participants = { new ApiScheduling.ShiftParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Context?.Actor?.Email ?? ""),
                    },
                } },
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _schedulingShiftValidationService.LintShiftsAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiScheduling.BrowseRostersResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "roster",
            "audit", "BrowseRosters", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Counts rosters by status.
    /// </summary>
    public override async Task<ApiScheduling.CountRostersResponse> CountRosters(ApiScheduling.CountRostersRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CountRosters for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "roster", "batch",
            "CountRosters", "accepted", token);
        var input0 = new ApiScheduling.ValidateRosterRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiScheduling.ValidateRosterRequest.Types.Details
            {
                Locale = (request.Context?.Locale ?? ""),
            },
            Roster = new ApiScheduling.Roster
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _schedulingRosterValidationService.ValidateRosterAsync);
        var response = new ApiScheduling.CountRostersResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "roster",
            "audit", "CountRosters", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams every matching roster.
    /// </summary>
    public override async Task ExportRosters(ApiScheduling.ExportRostersRequest request, IServerStreamWriter<ApiScheduling.ExportRostersResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ExportRosters for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "roster", "batch",
            "ExportRosters", "accepted", token);
        var input0 = new ApiScheduling.CheckRosterEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiScheduling.CheckRosterEligibilityRequest.Types.Details
            {
                Locale = (request.Context?.Locale ?? ""),
            },
            Roster = new ApiScheduling.Roster
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _sendExportRosters0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiScheduling.PreviewRosterRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiScheduling.PreviewRosterRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply0.Roster?.Counterparty?.Email ?? ""),
                },
            },
        };
        var named1 = _registry.Get<ApiScheduling.RosterValidationService.RosterValidationServiceClient>("acme.scheduling.v1.RosterValidationService");
        using var rpc1 = named1.PreviewRosterAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiScheduling.ExportRostersResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
            },
            Roster = new ApiScheduling.Roster
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (reply0.Roster?.Counterparty?.Email ?? ""),
                },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Format ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "roster",
            "audit", "ExportRosters", (request.Format ?? ""), token);
        await responses.WriteAsync(response, token);
    }
    /// <summary>
    /// Lists rosters a page at a time.
    /// </summary>
    public override async Task<ApiScheduling.FindRostersResponse> FindRosters(ApiScheduling.FindRostersRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling FindRosters for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "roster", "batch",
            "FindRosters", "accepted", token);
        var input0 = new ApiScheduling.NormalizeRosterRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiScheduling.NormalizeRosterRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Locale = (request.Context?.Locale ?? ""),
            },
            Roster = new ApiScheduling.Roster
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        var descriptor0 = new Method<ApiScheduling.NormalizeRosterRequest, ApiScheduling.NormalizeRosterResponse>(
            MethodType.Unary, "acme.scheduling.v1" + ".RosterValidationService", "NormalizeRoster",
            Wire.Marshaller<ApiScheduling.NormalizeRosterRequest>(), Wire.Marshaller<ApiScheduling.NormalizeRosterResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiScheduling.FindRostersResponse
        {
            Rosters = { new ApiScheduling.Roster
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Roster?.Audit?.CreatedAt ?? new Timestamp()),
                },
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "roster",
            "audit", "FindRosters", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a roster.
    /// </summary>
    public override async Task<ApiScheduling.LookupRosterResponse> LookupRoster(ApiScheduling.LookupRosterRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.RosterId, "roster_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling LookupRoster for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "roster", request.RosterId,
            "LookupRoster", "accepted", token);
        var input0 = new ApiAudiences.CountAudiencesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiAudiences.CountAudiencesRequest.Types.Filter
            {
                Labels = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.RosterId ?? ""),
            },
        };
        var command0 = new DispatchCommand<ApiAudiences.CountAudiencesRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _audiencesAudienceQueryService.CountAudiencesAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var profile1 = new MappingProfile<ApiScheduling.LookupRosterRequest, ApiCorporateevents.FindCorporateEventsRequest>(value => new ApiCorporateevents.FindCorporateEventsRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiCorporateevents.FindCorporateEventsRequest.Types.Filter
            {
                CreatedAfter = (value.Context?.Deadline ?? new Timestamp()),
                Labels = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply0.Result?.CorrelationId ?? ""),
            },
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _corporateeventsCorporateEventQueryService.FindCorporateEventsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiScheduling.LookupRosterResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
            Roster = new ApiScheduling.Roster
            {
                RosterId = (request.RosterId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Returns several rosters at once.
    /// </summary>
    public override async Task<ApiScheduling.ResolveRostersResponse> ResolveRosters(ApiScheduling.ResolveRostersRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ResolveRosters for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "roster", "batch",
            "ResolveRosters", "accepted", token);
        var input0 = new ApiScheduling.PreviewShiftRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiScheduling.PreviewShiftRequest.Types.Details
            {
                Locale = (request.Context?.Locale ?? ""),
            },
            Shift = new ApiScheduling.Shift
            {
                Organizer = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
                Participants = { new ApiScheduling.ShiftParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Context?.Actor?.Email ?? ""),
                    },
                } },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _schedulingShiftValidationService.PreviewShiftAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiScheduling.ResolveRostersResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Outcome?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        return response;
    }
    /// <summary>
    /// Lists rosters a page at a time.
    /// </summary>
    public override async Task<ApiScheduling.SuggestRostersResponse> SuggestRosters(ApiScheduling.SuggestRostersRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SuggestRosters for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "roster", "batch",
            "SuggestRosters", "accepted", token);
        var input0 = new ApiScheduling.ExportShiftsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiScheduling.ExportShiftsRequest.Types.Filter
            {
                OwnerId = (request.Filter?.OwnerId ?? ""),
                Query = (request.Filter?.Query ?? ""),
            },
        };
        using var stream0 = _schedulingShiftQueryService.ExportShifts(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportShifts frame {Type}", change0.GetType().Name);
        }
        var input1 = new ApiScheduling.ExplainSwapRequestDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiScheduling.ExplainSwapRequestDecisionRequest.Types.Details
            {
                Locale = (request.Context?.Locale ?? ""),
            },
            SwapRequest = new ApiScheduling.SwapRequest
            {
                Assignee = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
                History = { new ApiScheduling.SwapRequestStatusChange
                {
                    ChangedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                } },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        var reply1 = await SendSuggestRosters1Async(input1, token);
        var response = new ApiScheduling.SuggestRostersResponse
        {
            Rosters = { new ApiScheduling.Roster
            {
                DueAt = (reply1.SwapRequest?.Sla?.DueAt ?? new Timestamp()),
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "roster",
            "audit", "SuggestRosters", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Summarizes a roster.
    /// </summary>
    public override async Task<ApiScheduling.SummarizeRostersResponse> SummarizeRosters(ApiScheduling.SummarizeRostersRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SummarizeRosters for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "roster", request.RosterId,
            "SummarizeRosters", "accepted", token);
        var input0 = new ApiLedger.PreviewLedgerAccountRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLedger.PreviewLedgerAccountRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Locale = (request.Context?.Locale ?? ""),
            },
            Reason = (request.Reason ?? ""),
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _ledgerLedgerAccountValidationService.PreviewLedgerAccountAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiScheduling.GetRosterRatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            RosterId = (request.RosterId ?? ""),
        };
        var packed1 = Any.Pack(input1);
        input1 = packed1.Unpack<ApiScheduling.GetRosterRatesRequest>();
        using var rpc1 = _schedulingRosterCalculationService.GetRosterRatesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiScheduling.ExplainShiftDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Shift = new ApiScheduling.Shift
            {
                Participants = { new ApiScheduling.ShiftParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (reply0.LedgerAccount?.Contact?.Email ?? ""),
                        Phone = (reply0.LedgerAccount?.Contact?.Phone ?? ""),
                    },
                } },
            },
        };
        var json2 = JsonFormatter.Default.Format(input2);
        input2 = JsonParser.Default.Parse<ApiScheduling.ExplainShiftDecisionRequest>(json2);
        using var rpc2 = _schedulingShiftValidationService.ExplainShiftDecisionAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiScheduling.SummarizeRostersResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Outcome?.Message ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.Format ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "roster",
            "audit", "SummarizeRosters", (request.Details?.Format ?? ""), token);
        return response;
    }
    private async Task<ApiScheduling.ExplainSwapRequestDecisionResponse> SendSuggestRosters1Async(ApiScheduling.ExplainSwapRequestDecisionRequest request, CancellationToken token)
    {
        return await _rosterQueryEndpointSendSuggestRosters1Port.SendAsync(request, token);
    }
}
