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
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSearch = Acme.Search.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Devices;

/// <summary>
/// Moves a device policy between the states of its lifecycle.
/// </summary>
public sealed class DevicePolicyLifecycleEndpoint : ApiDevices.DevicePolicyLifecycleService.DevicePolicyLifecycleServiceBase
{
    private readonly IDevicePolicyLifecycleEndpointSendScheduleDevicePolicyExpiry0Port _devicePolicyLifecycleEndpointSendScheduleDevicePolicyExpiry0Port;
    private readonly ApiDevices.ComplianceScanCalculationService.ComplianceScanCalculationServiceClient _devicesComplianceScanCalculationService;
    private readonly ApiDevices.DevicePolicyQueryService.DevicePolicyQueryServiceClient _devicesDevicePolicyQueryService;
    private readonly ApiDevices.DevicePolicyValidationService.DevicePolicyValidationServiceClient _devicesDevicePolicyValidationService;
    private readonly ApiDevices.ManagedDeviceCalculationService.ManagedDeviceCalculationServiceClient _devicesManagedDeviceCalculationService;
    private readonly ApiDevices.ManagedDeviceService.ManagedDeviceServiceClient _devicesManagedDeviceService;
    private readonly ApiDevices.ManagedDeviceValidationService.ManagedDeviceValidationServiceClient _devicesManagedDeviceValidationService;
    private readonly ApiDocuments.DocumentSyncService.DocumentSyncServiceClient _documentsDocumentSyncService;
    private readonly ApiLedger.LedgerAccountQueryService.LedgerAccountQueryServiceClient _ledgerLedgerAccountQueryService;
    private readonly ApiLicensing.EntitlementWorkflowService.EntitlementWorkflowServiceClient _licensingEntitlementWorkflowService;
    private readonly ILogger<DevicePolicyLifecycleEndpoint> _logger;
    private readonly ApiPerformance.GoalValidationService.GoalValidationServiceClient _performanceGoalValidationService;
    private readonly ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient _recruitingJobOpeningCalculationService;
    private readonly ApiRecruiting.JobOpeningService.JobOpeningServiceClient _recruitingJobOpeningService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiSearch.SearchIndexQueryService.SearchIndexQueryServiceClient _searchSearchIndexQueryService;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTravel.TripService.TripServiceClient _travelTripService;

