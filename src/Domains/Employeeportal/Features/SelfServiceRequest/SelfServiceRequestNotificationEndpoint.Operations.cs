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
using ApiDocuments = Acme.Documents.V1;
using ApiEdgegateway = Acme.Edgegateway.V1;
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiFanportal = Acme.Fanportal.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;

namespace Acme.Workforce.Employeeportal;

public sealed partial class SelfServiceRequestNotificationEndpoint
{
    /// <summary>
    /// Returns a self service request.
    /// </summary>
    public override async Task<ApiEmployeeportal.GetSelfServiceRequestPreferencesResponse> GetSelfServiceRequestPreferences(ApiEmployeeportal.GetSelfServiceRequestPreferencesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.SelfServiceRequestId, "self_service_request_id");
        _logger.LogInformation("Handling GetSelfServiceRequestPreferences for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "self_service_request", request.SelfServiceRequestId,
            "GetSelfServiceRequestPreferences", "accepted", token);
        var input0 = new ApiEmployeeportal.ConnectSelfServiceRequestSourceRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployeeportal.ConnectSelfServiceRequestSourceRequest.Types.Details
            {
                Envelope = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                ReceivedAt = (request.Context?.Deadline ?? new Timestamp()),
            },
            SelfServiceRequestId = (request.SelfServiceRequestId ?? ""),
        };
        using var rpc0 = _employeeportalSelfServiceRequestIntegrationService.ConnectSelfServiceRequestSourceAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiEmployeeportal.GetSelfServiceRequestPreferencesResponse
        {
            SelfServiceRequest = new ApiEmployeeportal.SelfServiceRequest
            {
                Assignee = new ApiIdentity.UserRef
                {
                    UserId = (reply0.SelfServiceRequest?.Assignee?.UserId ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (reply0.SelfServiceRequest?.Requester?.PreferredChannel ?? ""),
                },
                SelfServiceRequestId = (request.SelfServiceRequestId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists self service requests a page at a time.
    /// </summary>
    public override async Task<ApiEmployeeportal.ListSelfServiceRequestSubscriptionsResponse> ListSelfServiceRequestSubscriptions(ApiEmployeeportal.ListSelfServiceRequestSubscriptionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListSelfServiceRequestSubscriptions for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "self_service_request", "batch",
            "ListSelfServiceRequestSubscriptions", "accepted", token);
        var input0 = new ApiFeatureflags.CreateFeatureFlagRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            FeatureFlag = new ApiFeatureflags.FeatureFlag
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Context?.Deadline ?? new Timestamp()),
                    UpdatedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        var descriptor0 = new Method<ApiFeatureflags.CreateFeatureFlagRequest, ApiFeatureflags.CreateFeatureFlagResponse>(
            MethodType.Unary, "acme.featureflags.v1" + ".FeatureFlagService", "CreateFeatureFlag",
            Wire.Marshaller<ApiFeatureflags.CreateFeatureFlagRequest>(), Wire.Marshaller<ApiFeatureflags.CreateFeatureFlagResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiHelpdesk.CancelTicketWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Ticket = new ApiHelpdesk.Ticket
            {
                Comments = { new ApiHelpdesk.TicketComment
                {
                    CreatedAt = (reply0.FeatureFlag?.Audit?.CreatedAt ?? new Timestamp()),
                } },
            },
        };
        var command1 = new DispatchCommand<ApiHelpdesk.CancelTicketWorkflowRequest>(input1,
            "primary", requestContext.TenantId);
        var routed1 = command1 with { Region = _settings.Value.Region };
        using var rpc1 = _helpdeskTicketWorkflowService.CancelTicketWorkflowAsync(routed1.Payload, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiEmployeeportal.ListSelfServiceRequestSubscriptionsResponse
        {
            SelfServiceRequests = { new ApiEmployeeportal.SelfServiceRequest
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.FeatureFlag?.Audit?.CreatedBy ?? ""),
                },
                Priority = (reply0.FeatureFlag?.Priority ?? 0),
                Requester = new ApiCommon.ContactInfo
                {
                    Phone = (reply1.Ticket?.Requester?.Phone ?? ""),
                },
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "self_service_request",
            "audit", "ListSelfServiceRequestSubscriptions", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Notifys a self service request.
    /// </summary>
    public override async Task<ApiEmployeeportal.NotifySelfServiceRequestWatchersResponse> NotifySelfServiceRequestWatchers(ApiEmployeeportal.NotifySelfServiceRequestWatchersRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling NotifySelfServiceRequestWatchers for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "self_service_request", request.SelfServiceRequestId,
            "NotifySelfServiceRequestWatchers", "accepted", token);
        var input0 = new ApiEmployeeportal.ReplayPortalHomeEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployeeportal.ReplayPortalHomeEventsRequest.Types.Details
            {
                Envelope = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
            Reason = (request.Reason ?? ""),
        };
        using var rpc0 = _employeeportalPortalHomeIntegrationService.ReplayPortalHomeEventsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiEmployeeportal.ReplayPortalHomeEventsRequest>("acme.employeeportal.v1.SelfServiceRequestNotificationService/NotifySelfServiceRequestWatchers/0", input0.Clone()), token);
        var input1 = new ApiEmployeeportal.ListPortalHomeSourcesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployeeportal.ListPortalHomeSourcesRequest.Types.Filter
            {
                CreatedAfter = (reply0.PortalHome?.Sla?.DueAt ?? new Timestamp()),
                Labels = { (reply0.PortalHome?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply0.PortalHome ?? new ApiEmployeeportal.PortalHome()).OriginCase switch
                {
                    ApiEmployeeportal.PortalHome.OriginOneofCase.Customer => (reply0.PortalHome?.Customer?.AnnouncementId ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc1 = _employeeportalPortalHomeIntegrationService.ListPortalHomeSourcesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiEmployeeportal.SubscribePortalHomeUpdatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployeeportal.SubscribePortalHomeUpdatesRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Recipient?.Name ?? new ApiCommon.PersonName()),
                },
            },
        };
        using var rpc2 = _employeeportalPortalHomeNotificationService.SubscribePortalHomeUpdatesAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiEmployeeportal.NotifySelfServiceRequestWatchersResponse
        {
            Outcome = new ApiEmployeeportal.NotifySelfServiceRequestWatchersResponse.Types.Outcome
            {
                EffectiveAt = (reply0.Outcome?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            SelfServiceRequest = new ApiEmployeeportal.SelfServiceRequest
            {
                Requester = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Recipient?.Name ?? new ApiCommon.PersonName()),
                },
                SelfServiceRequestId = (request.SelfServiceRequestId ?? ""),
                Tags = { (reply2.PortalHome?.Tags ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.TemplateId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "self_service_request",
            "audit", "NotifySelfServiceRequestWatchers", (request.Details?.TemplateId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Sends a self service request.
    /// </summary>
    public override async Task<ApiEmployeeportal.SendSelfServiceRequestTestNoticeResponse> SendSelfServiceRequestTestNotice(ApiEmployeeportal.SendSelfServiceRequestTestNoticeRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SendSelfServiceRequestTestNotice for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "self_service_request", request.SelfServiceRequestId,
            "SendSelfServiceRequestTestNotice", "accepted", token);
        var input0 = new ApiEmployeeportal.SendPortalHomeTestNoticeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployeeportal.SendPortalHomeTestNoticeRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.Recipient?.Phone ?? ""),
                },
            },
        };
        using var rpc0 = _employeeportalPortalHomeNotificationService.SendPortalHomeTestNoticeAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployeeportal.ListAnnouncementSourcesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployeeportal.ListAnnouncementSourcesRequest.Types.Filter
            {
                CreatedAfter = (reply0.PortalHome?.Sla?.DueAt ?? new Timestamp()),
                Labels = { (request.Details?.Variables ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply0.PortalHome?.Sla?.PolicyId ?? ""),
            },
        };
        var labels1 = input1.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input1.Context.Baggage.Clear();
        input1.Context.Baggage.Add(labels1);
        using var rpc1 = _employeeportalAnnouncementIntegrationService.ListAnnouncementSourcesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiEmployeeportal.SendSelfServiceRequestTestNoticeResponse
        {
            Outcome = new ApiEmployeeportal.SendSelfServiceRequestTestNoticeResponse.Types.Outcome
            {
                Status = (reply1.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            SelfServiceRequest = new ApiEmployeeportal.SelfServiceRequest
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Recipient?.Email ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.PortalHome?.Audit?.UpdatedBy ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Phone = (reply0.PortalHome?.Requester?.Phone ?? ""),
                },
                SelfServiceRequestId = (request.SelfServiceRequestId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "self_service_request",
            "audit", "SendSelfServiceRequestTestNotice", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams the changes of self service requests as they happen.
    /// </summary>
    public override async Task StreamSelfServiceRequestNotices(ApiEmployeeportal.StreamSelfServiceRequestNoticesRequest request, IServerStreamWriter<ApiEmployeeportal.StreamSelfServiceRequestNoticesResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling StreamSelfServiceRequestNotices for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "self_service_request", "batch",
            "StreamSelfServiceRequestNotices", "accepted", token);
        var input0 = new ApiEmployeeportal.UnsubscribePortalHomeUpdatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployeeportal.UnsubscribePortalHomeUpdatesRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Phone = (request.Subscriber?.Phone ?? ""),
                },
            },
        };
        using var rpc0 = _employeeportalPortalHomeNotificationService.UnsubscribePortalHomeUpdatesAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiFeatureflags.ListFeatureFlagsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiFeatureflags.ListFeatureFlagsRequest.Types.Filter
            {
                CreatedAfter = (request.Filter?.CreatedAfter ?? new Timestamp()),
                Labels = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Filter?.OwnerId ?? ""),
            },
        };
        using var rpc1 = _featureflagsFeatureFlagService.ListFeatureFlagsAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var input2 = new ApiEmployeeportal.RelayPortalHomeMessagesRequest
        {
            Change = new ApiEmployeeportal.PortalHome
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (reply0.PortalHome?.Requester?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var stream2 = _employeeportalPortalHomeIntegrationService.RelayPortalHomeMessages(cancellationToken: token);
        async Task Drain2Async()
        {
            await foreach (var change in stream2.ResponseStream.ReadAllAsync(token))
            {
                _logger.LogDebug("Received peer frame {Type}", change.GetType().Name);
            }
        }
        var reader2 = Drain2Async();
        await stream2.RequestStream.WriteAsync(input2, token);
        await stream2.RequestStream.CompleteAsync();
        await reader2;
        var response = new ApiEmployeeportal.StreamSelfServiceRequestNoticesResponse
        {
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            SelfServiceRequest = new ApiEmployeeportal.SelfServiceRequest
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.PortalHome?.Audit?.UpdatedBy ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.Subscriber?.Email ?? ""),
                    Phone = (request.Subscriber?.Phone ?? ""),
                },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Subscriber?.PreferredChannel ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "self_service_request",
            "audit", "StreamSelfServiceRequestNotices", (request.Subscriber?.PreferredChannel ?? ""), token);
        await responses.WriteAsync(response, token);
    }
    /// <summary>
    /// Subscribes a self service request.
    /// </summary>
    public override async Task<ApiEmployeeportal.SubscribeSelfServiceRequestUpdatesResponse> SubscribeSelfServiceRequestUpdates(ApiEmployeeportal.SubscribeSelfServiceRequestUpdatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SubscribeSelfServiceRequestUpdates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "self_service_request", request.SelfServiceRequestId,
            "SubscribeSelfServiceRequestUpdates", "accepted", token);
        var input0 = new ApiEdgegateway.SearchRouteBindingsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEdgegateway.SearchRouteBindingsRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
                Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Details?.TemplateId ?? ""),
            },
        };
        using var rpc0 = _edgegatewayRouteBindingService.SearchRouteBindingsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.NormalizeDocumentFolderRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.NormalizeDocumentFolderRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.Recipient?.Phone ?? ""),
                    PreferredChannel = (request.Details?.Recipient?.PreferredChannel ?? ""),
                },
            },
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                Children = { new ApiDocuments.DocumentFolder
                {
                    Manager = new ApiIdentity.UserRef
                    {
                        Email = (request.Details?.Recipient?.Email ?? ""),
                    },
                } },
            },
        };
        var reply1 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _documentsDocumentFolderValidationService.NormalizeDocumentFolderAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiEmployeeportal.SubscribeSelfServiceRequestUpdatesResponse
        {
            Outcome = new ApiEmployeeportal.SubscribeSelfServiceRequestUpdatesResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            SelfServiceRequest = new ApiEmployeeportal.SelfServiceRequest
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (reply1.DocumentFolder?.Manager?.Email ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.Recipient?.Phone ?? ""),
                },
                SelfServiceRequestId = (request.SelfServiceRequestId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "self_service_request",
            "audit", "SubscribeSelfServiceRequestUpdates", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Unsubscribes a self service request.
    /// </summary>
    public override async Task<ApiEmployeeportal.UnsubscribeSelfServiceRequestUpdatesResponse> UnsubscribeSelfServiceRequestUpdates(ApiEmployeeportal.UnsubscribeSelfServiceRequestUpdatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling UnsubscribeSelfServiceRequestUpdates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "self_service_request", request.SelfServiceRequestId,
            "UnsubscribeSelfServiceRequestUpdates", "accepted", token);
        var input0 = new ApiFanportal.ResolveFanSessionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _fanportalFanSessionQueryService.ResolveFanSessionsAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiDocuments.NotifyDocumentWatchersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.NotifyDocumentWatchersRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Recipient?.Email ?? ""),
                    Name = (request.Details?.Recipient?.Name ?? new ApiCommon.PersonName()),
                },
            },
        };
        using var rpc1 = _documentsDocumentNotificationService.NotifyDocumentWatchersAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiEmployeeportal.UnsubscribeSelfServiceRequestUpdatesResponse
        {
            Outcome = new ApiEmployeeportal.UnsubscribeSelfServiceRequestUpdatesResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply1.Outcome?.Message ?? ""),
            },
            SelfServiceRequest = new ApiEmployeeportal.SelfServiceRequest
            {
                Requester = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.Recipient?.Phone ?? ""),
                },
                SelfServiceRequestId = (request.SelfServiceRequestId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "self_service_request",
            "audit", "UnsubscribeSelfServiceRequestUpdates", (request.Reason ?? ""), token);
        return response;
    }
}
