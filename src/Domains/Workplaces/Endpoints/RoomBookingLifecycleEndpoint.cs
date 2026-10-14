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
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiRecommendations = Acme.Recommendations.V1;
using ApiSearch = Acme.Search.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

/// <summary>
/// Moves a room booking between the states of its lifecycle.
/// </summary>
public sealed class RoomBookingLifecycleEndpoint : ApiWorkplaces.RoomBookingLifecycleService.RoomBookingLifecycleServiceBase
{
    private readonly ApiCorporateevents.RegistrationApprovalService.RegistrationApprovalServiceClient _corporateeventsRegistrationApprovalService;
    private readonly ApiCorporateevents.RegistrationWorkflowService.RegistrationWorkflowServiceClient _corporateeventsRegistrationWorkflowService;
    private readonly ApiDocuments.DocumentTemplateHistoryService.DocumentTemplateHistoryServiceClient _documentsDocumentTemplateHistoryService;
    private readonly ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient _employeesEmployeeQueryService;
    private readonly ApiEmployees.EmployeeService.EmployeeServiceClient _employeesEmployeeService;
    private readonly ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient _employeesEmploymentContractCalculationService;
    private readonly ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient _employeesOrgUnitQueryService;
    private readonly ApiEmployees.PositionValidationService.PositionValidationServiceClient _employeesPositionValidationService;
    private readonly CallInvoker _invoker;
    private readonly ApiLedger.LedgerAccountValidationService.LedgerAccountValidationServiceClient _ledgerLedgerAccountValidationService;
    private readonly ILogger<RoomBookingLifecycleEndpoint> _logger;
    private readonly ApiMetrics.MetricSeriesService.MetricSeriesServiceClient _metricsMetricSeriesService;
    private readonly ApiMetrics.MetricSeriesValidationService.MetricSeriesValidationServiceClient _metricsMetricSeriesValidationService;
    private readonly ApiNotifications.NotificationService.NotificationServiceClient _notificationsNotificationService;
    private readonly ApiRecommendations.RecommendationSetQueryService.RecommendationSetQueryServiceClient _recommendationsRecommendationSetQueryService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly IRoomBookingLifecycleEndpointSendTerminateRoomBooking2Port _roomBookingLifecycleEndpointSendTerminateRoomBooking2Port;
    private readonly ApiSearch.SearchIndexQueryService.SearchIndexQueryServiceClient _searchSearchIndexQueryService;
    private readonly ApiSearch.SearchIndexService.SearchIndexServiceClient _searchSearchIndexService;
    private readonly Func<ApiSearch.BatchGetSearchIndexesRequest, CallOptions, AsyncUnaryCall<ApiSearch.BatchGetSearchIndexesResponse>> _sendRenewRoomBooking1;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiWorkplaces.DeskCalculationService.DeskCalculationServiceClient _workplacesDeskCalculationService;
    private readonly ApiWorkplaces.VisitorPassReportingService.VisitorPassReportingServiceClient _workplacesVisitorPassReportingService;
    private readonly ApiWorkplaces.VisitorPassValidationService.VisitorPassValidationServiceClient _workplacesVisitorPassValidationService;

