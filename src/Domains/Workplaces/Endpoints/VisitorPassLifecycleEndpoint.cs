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
using ApiBanking = Acme.Banking.V1;
using ApiChat = Acme.Chat.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiSearch = Acme.Search.V1;
using ApiTelehealth = Acme.Telehealth.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

/// <summary>
/// Moves a visitor pass between the states of its lifecycle.
/// </summary>
public sealed class VisitorPassLifecycleEndpoint : ApiWorkplaces.VisitorPassLifecycleService.VisitorPassLifecycleServiceBase
{
    private readonly ApiBanking.BankAccountService.BankAccountServiceClient _bankingBankAccountService;
    private readonly NotificationBus _bus;
    private readonly ApiChat.ChatRoomCalculationService.ChatRoomCalculationServiceClient _chatChatRoomCalculationService;
    private readonly ApiCorporateevents.VenueSyncService.VenueSyncServiceClient _corporateeventsVenueSyncService;
    private readonly ApiDevices.DevicePolicyWorkflowService.DevicePolicyWorkflowServiceClient _devicesDevicePolicyWorkflowService;
    private readonly ApiDocuments.DocumentFolderExportService.DocumentFolderExportServiceClient _documentsDocumentFolderExportService;
    private readonly ApiDocuments.DocumentFolderSyncService.DocumentFolderSyncServiceClient _documentsDocumentFolderSyncService;
    private readonly ApiDocuments.SignatureExportService.SignatureExportServiceClient _documentsSignatureExportService;
    private readonly ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient _employeesEmploymentContractValidationService;
    private readonly ApiEmployees.PositionValidationService.PositionValidationServiceClient _employeesPositionValidationService;
    private readonly ApiHelpdesk.KnowledgeArticleWorkflowService.KnowledgeArticleWorkflowServiceClient _helpdeskKnowledgeArticleWorkflowService;
    private readonly CallInvoker _invoker;
    private readonly ILogger<VisitorPassLifecycleEndpoint> _logger;
    private readonly ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient _metricsMetricSeriesQueryService;
    private readonly RetryPolicy _policy;
    private readonly IRecordRepository _repository;
    private readonly ApiSearch.SearchIndexValidationService.SearchIndexValidationServiceClient _searchSearchIndexValidationService;
    private readonly Func<ApiEmployees.ExplainPositionDecisionRequest, CallOptions, AsyncUnaryCall<ApiEmployees.ExplainPositionDecisionResponse>> _sendReinstateVisitorPass1;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTelehealth.VideoVisitCalculationService.VideoVisitCalculationServiceClient _telehealthVideoVisitCalculationService;
    private readonly ApiWorkplaces.BuildingValidationService.BuildingValidationServiceClient _workplacesBuildingValidationService;
    private readonly ApiWorkplaces.DeskCalculationService.DeskCalculationServiceClient _workplacesDeskCalculationService;
    private readonly ApiWorkplaces.RoomBookingLifecycleService.RoomBookingLifecycleServiceClient _workplacesRoomBookingLifecycleService;
    private readonly ApiWorkplaces.VisitorPassCalculationService.VisitorPassCalculationServiceClient _workplacesVisitorPassCalculationService;
    private readonly ApiWorkplaces.VisitorPassSyncService.VisitorPassSyncServiceClient _workplacesVisitorPassSyncService;

