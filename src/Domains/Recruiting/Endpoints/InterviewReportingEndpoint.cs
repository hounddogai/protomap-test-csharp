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
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Recruiting;

/// <summary>
/// Builds and schedules reports about interviews.
/// </summary>
public sealed class InterviewReportingEndpoint : ApiRecruiting.InterviewReportingService.InterviewReportingServiceBase
{
    private readonly NotificationBus _bus;
    private readonly ApiDocuments.SignatureHistoryService.SignatureHistoryServiceClient _documentsSignatureHistoryService;
    private readonly ApiEmployees.EmployeeService.EmployeeServiceClient _employeesEmployeeService;
    private readonly ApiHelpdesk.SlaPolicyApprovalService.SlaPolicyApprovalServiceClient _helpdeskSlaPolicyApprovalService;
    private readonly IInterviewReportingEndpointSendGenerateInterviewReport0Port _interviewReportingEndpointSendGenerateInterviewReport0Port;
    private readonly CallInvoker _invoker;
    private readonly ILogger<InterviewReportingEndpoint> _logger;
    private readonly ApiPerformance.CalibrationApprovalService.CalibrationApprovalServiceClient _performanceCalibrationApprovalService;
    private readonly RetryPolicy _policy;
    private readonly ApiRecruiting.CandidateCalculationService.CandidateCalculationServiceClient _recruitingCandidateCalculationService;
    private readonly ApiRecruiting.CandidateReportingService.CandidateReportingServiceClient _recruitingCandidateReportingService;
    private readonly ApiRecruiting.InterviewSyncService.InterviewSyncServiceClient _recruitingInterviewSyncService;
    private readonly ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient _recruitingJobOpeningCalculationService;
    private readonly ApiRecruiting.JobOpeningQueryService.JobOpeningQueryServiceClient _recruitingJobOpeningQueryService;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiHelpdesk.RecallSlaPolicyRequest, CallOptions, AsyncUnaryCall<ApiHelpdesk.RecallSlaPolicyResponse>> _sendStreamInterviewMetrics1;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTimekeeping.ClockEventApprovalService.ClockEventApprovalServiceClient _timekeepingClockEventApprovalService;
    private readonly ApiVenues.VenueQueryService.VenueQueryServiceClient _venuesVenueQueryService;

