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
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEdgegateway = Acme.Edgegateway.V1;
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiWebbff = Acme.Webbff.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Employeeportal;

/// <summary>
/// Connects external systems that send self service request events.
/// </summary>
public sealed class SelfServiceRequestIntegrationEndpoint : ApiEmployeeportal.SelfServiceRequestIntegrationService.SelfServiceRequestIntegrationServiceBase
{
    private readonly ApiAudit.AuditEventValidationService.AuditEventValidationServiceClient _auditAuditEventValidationService;
    private readonly ApiDocuments.DocumentFolderHistoryService.DocumentFolderHistoryServiceClient _documentsDocumentFolderHistoryService;
    private readonly ApiDocuments.DocumentFolderValidationService.DocumentFolderValidationServiceClient _documentsDocumentFolderValidationService;
    private readonly ApiDocuments.DocumentQueryService.DocumentQueryServiceClient _documentsDocumentQueryService;
    private readonly ApiDocuments.DocumentSyncService.DocumentSyncServiceClient _documentsDocumentSyncService;
    private readonly ApiEdgegateway.RouteBindingQueryService.RouteBindingQueryServiceClient _edgegatewayRouteBindingQueryService;
    private readonly ApiEmployeeportal.AnnouncementWorkflowService.AnnouncementWorkflowServiceClient _employeeportalAnnouncementWorkflowService;
    private readonly ApiEmployeeportal.PortalHomeIntegrationService.PortalHomeIntegrationServiceClient _employeeportalPortalHomeIntegrationService;
    private readonly CallInvoker _invoker;
    private readonly ApiLedger.LedgerAccountQueryService.LedgerAccountQueryServiceClient _ledgerLedgerAccountQueryService;
    private readonly ApiLedger.LedgerAccountService.LedgerAccountServiceClient _ledgerLedgerAccountService;
    private readonly ILogger<SelfServiceRequestIntegrationEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly IRecordRepository _repository;
    private readonly ApiScheduling.ShiftWorkflowService.ShiftWorkflowServiceClient _schedulingShiftWorkflowService;
    private readonly ISelfServiceRequestIntegrationEndpointSendListSelfServiceRequestSources0Port _selfServiceRequestIntegrationEndpointSendListSelfServiceRequestSources0Port;
    private readonly Func<ApiLedger.SummarizeLedgerAccountsRequest, CallOptions, AsyncUnaryCall<ApiLedger.SummarizeLedgerAccountsResponse>> _sendTestSelfServiceRequestConnection3;
    private readonly ApiTimekeeping.TimesheetWorkflowService.TimesheetWorkflowServiceClient _timekeepingTimesheetWorkflowService;
    private readonly ApiWebbff.PageCompositionService.PageCompositionServiceClient _webbffPageCompositionService;
    private readonly ApiWorkplaces.BuildingApprovalService.BuildingApprovalServiceClient _workplacesBuildingApprovalService;
    private readonly ApiWorkplaces.VisitorPassSyncService.VisitorPassSyncServiceClient _workplacesVisitorPassSyncService;

