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
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiPatientapp = Acme.Patientapp.V1;
using ApiSearch = Acme.Search.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Employeeportal;

/// <summary>
/// Tells the people who follow a portal home what changed.
/// </summary>
public sealed class PortalHomeNotificationEndpoint : ApiEmployeeportal.PortalHomeNotificationService.PortalHomeNotificationServiceBase
{
    private readonly ApiCorporateevents.VenueReportingService.VenueReportingServiceClient _corporateeventsVenueReportingService;
    private readonly ApiDocuments.DocumentFolderQueryService.DocumentFolderQueryServiceClient _documentsDocumentFolderQueryService;
    private readonly ApiDocuments.DocumentTemplateExportService.DocumentTemplateExportServiceClient _documentsDocumentTemplateExportService;
    private readonly ApiDocuments.DocumentTemplateNotificationService.DocumentTemplateNotificationServiceClient _documentsDocumentTemplateNotificationService;
    private readonly ApiDocuments.SignatureValidationService.SignatureValidationServiceClient _documentsSignatureValidationService;
    private readonly ApiEmployeeportal.AnnouncementWorkflowService.AnnouncementWorkflowServiceClient _employeeportalAnnouncementWorkflowService;
    private readonly ApiEmployeeportal.PortalHomeIntegrationService.PortalHomeIntegrationServiceClient _employeeportalPortalHomeIntegrationService;
    private readonly ApiIdentity.UserService.UserServiceClient _identityUserService;
    private readonly ILogger<PortalHomeNotificationEndpoint> _logger;
    private readonly ApiPatientapp.PatientHomeService.PatientHomeServiceClient _patientappPatientHomeService;
    private readonly RetryPolicy _policy;
    private readonly IPortalHomeNotificationEndpointSendSubscribePortalHomeUpdates1Port _portalHomeNotificationEndpointSendSubscribePortalHomeUpdates1Port;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiSearch.SearchIndexValidationService.SearchIndexValidationServiceClient _searchSearchIndexValidationService;
    private readonly Func<ApiIdentity.SearchUsersRequest, CallOptions, AsyncUnaryCall<ApiIdentity.SearchUsersResponse>> _sendNotifyPortalHomeWatchers2;
    private readonly ApiTravel.TravelPolicyReportingService.TravelPolicyReportingServiceClient _travelTravelPolicyReportingService;

