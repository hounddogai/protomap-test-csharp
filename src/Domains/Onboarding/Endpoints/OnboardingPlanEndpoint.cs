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
using ApiAppointments = Acme.Appointments.V1;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiScheduling = Acme.Scheduling.V1;

namespace Acme.Workforce.Onboarding;

/// <summary>
/// Manages onboarding plans: creates, reads, changes, and lists them.
/// </summary>
public sealed class OnboardingPlanEndpoint : ApiOnboarding.OnboardingPlanService.OnboardingPlanServiceBase
{
    private readonly ApiAppointments.AppointmentQueryService.AppointmentQueryServiceClient _appointmentsAppointmentQueryService;
    private readonly ApiDevices.ManagedDeviceCalculationService.ManagedDeviceCalculationServiceClient _devicesManagedDeviceCalculationService;
    private readonly ApiDocuments.DocumentFolderSyncService.DocumentFolderSyncServiceClient _documentsDocumentFolderSyncService;
    private readonly ApiDocuments.DocumentTemplateNotificationService.DocumentTemplateNotificationServiceClient _documentsDocumentTemplateNotificationService;
    private readonly ApiDocuments.DocumentValidationService.DocumentValidationServiceClient _documentsDocumentValidationService;
    private readonly ApiItsm.ServiceAssetCalculationService.ServiceAssetCalculationServiceClient _itsmServiceAssetCalculationService;
    private readonly ILogger<OnboardingPlanEndpoint> _logger;
    private readonly ApiNotifications.NotificationQueryService.NotificationQueryServiceClient _notificationsNotificationQueryService;
    private readonly ApiOnboarding.OnboardingPlanCalculationService.OnboardingPlanCalculationServiceClient _onboardingOnboardingPlanCalculationService;
    private readonly ApiOnboarding.OnboardingPlanValidationService.OnboardingPlanValidationServiceClient _onboardingOnboardingPlanValidationService;
    private readonly ApiOnboarding.OnboardingTaskValidationService.OnboardingTaskValidationServiceClient _onboardingOnboardingTaskValidationService;
    private readonly IOnboardingPlanEndpointSendWatchOnboardingPlans1Port _onboardingPlanEndpointSendWatchOnboardingPlans1Port;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiScheduling.RosterQueryService.RosterQueryServiceClient _schedulingRosterQueryService;
    private readonly ApiScheduling.ShiftService.ShiftServiceClient _schedulingShiftService;
    private readonly Func<ApiDocuments.CheckDocumentEligibilityRequest, CallOptions, AsyncUnaryCall<ApiDocuments.CheckDocumentEligibilityResponse>> _sendDeleteOnboardingPlan1;
    private readonly IOptions<MeshOptions> _settings;

