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
using ApiDriverapp = Acme.Driverapp.V1;
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFanportal = Acme.Fanportal.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiSearch = Acme.Search.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Employeeportal;

/// <summary>
/// Runs the long-lived workflow that takes an announcement from start to finish.
/// </summary>
public sealed class AnnouncementWorkflowEndpoint : ApiEmployeeportal.AnnouncementWorkflowService.AnnouncementWorkflowServiceBase
{
    private readonly IAnnouncementWorkflowEndpointSendListAnnouncementWorkflows0Port _announcementWorkflowEndpointSendListAnnouncementWorkflows0Port;
    private readonly NotificationBus _bus;
    private readonly ApiDocuments.DocumentHistoryService.DocumentHistoryServiceClient _documentsDocumentHistoryService;
    private readonly ApiDriverapp.DriverHomeService.DriverHomeServiceClient _driverappDriverHomeService;
    private readonly ApiEmployeeportal.PortalHomeIntegrationService.PortalHomeIntegrationServiceClient _employeeportalPortalHomeIntegrationService;
    private readonly ApiEmployees.EmploymentContractApprovalService.EmploymentContractApprovalServiceClient _employeesEmploymentContractApprovalService;
    private readonly ApiFanportal.FanSessionService.FanSessionServiceClient _fanportalFanSessionService;
    private readonly ApiIdentity.UserValidationService.UserValidationServiceClient _identityUserValidationService;
    private readonly CallInvoker _invoker;
    private readonly ApiLearning.CourseWorkflowService.CourseWorkflowServiceClient _learningCourseWorkflowService;
    private readonly ApiLicensing.SoftwareLicenseWorkflowService.SoftwareLicenseWorkflowServiceClient _licensingSoftwareLicenseWorkflowService;
    private readonly ILogger<AnnouncementWorkflowEndpoint> _logger;
    private readonly IRecordRepository _repository;
    private readonly ApiScheduling.SwapRequestApprovalService.SwapRequestApprovalServiceClient _schedulingSwapRequestApprovalService;
    private readonly ApiSearch.SearchIndexValidationService.SearchIndexValidationServiceClient _searchSearchIndexValidationService;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTravel.TravelBookingWorkflowService.TravelBookingWorkflowServiceClient _travelTravelBookingWorkflowService;
    private readonly ApiWorkplaces.BuildingApprovalService.BuildingApprovalServiceClient _workplacesBuildingApprovalService;

