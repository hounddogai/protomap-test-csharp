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
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Scheduling;

/// <summary>
/// Computes the figures of a swap request, such as totals, taxes, and estimates.
/// </summary>
public sealed class SwapRequestCalculationEndpoint : ApiScheduling.SwapRequestCalculationService.SwapRequestCalculationServiceBase
{
    private readonly NotificationBus _bus;
    private readonly ApiCorporateevents.CorporateEventValidationService.CorporateEventValidationServiceClient _corporateeventsCorporateEventValidationService;
    private readonly ApiDevices.ManagedDeviceService.ManagedDeviceServiceClient _devicesManagedDeviceService;
    private readonly ApiDocuments.SignatureCalculationService.SignatureCalculationServiceClient _documentsSignatureCalculationService;
    private readonly ApiLicensing.EntitlementValidationService.EntitlementValidationServiceClient _licensingEntitlementValidationService;
    private readonly ILogger<SwapRequestCalculationEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly IRecordRepository _repository;
    private readonly ApiScheduling.AvailabilityWindowCalculationService.AvailabilityWindowCalculationServiceClient _schedulingAvailabilityWindowCalculationService;
    private readonly ApiScheduling.RosterCalculationService.RosterCalculationServiceClient _schedulingRosterCalculationService;
    private readonly ApiScheduling.RosterValidationService.RosterValidationServiceClient _schedulingRosterValidationService;
    private readonly ApiScheduling.ShiftValidationService.ShiftValidationServiceClient _schedulingShiftValidationService;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ISwapRequestCalculationEndpointSendCalculateSwapRequest0Port _swapRequestCalculationEndpointSendCalculateSwapRequest0Port;
    private readonly ApiWorkplaces.DeskCalculationService.DeskCalculationServiceClient _workplacesDeskCalculationService;

