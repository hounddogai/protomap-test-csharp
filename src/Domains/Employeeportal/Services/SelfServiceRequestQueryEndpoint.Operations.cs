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
using ApiAuthorization = Acme.Authorization.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Employeeportal;

public sealed partial class SelfServiceRequestQueryEndpoint
{
    /// <summary>
    /// Lists self service requests a page at a time.
    /// </summary>
    public override async Task<ApiEmployeeportal.BrowseSelfServiceRequestsResponse> BrowseSelfServiceRequests(ApiEmployeeportal.BrowseSelfServiceRequestsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling BrowseSelfServiceRequests for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "self_service_request", "batch",
            "BrowseSelfServiceRequests", "accepted", token);
        var input0 = new ApiEmployeeportal.StreamSelfServiceRequestNoticesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployeeportal.StreamSelfServiceRequestNoticesRequest.Types.Filter
            {
                Statuses = { (request.Filter?.Statuses ?? new Google.Protobuf.Collections.RepeatedField<ApiEmployeeportal.SelfServiceRequestStatus>()) },
            },
        };
        using var stream0 = _employeeportalSelfServiceRequestNotificationService.StreamSelfServiceRequestNotices(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received StreamSelfServiceRequestNotices frame {Type}", change0.GetType().Name);
        }
        var response = new ApiEmployeeportal.BrowseSelfServiceRequestsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "self_service_request",
            "audit", "BrowseSelfServiceRequests", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams every matching self service request.
    /// </summary>
    public override async Task ExportSelfServiceRequests(ApiEmployeeportal.ExportSelfServiceRequestsRequest request, IServerStreamWriter<ApiEmployeeportal.ExportSelfServiceRequestsResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExportSelfServiceRequests for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "self_service_request", "batch",
            "ExportSelfServiceRequests", "accepted", token);
        var input0 = new ApiAuthorization.ResolveAccessPoliciesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var named0 = _registry.Get<ApiAuthorization.AccessPolicyQueryService.AccessPolicyQueryServiceClient>("acme.authorization.v1.AccessPolicyQueryService");
        using var rpc0 = named0.ResolveAccessPoliciesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployeeportal.PauseAnnouncementWorkflowRequest
        {
            Announcement = new ApiEmployeeportal.Announcement
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployeeportal.PauseAnnouncementWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        var descriptor1 = new Method<ApiEmployeeportal.PauseAnnouncementWorkflowRequest, ApiEmployeeportal.PauseAnnouncementWorkflowResponse>(
            MethodType.Unary, "acme.employeeportal.v1" + ".AnnouncementWorkflowService", "PauseAnnouncementWorkflow",
            Wire.Marshaller<ApiEmployeeportal.PauseAnnouncementWorkflowRequest>(), Wire.Marshaller<ApiEmployeeportal.PauseAnnouncementWorkflowResponse>());
        using var rpc1 = _invoker.AsyncUnaryCall(descriptor1, null,
            new CallOptions(cancellationToken: token), input1);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDocuments.ListDocumentTemplateConflictsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListDocumentTemplateConflictsRequest.Types.Filter
            {
                Labels = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Region = (request.Filter?.Region ?? ""),
            },
        };
        var command2 = new DispatchCommand<ApiDocuments.ListDocumentTemplateConflictsRequest>(input2,
            "primary", requestContext.TenantId);
        var routed2 = command2 with { Region = _settings.Value.Region };
        using var rpc2 = _documentsDocumentTemplateSyncService.ListDocumentTemplateConflictsAsync(routed2.Payload, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiEmployeeportal.ExportSelfServiceRequestsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
            SelfServiceRequest = new ApiEmployeeportal.SelfServiceRequest
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.Announcement?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Comments = { new ApiEmployeeportal.SelfServiceRequestComment
                {
                    CreatedAt = (reply1.Announcement?.Audit?.CreatedAt ?? new Timestamp()),
                } },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Format ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "self_service_request",
            "audit", "ExportSelfServiceRequests", (request.Format ?? ""), token);
        await responses.WriteAsync(response, token);
    }
    /// <summary>
    /// Lists self service requests a page at a time.
    /// </summary>
    public override async Task<ApiEmployeeportal.FindSelfServiceRequestsResponse> FindSelfServiceRequests(ApiEmployeeportal.FindSelfServiceRequestsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling FindSelfServiceRequests for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "self_service_request", "batch",
            "FindSelfServiceRequests", "accepted", token);
        var input0 = new ApiEmployeeportal.UnsubscribeAnnouncementUpdatesRequest
        {
            Annotations = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployeeportal.UnsubscribeAnnouncementUpdatesRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Variables = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        using var rpc0 = _employeeportalAnnouncementNotificationService.UnsubscribeAnnouncementUpdatesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiEmployeeportal.FindSelfServiceRequestsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "self_service_request",
            "audit", "FindSelfServiceRequests", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a self service request.
    /// </summary>
    public override async Task<ApiEmployeeportal.LookupSelfServiceRequestResponse> LookupSelfServiceRequest(ApiEmployeeportal.LookupSelfServiceRequestRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.SelfServiceRequestId, "self_service_request_id");
        _logger.LogInformation("Handling LookupSelfServiceRequest for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "self_service_request", request.SelfServiceRequestId,
            "LookupSelfServiceRequest", "accepted", token);
        var input0 = new ApiDocuments.PullDocumentTemplatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.PullDocumentTemplatesRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
                Metadata = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        using var rpc0 = _documentsDocumentTemplateSyncService.PullDocumentTemplatesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiEmployeeportal.LookupSelfServiceRequestResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
            SelfServiceRequest = new ApiEmployeeportal.SelfServiceRequest
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.DocumentTemplate?.Audit?.CreatedAt ?? new Timestamp()),
                },
                SelfServiceRequestId = (request.SelfServiceRequestId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Returns several self service requests at once.
    /// </summary>
    public override async Task<ApiEmployeeportal.ResolveSelfServiceRequestsResponse> ResolveSelfServiceRequests(ApiEmployeeportal.ResolveSelfServiceRequestsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ResolveSelfServiceRequests for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "self_service_request", "batch",
            "ResolveSelfServiceRequests", "accepted", token);
        var input0 = new ApiEmployeeportal.RelayPortalHomeMessagesRequest
        {
            Change = new ApiEmployeeportal.PortalHome
            {
                Assignee = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var stream0 = _employeeportalPortalHomeIntegrationService.RelayPortalHomeMessages(cancellationToken: token);
        async Task Drain0Async()
        {
            await foreach (var change in stream0.ResponseStream.ReadAllAsync(token))
            {
                _logger.LogDebug("Received peer frame {Type}", change.GetType().Name);
            }
        }
        var reader0 = Drain0Async();
        await stream0.RequestStream.WriteAsync(input0, token);
        await stream0.RequestStream.CompleteAsync();
        await reader0;
        var response = new ApiEmployeeportal.ResolveSelfServiceRequestsResponse();
        return response;
    }
    /// <summary>
    /// Lists self service requests a page at a time.
    /// </summary>
    public override async Task<ApiEmployeeportal.SuggestSelfServiceRequestsResponse> SuggestSelfServiceRequests(ApiEmployeeportal.SuggestSelfServiceRequestsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SuggestSelfServiceRequests for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "self_service_request", "batch",
            "SuggestSelfServiceRequests", "accepted", token);
        var input0 = new ApiEmployeeportal.SubscribeAnnouncementUpdatesRequest
        {
            Annotations = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployeeportal.SubscribeAnnouncementUpdatesRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Variables = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        using var rpc0 = _employeeportalAnnouncementNotificationService.SubscribeAnnouncementUpdatesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiEmployeeportal.SuggestSelfServiceRequestsResponse
        {
            SelfServiceRequests = { new ApiEmployeeportal.SelfServiceRequest
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.Announcement?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (reply0.Announcement?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "self_service_request",
            "audit", "SuggestSelfServiceRequests", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Summarizes a self service request.
    /// </summary>
    public override async Task<ApiEmployeeportal.SummarizeSelfServiceRequestsResponse> SummarizeSelfServiceRequests(ApiEmployeeportal.SummarizeSelfServiceRequestsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SummarizeSelfServiceRequests for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "self_service_request", request.SelfServiceRequestId,
            "SummarizeSelfServiceRequests", "accepted", token);
        var input0 = new ApiDocuments.ExplainDocumentFolderDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                Children = { new ApiDocuments.DocumentFolder
                {
                    Children = { new ApiDocuments.DocumentFolder
                    {
                        Manager = new ApiIdentity.UserRef
                        {
                            UserId = (request.Context?.Actor?.UserId ?? ""),
                        },
                    } },
                } },
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _documentsDocumentFolderValidationService.ExplainDocumentFolderDecisionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployeeportal.SubscribeAnnouncementUpdatesRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployeeportal.SubscribeAnnouncementUpdatesRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (reply0.DocumentFolder?.Manager?.Email ?? ""),
                },
            },
            Reason = (request.Reason ?? ""),
        };
        using var rpc1 = _employeeportalAnnouncementNotificationService.SubscribeAnnouncementUpdatesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiWorkplaces.ListPendingVisitorPassesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiWorkplaces.ListPendingVisitorPassesRequest.Types.Filter
            {
                CreatedAfter = (reply0.DocumentFolder?.Audit?.CreatedAt ?? new Timestamp()),
                OwnerId = (reply0.DocumentFolder?.Manager?.UserId ?? ""),
                Region = (reply0.DocumentFolder?.Location?.Region ?? ""),
            },
        };
        var json2 = JsonFormatter.Default.Format(input2);
        input2 = JsonParser.Default.Parse<ApiWorkplaces.ListPendingVisitorPassesRequest>(json2);
        using var rpc2 = _workplacesVisitorPassApprovalService.ListPendingVisitorPassesAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiEmployeeportal.SummarizeSelfServiceRequestsResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Details = { (reply2.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SelfServiceRequestId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "self_service_request",
            "audit", "SummarizeSelfServiceRequests", (request.SelfServiceRequestId ?? ""), token);
        return response;
    }
}