    public OnboardingPlanEndpoint(
        ApiAppointments.AppointmentQueryService.AppointmentQueryServiceClient appointmentsAppointmentQueryService,
        ApiDevices.ManagedDeviceCalculationService.ManagedDeviceCalculationServiceClient devicesManagedDeviceCalculationService,
        ApiDocuments.DocumentFolderSyncService.DocumentFolderSyncServiceClient documentsDocumentFolderSyncService,
        ApiDocuments.DocumentTemplateNotificationService.DocumentTemplateNotificationServiceClient documentsDocumentTemplateNotificationService,
        ApiDocuments.DocumentValidationService.DocumentValidationServiceClient documentsDocumentValidationService,
        ApiItsm.ServiceAssetCalculationService.ServiceAssetCalculationServiceClient itsmServiceAssetCalculationService,
        ILogger<OnboardingPlanEndpoint> logger,
        ApiNotifications.NotificationQueryService.NotificationQueryServiceClient notificationsNotificationQueryService,
        ApiOnboarding.OnboardingPlanCalculationService.OnboardingPlanCalculationServiceClient onboardingOnboardingPlanCalculationService,
        ApiOnboarding.OnboardingPlanValidationService.OnboardingPlanValidationServiceClient onboardingOnboardingPlanValidationService,
        ApiOnboarding.OnboardingTaskValidationService.OnboardingTaskValidationServiceClient onboardingOnboardingTaskValidationService,
        IOnboardingPlanEndpointSendWatchOnboardingPlans1Port onboardingPlanEndpointSendWatchOnboardingPlans1PortAdapter,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiScheduling.RosterQueryService.RosterQueryServiceClient schedulingRosterQueryService,
        ApiScheduling.ShiftService.ShiftServiceClient schedulingShiftService,
        IOptions<MeshOptions> settings)
    {
        _appointmentsAppointmentQueryService = appointmentsAppointmentQueryService;
        _devicesManagedDeviceCalculationService = devicesManagedDeviceCalculationService;
        _documentsDocumentFolderSyncService = documentsDocumentFolderSyncService;
        _documentsDocumentTemplateNotificationService = documentsDocumentTemplateNotificationService;
        _documentsDocumentValidationService = documentsDocumentValidationService;
        _itsmServiceAssetCalculationService = itsmServiceAssetCalculationService;
        _logger = logger;
        _notificationsNotificationQueryService = notificationsNotificationQueryService;
        _onboardingOnboardingPlanCalculationService = onboardingOnboardingPlanCalculationService;
        _onboardingOnboardingPlanValidationService = onboardingOnboardingPlanValidationService;
        _onboardingOnboardingTaskValidationService = onboardingOnboardingTaskValidationService;
        _onboardingPlanEndpointSendWatchOnboardingPlans1Port = onboardingPlanEndpointSendWatchOnboardingPlans1PortAdapter;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _schedulingRosterQueryService = schedulingRosterQueryService;
        _schedulingShiftService = schedulingShiftService;
        _settings = settings;
        _sendDeleteOnboardingPlan1 = _documentsDocumentValidationService.CheckDocumentEligibilityAsync;
    }
    /// <summary>
    /// Takes a stream of onboarding plans and answers once at the end.
    /// </summary>
    public override async Task<ApiOnboarding.BatchCreateOnboardingPlansResponse> BatchCreateOnboardingPlans(IAsyncStreamReader<ApiOnboarding.BatchCreateOnboardingPlansRequest> requests, ServerCallContext context)
    {
        var token = context.CancellationToken;
        var result = new ApiOnboarding.BatchCreateOnboardingPlansResponse();
        await foreach (var request in requests.ReadAllAsync(token))
        {
            RequestGuard.Tenant(request.Context);
            var requestContext = request.Context;
            using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
                requestContext.TenantId, requestContext.TraceId);
            _logger.LogInformation("Handling BatchCreateOnboardingPlans for {Tenant}", requestContext.TenantId);
            if (request.Record is { } record)
            {
                var snapshot = OnboardingPlanMapper.ToRecord(requestContext.TenantId, record);
                await _repository.SaveAsync(snapshot, token);
            }
            var input0 = new ApiDocuments.ListDocumentTemplateSubscriptionsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiDocuments.ListDocumentTemplateSubscriptionsRequest.Types.Filter
                {
                    CreatedAfter = request.ChunkCase switch
                    {
                        ApiOnboarding.BatchCreateOnboardingPlansRequest.ChunkOneofCase.Record => (request.Record?.Audit?.UpdatedAt ?? new Timestamp()),
                        _ => new Timestamp()
                    },
                    Labels = { request.ChunkCase switch
                    {
                        ApiOnboarding.BatchCreateOnboardingPlansRequest.ChunkOneofCase.Record => (request.Record?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()),
                        _ => new Google.Protobuf.Collections.MapField<string, string>()
                    } },
                    OwnerId = request.ChunkCase switch
                    {
                        ApiOnboarding.BatchCreateOnboardingPlansRequest.ChunkOneofCase.Record => (request.Record?.Counterparty?.EquipmentId ?? ""),
                        _ => ""
                    },
                    Region = request.ChunkCase switch
                    {
                        ApiOnboarding.BatchCreateOnboardingPlansRequest.ChunkOneofCase.Record => (request.Record?.BillingAddress?.Region ?? ""),
                        _ => ""
                    },
                },
            };
            using var rpc0 = _documentsDocumentTemplateNotificationService.ListDocumentTemplateSubscriptionsAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            var response = new ApiOnboarding.BatchCreateOnboardingPlansResponse
            {
                Result = new ApiCommon.OperationResult
                {
                    Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                },
            };
            _logger.LogDebug("Local audit detail {Detail}", (request.Record?.VoucherCode ?? ""));
            await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan",
                "audit", "BatchCreateOnboardingPlans", (request.Record?.VoucherCode ?? ""), token);
            result = response;
        }
        return result;
    }
    /// <summary>
    /// Returns several onboarding plans at once.
    /// </summary>
    public override async Task<ApiOnboarding.BatchGetOnboardingPlansResponse> BatchGetOnboardingPlans(ApiOnboarding.BatchGetOnboardingPlansRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling BatchGetOnboardingPlans for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan", "batch",
            "BatchGetOnboardingPlans", "accepted", token);
        var response = new ApiOnboarding.BatchGetOnboardingPlansResponse();
        return response;
    }
    /// <summary>
    /// Creates an onboarding plan.
    /// </summary>
    public override async Task<ApiOnboarding.CloneOnboardingPlanResponse> CloneOnboardingPlan(ApiOnboarding.CloneOnboardingPlanRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CloneOnboardingPlan for {Tenant}", requestContext.TenantId);
        if (request.OnboardingPlan is { } record)
        {
            var snapshot = OnboardingPlanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiNotifications.ResolveNotificationsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiNotifications.ResolveNotificationsRequest>();
        using var rpc0 = _notificationsNotificationQueryService.ResolveNotificationsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiScheduling.SuggestRostersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiScheduling.SuggestRostersRequest.Types.Filter
            {
                CreatedAfter = (request.OnboardingPlan?.IssuedAt ?? new Timestamp()),
                Region = (request.OnboardingPlan?.BillingAddress?.Region ?? ""),
            },
        };
        var json1 = JsonFormatter.Default.Format(input1);
        input1 = JsonParser.Default.Parse<ApiScheduling.SuggestRostersRequest>(json1);
        using var rpc1 = _schedulingRosterQueryService.SuggestRostersAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiOnboarding.CloneOnboardingPlanResponse
        {
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Notes = (request.OnboardingPlan?.Notes ?? ""),
                Status = (request.OnboardingPlan?.Status ?? (ApiOnboarding.OnboardingPlanStatus)0),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Message = (reply1.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.RequestId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan",
            "audit", "CloneOnboardingPlan", (request.RequestId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Creates an onboarding plan.
    /// </summary>
    public override async Task<ApiOnboarding.CreateOnboardingPlanResponse> CreateOnboardingPlan(ApiOnboarding.CreateOnboardingPlanRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CreateOnboardingPlan for {Tenant}", requestContext.TenantId);
        if (request.OnboardingPlan is { } record)
        {
            var snapshot = OnboardingPlanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiOnboarding.QuoteOnboardingPlanRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.OnboardingPlan = new ApiOnboarding.OnboardingPlan
        {
            Audit = new ApiCommon.AuditInfo
            {
                UpdatedAt = (request.OnboardingPlan?.Audit?.UpdatedAt ?? new Timestamp()),
            },
        };
        using var rpc0 = _onboardingOnboardingPlanCalculationService.QuoteOnboardingPlanAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiOnboarding.ExplainOnboardingTaskDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.OnboardingPlan?.Audit?.Etag ?? ""),
                },
            },
        };
        using var rpc1 = _onboardingOnboardingTaskValidationService.ExplainOnboardingTaskDecisionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiOnboarding.GetOnboardingPlanRatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            OnboardingPlanId = (request.OnboardingPlan?.OnboardingPlanId ?? ""),
        };
        var reply2 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _onboardingOnboardingPlanCalculationService.GetOnboardingPlanRatesAsync(input2, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input3 = new ApiScheduling.UpdateShiftRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Shift = new ApiScheduling.Shift
            {
                Participants = { new ApiScheduling.ShiftParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        PreferredChannel = (reply1.OnboardingTask?.Requester?.PreferredChannel ?? ""),
                    },
                } },
            },
        };
        var reply3 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _schedulingShiftService.UpdateShiftAsync(input3, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiOnboarding.CreateOnboardingPlanResponse
        {
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply2.OnboardingPlan?.Audit?.CreatedBy ?? ""),
                    Etag = (reply2.OnboardingPlan?.Audit?.Etag ?? ""),
                    UpdatedBy = (reply3.Shift?.Audit?.UpdatedBy ?? ""),
                },
                IssuedAt = (request.OnboardingPlan?.IssuedAt ?? new Timestamp()),
                OnboardingPlanId = (request.OnboardingPlan?.OnboardingPlanId ?? ""),
                Total = (reply0.OnboardingPlan?.Total ?? new ApiCommon.Money()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply3.Result?.CorrelationId ?? ""),
                Message = (reply1.Result?.Message ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.RequestId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan",
            "audit", "CreateOnboardingPlan", (request.RequestId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Deletes an onboarding plan.
    /// </summary>
    public override async Task<ApiOnboarding.DeleteOnboardingPlanResponse> DeleteOnboardingPlan(ApiOnboarding.DeleteOnboardingPlanRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.OnboardingPlanId, "onboarding_plan_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling DeleteOnboardingPlan for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan", request.OnboardingPlanId,
            "DeleteOnboardingPlan", "accepted", token);
        var input0 = new ApiOnboarding.QuoteOnboardingPlanRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Etag ?? ""),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _onboardingOnboardingPlanCalculationService.QuoteOnboardingPlanAsync);
        var input1 = new ApiDocuments.CheckDocumentEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Document = new ApiDocuments.Document
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply0.OnboardingPlan?.BillingAddress?.CountryCode ?? ""),
                    Locality = (reply0.OnboardingPlan?.BillingAddress?.Locality ?? ""),
                    Recipient = (reply0.OnboardingPlan?.BillingAddress?.Recipient ?? ""),
                },
            },
        };
        using var rpc1 = _sendDeleteOnboardingPlan1(input1, new CallOptions(cancellationToken: token));
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiOnboarding.DeleteOnboardingPlanResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan",
            "audit", "DeleteOnboardingPlan", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an onboarding plan.
    /// </summary>
    public override async Task<ApiOnboarding.GetOnboardingPlanResponse> GetOnboardingPlan(ApiOnboarding.GetOnboardingPlanRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.OnboardingPlanId, "onboarding_plan_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetOnboardingPlan for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan", request.OnboardingPlanId,
            "GetOnboardingPlan", "accepted", token);
        var input0 = new ApiOnboarding.NormalizeOnboardingPlanRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.NormalizeOnboardingPlanRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Locale = (request.Context?.Locale ?? ""),
            },
            OnboardingPlanId = (request.OnboardingPlanId ?? ""),
        };
        var named0 = _registry.Get<ApiOnboarding.OnboardingPlanValidationService.OnboardingPlanValidationServiceClient>("acme.onboarding.v1.OnboardingPlanValidationService");
        using var rpc0 = named0.NormalizeOnboardingPlanAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiOnboarding.GetOnboardingPlanResponse
        {
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.OnboardingPlan?.Audit?.CreatedAt ?? new Timestamp()),
                },
                OnboardingPlanId = (request.OnboardingPlanId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Outcome?.Message ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists onboarding plans a page at a time.
    /// </summary>
    public override async Task<ApiOnboarding.ListOnboardingPlansResponse> ListOnboardingPlans(ApiOnboarding.ListOnboardingPlansRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ListOnboardingPlans for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan", "batch",
            "ListOnboardingPlans", "accepted", token);
        var response = new ApiOnboarding.ListOnboardingPlansResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan",
            "audit", "ListOnboardingPlans", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Lists onboarding plans a page at a time.
    /// </summary>
    public override async Task<ApiOnboarding.SearchOnboardingPlansResponse> SearchOnboardingPlans(ApiOnboarding.SearchOnboardingPlansRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SearchOnboardingPlans for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan", "batch",
            "SearchOnboardingPlans", "accepted", token);
        var input0 = new ApiScheduling.ExportRostersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiScheduling.ExportRostersRequest.Types.Filter
            {
                CreatedAfter = (request.Filter?.CreatedAfter ?? new Timestamp()),
            },
        };
        using var stream0 = _schedulingRosterQueryService.ExportRosters(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportRosters frame {Type}", change0.GetType().Name);
        }
        var response = new ApiOnboarding.SearchOnboardingPlansResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan",
            "audit", "SearchOnboardingPlans", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Changes an onboarding plan.
    /// </summary>
    public override async Task<ApiOnboarding.UpdateOnboardingPlanResponse> UpdateOnboardingPlan(ApiOnboarding.UpdateOnboardingPlanRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling UpdateOnboardingPlan for {Tenant}", requestContext.TenantId);
        if (request.OnboardingPlan is { } record)
        {
            var snapshot = OnboardingPlanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.RecalculateManagedDevicesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.RecalculateManagedDevicesRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Locality = (request.OnboardingPlan?.BillingAddress?.Locality ?? ""),
                },
            },
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Address = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.OnboardingPlan?.BillingAddress?.CountryCode ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.OnboardingPlan?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        var command0 = new DispatchCommand<ApiDevices.RecalculateManagedDevicesRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _devicesManagedDeviceCalculationService.RecalculateManagedDevicesAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.ReconcileDocumentFoldersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ReconcileDocumentFoldersRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                Children = { new ApiDocuments.DocumentFolder
                {
                    Children = { new ApiDocuments.DocumentFolder
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            CreatedAt = (reply0.ManagedDevice?.Audit?.CreatedAt ?? new Timestamp()),
                            UpdatedBy = (reply0.ManagedDevice?.Audit?.UpdatedBy ?? ""),
                        },
                        Location = new ApiCommon.PostalAddress
                        {
                            Region = (reply0.ManagedDevice ?? new ApiDevices.ManagedDevice()).LocatorCase switch
                            {
                                ApiDevices.ManagedDevice.LocatorOneofCase.Address => (reply0.ManagedDevice?.Address?.Region ?? ""),
                                _ => ""
                            },
                        },
                    } },
                } },
            },
        };
        using var rpc1 = _documentsDocumentFolderSyncService.ReconcileDocumentFoldersAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiOnboarding.ExplainOnboardingPlanCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.OnboardingPlan?.Audit?.CreatedBy ?? ""),
                },
                Bank = new ApiOnboarding.OnboardingPlanBankSettlement
                {
                    AccountHolder = (request.OnboardingPlan ?? new ApiOnboarding.OnboardingPlan()).PaymentCase switch
                    {
                        ApiOnboarding.OnboardingPlan.SettlementOneofCase.Bank => (request.OnboardingPlan?.Bank?.AccountHolder ?? ""),
                        _ => ""
                    },
                    Bic = (request.OnboardingPlan ?? new ApiOnboarding.OnboardingPlan()).PaymentCase switch
                    {
                        ApiOnboarding.OnboardingPlan.SettlementOneofCase.Bank => (request.OnboardingPlan?.Bank?.Bic ?? ""),
                        _ => ""
                    },
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (reply0.ManagedDevice ?? new ApiDevices.ManagedDevice()).LocatorCase switch
                    {
                        ApiDevices.ManagedDevice.LocatorOneofCase.Address => (reply0.ManagedDevice?.Address?.Locality ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc2 = _onboardingOnboardingPlanCalculationService.ExplainOnboardingPlanCalculationAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiOnboarding.UpdateOnboardingPlanResponse
        {
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (reply1.DocumentFolder?.Location?.Locality ?? ""),
                },
                Counterparty = new ApiOnboarding.EquipmentRef
                {
                    DisplayName = (reply0.ManagedDevice?.Owner?.DisplayName ?? ""),
                },
                OnboardingPlanId = (request.OnboardingPlan?.OnboardingPlanId ?? ""),
                Status = (request.OnboardingPlan?.Status ?? (ApiOnboarding.OnboardingPlanStatus)0),
                Total = (reply2.OnboardingPlan?.Total ?? new ApiCommon.Money()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.OnboardingPlan?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan",
            "audit", "UpdateOnboardingPlan", (request.OnboardingPlan?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams the changes of onboarding plans as they happen.
    /// </summary>
    public override async Task WatchOnboardingPlans(ApiOnboarding.WatchOnboardingPlansRequest request, IServerStreamWriter<ApiOnboarding.WatchOnboardingPlansResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling WatchOnboardingPlans for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan", "batch",
            "WatchOnboardingPlans", "accepted", token);
        var input0 = request.WatchOnboardingPlansToCompareServiceAssetOptions0();
        using var rpc0 = _itsmServiceAssetCalculationService.CompareServiceAssetOptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiAppointments.SummarizeAppointmentsRequest
        {
            Annotations = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiAppointments.SummarizeAppointmentsRequest.Types.Details
            {
                Filters = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        var reply1 = await SendWatchOnboardingPlans1Async(input1, token);
        var response = new ApiOnboarding.WatchOnboardingPlansResponse
        {
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.ServiceAsset?.Audit?.CreatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply0.ServiceAsset?.Address?.PostalCode ?? ""),
                    Region = (request.Filter?.Region ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Subscriber?.PreferredChannel ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan",
            "audit", "WatchOnboardingPlans", (request.Subscriber?.PreferredChannel ?? ""), token);
        await responses.WriteAsync(response, token);
    }
    private async Task<ApiAppointments.SummarizeAppointmentsResponse> SendWatchOnboardingPlans1Async(ApiAppointments.SummarizeAppointmentsRequest request, CancellationToken token)
    {
        return await _onboardingPlanEndpointSendWatchOnboardingPlans1Port.SendAsync(request, token);
    }
}
