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
using ApiBenefits = Acme.Benefits.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiDriverapp = Acme.Driverapp.V1;
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiVenues = Acme.Venues.V1;
using ApiWebbff = Acme.Webbff.V1;

namespace Acme.Workforce.Employeeportal;

/// <summary>
/// Manages self service requests: creates, reads, changes, and lists them.
/// </summary>
public sealed class SelfServiceRequestEndpoint : ApiEmployeeportal.SelfServiceRequestService.SelfServiceRequestServiceBase
{
    private readonly ApiAudit.AuditEventValidationService.AuditEventValidationServiceClient _auditAuditEventValidationService;
    private readonly ApiBenefits.BenefitPlanWorkflowService.BenefitPlanWorkflowServiceClient _benefitsBenefitPlanWorkflowService;
    private readonly ApiCorporateevents.VenueReportingService.VenueReportingServiceClient _corporateeventsVenueReportingService;
    private readonly ApiDocuments.DocumentHistoryService.DocumentHistoryServiceClient _documentsDocumentHistoryService;
    private readonly ApiDocuments.SignatureSyncService.SignatureSyncServiceClient _documentsSignatureSyncService;
    private readonly ApiDriverapp.DriverHomeService.DriverHomeServiceClient _driverappDriverHomeService;
    private readonly ApiEmployeeportal.AnnouncementNotificationService.AnnouncementNotificationServiceClient _employeeportalAnnouncementNotificationService;
    private readonly ApiEmployeeportal.AnnouncementWorkflowService.AnnouncementWorkflowServiceClient _employeeportalAnnouncementWorkflowService;
    private readonly ApiEmployeeportal.PortalHomeQueryService.PortalHomeQueryServiceClient _employeeportalPortalHomeQueryService;
    private readonly ApiEmployeeportal.PortalHomeService.PortalHomeServiceClient _employeeportalPortalHomeService;
    private readonly ApiEmployeeportal.SelfServiceRequestNotificationService.SelfServiceRequestNotificationServiceClient _employeeportalSelfServiceRequestNotificationService;
    private readonly ApiEmployeeportal.SelfServiceRequestQueryService.SelfServiceRequestQueryServiceClient _employeeportalSelfServiceRequestQueryService;
    private readonly ApiIdentity.UserService.UserServiceClient _identityUserService;
    private readonly ApiLearning.CourseWorkflowService.CourseWorkflowServiceClient _learningCourseWorkflowService;
    private readonly ILogger<SelfServiceRequestEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly IRecordRepository _repository;
    private readonly ISelfServiceRequestEndpointSendListSelfServiceRequests0Port _selfServiceRequestEndpointSendListSelfServiceRequests0Port;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiVenues.VenueService.VenueServiceClient _venuesVenueService;
    private readonly ApiWebbff.PageCompositionQueryService.PageCompositionQueryServiceClient _webbffPageCompositionQueryService;

