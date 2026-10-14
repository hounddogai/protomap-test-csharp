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
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLearning = Acme.Learning.V1;
using ApiPatients = Acme.Patients.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiTravel = Acme.Travel.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Timekeeping;

/// <summary>
/// Routes clock events through approval, with escalation and delegation.
/// </summary>
public sealed class ClockEventApprovalEndpoint : ApiTimekeeping.ClockEventApprovalService.ClockEventApprovalServiceBase
{
    private readonly NotificationBus _bus;
    private readonly IClockEventApprovalEndpointSendCommentClockEventApproval0Port _clockEventApprovalEndpointSendCommentClockEventApproval0Port;
    private readonly ApiDevices.ComplianceScanWorkflowService.ComplianceScanWorkflowServiceClient _devicesComplianceScanWorkflowService;
    private readonly ApiDocuments.SignatureValidationService.SignatureValidationServiceClient _documentsSignatureValidationService;
    private readonly ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient _employeesEmployeeQueryService;
    private readonly ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient _employeesEmploymentContractCalculationService;
    private readonly ApiEmployees.PositionValidationService.PositionValidationServiceClient _employeesPositionValidationService;
    private readonly ApiHelpdesk.SlaPolicyWorkflowService.SlaPolicyWorkflowServiceClient _helpdeskSlaPolicyWorkflowService;
    private readonly ApiItsm.IncidentApprovalService.IncidentApprovalServiceClient _itsmIncidentApprovalService;
    private readonly ApiItsm.ProblemSyncService.ProblemSyncServiceClient _itsmProblemSyncService;
    private readonly ApiLearning.CourseEnrollmentApprovalService.CourseEnrollmentApprovalServiceClient _learningCourseEnrollmentApprovalService;
    private readonly ILogger<ClockEventApprovalEndpoint> _logger;
    private readonly ApiPatients.PatientCalculationService.PatientCalculationServiceClient _patientsPatientCalculationService;
    private readonly RetryPolicy _policy;
    private readonly IRecordRepository _repository;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTimekeeping.ClockEventCalculationService.ClockEventCalculationServiceClient _timekeepingClockEventCalculationService;
    private readonly ApiTimekeeping.ClockEventValidationService.ClockEventValidationServiceClient _timekeepingClockEventValidationService;
    private readonly ApiTimekeeping.TimesheetQueryService.TimesheetQueryServiceClient _timekeepingTimesheetQueryService;
    private readonly ApiTravel.ItinerarySyncService.ItinerarySyncServiceClient _travelItinerarySyncService;
    private readonly ApiVenues.VenueValidationService.VenueValidationServiceClient _venuesVenueValidationService;