    public SwapRequestCalculationEndpoint(
        NotificationBus bus,
        ApiCorporateevents.CorporateEventValidationService.CorporateEventValidationServiceClient corporateeventsCorporateEventValidationService,
        ApiDevices.ManagedDeviceService.ManagedDeviceServiceClient devicesManagedDeviceService,
        ApiDocuments.SignatureCalculationService.SignatureCalculationServiceClient documentsSignatureCalculationService,
        ApiLicensing.EntitlementValidationService.EntitlementValidationServiceClient licensingEntitlementValidationService,
        ILogger<SwapRequestCalculationEndpoint> logger,
        RetryPolicy policy,
        IRecordRepository repository,
        ApiScheduling.AvailabilityWindowCalculationService.AvailabilityWindowCalculationServiceClient schedulingAvailabilityWindowCalculationService,
        ApiScheduling.RosterCalculationService.RosterCalculationServiceClient schedulingRosterCalculationService,
        ApiScheduling.RosterValidationService.RosterValidationServiceClient schedulingRosterValidationService,
        ApiScheduling.ShiftValidationService.ShiftValidationServiceClient schedulingShiftValidationService,
        IOptions<MeshOptions> settings,
        ISwapRequestCalculationEndpointSendCalculateSwapRequest0Port swapRequestCalculationEndpointSendCalculateSwapRequest0PortAdapter,
        ApiWorkplaces.DeskCalculationService.DeskCalculationServiceClient workplacesDeskCalculationService)
    {
        _bus = bus;
        _corporateeventsCorporateEventValidationService = corporateeventsCorporateEventValidationService;
        _devicesManagedDeviceService = devicesManagedDeviceService;
        _documentsSignatureCalculationService = documentsSignatureCalculationService;
        _licensingEntitlementValidationService = licensingEntitlementValidationService;
        _logger = logger;
        _policy = policy;
        _repository = repository;
        _schedulingAvailabilityWindowCalculationService = schedulingAvailabilityWindowCalculationService;
        _schedulingRosterCalculationService = schedulingRosterCalculationService;
        _schedulingRosterValidationService = schedulingRosterValidationService;
        _schedulingShiftValidationService = schedulingShiftValidationService;
        _settings = settings;
        _swapRequestCalculationEndpointSendCalculateSwapRequest0Port = swapRequestCalculationEndpointSendCalculateSwapRequest0PortAdapter;
        _workplacesDeskCalculationService = workplacesDeskCalculationService;
    }
    /// <summary>
    /// Calculates a swap request.
    /// </summary>
    public override async Task<ApiScheduling.CalculateSwapRequestResponse> CalculateSwapRequest(ApiScheduling.CalculateSwapRequestRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CalculateSwapRequest for {Tenant}", requestContext.TenantId);
        if (request.SwapRequest is { } record)
        {
            var snapshot = SwapRequestMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.ValidateEntitlementRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.SwapRequest?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.SwapRequest?.Assignee?.UserId ?? ""),
                },
            },
        };
        var reply0 = await SendCalculateSwapRequest0Async(input0, token);
        var response = new ApiScheduling.CalculateSwapRequestResponse
        {
            SwapRequest = new ApiScheduling.SwapRequest
            {
                Comments = { new ApiScheduling.SwapRequestComment
                {
                    CreatedAt = (reply0.Entitlement?.Audit?.CreatedAt ?? new Timestamp()),
                } },
                Priority = (request.SwapRequest?.Priority ?? 0),
                SwapRequestId = (request.SwapRequestId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SwapRequest?.SwapRequestId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "swap_request",
            "audit", "CalculateSwapRequest", (request.SwapRequest?.SwapRequestId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Compares a swap request.
    /// </summary>
    public override async Task<ApiScheduling.CompareSwapRequestOptionsResponse> CompareSwapRequestOptions(ApiScheduling.CompareSwapRequestOptionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CompareSwapRequestOptions for {Tenant}", requestContext.TenantId);
        if (request.SwapRequest is { } record)
        {
            var snapshot = SwapRequestMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiScheduling.QuoteRosterRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiScheduling.QuoteRosterRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Locality = (request.Details?.Destination?.Locality ?? ""),
                    Region = (request.Details?.Destination?.Region ?? ""),
                },
            },
            Roster = new ApiScheduling.Roster
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.SwapRequest?.Audit?.CreatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (request.Details?.Destination?.Recipient ?? ""),
                },
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _schedulingRosterCalculationService.QuoteRosterAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiScheduling.CompareSwapRequestOptionsResponse
        {
            Breakdown = new ApiScheduling.CompareSwapRequestOptionsResponse.Types.Breakdown
            {
                CurrencyCode = (reply0.Breakdown?.CurrencyCode ?? ""),
            },
            SwapRequest = new ApiScheduling.SwapRequest
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Roster?.Counterparty?.DisplayName ?? ""),
                },
                Status = (request.SwapRequest?.Status ?? (ApiScheduling.SwapRequestStatus)0),
                SwapRequestId = (request.SwapRequestId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SwapRequest?.SwapRequestId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "swap_request",
            "audit", "CompareSwapRequestOptions", (request.SwapRequest?.SwapRequestId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Estimates a swap request.
    /// </summary>
    public override async Task<ApiScheduling.EstimateSwapRequestResponse> EstimateSwapRequest(ApiScheduling.EstimateSwapRequestRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling EstimateSwapRequest for {Tenant}", requestContext.TenantId);
        if (request.SwapRequest is { } record)
        {
            var snapshot = SwapRequestMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.NormalizeCorporateEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Organizer = new ApiIdentity.UserRef
                {
                    Email = (request.SwapRequest?.Requester?.Email ?? ""),
                },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiCorporateevents.NormalizeCorporateEventRequest>();
        using var rpc0 = _corporateeventsCorporateEventValidationService.NormalizeCorporateEventAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiScheduling.ScoreShiftRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Shift = new ApiScheduling.Shift
            {
                Participants = { new ApiScheduling.ShiftParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.SwapRequest ?? new ApiScheduling.SwapRequest()).OriginCase switch
                        {
                            ApiScheduling.SwapRequest.OriginOneofCase.Customer => (request.SwapRequest?.Customer?.Email ?? ""),
                            _ => ""
                        },
                    },
                } },
            },
        };
        using var rpc1 = _schedulingShiftValidationService.ScoreShiftAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiWorkplaces.QuoteDeskRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Desk = new ApiWorkplaces.Desk
            {
                Address = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply1.Shift?.Location?.CountryCode ?? ""),
                    Locality = (reply0.CorporateEvent?.Location?.Locality ?? ""),
                    Recipient = (reply1.Shift?.Location?.Recipient ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.CorporateEvent?.Audit?.CreatedBy ?? ""),
                },
            },
        };
        using var rpc2 = _workplacesDeskCalculationService.QuoteDeskAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        var input3 = new ApiDevices.UpdateManagedDeviceRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Address = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Details?.Destination?.CountryCode ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.Shift?.Audit?.Etag ?? ""),
                },
            },
        };
        using var rpc3 = _devicesManagedDeviceService.UpdateManagedDeviceAsync(input3, cancellationToken: token);
        var pending3 = rpc3.ResponseAsync;
        await Task.WhenAll(pending2, pending3);
        var reply2 = await pending2;
        var reply3 = await pending3;
        var response = new ApiScheduling.EstimateSwapRequestResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply2.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            SwapRequest = new ApiScheduling.SwapRequest
            {
                Assignee = new ApiIdentity.UserRef
                {
                    UserId = (reply3.ManagedDevice?.Owner?.UserId ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply3.ManagedDevice?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedAt = (reply1.Shift?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Priority = (request.SwapRequest?.Priority ?? 0),
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (reply0.CorporateEvent?.Organizer?.Email ?? ""),
                },
                SwapRequestId = (request.SwapRequestId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SwapRequest?.SwapRequestId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "swap_request",
            "audit", "EstimateSwapRequest", (request.SwapRequest?.SwapRequestId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a swap request.
    /// </summary>
    public override async Task<ApiScheduling.ExplainSwapRequestCalculationResponse> ExplainSwapRequestCalculation(ApiScheduling.ExplainSwapRequestCalculationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ExplainSwapRequestCalculation for {Tenant}", requestContext.TenantId);
        if (request.SwapRequest is { } record)
        {
            var snapshot = SwapRequestMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiScheduling.LintShiftsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Shift = new ApiScheduling.Shift
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.SwapRequest?.Audit?.CreatedBy ?? ""),
                },
                Organizer = new ApiIdentity.UserRef
                {
                    Email = (request.SwapRequest ?? new ApiScheduling.SwapRequest()).OriginCase switch
                    {
                        ApiScheduling.SwapRequest.OriginOneofCase.Customer => (request.SwapRequest?.Customer?.Email ?? ""),
                        _ => ""
                    },
                },
                Participants = { new ApiScheduling.ShiftParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Details?.Contact?.Email ?? ""),
                        PreferredChannel = (request.SwapRequest?.Requester?.PreferredChannel ?? ""),
                    },
                } },
            },
        };
        using var rpc0 = _schedulingShiftValidationService.LintShiftsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiScheduling.ExplainSwapRequestCalculationResponse
        {
            Outcome = new ApiScheduling.ExplainSwapRequestCalculationResponse.Types.Outcome
            {
                NextSteps = { (reply0.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            SwapRequest = new ApiScheduling.SwapRequest
            {
                Subject = (request.SwapRequest?.Subject ?? ""),
                SwapRequestId = (request.SwapRequestId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SwapRequest?.SwapRequestId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "swap_request",
            "audit", "ExplainSwapRequestCalculation", (request.SwapRequest?.SwapRequestId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a swap request.
    /// </summary>
    public override async Task<ApiScheduling.GetSwapRequestRatesResponse> GetSwapRequestRates(ApiScheduling.GetSwapRequestRatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.SwapRequestId, "swap_request_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetSwapRequestRates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "swap_request", request.SwapRequestId,
            "GetSwapRequestRates", "accepted", token);
        var input0 = new ApiScheduling.LintRostersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiScheduling.LintRostersRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _schedulingRosterValidationService.LintRostersAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiScheduling.GetSwapRequestRatesResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Outcome?.Status ?? ""),
            },
            SwapRequest = new ApiScheduling.SwapRequest
            {
                Sla = new ApiScheduling.SwapRequestSla
                {
                    DueAt = (reply0.Roster?.DueAt ?? new Timestamp()),
                },
                SwapRequestId = (request.SwapRequestId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Quotes a swap request.
    /// </summary>
    public override async Task<ApiScheduling.QuoteSwapRequestResponse> QuoteSwapRequest(ApiScheduling.QuoteSwapRequestRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling QuoteSwapRequest for {Tenant}", requestContext.TenantId);
        if (request.SwapRequest is { } record)
        {
            var snapshot = SwapRequestMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiScheduling.SimulateRosterRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Roster = new ApiScheduling.Roster
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.SwapRequest?.Audit?.UpdatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Details?.Destination?.PostalCode ?? ""),
                    Recipient = (request.Details?.Destination?.Recipient ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.SwapRequest?.Requester?.Email ?? ""),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _schedulingRosterCalculationService.SimulateRosterAsync);
        await _bus.PublishAsync(new TransportNotification<ApiScheduling.SimulateRosterRequest>("acme.scheduling.v1.SwapRequestCalculationService/QuoteSwapRequest/0", input0.Clone()), token);
        var input1 = new ApiDocuments.RecalculateSignaturesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.RecalculateSignaturesRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Details?.Destination?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    PostalCode = (request.Details?.Destination?.PostalCode ?? ""),
                    Region = (reply0.Roster?.BillingAddress?.Region ?? ""),
                },
            },
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
        };
        using var rpc1 = _documentsSignatureCalculationService.RecalculateSignaturesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var reply2 = new ApiScheduling.ValidateRosterResponse();
        foreach (var item in (request.SwapRequest?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiScheduling.SwapRequestComment>()))
        {
            var input2 = new ApiScheduling.ValidateRosterRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiScheduling.ValidateRosterRequest.Types.Details
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (item.AuthorEmail ?? ""),
                    },
                },
                Roster = new ApiScheduling.Roster
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedAt = (item.CreatedAt ?? new Timestamp()),
                    },
                    Bank = new ApiScheduling.RosterBankSettlement
                    {
                        Bic = (reply0.Roster?.Bank?.Bic ?? ""),
                    },
                    BillingAddress = new ApiCommon.PostalAddress
                    {
                        Lines = { (reply0.Roster?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    },
                },
            };
            using var rpc2 = _schedulingRosterValidationService.ValidateRosterAsync(input2, cancellationToken: token);
            reply2 = await rpc2.ResponseAsync;
        }
        var input3 = new ApiScheduling.ExplainRosterDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiScheduling.ExplainRosterDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.SwapRequest ?? new ApiScheduling.SwapRequest()).OriginCase switch
                    {
                        ApiScheduling.SwapRequest.OriginOneofCase.Customer => (request.SwapRequest?.Customer?.Email ?? ""),
                        _ => ""
                    },
                },
            },
            Roster = new ApiScheduling.Roster
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.SwapRequest?.Audit?.Etag ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (reply2.Roster?.BillingAddress?.Region ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (reply0.Roster?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        using var rpc3 = _schedulingRosterValidationService.ExplainRosterDecisionAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiScheduling.QuoteSwapRequestResponse
        {
            Breakdown = new ApiScheduling.QuoteSwapRequestResponse.Types.Breakdown
            {
                Total = (reply1.Breakdown?.Total ?? new ApiCommon.Money()),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply3.Outcome?.Message ?? ""),
            },
            SwapRequest = new ApiScheduling.SwapRequest
            {
                Assignee = (request.SwapRequest?.Assignee ?? new ApiIdentity.UserRef()),
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.Roster?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                SwapRequestId = (request.SwapRequestId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SwapRequest?.SwapRequestId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "swap_request",
            "audit", "QuoteSwapRequest", (request.SwapRequest?.SwapRequestId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalculates a swap request.
    /// </summary>
    public override async Task<ApiScheduling.RecalculateSwapRequestsResponse> RecalculateSwapRequests(ApiScheduling.RecalculateSwapRequestsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RecalculateSwapRequests for {Tenant}", requestContext.TenantId);
        if (request.SwapRequest is { } record)
        {
            var snapshot = SwapRequestMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiScheduling.CompareAvailabilityWindowOptionsRequest
        {
            AvailabilityWindow = new ApiScheduling.AvailabilityWindow
            {
                Location = new ApiCommon.PostalAddress
                {
                    Recipient = (request.Details?.Destination?.Recipient ?? ""),
                },
                Participants = { new ApiScheduling.AvailabilityWindowParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.SwapRequest?.Assignee?.Email ?? ""),
                    },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var command0 = new DispatchCommand<ApiScheduling.CompareAvailabilityWindowOptionsRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _schedulingAvailabilityWindowCalculationService.CompareAvailabilityWindowOptionsAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var profile1 = new MappingProfile<ApiScheduling.RecalculateSwapRequestsRequest, ApiScheduling.LintRostersRequest>(value => new ApiScheduling.LintRostersRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Roster = new ApiScheduling.Roster
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (value.Details?.Destination?.CountryCode ?? ""),
                    PostalCode = (reply0.AvailabilityWindow?.Location?.PostalCode ?? ""),
                },
            },
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _schedulingRosterValidationService.LintRostersAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiScheduling.EstimateRosterRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiScheduling.EstimateRosterRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Locality = (reply1.Roster?.BillingAddress?.Locality ?? ""),
                },
            },
            Roster = new ApiScheduling.Roster
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.SwapRequest?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (reply1.Roster?.Counterparty?.Email ?? ""),
                },
            },
        };
        input2 = PropertyMap.Copy(input2);
        using var rpc2 = _schedulingRosterCalculationService.EstimateRosterAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = request.RecalculateSwapRequestsToRecalculateRosters3(reply2);
        using var rpc3 = _schedulingRosterCalculationService.RecalculateRostersAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiScheduling.RecalculateSwapRequestsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Message = (reply2.Result?.Message ?? ""),
                Status = (reply3.Result?.Status ?? ""),
            },
            SwapRequest = new ApiScheduling.SwapRequest
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.Roster?.Counterparty?.DisplayName ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.AvailabilityWindow?.Audit?.CreatedAt ?? new Timestamp()),
                },
                CustomFields = { (request.SwapRequest?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                SwapRequestId = (request.SwapRequestId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SwapRequest?.SwapRequestId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "swap_request",
            "audit", "RecalculateSwapRequests", (request.SwapRequest?.SwapRequestId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Simulates a swap request.
    /// </summary>
    public override async Task<ApiScheduling.SimulateSwapRequestResponse> SimulateSwapRequest(ApiScheduling.SimulateSwapRequestRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SimulateSwapRequest for {Tenant}", requestContext.TenantId);
        if (request.SwapRequest is { } record)
        {
            var snapshot = SwapRequestMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiScheduling.ExplainRosterCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Roster = new ApiScheduling.Roster
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.SwapRequest?.Audit?.UpdatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Details?.Destination?.CountryCode ?? ""),
                    Lines = { (request.Details?.Destination?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.SwapRequest ?? new ApiScheduling.SwapRequest()).OriginCase switch
                    {
                        ApiScheduling.SwapRequest.OriginOneofCase.Customer => (request.SwapRequest?.Customer?.Email ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc0 = _schedulingRosterCalculationService.ExplainRosterCalculationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiScheduling.LintShiftsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Shift = new ApiScheduling.Shift
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.SwapRequest?.Audit?.CreatedBy ?? ""),
                },
                Participants = { new ApiScheduling.ShiftParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.SwapRequest?.Requester?.Email ?? ""),
                        PreferredChannel = (request.SwapRequest?.Requester?.PreferredChannel ?? ""),
                    },
                } },
            },
        };
        var labels1 = input1.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input1.Context.Baggage.Clear();
        input1.Context.Baggage.Add(labels1);
        using var rpc1 = _schedulingShiftValidationService.LintShiftsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiScheduling.SimulateSwapRequestResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            SwapRequest = new ApiScheduling.SwapRequest
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.Shift?.Organizer?.DisplayName ?? ""),
                },
                CustomFields = { (request.SwapRequest?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                SwapRequestId = (request.SwapRequestId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SwapRequest?.SwapRequestId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "swap_request",
            "audit", "SimulateSwapRequest", (request.SwapRequest?.SwapRequestId ?? ""), token);
        return response;
    }
    private async Task<ApiLicensing.ValidateEntitlementResponse> SendCalculateSwapRequest0Async(ApiLicensing.ValidateEntitlementRequest request, CancellationToken token)
    {
        return await _swapRequestCalculationEndpointSendCalculateSwapRequest0Port.SendAsync(request, token);
    }
}
