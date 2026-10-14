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
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiRouting = Acme.Routing.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Recruiting;

/// <summary>
/// Builds and schedules reports about candidates.
/// </summary>
public sealed class CandidateReportingEndpoint : ApiRecruiting.CandidateReportingService.CandidateReportingServiceBase
{
    private readonly ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient _employeesEmploymentContractCalculationService;
    private readonly ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient _employeesEmploymentContractValidationService;
    private readonly ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient _employeesOrgUnitQueryService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.ServiceAssetApprovalService.ServiceAssetApprovalServiceClient _itsmServiceAssetApprovalService;
    private readonly ILogger<CandidateReportingEndpoint> _logger;
    private readonly ApiRecruiting.CandidateCalculationService.CandidateCalculationServiceClient _recruitingCandidateCalculationService;
    private readonly ApiRecruiting.CandidateValidationService.CandidateValidationServiceClient _recruitingCandidateValidationService;
    private readonly ApiRecruiting.InterviewSyncService.InterviewSyncServiceClient _recruitingInterviewSyncService;
    private readonly ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient _recruitingJobOpeningCalculationService;
    private readonly ApiRecruiting.JobOpeningService.JobOpeningServiceClient _recruitingJobOpeningService;
    private readonly ApiRecruiting.OfferCalculationService.OfferCalculationServiceClient _recruitingOfferCalculationService;
    private readonly ApiRecruiting.OfferValidationService.OfferValidationServiceClient _recruitingOfferValidationService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiRouting.RouteQueryService.RouteQueryServiceClient _routingRouteQueryService;
    private readonly Func<ApiItsm.RejectServiceAssetRequest, CallOptions, AsyncUnaryCall<ApiItsm.RejectServiceAssetResponse>> _sendListCandidateReports0;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiWorkplaces.RoomBookingApprovalService.RoomBookingApprovalServiceClient _workplacesRoomBookingApprovalService;