    public InterviewReportingEndpoint(
        NotificationBus bus,
        ApiDocuments.SignatureHistoryService.SignatureHistoryServiceClient documentsSignatureHistoryService,
        ApiEmployees.EmployeeService.EmployeeServiceClient employeesEmployeeService,
        ApiHelpdesk.SlaPolicyApprovalService.SlaPolicyApprovalServiceClient helpdeskSlaPolicyApprovalService,
        IInterviewReportingEndpointSendGenerateInterviewReport0Port interviewReportingEndpointSendGenerateInterviewReport0PortAdapter,
        CallInvoker invoker,
        ILogger<InterviewReportingEndpoint> logger,
        ApiPerformance.CalibrationApprovalService.CalibrationApprovalServiceClient performanceCalibrationApprovalService,
        RetryPolicy policy,
        ApiRecruiting.CandidateCalculationService.CandidateCalculationServiceClient recruitingCandidateCalculationService,
        ApiRecruiting.CandidateReportingService.CandidateReportingServiceClient recruitingCandidateReportingService,
        ApiRecruiting.InterviewSyncService.InterviewSyncServiceClient recruitingInterviewSyncService,
        ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient recruitingJobOpeningCalculationService,
        ApiRecruiting.JobOpeningQueryService.JobOpeningQueryServiceClient recruitingJobOpeningQueryService,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiTimekeeping.ClockEventApprovalService.ClockEventApprovalServiceClient timekeepingClockEventApprovalService,
        ApiVenues.VenueQueryService.VenueQueryServiceClient venuesVenueQueryService)
    {
        _bus = bus;
        _documentsSignatureHistoryService = documentsSignatureHistoryService;
        _employeesEmployeeService = employeesEmployeeService;
        _helpdeskSlaPolicyApprovalService = helpdeskSlaPolicyApprovalService;
        _interviewReportingEndpointSendGenerateInterviewReport0Port = interviewReportingEndpointSendGenerateInterviewReport0PortAdapter;
        _invoker = invoker;
        _logger = logger;
        _performanceCalibrationApprovalService = performanceCalibrationApprovalService;
        _policy = policy;
        _recruitingCandidateCalculationService = recruitingCandidateCalculationService;
        _recruitingCandidateReportingService = recruitingCandidateReportingService;
        _recruitingInterviewSyncService = recruitingInterviewSyncService;
        _recruitingJobOpeningCalculationService = recruitingJobOpeningCalculationService;
        _recruitingJobOpeningQueryService = recruitingJobOpeningQueryService;
        _repository = repository;
        _settings = settings;
        _timekeepingClockEventApprovalService = timekeepingClockEventApprovalService;
        _venuesVenueQueryService = venuesVenueQueryService;
        _sendStreamInterviewMetrics1 = _helpdeskSlaPolicyApprovalService.RecallSlaPolicyAsync;
    }
    /// <summary>
    /// Deletes an interview.
    /// </summary>
    public override async Task<ApiRecruiting.DeleteInterviewReportResponse> DeleteInterviewReport(ApiRecruiting.DeleteInterviewReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.InterviewId, "interview_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling DeleteInterviewReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "interview", request.InterviewId,
            "DeleteInterviewReport", "accepted", token);
        var input0 = new ApiHelpdesk.EscalateSlaPolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Block = new ApiHelpdesk.SlaPolicyBlockAction
                {
                    Reason = (request.Reason ?? ""),
                },
            },
        };
        var descriptor0 = new Method<ApiHelpdesk.EscalateSlaPolicyRequest, ApiHelpdesk.EscalateSlaPolicyResponse>(
            MethodType.Unary, "acme.helpdesk.v1" + ".SlaPolicyApprovalService", "EscalateSlaPolicy",
            Wire.Marshaller<ApiHelpdesk.EscalateSlaPolicyRequest>(), Wire.Marshaller<ApiHelpdesk.EscalateSlaPolicyResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.BrowseJobOpeningsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiRecruiting.BrowseJobOpeningsRequest.Types.Filter
            {
                CreatedAfter = (reply0.Outcome?.EffectiveAt ?? new Timestamp()),
                Labels = { (reply0.SlaPolicy?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply0.SlaPolicy?.Owner?.UserId ?? ""),
            },
        };
        var command1 = new DispatchCommand<ApiRecruiting.BrowseJobOpeningsRequest>(input1,
            "primary", requestContext.TenantId);
        var routed1 = command1 with { Region = _settings.Value.Region };
        using var rpc1 = _recruitingJobOpeningQueryService.BrowseJobOpeningsAsync(routed1.Payload, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiTimekeeping.RejectClockEventRequest
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiTimekeeping.TimesheetRef
                {
                    DisplayName = (reply0.SlaPolicy?.Owner?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTimekeeping.RejectClockEventRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    Email = (reply0.SlaPolicy?.Owner?.Email ?? ""),
                    UserId = (reply0.SlaPolicy?.Owner?.UserId ?? ""),
                },
                EffectiveAt = (reply0.Outcome?.EffectiveAt ?? new Timestamp()),
            },
        };
        using var rpc2 = _timekeepingClockEventApprovalService.RejectClockEventAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiRecruiting.CalculateJobOpeningRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            JobOpening = new ApiRecruiting.JobOpening
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.SlaPolicy?.Audit?.Etag ?? ""),
                    UpdatedBy = (reply0.SlaPolicy?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        input3 = PropertyMap.Copy(input3);
        using var rpc3 = _recruitingJobOpeningCalculationService.CalculateJobOpeningAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiRecruiting.DeleteInterviewReportResponse
        {
            EffectiveAt = (reply2.Outcome?.EffectiveAt ?? new Timestamp()),
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply3.Result?.CorrelationId ?? ""),
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Outcome?.Message ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.InterviewId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "interview",
            "audit", "DeleteInterviewReport", (request.InterviewId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an interview.
    /// </summary>
    public override async Task<ApiRecruiting.FetchInterviewReportResponse> FetchInterviewReport(ApiRecruiting.FetchInterviewReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.InterviewId, "interview_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling FetchInterviewReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "interview", request.InterviewId,
            "FetchInterviewReport", "accepted", token);
        var input0 = request.FetchInterviewReportToBrowseVenues0();
        using var rpc0 = _venuesVenueQueryService.BrowseVenuesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiRecruiting.FetchInterviewReportResponse
        {
            Interview = new ApiRecruiting.Interview
            {
                InterviewId = (request.InterviewId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Generates an interview.
    /// </summary>
    public override async Task<ApiRecruiting.GenerateInterviewReportResponse> GenerateInterviewReport(ApiRecruiting.GenerateInterviewReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GenerateInterviewReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "interview", request.InterviewId,
            "GenerateInterviewReport", "accepted", token);
        var input0 = new ApiEmployees.CreateEmployeeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Employee = new ApiEmployees.Employee
            {
                Attributes = { (request.Details?.Filters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Preferences = new ApiEmployees.EmployeePreferences
                {
                    QuietHours = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        var reply0 = await SendGenerateInterviewReport0Async(input0, token);
        var response = new ApiRecruiting.GenerateInterviewReportResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "interview",
            "audit", "GenerateInterviewReport", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Lists interviews a page at a time.
    /// </summary>
    public override async Task<ApiRecruiting.ListInterviewReportsResponse> ListInterviewReports(ApiRecruiting.ListInterviewReportsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ListInterviewReports for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "interview", "batch",
            "ListInterviewReports", "accepted", token);
        var input0 = new ApiPerformance.EscalateCalibrationRequest
        {
            Calibration = new ApiPerformance.Calibration
            {
                History = { new ApiPerformance.CalibrationStatusChange
                {
                    ChangedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                } },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiPerformance.EscalateCalibrationRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _performanceCalibrationApprovalService.EscalateCalibrationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiRecruiting.ListInterviewReportsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "interview",
            "audit", "ListInterviewReports", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Schedules an interview.
    /// </summary>
    public override async Task<ApiRecruiting.ScheduleInterviewReportResponse> ScheduleInterviewReport(ApiRecruiting.ScheduleInterviewReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ScheduleInterviewReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "interview", request.InterviewId,
            "ScheduleInterviewReport", "accepted", token);
        var input0 = new ApiRecruiting.ScheduleCandidateReportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.ScheduleCandidateReportRequest.Types.Details
            {
                Format = (request.Details?.Format ?? ""),
                Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
                RecipientEmails = { (request.Details?.RecipientEmails ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiRecruiting.ScheduleCandidateReportRequest>();
        using var rpc0 = _recruitingCandidateReportingService.ScheduleCandidateReportAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.RecordSignatureVersionRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.RecordSignatureVersionRequest.Types.Details
            {
                AuthorEmail = (request.Context?.Actor?.Email ?? ""),
                Snapshot = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Reason = (request.Reason ?? ""),
            Signature = new ApiDocuments.Signature
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
        };
        using var rpc1 = _documentsSignatureHistoryService.RecordSignatureVersionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiRecruiting.SyncInterviewsRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.SyncInterviewsRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Interview = new ApiRecruiting.Interview
            {
                Organizer = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.Signature?.Subject?.DisplayName ?? ""),
                },
                Participants = { new ApiRecruiting.InterviewParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Context?.Actor?.Email ?? ""),
                    },
                } },
            },
        };
        using var rpc2 = _recruitingInterviewSyncService.SyncInterviewsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiRecruiting.SyncInterviewsRequest>("acme.recruiting.v1.InterviewReportingService/ScheduleInterviewReport/2", input2.Clone()), token);
        var input3 = new ApiEmployees.ListEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.ListEmployeesRequest.Types.Filter
            {
                CreatedAfter = (reply2.Outcome?.EffectiveAt ?? new Timestamp()),
                Labels = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Region = (reply2.Interview?.Location?.Region ?? ""),
            },
        };
        using var rpc3 = _employeesEmployeeService.ListEmployeesAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiRecruiting.ScheduleInterviewReportResponse
        {
            Period = (reply0.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply3.Result?.CorrelationId ?? ""),
                Message = (reply2.Result?.Message ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply1.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.InterviewId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "interview",
            "audit", "ScheduleInterviewReport", (request.InterviewId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Shares an interview.
    /// </summary>
    public override async Task<ApiRecruiting.ShareInterviewReportResponse> ShareInterviewReport(ApiRecruiting.ShareInterviewReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ShareInterviewReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "interview", request.InterviewId,
            "ShareInterviewReport", "accepted", token);
        var input0 = new ApiEmployees.ListEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.ListEmployeesRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
                Labels = { (request.Details?.Variables ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Details?.TemplateId ?? ""),
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _employeesEmployeeService.ListEmployeesAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiRecruiting.CalculateCandidateRequest
        {
            Candidate = new ApiRecruiting.Candidate
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Recipient?.Email ?? ""),
                    Name = (request.Details?.Recipient?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.Details?.Recipient?.Phone ?? ""),
                    PreferredChannel = (request.Details?.Recipient?.PreferredChannel ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply1 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _recruitingCandidateCalculationService.CalculateCandidateAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiRecruiting.ShareInterviewReportResponse
        {
            Interview = new ApiRecruiting.Interview
            {
                InterviewId = (request.InterviewId ?? ""),
                Organizer = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Recipient?.Email ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "interview",
            "audit", "ShareInterviewReport", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams the changes of interviews as they happen.
    /// </summary>
    public override async Task StreamInterviewMetrics(ApiRecruiting.StreamInterviewMetricsRequest request, IServerStreamWriter<ApiRecruiting.StreamInterviewMetricsResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling StreamInterviewMetrics for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "interview", "batch",
            "StreamInterviewMetrics", "accepted", token);
        var input0 = new ApiRecruiting.RecalculateJobOpeningsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.RecalculateJobOpeningsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
            JobOpening = new ApiRecruiting.JobOpening
            {
                DueAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _recruitingJobOpeningCalculationService.RecalculateJobOpeningsAsync);
        var input1 = new ApiHelpdesk.RecallSlaPolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.JobOpening?.Audit?.Etag ?? ""),
                    UpdatedAt = (reply0.JobOpening?.Audit?.UpdatedAt ?? new Timestamp()),
                    UpdatedBy = (reply0.JobOpening?.Audit?.UpdatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.JobOpening?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        using var rpc1 = _sendStreamInterviewMetrics1(input1, new CallOptions(cancellationToken: token));
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.StreamInterviewMetricsResponse
        {
            Interview = new ApiRecruiting.Interview
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.SlaPolicy?.Audit?.CreatedAt ?? new Timestamp()),
                    CreatedBy = (reply0.JobOpening?.Audit?.CreatedBy ?? ""),
                },
                Location = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
                Organizer = new ApiIdentity.UserRef
                {
                    Email = (request.Subscriber?.Email ?? ""),
                },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Subscriber?.PreferredChannel ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "interview",
            "audit", "StreamInterviewMetrics", (request.Subscriber?.PreferredChannel ?? ""), token);
        await responses.WriteAsync(response, token);
    }
    private async Task<ApiEmployees.CreateEmployeeResponse> SendGenerateInterviewReport0Async(ApiEmployees.CreateEmployeeRequest request, CancellationToken token)
    {
        return await _interviewReportingEndpointSendGenerateInterviewReport0Port.SendAsync(request, token);
    }
}