    public AnnouncementWorkflowEndpoint(
        IAnnouncementWorkflowEndpointSendListAnnouncementWorkflows0Port announcementWorkflowEndpointSendListAnnouncementWorkflows0PortAdapter,
        NotificationBus bus,
        ApiDocuments.DocumentHistoryService.DocumentHistoryServiceClient documentsDocumentHistoryService,
        ApiDriverapp.DriverHomeService.DriverHomeServiceClient driverappDriverHomeService,
        ApiEmployeeportal.PortalHomeIntegrationService.PortalHomeIntegrationServiceClient employeeportalPortalHomeIntegrationService,
        ApiEmployees.EmploymentContractApprovalService.EmploymentContractApprovalServiceClient employeesEmploymentContractApprovalService,
        ApiFanportal.FanSessionService.FanSessionServiceClient fanportalFanSessionService,
        ApiIdentity.UserValidationService.UserValidationServiceClient identityUserValidationService,
        CallInvoker invoker,
        ApiLearning.CourseWorkflowService.CourseWorkflowServiceClient learningCourseWorkflowService,
        ApiLicensing.SoftwareLicenseWorkflowService.SoftwareLicenseWorkflowServiceClient licensingSoftwareLicenseWorkflowService,
        ILogger<AnnouncementWorkflowEndpoint> logger,
        IRecordRepository repository,
        ApiScheduling.SwapRequestApprovalService.SwapRequestApprovalServiceClient schedulingSwapRequestApprovalService,
        ApiSearch.SearchIndexValidationService.SearchIndexValidationServiceClient searchSearchIndexValidationService,
        IOptions<MeshOptions> settings,
        ApiTravel.TravelBookingWorkflowService.TravelBookingWorkflowServiceClient travelTravelBookingWorkflowService,
        ApiWorkplaces.BuildingApprovalService.BuildingApprovalServiceClient workplacesBuildingApprovalService)
    {
        _announcementWorkflowEndpointSendListAnnouncementWorkflows0Port = announcementWorkflowEndpointSendListAnnouncementWorkflows0PortAdapter;
        _bus = bus;
        _documentsDocumentHistoryService = documentsDocumentHistoryService;
        _driverappDriverHomeService = driverappDriverHomeService;
        _employeeportalPortalHomeIntegrationService = employeeportalPortalHomeIntegrationService;
        _employeesEmploymentContractApprovalService = employeesEmploymentContractApprovalService;
        _fanportalFanSessionService = fanportalFanSessionService;
        _identityUserValidationService = identityUserValidationService;
        _invoker = invoker;
        _learningCourseWorkflowService = learningCourseWorkflowService;
        _licensingSoftwareLicenseWorkflowService = licensingSoftwareLicenseWorkflowService;
        _logger = logger;
        _repository = repository;
        _schedulingSwapRequestApprovalService = schedulingSwapRequestApprovalService;
        _searchSearchIndexValidationService = searchSearchIndexValidationService;
        _settings = settings;
        _travelTravelBookingWorkflowService = travelTravelBookingWorkflowService;
        _workplacesBuildingApprovalService = workplacesBuildingApprovalService;
    }
    /// <summary>
    /// Advances an announcement.
    /// </summary>
    public override async Task<ApiEmployeeportal.AdvanceAnnouncementWorkflowResponse> AdvanceAnnouncementWorkflow(ApiEmployeeportal.AdvanceAnnouncementWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling AdvanceAnnouncementWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Announcement is { } record)
        {
            var snapshot = AnnouncementMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiEmployeeportal.AcknowledgePortalHomeEventResponse();
        foreach (var item in (request.Announcement?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiEmployeeportal.AnnouncementLine>()))
        {
            var input0 = new ApiEmployeeportal.AcknowledgePortalHomeEventRequest
            {
                Annotations = { (item.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiEmployeeportal.AcknowledgePortalHomeEventRequest.Types.Details
                {
                    Envelope = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                    MessageId = (item.LineId ?? ""),
                },
                PortalHomeId = (request.Announcement?.Counterparty?.PortalHomeId ?? ""),
                Reason = (request.Reason ?? ""),
            };
            using var rpc0 = _employeeportalPortalHomeIntegrationService.AcknowledgePortalHomeEventAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var input1 = new ApiSearch.LintSearchIndexesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SearchIndex = new ApiSearch.SearchIndex
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Announcement?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
        };
        var reply1 = await Workflow.SendAsync(input1, token, _searchSearchIndexValidationService.LintSearchIndexesAsync);
        var input2 = new ApiEmployeeportal.ReplayPortalHomeEventsRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployeeportal.ReplayPortalHomeEventsRequest.Types.Details
            {
                Envelope = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                MessageId = (reply1.Result?.Message ?? ""),
            },
            PortalHomeId = (reply0.PortalHome?.PortalHomeId ?? ""),
            Reason = (request.Reason ?? ""),
        };
        using var rpc2 = _employeeportalPortalHomeIntegrationService.ReplayPortalHomeEventsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiIdentity.PreviewUserRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiIdentity.PreviewUserRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Details?.Notify?.PreferredChannel ?? ""),
                },
            },
            User = new ApiIdentity.User
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.PortalHome?.Audit?.Etag ?? ""),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (reply0.PortalHome?.Requester?.Name ?? new ApiCommon.PersonName()),
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.PortalHome ?? new ApiEmployeeportal.PortalHome()).OriginCase switch
                    {
                        ApiEmployeeportal.PortalHome.OriginOneofCase.Customer => (reply0.PortalHome?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc3 = _identityUserValidationService.PreviewUserAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiEmployeeportal.AdvanceAnnouncementWorkflowResponse
        {
            Announcement = new ApiEmployeeportal.Announcement
            {
                AnnouncementId = (request.AnnouncementId ?? ""),
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply3.User?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                IssuedAt = (request.Announcement?.IssuedAt ?? new Timestamp()),
            },
            Outcome = new ApiEmployeeportal.AdvanceAnnouncementWorkflowResponse.Types.Outcome
            {
                Message = (reply2.Outcome?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
                Status = (reply3.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.Step ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "announcement",
            "audit", "AdvanceAnnouncementWorkflow", (request.Details?.Step ?? ""), token);
        return response;
    }
    /// <summary>
    /// Cancels an announcement.
    /// </summary>
    public override async Task<ApiEmployeeportal.CancelAnnouncementWorkflowResponse> CancelAnnouncementWorkflow(ApiEmployeeportal.CancelAnnouncementWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CancelAnnouncementWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Announcement is { } record)
        {
            var snapshot = AnnouncementMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiFanportal.WatchFanSessionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Subscriber = new ApiCommon.ContactInfo
            {
                Phone = (request.Details?.Notify?.Phone ?? ""),
                PreferredChannel = (request.Details?.Notify?.PreferredChannel ?? ""),
            },
        };
        using var stream0 = _fanportalFanSessionService.WatchFanSessions(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received WatchFanSessions frame {Type}", change0.GetType().Name);
        }
        var input1 = new ApiLicensing.PauseSoftwareLicenseWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.PauseSoftwareLicenseWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Details?.Notify?.PreferredChannel ?? ""),
                },
            },
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Holder = new ApiLicensing.LicenseAssignmentRef
                {
                    DisplayName = (request.Announcement?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        var command1 = new DispatchCommand<ApiLicensing.PauseSoftwareLicenseWorkflowRequest>(input1,
            "primary", requestContext.TenantId);
        var routed1 = command1 with { Region = _settings.Value.Region };
        using var rpc1 = _licensingSoftwareLicenseWorkflowService.PauseSoftwareLicenseWorkflowAsync(routed1.Payload, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiLicensing.PauseSoftwareLicenseWorkflowRequest>("acme.employeeportal.v1.AnnouncementWorkflowService/CancelAnnouncementWorkflow/1", routed1.Payload.Clone()), token);
        var input2 = new ApiScheduling.ApproveSwapRequestRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SwapRequest = new ApiScheduling.SwapRequest
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                },
            },
        };
        using var rpc2 = _schedulingSwapRequestApprovalService.ApproveSwapRequestAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        var input3 = new ApiEmployeeportal.DisconnectPortalHomeSourceRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployeeportal.DisconnectPortalHomeSourceRequest.Types.Details
            {
                Envelope = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
            PortalHomeId = (request.Announcement?.Counterparty?.PortalHomeId ?? ""),
            Reason = (request.Reason ?? ""),
        };
        using var rpc3 = _employeeportalPortalHomeIntegrationService.DisconnectPortalHomeSourceAsync(input3, cancellationToken: token);
        var pending3 = rpc3.ResponseAsync;
        await Task.WhenAll(pending2, pending3);
        var reply2 = await pending2;
        var reply3 = await pending3;
        var response = new ApiEmployeeportal.CancelAnnouncementWorkflowResponse
        {
            Announcement = new ApiEmployeeportal.Announcement
            {
                AnnouncementId = (request.AnnouncementId ?? ""),
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply2.SwapRequest?.Audit?.UpdatedAt ?? new Timestamp()),
                    UpdatedBy = (reply1.SoftwareLicense?.Audit?.UpdatedBy ?? ""),
                },
                Status = (request.Announcement?.Status ?? (ApiEmployeeportal.AnnouncementStatus)0),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply3.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.Step ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "announcement",
            "audit", "CancelAnnouncementWorkflow", (request.Details?.Step ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an announcement.
    /// </summary>
    public override async Task<ApiEmployeeportal.InspectAnnouncementWorkflowResponse> InspectAnnouncementWorkflow(ApiEmployeeportal.InspectAnnouncementWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.AnnouncementId, "announcement_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling InspectAnnouncementWorkflow for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "announcement", request.AnnouncementId,
            "InspectAnnouncementWorkflow", "accepted", token);
        var input0 = new ApiEmployeeportal.TestPortalHomeConnectionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployeeportal.TestPortalHomeConnectionRequest.Types.Details
            {
                Envelope = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                MessageId = (request.AnnouncementId ?? ""),
                ReceivedAt = (request.Context?.Deadline ?? new Timestamp()),
            },
        };
        using var rpc0 = _employeeportalPortalHomeIntegrationService.TestPortalHomeConnectionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiEmployeeportal.InspectAnnouncementWorkflowResponse
        {
            Announcement = new ApiEmployeeportal.Announcement
            {
                AnnouncementId = (request.AnnouncementId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        return response;
    }
    /// <summary>
    /// Lists announcements a page at a time.
    /// </summary>
    public override async Task<ApiEmployeeportal.ListAnnouncementWorkflowsResponse> ListAnnouncementWorkflows(ApiEmployeeportal.ListAnnouncementWorkflowsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ListAnnouncementWorkflows for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "announcement", "batch",
            "ListAnnouncementWorkflows", "accepted", token);
        var input0 = new ApiSearch.NormalizeSearchIndexRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SearchIndex = new ApiSearch.SearchIndex
            {
                Address = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        var reply0 = await SendListAnnouncementWorkflows0Async(input0, token);
        var input1 = new ApiEmployeeportal.ListPortalHomeSourcesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployeeportal.ListPortalHomeSourcesRequest.Types.Filter
            {
                OwnerId = (request.Filter?.OwnerId ?? ""),
                Region = (request.Filter?.Region ?? ""),
            },
            Page = new ApiCommon.PageRequest
            {
                PageSize = (request.Page?.PageSize ?? 0),
                PageToken = (request.Page?.PageToken ?? ""),
            },
        };
        var labels1 = input1.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input1.Context.Baggage.Clear();
        input1.Context.Baggage.Add(labels1);
        using var rpc1 = _employeeportalPortalHomeIntegrationService.ListPortalHomeSourcesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiEmployeeportal.ListAnnouncementWorkflowsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.OrderBy ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "announcement",
            "audit", "ListAnnouncementWorkflows", (request.OrderBy ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pauses an announcement.
    /// </summary>
    public override async Task<ApiEmployeeportal.PauseAnnouncementWorkflowResponse> PauseAnnouncementWorkflow(ApiEmployeeportal.PauseAnnouncementWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling PauseAnnouncementWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Announcement is { } record)
        {
            var snapshot = AnnouncementMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.SubmitEmploymentContractRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Bank = new ApiEmployees.EmploymentContractBankSettlement
                {
                    Bic = (request.Announcement ?? new ApiEmployeeportal.Announcement()).PaymentCase switch
                    {
                        ApiEmployeeportal.Announcement.SettlementOneofCase.Bank => (request.Announcement?.Bank?.Bic ?? ""),
                        _ => ""
                    },
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _employeesEmploymentContractApprovalService.SubmitEmploymentContractAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployeeportal.DisconnectPortalHomeSourceRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployeeportal.DisconnectPortalHomeSourceRequest.Types.Details
            {
                Envelope = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
            Reason = (request.Reason ?? ""),
        };
        using var rpc1 = _employeeportalPortalHomeIntegrationService.DisconnectPortalHomeSourceAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiEmployeeportal.PauseAnnouncementWorkflowResponse
        {
            Announcement = new ApiEmployeeportal.Announcement
            {
                AnnouncementId = (request.AnnouncementId ?? ""),
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply1.PortalHome?.Audit?.CreatedBy ?? ""),
                },
                DueAt = (request.Announcement?.DueAt ?? new Timestamp()),
                Notes = (reply0.EmploymentContract?.Notes ?? ""),
            },
            Outcome = new ApiEmployeeportal.PauseAnnouncementWorkflowResponse.Types.Outcome
            {
                EffectiveAt = (reply1.Outcome?.EffectiveAt ?? new Timestamp()),
                ReferenceId = (reply0.Outcome?.ReferenceId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.Step ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "announcement",
            "audit", "PauseAnnouncementWorkflow", (request.Details?.Step ?? ""), token);
        return response;
    }
    /// <summary>
    /// Retrys an announcement.
    /// </summary>
    public override async Task<ApiEmployeeportal.RetryAnnouncementWorkflowResponse> RetryAnnouncementWorkflow(ApiEmployeeportal.RetryAnnouncementWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RetryAnnouncementWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Announcement is { } record)
        {
            var snapshot = AnnouncementMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployeeportal.ReplayPortalHomeEventsRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployeeportal.ReplayPortalHomeEventsRequest.Types.Details
            {
                Envelope = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                ReceivedAt = (request.Announcement?.Audit?.CreatedAt ?? new Timestamp()),
            },
            PortalHomeId = (request.Announcement?.Counterparty?.PortalHomeId ?? ""),
            Reason = (request.Reason ?? ""),
        };
        using var rpc0 = _employeeportalPortalHomeIntegrationService.ReplayPortalHomeEventsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var batch1 = (request.Announcement?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiEmployeeportal.AnnouncementLine>()).Select(async item =>
        {
            var input1 = new ApiLearning.ResumeCourseWorkflowRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Course = new ApiLearning.Course
                {
                    Components = { { "primary", new ApiLearning.CourseComponent
                    {
                        Description = (item.Description ?? ""),
                        Quantity = item.Quantity,
                    } } },
                    Owner = new ApiIdentity.UserRef
                    {
                        DisplayName = (reply0.PortalHome?.Assignee?.DisplayName ?? ""),
                    },
                },
            };
            using var rpc1 = _learningCourseWorkflowService.ResumeCourseWorkflowAsync(input1, cancellationToken: token);
            var reply1 = await rpc1.ResponseAsync;
            return reply1;
        }).ToArray();
        var replies1 = await Task.WhenAll(batch1);
        var reply1 = replies1.LastOrDefault() ?? new ApiLearning.ResumeCourseWorkflowResponse();
        var input2 = new ApiEmployeeportal.ConnectPortalHomeSourceRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployeeportal.ConnectPortalHomeSourceRequest.Types.Details
            {
                Envelope = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                MessageId = (reply0.Result?.Message ?? ""),
                ReceivedAt = (request.Announcement?.Audit?.UpdatedAt ?? new Timestamp()),
            },
            PortalHomeId = (request.Announcement?.Counterparty?.PortalHomeId ?? ""),
        };
        using var rpc2 = _employeeportalPortalHomeIntegrationService.ConnectPortalHomeSourceAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiWorkplaces.ListPendingBuildingsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiWorkplaces.ListPendingBuildingsRequest.Types.Filter
            {
                CreatedAfter = (reply0.PortalHome?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (reply0.PortalHome?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply0.PortalHome?.Sla?.PolicyId ?? ""),
                Region = (reply1.Course ?? new ApiLearning.Course()).LocatorCase switch
                {
                    ApiLearning.Course.LocatorOneofCase.Address => (reply1.Course?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc3 = _workplacesBuildingApprovalService.ListPendingBuildingsAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiEmployeeportal.RetryAnnouncementWorkflowResponse
        {
            Announcement = new ApiEmployeeportal.Announcement
            {
                AnnouncementId = (request.AnnouncementId ?? ""),
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.PortalHome?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Counterparty = new ApiEmployeeportal.PortalHomeRef
                {
                    DisplayName = (reply2.PortalHome?.Assignee?.DisplayName ?? ""),
                },
                DueAt = (request.Announcement?.DueAt ?? new Timestamp()),
            },
            Outcome = new ApiEmployeeportal.RetryAnnouncementWorkflowResponse.Types.Outcome
            {
                NextSteps = { (reply0.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply3.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.Step ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "announcement",
            "audit", "RetryAnnouncementWorkflow", (request.Details?.Step ?? ""), token);
        return response;
    }
    /// <summary>
    /// Signals an announcement.
    /// </summary>
    public override async Task<ApiEmployeeportal.SignalAnnouncementWorkflowResponse> SignalAnnouncementWorkflow(ApiEmployeeportal.SignalAnnouncementWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SignalAnnouncementWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Announcement is { } record)
        {
            var snapshot = AnnouncementMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiWorkplaces.DelegateBuildingApprovalRequest
        {
            Building = new ApiWorkplaces.Building
            {
                Children = { new ApiWorkplaces.Building
                {
                    Children = { new ApiWorkplaces.Building
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            CreatedBy = (request.Announcement?.Audit?.CreatedBy ?? ""),
                        },
                        Location = new ApiCommon.PostalAddress
                        {
                            PostalCode = (request.Announcement?.BillingAddress?.PostalCode ?? ""),
                            Region = (request.Announcement?.BillingAddress?.Region ?? ""),
                        },
                        Manager = new ApiIdentity.UserRef
                        {
                            Email = (request.Details?.Notify?.Email ?? ""),
                        },
                    } },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply0 = await Workflow.SendAsync(input0, token, _workplacesBuildingApprovalService.DelegateBuildingApprovalAsync);
        var input1 = new ApiEmployeeportal.ConnectPortalHomeSourceRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployeeportal.ConnectPortalHomeSourceRequest.Types.Details
            {
                Envelope = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
            PortalHomeId = (request.Announcement?.Counterparty?.PortalHomeId ?? ""),
            Reason = (request.Reason ?? ""),
        };
        using var rpc1 = _employeeportalPortalHomeIntegrationService.ConnectPortalHomeSourceAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
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
                    Etag = (reply1.PortalHome?.Audit?.Etag ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (request.Announcement?.BillingAddress?.Recipient ?? ""),
                },
                Card = new ApiDocuments.DocumentCardSettlement
                {
                    Network = (request.Announcement ?? new ApiEmployeeportal.Announcement()).PaymentCase switch
                    {
                        ApiEmployeeportal.Announcement.SettlementOneofCase.Card => (request.Announcement?.Card?.Network ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc2 = _documentsDocumentHistoryService.RevertDocumentAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiDriverapp.SearchDriverHomesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDriverapp.SearchDriverHomesRequest.Types.Filter
            {
                CreatedAfter = (reply2.Document?.Audit?.UpdatedAt ?? new Timestamp()),
                Region = (reply2.Document?.BillingAddress?.Region ?? ""),
            },
        };
        var descriptor3 = new Method<ApiDriverapp.SearchDriverHomesRequest, ApiDriverapp.SearchDriverHomesResponse>(
            MethodType.Unary, "acme.driverapp.v1" + ".DriverHomeService", "SearchDriverHomes",
            Wire.Marshaller<ApiDriverapp.SearchDriverHomesRequest>(), Wire.Marshaller<ApiDriverapp.SearchDriverHomesResponse>());
        using var rpc3 = _invoker.AsyncUnaryCall(descriptor3, null,
            new CallOptions(cancellationToken: token), input3);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiEmployeeportal.SignalAnnouncementWorkflowResponse
        {
            Announcement = new ApiEmployeeportal.Announcement
            {
                AnnouncementId = (request.AnnouncementId ?? ""),
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (reply2.Document?.BillingAddress?.Region ?? ""),
                },
                Counterparty = new ApiEmployeeportal.PortalHomeRef
                {
                    DisplayName = (reply1.PortalHome?.Customer?.DisplayName ?? ""),
                },
                IssuedAt = (request.Announcement?.IssuedAt ?? new Timestamp()),
            },
            Outcome = new ApiEmployeeportal.SignalAnnouncementWorkflowResponse.Types.Outcome
            {
                ReferenceId = (reply0.Outcome?.ReferenceId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Message = (reply3.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.Step ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "announcement",
            "audit", "SignalAnnouncementWorkflow", (request.Details?.Step ?? ""), token);
        return response;
    }
    /// <summary>
    /// Starts an announcement.
    /// </summary>
    public override async Task<ApiEmployeeportal.StartAnnouncementWorkflowResponse> StartAnnouncementWorkflow(ApiEmployeeportal.StartAnnouncementWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling StartAnnouncementWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Announcement is { } record)
        {
            var snapshot = AnnouncementMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployeeportal.ListPortalHomeSourcesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployeeportal.ListPortalHomeSourcesRequest.Types.Filter
            {
                CreatedAfter = (request.Announcement?.IssuedAt ?? new Timestamp()),
                Labels = { (request.Details?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Announcement?.AnnouncementId ?? ""),
                Region = (request.Announcement?.BillingAddress?.Region ?? ""),
            },
        };
        var command0 = new DispatchCommand<ApiEmployeeportal.ListPortalHomeSourcesRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _employeeportalPortalHomeIntegrationService.ListPortalHomeSourcesAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var profile1 = new MappingProfile<ApiEmployeeportal.StartAnnouncementWorkflowRequest, ApiTravel.SignalTravelBookingWorkflowRequest>(value => new ApiTravel.SignalTravelBookingWorkflowRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            TravelBooking = new ApiTravel.TravelBooking
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (value.Details?.Notify?.Email ?? ""),
                },
            },
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _travelTravelBookingWorkflowService.SignalTravelBookingWorkflowAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiEmployeeportal.ReplayPortalHomeEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployeeportal.ReplayPortalHomeEventsRequest.Types.Details
            {
                Envelope = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
            PortalHomeId = (request.Announcement?.Counterparty?.PortalHomeId ?? ""),
        };
        using var rpc2 = _employeeportalPortalHomeIntegrationService.ReplayPortalHomeEventsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiEmployeeportal.StartAnnouncementWorkflowResponse
        {
            Announcement = new ApiEmployeeportal.Announcement
            {
                AnnouncementId = (request.AnnouncementId ?? ""),
                Number = (reply2.PortalHome?.Customer?.Number ?? ""),
                Total = (request.Announcement?.Total ?? new ApiCommon.Money()),
            },
            Outcome = new ApiEmployeeportal.StartAnnouncementWorkflowResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply1.Outcome?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "announcement",
            "audit", "StartAnnouncementWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiSearch.NormalizeSearchIndexResponse> SendListAnnouncementWorkflows0Async(ApiSearch.NormalizeSearchIndexRequest request, CancellationToken token)
    {
        return await _announcementWorkflowEndpointSendListAnnouncementWorkflows0Port.SendAsync(request, token);
    }
}