    public CandidateReportingEndpoint(
        ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient employeesEmploymentContractCalculationService,
        ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient employeesEmploymentContractValidationService,
        ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient employeesOrgUnitQueryService,
        CallInvoker invoker,
        ApiItsm.ServiceAssetApprovalService.ServiceAssetApprovalServiceClient itsmServiceAssetApprovalService,
        ILogger<CandidateReportingEndpoint> logger,
        ApiRecruiting.CandidateCalculationService.CandidateCalculationServiceClient recruitingCandidateCalculationService,
        ApiRecruiting.CandidateValidationService.CandidateValidationServiceClient recruitingCandidateValidationService,
        ApiRecruiting.InterviewSyncService.InterviewSyncServiceClient recruitingInterviewSyncService,
        ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient recruitingJobOpeningCalculationService,
        ApiRecruiting.JobOpeningService.JobOpeningServiceClient recruitingJobOpeningService,
        ApiRecruiting.OfferCalculationService.OfferCalculationServiceClient recruitingOfferCalculationService,
        ApiRecruiting.OfferValidationService.OfferValidationServiceClient recruitingOfferValidationService,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiRouting.RouteQueryService.RouteQueryServiceClient routingRouteQueryService,
        IOptions<MeshOptions> settings,
        ApiWorkplaces.RoomBookingApprovalService.RoomBookingApprovalServiceClient workplacesRoomBookingApprovalService)
    {
        _employeesEmploymentContractCalculationService = employeesEmploymentContractCalculationService;
        _employeesEmploymentContractValidationService = employeesEmploymentContractValidationService;
        _employeesOrgUnitQueryService = employeesOrgUnitQueryService;
        _invoker = invoker;
        _itsmServiceAssetApprovalService = itsmServiceAssetApprovalService;
        _logger = logger;
        _recruitingCandidateCalculationService = recruitingCandidateCalculationService;
        _recruitingCandidateValidationService = recruitingCandidateValidationService;
        _recruitingInterviewSyncService = recruitingInterviewSyncService;
        _recruitingJobOpeningCalculationService = recruitingJobOpeningCalculationService;
        _recruitingJobOpeningService = recruitingJobOpeningService;
        _recruitingOfferCalculationService = recruitingOfferCalculationService;
        _recruitingOfferValidationService = recruitingOfferValidationService;
        _registry = registry;
        _repository = repository;
        _routingRouteQueryService = routingRouteQueryService;
        _settings = settings;
        _workplacesRoomBookingApprovalService = workplacesRoomBookingApprovalService;
        _sendListCandidateReports0 = _itsmServiceAssetApprovalService.RejectServiceAssetAsync;
    }
    /// <summary>
    /// Counts candidates by status.
    /// </summary>
    public override async Task<ApiRecruiting.CountCandidateTotalsResponse> CountCandidateTotals(ApiRecruiting.CountCandidateTotalsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CountCandidateTotals for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "candidate", "batch",
            "CountCandidateTotals", "accepted", token);
        var input0 = new ApiRecruiting.WatchJobOpeningsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiRecruiting.WatchJobOpeningsRequest.Types.Filter
            {
                Labels = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        using var stream0 = _recruitingJobOpeningService.WatchJobOpenings(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received WatchJobOpenings frame {Type}", change0.GetType().Name);
        }
        var input1 = new ApiRecruiting.EstimateCandidateRequest
        {
            Candidate = new ApiRecruiting.Candidate
            {
                RelationshipManager = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.EstimateCandidateRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        var labels1 = input1.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input1.Context.Baggage.Clear();
        input1.Context.Baggage.Add(labels1);
        using var rpc1 = _recruitingCandidateCalculationService.EstimateCandidateAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.CountCandidateTotalsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "candidate",
            "audit", "CountCandidateTotals", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Deletes a candidate.
    /// </summary>
    public override async Task<ApiRecruiting.DeleteCandidateReportResponse> DeleteCandidateReport(ApiRecruiting.DeleteCandidateReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.CandidateId, "candidate_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling DeleteCandidateReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "candidate", request.CandidateId,
            "DeleteCandidateReport", "accepted", token);
        var input0 = new ApiRecruiting.ScoreOfferRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Offer = new ApiRecruiting.Offer
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Etag ?? ""),
                },
            },
            Reason = (request.Reason ?? ""),
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiRecruiting.ScoreOfferRequest>();
        using var rpc0 = _recruitingOfferValidationService.ScoreOfferAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.NormalizeEmploymentContractRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.NormalizeEmploymentContractRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply0.Offer?.Counterparty?.Email ?? ""),
                },
            },
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.Offer?.Audit?.UpdatedBy ?? ""),
                },
                Card = new ApiEmployees.EmploymentContractCardSettlement
                {
                    HolderName = (reply0.Offer ?? new ApiRecruiting.Offer()).PaymentCase switch
                    {
                        ApiRecruiting.Offer.SettlementOneofCase.Card => (reply0.Offer?.Card?.HolderName ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _employeesEmploymentContractValidationService.NormalizeEmploymentContractAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.DeleteCandidateReportResponse
        {
            EffectiveAt = (reply1.Outcome?.EffectiveAt ?? new Timestamp()),
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.CandidateId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "candidate",
            "audit", "DeleteCandidateReport", (request.CandidateId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a candidate.
    /// </summary>
    public override async Task<ApiRecruiting.FetchCandidateReportResponse> FetchCandidateReport(ApiRecruiting.FetchCandidateReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.CandidateId, "candidate_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling FetchCandidateReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "candidate", request.CandidateId,
            "FetchCandidateReport", "accepted", token);
        var input0 = new ApiRecruiting.SimulateJobOpeningRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.JobOpening = new ApiRecruiting.JobOpening
        {
            DueAt = (request.Context?.Deadline ?? new Timestamp()),
            Metadata = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
        };
        using var rpc0 = _recruitingJobOpeningCalculationService.SimulateJobOpeningAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.CalculateOfferRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.CalculateOfferRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply0.JobOpening?.BillingAddress?.PostalCode ?? ""),
                    Region = (reply0.JobOpening?.BillingAddress?.Region ?? ""),
                },
            },
            Offer = new ApiRecruiting.Offer
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.JobOpening?.Audit?.CreatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.JobOpening?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
        };
        using var rpc1 = _recruitingOfferCalculationService.CalculateOfferAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.FetchCandidateReportResponse
        {
            Candidate = new ApiRecruiting.Candidate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.JobOpening?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedAt = (reply1.Offer?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                CandidateId = (request.CandidateId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Generates a candidate.
    /// </summary>
    public override async Task<ApiRecruiting.GenerateCandidateReportResponse> GenerateCandidateReport(ApiRecruiting.GenerateCandidateReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GenerateCandidateReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "candidate", request.CandidateId,
            "GenerateCandidateReport", "accepted", token);
        var input0 = new ApiEmployees.SummarizeOrgUnitsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.SummarizeOrgUnitsRequest.Types.Details
            {
                Cron = (request.Details?.Cron ?? ""),
                Filters = { (request.Details?.Filters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Format = (request.Details?.Format ?? ""),
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _employeesOrgUnitQueryService.SummarizeOrgUnitsAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiRecruiting.ReconcileInterviewsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.ReconcileInterviewsRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Interview = new ApiRecruiting.Interview
            {
                Organizer = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _recruitingInterviewSyncService.ReconcileInterviewsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiWorkplaces.ApproveRoomBookingRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                Participants = { new ApiWorkplaces.RoomBookingParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (reply1.Interview?.Organizer?.Email ?? ""),
                    },
                } },
            },
        };
        var reply2 = await Workflow.SendAsync(input2, token, _workplacesRoomBookingApprovalService.ApproveRoomBookingAsync);
        var response = new ApiRecruiting.GenerateCandidateReportResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply2.Result?.Message ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "candidate",
            "audit", "GenerateCandidateReport", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Lists candidates a page at a time.
    /// </summary>
    public override async Task<ApiRecruiting.ListCandidateReportsResponse> ListCandidateReports(ApiRecruiting.ListCandidateReportsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ListCandidateReports for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "candidate", "batch",
            "ListCandidateReports", "accepted", token);
        var input0 = new ApiItsm.RejectServiceAssetRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Address = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        using var rpc0 = _sendListCandidateReports0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiRecruiting.ListCandidateReportsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "candidate",
            "audit", "ListCandidateReports", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Schedules a candidate.
    /// </summary>
    public override async Task<ApiRecruiting.ScheduleCandidateReportResponse> ScheduleCandidateReport(ApiRecruiting.ScheduleCandidateReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ScheduleCandidateReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "candidate", request.CandidateId,
            "ScheduleCandidateReport", "accepted", token);
        var input0 = new ApiRecruiting.LintCandidatesRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Candidate = new ApiRecruiting.Candidate
            {
                CandidateId = (request.CandidateId ?? ""),
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Reason ?? ""),
        };
        var named0 = _registry.Get<ApiRecruiting.CandidateValidationService.CandidateValidationServiceClient>("acme.recruiting.v1.CandidateValidationService");
        using var rpc0 = named0.LintCandidatesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.RecalculateEmploymentContractsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.RecalculateEmploymentContractsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.Candidate?.PrimaryAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Locality = (reply0.Candidate?.PrimaryAddress?.Locality ?? ""),
                    Recipient = (reply0.Candidate?.PrimaryAddress?.Recipient ?? ""),
                },
            },
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Candidate?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
        };
        var descriptor1 = new Method<ApiEmployees.RecalculateEmploymentContractsRequest, ApiEmployees.RecalculateEmploymentContractsResponse>(
            MethodType.Unary, "acme.employees.v1" + ".EmploymentContractCalculationService", "RecalculateEmploymentContracts",
            Wire.Marshaller<ApiEmployees.RecalculateEmploymentContractsRequest>(), Wire.Marshaller<ApiEmployees.RecalculateEmploymentContractsResponse>());
        using var rpc1 = _invoker.AsyncUnaryCall(descriptor1, null,
            new CallOptions(cancellationToken: token), input1);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.ScheduleCandidateReportResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.Format ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "candidate",
            "audit", "ScheduleCandidateReport", (request.Details?.Format ?? ""), token);
        return response;
    }
    /// <summary>
    /// Shares a candidate.
    /// </summary>
    public override async Task<ApiRecruiting.ShareCandidateReportResponse> ShareCandidateReport(ApiRecruiting.ShareCandidateReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ShareCandidateReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "candidate", request.CandidateId,
            "ShareCandidateReport", "accepted", token);
        var input0 = new ApiRouting.CountRoutesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiRouting.CountRoutesRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
                Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.CandidateId ?? ""),
            },
        };
        var command0 = new DispatchCommand<ApiRouting.CountRoutesRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _routingRouteQueryService.CountRoutesAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var profile1 = new MappingProfile<ApiRecruiting.ShareCandidateReportRequest, ApiEmployees.ResolveOrgUnitsRequest>(value => new ApiEmployees.ResolveOrgUnitsRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _employeesOrgUnitQueryService.ResolveOrgUnitsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.ShareCandidateReportResponse
        {
            Candidate = new ApiRecruiting.Candidate
            {
                CandidateId = (request.CandidateId ?? ""),
                Name = (request.Details?.Recipient?.Name ?? new ApiCommon.PersonName()),
            },
            Outcome = new ApiRecruiting.ShareCandidateReportResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "candidate",
            "audit", "ShareCandidateReport", (request.Reason ?? ""), token);
        return response;
    }
}