    public DevicePolicyLifecycleEndpoint(
        IDevicePolicyLifecycleEndpointSendScheduleDevicePolicyExpiry0Port devicePolicyLifecycleEndpointSendScheduleDevicePolicyExpiry0PortAdapter,
        ApiDevices.ComplianceScanCalculationService.ComplianceScanCalculationServiceClient devicesComplianceScanCalculationService,
        ApiDevices.DevicePolicyQueryService.DevicePolicyQueryServiceClient devicesDevicePolicyQueryService,
        ApiDevices.DevicePolicyValidationService.DevicePolicyValidationServiceClient devicesDevicePolicyValidationService,
        ApiDevices.ManagedDeviceCalculationService.ManagedDeviceCalculationServiceClient devicesManagedDeviceCalculationService,
        ApiDevices.ManagedDeviceService.ManagedDeviceServiceClient devicesManagedDeviceService,
        ApiDevices.ManagedDeviceValidationService.ManagedDeviceValidationServiceClient devicesManagedDeviceValidationService,
        ApiDocuments.DocumentSyncService.DocumentSyncServiceClient documentsDocumentSyncService,
        ApiLedger.LedgerAccountQueryService.LedgerAccountQueryServiceClient ledgerLedgerAccountQueryService,
        ApiLicensing.EntitlementWorkflowService.EntitlementWorkflowServiceClient licensingEntitlementWorkflowService,
        ILogger<DevicePolicyLifecycleEndpoint> logger,
        ApiPerformance.GoalValidationService.GoalValidationServiceClient performanceGoalValidationService,
        ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient recruitingJobOpeningCalculationService,
        ApiRecruiting.JobOpeningService.JobOpeningServiceClient recruitingJobOpeningService,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiSearch.SearchIndexQueryService.SearchIndexQueryServiceClient searchSearchIndexQueryService,
        IOptions<MeshOptions> settings,
        ApiTravel.TripService.TripServiceClient travelTripService)
    {
        _devicePolicyLifecycleEndpointSendScheduleDevicePolicyExpiry0Port = devicePolicyLifecycleEndpointSendScheduleDevicePolicyExpiry0PortAdapter;
        _devicesComplianceScanCalculationService = devicesComplianceScanCalculationService;
        _devicesDevicePolicyQueryService = devicesDevicePolicyQueryService;
        _devicesDevicePolicyValidationService = devicesDevicePolicyValidationService;
        _devicesManagedDeviceCalculationService = devicesManagedDeviceCalculationService;
        _devicesManagedDeviceService = devicesManagedDeviceService;
        _devicesManagedDeviceValidationService = devicesManagedDeviceValidationService;
        _documentsDocumentSyncService = documentsDocumentSyncService;
        _ledgerLedgerAccountQueryService = ledgerLedgerAccountQueryService;
        _licensingEntitlementWorkflowService = licensingEntitlementWorkflowService;
        _logger = logger;
        _performanceGoalValidationService = performanceGoalValidationService;
        _recruitingJobOpeningCalculationService = recruitingJobOpeningCalculationService;
        _recruitingJobOpeningService = recruitingJobOpeningService;
        _registry = registry;
        _repository = repository;
        _searchSearchIndexQueryService = searchSearchIndexQueryService;
        _settings = settings;
        _travelTripService = travelTripService;
    }
    /// <summary>
    /// Activates a device policy.
    /// </summary>
    public override async Task<ApiDevices.ActivateDevicePolicyResponse> ActivateDevicePolicy(ApiDevices.ActivateDevicePolicyRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DevicePolicyId, "device_policy_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ActivateDevicePolicy for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "device_policy", request.DevicePolicyId,
            "ActivateDevicePolicy", "accepted", token);
        var input0 = new ApiDevices.ExplainDevicePolicyDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.ExplainDevicePolicyDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _devicesDevicePolicyValidationService.ExplainDevicePolicyDecisionAsync);
        var input1 = new ApiDevices.RecalculateComplianceScansRequest
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (reply0.DevicePolicy?.Owner?.DisplayName ?? ""),
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc1 = _devicesComplianceScanCalculationService.RecalculateComplianceScansAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDevices.NormalizeManagedDeviceRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.NormalizeManagedDeviceRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply0.DevicePolicy?.Owner?.Email ?? ""),
                },
            },
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.DevicePolicy?.Audit?.Etag ?? ""),
                },
            },
        };
        var named2 = _registry.Get<ApiDevices.ManagedDeviceValidationService.ManagedDeviceValidationServiceClient>("acme.devices.v1.ManagedDeviceValidationService");
        using var rpc2 = named2.NormalizeManagedDeviceAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiDevices.ActivateDevicePolicyResponse
        {
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply2.ManagedDevice?.Audit?.CreatedAt ?? new Timestamp()),
                },
                DevicePolicyId = (request.DevicePolicyId ?? ""),
                Name = (reply0.DevicePolicy?.Name ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                },
            },
            Outcome = new ApiDevices.ActivateDevicePolicyResponse.Types.Outcome
            {
                EffectiveAt = (reply0.Outcome?.EffectiveAt ?? new Timestamp()),
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "device_policy",
            "audit", "ActivateDevicePolicy", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a device policy.
    /// </summary>
    public override async Task<ApiDevices.GetDevicePolicyLifecycleResponse> GetDevicePolicyLifecycle(ApiDevices.GetDevicePolicyLifecycleRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DevicePolicyId, "device_policy_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetDevicePolicyLifecycle for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "device_policy", request.DevicePolicyId,
            "GetDevicePolicyLifecycle", "accepted", token);
        var input0 = new ApiDevices.CalculateComplianceScanRequest
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Measurements = { new ApiDevices.ComplianceScanMeasurement
                {
                    ObservedAt = (request.Context?.Deadline ?? new Timestamp()),
                } },
                Source = (request.DevicePolicyId ?? ""),
                Subject = new ApiRecruiting.CandidateRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _devicesComplianceScanCalculationService.CalculateComplianceScanAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDevices.GetDevicePolicyLifecycleResponse
        {
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                DevicePolicyId = (request.DevicePolicyId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.ComplianceScan?.Subject?.DisplayName ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.ComplianceScan?.CorrelationId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Reinstates a device policy.
    /// </summary>
    public override async Task<ApiDevices.ReinstateDevicePolicyResponse> ReinstateDevicePolicy(ApiDevices.ReinstateDevicePolicyRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DevicePolicyId, "device_policy_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ReinstateDevicePolicy for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "device_policy", request.DevicePolicyId,
            "ReinstateDevicePolicy", "accepted", token);
        var input0 = new ApiRecruiting.SearchJobOpeningsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiRecruiting.SearchJobOpeningsRequest.Types.Filter
            {
                CreatedAfter = (request.Details?.EffectiveAt ?? new Timestamp()),
                OwnerId = (request.Details?.NewOwner?.UserId ?? ""),
            },
        };
        var command0 = new DispatchCommand<ApiRecruiting.SearchJobOpeningsRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _recruitingJobOpeningService.SearchJobOpeningsAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var profile1 = new MappingProfile<ApiDevices.ReinstateDevicePolicyRequest, ApiDevices.LookupDevicePolicyRequest>(value => new ApiDevices.LookupDevicePolicyRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            DevicePolicyId = (value.DevicePolicyId ?? ""),
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _devicesDevicePolicyQueryService.LookupDevicePolicyAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiDevices.ReinstateDevicePolicyResponse
        {
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply1.DevicePolicy?.Audit?.UpdatedBy ?? ""),
                },
                DevicePolicyId = (request.DevicePolicyId ?? ""),
            },
            Outcome = new ApiDevices.ReinstateDevicePolicyResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "device_policy",
            "audit", "ReinstateDevicePolicy", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Renews a device policy.
    /// </summary>
    public override async Task<ApiDevices.RenewDevicePolicyResponse> RenewDevicePolicy(ApiDevices.RenewDevicePolicyRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DevicePolicyId, "device_policy_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RenewDevicePolicy for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "device_policy", request.DevicePolicyId,
            "RenewDevicePolicy", "accepted", token);
        var input0 = new ApiLicensing.AdvanceEntitlementWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Entitlement = new ApiLicensing.Entitlement
            {
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                    UserId = (request.Details?.NewOwner?.UserId ?? ""),
                },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _licensingEntitlementWorkflowService.AdvanceEntitlementWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = request.RenewDevicePolicyToPeekDevicePolicy1();
        using var rpc1 = _devicesDevicePolicyQueryService.PeekDevicePolicyAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiDevices.RenewDevicePolicyResponse
        {
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.DevicePolicy?.Audit?.CreatedAt ?? new Timestamp()),
                },
                DevicePolicyId = (request.DevicePolicyId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (reply0.Entitlement?.Owner?.UserId ?? ""),
                },
            },
            Outcome = new ApiDevices.RenewDevicePolicyResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "device_policy",
            "audit", "RenewDevicePolicy", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Schedules a device policy.
    /// </summary>
    public override async Task<ApiDevices.ScheduleDevicePolicyExpiryResponse> ScheduleDevicePolicyExpiry(ApiDevices.ScheduleDevicePolicyExpiryRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ScheduleDevicePolicyExpiry for {Tenant}", requestContext.TenantId);
        if (request.DevicePolicy is { } record)
        {
            var snapshot = DevicePolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.BrowseDevicePoliciesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDevices.BrowseDevicePoliciesRequest.Types.Filter
            {
                CreatedAfter = (request.DevicePolicy?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (request.DevicePolicy?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.DevicePolicy?.Owner?.UserId ?? ""),
            },
        };
        var reply0 = await SendScheduleDevicePolicyExpiry0Async(input0, token);
        var input1 = new ApiPerformance.ScoreGoalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Goal = new ApiPerformance.Goal
            {
                Assignee = new ApiIdentity.UserRef
                {
                    UserId = (request.DevicePolicy?.Owner?.UserId ?? ""),
                },
            },
        };
        var labels1 = input1.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input1.Context.Baggage.Clear();
        input1.Context.Baggage.Add(labels1);
        using var rpc1 = _performanceGoalValidationService.ScoreGoalAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiSearch.ExportSearchIndexesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiSearch.ExportSearchIndexesRequest.Types.Filter
            {
                CreatedAfter = (reply1.Goal?.Sla?.DueAt ?? new Timestamp()),
                Labels = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.DevicePolicy?.Owner?.UserId ?? ""),
            },
        };
        using var stream2 = _searchSearchIndexQueryService.ExportSearchIndexes(input2, cancellationToken: token);
        await foreach (var change2 in stream2.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportSearchIndexes frame {Type}", change2.GetType().Name);
        }
        var input3 = new ApiDevices.ExplainManagedDeviceCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.ExplainManagedDeviceCalculationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                },
            },
        };
        var json3 = JsonFormatter.Default.Format(input3);
        input3 = JsonParser.Default.Parse<ApiDevices.ExplainManagedDeviceCalculationRequest>(json3);
        using var rpc3 = _devicesManagedDeviceCalculationService.ExplainManagedDeviceCalculationAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiDevices.ScheduleDevicePolicyExpiryResponse
        {
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                DevicePolicyId = (request.DevicePolicyId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply1.Goal?.Assignee?.Email ?? ""),
                },
                Parameters = { (request.Details?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply3.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "device_policy",
            "audit", "ScheduleDevicePolicyExpiry", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Suspends a device policy.
    /// </summary>
    public override async Task<ApiDevices.SuspendDevicePolicyResponse> SuspendDevicePolicy(ApiDevices.SuspendDevicePolicyRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DevicePolicyId, "device_policy_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SuspendDevicePolicy for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "device_policy", request.DevicePolicyId,
            "SuspendDevicePolicy", "accepted", token);
        var input0 = new ApiRecruiting.EstimateJobOpeningRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.JobOpening = new ApiRecruiting.JobOpening
        {
            Counterparty = new ApiRecruiting.InterviewRef
            {
                DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
            },
        };
        using var rpc0 = _recruitingJobOpeningCalculationService.EstimateJobOpeningAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTravel.CloneTripRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Trip = new ApiTravel.Trip
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.JobOpening?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (reply0.JobOpening?.BillingAddress?.Recipient ?? ""),
                },
                Card = new ApiTravel.TripCardSettlement
                {
                    Last4 = (reply0.JobOpening ?? new ApiRecruiting.JobOpening()).PaymentCase switch
                    {
                        ApiRecruiting.JobOpening.SettlementOneofCase.Card => (reply0.JobOpening?.Card?.Last4 ?? ""),
                        _ => ""
                    },
                    Token = (reply0.JobOpening ?? new ApiRecruiting.JobOpening()).PaymentCase switch
                    {
                        ApiRecruiting.JobOpening.SettlementOneofCase.Card => (reply0.JobOpening?.Card?.Token ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _travelTripService.CloneTripAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiDevices.SuspendDevicePolicyResponse
        {
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.JobOpening?.Audit?.Etag ?? ""),
                },
                DevicePolicyId = (request.DevicePolicyId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.Trip?.Counterparty?.DisplayName ?? ""),
                },
            },
            Outcome = new ApiDevices.SuspendDevicePolicyResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "device_policy",
            "audit", "SuspendDevicePolicy", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Terminates a device policy.
    /// </summary>
    public override async Task<ApiDevices.TerminateDevicePolicyResponse> TerminateDevicePolicy(ApiDevices.TerminateDevicePolicyRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DevicePolicyId, "device_policy_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling TerminateDevicePolicy for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "device_policy", request.DevicePolicyId,
            "TerminateDevicePolicy", "accepted", token);
        var input0 = new ApiLedger.BrowseLedgerAccountsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLedger.BrowseLedgerAccountsRequest.Types.Filter
            {
                CreatedAfter = (request.Details?.EffectiveAt ?? new Timestamp()),
                Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Details?.NewOwner?.UserId ?? ""),
            },
        };
        using var rpc0 = _ledgerLedgerAccountQueryService.BrowseLedgerAccountsAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiDevices.BatchGetManagedDevicesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc1 = _devicesManagedDeviceService.BatchGetManagedDevicesAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var input2 = new ApiRecruiting.CloneJobOpeningRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            JobOpening = new ApiRecruiting.JobOpening
            {
                Counterparty = new ApiRecruiting.InterviewRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                    StartsAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                },
                Lines = { new ApiRecruiting.JobOpeningLine
                {
                    Attributes = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                } },
            },
        };
        var reply2 = await Workflow.SendAsync(input2, token, _recruitingJobOpeningService.CloneJobOpeningAsync);
        var input3 = new ApiDocuments.SyncDocumentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.SyncDocumentsRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Document = new ApiDocuments.Document
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (reply2.JobOpening?.BillingAddress?.Locality ?? ""),
                },
                Card = new ApiDocuments.DocumentCardSettlement
                {
                    Last4 = (reply2.JobOpening ?? new ApiRecruiting.JobOpening()).PaymentCase switch
                    {
                        ApiRecruiting.JobOpening.SettlementOneofCase.Card => (reply2.JobOpening?.Card?.Last4 ?? ""),
                        _ => ""
                    },
                    Token = (reply2.JobOpening ?? new ApiRecruiting.JobOpening()).PaymentCase switch
                    {
                        ApiRecruiting.JobOpening.SettlementOneofCase.Card => (reply2.JobOpening?.Card?.Token ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc3 = _documentsDocumentSyncService.SyncDocumentsAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiDevices.TerminateDevicePolicyResponse
        {
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                DevicePolicyId = (request.DevicePolicyId ?? ""),
            },
            Outcome = new ApiDevices.TerminateDevicePolicyResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Message = (reply2.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply1.Result?.Message ?? ""),
                ReferenceIds = { (reply3.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "device_policy",
            "audit", "TerminateDevicePolicy", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiDevices.BrowseDevicePoliciesResponse> SendScheduleDevicePolicyExpiry0Async(ApiDevices.BrowseDevicePoliciesRequest request, CancellationToken token)
    {
        return await _devicePolicyLifecycleEndpointSendScheduleDevicePolicyExpiry0Port.SendAsync(request, token);
    }
}