    public VisitorPassLifecycleEndpoint(
        ApiBanking.BankAccountService.BankAccountServiceClient bankingBankAccountService,
        NotificationBus bus,
        ApiChat.ChatRoomCalculationService.ChatRoomCalculationServiceClient chatChatRoomCalculationService,
        ApiCorporateevents.VenueSyncService.VenueSyncServiceClient corporateeventsVenueSyncService,
        ApiDevices.DevicePolicyWorkflowService.DevicePolicyWorkflowServiceClient devicesDevicePolicyWorkflowService,
        ApiDocuments.DocumentFolderExportService.DocumentFolderExportServiceClient documentsDocumentFolderExportService,
        ApiDocuments.DocumentFolderSyncService.DocumentFolderSyncServiceClient documentsDocumentFolderSyncService,
        ApiDocuments.SignatureExportService.SignatureExportServiceClient documentsSignatureExportService,
        ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient employeesEmploymentContractValidationService,
        ApiEmployees.PositionValidationService.PositionValidationServiceClient employeesPositionValidationService,
        ApiHelpdesk.KnowledgeArticleWorkflowService.KnowledgeArticleWorkflowServiceClient helpdeskKnowledgeArticleWorkflowService,
        CallInvoker invoker,
        ILogger<VisitorPassLifecycleEndpoint> logger,
        ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient metricsMetricSeriesQueryService,
        RetryPolicy policy,
        IRecordRepository repository,
        ApiSearch.SearchIndexValidationService.SearchIndexValidationServiceClient searchSearchIndexValidationService,
        IOptions<MeshOptions> settings,
        ApiTelehealth.VideoVisitCalculationService.VideoVisitCalculationServiceClient telehealthVideoVisitCalculationService,
        ApiWorkplaces.BuildingValidationService.BuildingValidationServiceClient workplacesBuildingValidationService,
        ApiWorkplaces.DeskCalculationService.DeskCalculationServiceClient workplacesDeskCalculationService,
        ApiWorkplaces.RoomBookingLifecycleService.RoomBookingLifecycleServiceClient workplacesRoomBookingLifecycleService,
        ApiWorkplaces.VisitorPassCalculationService.VisitorPassCalculationServiceClient workplacesVisitorPassCalculationService,
        ApiWorkplaces.VisitorPassSyncService.VisitorPassSyncServiceClient workplacesVisitorPassSyncService)
    {
        _bankingBankAccountService = bankingBankAccountService;
        _bus = bus;
        _chatChatRoomCalculationService = chatChatRoomCalculationService;
        _corporateeventsVenueSyncService = corporateeventsVenueSyncService;
        _devicesDevicePolicyWorkflowService = devicesDevicePolicyWorkflowService;
        _documentsDocumentFolderExportService = documentsDocumentFolderExportService;
        _documentsDocumentFolderSyncService = documentsDocumentFolderSyncService;
        _documentsSignatureExportService = documentsSignatureExportService;
        _employeesEmploymentContractValidationService = employeesEmploymentContractValidationService;
        _employeesPositionValidationService = employeesPositionValidationService;
        _helpdeskKnowledgeArticleWorkflowService = helpdeskKnowledgeArticleWorkflowService;
        _invoker = invoker;
        _logger = logger;
        _metricsMetricSeriesQueryService = metricsMetricSeriesQueryService;
        _policy = policy;
        _repository = repository;
        _searchSearchIndexValidationService = searchSearchIndexValidationService;
        _settings = settings;
        _telehealthVideoVisitCalculationService = telehealthVideoVisitCalculationService;
        _workplacesBuildingValidationService = workplacesBuildingValidationService;
        _workplacesDeskCalculationService = workplacesDeskCalculationService;
        _workplacesRoomBookingLifecycleService = workplacesRoomBookingLifecycleService;
        _workplacesVisitorPassCalculationService = workplacesVisitorPassCalculationService;
        _workplacesVisitorPassSyncService = workplacesVisitorPassSyncService;
        _sendReinstateVisitorPass1 = _employeesPositionValidationService.ExplainPositionDecisionAsync;
    }
    /// <summary>
    /// Activates a visitor pass.
    /// </summary>
    public override async Task<ApiWorkplaces.ActivateVisitorPassResponse> ActivateVisitorPass(ApiWorkplaces.ActivateVisitorPassRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.VisitorPassId, "visitor_pass_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ActivateVisitorPass for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass", request.VisitorPassId,
            "ActivateVisitorPass", "accepted", token);
        var input0 = new ApiMetrics.CountMetricSeriesesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiMetrics.CountMetricSeriesesRequest.Types.Filter
            {
                OwnerId = (request.Details?.NewOwner?.UserId ?? ""),
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _metricsMetricSeriesQueryService.CountMetricSeriesesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.NormalizePositionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.NormalizePositionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                },
            },
        };
        var packed1 = Any.Pack(input1);
        input1 = packed1.Unpack<ApiEmployees.NormalizePositionRequest>();
        using var rpc1 = _employeesPositionValidationService.NormalizePositionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiWorkplaces.CompareDeskOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Desk = new ApiWorkplaces.Desk
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.Position?.Audit?.Etag ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply1.Position?.Owner?.Email ?? ""),
                    UserId = (reply1.Position?.Owner?.UserId ?? ""),
                },
            },
        };
        var json2 = JsonFormatter.Default.Format(input2);
        input2 = JsonParser.Default.Parse<ApiWorkplaces.CompareDeskOptionsRequest>(json2);
        using var rpc2 = _workplacesDeskCalculationService.CompareDeskOptionsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiWorkplaces.LintBuildingsRequest();
        input3.Building = new ApiWorkplaces.Building
        {
            Children = { new ApiWorkplaces.Building
            {
                Children = { new ApiWorkplaces.Building
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedAt = (reply1.Position?.Audit?.CreatedAt ?? new Timestamp()),
                    },
                } },
            } },
        };
        input3.Context = (request.Context ?? new ApiCommon.RequestContext());
        using var rpc3 = _workplacesBuildingValidationService.LintBuildingsAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiWorkplaces.ActivateVisitorPassResponse
        {
            Outcome = new ApiWorkplaces.ActivateVisitorPassResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply3.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply3.Building?.Audit?.CreatedAt ?? new Timestamp()),
                },
                VisitorPassId = (request.VisitorPassId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass",
            "audit", "ActivateVisitorPass", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Expires a visitor pass.
    /// </summary>
    public override async Task<ApiWorkplaces.ExpireVisitorPassResponse> ExpireVisitorPass(ApiWorkplaces.ExpireVisitorPassRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.VisitorPassId, "visitor_pass_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ExpireVisitorPass for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass", request.VisitorPassId,
            "ExpireVisitorPass", "accepted", token);
        var input0 = new ApiDocuments.SyncDocumentFoldersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.SyncDocumentFoldersRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                Children = { new ApiDocuments.DocumentFolder
                {
                    Children = { new ApiDocuments.DocumentFolder
                    {
                        Manager = new ApiIdentity.UserRef
                        {
                            Email = (request.Details?.NotifyContact?.Email ?? ""),
                            UserId = (request.Details?.NewOwner?.UserId ?? ""),
                        },
                    } },
                    Manager = new ApiIdentity.UserRef
                    {
                        Email = (request.Details?.NewOwner?.Email ?? ""),
                    },
                } },
            },
        };
        using var rpc0 = _documentsDocumentFolderSyncService.SyncDocumentFoldersAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTelehealth.QuoteVideoVisitRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            VideoVisit = new ApiTelehealth.VideoVisit
            {
                Location = new ApiCommon.PostalAddress
                {
                    Recipient = (reply0.DocumentFolder?.Location?.Recipient ?? ""),
                },
                Organizer = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.DocumentFolder?.Manager?.DisplayName ?? ""),
                },
                Participants = { new ApiTelehealth.VideoVisitParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (reply0.DocumentFolder?.Manager?.Email ?? ""),
                    },
                } },
            },
        };
        var reply1 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _telehealthVideoVisitCalculationService.QuoteVideoVisitAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input2 = new ApiDocuments.ListDocumentFolderConflictsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListDocumentFolderConflictsRequest.Types.Filter
            {
                CreatedAfter = (reply0.DocumentFolder?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply0.DocumentFolder?.Manager?.UserId ?? ""),
                Region = (reply0.DocumentFolder?.Location?.Region ?? ""),
            },
        };
        var reply2 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _documentsDocumentFolderSyncService.ListDocumentFolderConflictsAsync(input2, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiWorkplaces.ExpireVisitorPassResponse
        {
            Outcome = new ApiWorkplaces.ExpireVisitorPassResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                ReferenceId = (reply0.Outcome?.ReferenceId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
            },
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.VideoVisit?.Audit?.CreatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (reply0.DocumentFolder?.Location?.Recipient ?? ""),
                },
                VisitorPassId = (request.VisitorPassId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass",
            "audit", "ExpireVisitorPass", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a visitor pass.
    /// </summary>
    public override async Task<ApiWorkplaces.GetVisitorPassLifecycleResponse> GetVisitorPassLifecycle(ApiWorkplaces.GetVisitorPassLifecycleRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.VisitorPassId, "visitor_pass_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetVisitorPassLifecycle for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass", request.VisitorPassId,
            "GetVisitorPassLifecycle", "accepted", token);
        var response = new ApiWorkplaces.GetVisitorPassLifecycleResponse
        {
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                VisitorPassId = (request.VisitorPassId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Reinstates a visitor pass.
    /// </summary>
    public override async Task<ApiWorkplaces.ReinstateVisitorPassResponse> ReinstateVisitorPass(ApiWorkplaces.ReinstateVisitorPassRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.VisitorPassId, "visitor_pass_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ReinstateVisitorPass for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass", request.VisitorPassId,
            "ReinstateVisitorPass", "accepted", token);
        var input0 = new ApiWorkplaces.ResolveVisitorPassConflictRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiWorkplaces.ResolveVisitorPassConflictRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Reason = (request.Reason ?? ""),
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                },
            },
            VisitorPassId = (request.VisitorPassId ?? ""),
        };
        using var rpc0 = _workplacesVisitorPassSyncService.ResolveVisitorPassConflictAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.ExplainPositionDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.ExplainPositionDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                },
            },
            Position = new ApiEmployees.Position
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                    UserId = (request.Details?.NewOwner?.UserId ?? ""),
                },
            },
        };
        using var rpc1 = _sendReinstateVisitorPass1(input1, new CallOptions(cancellationToken: token));
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiMetrics.SummarizeMetricSeriesesRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiMetrics.SummarizeMetricSeriesesRequest.Types.Details
            {
                Filters = { (reply1.Position?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Period = (reply1.Position?.Effective ?? new ApiCommon.TimeWindow()),
            },
            Reason = (reply1.Position ?? new ApiEmployees.Position()).ActionCase switch
            {
                ApiEmployees.Position.ActionOneofCase.Block => (reply1.Position?.Block?.Reason ?? ""),
                _ => ""
            },
        };
        using var rpc2 = _metricsMetricSeriesQueryService.SummarizeMetricSeriesesAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiWorkplaces.ReinstateVisitorPassResponse
        {
            Outcome = new ApiWorkplaces.ReinstateVisitorPassResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply2.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply1.Outcome?.Status ?? ""),
            },
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (reply0.VisitorPass?.Counterparty?.Email ?? ""),
                },
                VisitorPassId = (request.VisitorPassId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.ReasonCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass",
            "audit", "ReinstateVisitorPass", (request.Details?.ReasonCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Renews a visitor pass.
    /// </summary>
    public override async Task<ApiWorkplaces.RenewVisitorPassResponse> RenewVisitorPass(ApiWorkplaces.RenewVisitorPassRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.VisitorPassId, "visitor_pass_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RenewVisitorPass for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass", request.VisitorPassId,
            "RenewVisitorPass", "accepted", token);
        var input0 = new ApiWorkplaces.ScheduleRoomBookingExpiryRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                Participants = { new ApiWorkplaces.RoomBookingParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Details?.NotifyContact?.Email ?? ""),
                        Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                        PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                    },
                } },
            },
        };
        var descriptor0 = new Method<ApiWorkplaces.ScheduleRoomBookingExpiryRequest, ApiWorkplaces.ScheduleRoomBookingExpiryResponse>(
            MethodType.Unary, "acme.workplaces.v1" + ".RoomBookingLifecycleService", "ScheduleRoomBookingExpiry",
            Wire.Marshaller<ApiWorkplaces.ScheduleRoomBookingExpiryRequest>(), Wire.Marshaller<ApiWorkplaces.ScheduleRoomBookingExpiryResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiWorkplaces.ExplainVisitorPassCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.RoomBooking?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (reply0.RoomBooking?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                },
            },
        };
        var command1 = new DispatchCommand<ApiWorkplaces.ExplainVisitorPassCalculationRequest>(input1,
            "primary", requestContext.TenantId);
        var routed1 = command1 with { Region = _settings.Value.Region };
        using var rpc1 = _workplacesVisitorPassCalculationService.ExplainVisitorPassCalculationAsync(routed1.Payload, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiCorporateevents.PullVenuesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.PullVenuesRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Venue = new ApiCorporateevents.Venue
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply1.VisitorPass?.Audit?.UpdatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.Details?.NewOwner?.UserId ?? ""),
                },
            },
        };
        using var rpc2 = _corporateeventsVenueSyncService.PullVenuesAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiWorkplaces.RenewVisitorPassResponse
        {
            Outcome = new ApiWorkplaces.RenewVisitorPassResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Message = (reply0.Outcome?.Message ?? ""),
                NextSteps = { (reply1.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Outcome?.Message ?? ""),
            },
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply2.Venue?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                VisitorPassId = (request.VisitorPassId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass",
            "audit", "RenewVisitorPass", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Schedules a visitor pass.
    /// </summary>
    public override async Task<ApiWorkplaces.ScheduleVisitorPassExpiryResponse> ScheduleVisitorPassExpiry(ApiWorkplaces.ScheduleVisitorPassExpiryRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ScheduleVisitorPassExpiry for {Tenant}", requestContext.TenantId);
        if (request.VisitorPass is { } record)
        {
            var snapshot = VisitorPassMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiBanking.UpdateBankAccountRequest
        {
            BankAccount = new ApiBanking.BankAccount
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _bankingBankAccountService.UpdateBankAccountAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiWorkplaces.ScheduleVisitorPassExpiryResponse
        {
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                BillingAddress = (request.VisitorPass?.BillingAddress ?? new ApiCommon.PostalAddress()),
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (reply0.BankAccount?.Holder?.Email ?? ""),
                },
                VisitorPassId = (request.VisitorPassId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.VisitorPass?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass",
            "audit", "ScheduleVisitorPassExpiry", (request.VisitorPass?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Suspends a visitor pass.
    /// </summary>
    public override async Task<ApiWorkplaces.SuspendVisitorPassResponse> SuspendVisitorPass(ApiWorkplaces.SuspendVisitorPassRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.VisitorPassId, "visitor_pass_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SuspendVisitorPass for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass", request.VisitorPassId,
            "SuspendVisitorPass", "accepted", token);
        var input0 = request.SuspendVisitorPassToCheckSearchIndexEligibility0();
        using var rpc0 = _searchSearchIndexValidationService.CheckSearchIndexEligibilityAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiChat.CalculateChatRoomRequest
        {
            ChatRoom = new ApiChat.ChatRoom
            {
                Address = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.SearchIndex ?? new ApiSearch.SearchIndex()).LocatorCase switch
                    {
                        ApiSearch.SearchIndex.LocatorOneofCase.Address => (reply0.SearchIndex?.Address?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()),
                        _ => new Google.Protobuf.Collections.RepeatedField<string>()
                    } },
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiChat.CalculateChatRoomRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply0.SearchIndex ?? new ApiSearch.SearchIndex()).LocatorCase switch
                    {
                        ApiSearch.SearchIndex.LocatorOneofCase.Address => (reply0.SearchIndex?.Address?.PostalCode ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _chatChatRoomCalculationService.CalculateChatRoomAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiChat.CalculateChatRoomRequest>("acme.workplaces.v1.VisitorPassLifecycleService/SuspendVisitorPass/1", input1.Clone()), token);
        var input2 = new ApiDocuments.ListDocumentFolderExportsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListDocumentFolderExportsRequest.Types.Filter
            {
                CreatedAfter = (reply0.SearchIndex?.Audit?.UpdatedAt ?? new Timestamp()),
                OwnerId = (reply0.Result?.CorrelationId ?? ""),
                Region = (reply1.ChatRoom ?? new ApiChat.ChatRoom()).LocatorCase switch
                {
                    ApiChat.ChatRoom.LocatorOneofCase.Address => (reply1.ChatRoom?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc2 = _documentsDocumentFolderExportService.ListDocumentFolderExportsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiEmployees.LintPositionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.LintPositionsRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                },
            },
        };
        var packed3 = Any.Pack(input3);
        input3 = packed3.Unpack<ApiEmployees.LintPositionsRequest>();
        using var rpc3 = _employeesPositionValidationService.LintPositionsAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiWorkplaces.SuspendVisitorPassResponse
        {
            Outcome = new ApiWorkplaces.SuspendVisitorPassResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply2.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply2.Result?.Status ?? ""),
            },
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.ChatRoom?.Audit?.Etag ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.SearchIndex?.Address?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Region = (reply1.ChatRoom?.Address?.Region ?? ""),
                },
                Lines = { new ApiWorkplaces.VisitorPassLine
                {
                    Amount = (reply3.Position?.Discount?.Amount ?? new ApiCommon.Money()),
                } },
                VisitorPassId = (request.VisitorPassId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass",
            "audit", "SuspendVisitorPass", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Terminates a visitor pass.
    /// </summary>
    public override async Task<ApiWorkplaces.TerminateVisitorPassResponse> TerminateVisitorPass(ApiWorkplaces.TerminateVisitorPassRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.VisitorPassId, "visitor_pass_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling TerminateVisitorPass for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass", request.VisitorPassId,
            "TerminateVisitorPass", "accepted", token);
        var input0 = new ApiEmployees.ScoreEmploymentContractRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                },
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiEmployees.ScoreEmploymentContractRequest>(json0);
        using var rpc0 = _employeesEmploymentContractValidationService.ScoreEmploymentContractAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiHelpdesk.AdvanceKnowledgeArticleWorkflowRequest();
        input1.Context = (request.Context ?? new ApiCommon.RequestContext());
        input1.KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
        {
            BillingAddress = new ApiCommon.PostalAddress
            {
                Locality = (reply0.EmploymentContract?.BillingAddress?.Locality ?? ""),
            },
        };
        using var rpc1 = _helpdeskKnowledgeArticleWorkflowService.AdvanceKnowledgeArticleWorkflowAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDocuments.RetrySignatureExportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.RetrySignatureExportRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (reply0.EmploymentContract?.Counterparty?.Email ?? ""),
                    PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                },
            },
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiDocuments.DocumentRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                },
            },
        };
        using var rpc2 = _documentsSignatureExportService.RetrySignatureExportAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiDevices.AdvanceDevicePolicyWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.AdvanceDevicePolicyWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (reply1.KnowledgeArticle?.Counterparty?.Email ?? ""),
                },
            },
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.KnowledgeArticle?.Audit?.Etag ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.EmploymentContract?.Counterparty?.DisplayName ?? ""),
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                },
            },
        };
        var reply3 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _devicesDevicePolicyWorkflowService.AdvanceDevicePolicyWorkflowAsync(input3, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiWorkplaces.TerminateVisitorPassResponse
        {
            Outcome = new ApiWorkplaces.TerminateVisitorPassResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Message = (reply0.Result?.Message ?? ""),
                NextSteps = { (reply3.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply1.Outcome?.Message ?? ""),
            },
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                BillingAddress = (reply0.EmploymentContract?.BillingAddress ?? new ApiCommon.PostalAddress()),
                Lines = { new ApiWorkplaces.VisitorPassLine
                {
                    Tax = new ApiWorkplaces.VisitorPassLineTax
                    {
                        Amount = (reply3.DevicePolicy?.Discount?.Amount ?? new ApiCommon.Money()),
                    },
                } },
                VisitorPassId = (request.VisitorPassId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass",
            "audit", "TerminateVisitorPass", (request.Reason ?? ""), token);
        return response;
    }
}