    public SelfServiceRequestIntegrationEndpoint(
        ApiAudit.AuditEventValidationService.AuditEventValidationServiceClient auditAuditEventValidationService,
        ApiDocuments.DocumentFolderHistoryService.DocumentFolderHistoryServiceClient documentsDocumentFolderHistoryService,
        ApiDocuments.DocumentFolderValidationService.DocumentFolderValidationServiceClient documentsDocumentFolderValidationService,
        ApiDocuments.DocumentQueryService.DocumentQueryServiceClient documentsDocumentQueryService,
        ApiDocuments.DocumentSyncService.DocumentSyncServiceClient documentsDocumentSyncService,
        ApiEdgegateway.RouteBindingQueryService.RouteBindingQueryServiceClient edgegatewayRouteBindingQueryService,
        ApiEmployeeportal.AnnouncementWorkflowService.AnnouncementWorkflowServiceClient employeeportalAnnouncementWorkflowService,
        ApiEmployeeportal.PortalHomeIntegrationService.PortalHomeIntegrationServiceClient employeeportalPortalHomeIntegrationService,
        CallInvoker invoker,
        ApiLedger.LedgerAccountQueryService.LedgerAccountQueryServiceClient ledgerLedgerAccountQueryService,
        ApiLedger.LedgerAccountService.LedgerAccountServiceClient ledgerLedgerAccountService,
        ILogger<SelfServiceRequestIntegrationEndpoint> logger,
        RetryPolicy policy,
        IRecordRepository repository,
        ApiScheduling.ShiftWorkflowService.ShiftWorkflowServiceClient schedulingShiftWorkflowService,
        ISelfServiceRequestIntegrationEndpointSendListSelfServiceRequestSources0Port selfServiceRequestIntegrationEndpointSendListSelfServiceRequestSources0PortAdapter,
        ApiTimekeeping.TimesheetWorkflowService.TimesheetWorkflowServiceClient timekeepingTimesheetWorkflowService,
        ApiWebbff.PageCompositionService.PageCompositionServiceClient webbffPageCompositionService,
        ApiWorkplaces.BuildingApprovalService.BuildingApprovalServiceClient workplacesBuildingApprovalService,
        ApiWorkplaces.VisitorPassSyncService.VisitorPassSyncServiceClient workplacesVisitorPassSyncService)
    {
        _auditAuditEventValidationService = auditAuditEventValidationService;
        _documentsDocumentFolderHistoryService = documentsDocumentFolderHistoryService;
        _documentsDocumentFolderValidationService = documentsDocumentFolderValidationService;
        _documentsDocumentQueryService = documentsDocumentQueryService;
        _documentsDocumentSyncService = documentsDocumentSyncService;
        _edgegatewayRouteBindingQueryService = edgegatewayRouteBindingQueryService;
        _employeeportalAnnouncementWorkflowService = employeeportalAnnouncementWorkflowService;
        _employeeportalPortalHomeIntegrationService = employeeportalPortalHomeIntegrationService;
        _invoker = invoker;
        _ledgerLedgerAccountQueryService = ledgerLedgerAccountQueryService;
        _ledgerLedgerAccountService = ledgerLedgerAccountService;
        _logger = logger;
        _policy = policy;
        _repository = repository;
        _schedulingShiftWorkflowService = schedulingShiftWorkflowService;
        _selfServiceRequestIntegrationEndpointSendListSelfServiceRequestSources0Port = selfServiceRequestIntegrationEndpointSendListSelfServiceRequestSources0PortAdapter;
        _timekeepingTimesheetWorkflowService = timekeepingTimesheetWorkflowService;
        _webbffPageCompositionService = webbffPageCompositionService;
        _workplacesBuildingApprovalService = workplacesBuildingApprovalService;
        _workplacesVisitorPassSyncService = workplacesVisitorPassSyncService;
        _sendTestSelfServiceRequestConnection3 = _ledgerLedgerAccountQueryService.SummarizeLedgerAccountsAsync;
    }
    /// <summary>
    /// Acknowledges a self service request.
    /// </summary>
    public override async Task<ApiEmployeeportal.AcknowledgeSelfServiceRequestEventResponse> AcknowledgeSelfServiceRequestEvent(ApiEmployeeportal.AcknowledgeSelfServiceRequestEventRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling AcknowledgeSelfServiceRequestEvent for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "self_service_request", request.SelfServiceRequestId,
            "AcknowledgeSelfServiceRequestEvent", "accepted", token);
        var input0 = new ApiWorkplaces.ResolveVisitorPassConflictRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiWorkplaces.ResolveVisitorPassConflictRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Reason = (request.Reason ?? ""),
        };
        using var rpc0 = _workplacesVisitorPassSyncService.ResolveVisitorPassConflictAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployeeportal.ReplayPortalHomeEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployeeportal.ReplayPortalHomeEventsRequest.Types.Details
            {
                Endpoint = (request.Details?.Endpoint ?? ""),
                Envelope = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                MessageId = (request.Details?.MessageId ?? ""),
                ReceivedAt = (request.Details?.ReceivedAt ?? new Timestamp()),
            },
        };
        using var rpc1 = _employeeportalPortalHomeIntegrationService.ReplayPortalHomeEventsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDocuments.SuggestDocumentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.SuggestDocumentsRequest.Types.Filter
            {
                CreatedAfter = (reply1.PortalHome?.Sla?.DueAt ?? new Timestamp()),
                Labels = { (reply1.PortalHome?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply1.PortalHome?.Sla?.PolicyId ?? ""),
                Region = (reply0.VisitorPass?.BillingAddress?.Region ?? ""),
            },
        };
        using var rpc2 = _documentsDocumentQueryService.SuggestDocumentsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiTimekeeping.StartTimesheetWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Timesheet = new ApiTimekeeping.Timesheet
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.VisitorPass?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (reply0.VisitorPass?.BillingAddress?.Recipient ?? ""),
                },
            },
        };
        var reply3 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _timekeepingTimesheetWorkflowService.StartTimesheetWorkflowAsync(input3, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiEmployeeportal.AcknowledgeSelfServiceRequestEventResponse
        {
            Outcome = new ApiEmployeeportal.AcknowledgeSelfServiceRequestEventResponse.Types.Outcome
            {
                Message = (reply0.Outcome?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
                Message = (reply2.Result?.Message ?? ""),
            },
            SelfServiceRequest = new ApiEmployeeportal.SelfServiceRequest
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply3.Timesheet?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (reply0.VisitorPass?.Counterparty?.Email ?? ""),
                },
                SelfServiceRequestId = (request.SelfServiceRequestId ?? ""),
                Sla = new ApiEmployeeportal.SelfServiceRequestSla
                {
                    DueAt = (reply1.PortalHome?.Sla?.DueAt ?? new Timestamp()),
                },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.SourceSystem ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "self_service_request",
            "audit", "AcknowledgeSelfServiceRequestEvent", (request.Details?.SourceSystem ?? ""), token);
        return response;
    }
    /// <summary>
    /// Connects a self service request.
    /// </summary>
    public override async Task<ApiEmployeeportal.ConnectSelfServiceRequestSourceResponse> ConnectSelfServiceRequestSource(ApiEmployeeportal.ConnectSelfServiceRequestSourceRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ConnectSelfServiceRequestSource for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "self_service_request", request.SelfServiceRequestId,
            "ConnectSelfServiceRequestSource", "accepted", token);
        var input0 = new ApiDocuments.ListDocumentConflictsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListDocumentConflictsRequest.Types.Filter
            {
                CreatedAfter = (request.Details?.ReceivedAt ?? new Timestamp()),
                Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Details?.MessageId ?? ""),
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _documentsDocumentSyncService.ListDocumentConflictsAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiEmployeeportal.AdvanceAnnouncementWorkflowRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployeeportal.AdvanceAnnouncementWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        var reply1 = await Workflow.SendAsync(input1, token, _employeeportalAnnouncementWorkflowService.AdvanceAnnouncementWorkflowAsync);
        var input2 = new ApiAudit.CheckAuditEventEligibilityRequest
        {
            AuditEvent = new ApiAudit.AuditEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiAudit.AuditTrailRef
                {
                    DisplayName = (reply1.Announcement?.Counterparty?.DisplayName ?? ""),
                    Number = (reply1.Announcement?.Number ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc2 = _auditAuditEventValidationService.CheckAuditEventEligibilityAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiEmployeeportal.ConnectSelfServiceRequestSourceResponse
        {
            Outcome = new ApiEmployeeportal.ConnectSelfServiceRequestSourceResponse.Types.Outcome
            {
                NextSteps = { (reply2.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            SelfServiceRequest = new ApiEmployeeportal.SelfServiceRequest
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.Announcement?.Counterparty?.DisplayName ?? ""),
                },
                SelfServiceRequestId = (request.SelfServiceRequestId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "self_service_request",
            "audit", "ConnectSelfServiceRequestSource", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Disconnects a self service request.
    /// </summary>
    public override async Task<ApiEmployeeportal.DisconnectSelfServiceRequestSourceResponse> DisconnectSelfServiceRequestSource(ApiEmployeeportal.DisconnectSelfServiceRequestSourceRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling DisconnectSelfServiceRequestSource for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "self_service_request", request.SelfServiceRequestId,
            "DisconnectSelfServiceRequestSource", "accepted", token);
        var input0 = new ApiAudit.PreviewAuditEventRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            AuditEvent = new ApiAudit.AuditEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Measurements = { new ApiAudit.AuditEventMeasurement
                {
                    ObservedAt = (request.Details?.ReceivedAt ?? new Timestamp()),
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiAudit.PreviewAuditEventRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            Reason = (request.Reason ?? ""),
        };
        using var rpc0 = _auditAuditEventValidationService.PreviewAuditEventAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiScheduling.StartShiftWorkflowRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Shift = new ApiScheduling.Shift
            {
                Organizer = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.AuditEvent?.Subject?.DisplayName ?? ""),
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        var descriptor1 = new Method<ApiScheduling.StartShiftWorkflowRequest, ApiScheduling.StartShiftWorkflowResponse>(
            MethodType.Unary, "acme.scheduling.v1" + ".ShiftWorkflowService", "StartShiftWorkflow",
            Wire.Marshaller<ApiScheduling.StartShiftWorkflowRequest>(), Wire.Marshaller<ApiScheduling.StartShiftWorkflowResponse>());
        using var rpc1 = _invoker.AsyncUnaryCall(descriptor1, null,
            new CallOptions(cancellationToken: token), input1);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiEmployeeportal.AcknowledgePortalHomeEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployeeportal.AcknowledgePortalHomeEventRequest.Types.Details
            {
                Envelope = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                ReceivedAt = (request.Details?.ReceivedAt ?? new Timestamp()),
            },
        };
        using var rpc2 = _employeeportalPortalHomeIntegrationService.AcknowledgePortalHomeEventAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiEmployeeportal.DisconnectSelfServiceRequestSourceResponse
        {
            Outcome = new ApiEmployeeportal.DisconnectSelfServiceRequestSourceResponse.Types.Outcome
            {
                EffectiveAt = (reply0.Outcome?.EffectiveAt ?? new Timestamp()),
                Message = (reply2.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply1.Outcome?.Status ?? ""),
            },
            SelfServiceRequest = new ApiEmployeeportal.SelfServiceRequest
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (reply1.Shift?.Organizer?.Email ?? ""),
                },
                SelfServiceRequestId = (request.SelfServiceRequestId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.SourceSystem ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "self_service_request",
            "audit", "DisconnectSelfServiceRequestSource", (request.Details?.SourceSystem ?? ""), token);
        return response;
    }
    /// <summary>
    /// Takes a stream of self service requests and answers once at the end.
    /// </summary>
    public override async Task<ApiEmployeeportal.IngestSelfServiceRequestEventsResponse> IngestSelfServiceRequestEvents(IAsyncStreamReader<ApiEmployeeportal.IngestSelfServiceRequestEventsRequest> requests, ServerCallContext context)
    {
        var token = context.CancellationToken;
        var result = new ApiEmployeeportal.IngestSelfServiceRequestEventsResponse();
        await foreach (var request in requests.ReadAllAsync(token))
        {
            RequestGuard.Tenant(request.Context);
            var requestContext = request.Context;
            using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
                requestContext.TenantId, requestContext.TraceId);
            _logger.LogInformation("Handling IngestSelfServiceRequestEvents for {Tenant}", requestContext.TenantId);
            if (request.Record is { } record)
            {
                var snapshot = SelfServiceRequestMapper.ToRecord(requestContext.TenantId, record);
                await _repository.SaveAsync(snapshot, token);
            }
            var reply0 = new ApiDocuments.SuggestDocumentsResponse();
            foreach (var item in (request.Record?.History ?? new Google.Protobuf.Collections.RepeatedField<ApiEmployeeportal.SelfServiceRequestStatusChange>()).Select(value => new[] { value }).SelectMany(group => group))
            {
                var input0 = new ApiDocuments.SuggestDocumentsRequest
                {
                    Context = (request.Context ?? new ApiCommon.RequestContext()),
                    Filter = new ApiDocuments.SuggestDocumentsRequest.Types.Filter
                    {
                        CreatedAfter = (item.ChangedAt ?? new Timestamp()),
                    },
                };
                using var rpc0 = _documentsDocumentQueryService.SuggestDocumentsAsync(input0, cancellationToken: token);
                reply0 = await rpc0.ResponseAsync;
            }
            var input1 = new ApiWorkplaces.CommentBuildingApprovalRequest
            {
                Building = new ApiWorkplaces.Building
                {
                    Children = { new ApiWorkplaces.Building
                    {
                        Children = { new ApiWorkplaces.Building
                        {
                            Audit = new ApiCommon.AuditInfo
                            {
                                UpdatedBy = request.ChunkCase switch
                                {
                                    ApiEmployeeportal.IngestSelfServiceRequestEventsRequest.ChunkOneofCase.Record => (request.Record?.Audit?.UpdatedBy ?? ""),
                                    _ => ""
                                },
                            },
                        } },
                    } },
                },
                Context = (request.Context ?? new ApiCommon.RequestContext()),
            };
            using var rpc1 = _workplacesBuildingApprovalService.CommentBuildingApprovalAsync(input1, cancellationToken: token);
            var reply1 = await rpc1.ResponseAsync;
            var input2 = request.IngestSelfServiceRequestEventsToResolveRouteBindings2();
            using var rpc2 = _edgegatewayRouteBindingQueryService.ResolveRouteBindingsAsync(input2, cancellationToken: token);
            var reply2 = await rpc2.ResponseAsync;
            var response = new ApiEmployeeportal.IngestSelfServiceRequestEventsResponse
            {
                Result = new ApiCommon.OperationResult
                {
                    Details = { (reply2.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                    Message = (reply1.Result?.Message ?? ""),
                    ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            };
            _logger.LogDebug("Local audit detail {Detail}", (request.Record?.Subject ?? ""));
            await _repository.RecordAsync(requestContext.TenantId, "self_service_request",
                "audit", "IngestSelfServiceRequestEvents", (request.Record?.Subject ?? ""), token);
            result = response;
        }
        return result;
    }
    /// <summary>
    /// Lists self service requests a page at a time.
    /// </summary>
    public override async Task<ApiEmployeeportal.ListSelfServiceRequestSourcesResponse> ListSelfServiceRequestSources(ApiEmployeeportal.ListSelfServiceRequestSourcesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ListSelfServiceRequestSources for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "self_service_request", "batch",
            "ListSelfServiceRequestSources", "accepted", token);
        var input0 = new ApiEmployeeportal.ListAnnouncementWorkflowsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployeeportal.ListAnnouncementWorkflowsRequest.Types.Filter
            {
                Query = (request.Filter?.Query ?? ""),
            },
            Page = new ApiCommon.PageRequest
            {
                PageSize = (request.Page?.PageSize ?? 0),
            },
        };
        var reply0 = await SendListSelfServiceRequestSources0Async(input0, token);
        var response = new ApiEmployeeportal.ListSelfServiceRequestSourcesResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "self_service_request",
            "audit", "ListSelfServiceRequestSources", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Exchanges changes of self service requests with a peer.
    /// </summary>
    public override async Task RelaySelfServiceRequestMessages(IAsyncStreamReader<ApiEmployeeportal.RelaySelfServiceRequestMessagesRequest> requests, IServerStreamWriter<ApiEmployeeportal.RelaySelfServiceRequestMessagesResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        await foreach (var request in requests.ReadAllAsync(token))
        {
            RequestGuard.Tenant(request.Context);
            var requestContext = request.Context;
            using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
                requestContext.TenantId, requestContext.TraceId);
            _logger.LogInformation("Handling RelaySelfServiceRequestMessages for {Tenant}", requestContext.TenantId);
            if (request.Change is { } record)
            {
                var snapshot = SelfServiceRequestMapper.ToRecord(requestContext.TenantId, record);
                await _repository.SaveAsync(snapshot, token);
            }
            var input0 = new ApiLedger.CreateLedgerAccountRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                LedgerAccount = new ApiLedger.LedgerAccount
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedAt = request.FrameCase switch
                        {
                            ApiEmployeeportal.RelaySelfServiceRequestMessagesRequest.FrameOneofCase.Change => (request.Change?.Audit?.CreatedAt ?? new Timestamp()),
                            _ => new Timestamp()
                        },
                        CreatedBy = request.FrameCase switch
                        {
                            ApiEmployeeportal.RelaySelfServiceRequestMessagesRequest.FrameOneofCase.Change => (request.Change?.Audit?.CreatedBy ?? ""),
                            _ => ""
                        },
                    },
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = request.FrameCase switch
                        {
                            ApiEmployeeportal.RelaySelfServiceRequestMessagesRequest.FrameOneofCase.Change => (request.Change?.Requester?.Email ?? ""),
                            _ => ""
                        },
                    },
                    Holder = new ApiLedger.PostingBatchRef
                    {
                        DisplayName = request.FrameCase switch
                        {
                            ApiEmployeeportal.RelaySelfServiceRequestMessagesRequest.FrameOneofCase.Change => (request.Change?.Customer?.DisplayName ?? ""),
                            _ => ""
                        },
                    },
                },
            };
            using var rpc0 = _ledgerLedgerAccountService.CreateLedgerAccountAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            var input1 = new ApiEmployeeportal.RelayPortalHomeMessagesRequest
            {
                Change = new ApiEmployeeportal.PortalHome
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        UpdatedBy = (reply0.LedgerAccount?.Audit?.UpdatedBy ?? ""),
                    },
                    Channel = new ApiEmployeeportal.PortalHomeChannelOrigin
                    {
                        Address = (request.Change?.Channel?.Address ?? ""),
                    },
                },
                Context = (request.Context ?? new ApiCommon.RequestContext()),
            };
            using var stream1 = _employeeportalPortalHomeIntegrationService.RelayPortalHomeMessages(cancellationToken: token);
            async Task Drain1Async()
            {
                await foreach (var change in stream1.ResponseStream.ReadAllAsync(token))
                {
                    _logger.LogDebug("Received peer frame {Type}", change.GetType().Name);
                }
            }
            var reader1 = Drain1Async();
            await stream1.RequestStream.WriteAsync(input1, token);
            await stream1.RequestStream.CompleteAsync();
            await reader1;
            var input2 = new ApiWebbff.CreatePageCompositionRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                PageComposition = new ApiWebbff.PageComposition
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedBy = request.FrameCase switch
                        {
                            ApiEmployeeportal.RelaySelfServiceRequestMessagesRequest.FrameOneofCase.Change => (request.Change?.Audit?.CreatedBy ?? ""),
                            _ => ""
                        },
                        UpdatedBy = (reply0.LedgerAccount?.Audit?.UpdatedBy ?? ""),
                    },
                    Bank = new ApiWebbff.PageCompositionBankSettlement
                    {
                        AccountHolder = (reply0.LedgerAccount ?? new ApiLedger.LedgerAccount()).PaymentCase switch
                        {
                            ApiLedger.LedgerAccount.FundingOneofCase.Bank => (reply0.LedgerAccount?.Bank?.AccountHolder ?? ""),
                            _ => ""
                        },
                    },
                },
            };
            using var rpc2 = _webbffPageCompositionService.CreatePageCompositionAsync(input2, cancellationToken: token);
            var reply2 = await rpc2.ResponseAsync;
            var response = new ApiEmployeeportal.RelaySelfServiceRequestMessagesResponse
            {
                Result = new ApiCommon.OperationResult
                {
                    Status = (reply2.Result?.Status ?? ""),
                },
                SelfServiceRequest = new ApiEmployeeportal.SelfServiceRequest
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        Etag = (reply0.LedgerAccount?.Audit?.Etag ?? ""),
                        UpdatedBy = (reply2.PageComposition?.Audit?.UpdatedBy ?? ""),
                    },
                    Priority = (request.Change?.Priority ?? 0),
                    Requester = new ApiCommon.ContactInfo
                    {
                        Email = (reply0.LedgerAccount?.Contact?.Email ?? ""),
                    },
                    Subject = (request.Change?.Subject ?? ""),
                },
            };
            _logger.LogDebug("Local audit detail {Detail}", (request.Cursor ?? ""));
            await _repository.RecordAsync(requestContext.TenantId, "self_service_request",
                "audit", "RelaySelfServiceRequestMessages", (request.Cursor ?? ""), token);
            await responses.WriteAsync(response, token);
        }
    }
    /// <summary>
    /// Replays a self service request.
    /// </summary>
    public override async Task<ApiEmployeeportal.ReplaySelfServiceRequestEventsResponse> ReplaySelfServiceRequestEvents(ApiEmployeeportal.ReplaySelfServiceRequestEventsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ReplaySelfServiceRequestEvents for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "self_service_request", request.SelfServiceRequestId,
            "ReplaySelfServiceRequestEvents", "accepted", token);
        var input0 = new ApiEmployeeportal.TestPortalHomeConnectionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployeeportal.TestPortalHomeConnectionRequest.Types.Details
            {
                Endpoint = (request.Details?.Endpoint ?? ""),
                Envelope = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                MessageId = (request.Details?.MessageId ?? ""),
                SourceSystem = (request.Details?.SourceSystem ?? ""),
            },
        };
        using var rpc0 = _employeeportalPortalHomeIntegrationService.TestPortalHomeConnectionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployeeportal.ConnectPortalHomeSourceRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployeeportal.ConnectPortalHomeSourceRequest.Types.Details
            {
                Endpoint = (request.Details?.Endpoint ?? ""),
                Envelope = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                ReceivedAt = (request.Details?.ReceivedAt ?? new Timestamp()),
                SourceSystem = (request.Details?.SourceSystem ?? ""),
            },
            PortalHomeId = (reply0.PortalHome?.PortalHomeId ?? ""),
        };
        using var rpc1 = _employeeportalPortalHomeIntegrationService.ConnectPortalHomeSourceAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiEmployeeportal.ReplaySelfServiceRequestEventsResponse
        {
            Outcome = new ApiEmployeeportal.ReplaySelfServiceRequestEventsResponse.Types.Outcome
            {
                Message = (reply1.Result?.Message ?? ""),
                Status = (reply0.Outcome?.Status ?? ""),
            },
            SelfServiceRequest = new ApiEmployeeportal.SelfServiceRequest
            {
                Requester = (reply1.PortalHome?.Requester ?? new ApiCommon.ContactInfo()),
                SelfServiceRequestId = (request.SelfServiceRequestId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "self_service_request",
            "audit", "ReplaySelfServiceRequestEvents", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Tests a self service request.
    /// </summary>
    public override async Task<ApiEmployeeportal.TestSelfServiceRequestConnectionResponse> TestSelfServiceRequestConnection(ApiEmployeeportal.TestSelfServiceRequestConnectionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling TestSelfServiceRequestConnection for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "self_service_request", request.SelfServiceRequestId,
            "TestSelfServiceRequestConnection", "accepted", token);
        var input0 = new ApiEmployeeportal.TestPortalHomeConnectionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployeeportal.TestPortalHomeConnectionRequest.Types.Details
            {
                Envelope = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                MessageId = (request.Details?.MessageId ?? ""),
                ReceivedAt = (request.Details?.ReceivedAt ?? new Timestamp()),
            },
        };
        using var rpc0 = _employeeportalPortalHomeIntegrationService.TestPortalHomeConnectionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.RevertDocumentFolderRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.RevertDocumentFolderRequest.Types.Details
            {
                Snapshot = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                Children = { new ApiDocuments.DocumentFolder
                {
                    Children = { new ApiDocuments.DocumentFolder
                    {
                        Manager = new ApiIdentity.UserRef
                        {
                            DisplayName = (reply0.PortalHome ?? new ApiEmployeeportal.PortalHome()).OriginCase switch
                            {
                                ApiEmployeeportal.PortalHome.OriginOneofCase.Customer => (reply0.PortalHome?.Customer?.DisplayName ?? ""),
                                _ => ""
                            },
                        },
                    } },
                } },
            },
        };
        using var rpc1 = _documentsDocumentFolderHistoryService.RevertDocumentFolderAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDocuments.ValidateDocumentFolderRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                Children = { new ApiDocuments.DocumentFolder
                {
                    Children = { new ApiDocuments.DocumentFolder
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            CreatedAt = (reply0.PortalHome?.Audit?.CreatedAt ?? new Timestamp()),
                            UpdatedAt = (reply0.PortalHome?.Audit?.UpdatedAt ?? new Timestamp()),
                        },
                        Location = new ApiCommon.PostalAddress
                        {
                            Locality = (reply1.DocumentFolder?.Location?.Locality ?? ""),
                            Region = (reply1.DocumentFolder?.Location?.Region ?? ""),
                        },
                    } },
                } },
            },
        };
        var reply2 = await Workflow.SendAsync(input2, token, _documentsDocumentFolderValidationService.ValidateDocumentFolderAsync);
        var input3 = new ApiLedger.SummarizeLedgerAccountsRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc3 = _sendTestSelfServiceRequestConnection3(input3, new CallOptions(cancellationToken: token));
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiEmployeeportal.TestSelfServiceRequestConnectionResponse
        {
            Outcome = new ApiEmployeeportal.TestSelfServiceRequestConnectionResponse.Types.Outcome
            {
                Message = (reply0.Outcome?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply3.Result?.CorrelationId ?? ""),
                ReferenceIds = { (reply3.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            SelfServiceRequest = new ApiEmployeeportal.SelfServiceRequest
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (reply1.DocumentFolder?.Manager?.Email ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply2.DocumentFolder?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedAt = (reply2.DocumentFolder?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                SelfServiceRequestId = (request.SelfServiceRequestId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "self_service_request",
            "audit", "TestSelfServiceRequestConnection", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiEmployeeportal.ListAnnouncementWorkflowsResponse> SendListSelfServiceRequestSources0Async(ApiEmployeeportal.ListAnnouncementWorkflowsRequest request, CancellationToken token)
    {
        return await _selfServiceRequestIntegrationEndpointSendListSelfServiceRequestSources0Port.SendAsync(request, token);
    }
}
