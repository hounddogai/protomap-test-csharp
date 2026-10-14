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
using ApiEmployees = Acme.Employees.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

/// <summary>
/// Runs the long-lived workflow that takes a room booking from start to finish.
/// </summary>
public sealed class RoomBookingWorkflowEndpoint : ApiWorkplaces.RoomBookingWorkflowService.RoomBookingWorkflowServiceBase
{
    private readonly ApiDocuments.DocumentHistoryService.DocumentHistoryServiceClient _documentsDocumentHistoryService;
    private readonly ApiEmployees.EmployeeService.EmployeeServiceClient _employeesEmployeeService;
    private readonly ApiFeatureflags.FeatureFlagQueryService.FeatureFlagQueryServiceClient _featureflagsFeatureFlagQueryService;
    private readonly ApiHelpdesk.SlaPolicyWorkflowService.SlaPolicyWorkflowServiceClient _helpdeskSlaPolicyWorkflowService;
    private readonly ApiIdentity.UserValidationService.UserValidationServiceClient _identityUserValidationService;
    private readonly ILogger<RoomBookingWorkflowEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiEmployees.ListEmployeesRequest, CallOptions, AsyncUnaryCall<ApiEmployees.ListEmployeesResponse>> _sendResumeRoomBookingWorkflow0;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTravel.ItineraryReportingService.ItineraryReportingServiceClient _travelItineraryReportingService;
    private readonly ApiTravel.TravelPolicyReportingService.TravelPolicyReportingServiceClient _travelTravelPolicyReportingService;
    private readonly ApiWorkplaces.BuildingApprovalService.BuildingApprovalServiceClient _workplacesBuildingApprovalService;
    private readonly ApiWorkplaces.BuildingWorkflowService.BuildingWorkflowServiceClient _workplacesBuildingWorkflowService;
    private readonly ApiWorkplaces.RoomBookingApprovalService.RoomBookingApprovalServiceClient _workplacesRoomBookingApprovalService;

