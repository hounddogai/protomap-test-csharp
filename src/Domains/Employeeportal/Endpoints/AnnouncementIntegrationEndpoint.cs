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
using ApiAuthorization = Acme.Authorization.V1;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiEdgegateway = Acme.Edgegateway.V1;
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;

namespace Acme.Workforce.Employeeportal;

/// <summary>
/// Connects external systems that send announcement events.
/// </summary>
public sealed class AnnouncementIntegrationEndpoint : ApiEmployeeportal.AnnouncementIntegrationService.AnnouncementIntegrationServiceBase
{
    private readonly IAnnouncementIntegrationEndpointSendDisconnectAnnouncementSource3Port _announcementIntegrationEndpointSendDisconnectAnnouncementSource3Port;
    private readonly ApiAudit.AuditEventValidationService.AuditEventValidationServiceClient _auditAuditEventValidationService;
    private readonly ApiAuthorization.AccessPolicyQueryService.AccessPolicyQueryServiceClient _authorizationAccessPolicyQueryService;
    private readonly NotificationBus _bus;
    private readonly ApiDevices.DevicePolicyWorkflowService.DevicePolicyWorkflowServiceClient _devicesDevicePolicyWorkflowService;
    private readonly ApiEdgegateway.RouteBindingQueryService.RouteBindingQueryServiceClient _edgegatewayRouteBindingQueryService;
    private readonly ApiEmployeeportal.AnnouncementWorkflowService.AnnouncementWorkflowServiceClient _employeeportalAnnouncementWorkflowService;
    private readonly ApiEmployeeportal.PortalHomeIntegrationService.PortalHomeIntegrationServiceClient _employeeportalPortalHomeIntegrationService;
    private readonly ApiEmployeeportal.PortalHomeNotificationService.PortalHomeNotificationServiceClient _employeeportalPortalHomeNotificationService;
    private readonly ApiEmployeeportal.SelfServiceRequestIntegrationService.SelfServiceRequestIntegrationServiceClient _employeeportalSelfServiceRequestIntegrationService;
    private readonly ApiFeatureflags.FeatureFlagValidationService.FeatureFlagValidationServiceClient _featureflagsFeatureFlagValidationService;
    private readonly ApiLicensing.LicenseAssignmentLifecycleService.LicenseAssignmentLifecycleServiceClient _licensingLicenseAssignmentLifecycleService;
    private readonly ILogger<AnnouncementIntegrationEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiDevices.SignalDevicePolicyWorkflowRequest, CallOptions, AsyncUnaryCall<ApiDevices.SignalDevicePolicyWorkflowResponse>> _sendConnectAnnouncementSource0;
    private readonly IOptions<MeshOptions> _settings;

