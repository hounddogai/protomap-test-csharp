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
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiPatientapp = Acme.Patientapp.V1;

namespace Acme.Workforce.Employeeportal;

public sealed partial class AnnouncementNotificationEndpoint
{
    /// <summary>
    /// Returns an announcement.
    /// </summary>
    public override async Task<ApiEmployeeportal.GetAnnouncementPreferencesResponse> GetAnnouncementPreferences(ApiEmployeeportal.GetAnnouncementPreferencesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.AnnouncementId, "announcement_id");
        _logger.LogInformation("Handling GetAnnouncementPreferences for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "announcement", request.AnnouncementId,
            "GetAnnouncementPreferences", "accepted", token);
        var input0 = new ApiHelpdesk.ScheduleEscalationReportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.ScheduleEscalationReportRequest.Types.Details
            {
                Filters = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        var descriptor0 = new Method<ApiHelpdesk.ScheduleEscalationReportRequest, ApiHelpdesk.ScheduleEscalationReportResponse>(
            MethodType.Unary, "acme.helpdesk.v1" + ".EscalationReportingService", "ScheduleEscalationReport",
            Wire.Marshaller<ApiHelpdesk.ScheduleEscalationReportRequest>(), Wire.Marshaller<ApiHelpdesk.ScheduleEscalationReportResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiEmployeeportal.GetAnnouncementPreferencesResponse
        {
            Announcement = new ApiEmployeeportal.Announcement
            {
                AnnouncementId = (request.AnnouncementId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists announcements a page at a time.
    /// </summary>
    public override async Task<ApiEmployeeportal.ListAnnouncementSubscriptionsResponse> ListAnnouncementSubscriptions(ApiEmployeeportal.ListAnnouncementSubscriptionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListAnnouncementSubscriptions for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "announcement", "batch",
            "ListAnnouncementSubscriptions", "accepted", token);
        var input0 = new ApiNotifications.CheckNotificationEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Notification = new ApiNotifications.Notification
            {
                Comments = { new ApiNotifications.NotificationComment
                {
                    AuthorEmail = (request.Context?.Actor?.Email ?? ""),
                } },
            },
        };
        var command0 = new DispatchCommand<ApiNotifications.CheckNotificationEligibilityRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _notificationsNotificationValidationService.CheckNotificationEligibilityAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiEmployeeportal.ListAnnouncementSubscriptionsResponse
        {
            Announcements = { new ApiEmployeeportal.Announcement
            {
                Counterparty = new ApiEmployeeportal.PortalHomeRef
                {
                    Priority = (reply0.Notification?.Priority ?? 0),
                },
                DueAt = (reply0.Notification?.Sla?.DueAt ?? new Timestamp()),
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "announcement",
            "audit", "ListAnnouncementSubscriptions", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Notifys an announcement.
    /// </summary>
    public override async Task<ApiEmployeeportal.NotifyAnnouncementWatchersResponse> NotifyAnnouncementWatchers(ApiEmployeeportal.NotifyAnnouncementWatchersRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling NotifyAnnouncementWatchers for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "announcement", request.AnnouncementId,
            "NotifyAnnouncementWatchers", "accepted", token);
        var profile0 = new MappingProfile<ApiEmployeeportal.NotifyAnnouncementWatchersRequest, ApiDocuments.ListSignatureConflictsRequest>(value => new ApiDocuments.ListSignatureConflictsRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListSignatureConflictsRequest.Types.Filter
            {
                CreatedAfter = (value.Context?.Deadline ?? new Timestamp()),
                Labels = { (value.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (value.Details?.TemplateId ?? ""),
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _documentsSignatureSyncService.ListSignatureConflictsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployeeportal.SubscribePortalHomeUpdatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployeeportal.SubscribePortalHomeUpdatesRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Recipient?.Email ?? ""),
                    Name = (request.Details?.Recipient?.Name ?? new ApiCommon.PersonName()),
                    PreferredChannel = (request.Details?.Recipient?.PreferredChannel ?? ""),
                },
                TemplateId = (request.Details?.TemplateId ?? ""),
            },
        };
        using var rpc1 = _employeeportalPortalHomeNotificationService.SubscribePortalHomeUpdatesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiEmployeeportal.DisconnectSelfServiceRequestSourceRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployeeportal.DisconnectSelfServiceRequestSourceRequest.Types.Details
            {
                Envelope = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
            Reason = (request.Reason ?? ""),
        };
        using var rpc2 = _employeeportalSelfServiceRequestIntegrationService.DisconnectSelfServiceRequestSourceAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiEmployeeportal.NotifyAnnouncementWatchersResponse
        {
            Announcement = new ApiEmployeeportal.Announcement
            {
                AnnouncementId = (request.AnnouncementId ?? ""),
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply2.SelfServiceRequest?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Counterparty = new ApiEmployeeportal.PortalHomeRef
                {
                    PortalHomeId = (reply1.PortalHome?.PortalHomeId ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.Recipient?.Phone ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "announcement",
            "audit", "NotifyAnnouncementWatchers", (request.Details?.Recipient?.Phone ?? ""), token);
        return response;
    }
    /// <summary>
    /// Sends an announcement.
    /// </summary>
    public override async Task<ApiEmployeeportal.SendAnnouncementTestNoticeResponse> SendAnnouncementTestNotice(ApiEmployeeportal.SendAnnouncementTestNoticeRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SendAnnouncementTestNotice for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "announcement", request.AnnouncementId,
            "SendAnnouncementTestNotice", "accepted", token);
        var input0 = new ApiEmployeeportal.ListPortalHomeSubscriptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployeeportal.ListPortalHomeSubscriptionsRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
                Labels = { (request.Details?.Variables ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.AnnouncementId ?? ""),
            },
        };
        var reply0 = await SendSendAnnouncementTestNotice0Async(input0, token);
        var input1 = new ApiEmployeeportal.ConnectAnnouncementSourceRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployeeportal.ConnectAnnouncementSourceRequest.Types.Details
            {
                Envelope = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
            Reason = (request.Reason ?? ""),
        };
        using var rpc1 = _employeeportalAnnouncementIntegrationService.ConnectAnnouncementSourceAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiEmployeeportal.SendAnnouncementTestNoticeResponse
        {
            Announcement = new ApiEmployeeportal.Announcement
            {
                AnnouncementId = (request.AnnouncementId ?? ""),
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (reply1.Announcement?.BillingAddress?.Locality ?? ""),
                },
                Number = (reply1.Announcement?.Number ?? ""),
            },
            Outcome = new ApiEmployeeportal.SendAnnouncementTestNoticeResponse.Types.Outcome
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.TemplateId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "announcement",
            "audit", "SendAnnouncementTestNotice", (request.Details?.TemplateId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams the changes of announcements as they happen.
    /// </summary>
    public override async Task StreamAnnouncementNotices(ApiEmployeeportal.StreamAnnouncementNoticesRequest request, IServerStreamWriter<ApiEmployeeportal.StreamAnnouncementNoticesResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling StreamAnnouncementNotices for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "announcement", "batch",
            "StreamAnnouncementNotices", "accepted", token);
        var input0 = new ApiEmployeeportal.TestAnnouncementConnectionRequest
        {
            Annotations = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployeeportal.TestAnnouncementConnectionRequest.Types.Details
            {
                Envelope = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                MessageId = (request.Filter?.OwnerId ?? ""),
                ReceivedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
            },
        };
        using var rpc0 = _employeeportalAnnouncementIntegrationService.TestAnnouncementConnectionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiMetrics.BrowseMetricSeriesesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiMetrics.BrowseMetricSeriesesRequest.Types.Filter
            {
                CreatedAfter = (request.Filter?.CreatedAfter ?? new Timestamp()),
                Labels = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Query = (request.Filter?.Query ?? ""),
                Region = (reply0.Announcement?.BillingAddress?.Region ?? ""),
            },
        };
        var json1 = JsonFormatter.Default.Format(input1);
        input1 = JsonParser.Default.Parse<ApiMetrics.BrowseMetricSeriesesRequest>(json1);
        using var rpc1 = _metricsMetricSeriesQueryService.BrowseMetricSeriesesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiNotifications.ValidateNotificationRequest();
        input2.Context = (request.Context ?? new ApiCommon.RequestContext());
        input2.Notification = new ApiNotifications.Notification
        {
            Audit = new ApiCommon.AuditInfo
            {
                Etag = (reply0.Announcement?.Audit?.Etag ?? ""),
            },
            Comments = { new ApiNotifications.NotificationComment
            {
                CreatedAt = (reply0.Announcement?.Audit?.CreatedAt ?? new Timestamp()),
            } },
        };
        using var rpc2 = _notificationsNotificationValidationService.ValidateNotificationAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiEmployeeportal.StreamAnnouncementNoticesResponse
        {
            Announcement = new ApiEmployeeportal.Announcement
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
                Counterparty = new ApiEmployeeportal.PortalHomeRef
                {
                    DisplayName = (reply0.Announcement?.Counterparty?.DisplayName ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Subscriber?.PreferredChannel ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "announcement",
            "audit", "StreamAnnouncementNotices", (request.Subscriber?.PreferredChannel ?? ""), token);
        await responses.WriteAsync(response, token);
    }
    /// <summary>
    /// Subscribes an announcement.
    /// </summary>
    public override async Task<ApiEmployeeportal.SubscribeAnnouncementUpdatesResponse> SubscribeAnnouncementUpdates(ApiEmployeeportal.SubscribeAnnouncementUpdatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SubscribeAnnouncementUpdates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "announcement", request.AnnouncementId,
            "SubscribeAnnouncementUpdates", "accepted", token);
        var input0 = new ApiPatientapp.ListPatientHomesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiPatientapp.ListPatientHomesRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
                Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Details?.TemplateId ?? ""),
            },
        };
        using var rpc0 = _patientappPatientHomeService.ListPatientHomesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiPatientapp.SuggestPatientHomesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiPatientapp.SuggestPatientHomesRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
                Labels = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Details?.TemplateId ?? ""),
            },
        };
        var reply1 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _patientappPatientHomeQueryService.SuggestPatientHomesAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input2 = new ApiNotifications.ScoreNotificationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Notification = new ApiNotifications.Notification
            {
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Recipient?.Email ?? ""),
                },
            },
        };
        var reply2 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _notificationsNotificationValidationService.ScoreNotificationAsync(input2, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input3 = new ApiDevices.AdvanceDevicePolicyWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply2.Notification?.Requester?.Email ?? ""),
                    UserId = (reply2.Notification?.Assignee?.UserId ?? ""),
                },
            },
        };
        var reply3 = await Workflow.SendAsync(input3, token, _devicesDevicePolicyWorkflowService.AdvanceDevicePolicyWorkflowAsync);
        var response = new ApiEmployeeportal.SubscribeAnnouncementUpdatesResponse
        {
            Announcement = new ApiEmployeeportal.Announcement
            {
                AnnouncementId = (request.AnnouncementId ?? ""),
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply2.Notification?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedAt = (reply3.DevicePolicy?.Audit?.UpdatedAt ?? new Timestamp()),
                    UpdatedBy = (reply3.DevicePolicy?.Audit?.UpdatedBy ?? ""),
                },
            },
            Outcome = new ApiEmployeeportal.SubscribeAnnouncementUpdatesResponse.Types.Outcome
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "announcement",
            "audit", "SubscribeAnnouncementUpdates", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Unsubscribes an announcement.
    /// </summary>
    public override async Task<ApiEmployeeportal.UnsubscribeAnnouncementUpdatesResponse> UnsubscribeAnnouncementUpdates(ApiEmployeeportal.UnsubscribeAnnouncementUpdatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling UnsubscribeAnnouncementUpdates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "announcement", request.AnnouncementId,
            "UnsubscribeAnnouncementUpdates", "accepted", token);
        var input0 = new ApiEmployeeportal.SignalAnnouncementWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployeeportal.SignalAnnouncementWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Recipient?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.Details?.Recipient?.Phone ?? ""),
                    PreferredChannel = (request.Details?.Recipient?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc0 = _sendUnsubscribeAnnouncementUpdates0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.StreamDocumentFolderNoticesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.StreamDocumentFolderNoticesRequest.Types.Filter
            {
                Region = (reply0.Announcement?.BillingAddress?.Region ?? ""),
            },
            Subscriber = new ApiCommon.ContactInfo
            {
                Email = (request.Details?.Recipient?.Email ?? ""),
                Name = (request.Details?.Recipient?.Name ?? new ApiCommon.PersonName()),
                Phone = (request.Details?.Recipient?.Phone ?? ""),
            },
        };
        using var stream1 = _documentsDocumentFolderNotificationService.StreamDocumentFolderNotices(input1, cancellationToken: token);
        await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received StreamDocumentFolderNotices frame {Type}", change1.GetType().Name);
        }
        var input2 = new ApiEmployeeportal.ListSelfServiceRequestSubscriptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployeeportal.ListSelfServiceRequestSubscriptionsRequest.Types.Filter
            {
                CreatedAfter = (reply0.Announcement?.DueAt ?? new Timestamp()),
                Labels = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Details?.TemplateId ?? ""),
                Region = (reply0.Announcement?.BillingAddress?.Region ?? ""),
            },
        };
        var descriptor2 = new Method<ApiEmployeeportal.ListSelfServiceRequestSubscriptionsRequest, ApiEmployeeportal.ListSelfServiceRequestSubscriptionsResponse>(
            MethodType.Unary, "acme.employeeportal.v1" + ".SelfServiceRequestNotificationService", "ListSelfServiceRequestSubscriptions",
            Wire.Marshaller<ApiEmployeeportal.ListSelfServiceRequestSubscriptionsRequest>(), Wire.Marshaller<ApiEmployeeportal.ListSelfServiceRequestSubscriptionsResponse>());
        using var rpc2 = _invoker.AsyncUnaryCall(descriptor2, null,
            new CallOptions(cancellationToken: token), input2);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiEmployeeportal.UnsubscribeAnnouncementUpdatesResponse
        {
            Announcement = new ApiEmployeeportal.Announcement
            {
                AnnouncementId = (request.AnnouncementId ?? ""),
                Number = (reply0.Announcement?.Number ?? ""),
            },
            Outcome = new ApiEmployeeportal.UnsubscribeAnnouncementUpdatesResponse.Types.Outcome
            {
                NextSteps = { (reply0.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply2.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply2.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "announcement",
            "audit", "UnsubscribeAnnouncementUpdates", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiEmployeeportal.ListPortalHomeSubscriptionsResponse> SendSendAnnouncementTestNotice0Async(ApiEmployeeportal.ListPortalHomeSubscriptionsRequest request, CancellationToken token)
    {
        return await _announcementNotificationEndpointSendSendAnnouncementTestNotice0Port.SendAsync(request, token);
    }
}
