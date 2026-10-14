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
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Scheduling;

/// <summary>
/// Runs the long-lived workflow that takes an availability window from start to finish.
/// </summary>
public sealed class AvailabilityWindowWorkflowEndpoint : ApiScheduling.AvailabilityWindowWorkflowService.AvailabilityWindowWorkflowServiceBase
{
    private readonly ApiAudit.AuditEventService.AuditEventServiceClient _auditAuditEventService;
    private readonly ApiCorporateevents.VenueApprovalService.VenueApprovalServiceClient _corporateeventsVenueApprovalService;
    private readonly ApiDevices.ComplianceScanWorkflowService.ComplianceScanWorkflowServiceClient _devicesComplianceScanWorkflowService;
    private readonly ApiDocuments.DocumentQueryService.DocumentQueryServiceClient _documentsDocumentQueryService;
    private readonly ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient _employeesEmploymentContractCalculationService;
    private readonly ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient _employeesEmploymentContractValidationService;
    private readonly ApiEmployees.PositionValidationService.PositionValidationServiceClient _employeesPositionValidationService;
    private readonly CallInvoker _invoker;
    private readonly ILogger<AvailabilityWindowWorkflowEndpoint> _logger;
    private readonly ApiPerformance.ReviewWorkflowService.ReviewWorkflowServiceClient _performanceReviewWorkflowService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiScheduling.RosterApprovalService.RosterApprovalServiceClient _schedulingRosterApprovalService;
    private readonly ApiScheduling.RosterWorkflowService.RosterWorkflowServiceClient _schedulingRosterWorkflowService;
    private readonly ApiScheduling.ShiftApprovalService.ShiftApprovalServiceClient _schedulingShiftApprovalService;
    private readonly ApiScheduling.ShiftWorkflowService.ShiftWorkflowServiceClient _schedulingShiftWorkflowService;
    private readonly ApiScheduling.SwapRequestApprovalService.SwapRequestApprovalServiceClient _schedulingSwapRequestApprovalService;
    private readonly ApiScheduling.SwapRequestValidationService.SwapRequestValidationServiceClient _schedulingSwapRequestValidationService;
    private readonly Func<ApiScheduling.ApproveSwapRequestRequest, CallOptions, AsyncUnaryCall<ApiScheduling.ApproveSwapRequestResponse>> _sendStartAvailabilityWindowWorkflow2;
    private readonly ApiVenues.VenueValidationService.VenueValidationServiceClient _venuesVenueValidationService;