    public AnnouncementIntegrationEndpoint(
        IAnnouncementIntegrationEndpointSendDisconnectAnnouncementSource3Port announcementIntegrationEndpointSendDisconnectAnnouncementSource3PortAdapter,
        ApiAudit.AuditEventValidationService.AuditEventValidationServiceClient auditAuditEventValidationService,
        ApiAuthorization.AccessPolicyQueryService.AccessPolicyQueryServiceClient authorizationAccessPolicyQueryService,
        NotificationBus bus,
        ApiDevices.DevicePolicyWorkflowService.DevicePolicyWorkflowServiceClient devicesDevicePolicyWorkflowService,
        ApiEdgegateway.RouteBindingQueryService.RouteBindingQueryServiceClient edgegatewayRouteBindingQueryService,
        ApiEmployeeportal.AnnouncementWorkflowService.AnnouncementWorkflowServiceClient employeeportalAnnouncementWorkflowService,
        ApiEmployeeportal.PortalHomeIntegrationService.PortalHomeIntegrationServiceClient employeeportalPortalHomeIntegrationService,
        ApiEmployeeportal.PortalHomeNotificationService.PortalHomeNotificationServiceClient employeeportalPortalHomeNotificationService,
        ApiEmployeeportal.SelfServiceRequestIntegrationService.SelfServiceRequestIntegrationServiceClient employeeportalSelfServiceRequestIntegrationService,
        ApiFeatureflags.FeatureFlagValidationService.FeatureFlagValidationServiceClient featureflagsFeatureFlagValidationService,
        ApiLicensing.LicenseAssignmentLifecycleService.LicenseAssignmentLifecycleServiceClient licensingLicenseAssignmentLifecycleService,
        ILogger<AnnouncementIntegrationEndpoint> logger,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings)
    {
        _announcementIntegrationEndpointSendDisconnectAnnouncementSource3Port = announcementIntegrationEndpointSendDisconnectAnnouncementSource3PortAdapter;
        _auditAuditEventValidationService = auditAuditEventValidationService;
        _authorizationAccessPolicyQueryService = authorizationAccessPolicyQueryService;
        _bus = bus;
        _devicesDevicePolicyWorkflowService = devicesDevicePolicyWorkflowService;
        _edgegatewayRouteBindingQueryService = edgegatewayRouteBindingQueryService;
        _employeeportalAnnouncementWorkflowService = employeeportalAnnouncementWorkflowService;
        _employeeportalPortalHomeIntegrationService = employeeportalPortalHomeIntegrationService;
        _employeeportalPortalHomeNotificationService = employeeportalPortalHomeNotificationService;
        _employeeportalSelfServiceRequestIntegrationService = employeeportalSelfServiceRequestIntegrationService;
        _featureflagsFeatureFlagValidationService = featureflagsFeatureFlagValidationService;
        _licensingLicenseAssignmentLifecycleService = licensingLicenseAssignmentLifecycleService;
        _logger = logger;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _settings = settings;
        _sendConnectAnnouncementSource0 = _devicesDevicePolicyWorkflowService.SignalDevicePolicyWorkflowAsync;
    }
    /// <summary>
    /// Acknowledges an announcement.
    /// </summary>
    public override async Task<ApiEmployeeportal.AcknowledgeAnnouncementEventResponse> AcknowledgeAnnouncementEvent(ApiEmployeeportal.AcknowledgeAnnouncementEventRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling AcknowledgeAnnouncementEvent for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "announcement", request.AnnouncementId,
            "AcknowledgeAnnouncementEvent", "accepted", token);
        var input0 = new ApiEmployeeportal.PauseAnnouncementWorkflowRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Announcement = new ApiEmployeeportal.Announcement
            {
                AnnouncementId = (request.AnnouncementId ?? ""),
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Reason ?? ""),
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _employeeportalAnnouncementWorkflowService.PauseAnnouncementWorkflowAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiEmployeeportal.UnsubscribePortalHomeUpdatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployeeportal.UnsubscribePortalHomeUpdatesRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
            Reason = (request.Reason ?? ""),
        };
        using var rpc1 = _employeeportalPortalHomeNotificationService.UnsubscribePortalHomeUpdatesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiEmployeeportal.AcknowledgeAnnouncementEventResponse
        {
            Announcement = new ApiEmployeeportal.Announcement
            {
                AnnouncementId = (reply1.PortalHome?.Customer?.AnnouncementId ?? ""),
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply0.Announcement?.BillingAddress?.CountryCode ?? ""),
                },
            },
            Outcome = new ApiEmployeeportal.AcknowledgeAnnouncementEventResponse.Types.Outcome
            {
                ReferenceId = (reply0.Outcome?.ReferenceId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.SourceSystem ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "announcement",
            "audit", "AcknowledgeAnnouncementEvent", (request.Details?.SourceSystem ?? ""), token);
        return response;
    }
    /// <summary>
    /// Connects an announcement.
    /// </summary>
    public override async Task<ApiEmployeeportal.ConnectAnnouncementSourceResponse> ConnectAnnouncementSource(ApiEmployeeportal.ConnectAnnouncementSourceRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ConnectAnnouncementSource for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "announcement", request.AnnouncementId,
            "ConnectAnnouncementSource", "accepted", token);
        var input0 = new ApiDevices.SignalDevicePolicyWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _sendConnectAnnouncementSource0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployeeportal.DisconnectPortalHomeSourceRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployeeportal.DisconnectPortalHomeSourceRequest.Types.Details
            {
                Envelope = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                MessageId = (request.Details?.MessageId ?? ""),
                SourceSystem = (request.Details?.SourceSystem ?? ""),
            },
        };
        using var rpc1 = _employeeportalPortalHomeIntegrationService.DisconnectPortalHomeSourceAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiEmployeeportal.UnsubscribePortalHomeUpdatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployeeportal.UnsubscribePortalHomeUpdatesRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Phone = (reply1.PortalHome?.Requester?.Phone ?? ""),
                },
            },
        };
        using var rpc2 = _employeeportalPortalHomeNotificationService.UnsubscribePortalHomeUpdatesAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiLicensing.ScheduleLicenseAssignmentExpiryRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.ScheduleLicenseAssignmentExpiryRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (reply0.DevicePolicy?.Owner?.Email ?? ""),
                    Phone = (reply1.PortalHome?.Requester?.Phone ?? ""),
                },
            },
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.PortalHome?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Counterparty = new ApiLicensing.EntitlementRef
                {
                    DisplayName = (reply2.PortalHome?.Assignee?.DisplayName ?? ""),
                },
            },
        };
        var command3 = new DispatchCommand<ApiLicensing.ScheduleLicenseAssignmentExpiryRequest>(input3,
            "primary", requestContext.TenantId);
        var routed3 = command3 with { Region = _settings.Value.Region };
        using var rpc3 = _licensingLicenseAssignmentLifecycleService.ScheduleLicenseAssignmentExpiryAsync(routed3.Payload, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiEmployeeportal.ConnectAnnouncementSourceResponse
        {
            Announcement = new ApiEmployeeportal.Announcement
            {
                AnnouncementId = (request.AnnouncementId ?? ""),
            },
            Outcome = new ApiEmployeeportal.ConnectAnnouncementSourceResponse.Types.Outcome
            {
                EffectiveAt = (reply2.Outcome?.EffectiveAt ?? new Timestamp()),
                Message = (reply1.Result?.Message ?? ""),
                NextSteps = { (reply3.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply3.Result?.CorrelationId ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "announcement",
            "audit", "ConnectAnnouncementSource", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Disconnects an announcement.
    /// </summary>
    public override async Task<ApiEmployeeportal.DisconnectAnnouncementSourceResponse> DisconnectAnnouncementSource(ApiEmployeeportal.DisconnectAnnouncementSourceRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling DisconnectAnnouncementSource for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "announcement", request.AnnouncementId,
            "DisconnectAnnouncementSource", "accepted", token);
        var profile0 = new MappingProfile<ApiEmployeeportal.DisconnectAnnouncementSourceRequest, ApiEmployeeportal.ListSelfServiceRequestSourcesRequest>(value => new ApiEmployeeportal.ListSelfServiceRequestSourcesRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployeeportal.ListSelfServiceRequestSourcesRequest.Types.Filter
            {
                CreatedAfter = (value.Details?.ReceivedAt ?? new Timestamp()),
                Labels = { (value.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (value.AnnouncementId ?? ""),
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _employeeportalSelfServiceRequestIntegrationService.ListSelfServiceRequestSourcesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployeeportal.ListAnnouncementWorkflowsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployeeportal.ListAnnouncementWorkflowsRequest.Types.Filter
            {
                CreatedAfter = (request.Details?.ReceivedAt ?? new Timestamp()),
                Labels = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        input1 = PropertyMap.Copy(input1);
        using var rpc1 = _employeeportalAnnouncementWorkflowService.ListAnnouncementWorkflowsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = request.DisconnectAnnouncementSourceToBrowseAccessPolicies2(reply0, reply1);
        using var rpc2 = _authorizationAccessPolicyQueryService.BrowseAccessPoliciesAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiEmployeeportal.PauseAnnouncementWorkflowRequest
        {
            Announcement = new ApiEmployeeportal.Announcement
            {
                AnnouncementId = (request.AnnouncementId ?? ""),
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployeeportal.PauseAnnouncementWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            Reason = (request.Reason ?? ""),
        };
        var reply3 = await SendDisconnectAnnouncementSource3Async(input3, token);
        var response = new ApiEmployeeportal.DisconnectAnnouncementSourceResponse
        {
            Announcement = new ApiEmployeeportal.Announcement
            {
                AnnouncementId = (request.AnnouncementId ?? ""),
                Metadata = { (reply3.Announcement?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Outcome = new ApiEmployeeportal.DisconnectAnnouncementSourceResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply2.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.SourceSystem ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "announcement",
            "audit", "DisconnectAnnouncementSource", (request.Details?.SourceSystem ?? ""), token);
        return response;
    }
    /// <summary>
    /// Lists announcements a page at a time.
    /// </summary>
    public override async Task<ApiEmployeeportal.ListAnnouncementSourcesResponse> ListAnnouncementSources(ApiEmployeeportal.ListAnnouncementSourcesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ListAnnouncementSources for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "announcement", "batch",
            "ListAnnouncementSources", "accepted", token);
        var input0 = new ApiEmployeeportal.RetryAnnouncementWorkflowRequest
        {
            Announcement = new ApiEmployeeportal.Announcement
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _employeeportalAnnouncementWorkflowService.RetryAnnouncementWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiEmployeeportal.RetryAnnouncementWorkflowRequest>("acme.employeeportal.v1.AnnouncementIntegrationService/ListAnnouncementSources/0", input0.Clone()), token);
        var input1 = new ApiEmployeeportal.ReplayPortalHomeEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployeeportal.ReplayPortalHomeEventsRequest.Types.Details
            {
                Envelope = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                MessageId = (reply0.Result?.Message ?? ""),
            },
            PortalHomeId = (reply0.Announcement?.Counterparty?.PortalHomeId ?? ""),
        };
        using var rpc1 = _employeeportalPortalHomeIntegrationService.ReplayPortalHomeEventsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiEmployeeportal.ListAnnouncementSourcesResponse
        {
            Announcements = { new ApiEmployeeportal.Announcement
            {
                Counterparty = new ApiEmployeeportal.PortalHomeRef
                {
                    DisplayName = (reply1.PortalHome?.Assignee?.DisplayName ?? ""),
                },
                DueAt = (reply1.PortalHome?.Sla?.DueAt ?? new Timestamp()),
            } },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "announcement",
            "audit", "ListAnnouncementSources", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Exchanges changes of announcements with a peer.
    /// </summary>
    public override async Task RelayAnnouncementMessages(IAsyncStreamReader<ApiEmployeeportal.RelayAnnouncementMessagesRequest> requests, IServerStreamWriter<ApiEmployeeportal.RelayAnnouncementMessagesResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        await foreach (var request in requests.ReadAllAsync(token))
        {
            RequestGuard.Tenant(request.Context);
            var requestContext = request.Context;
            using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
                requestContext.TenantId, requestContext.TraceId);
            _logger.LogInformation("Handling RelayAnnouncementMessages for {Tenant}", requestContext.TenantId);
            if (request.Change is { } record)
            {
                var snapshot = AnnouncementMapper.ToRecord(requestContext.TenantId, record);
                await _repository.SaveAsync(snapshot, token);
            }
            var input0 = new ApiEdgegateway.ExportRouteBindingsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiEdgegateway.ExportRouteBindingsRequest.Types.Filter
                {
                    Region = (request.Change?.BillingAddress?.Region ?? ""),
                },
            };
            using var stream0 = _edgegatewayRouteBindingQueryService.ExportRouteBindings(input0, cancellationToken: token);
            await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
            {
                _logger.LogDebug("Received ExportRouteBindings frame {Type}", change0.GetType().Name);
            }
            var input1 = new ApiEmployeeportal.SignalAnnouncementWorkflowRequest
            {
                Announcement = new ApiEmployeeportal.Announcement
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedBy = request.FrameCase switch
                        {
                            ApiEmployeeportal.RelayAnnouncementMessagesRequest.FrameOneofCase.Change => (request.Change?.Audit?.CreatedBy ?? ""),
                            _ => ""
                        },
                    },
                    Card = new ApiEmployeeportal.AnnouncementCardSettlement
                    {
                        HolderName = request.FrameCase switch
                        {
                            ApiEmployeeportal.RelayAnnouncementMessagesRequest.FrameOneofCase.Change => (request.Change?.Card?.HolderName ?? ""),
                            _ => ""
                        },
                    },
                },
                Context = (request.Context ?? new ApiCommon.RequestContext()),
            };
            using var rpc1 = _employeeportalAnnouncementWorkflowService.SignalAnnouncementWorkflowAsync(input1, cancellationToken: token);
            var reply1 = await rpc1.ResponseAsync;
            var input2 = new ApiEmployeeportal.ReplayPortalHomeEventsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiEmployeeportal.ReplayPortalHomeEventsRequest.Types.Details
                {
                    Envelope = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                    MessageId = (reply1.Outcome?.Message ?? ""),
                },
                PortalHomeId = request.FrameCase switch
                {
                    ApiEmployeeportal.RelayAnnouncementMessagesRequest.FrameOneofCase.Change => (request.Change?.Counterparty?.PortalHomeId ?? ""),
                    _ => ""
                },
            };
            using var rpc2 = _employeeportalPortalHomeIntegrationService.ReplayPortalHomeEventsAsync(input2, cancellationToken: token);
            var reply2 = await rpc2.ResponseAsync;
            var response = new ApiEmployeeportal.RelayAnnouncementMessagesResponse
            {
                Announcement = new ApiEmployeeportal.Announcement
                {
                    BillingAddress = new ApiCommon.PostalAddress
                    {
                        Region = (reply1.Announcement?.BillingAddress?.Region ?? ""),
                    },
                    IssuedAt = (request.Change?.IssuedAt ?? new Timestamp()),
                    Metadata = { (request.Change?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                    Number = (reply1.Announcement?.Number ?? ""),
                },
                Result = new ApiCommon.OperationResult
                {
                    CorrelationId = (reply2.Result?.CorrelationId ?? ""),
                    Message = (reply2.Result?.Message ?? ""),
                },
            };
            _logger.LogDebug("Local audit detail {Detail}", (request.Cursor ?? ""));
            await _repository.RecordAsync(requestContext.TenantId, "announcement",
                "audit", "RelayAnnouncementMessages", (request.Cursor ?? ""), token);
            await responses.WriteAsync(response, token);
        }
    }
    /// <summary>
    /// Replays an announcement.
    /// </summary>
    public override async Task<ApiEmployeeportal.ReplayAnnouncementEventsResponse> ReplayAnnouncementEvents(ApiEmployeeportal.ReplayAnnouncementEventsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ReplayAnnouncementEvents for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "announcement", request.AnnouncementId,
            "ReplayAnnouncementEvents", "accepted", token);
        var input0 = new ApiEmployeeportal.SignalAnnouncementWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Reason ?? ""),
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _employeeportalAnnouncementWorkflowService.SignalAnnouncementWorkflowAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiEmployeeportal.InspectAnnouncementWorkflowRequest
        {
            AnnouncementId = (request.AnnouncementId ?? ""),
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply1 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _employeeportalAnnouncementWorkflowService.InspectAnnouncementWorkflowAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input2 = new ApiEmployeeportal.StartAnnouncementWorkflowRequest
        {
            Announcement = new ApiEmployeeportal.Announcement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.Announcement?.Audit?.CreatedBy ?? ""),
                    Etag = (reply1.Announcement?.Audit?.Etag ?? ""),
                },
                Bank = new ApiEmployeeportal.AnnouncementBankSettlement
                {
                    Bic = (reply1.Announcement ?? new ApiEmployeeportal.Announcement()).PaymentCase switch
                    {
                        ApiEmployeeportal.Announcement.SettlementOneofCase.Bank => (reply1.Announcement?.Bank?.Bic ?? ""),
                        _ => ""
                    },
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply0.Announcement?.BillingAddress?.CountryCode ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc2 = _employeeportalAnnouncementWorkflowService.StartAnnouncementWorkflowAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiEmployeeportal.ReplayAnnouncementEventsResponse
        {
            Announcement = new ApiEmployeeportal.Announcement
            {
                AnnouncementId = (reply2.Announcement?.AnnouncementId ?? ""),
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.Announcement?.Audit?.UpdatedBy ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.SourceSystem ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "announcement",
            "audit", "ReplayAnnouncementEvents", (request.Details?.SourceSystem ?? ""), token);
        return response;
    }
    /// <summary>
    /// Tests an announcement.
    /// </summary>
    public override async Task<ApiEmployeeportal.TestAnnouncementConnectionResponse> TestAnnouncementConnection(ApiEmployeeportal.TestAnnouncementConnectionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling TestAnnouncementConnection for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "announcement", request.AnnouncementId,
            "TestAnnouncementConnection", "accepted", token);
        var input0 = new ApiAudit.ExplainAuditEventDecisionRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            AuditEvent = new ApiAudit.AuditEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiAudit.ExplainAuditEventDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _auditAuditEventValidationService.ExplainAuditEventDecisionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiFeatureflags.NormalizeFeatureFlagRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            FeatureFlag = new ApiFeatureflags.FeatureFlag
            {
                Block = new ApiFeatureflags.FeatureFlagBlockAction
                {
                    Reason = (request.Reason ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.AuditEvent?.Subject?.DisplayName ?? ""),
                },
            },
        };
        var named1 = _registry.Get<ApiFeatureflags.FeatureFlagValidationService.FeatureFlagValidationServiceClient>("acme.featureflags.v1.FeatureFlagValidationService");
        using var rpc1 = named1.NormalizeFeatureFlagAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiEmployeeportal.TestAnnouncementConnectionResponse
        {
            Announcement = new ApiEmployeeportal.Announcement
            {
                AnnouncementId = (request.AnnouncementId ?? ""),
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.FeatureFlag?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
            Outcome = new ApiEmployeeportal.TestAnnouncementConnectionResponse.Types.Outcome
            {
                Message = (reply1.Outcome?.Message ?? ""),
                ReferenceId = (reply0.Outcome?.ReferenceId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.SourceSystem ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "announcement",
            "audit", "TestAnnouncementConnection", (request.Details?.SourceSystem ?? ""), token);
        return response;
    }
    private async Task<ApiEmployeeportal.PauseAnnouncementWorkflowResponse> SendDisconnectAnnouncementSource3Async(ApiEmployeeportal.PauseAnnouncementWorkflowRequest request, CancellationToken token)
    {
        return await _announcementIntegrationEndpointSendDisconnectAnnouncementSource3Port.SendAsync(request, token);
    }
}
