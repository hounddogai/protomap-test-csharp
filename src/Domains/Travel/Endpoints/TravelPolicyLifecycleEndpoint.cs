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
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

/// <summary>
/// Moves a travel policy between the states of its lifecycle.
/// </summary>
public sealed class TravelPolicyLifecycleEndpoint : ApiTravel.TravelPolicyLifecycleService.TravelPolicyLifecycleServiceBase
{
    private readonly ApiCorporateevents.CorporateEventReportingService.CorporateEventReportingServiceClient _corporateeventsCorporateEventReportingService;
    private readonly ApiDevices.DevicePolicyWorkflowService.DevicePolicyWorkflowServiceClient _devicesDevicePolicyWorkflowService;
    private readonly ApiDocuments.DocumentNotificationService.DocumentNotificationServiceClient _documentsDocumentNotificationService;
    private readonly ApiFeatureflags.FeatureFlagQueryService.FeatureFlagQueryServiceClient _featureflagsFeatureFlagQueryService;
    private readonly ApiHelpdesk.TicketApprovalService.TicketApprovalServiceClient _helpdeskTicketApprovalService;
    private readonly ApiLedger.LedgerAccountQueryService.LedgerAccountQueryServiceClient _ledgerLedgerAccountQueryService;
    private readonly ApiLicensing.LicenseAssignmentReportingService.LicenseAssignmentReportingServiceClient _licensingLicenseAssignmentReportingService;
    private readonly ILogger<TravelPolicyLifecycleEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiFeatureflags.BrowseFeatureFlagsRequest, CallOptions, AsyncUnaryCall<ApiFeatureflags.BrowseFeatureFlagsResponse>> _sendSuspendTravelPolicy1;
    private readonly ApiTravel.ItineraryCalculationService.ItineraryCalculationServiceClient _travelItineraryCalculationService;
    private readonly ApiTravel.ItineraryReportingService.ItineraryReportingServiceClient _travelItineraryReportingService;
    private readonly ITravelPolicyLifecycleEndpointSendActivateTravelPolicy3Port _travelPolicyLifecycleEndpointSendActivateTravelPolicy3Port;
    private readonly ApiTravel.TravelPolicyReportingService.TravelPolicyReportingServiceClient _travelTravelPolicyReportingService;
    private readonly ApiTravel.TravelPolicySyncService.TravelPolicySyncServiceClient _travelTravelPolicySyncService;