    public RoomBookingLifecycleEndpoint(
        ApiCorporateevents.RegistrationApprovalService.RegistrationApprovalServiceClient corporateeventsRegistrationApprovalService,
        ApiCorporateevents.RegistrationWorkflowService.RegistrationWorkflowServiceClient corporateeventsRegistrationWorkflowService,
        ApiDocuments.DocumentTemplateHistoryService.DocumentTemplateHistoryServiceClient documentsDocumentTemplateHistoryService,
        ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient employeesEmployeeQueryService,
        ApiEmployees.EmployeeService.EmployeeServiceClient employeesEmployeeService,
        ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient employeesEmploymentContractCalculationService,
        ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient employeesOrgUnitQueryService,
        ApiEmployees.PositionValidationService.PositionValidationServiceClient employeesPositionValidationService,
        CallInvoker invoker,
        ApiLedger.LedgerAccountValidationService.LedgerAccountValidationServiceClient ledgerLedgerAccountValidationService,
        ILogger<RoomBookingLifecycleEndpoint> logger,
        ApiMetrics.MetricSeriesService.MetricSeriesServiceClient metricsMetricSeriesService,
        ApiMetrics.MetricSeriesValidationService.MetricSeriesValidationServiceClient metricsMetricSeriesValidationService,
        ApiNotifications.NotificationService.NotificationServiceClient notificationsNotificationService,
        ApiRecommendations.RecommendationSetQueryService.RecommendationSetQueryServiceClient recommendationsRecommendationSetQueryService,
        ClientRegistry registry,
        IRecordRepository repository,
        IRoomBookingLifecycleEndpointSendTerminateRoomBooking2Port roomBookingLifecycleEndpointSendTerminateRoomBooking2PortAdapter,
        ApiSearch.SearchIndexQueryService.SearchIndexQueryServiceClient searchSearchIndexQueryService,
        ApiSearch.SearchIndexService.SearchIndexServiceClient searchSearchIndexService,
        IOptions<MeshOptions> settings,
        ApiWorkplaces.DeskCalculationService.DeskCalculationServiceClient workplacesDeskCalculationService,
        ApiWorkplaces.VisitorPassReportingService.VisitorPassReportingServiceClient workplacesVisitorPassReportingService,
        ApiWorkplaces.VisitorPassValidationService.VisitorPassValidationServiceClient workplacesVisitorPassValidationService)
    {
        _corporateeventsRegistrationApprovalService = corporateeventsRegistrationApprovalService;
        _corporateeventsRegistrationWorkflowService = corporateeventsRegistrationWorkflowService;
        _documentsDocumentTemplateHistoryService = documentsDocumentTemplateHistoryService;
        _employeesEmployeeQueryService = employeesEmployeeQueryService;
        _employeesEmployeeService = employeesEmployeeService;
        _employeesEmploymentContractCalculationService = employeesEmploymentContractCalculationService;
        _employeesOrgUnitQueryService = employeesOrgUnitQueryService;
        _employeesPositionValidationService = employeesPositionValidationService;
        _invoker = invoker;
        _ledgerLedgerAccountValidationService = ledgerLedgerAccountValidationService;
        _logger = logger;
        _metricsMetricSeriesService = metricsMetricSeriesService;
        _metricsMetricSeriesValidationService = metricsMetricSeriesValidationService;
        _notificationsNotificationService = notificationsNotificationService;
        _recommendationsRecommendationSetQueryService = recommendationsRecommendationSetQueryService;
        _registry = registry;
        _repository = repository;
        _roomBookingLifecycleEndpointSendTerminateRoomBooking2Port = roomBookingLifecycleEndpointSendTerminateRoomBooking2PortAdapter;
        _searchSearchIndexQueryService = searchSearchIndexQueryService;
        _searchSearchIndexService = searchSearchIndexService;
        _settings = settings;
        _workplacesDeskCalculationService = workplacesDeskCalculationService;
        _workplacesVisitorPassReportingService = workplacesVisitorPassReportingService;
        _workplacesVisitorPassValidationService = workplacesVisitorPassValidationService;
        _sendRenewRoomBooking1 = _searchSearchIndexService.BatchGetSearchIndexesAsync;
    }
    /// <summary>
    /// Activates a room booking.
    /// </summary>
    public override async Task<ApiWorkplaces.ActivateRoomBookingResponse> ActivateRoomBooking(ApiWorkplaces.ActivateRoomBookingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.RoomBookingId, "room_booking_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ActivateRoomBooking for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "room_booking", request.RoomBookingId,
            "ActivateRoomBooking", "accepted", token);
        var profile0 = new MappingProfile<ApiWorkplaces.ActivateRoomBookingRequest, ApiWorkplaces.PreviewVisitorPassRequest>(value => new ApiWorkplaces.PreviewVisitorPassRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiWorkplaces.PreviewVisitorPassRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (value.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                    PreferredChannel = (value.Details?.NotifyContact?.PreferredChannel ?? ""),
                },
            },
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (value.Details?.NotifyContact?.Email ?? ""),
                },
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _workplacesVisitorPassValidationService.PreviewVisitorPassAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLedger.ScoreLedgerAccountRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLedger.ScoreLedgerAccountRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.VisitorPass?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
            LedgerAccount = new ApiLedger.LedgerAccount
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.VisitorPass?.Audit?.Etag ?? ""),
                },
            },
        };
        input1 = PropertyMap.Copy(input1);
        using var rpc1 = _ledgerLedgerAccountValidationService.ScoreLedgerAccountAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDocuments.AnnotateDocumentTemplateVersionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.AnnotateDocumentTemplateVersionRequest.Types.Details
            {
                Snapshot = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply1.LedgerAccount?.Audit?.CreatedBy ?? ""),
                },
            },
        };
        using var rpc2 = _documentsDocumentTemplateHistoryService.AnnotateDocumentTemplateVersionAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiWorkplaces.ActivateRoomBookingResponse
        {
            Outcome = new ApiWorkplaces.ActivateRoomBookingResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply2.Outcome?.Status ?? ""),
            },
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.LedgerAccount?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Location = new ApiCommon.PostalAddress
                {
                    Recipient = (reply0.VisitorPass?.BillingAddress?.Recipient ?? ""),
                },
                Organizer = new ApiIdentity.UserRef
                {
                    DisplayName = (reply2.DocumentTemplate?.Counterparty?.DisplayName ?? ""),
                },
                Participants = { new ApiWorkplaces.RoomBookingParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (reply0.VisitorPass?.Counterparty?.Email ?? ""),
                    },
                } },
                RoomBookingId = (request.RoomBookingId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "room_booking",
            "audit", "ActivateRoomBooking", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Expires a room booking.
    /// </summary>
    public override async Task<ApiWorkplaces.ExpireRoomBookingResponse> ExpireRoomBooking(ApiWorkplaces.ExpireRoomBookingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.RoomBookingId, "room_booking_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ExpireRoomBooking for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "room_booking", request.RoomBookingId,
            "ExpireRoomBooking", "accepted", token);
        var input0 = new ApiWorkplaces.CountVisitorPassTotalsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiWorkplaces.CountVisitorPassTotalsRequest.Types.Filter
            {
                CreatedAfter = (request.Details?.EffectiveAt ?? new Timestamp()),
                Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.RoomBookingId ?? ""),
            },
        };
        using var rpc0 = _workplacesVisitorPassReportingService.CountVisitorPassTotalsAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiRecommendations.FindRecommendationSetsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiRecommendations.FindRecommendationSetsRequest.Types.Filter
            {
                CreatedAfter = (request.Details?.EffectiveAt ?? new Timestamp()),
                OwnerId = (request.Details?.NewOwner?.UserId ?? ""),
            },
        };
        using var rpc1 = _recommendationsRecommendationSetQueryService.FindRecommendationSetsAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var response = new ApiWorkplaces.ExpireRoomBookingResponse
        {
            Outcome = new ApiWorkplaces.ExpireRoomBookingResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply1.Result?.Message ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                RoomBookingId = (request.RoomBookingId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "room_booking",
            "audit", "ExpireRoomBooking", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a room booking.
    /// </summary>
    public override async Task<ApiWorkplaces.GetRoomBookingLifecycleResponse> GetRoomBookingLifecycle(ApiWorkplaces.GetRoomBookingLifecycleRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.RoomBookingId, "room_booking_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetRoomBookingLifecycle for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "room_booking", request.RoomBookingId,
            "GetRoomBookingLifecycle", "accepted", token);
        var input0 = new ApiEmployees.WatchEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.WatchEmployeesRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
            },
            Subscriber = new ApiCommon.ContactInfo
            {
                Email = (request.Context?.Actor?.Email ?? ""),
            },
        };
        using var stream0 = _employeesEmployeeService.WatchEmployees(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received WatchEmployees frame {Type}", change0.GetType().Name);
        }
        var input1 = new ApiEmployees.SuggestEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.SuggestEmployeesRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
            },
        };
        var json1 = JsonFormatter.Default.Format(input1);
        input1 = JsonParser.Default.Parse<ApiEmployees.SuggestEmployeesRequest>(json1);
        using var rpc1 = _employeesEmployeeQueryService.SuggestEmployeesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiWorkplaces.GetRoomBookingLifecycleResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                RoomBookingId = (request.RoomBookingId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Reinstates a room booking.
    /// </summary>
    public override async Task<ApiWorkplaces.ReinstateRoomBookingResponse> ReinstateRoomBooking(ApiWorkplaces.ReinstateRoomBookingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.RoomBookingId, "room_booking_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ReinstateRoomBooking for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "room_booking", request.RoomBookingId,
            "ReinstateRoomBooking", "accepted", token);
        var input0 = new ApiEmployees.QuoteEmploymentContractRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.EmploymentContract = new ApiEmployees.EmploymentContract
        {
            Counterparty = new ApiEmployees.EmployeeRef
            {
                DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                Email = (request.Details?.NewOwner?.Email ?? ""),
            },
        };
        using var rpc0 = _employeesEmploymentContractCalculationService.QuoteEmploymentContractAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.ResolveOrgUnitsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc1 = _employeesOrgUnitQueryService.ResolveOrgUnitsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiMetrics.PreviewMetricSeriesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiMetrics.PreviewMetricSeriesRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                    Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                },
            },
            MetricSeries = new ApiMetrics.MetricSeries
            {
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                    Email = (reply0.EmploymentContract?.Counterparty?.Email ?? ""),
                },
            },
        };
        var reply2 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _metricsMetricSeriesValidationService.PreviewMetricSeriesAsync(input2, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input3 = new ApiSearch.ExportSearchIndexesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiSearch.ExportSearchIndexesRequest.Types.Filter
            {
                CreatedAfter = (reply0.EmploymentContract?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (reply2.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply2.MetricSeries?.Owner?.UserId ?? ""),
                Region = (reply2.MetricSeries?.Address?.Region ?? ""),
            },
        };
        using var stream3 = _searchSearchIndexQueryService.ExportSearchIndexes(input3, cancellationToken: token);
        await foreach (var change3 in stream3.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportSearchIndexes frame {Type}", change3.GetType().Name);
        }
        var response = new ApiWorkplaces.ReinstateRoomBookingResponse
        {
            Outcome = new ApiWorkplaces.ReinstateRoomBookingResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Message = (reply1.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                ReferenceIds = { (reply2.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                Location = new ApiCommon.PostalAddress
                {
                    Region = (reply0.EmploymentContract?.BillingAddress?.Region ?? ""),
                },
                RoomBookingId = (request.RoomBookingId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "room_booking",
            "audit", "ReinstateRoomBooking", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Renews a room booking.
    /// </summary>
    public override async Task<ApiWorkplaces.RenewRoomBookingResponse> RenewRoomBooking(ApiWorkplaces.RenewRoomBookingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.RoomBookingId, "room_booking_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RenewRoomBooking for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "room_booking", request.RoomBookingId,
            "RenewRoomBooking", "accepted", token);
        var input0 = new ApiNotifications.BatchGetNotificationsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply0 = await Workflow.SendAsync(input0, token, _notificationsNotificationService.BatchGetNotificationsAsync);
        var input1 = new ApiSearch.BatchGetSearchIndexesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc1 = _sendRenewRoomBooking1(input1, new CallOptions(cancellationToken: token));
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiWorkplaces.RenewRoomBookingResponse
        {
            Outcome = new ApiWorkplaces.RenewRoomBookingResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                RoomBookingId = (request.RoomBookingId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "room_booking",
            "audit", "RenewRoomBooking", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Schedules a room booking.
    /// </summary>
    public override async Task<ApiWorkplaces.ScheduleRoomBookingExpiryResponse> ScheduleRoomBookingExpiry(ApiWorkplaces.ScheduleRoomBookingExpiryRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ScheduleRoomBookingExpiry for {Tenant}", requestContext.TenantId);
        if (request.RoomBooking is { } record)
        {
            var snapshot = RoomBookingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.BrowseOrgUnitsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.BrowseOrgUnitsRequest.Types.Filter
            {
                Region = (request.RoomBooking?.Location?.Region ?? ""),
            },
        };
        var named0 = _registry.Get<ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient>("acme.employees.v1.OrgUnitQueryService");
        using var rpc0 = named0.BrowseOrgUnitsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.SummarizeEmployeesRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var descriptor1 = new Method<ApiEmployees.SummarizeEmployeesRequest, ApiEmployees.SummarizeEmployeesResponse>(
            MethodType.Unary, "acme.employees.v1" + ".EmployeeQueryService", "SummarizeEmployees",
            Wire.Marshaller<ApiEmployees.SummarizeEmployeesRequest>(), Wire.Marshaller<ApiEmployees.SummarizeEmployeesResponse>());
        using var rpc1 = _invoker.AsyncUnaryCall(descriptor1, null,
            new CallOptions(cancellationToken: token), input1);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiWorkplaces.ScheduleRoomBookingExpiryResponse
        {
            Outcome = new ApiWorkplaces.ScheduleRoomBookingExpiryResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                RoomBookingId = (request.RoomBookingId ?? ""),
                Window = (request.RoomBooking?.Window ?? new ApiCommon.TimeWindow()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.RoomBooking?.Title ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "room_booking",
            "audit", "ScheduleRoomBookingExpiry", (request.RoomBooking?.Title ?? ""), token);
        return response;
    }
    /// <summary>
    /// Suspends a room booking.
    /// </summary>
    public override async Task<ApiWorkplaces.SuspendRoomBookingResponse> SuspendRoomBooking(ApiWorkplaces.SuspendRoomBookingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.RoomBookingId, "room_booking_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SuspendRoomBooking for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "room_booking", request.RoomBookingId,
            "SuspendRoomBooking", "accepted", token);
        var input0 = new ApiCorporateevents.AdvanceRegistrationWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.AdvanceRegistrationWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                },
            },
            Registration = new ApiCorporateevents.Registration
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                    Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                },
            },
        };
        var command0 = new DispatchCommand<ApiCorporateevents.AdvanceRegistrationWorkflowRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _corporateeventsRegistrationWorkflowService.AdvanceRegistrationWorkflowAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var profile1 = new MappingProfile<ApiWorkplaces.SuspendRoomBookingRequest, ApiEmployees.ScorePositionRequest>(value => new ApiEmployees.ScorePositionRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Position = new ApiEmployees.Position
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply0.Registration?.Contact?.Email ?? ""),
                },
            },
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _employeesPositionValidationService.ScorePositionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiWorkplaces.SuspendRoomBookingResponse
        {
            Outcome = new ApiWorkplaces.SuspendRoomBookingResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply1.Result?.Status ?? ""),
            },
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.Registration?.Audit?.CreatedBy ?? ""),
                },
                RoomBookingId = (request.RoomBookingId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "room_booking",
            "audit", "SuspendRoomBooking", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Terminates a room booking.
    /// </summary>
    public override async Task<ApiWorkplaces.TerminateRoomBookingResponse> TerminateRoomBooking(ApiWorkplaces.TerminateRoomBookingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.RoomBookingId, "room_booking_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling TerminateRoomBooking for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "room_booking", request.RoomBookingId,
            "TerminateRoomBooking", "accepted", token);
        var input0 = new ApiCorporateevents.ApproveRegistrationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.ApproveRegistrationRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                },
            },
            Registration = new ApiCorporateevents.Registration
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                    PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _corporateeventsRegistrationApprovalService.ApproveRegistrationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = request.TerminateRoomBookingToEstimateDesk1(reply0);
        using var rpc1 = _workplacesDeskCalculationService.EstimateDeskAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiLedger.NormalizeLedgerAccountRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LedgerAccount = new ApiLedger.LedgerAccount
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.Registration?.Audit?.CreatedBy ?? ""),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (reply0.Registration?.Contact?.Name ?? new ApiCommon.PersonName()),
                },
            },
        };
        var reply2 = await SendTerminateRoomBooking2Async(input2, token);
        var input3 = new ApiMetrics.BatchCreateMetricSeriesesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Record = new ApiMetrics.MetricSeries
            {
                Address = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply1.Desk?.Address?.PostalCode ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.Desk?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (reply1.Desk?.Owner?.UserId ?? ""),
                },
            },
        };
        using var stream3 = _metricsMetricSeriesService.BatchCreateMetricSerieses(cancellationToken: token);
        await stream3.RequestStream.WriteAsync(input3, token);
        await stream3.RequestStream.CompleteAsync();
        var reply3 = await stream3.ResponseAsync;
        var response = new ApiWorkplaces.TerminateRoomBookingResponse
        {
            Outcome = new ApiWorkplaces.TerminateRoomBookingResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Message = (reply1.Result?.Message ?? ""),
                ReferenceId = (reply2.Outcome?.ReferenceId ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                Location = new ApiCommon.PostalAddress
                {
                    Locality = (reply1.Desk?.Address?.Locality ?? ""),
                },
                RoomBookingId = (request.RoomBookingId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "room_booking",
            "audit", "TerminateRoomBooking", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiLedger.NormalizeLedgerAccountResponse> SendTerminateRoomBooking2Async(ApiLedger.NormalizeLedgerAccountRequest request, CancellationToken token)
    {
        return await _roomBookingLifecycleEndpointSendTerminateRoomBooking2Port.SendAsync(request, token);
    }
}