    public SelfServiceRequestEndpoint(
        ApiAudit.AuditEventValidationService.AuditEventValidationServiceClient auditAuditEventValidationService,
        ApiBenefits.BenefitPlanWorkflowService.BenefitPlanWorkflowServiceClient benefitsBenefitPlanWorkflowService,
        ApiCorporateevents.VenueReportingService.VenueReportingServiceClient corporateeventsVenueReportingService,
        ApiDocuments.DocumentHistoryService.DocumentHistoryServiceClient documentsDocumentHistoryService,
        ApiDocuments.SignatureSyncService.SignatureSyncServiceClient documentsSignatureSyncService,
        ApiDriverapp.DriverHomeService.DriverHomeServiceClient driverappDriverHomeService,
        ApiEmployeeportal.AnnouncementNotificationService.AnnouncementNotificationServiceClient employeeportalAnnouncementNotificationService,
        ApiEmployeeportal.AnnouncementWorkflowService.AnnouncementWorkflowServiceClient employeeportalAnnouncementWorkflowService,
        ApiEmployeeportal.PortalHomeQueryService.PortalHomeQueryServiceClient employeeportalPortalHomeQueryService,
        ApiEmployeeportal.PortalHomeService.PortalHomeServiceClient employeeportalPortalHomeService,
        ApiEmployeeportal.SelfServiceRequestNotificationService.SelfServiceRequestNotificationServiceClient employeeportalSelfServiceRequestNotificationService,
        ApiEmployeeportal.SelfServiceRequestQueryService.SelfServiceRequestQueryServiceClient employeeportalSelfServiceRequestQueryService,
        ApiIdentity.UserService.UserServiceClient identityUserService,
        ApiLearning.CourseWorkflowService.CourseWorkflowServiceClient learningCourseWorkflowService,
        ILogger<SelfServiceRequestEndpoint> logger,
        RetryPolicy policy,
        IRecordRepository repository,
        ISelfServiceRequestEndpointSendListSelfServiceRequests0Port selfServiceRequestEndpointSendListSelfServiceRequests0PortAdapter,
        IOptions<MeshOptions> settings,
        ApiVenues.VenueService.VenueServiceClient venuesVenueService,
        ApiWebbff.PageCompositionQueryService.PageCompositionQueryServiceClient webbffPageCompositionQueryService)
    {
        _auditAuditEventValidationService = auditAuditEventValidationService;
        _benefitsBenefitPlanWorkflowService = benefitsBenefitPlanWorkflowService;
        _corporateeventsVenueReportingService = corporateeventsVenueReportingService;
        _documentsDocumentHistoryService = documentsDocumentHistoryService;
        _documentsSignatureSyncService = documentsSignatureSyncService;
        _driverappDriverHomeService = driverappDriverHomeService;
        _employeeportalAnnouncementNotificationService = employeeportalAnnouncementNotificationService;
        _employeeportalAnnouncementWorkflowService = employeeportalAnnouncementWorkflowService;
        _employeeportalPortalHomeQueryService = employeeportalPortalHomeQueryService;
        _employeeportalPortalHomeService = employeeportalPortalHomeService;
        _employeeportalSelfServiceRequestNotificationService = employeeportalSelfServiceRequestNotificationService;
        _employeeportalSelfServiceRequestQueryService = employeeportalSelfServiceRequestQueryService;
        _identityUserService = identityUserService;
        _learningCourseWorkflowService = learningCourseWorkflowService;
        _logger = logger;
        _policy = policy;
        _repository = repository;
        _selfServiceRequestEndpointSendListSelfServiceRequests0Port = selfServiceRequestEndpointSendListSelfServiceRequests0PortAdapter;
        _settings = settings;
        _venuesVenueService = venuesVenueService;
        _webbffPageCompositionQueryService = webbffPageCompositionQueryService;
    }
    /// <summary>
    /// Returns several self service requests at once.
    /// </summary>
    public override async Task<ApiEmployeeportal.BatchGetSelfServiceRequestsResponse> BatchGetSelfServiceRequests(ApiEmployeeportal.BatchGetSelfServiceRequestsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling BatchGetSelfServiceRequests for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "self_service_request", "batch",
            "BatchGetSelfServiceRequests", "accepted", token);
        var input0 = new ApiAudit.CheckAuditEventEligibilityRequest
        {
            AuditEvent = new ApiAudit.AuditEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiAudit.CheckAuditEventEligibilityRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Locale = (request.Context?.Locale ?? ""),
            },
        };
        using var rpc0 = _auditAuditEventValidationService.CheckAuditEventEligibilityAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiWebbff.BrowsePageCompositionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiWebbff.BrowsePageCompositionsRequest.Types.Filter
            {
                CreatedAfter = (reply0.Outcome?.EffectiveAt ?? new Timestamp()),
                Labels = { (reply0.AuditEvent?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply0.AuditEvent?.Subject?.AuditTrailId ?? ""),
            },
        };
        using var rpc1 = _webbffPageCompositionQueryService.BrowsePageCompositionsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiEmployeeportal.BatchGetSelfServiceRequestsResponse
        {
            PageInfo = new ApiCommon.PageInfo
            {
                TotalSize = (reply1.PageInfo?.TotalSize ?? 0L),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        return response;
    }
    /// <summary>
    /// Creates a self service request.
    /// </summary>
    public override async Task<ApiEmployeeportal.CloneSelfServiceRequestResponse> CloneSelfServiceRequest(ApiEmployeeportal.CloneSelfServiceRequestRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CloneSelfServiceRequest for {Tenant}", requestContext.TenantId);
        if (request.SelfServiceRequest is { } record)
        {
            var snapshot = SelfServiceRequestMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        foreach (var item in (request.SelfServiceRequest?.History ?? new Google.Protobuf.Collections.RepeatedField<ApiEmployeeportal.SelfServiceRequestStatusChange>()))
        {
            var input0 = new ApiEmployeeportal.StreamAnnouncementNoticesRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiEmployeeportal.StreamAnnouncementNoticesRequest.Types.Filter
                {
                    CreatedAfter = (item.ChangedAt ?? new Timestamp()),
                },
                Subscriber = new ApiCommon.ContactInfo
                {
                    Name = (request.SelfServiceRequest?.Requester?.Name ?? new ApiCommon.PersonName()),
                    PreferredChannel = (request.SelfServiceRequest?.Requester?.PreferredChannel ?? ""),
                },
            };
            using var stream0 = _employeeportalAnnouncementNotificationService.StreamAnnouncementNotices(input0, cancellationToken: token);
            await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
            {
                _logger.LogDebug("Received StreamAnnouncementNotices frame {Type}", change0.GetType().Name);
            }
        }
        var input1 = new ApiLearning.PauseCourseWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Course = new ApiLearning.Course
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.SelfServiceRequest?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.SelfServiceRequest?.Assignee?.UserId ?? ""),
                },
            },
            Details = new ApiLearning.PauseCourseWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.SelfServiceRequest?.Requester?.Email ?? ""),
                },
            },
        };
        var reply1 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _learningCourseWorkflowService.PauseCourseWorkflowAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input2 = new ApiDocuments.RevertDocumentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.RevertDocumentRequest.Types.Details
            {
                Snapshot = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Document = new ApiDocuments.Document
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply1.Course?.Audit?.CreatedBy ?? ""),
                },
            },
        };
        using var rpc2 = _documentsDocumentHistoryService.RevertDocumentAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiEmployeeportal.CloneSelfServiceRequestResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply2.Outcome?.Status ?? ""),
            },
            SelfServiceRequest = new ApiEmployeeportal.SelfServiceRequest
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply2.Document?.Audit?.CreatedBy ?? ""),
                    UpdatedBy = (reply1.Course?.Audit?.UpdatedBy ?? ""),
                },
                SelfServiceRequestId = (request.SelfServiceRequest?.SelfServiceRequestId ?? ""),
                Status = (request.SelfServiceRequest?.Status ?? (ApiEmployeeportal.SelfServiceRequestStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SelfServiceRequest?.Subject ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "self_service_request",
            "audit", "CloneSelfServiceRequest", (request.SelfServiceRequest?.Subject ?? ""), token);
        return response;
    }
    /// <summary>
    /// Creates a self service request.
    /// </summary>
    public override async Task<ApiEmployeeportal.CreateSelfServiceRequestResponse> CreateSelfServiceRequest(ApiEmployeeportal.CreateSelfServiceRequestRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CreateSelfServiceRequest for {Tenant}", requestContext.TenantId);
        if (request.SelfServiceRequest is { } record)
        {
            var snapshot = SelfServiceRequestMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDriverapp.SearchDriverHomesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDriverapp.SearchDriverHomesRequest.Types.Filter
            {
                CreatedAfter = (request.SelfServiceRequest?.Audit?.CreatedAt ?? new Timestamp()),
                OwnerId = (request.SelfServiceRequest ?? new ApiEmployeeportal.SelfServiceRequest()).OriginCase switch
                {
                    ApiEmployeeportal.SelfServiceRequest.OriginOneofCase.Customer => (request.SelfServiceRequest?.Customer?.PortalHomeId ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc0 = _driverappDriverHomeService.SearchDriverHomesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployeeportal.ExportSelfServiceRequestsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployeeportal.ExportSelfServiceRequestsRequest.Types.Filter
            {
                CreatedAfter = (request.SelfServiceRequest?.Sla?.DueAt ?? new Timestamp()),
                Labels = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.SelfServiceRequest?.Assignee?.UserId ?? ""),
            },
        };
        using var stream1 = _employeeportalSelfServiceRequestQueryService.ExportSelfServiceRequests(input1, cancellationToken: token);
        await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportSelfServiceRequests frame {Type}", change1.GetType().Name);
        }
        var input2 = new ApiEmployeeportal.ExportPortalHomesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployeeportal.ExportPortalHomesRequest.Types.Filter
            {
                CreatedAfter = (request.SelfServiceRequest?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (request.SelfServiceRequest?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        using var stream2 = _employeeportalPortalHomeQueryService.ExportPortalHomes(input2, cancellationToken: token);
        await foreach (var change2 in stream2.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportPortalHomes frame {Type}", change2.GetType().Name);
        }
        var response = new ApiEmployeeportal.CreateSelfServiceRequestResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Message = (reply0.Result?.Message ?? ""),
            },
            SelfServiceRequest = new ApiEmployeeportal.SelfServiceRequest
            {
                CustomFields = { (request.SelfServiceRequest?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Priority = (request.SelfServiceRequest?.Priority ?? 0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SelfServiceRequest?.Subject ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "self_service_request",
            "audit", "CreateSelfServiceRequest", (request.SelfServiceRequest?.Subject ?? ""), token);
        return response;
    }
    /// <summary>
    /// Deletes a self service request.
    /// </summary>
    public override async Task<ApiEmployeeportal.DeleteSelfServiceRequestResponse> DeleteSelfServiceRequest(ApiEmployeeportal.DeleteSelfServiceRequestRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.SelfServiceRequestId, "self_service_request_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling DeleteSelfServiceRequest for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "self_service_request", request.SelfServiceRequestId,
            "DeleteSelfServiceRequest", "accepted", token);
        var input0 = new ApiVenues.UpdateVenueRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Venue = new ApiVenues.Venue
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Etag ?? ""),
                },
            },
        };
        var command0 = new DispatchCommand<ApiVenues.UpdateVenueRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _venuesVenueService.UpdateVenueAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var profile1 = new MappingProfile<ApiEmployeeportal.DeleteSelfServiceRequestRequest, ApiBenefits.RetryBenefitPlanWorkflowRequest>(value => new ApiBenefits.RetryBenefitPlanWorkflowRequest
        {
            BenefitPlan = new ApiBenefits.BenefitPlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Venue?.Audit?.CreatedAt ?? new Timestamp()),
                    Etag = (reply0.Venue?.Audit?.Etag ?? ""),
                    UpdatedBy = (reply0.Venue?.Audit?.UpdatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Venue?.Owner?.DisplayName ?? ""),
                },
            },
            Context = (value.Context ?? new ApiCommon.RequestContext()),
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _benefitsBenefitPlanWorkflowService.RetryBenefitPlanWorkflowAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiVenues.SearchVenuesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiVenues.SearchVenuesRequest.Types.Filter
            {
                CreatedAfter = (reply1.BenefitPlan?.Audit?.UpdatedAt ?? new Timestamp()),
                Region = (reply0.Venue ?? new ApiVenues.Venue()).LocatorCase switch
                {
                    ApiVenues.Venue.LocatorOneofCase.Address => (reply0.Venue?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc2 = _venuesVenueService.SearchVenuesAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiEmployeeportal.DeleteSelfServiceRequestResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SelfServiceRequestId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "self_service_request",
            "audit", "DeleteSelfServiceRequest", (request.SelfServiceRequestId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a self service request.
    /// </summary>
    public override async Task<ApiEmployeeportal.GetSelfServiceRequestResponse> GetSelfServiceRequest(ApiEmployeeportal.GetSelfServiceRequestRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.SelfServiceRequestId, "self_service_request_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetSelfServiceRequest for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "self_service_request", request.SelfServiceRequestId,
            "GetSelfServiceRequest", "accepted", token);
        var input0 = new ApiDocuments.ReconcileSignaturesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ReconcileSignaturesRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Signature = new ApiDocuments.Signature
            {
                CorrelationId = (request.SelfServiceRequestId ?? ""),
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
        };
        using var rpc0 = _documentsSignatureSyncService.ReconcileSignaturesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiEmployeeportal.GetSelfServiceRequestResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply0.Outcome?.Status ?? ""),
            },
            SelfServiceRequest = new ApiEmployeeportal.SelfServiceRequest
            {
                SelfServiceRequestId = (request.SelfServiceRequestId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists self service requests a page at a time.
    /// </summary>
    public override async Task<ApiEmployeeportal.ListSelfServiceRequestsResponse> ListSelfServiceRequests(ApiEmployeeportal.ListSelfServiceRequestsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ListSelfServiceRequests for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "self_service_request", "batch",
            "ListSelfServiceRequests", "accepted", token);
        var input0 = new ApiEmployeeportal.RetryAnnouncementWorkflowRequest
        {
            Announcement = new ApiEmployeeportal.Announcement
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
                DueAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployeeportal.RetryAnnouncementWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        var reply0 = await SendListSelfServiceRequests0Async(input0, token);
        var response = new ApiEmployeeportal.ListSelfServiceRequestsResponse
        {
            SelfServiceRequests = { new ApiEmployeeportal.SelfServiceRequest
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.Announcement?.Audit?.Etag ?? ""),
                    UpdatedAt = (reply0.Announcement?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "self_service_request",
            "audit", "ListSelfServiceRequests", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Lists self service requests a page at a time.
    /// </summary>
    public override async Task<ApiEmployeeportal.SearchSelfServiceRequestsResponse> SearchSelfServiceRequests(ApiEmployeeportal.SearchSelfServiceRequestsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SearchSelfServiceRequests for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "self_service_request", "batch",
            "SearchSelfServiceRequests", "accepted", token);
        var input0 = new ApiEmployeeportal.CreatePortalHomeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            PortalHome = new ApiEmployeeportal.PortalHome
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                },
                History = { new ApiEmployeeportal.PortalHomeStatusChange
                {
                    ChangedAt = (request.Context?.Deadline ?? new Timestamp()),
                } },
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _employeeportalPortalHomeService.CreatePortalHomeAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiEmployeeportal.SearchSelfServiceRequestsResponse
        {
            SelfServiceRequests = { new ApiEmployeeportal.SelfServiceRequest
            {
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (reply0.PortalHome?.Requester?.Email ?? ""),
                },
                Subject = (reply0.PortalHome?.Subject ?? ""),
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "self_service_request",
            "audit", "SearchSelfServiceRequests", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Changes a self service request.
    /// </summary>
    public override async Task<ApiEmployeeportal.UpdateSelfServiceRequestResponse> UpdateSelfServiceRequest(ApiEmployeeportal.UpdateSelfServiceRequestRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling UpdateSelfServiceRequest for {Tenant}", requestContext.TenantId);
        if (request.SelfServiceRequest is { } record)
        {
            var snapshot = SelfServiceRequestMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiIdentity.SearchUsersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiIdentity.SearchUsersRequest.Types.Filter
            {
                CreatedAfter = (request.SelfServiceRequest?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (request.SelfServiceRequest?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.SelfServiceRequest ?? new ApiEmployeeportal.SelfServiceRequest()).OriginCase switch
                {
                    ApiEmployeeportal.SelfServiceRequest.OriginOneofCase.Customer => (request.SelfServiceRequest?.Customer?.PortalHomeId ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc0 = _identityUserService.SearchUsersAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployeeportal.ExportSelfServiceRequestsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployeeportal.ExportSelfServiceRequestsRequest.Types.Filter
            {
                CreatedAfter = (request.SelfServiceRequest?.Audit?.CreatedAt ?? new Timestamp()),
                OwnerId = (request.SelfServiceRequest?.Sla?.PolicyId ?? ""),
            },
        };
        using var stream1 = _employeeportalSelfServiceRequestQueryService.ExportSelfServiceRequests(input1, cancellationToken: token);
        await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportSelfServiceRequests frame {Type}", change1.GetType().Name);
        }
        var batch2 = (request.SelfServiceRequest?.History ?? new Google.Protobuf.Collections.RepeatedField<ApiEmployeeportal.SelfServiceRequestStatusChange>()).Select(async item =>
        {
            var input2 = new ApiBenefits.StartBenefitPlanWorkflowRequest
            {
                BenefitPlan = new ApiBenefits.BenefitPlan
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        UpdatedAt = (item.ChangedAt ?? new Timestamp()),
                    },
                },
                Context = (request.Context ?? new ApiCommon.RequestContext()),
            };
            using var rpc2 = _benefitsBenefitPlanWorkflowService.StartBenefitPlanWorkflowAsync(input2, cancellationToken: token);
            var reply2 = await rpc2.ResponseAsync;
            return reply2;
        }).ToArray();
        var replies2 = await Task.WhenAll(batch2);
        var reply2 = replies2.LastOrDefault() ?? new ApiBenefits.StartBenefitPlanWorkflowResponse();
        var input3 = new ApiEmployeeportal.PauseAnnouncementWorkflowRequest
        {
            Announcement = new ApiEmployeeportal.Announcement
            {
                Counterparty = new ApiEmployeeportal.PortalHomeRef
                {
                    DisplayName = (reply2.BenefitPlan?.Owner?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployeeportal.PauseAnnouncementWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.SelfServiceRequest?.Requester?.Email ?? ""),
                },
            },
        };
        using var rpc3 = _employeeportalAnnouncementWorkflowService.PauseAnnouncementWorkflowAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiEmployeeportal.UpdateSelfServiceRequestResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Message = (reply0.Result?.Message ?? ""),
            },
            SelfServiceRequest = new ApiEmployeeportal.SelfServiceRequest
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply3.Announcement?.Audit?.UpdatedAt ?? new Timestamp()),
                    UpdatedBy = (reply3.Announcement?.Audit?.UpdatedBy ?? ""),
                },
                Requester = (request.SelfServiceRequest?.Requester ?? new ApiCommon.ContactInfo()),
                Subject = (request.SelfServiceRequest?.Subject ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SelfServiceRequest?.SelfServiceRequestId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "self_service_request",
            "audit", "UpdateSelfServiceRequest", (request.SelfServiceRequest?.SelfServiceRequestId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams the changes of self service requests as they happen.
    /// </summary>
    public override async Task WatchSelfServiceRequests(ApiEmployeeportal.WatchSelfServiceRequestsRequest request, IServerStreamWriter<ApiEmployeeportal.WatchSelfServiceRequestsResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling WatchSelfServiceRequests for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "self_service_request", "batch",
            "WatchSelfServiceRequests", "accepted", token);
        var input0 = new ApiEmployeeportal.ListSelfServiceRequestSubscriptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployeeportal.ListSelfServiceRequestSubscriptionsRequest.Types.Filter
            {
                Labels = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Region = (request.Filter?.Region ?? ""),
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _employeeportalSelfServiceRequestNotificationService.ListSelfServiceRequestSubscriptionsAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiBenefits.RetryBenefitPlanWorkflowRequest
        {
            BenefitPlan = new ApiBenefits.BenefitPlan
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiBenefits.RetryBenefitPlanWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.Subscriber?.Email ?? ""),
                    Name = (request.Subscriber?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.Subscriber?.Phone ?? ""),
                },
            },
        };
        var reply1 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _benefitsBenefitPlanWorkflowService.RetryBenefitPlanWorkflowAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input2 = new ApiEmployeeportal.ExportPortalHomesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployeeportal.ExportPortalHomesRequest.Types.Filter
            {
                Labels = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Query = (request.Filter?.Query ?? ""),
            },
        };
        using var stream2 = _employeeportalPortalHomeQueryService.ExportPortalHomes(input2, cancellationToken: token);
        await foreach (var change2 in stream2.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportPortalHomes frame {Type}", change2.GetType().Name);
        }
        var input3 = new ApiCorporateevents.ShareVenueReportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.ShareVenueReportRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (reply1.BenefitPlan?.Owner?.Email ?? ""),
                    PreferredChannel = (request.Subscriber?.PreferredChannel ?? ""),
                },
            },
            Reason = (reply1.BenefitPlan ?? new ApiBenefits.BenefitPlan()).ActionCase switch
            {
                ApiBenefits.BenefitPlan.ActionOneofCase.Block => (reply1.BenefitPlan?.Block?.Reason ?? ""),
                _ => ""
            },
        };
        using var rpc3 = _corporateeventsVenueReportingService.ShareVenueReportAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiEmployeeportal.WatchSelfServiceRequestsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply3.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
            SelfServiceRequest = new ApiEmployeeportal.SelfServiceRequest
            {
                Comments = { new ApiEmployeeportal.SelfServiceRequestComment
                {
                    CreatedAt = (reply1.BenefitPlan?.Audit?.CreatedAt ?? new Timestamp()),
                } },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.Subscriber?.Email ?? ""),
                    Phone = (request.Subscriber?.Phone ?? ""),
                },
                Tags = { (reply3.Venue?.Tags ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Filter?.OwnerId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "self_service_request",
            "audit", "WatchSelfServiceRequests", (request.Filter?.OwnerId ?? ""), token);
        await responses.WriteAsync(response, token);
    }
    private async Task<ApiEmployeeportal.RetryAnnouncementWorkflowResponse> SendListSelfServiceRequests0Async(ApiEmployeeportal.RetryAnnouncementWorkflowRequest request, CancellationToken token)
    {
        return await _selfServiceRequestEndpointSendListSelfServiceRequests0Port.SendAsync(request, token);
    }
}