    public TravelPolicyLifecycleEndpoint(
        ApiCorporateevents.CorporateEventReportingService.CorporateEventReportingServiceClient corporateeventsCorporateEventReportingService,
        ApiDevices.DevicePolicyWorkflowService.DevicePolicyWorkflowServiceClient devicesDevicePolicyWorkflowService,
        ApiDocuments.DocumentNotificationService.DocumentNotificationServiceClient documentsDocumentNotificationService,
        ApiFeatureflags.FeatureFlagQueryService.FeatureFlagQueryServiceClient featureflagsFeatureFlagQueryService,
        ApiHelpdesk.TicketApprovalService.TicketApprovalServiceClient helpdeskTicketApprovalService,
        ApiLedger.LedgerAccountQueryService.LedgerAccountQueryServiceClient ledgerLedgerAccountQueryService,
        ApiLicensing.LicenseAssignmentReportingService.LicenseAssignmentReportingServiceClient licensingLicenseAssignmentReportingService,
        ILogger<TravelPolicyLifecycleEndpoint> logger,
        RetryPolicy policy,
        IRecordRepository repository,
        ApiTravel.ItineraryCalculationService.ItineraryCalculationServiceClient travelItineraryCalculationService,
        ApiTravel.ItineraryReportingService.ItineraryReportingServiceClient travelItineraryReportingService,
        ITravelPolicyLifecycleEndpointSendActivateTravelPolicy3Port travelPolicyLifecycleEndpointSendActivateTravelPolicy3PortAdapter,
        ApiTravel.TravelPolicyReportingService.TravelPolicyReportingServiceClient travelTravelPolicyReportingService,
        ApiTravel.TravelPolicySyncService.TravelPolicySyncServiceClient travelTravelPolicySyncService)
    {
        _corporateeventsCorporateEventReportingService = corporateeventsCorporateEventReportingService;
        _devicesDevicePolicyWorkflowService = devicesDevicePolicyWorkflowService;
        _documentsDocumentNotificationService = documentsDocumentNotificationService;
        _featureflagsFeatureFlagQueryService = featureflagsFeatureFlagQueryService;
        _helpdeskTicketApprovalService = helpdeskTicketApprovalService;
        _ledgerLedgerAccountQueryService = ledgerLedgerAccountQueryService;
        _licensingLicenseAssignmentReportingService = licensingLicenseAssignmentReportingService;
        _logger = logger;
        _policy = policy;
        _repository = repository;
        _travelItineraryCalculationService = travelItineraryCalculationService;
        _travelItineraryReportingService = travelItineraryReportingService;
        _travelPolicyLifecycleEndpointSendActivateTravelPolicy3Port = travelPolicyLifecycleEndpointSendActivateTravelPolicy3PortAdapter;
        _travelTravelPolicyReportingService = travelTravelPolicyReportingService;
        _travelTravelPolicySyncService = travelTravelPolicySyncService;
        _sendSuspendTravelPolicy1 = _featureflagsFeatureFlagQueryService.BrowseFeatureFlagsAsync;
    }
    /// <summary>
    /// Activates a travel policy.
    /// </summary>
    public override async Task<ApiTravel.ActivateTravelPolicyResponse> ActivateTravelPolicy(ApiTravel.ActivateTravelPolicyRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.TravelPolicyId, "travel_policy_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ActivateTravelPolicy for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy", request.TravelPolicyId,
            "ActivateTravelPolicy", "accepted", token);
        var input0 = new ApiTravel.PullTravelPoliciesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.PullTravelPoliciesRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                    UserId = (request.Details?.NewOwner?.UserId ?? ""),
                },
            },
        };
        using var rpc0 = _travelTravelPolicySyncService.PullTravelPoliciesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTravel.GenerateTravelPolicyReportRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (reply0.TravelPolicy ?? new ApiTravel.TravelPolicy()).ActionCase switch
            {
                ApiTravel.TravelPolicy.ActionOneofCase.Block => (reply0.TravelPolicy?.Block?.Reason ?? ""),
                _ => ""
            },
            TravelPolicyId = (reply0.TravelPolicy?.TravelPolicyId ?? ""),
        };
        using var rpc1 = _travelTravelPolicyReportingService.GenerateTravelPolicyReportAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = request.ActivateTravelPolicyToGetTravelPolicySyncState2();
        using var rpc2 = _travelTravelPolicySyncService.GetTravelPolicySyncStateAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiLedger.FindLedgerAccountsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLedger.FindLedgerAccountsRequest.Types.Filter
            {
                CreatedAfter = (reply0.Outcome?.EffectiveAt ?? new Timestamp()),
            },
        };
        var reply3 = await SendActivateTravelPolicy3Async(input3, token);
        var response = new ApiTravel.ActivateTravelPolicyResponse
        {
            Outcome = new ApiTravel.ActivateTravelPolicyResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Message = (reply1.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply3.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.TravelPolicy?.Audit?.UpdatedBy ?? ""),
                },
                Owner = (reply2.TravelPolicy?.Owner ?? new ApiIdentity.UserRef()),
                TravelPolicyId = (request.TravelPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy",
            "audit", "ActivateTravelPolicy", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Expires a travel policy.
    /// </summary>
    public override async Task<ApiTravel.ExpireTravelPolicyResponse> ExpireTravelPolicy(ApiTravel.ExpireTravelPolicyRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.TravelPolicyId, "travel_policy_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ExpireTravelPolicy for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy", request.TravelPolicyId,
            "ExpireTravelPolicy", "accepted", token);
        var input0 = new ApiDocuments.StreamDocumentNoticesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Subscriber = new ApiCommon.ContactInfo
            {
                Email = (request.Details?.NewOwner?.Email ?? ""),
                Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
            },
        };
        using var stream0 = _documentsDocumentNotificationService.StreamDocumentNotices(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received StreamDocumentNotices frame {Type}", change0.GetType().Name);
        }
        var input1 = new ApiCorporateevents.CountCorporateEventTotalsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiCorporateevents.CountCorporateEventTotalsRequest.Types.Filter
            {
                CreatedAfter = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
        };
        var packed1 = Any.Pack(input1);
        input1 = packed1.Unpack<ApiCorporateevents.CountCorporateEventTotalsRequest>();
        using var rpc1 = _corporateeventsCorporateEventReportingService.CountCorporateEventTotalsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiTravel.GetTravelPolicySyncStateRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            TravelPolicyId = (request.TravelPolicyId ?? ""),
        };
        var json2 = JsonFormatter.Default.Format(input2);
        input2 = JsonParser.Default.Parse<ApiTravel.GetTravelPolicySyncStateRequest>(json2);
        using var rpc2 = _travelTravelPolicySyncService.GetTravelPolicySyncStateAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiTravel.ExpireTravelPolicyResponse
        {
            Outcome = new ApiTravel.ExpireTravelPolicyResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
                Message = (reply1.Result?.Message ?? ""),
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Priority = (reply2.TravelPolicy?.Priority ?? 0),
                TravelPolicyId = (request.TravelPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy",
            "audit", "ExpireTravelPolicy", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a travel policy.
    /// </summary>
    public override async Task<ApiTravel.GetTravelPolicyLifecycleResponse> GetTravelPolicyLifecycle(ApiTravel.GetTravelPolicyLifecycleRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.TravelPolicyId, "travel_policy_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetTravelPolicyLifecycle for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy", request.TravelPolicyId,
            "GetTravelPolicyLifecycle", "accepted", token);
        var input0 = new ApiHelpdesk.DelegateTicketApprovalRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Ticket = new ApiHelpdesk.Ticket
        {
            Assignee = new ApiIdentity.UserRef
            {
                Email = (request.Context?.Actor?.Email ?? ""),
                UserId = (request.Context?.Actor?.UserId ?? ""),
            },
            ExternalReference = (request.TravelPolicyId ?? ""),
        };
        using var rpc0 = _helpdeskTicketApprovalService.DelegateTicketApprovalAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTravel.GetTravelPolicyLifecycleResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply0.Ticket?.Assignee?.Email ?? ""),
                },
                TravelPolicyId = (request.TravelPolicyId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Reinstates a travel policy.
    /// </summary>
    public override async Task<ApiTravel.ReinstateTravelPolicyResponse> ReinstateTravelPolicy(ApiTravel.ReinstateTravelPolicyRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.TravelPolicyId, "travel_policy_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ReinstateTravelPolicy for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy", request.TravelPolicyId,
            "ReinstateTravelPolicy", "accepted", token);
        var input0 = new ApiTravel.CalculateItineraryRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Itinerary = new ApiTravel.Itinerary
            {
                Counterparty = new ApiTravel.TravelBookingRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                },
            },
        };
        using var rpc0 = _travelItineraryCalculationService.CalculateItineraryAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.StreamLicenseAssignmentMetricsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Subscriber = new ApiCommon.ContactInfo
            {
                Phone = (request.Details?.NotifyContact?.Phone ?? ""),
            },
        };
        using var stream1 = _licensingLicenseAssignmentReportingService.StreamLicenseAssignmentMetrics(input1, cancellationToken: token);
        await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received StreamLicenseAssignmentMetrics frame {Type}", change1.GetType().Name);
        }
        var response = new ApiTravel.ReinstateTravelPolicyResponse
        {
            Outcome = new ApiTravel.ReinstateTravelPolicyResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                TravelPolicyId = (request.TravelPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy",
            "audit", "ReinstateTravelPolicy", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Renews a travel policy.
    /// </summary>
    public override async Task<ApiTravel.RenewTravelPolicyResponse> RenewTravelPolicy(ApiTravel.RenewTravelPolicyRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.TravelPolicyId, "travel_policy_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RenewTravelPolicy for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy", request.TravelPolicyId,
            "RenewTravelPolicy", "accepted", token);
        var input0 = new ApiDevices.StartDevicePolicyWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.StartDevicePolicyWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                    PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                },
            },
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                    UserId = (request.Details?.NewOwner?.UserId ?? ""),
                },
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _devicesDevicePolicyWorkflowService.StartDevicePolicyWorkflowAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiTravel.RenewTravelPolicyResponse
        {
            Outcome = new ApiTravel.RenewTravelPolicyResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Priority = (reply0.DevicePolicy?.Priority ?? 0),
                TravelPolicyId = (request.TravelPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy",
            "audit", "RenewTravelPolicy", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Suspends a travel policy.
    /// </summary>
    public override async Task<ApiTravel.SuspendTravelPolicyResponse> SuspendTravelPolicy(ApiTravel.SuspendTravelPolicyRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.TravelPolicyId, "travel_policy_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SuspendTravelPolicy for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy", request.TravelPolicyId,
            "SuspendTravelPolicy", "accepted", token);
        var input0 = new ApiTravel.StreamItineraryMetricsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Subscriber = new ApiCommon.ContactInfo
            {
                Email = (request.Details?.NotifyContact?.Email ?? ""),
                Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
            },
        };
        using var stream0 = _travelItineraryReportingService.StreamItineraryMetrics(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received StreamItineraryMetrics frame {Type}", change0.GetType().Name);
        }
        var input1 = new ApiFeatureflags.BrowseFeatureFlagsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiFeatureflags.BrowseFeatureFlagsRequest.Types.Filter
            {
                CreatedAfter = (request.Details?.EffectiveAt ?? new Timestamp()),
                Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Details?.NewOwner?.UserId ?? ""),
            },
        };
        using var rpc1 = _sendSuspendTravelPolicy1(input1, new CallOptions(cancellationToken: token));
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTravel.SuspendTravelPolicyResponse
        {
            Outcome = new ApiTravel.SuspendTravelPolicyResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                TravelPolicyId = (request.TravelPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy",
            "audit", "SuspendTravelPolicy", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiLedger.FindLedgerAccountsResponse> SendActivateTravelPolicy3Async(ApiLedger.FindLedgerAccountsRequest request, CancellationToken token)
    {
        return await _travelPolicyLifecycleEndpointSendActivateTravelPolicy3Port.SendAsync(request, token);
    }
}