    public PortalHomeNotificationEndpoint(
        ApiCorporateevents.VenueReportingService.VenueReportingServiceClient corporateeventsVenueReportingService,
        ApiDocuments.DocumentFolderQueryService.DocumentFolderQueryServiceClient documentsDocumentFolderQueryService,
        ApiDocuments.DocumentTemplateExportService.DocumentTemplateExportServiceClient documentsDocumentTemplateExportService,
        ApiDocuments.DocumentTemplateNotificationService.DocumentTemplateNotificationServiceClient documentsDocumentTemplateNotificationService,
        ApiDocuments.SignatureValidationService.SignatureValidationServiceClient documentsSignatureValidationService,
        ApiEmployeeportal.AnnouncementWorkflowService.AnnouncementWorkflowServiceClient employeeportalAnnouncementWorkflowService,
        ApiEmployeeportal.PortalHomeIntegrationService.PortalHomeIntegrationServiceClient employeeportalPortalHomeIntegrationService,
        ApiIdentity.UserService.UserServiceClient identityUserService,
        ILogger<PortalHomeNotificationEndpoint> logger,
        ApiPatientapp.PatientHomeService.PatientHomeServiceClient patientappPatientHomeService,
        RetryPolicy policy,
        IPortalHomeNotificationEndpointSendSubscribePortalHomeUpdates1Port portalHomeNotificationEndpointSendSubscribePortalHomeUpdates1PortAdapter,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiSearch.SearchIndexValidationService.SearchIndexValidationServiceClient searchSearchIndexValidationService,
        ApiTravel.TravelPolicyReportingService.TravelPolicyReportingServiceClient travelTravelPolicyReportingService)
    {
        _corporateeventsVenueReportingService = corporateeventsVenueReportingService;
        _documentsDocumentFolderQueryService = documentsDocumentFolderQueryService;
        _documentsDocumentTemplateExportService = documentsDocumentTemplateExportService;
        _documentsDocumentTemplateNotificationService = documentsDocumentTemplateNotificationService;
        _documentsSignatureValidationService = documentsSignatureValidationService;
        _employeeportalAnnouncementWorkflowService = employeeportalAnnouncementWorkflowService;
        _employeeportalPortalHomeIntegrationService = employeeportalPortalHomeIntegrationService;
        _identityUserService = identityUserService;
        _logger = logger;
        _patientappPatientHomeService = patientappPatientHomeService;
        _policy = policy;
        _portalHomeNotificationEndpointSendSubscribePortalHomeUpdates1Port = portalHomeNotificationEndpointSendSubscribePortalHomeUpdates1PortAdapter;
        _registry = registry;
        _repository = repository;
        _searchSearchIndexValidationService = searchSearchIndexValidationService;
        _travelTravelPolicyReportingService = travelTravelPolicyReportingService;
        _sendNotifyPortalHomeWatchers2 = _identityUserService.SearchUsersAsync;
    }
    /// <summary>
    /// Returns a portal home.
    /// </summary>
    public override async Task<ApiEmployeeportal.GetPortalHomePreferencesResponse> GetPortalHomePreferences(ApiEmployeeportal.GetPortalHomePreferencesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.PortalHomeId, "portal_home_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetPortalHomePreferences for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "portal_home", request.PortalHomeId,
            "GetPortalHomePreferences", "accepted", token);
        var input0 = new ApiEmployeeportal.ReplayPortalHomeEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployeeportal.ReplayPortalHomeEventsRequest.Types.Details
            {
                Envelope = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
            PortalHomeId = (request.PortalHomeId ?? ""),
        };
        using var rpc0 = _employeeportalPortalHomeIntegrationService.ReplayPortalHomeEventsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiEmployeeportal.GetPortalHomePreferencesResponse
        {
            PortalHome = new ApiEmployeeportal.PortalHome
            {
                PortalHomeId = (reply0.PortalHome?.PortalHomeId ?? ""),
                Tags = { (reply0.PortalHome?.Tags ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        return response;
    }
    /// <summary>
    /// Lists portal homes a page at a time.
    /// </summary>
    public override async Task<ApiEmployeeportal.ListPortalHomeSubscriptionsResponse> ListPortalHomeSubscriptions(ApiEmployeeportal.ListPortalHomeSubscriptionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ListPortalHomeSubscriptions for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "portal_home", "batch",
            "ListPortalHomeSubscriptions", "accepted", token);
        var response = new ApiEmployeeportal.ListPortalHomeSubscriptionsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "portal_home",
            "audit", "ListPortalHomeSubscriptions", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Notifys a portal home.
    /// </summary>
    public override async Task<ApiEmployeeportal.NotifyPortalHomeWatchersResponse> NotifyPortalHomeWatchers(ApiEmployeeportal.NotifyPortalHomeWatchersRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling NotifyPortalHomeWatchers for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "portal_home", request.PortalHomeId,
            "NotifyPortalHomeWatchers", "accepted", token);
        var input0 = new ApiDocuments.ResolveDocumentFoldersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _documentsDocumentFolderQueryService.ResolveDocumentFoldersAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiCorporateevents.ScheduleVenueReportRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply1 = await Workflow.SendAsync(input1, token, _corporateeventsVenueReportingService.ScheduleVenueReportAsync);
        var input2 = new ApiIdentity.SearchUsersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiIdentity.SearchUsersRequest.Types.Filter
            {
                Labels = { (request.Details?.Variables ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Details?.TemplateId ?? ""),
            },
        };
        using var rpc2 = _sendNotifyPortalHomeWatchers2(input2, new CallOptions(cancellationToken: token));
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiEmployeeportal.NotifyPortalHomeWatchersResponse
        {
            Outcome = new ApiEmployeeportal.NotifyPortalHomeWatchersResponse.Types.Outcome
            {
                Status = (reply1.Result?.Status ?? ""),
            },
            PortalHome = new ApiEmployeeportal.PortalHome
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Recipient?.Email ?? ""),
                },
                PortalHomeId = (request.PortalHomeId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "portal_home",
            "audit", "NotifyPortalHomeWatchers", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Sends a portal home.
    /// </summary>
    public override async Task<ApiEmployeeportal.SendPortalHomeTestNoticeResponse> SendPortalHomeTestNotice(ApiEmployeeportal.SendPortalHomeTestNoticeRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SendPortalHomeTestNotice for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "portal_home", request.PortalHomeId,
            "SendPortalHomeTestNotice", "accepted", token);
        var input0 = new ApiPatientapp.BatchGetPatientHomesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var named0 = _registry.Get<ApiPatientapp.PatientHomeService.PatientHomeServiceClient>("acme.patientapp.v1.PatientHomeService");
        using var rpc0 = named0.BatchGetPatientHomesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.UnsubscribeDocumentTemplateUpdatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.UnsubscribeDocumentTemplateUpdatesRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Recipient?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _documentsDocumentTemplateNotificationService.UnsubscribeDocumentTemplateUpdatesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiEmployeeportal.ReplayPortalHomeEventsRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployeeportal.ReplayPortalHomeEventsRequest.Types.Details
            {
                Envelope = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
            PortalHomeId = (request.PortalHomeId ?? ""),
        };
        using var rpc2 = _employeeportalPortalHomeIntegrationService.ReplayPortalHomeEventsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiEmployeeportal.SendPortalHomeTestNoticeResponse
        {
            PortalHome = new ApiEmployeeportal.PortalHome
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.DocumentTemplate?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                PortalHomeId = (request.PortalHomeId ?? ""),
                Requester = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Details?.Recipient?.PreferredChannel ?? ""),
                },
                Subject = (reply2.PortalHome?.Subject ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply2.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "portal_home",
            "audit", "SendPortalHomeTestNotice", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams the changes of portal homes as they happen.
    /// </summary>
    public override async Task StreamPortalHomeNotices(ApiEmployeeportal.StreamPortalHomeNoticesRequest request, IServerStreamWriter<ApiEmployeeportal.StreamPortalHomeNoticesResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling StreamPortalHomeNotices for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "portal_home", "batch",
            "StreamPortalHomeNotices", "accepted", token);
        var profile0 = new MappingProfile<ApiEmployeeportal.StreamPortalHomeNoticesRequest, ApiEmployeeportal.PauseAnnouncementWorkflowRequest>(value => new ApiEmployeeportal.PauseAnnouncementWorkflowRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployeeportal.PauseAnnouncementWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Name = (value.Subscriber?.Name ?? new ApiCommon.PersonName()),
                },
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _employeeportalAnnouncementWorkflowService.PauseAnnouncementWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployeeportal.DisconnectPortalHomeSourceRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployeeportal.DisconnectPortalHomeSourceRequest.Types.Details
            {
                Envelope = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
            PortalHomeId = (reply0.Announcement?.Counterparty?.PortalHomeId ?? ""),
        };
        using var rpc1 = _employeeportalPortalHomeIntegrationService.DisconnectPortalHomeSourceAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiEmployeeportal.StreamPortalHomeNoticesResponse
        {
            PortalHome = new ApiEmployeeportal.PortalHome
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (request.Subscriber?.Email ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.Announcement?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Requester = (reply1.PortalHome?.Requester ?? new ApiCommon.ContactInfo()),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Subscriber?.PreferredChannel ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "portal_home",
            "audit", "StreamPortalHomeNotices", (request.Subscriber?.PreferredChannel ?? ""), token);
        await responses.WriteAsync(response, token);
    }
    /// <summary>
    /// Subscribes a portal home.
    /// </summary>
    public override async Task<ApiEmployeeportal.SubscribePortalHomeUpdatesResponse> SubscribePortalHomeUpdates(ApiEmployeeportal.SubscribePortalHomeUpdatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SubscribePortalHomeUpdates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "portal_home", request.PortalHomeId,
            "SubscribePortalHomeUpdates", "accepted", token);
        var input0 = new ApiDocuments.NormalizeSignatureRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.NormalizeSignatureRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Recipient?.Email ?? ""),
                },
            },
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
        };
        using var rpc0 = _documentsSignatureValidationService.NormalizeSignatureAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiSearch.PreviewSearchIndexRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiSearch.PreviewSearchIndexRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.Recipient?.Phone ?? ""),
                },
            },
            SearchIndex = new ApiSearch.SearchIndex
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Recipient?.Email ?? ""),
                },
            },
        };
        var reply1 = await SendSubscribePortalHomeUpdates1Async(input1, token);
        var input2 = new ApiDocuments.StreamDocumentTemplateExportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.StreamDocumentTemplateExportRequest.Types.Filter
            {
                CreatedAfter = (reply1.Outcome?.EffectiveAt ?? new Timestamp()),
                Region = (reply1.SearchIndex?.Address?.Region ?? ""),
            },
        };
        using var stream2 = _documentsDocumentTemplateExportService.StreamDocumentTemplateExport(input2, cancellationToken: token);
        await foreach (var change2 in stream2.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received StreamDocumentTemplateExport frame {Type}", change2.GetType().Name);
        }
        var response = new ApiEmployeeportal.SubscribePortalHomeUpdatesResponse
        {
            PortalHome = new ApiEmployeeportal.PortalHome
            {
                Comments = { new ApiEmployeeportal.PortalHomeComment
                {
                    CreatedAt = (reply1.SearchIndex?.Audit?.CreatedAt ?? new Timestamp()),
                } },
                PortalHomeId = (request.PortalHomeId ?? ""),
                Requester = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Recipient?.Name ?? new ApiCommon.PersonName()),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "portal_home",
            "audit", "SubscribePortalHomeUpdates", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Unsubscribes a portal home.
    /// </summary>
    public override async Task<ApiEmployeeportal.UnsubscribePortalHomeUpdatesResponse> UnsubscribePortalHomeUpdates(ApiEmployeeportal.UnsubscribePortalHomeUpdatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling UnsubscribePortalHomeUpdates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "portal_home", request.PortalHomeId,
            "UnsubscribePortalHomeUpdates", "accepted", token);
        var input0 = new ApiTravel.GenerateTravelPolicyReportRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.GenerateTravelPolicyReportRequest.Types.Details
            {
                Filters = { (request.Details?.Variables ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Reason = (request.Reason ?? ""),
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiTravel.GenerateTravelPolicyReportRequest>();
        using var rpc0 = _travelTravelPolicyReportingService.GenerateTravelPolicyReportAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployeeportal.IngestPortalHomeEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Header = new ApiEmployeeportal.IngestPortalHomeEventsRequest.Types.StreamHeader
            {
                SubmittedBy = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Recipient?.Email ?? ""),
                    Phone = (request.Details?.Recipient?.Phone ?? ""),
                    PreferredChannel = (request.Details?.Recipient?.PreferredChannel ?? ""),
                },
            },
        };
        using var stream1 = _employeeportalPortalHomeIntegrationService.IngestPortalHomeEvents(cancellationToken: token);
        await stream1.RequestStream.WriteAsync(input1, token);
        await stream1.RequestStream.CompleteAsync();
        var reply1 = await stream1.ResponseAsync;
        var input2 = new ApiEmployeeportal.ListPortalHomeSourcesRequest();
        input2.Context = (request.Context ?? new ApiCommon.RequestContext());
        input2.Filter = new ApiEmployeeportal.ListPortalHomeSourcesRequest.Types.Filter
        {
            CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
            Labels = { (request.Details?.Variables ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            OwnerId = (reply0.ReportId ?? ""),
        };
        using var rpc2 = _employeeportalPortalHomeIntegrationService.ListPortalHomeSourcesAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiEmployeeportal.UnsubscribePortalHomeUpdatesResponse
        {
            PortalHome = new ApiEmployeeportal.PortalHome
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Recipient?.Email ?? ""),
                },
                PortalHomeId = (request.PortalHomeId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.TemplateId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "portal_home",
            "audit", "UnsubscribePortalHomeUpdates", (request.Details?.TemplateId ?? ""), token);
        return response;
    }
    private async Task<ApiSearch.PreviewSearchIndexResponse> SendSubscribePortalHomeUpdates1Async(ApiSearch.PreviewSearchIndexRequest request, CancellationToken token)
    {
        return await _portalHomeNotificationEndpointSendSubscribePortalHomeUpdates1Port.SendAsync(request, token);
    }
}