    public RoomBookingWorkflowEndpoint(
        ApiDocuments.DocumentHistoryService.DocumentHistoryServiceClient documentsDocumentHistoryService,
        ApiEmployees.EmployeeService.EmployeeServiceClient employeesEmployeeService,
        ApiFeatureflags.FeatureFlagQueryService.FeatureFlagQueryServiceClient featureflagsFeatureFlagQueryService,
        ApiHelpdesk.SlaPolicyWorkflowService.SlaPolicyWorkflowServiceClient helpdeskSlaPolicyWorkflowService,
        ApiIdentity.UserValidationService.UserValidationServiceClient identityUserValidationService,
        ILogger<RoomBookingWorkflowEndpoint> logger,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiTravel.ItineraryReportingService.ItineraryReportingServiceClient travelItineraryReportingService,
        ApiTravel.TravelPolicyReportingService.TravelPolicyReportingServiceClient travelTravelPolicyReportingService,
        ApiWorkplaces.BuildingApprovalService.BuildingApprovalServiceClient workplacesBuildingApprovalService,
        ApiWorkplaces.BuildingWorkflowService.BuildingWorkflowServiceClient workplacesBuildingWorkflowService,
        ApiWorkplaces.RoomBookingApprovalService.RoomBookingApprovalServiceClient workplacesRoomBookingApprovalService)
    {
        _documentsDocumentHistoryService = documentsDocumentHistoryService;
        _employeesEmployeeService = employeesEmployeeService;
        _featureflagsFeatureFlagQueryService = featureflagsFeatureFlagQueryService;
        _helpdeskSlaPolicyWorkflowService = helpdeskSlaPolicyWorkflowService;
        _identityUserValidationService = identityUserValidationService;
        _logger = logger;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _settings = settings;
        _travelItineraryReportingService = travelItineraryReportingService;
        _travelTravelPolicyReportingService = travelTravelPolicyReportingService;
        _workplacesBuildingApprovalService = workplacesBuildingApprovalService;
        _workplacesBuildingWorkflowService = workplacesBuildingWorkflowService;
        _workplacesRoomBookingApprovalService = workplacesRoomBookingApprovalService;
        _sendResumeRoomBookingWorkflow0 = _employeesEmployeeService.ListEmployeesAsync;
    }
    /// <summary>
    /// Advances a room booking.
    /// </summary>
    public override async Task<ApiWorkplaces.AdvanceRoomBookingWorkflowResponse> AdvanceRoomBookingWorkflow(ApiWorkplaces.AdvanceRoomBookingWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling AdvanceRoomBookingWorkflow for {Tenant}", requestContext.TenantId);
        if (request.RoomBooking is { } record)
        {
            var snapshot = RoomBookingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.GenerateTravelPolicyReportRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Reason ?? ""),
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _travelTravelPolicyReportingService.GenerateTravelPolicyReportAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiWorkplaces.AdvanceRoomBookingWorkflowResponse
        {
            Outcome = new ApiWorkplaces.AdvanceRoomBookingWorkflowResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                RoomBookingId = (request.RoomBookingId ?? ""),
                Status = (request.RoomBooking?.Status ?? (ApiWorkplaces.RoomBookingStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.RoomBooking?.Title ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "room_booking",
            "audit", "AdvanceRoomBookingWorkflow", (request.RoomBooking?.Title ?? ""), token);
        return response;
    }
    /// <summary>
    /// Cancels a room booking.
    /// </summary>
    public override async Task<ApiWorkplaces.CancelRoomBookingWorkflowResponse> CancelRoomBookingWorkflow(ApiWorkplaces.CancelRoomBookingWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CancelRoomBookingWorkflow for {Tenant}", requestContext.TenantId);
        if (request.RoomBooking is { } record)
        {
            var snapshot = RoomBookingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.ScheduleTravelPolicyReportRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.ScheduleTravelPolicyReportRequest.Types.Details
            {
                Period = (request.RoomBooking?.Window ?? new ApiCommon.TimeWindow()),
            },
            Reason = (request.Reason ?? ""),
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiTravel.ScheduleTravelPolicyReportRequest>();
        using var rpc0 = _travelTravelPolicyReportingService.ScheduleTravelPolicyReportAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiWorkplaces.ApproveBuildingRequest
        {
            Building = new ApiWorkplaces.Building
            {
                Children = { new ApiWorkplaces.Building
                {
                    Children = { new ApiWorkplaces.Building
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            CreatedAt = (request.RoomBooking?.Audit?.CreatedAt ?? new Timestamp()),
                            CreatedBy = (request.RoomBooking?.Audit?.CreatedBy ?? ""),
                        },
                        Location = new ApiCommon.PostalAddress
                        {
                            Recipient = (request.RoomBooking?.Location?.Recipient ?? ""),
                        },
                    } },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var json1 = JsonFormatter.Default.Format(input1);
        input1 = JsonParser.Default.Parse<ApiWorkplaces.ApproveBuildingRequest>(json1);
        using var rpc1 = _workplacesBuildingApprovalService.ApproveBuildingAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiWorkplaces.CancelRoomBookingWorkflowResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                Location = new ApiCommon.PostalAddress
                {
                    Lines = { (reply1.Building?.Location?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
                RoomBookingId = (request.RoomBookingId ?? ""),
                Status = (request.RoomBooking?.Status ?? (ApiWorkplaces.RoomBookingStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.RoomBooking?.Title ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "room_booking",
            "audit", "CancelRoomBookingWorkflow", (request.RoomBooking?.Title ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a room booking.
    /// </summary>
    public override async Task<ApiWorkplaces.InspectRoomBookingWorkflowResponse> InspectRoomBookingWorkflow(ApiWorkplaces.InspectRoomBookingWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.RoomBookingId, "room_booking_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling InspectRoomBookingWorkflow for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "room_booking", request.RoomBookingId,
            "InspectRoomBookingWorkflow", "accepted", token);
        var input0 = new ApiDocuments.ListDocumentVersionsRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Filter = new ApiDocuments.ListDocumentVersionsRequest.Types.Filter
        {
            CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
            Labels = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            OwnerId = (request.RoomBookingId ?? ""),
        };
        using var rpc0 = _documentsDocumentHistoryService.ListDocumentVersionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiIdentity.ExplainUserDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiIdentity.ExplainUserDecisionRequest.Types.Details
            {
                Locale = (request.Context?.Locale ?? ""),
            },
            User = new ApiIdentity.User
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _identityUserValidationService.ExplainUserDecisionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiWorkplaces.InspectRoomBookingWorkflowResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                RoomBookingId = (request.RoomBookingId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists room bookings a page at a time.
    /// </summary>
    public override async Task<ApiWorkplaces.ListRoomBookingWorkflowsResponse> ListRoomBookingWorkflows(ApiWorkplaces.ListRoomBookingWorkflowsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ListRoomBookingWorkflows for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "room_booking", "batch",
            "ListRoomBookingWorkflows", "accepted", token);
        var input0 = new ApiEmployees.BatchGetEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _employeesEmployeeService.BatchGetEmployeesAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiWorkplaces.ListRoomBookingWorkflowsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "room_booking",
            "audit", "ListRoomBookingWorkflows", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pauses a room booking.
    /// </summary>
    public override async Task<ApiWorkplaces.PauseRoomBookingWorkflowResponse> PauseRoomBookingWorkflow(ApiWorkplaces.PauseRoomBookingWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling PauseRoomBookingWorkflow for {Tenant}", requestContext.TenantId);
        if (request.RoomBooking is { } record)
        {
            var snapshot = RoomBookingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiWorkplaces.SignalBuildingWorkflowRequest
        {
            Building = new ApiWorkplaces.Building
            {
                Children = { new ApiWorkplaces.Building
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        UpdatedAt = (request.RoomBooking?.Audit?.UpdatedAt ?? new Timestamp()),
                    },
                    Children = { new ApiWorkplaces.Building
                    {
                        Location = new ApiCommon.PostalAddress
                        {
                            Locality = (request.RoomBooking?.Location?.Locality ?? ""),
                            PostalCode = (request.RoomBooking?.Location?.PostalCode ?? ""),
                        },
                        Manager = new ApiIdentity.UserRef
                        {
                            UserId = (request.RoomBooking?.Organizer?.UserId ?? ""),
                        },
                    } },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _workplacesBuildingWorkflowService.SignalBuildingWorkflowAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiFeatureflags.BrowseFeatureFlagsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiFeatureflags.BrowseFeatureFlagsRequest.Types.Filter
            {
                CreatedAfter = (reply0.Building?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply0.Building?.Manager?.UserId ?? ""),
                Region = (reply0.Building?.Location?.Region ?? ""),
            },
        };
        var reply1 = await Workflow.SendAsync(input1, token, _featureflagsFeatureFlagQueryService.BrowseFeatureFlagsAsync);
        var response = new ApiWorkplaces.PauseRoomBookingWorkflowResponse
        {
            Outcome = new ApiWorkplaces.PauseRoomBookingWorkflowResponse.Types.Outcome
            {
                Status = (reply0.Outcome?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                Location = new ApiCommon.PostalAddress
                {
                    Region = (reply0.Building?.Location?.Region ?? ""),
                },
                Metadata = { (request.RoomBooking?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                RoomBookingId = (request.RoomBookingId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.RoomBooking?.Title ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "room_booking",
            "audit", "PauseRoomBookingWorkflow", (request.RoomBooking?.Title ?? ""), token);
        return response;
    }
    /// <summary>
    /// Resumes a room booking.
    /// </summary>
    public override async Task<ApiWorkplaces.ResumeRoomBookingWorkflowResponse> ResumeRoomBookingWorkflow(ApiWorkplaces.ResumeRoomBookingWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ResumeRoomBookingWorkflow for {Tenant}", requestContext.TenantId);
        if (request.RoomBooking is { } record)
        {
            var snapshot = RoomBookingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.ListEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.ListEmployeesRequest.Types.Filter
            {
                CreatedAfter = (request.RoomBooking?.Audit?.UpdatedAt ?? new Timestamp()),
                OwnerId = (request.RoomBooking?.Organizer?.UserId ?? ""),
                Region = (request.RoomBooking?.Location?.Region ?? ""),
            },
        };
        using var rpc0 = _sendResumeRoomBookingWorkflow0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiFeatureflags.SummarizeFeatureFlagsRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiFeatureflags.SummarizeFeatureFlagsRequest.Types.Details
            {
                Filters = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Period = (request.RoomBooking?.Window ?? new ApiCommon.TimeWindow()),
            },
            Reason = (request.Reason ?? ""),
        };
        var named1 = _registry.Get<ApiFeatureflags.FeatureFlagQueryService.FeatureFlagQueryServiceClient>("acme.featureflags.v1.FeatureFlagQueryService");
        using var rpc1 = named1.SummarizeFeatureFlagsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var batch2 = (request.RoomBooking?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiWorkplaces.RoomBookingParticipant>()).Select(async item =>
        {
            var input2 = new ApiWorkplaces.ApproveRoomBookingRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                RoomBooking = new ApiWorkplaces.RoomBooking
                {
                    Participants = { new ApiWorkplaces.RoomBookingParticipant
                    {
                        Contact = new ApiCommon.ContactInfo
                        {
                            Email = (item.Contact?.Email ?? ""),
                            Phone = (item.Contact?.Phone ?? ""),
                            PreferredChannel = (item.Contact?.PreferredChannel ?? ""),
                        },
                    } },
                },
            };
            using var rpc2 = _workplacesRoomBookingApprovalService.ApproveRoomBookingAsync(input2, cancellationToken: token);
            var reply2 = await rpc2.ResponseAsync;
            return reply2;
        }).ToArray();
        var replies2 = await Task.WhenAll(batch2);
        var reply2 = replies2.LastOrDefault() ?? new ApiWorkplaces.ApproveRoomBookingResponse();
        var response = new ApiWorkplaces.ResumeRoomBookingWorkflowResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Message = (reply0.Result?.Message ?? ""),
            },
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                Organizer = (request.RoomBooking?.Organizer ?? new ApiIdentity.UserRef()),
                RoomBookingId = (request.RoomBookingId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.RoomBooking?.Title ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "room_booking",
            "audit", "ResumeRoomBookingWorkflow", (request.RoomBooking?.Title ?? ""), token);
        return response;
    }
    /// <summary>
    /// Signals a room booking.
    /// </summary>
    public override async Task<ApiWorkplaces.SignalRoomBookingWorkflowResponse> SignalRoomBookingWorkflow(ApiWorkplaces.SignalRoomBookingWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SignalRoomBookingWorkflow for {Tenant}", requestContext.TenantId);
        if (request.RoomBooking is { } record)
        {
            var snapshot = RoomBookingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.ListSlaPolicyWorkflowsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiHelpdesk.ListSlaPolicyWorkflowsRequest.Types.Filter
            {
                CreatedAfter = (request.RoomBooking?.Audit?.UpdatedAt ?? new Timestamp()),
                Region = (request.RoomBooking?.Location?.Region ?? ""),
            },
        };
        var command0 = new DispatchCommand<ApiHelpdesk.ListSlaPolicyWorkflowsRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _helpdeskSlaPolicyWorkflowService.ListSlaPolicyWorkflowsAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var profile1 = new MappingProfile<ApiWorkplaces.SignalRoomBookingWorkflowRequest, ApiTravel.ListItineraryReportsRequest>(value => new ApiTravel.ListItineraryReportsRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiTravel.ListItineraryReportsRequest.Types.Filter
            {
                CreatedAfter = (value.RoomBooking?.Audit?.CreatedAt ?? new Timestamp()),
                Region = (value.RoomBooking?.Location?.Region ?? ""),
            },
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _travelItineraryReportingService.ListItineraryReportsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiWorkplaces.SignalRoomBookingWorkflowResponse
        {
            Outcome = new ApiWorkplaces.SignalRoomBookingWorkflowResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
            },
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                RoomBookingId = (request.RoomBookingId ?? ""),
                Window = (request.RoomBooking?.Window ?? new ApiCommon.TimeWindow()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.RoomBooking?.Title ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "room_booking",
            "audit", "SignalRoomBookingWorkflow", (request.RoomBooking?.Title ?? ""), token);
        return response;
    }
    /// <summary>
    /// Starts a room booking.
    /// </summary>
    public override async Task<ApiWorkplaces.StartRoomBookingWorkflowResponse> StartRoomBookingWorkflow(ApiWorkplaces.StartRoomBookingWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling StartRoomBookingWorkflow for {Tenant}", requestContext.TenantId);
        if (request.RoomBooking is { } record)
        {
            var snapshot = RoomBookingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.ListEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.ListEmployeesRequest.Types.Filter
            {
                CreatedAfter = (request.RoomBooking?.Audit?.CreatedAt ?? new Timestamp()),
                Region = (request.RoomBooking?.Location?.Region ?? ""),
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _employeesEmployeeService.ListEmployeesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = request.StartRoomBookingWorkflowToPauseSlaPolicyWorkflow1();
        using var rpc1 = _helpdeskSlaPolicyWorkflowService.PauseSlaPolicyWorkflowAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiWorkplaces.StartRoomBookingWorkflowResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.SlaPolicy?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Metadata = { (request.RoomBooking?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                RoomBookingId = (request.RoomBookingId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.RoomBooking?.Title ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "room_booking",
            "audit", "StartRoomBookingWorkflow", (request.RoomBooking?.Title ?? ""), token);
        return response;
    }
}