    public ClockEventApprovalEndpoint(
        NotificationBus bus,
        IClockEventApprovalEndpointSendCommentClockEventApproval0Port clockEventApprovalEndpointSendCommentClockEventApproval0PortAdapter,
        ApiDevices.ComplianceScanWorkflowService.ComplianceScanWorkflowServiceClient devicesComplianceScanWorkflowService,
        ApiDocuments.SignatureValidationService.SignatureValidationServiceClient documentsSignatureValidationService,
        ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient employeesEmployeeQueryService,
        ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient employeesEmploymentContractCalculationService,
        ApiEmployees.PositionValidationService.PositionValidationServiceClient employeesPositionValidationService,
        ApiHelpdesk.SlaPolicyWorkflowService.SlaPolicyWorkflowServiceClient helpdeskSlaPolicyWorkflowService,
        ApiItsm.IncidentApprovalService.IncidentApprovalServiceClient itsmIncidentApprovalService,
        ApiItsm.ProblemSyncService.ProblemSyncServiceClient itsmProblemSyncService,
        ApiLearning.CourseEnrollmentApprovalService.CourseEnrollmentApprovalServiceClient learningCourseEnrollmentApprovalService,
        ILogger<ClockEventApprovalEndpoint> logger,
        ApiPatients.PatientCalculationService.PatientCalculationServiceClient patientsPatientCalculationService,
        RetryPolicy policy,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiTimekeeping.ClockEventCalculationService.ClockEventCalculationServiceClient timekeepingClockEventCalculationService,
        ApiTimekeeping.ClockEventValidationService.ClockEventValidationServiceClient timekeepingClockEventValidationService,
        ApiTimekeeping.TimesheetQueryService.TimesheetQueryServiceClient timekeepingTimesheetQueryService,
        ApiTravel.ItinerarySyncService.ItinerarySyncServiceClient travelItinerarySyncService,
        ApiVenues.VenueValidationService.VenueValidationServiceClient venuesVenueValidationService)
    {
        _bus = bus;
        _clockEventApprovalEndpointSendCommentClockEventApproval0Port = clockEventApprovalEndpointSendCommentClockEventApproval0PortAdapter;
        _devicesComplianceScanWorkflowService = devicesComplianceScanWorkflowService;
        _documentsSignatureValidationService = documentsSignatureValidationService;
        _employeesEmployeeQueryService = employeesEmployeeQueryService;
        _employeesEmploymentContractCalculationService = employeesEmploymentContractCalculationService;
        _employeesPositionValidationService = employeesPositionValidationService;
        _helpdeskSlaPolicyWorkflowService = helpdeskSlaPolicyWorkflowService;
        _itsmIncidentApprovalService = itsmIncidentApprovalService;
        _itsmProblemSyncService = itsmProblemSyncService;
        _learningCourseEnrollmentApprovalService = learningCourseEnrollmentApprovalService;
        _logger = logger;
        _patientsPatientCalculationService = patientsPatientCalculationService;
        _policy = policy;
        _repository = repository;
        _settings = settings;
        _timekeepingClockEventCalculationService = timekeepingClockEventCalculationService;
        _timekeepingClockEventValidationService = timekeepingClockEventValidationService;
        _timekeepingTimesheetQueryService = timekeepingTimesheetQueryService;
        _travelItinerarySyncService = travelItinerarySyncService;
        _venuesVenueValidationService = venuesVenueValidationService;
    }
    /// <summary>
    /// Approves a clock event.
    /// </summary>
    public override async Task<ApiTimekeeping.ApproveClockEventResponse> ApproveClockEvent(ApiTimekeeping.ApproveClockEventRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ApproveClockEvent for {Tenant}", requestContext.TenantId);
        if (request.ClockEvent is { } record)
        {
            var snapshot = ClockEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = request.ApproveClockEventToExplainPositionDecision0();
        using var rpc0 = _employeesPositionValidationService.ExplainPositionDecisionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTimekeeping.ApproveClockEventResponse
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                Attributes = { (request.ClockEvent?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ClockEventId = (request.ClockEventId ?? ""),
            },
            Outcome = new ApiTimekeeping.ApproveClockEventResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "clock_event",
            "audit", "ApproveClockEvent", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Comments a clock event.
    /// </summary>
    public override async Task<ApiTimekeeping.CommentClockEventApprovalResponse> CommentClockEventApproval(ApiTimekeeping.CommentClockEventApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CommentClockEventApproval for {Tenant}", requestContext.TenantId);
        if (request.ClockEvent is { } record)
        {
            var snapshot = ClockEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.CommentIncidentApprovalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Incident = new ApiItsm.Incident
            {
                Assignee = new ApiIdentity.UserRef
                {
                    UserId = (request.Details?.Approver?.UserId ?? ""),
                },
                Customer = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                },
            },
        };
        var reply0 = await SendCommentClockEventApproval0Async(input0, token);
        var input1 = new ApiEmployees.NormalizePositionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Position = new ApiEmployees.Position
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply0.Incident ?? new ApiItsm.Incident()).OriginCase switch
                    {
                        ApiItsm.Incident.OriginOneofCase.Customer => (reply0.Incident?.Customer?.Email ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _employeesPositionValidationService.NormalizePositionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiItsm.ListProblemConflictsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiItsm.ListProblemConflictsRequest.Types.Filter
            {
                CreatedAfter = (reply0.Incident?.Sla?.DueAt ?? new Timestamp()),
            },
        };
        var packed2 = Any.Pack(input2);
        input2 = packed2.Unpack<ApiItsm.ListProblemConflictsRequest>();
        using var rpc2 = _itsmProblemSyncService.ListProblemConflictsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiTimekeeping.CommentClockEventApprovalResponse
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                ClockEventId = (request.ClockEventId ?? ""),
                Subject = (request.ClockEvent?.Subject ?? new ApiTimekeeping.TimesheetRef()),
            },
            Outcome = new ApiTimekeeping.CommentClockEventApprovalResponse.Types.Outcome
            {
                Message = (reply1.Result?.Message ?? ""),
                ReferenceId = (reply1.Outcome?.ReferenceId ?? ""),
                Status = (reply0.Outcome?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "clock_event",
            "audit", "CommentClockEventApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Delegates a clock event.
    /// </summary>
    public override async Task<ApiTimekeeping.DelegateClockEventApprovalResponse> DelegateClockEventApproval(ApiTimekeeping.DelegateClockEventApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling DelegateClockEventApproval for {Tenant}", requestContext.TenantId);
        if (request.ClockEvent is { } record)
        {
            var snapshot = ClockEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.BrowseEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.BrowseEmployeesRequest.Types.Filter
            {
                CreatedAfter = (request.Details?.EffectiveAt ?? new Timestamp()),
                Labels = { (request.ClockEvent?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.ClockEvent?.Subject?.TimesheetId ?? ""),
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiEmployees.BrowseEmployeesRequest>(json0);
        using var rpc0 = _employeesEmployeeQueryService.BrowseEmployeesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiEmployees.BrowseEmployeesRequest>("acme.timekeeping.v1.ClockEventApprovalService/DelegateClockEventApproval/0", input0.Clone()), token);
        var response = new ApiTimekeeping.DelegateClockEventApprovalResponse
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                ClockEventId = (request.ClockEventId ?? ""),
                Sequence = (request.ClockEvent?.Sequence ?? 0L),
            },
            Outcome = new ApiTimekeeping.DelegateClockEventApprovalResponse.Types.Outcome
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "clock_event",
            "audit", "DelegateClockEventApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Escalates a clock event.
    /// </summary>
    public override async Task<ApiTimekeeping.EscalateClockEventResponse> EscalateClockEvent(ApiTimekeeping.EscalateClockEventRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling EscalateClockEvent for {Tenant}", requestContext.TenantId);
        if (request.ClockEvent is { } record)
        {
            var snapshot = ClockEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiVenues.ValidateVenueRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Venue = new ApiVenues.Venue
        {
            Owner = new ApiIdentity.UserRef
            {
                DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
            },
        };
        using var rpc0 = _venuesVenueValidationService.ValidateVenueAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTravel.PushItineraryRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.PushItineraryRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Itinerary = new ApiTravel.Itinerary
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (reply0.Venue?.Address?.Region ?? ""),
                },
            },
        };
        using var rpc1 = _travelItinerarySyncService.PushItineraryAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        var input2 = new ApiLearning.SubmitCourseEnrollmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CourseEnrollment = new ApiLearning.CourseEnrollment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Venue?.Audit?.CreatedAt ?? new Timestamp()),
                    Etag = (reply0.Venue?.Audit?.Etag ?? ""),
                },
            },
            Details = new ApiLearning.SubmitCourseEnrollmentRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
        };
        using var rpc2 = _learningCourseEnrollmentApprovalService.SubmitCourseEnrollmentAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        await Task.WhenAll(pending1, pending2);
        var reply1 = await pending1;
        var reply2 = await pending2;
        var response = new ApiTimekeeping.EscalateClockEventResponse
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                ClockEventId = (request.ClockEventId ?? ""),
                Subject = new ApiTimekeeping.TimesheetRef
                {
                    Number = (reply1.Itinerary?.Counterparty?.Number ?? ""),
                },
            },
            Outcome = new ApiTimekeeping.EscalateClockEventResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Outcome?.Message ?? ""),
                Status = (reply1.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "clock_event",
            "audit", "EscalateClockEvent", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a clock event.
    /// </summary>
    public override async Task<ApiTimekeeping.GetClockEventApprovalStatusResponse> GetClockEventApprovalStatus(ApiTimekeeping.GetClockEventApprovalStatusRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ClockEventId, "clock_event_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetClockEventApprovalStatus for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "clock_event", request.ClockEventId,
            "GetClockEventApprovalStatus", "accepted", token);
        var input0 = new ApiTimekeeping.GetClockEventRatesRequest
        {
            ClockEventId = (request.ClockEventId ?? ""),
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _timekeepingClockEventCalculationService.GetClockEventRatesAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiTimekeeping.PreviewClockEventRequest
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                ClockEventId = (reply0.ClockEvent?.ClockEventId ?? ""),
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Sequence = (reply0.ClockEvent?.Sequence ?? 0L),
                Subject = new ApiTimekeeping.TimesheetRef
                {
                    TimesheetId = (reply0.ClockEvent?.Subject?.TimesheetId ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc1 = _timekeepingClockEventValidationService.PreviewClockEventAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTimekeeping.GetClockEventApprovalStatusResponse
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                ClockEventId = (request.ClockEventId ?? ""),
                Subject = new ApiTimekeeping.TimesheetRef
                {
                    DisplayName = (reply0.ClockEvent?.Subject?.DisplayName ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.ClockEvent?.CorrelationId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists clock events a page at a time.
    /// </summary>
    public override async Task<ApiTimekeeping.ListPendingClockEventsResponse> ListPendingClockEvents(ApiTimekeeping.ListPendingClockEventsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ListPendingClockEvents for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "clock_event", "batch",
            "ListPendingClockEvents", "accepted", token);
        var input0 = new ApiDevices.RetryComplianceScanWorkflowRequest
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                CorrelationId = (request.Filter?.OwnerId ?? ""),
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                OccurredAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                Subject = new ApiRecruiting.CandidateRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _devicesComplianceScanWorkflowService.RetryComplianceScanWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTimekeeping.ListPendingClockEventsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "clock_event",
            "audit", "ListPendingClockEvents", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalls a clock event.
    /// </summary>
    public override async Task<ApiTimekeeping.RecallClockEventResponse> RecallClockEvent(ApiTimekeeping.RecallClockEventRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RecallClockEvent for {Tenant}", requestContext.TenantId);
        if (request.ClockEvent is { } record)
        {
            var snapshot = ClockEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiDocuments.ValidateSignatureResponse();
        foreach (var item in (request.ClockEvent?.Measurements ?? new Google.Protobuf.Collections.RepeatedField<ApiTimekeeping.ClockEventMeasurement>()))
        {
            var input0 = new ApiDocuments.ValidateSignatureRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Signature = new ApiDocuments.Signature
                {
                    Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                    Measurements = { new ApiDocuments.SignatureMeasurement
                    {
                        ObservedAt = (item.ObservedAt ?? new Timestamp()),
                        Unit = (item.Unit ?? ""),
                    } },
                },
            };
            using var rpc0 = _documentsSignatureValidationService.ValidateSignatureAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var input1 = new ApiTimekeeping.QuoteClockEventRequest
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                OccurredAt = (reply0.Signature?.OccurredAt ?? new Timestamp()),
                Sequence = (request.ClockEvent?.Sequence ?? 0L),
                Subject = new ApiTimekeeping.TimesheetRef
                {
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                    Number = (request.ClockEvent?.Subject?.Number ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc1 = _timekeepingClockEventCalculationService.QuoteClockEventAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiPatients.EstimatePatientRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Patient = new ApiPatients.Patient
            {
                NationalId = new ApiPatients.PatientNationalId
                {
                    Number = (reply0.Signature?.Subject?.Number ?? ""),
                },
            },
        };
        var command2 = new DispatchCommand<ApiPatients.EstimatePatientRequest>(input2,
            "primary", requestContext.TenantId);
        var routed2 = command2 with { Region = _settings.Value.Region };
        using var rpc2 = _patientsPatientCalculationService.EstimatePatientAsync(routed2.Payload, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var profile3 = new MappingProfile<ApiTimekeeping.RecallClockEventRequest, ApiTimekeeping.FindTimesheetsRequest>(value => new ApiTimekeeping.FindTimesheetsRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiTimekeeping.FindTimesheetsRequest.Types.Filter
            {
                Region = (reply2.Patient?.PrimaryAddress?.Region ?? ""),
            },
        });
        var input3 = profile3.Map(request);
        using var rpc3 = _timekeepingTimesheetQueryService.FindTimesheetsAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiTimekeeping.RecallClockEventResponse
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                Attributes = { (reply1.ClockEvent?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ClockEventId = (request.ClockEventId ?? ""),
                OccurredAt = (request.ClockEvent?.OccurredAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
                ReferenceIds = { (reply3.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "clock_event",
            "audit", "RecallClockEvent", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Rejects a clock event.
    /// </summary>
    public override async Task<ApiTimekeeping.RejectClockEventResponse> RejectClockEvent(ApiTimekeeping.RejectClockEventRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RejectClockEvent for {Tenant}", requestContext.TenantId);
        if (request.ClockEvent is { } record)
        {
            var snapshot = ClockEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.ExplainEmploymentContractCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.ExplainEmploymentContractCalculationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.ClockEvent?.Subject?.DisplayName ?? ""),
                },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _employeesEmploymentContractCalculationService.ExplainEmploymentContractCalculationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = request.RejectClockEventToCancelSlaPolicyWorkflow1(reply0);
        using var rpc1 = _helpdeskSlaPolicyWorkflowService.CancelSlaPolicyWorkflowAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTimekeeping.RejectClockEventResponse
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                ClockEventId = (request.ClockEventId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (request.ClockEvent?.CorrelationId ?? ""),
                Message = (reply1.Result?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "clock_event",
            "audit", "RejectClockEvent", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Submits a clock event.
    /// </summary>
    public override async Task<ApiTimekeeping.SubmitClockEventResponse> SubmitClockEvent(ApiTimekeeping.SubmitClockEventRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SubmitClockEvent for {Tenant}", requestContext.TenantId);
        if (request.ClockEvent is { } record)
        {
            var snapshot = ClockEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTimekeeping.QuoteClockEventRequest
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Source = (request.ClockEvent?.Source ?? ""),
                Subject = new ApiTimekeeping.TimesheetRef
                {
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                    TimesheetId = (request.ClockEvent?.Subject?.TimesheetId ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _timekeepingClockEventCalculationService.QuoteClockEventAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTimekeeping.EstimateClockEventRequest
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                Attributes = { (request.ClockEvent?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiTimekeeping.TimesheetRef
                {
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                    Number = (request.ClockEvent?.Subject?.Number ?? ""),
                    TimesheetId = (request.ClockEvent?.Subject?.TimesheetId ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc1 = _timekeepingClockEventCalculationService.EstimateClockEventAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiEmployees.CalculateEmploymentContractRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (reply0.ClockEvent?.Subject?.DisplayName ?? ""),
                },
                Lines = { new ApiEmployees.EmploymentContractLine
                {
                    Attributes = { (reply1.ClockEvent?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                } },
            },
        };
        var packed2 = Any.Pack(input2);
        input2 = packed2.Unpack<ApiEmployees.CalculateEmploymentContractRequest>();
        using var rpc2 = _employeesEmploymentContractCalculationService.CalculateEmploymentContractAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var reply3 = new ApiEmployees.PreviewPositionResponse();
        foreach (var item in (request.ClockEvent?.Measurements ?? new Google.Protobuf.Collections.RepeatedField<ApiTimekeeping.ClockEventMeasurement>()))
        {
            var input3 = new ApiEmployees.PreviewPositionRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Position = new ApiEmployees.Position
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        Etag = (reply2.EmploymentContract?.Audit?.Etag ?? ""),
                        UpdatedAt = (item.ObservedAt ?? new Timestamp()),
                    },
                    Owner = new ApiIdentity.UserRef
                    {
                        Email = (request.Details?.Approver?.Email ?? ""),
                    },
                },
            };
            using var rpc3 = _employeesPositionValidationService.PreviewPositionAsync(input3, cancellationToken: token);
            reply3 = await rpc3.ResponseAsync;
        }
        var response = new ApiTimekeeping.SubmitClockEventResponse
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                Attributes = { (request.ClockEvent?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ClockEventId = (request.ClockEventId ?? ""),
                Kind = (reply0.ClockEvent?.Kind ?? ""),
                OccurredAt = (reply0.ClockEvent?.OccurredAt ?? new Timestamp()),
                Subject = new ApiTimekeeping.TimesheetRef
                {
                    DisplayName = (reply1.ClockEvent?.Subject?.DisplayName ?? ""),
                },
            },
            Outcome = new ApiTimekeeping.SubmitClockEventResponse.Types.Outcome
            {
                Message = (reply1.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "clock_event",
            "audit", "SubmitClockEvent", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiItsm.CommentIncidentApprovalResponse> SendCommentClockEventApproval0Async(ApiItsm.CommentIncidentApprovalRequest request, CancellationToken token)
    {
        return await _clockEventApprovalEndpointSendCommentClockEventApproval0Port.SendAsync(request, token);
    }
}