    public AvailabilityWindowWorkflowEndpoint(
        ApiAudit.AuditEventService.AuditEventServiceClient auditAuditEventService,
        ApiCorporateevents.VenueApprovalService.VenueApprovalServiceClient corporateeventsVenueApprovalService,
        ApiDevices.ComplianceScanWorkflowService.ComplianceScanWorkflowServiceClient devicesComplianceScanWorkflowService,
        ApiDocuments.DocumentQueryService.DocumentQueryServiceClient documentsDocumentQueryService,
        ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient employeesEmploymentContractCalculationService,
        ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient employeesEmploymentContractValidationService,
        ApiEmployees.PositionValidationService.PositionValidationServiceClient employeesPositionValidationService,
        CallInvoker invoker,
        ILogger<AvailabilityWindowWorkflowEndpoint> logger,
        ApiPerformance.ReviewWorkflowService.ReviewWorkflowServiceClient performanceReviewWorkflowService,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiScheduling.RosterApprovalService.RosterApprovalServiceClient schedulingRosterApprovalService,
        ApiScheduling.RosterWorkflowService.RosterWorkflowServiceClient schedulingRosterWorkflowService,
        ApiScheduling.ShiftApprovalService.ShiftApprovalServiceClient schedulingShiftApprovalService,
        ApiScheduling.ShiftWorkflowService.ShiftWorkflowServiceClient schedulingShiftWorkflowService,
        ApiScheduling.SwapRequestApprovalService.SwapRequestApprovalServiceClient schedulingSwapRequestApprovalService,
        ApiScheduling.SwapRequestValidationService.SwapRequestValidationServiceClient schedulingSwapRequestValidationService,
        ApiVenues.VenueValidationService.VenueValidationServiceClient venuesVenueValidationService)
    {
        _auditAuditEventService = auditAuditEventService;
        _corporateeventsVenueApprovalService = corporateeventsVenueApprovalService;
        _devicesComplianceScanWorkflowService = devicesComplianceScanWorkflowService;
        _documentsDocumentQueryService = documentsDocumentQueryService;
        _employeesEmploymentContractCalculationService = employeesEmploymentContractCalculationService;
        _employeesEmploymentContractValidationService = employeesEmploymentContractValidationService;
        _employeesPositionValidationService = employeesPositionValidationService;
        _invoker = invoker;
        _logger = logger;
        _performanceReviewWorkflowService = performanceReviewWorkflowService;
        _registry = registry;
        _repository = repository;
        _schedulingRosterApprovalService = schedulingRosterApprovalService;
        _schedulingRosterWorkflowService = schedulingRosterWorkflowService;
        _schedulingShiftApprovalService = schedulingShiftApprovalService;
        _schedulingShiftWorkflowService = schedulingShiftWorkflowService;
        _schedulingSwapRequestApprovalService = schedulingSwapRequestApprovalService;
        _schedulingSwapRequestValidationService = schedulingSwapRequestValidationService;
        _venuesVenueValidationService = venuesVenueValidationService;
        _sendStartAvailabilityWindowWorkflow2 = _schedulingSwapRequestApprovalService.ApproveSwapRequestAsync;
    }
    /// <summary>
    /// Advances an availability window.
    /// </summary>
    public override async Task<ApiScheduling.AdvanceAvailabilityWindowWorkflowResponse> AdvanceAvailabilityWindowWorkflow(ApiScheduling.AdvanceAvailabilityWindowWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling AdvanceAvailabilityWindowWorkflow for {Tenant}", requestContext.TenantId);
        if (request.AvailabilityWindow is { } record)
        {
            var snapshot = AvailabilityWindowMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiScheduling.DelegateRosterApprovalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Roster = new ApiScheduling.Roster
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.AvailabilityWindow?.Audit?.Etag ?? ""),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _schedulingRosterApprovalService.DelegateRosterApprovalAsync);
        var input1 = new ApiDevices.StartComplianceScanWorkflowRequest
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (reply0.Roster?.Counterparty?.DisplayName ?? ""),
                    Email = (request.AvailabilityWindow?.Organizer?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc1 = _devicesComplianceScanWorkflowService.StartComplianceScanWorkflowAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiScheduling.AdvanceAvailabilityWindowWorkflowResponse
        {
            AvailabilityWindow = new ApiScheduling.AvailabilityWindow
            {
                AvailabilityWindowId = (request.AvailabilityWindowId ?? ""),
                Organizer = new ApiIdentity.UserRef
                {
                    Email = (reply1.ComplianceScan?.Subject?.Email ?? ""),
                },
                Window = (request.AvailabilityWindow?.Window ?? new ApiCommon.TimeWindow()),
            },
            Outcome = new ApiScheduling.AdvanceAvailabilityWindowWorkflowResponse.Types.Outcome
            {
                Status = (reply1.Outcome?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "availability_window",
            "audit", "AdvanceAvailabilityWindowWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Cancels an availability window.
    /// </summary>
    public override async Task<ApiScheduling.CancelAvailabilityWindowWorkflowResponse> CancelAvailabilityWindowWorkflow(ApiScheduling.CancelAvailabilityWindowWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CancelAvailabilityWindowWorkflow for {Tenant}", requestContext.TenantId);
        if (request.AvailabilityWindow is { } record)
        {
            var snapshot = AvailabilityWindowMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.CalculateEmploymentContractRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.CalculateEmploymentContractRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Locality = (request.AvailabilityWindow?.Location?.Locality ?? ""),
                },
            },
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.AvailabilityWindow?.Location?.Region ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                },
            },
        };
        var named0 = _registry.Get<ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient>("acme.employees.v1.EmploymentContractCalculationService");
        using var rpc0 = named0.CalculateEmploymentContractAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.ValidatePositionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Position = new ApiEmployees.Position
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.EmploymentContract?.Audit?.UpdatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.AvailabilityWindow?.Organizer?.UserId ?? ""),
                },
            },
        };
        var descriptor1 = new Method<ApiEmployees.ValidatePositionRequest, ApiEmployees.ValidatePositionResponse>(
            MethodType.Unary, "acme.employees.v1" + ".PositionValidationService", "ValidatePosition",
            Wire.Marshaller<ApiEmployees.ValidatePositionRequest>(), Wire.Marshaller<ApiEmployees.ValidatePositionResponse>());
        using var rpc1 = _invoker.AsyncUnaryCall(descriptor1, null,
            new CallOptions(cancellationToken: token), input1);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiScheduling.CancelAvailabilityWindowWorkflowResponse
        {
            AvailabilityWindow = new ApiScheduling.AvailabilityWindow
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply1.Position?.Audit?.CreatedBy ?? ""),
                },
                AvailabilityWindowId = (request.AvailabilityWindowId ?? ""),
                Location = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply0.EmploymentContract?.BillingAddress?.PostalCode ?? ""),
                },
                Organizer = (request.AvailabilityWindow?.Organizer ?? new ApiIdentity.UserRef()),
            },
            Outcome = new ApiScheduling.CancelAvailabilityWindowWorkflowResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "availability_window",
            "audit", "CancelAvailabilityWindowWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an availability window.
    /// </summary>
    public override async Task<ApiScheduling.InspectAvailabilityWindowWorkflowResponse> InspectAvailabilityWindowWorkflow(ApiScheduling.InspectAvailabilityWindowWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.AvailabilityWindowId, "availability_window_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling InspectAvailabilityWindowWorkflow for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "availability_window", request.AvailabilityWindowId,
            "InspectAvailabilityWindowWorkflow", "accepted", token);
        var response = new ApiScheduling.InspectAvailabilityWindowWorkflowResponse
        {
            AvailabilityWindow = new ApiScheduling.AvailabilityWindow
            {
                AvailabilityWindowId = (request.AvailabilityWindowId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists availability windows a page at a time.
    /// </summary>
    public override async Task<ApiScheduling.ListAvailabilityWindowWorkflowsResponse> ListAvailabilityWindowWorkflows(ApiScheduling.ListAvailabilityWindowWorkflowsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ListAvailabilityWindowWorkflows for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "availability_window", "batch",
            "ListAvailabilityWindowWorkflows", "accepted", token);
        var response = new ApiScheduling.ListAvailabilityWindowWorkflowsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "availability_window",
            "audit", "ListAvailabilityWindowWorkflows", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pauses an availability window.
    /// </summary>
    public override async Task<ApiScheduling.PauseAvailabilityWindowWorkflowResponse> PauseAvailabilityWindowWorkflow(ApiScheduling.PauseAvailabilityWindowWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling PauseAvailabilityWindowWorkflow for {Tenant}", requestContext.TenantId);
        if (request.AvailabilityWindow is { } record)
        {
            var snapshot = AvailabilityWindowMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiScheduling.AdvanceShiftWorkflowResponse();
        foreach (var item in (request.AvailabilityWindow?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiScheduling.AvailabilityWindowParticipant>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input0 = new ApiScheduling.AdvanceShiftWorkflowRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiScheduling.AdvanceShiftWorkflowRequest.Types.Details
                {
                    Notify = new ApiCommon.ContactInfo
                    {
                        Email = (item.Contact?.Email ?? ""),
                    },
                },
            };
            using var rpc0 = _schedulingShiftWorkflowService.AdvanceShiftWorkflowAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var response = new ApiScheduling.PauseAvailabilityWindowWorkflowResponse
        {
            AvailabilityWindow = new ApiScheduling.AvailabilityWindow
            {
                AvailabilityWindowId = (request.AvailabilityWindowId ?? ""),
                Organizer = (request.AvailabilityWindow?.Organizer ?? new ApiIdentity.UserRef()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "availability_window",
            "audit", "PauseAvailabilityWindowWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Resumes an availability window.
    /// </summary>
    public override async Task<ApiScheduling.ResumeAvailabilityWindowWorkflowResponse> ResumeAvailabilityWindowWorkflow(ApiScheduling.ResumeAvailabilityWindowWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ResumeAvailabilityWindowWorkflow for {Tenant}", requestContext.TenantId);
        if (request.AvailabilityWindow is { } record)
        {
            var snapshot = AvailabilityWindowMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var profile0 = new MappingProfile<ApiScheduling.ResumeAvailabilityWindowWorkflowRequest, ApiCorporateevents.EscalateVenueRequest>(value => new ApiCorporateevents.EscalateVenueRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.EscalateVenueRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    UserId = (value.AvailabilityWindow?.Organizer?.UserId ?? ""),
                },
            },
            Venue = new ApiCorporateevents.Venue
            {
                Address = new ApiCommon.PostalAddress
                {
                    CountryCode = (value.AvailabilityWindow?.Location?.CountryCode ?? ""),
                    Recipient = (value.AvailabilityWindow?.Location?.Recipient ?? ""),
                },
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _corporateeventsVenueApprovalService.EscalateVenueAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var reply1 = new ApiScheduling.EscalateRosterResponse();
        foreach (var item in (request.AvailabilityWindow?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiScheduling.AvailabilityWindowParticipant>()))
        {
            var input1 = new ApiScheduling.EscalateRosterRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiScheduling.EscalateRosterRequest.Types.Details
                {
                    Approver = new ApiIdentity.UserRef
                    {
                        DisplayName = (reply0.Venue?.Owner?.DisplayName ?? ""),
                    },
                },
                Roster = new ApiScheduling.Roster
                {
                    BillingAddress = new ApiCommon.PostalAddress
                    {
                        Lines = { (reply0.Venue?.Address?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    },
                    Counterparty = new ApiEmployees.EmployeeRef
                    {
                        Email = (item.Contact?.Email ?? ""),
                    },
                },
            };
            using var rpc1 = _schedulingRosterApprovalService.EscalateRosterAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var input2 = request.ResumeAvailabilityWindowWorkflowToExplainVenueDecision2();
        using var rpc2 = _venuesVenueValidationService.ExplainVenueDecisionAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiEmployees.EstimateEmploymentContractRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.EstimateEmploymentContractRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (reply2.Venue ?? new ApiVenues.Venue()).LocatorCase switch
                    {
                        ApiVenues.Venue.LocatorOneofCase.Address => (reply2.Venue?.Address?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()),
                        _ => new Google.Protobuf.Collections.RepeatedField<string>()
                    } },
                },
            },
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply2.Venue?.Audit?.Etag ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (request.AvailabilityWindow?.Location?.Locality ?? ""),
                    Recipient = (request.AvailabilityWindow?.Location?.Recipient ?? ""),
                },
            },
        };
        using var rpc3 = _employeesEmploymentContractCalculationService.EstimateEmploymentContractAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiScheduling.ResumeAvailabilityWindowWorkflowResponse
        {
            AvailabilityWindow = new ApiScheduling.AvailabilityWindow
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply3.EmploymentContract?.Audit?.Etag ?? ""),
                },
                AvailabilityWindowId = (request.AvailabilityWindowId ?? ""),
                Location = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply0.Venue?.Address?.CountryCode ?? ""),
                    Lines = { (reply2.Venue?.Address?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
                Organizer = new ApiIdentity.UserRef
                {
                    UserId = (reply2.Venue?.Owner?.UserId ?? ""),
                },
                Status = (request.AvailabilityWindow?.Status ?? (ApiScheduling.AvailabilityWindowStatus)0),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply3.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "availability_window",
            "audit", "ResumeAvailabilityWindowWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Retrys an availability window.
    /// </summary>
    public override async Task<ApiScheduling.RetryAvailabilityWindowWorkflowResponse> RetryAvailabilityWindowWorkflow(ApiScheduling.RetryAvailabilityWindowWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RetryAvailabilityWindowWorkflow for {Tenant}", requestContext.TenantId);
        if (request.AvailabilityWindow is { } record)
        {
            var snapshot = AvailabilityWindowMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.PreviewEmploymentContractRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.AvailabilityWindow?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.AvailabilityWindow?.Organizer?.Email ?? ""),
                },
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _employeesEmploymentContractValidationService.PreviewEmploymentContractAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiPerformance.ListReviewWorkflowsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiPerformance.ListReviewWorkflowsRequest.Types.Filter
            {
                CreatedAfter = (reply0.Outcome?.EffectiveAt ?? new Timestamp()),
                Region = (reply0.EmploymentContract?.BillingAddress?.Region ?? ""),
            },
        };
        var packed1 = Any.Pack(input1);
        input1 = packed1.Unpack<ApiPerformance.ListReviewWorkflowsRequest>();
        using var rpc1 = _performanceReviewWorkflowService.ListReviewWorkflowsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var batch2 = (request.AvailabilityWindow?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiScheduling.AvailabilityWindowParticipant>()).Select(async item =>
        {
            var input2 = new ApiScheduling.AdvanceRosterWorkflowRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiScheduling.AdvanceRosterWorkflowRequest.Types.Details
                {
                    Notify = new ApiCommon.ContactInfo
                    {
                        Phone = (item.Contact?.Phone ?? ""),
                        PreferredChannel = (item.Contact?.PreferredChannel ?? ""),
                    },
                },
                Roster = new ApiScheduling.Roster
                {
                    Counterparty = new ApiEmployees.EmployeeRef
                    {
                        Email = (item.Contact?.Email ?? ""),
                    },
                },
            };
            using var rpc2 = _schedulingRosterWorkflowService.AdvanceRosterWorkflowAsync(input2, cancellationToken: token);
            var reply2 = await rpc2.ResponseAsync;
            return reply2;
        }).ToArray();
        var replies2 = await Task.WhenAll(batch2);
        var reply2 = replies2.LastOrDefault() ?? new ApiScheduling.AdvanceRosterWorkflowResponse();
        var response = new ApiScheduling.RetryAvailabilityWindowWorkflowResponse
        {
            AvailabilityWindow = new ApiScheduling.AvailabilityWindow
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.EmploymentContract?.Audit?.CreatedBy ?? ""),
                },
                AvailabilityWindowId = (request.AvailabilityWindowId ?? ""),
                Location = (request.AvailabilityWindow?.Location ?? new ApiCommon.PostalAddress()),
            },
            Outcome = new ApiScheduling.RetryAvailabilityWindowWorkflowResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "availability_window",
            "audit", "RetryAvailabilityWindowWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Signals an availability window.
    /// </summary>
    public override async Task<ApiScheduling.SignalAvailabilityWindowWorkflowResponse> SignalAvailabilityWindowWorkflow(ApiScheduling.SignalAvailabilityWindowWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SignalAvailabilityWindowWorkflow for {Tenant}", requestContext.TenantId);
        if (request.AvailabilityWindow is { } record)
        {
            var snapshot = AvailabilityWindowMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiScheduling.EscalateShiftResponse();
        foreach (var item in (request.AvailabilityWindow?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiScheduling.AvailabilityWindowParticipant>()))
        {
            var input0 = new ApiScheduling.EscalateShiftRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiScheduling.EscalateShiftRequest.Types.Details
                {
                    Approver = new ApiIdentity.UserRef
                    {
                        Email = (item.Contact?.Email ?? ""),
                    },
                },
                Shift = new ApiScheduling.Shift
                {
                    Participants = { new ApiScheduling.ShiftParticipant
                    {
                        Contact = new ApiCommon.ContactInfo
                        {
                            Phone = (item.Contact?.Phone ?? ""),
                            PreferredChannel = (item.Contact?.PreferredChannel ?? ""),
                        },
                        Required = item.Required,
                    } },
                },
            };
            using var rpc0 = _schedulingShiftApprovalService.EscalateShiftAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var input1 = new ApiDocuments.SuggestDocumentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.SuggestDocumentsRequest.Types.Filter
            {
                Region = (reply0.Shift?.Location?.Region ?? ""),
            },
        };
        using var rpc1 = _documentsDocumentQueryService.SuggestDocumentsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiScheduling.ExplainSwapRequestDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SwapRequest = new ApiScheduling.SwapRequest
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Shift?.Organizer?.DisplayName ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.Shift?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (reply0.Shift?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.AvailabilityWindow?.Organizer?.Email ?? ""),
                },
            },
        };
        var reply2 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _schedulingSwapRequestValidationService.ExplainSwapRequestDecisionAsync(input2, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiScheduling.SignalAvailabilityWindowWorkflowResponse
        {
            AvailabilityWindow = new ApiScheduling.AvailabilityWindow
            {
                AvailabilityWindowId = (request.AvailabilityWindowId ?? ""),
                Window = (request.AvailabilityWindow?.Window ?? new ApiCommon.TimeWindow()),
            },
            Outcome = new ApiScheduling.SignalAvailabilityWindowWorkflowResponse.Types.Outcome
            {
                Message = (reply1.Result?.Message ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply2.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply2.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "availability_window",
            "audit", "SignalAvailabilityWindowWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Starts an availability window.
    /// </summary>
    public override async Task<ApiScheduling.StartAvailabilityWindowWorkflowResponse> StartAvailabilityWindowWorkflow(ApiScheduling.StartAvailabilityWindowWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling StartAvailabilityWindowWorkflow for {Tenant}", requestContext.TenantId);
        if (request.AvailabilityWindow is { } record)
        {
            var snapshot = AvailabilityWindowMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.CommentVenueApprovalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Venue = new ApiCorporateevents.Venue
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.AvailabilityWindow?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        using var rpc0 = _corporateeventsVenueApprovalService.CommentVenueApprovalAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiAudit.SearchAuditEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiAudit.SearchAuditEventsRequest.Types.Filter
            {
                Region = (request.AvailabilityWindow?.Location?.Region ?? ""),
            },
        };
        using var rpc1 = _auditAuditEventService.SearchAuditEventsAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var input2 = new ApiScheduling.ApproveSwapRequestRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiScheduling.ApproveSwapRequestRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    Email = (request.AvailabilityWindow?.Organizer?.Email ?? ""),
                },
            },
            SwapRequest = new ApiScheduling.SwapRequest
            {
                Assignee = new ApiIdentity.UserRef
                {
                    UserId = (reply0.Venue?.Owner?.UserId ?? ""),
                },
            },
        };
        using var rpc2 = _sendStartAvailabilityWindowWorkflow2(input2, new CallOptions(cancellationToken: token));
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiScheduling.AdvanceRosterWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Roster = new ApiScheduling.Roster
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.AvailabilityWindow?.Audit?.UpdatedBy ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.AvailabilityWindow?.Organizer?.Email ?? ""),
                },
            },
        };
        var named3 = _registry.Get<ApiScheduling.RosterWorkflowService.RosterWorkflowServiceClient>("acme.scheduling.v1.RosterWorkflowService");
        using var rpc3 = named3.AdvanceRosterWorkflowAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiScheduling.StartAvailabilityWindowWorkflowResponse
        {
            AvailabilityWindow = new ApiScheduling.AvailabilityWindow
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.Venue?.Audit?.UpdatedAt ?? new Timestamp()),
                    UpdatedBy = (reply3.Roster?.Audit?.UpdatedBy ?? ""),
                },
                AvailabilityWindowId = (request.AvailabilityWindowId ?? ""),
                Status = (request.AvailabilityWindow?.Status ?? (ApiScheduling.AvailabilityWindowStatus)0),
            },
            Outcome = new ApiScheduling.StartAvailabilityWindowWorkflowResponse.Types.Outcome
            {
                Message = (reply1.Result?.Message ?? ""),
                ReferenceId = (reply0.Outcome?.ReferenceId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "availability_window",
            "audit", "StartAvailabilityWindowWorkflow", (request.Reason ?? ""), token);
        return response;
    }
}
