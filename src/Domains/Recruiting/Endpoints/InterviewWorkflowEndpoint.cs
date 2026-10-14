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
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSearch = Acme.Search.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Recruiting;

/// <summary>
/// Runs the long-lived workflow that takes an interview from start to finish.
/// </summary>
public sealed class InterviewWorkflowEndpoint : ApiRecruiting.InterviewWorkflowService.InterviewWorkflowServiceBase
{
    private readonly ApiAudit.AuditEventValidationService.AuditEventValidationServiceClient _auditAuditEventValidationService;
    private readonly ApiBenefits.EnrollmentApprovalService.EnrollmentApprovalServiceClient _benefitsEnrollmentApprovalService;
    private readonly NotificationBus _bus;
    private readonly ApiDocuments.SignatureSyncService.SignatureSyncServiceClient _documentsSignatureSyncService;
    private readonly ApiEmployees.PositionValidationService.PositionValidationServiceClient _employeesPositionValidationService;
    private readonly ApiIdentity.UserValidationService.UserValidationServiceClient _identityUserValidationService;
    private readonly CallInvoker _invoker;
    private readonly ILogger<InterviewWorkflowEndpoint> _logger;
    private readonly ApiRecruiting.InterviewLifecycleService.InterviewLifecycleServiceClient _recruitingInterviewLifecycleService;
    private readonly ApiRecruiting.InterviewReportingService.InterviewReportingServiceClient _recruitingInterviewReportingService;
    private readonly ApiRecruiting.JobOpeningLifecycleService.JobOpeningLifecycleServiceClient _recruitingJobOpeningLifecycleService;
    private readonly ApiRecruiting.OfferApprovalService.OfferApprovalServiceClient _recruitingOfferApprovalService;
    private readonly IRecordRepository _repository;
    private readonly ApiSearch.SearchIndexQueryService.SearchIndexQueryServiceClient _searchSearchIndexQueryService;
    private readonly Func<ApiBenefits.EscalateEnrollmentRequest, CallOptions, AsyncUnaryCall<ApiBenefits.EscalateEnrollmentResponse>> _sendAdvanceInterviewWorkflow0;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiWorkplaces.RoomBookingWorkflowService.RoomBookingWorkflowServiceClient _workplacesRoomBookingWorkflowService;

    public InterviewWorkflowEndpoint(
        ApiAudit.AuditEventValidationService.AuditEventValidationServiceClient auditAuditEventValidationService,
        ApiBenefits.EnrollmentApprovalService.EnrollmentApprovalServiceClient benefitsEnrollmentApprovalService,
        NotificationBus bus,
        ApiDocuments.SignatureSyncService.SignatureSyncServiceClient documentsSignatureSyncService,
        ApiEmployees.PositionValidationService.PositionValidationServiceClient employeesPositionValidationService,
        ApiIdentity.UserValidationService.UserValidationServiceClient identityUserValidationService,
        CallInvoker invoker,
        ILogger<InterviewWorkflowEndpoint> logger,
        ApiRecruiting.InterviewLifecycleService.InterviewLifecycleServiceClient recruitingInterviewLifecycleService,
        ApiRecruiting.InterviewReportingService.InterviewReportingServiceClient recruitingInterviewReportingService,
        ApiRecruiting.JobOpeningLifecycleService.JobOpeningLifecycleServiceClient recruitingJobOpeningLifecycleService,
        ApiRecruiting.OfferApprovalService.OfferApprovalServiceClient recruitingOfferApprovalService,
        IRecordRepository repository,
        ApiSearch.SearchIndexQueryService.SearchIndexQueryServiceClient searchSearchIndexQueryService,
        IOptions<MeshOptions> settings,
        ApiWorkplaces.RoomBookingWorkflowService.RoomBookingWorkflowServiceClient workplacesRoomBookingWorkflowService)
    {
        _auditAuditEventValidationService = auditAuditEventValidationService;
        _benefitsEnrollmentApprovalService = benefitsEnrollmentApprovalService;
        _bus = bus;
        _documentsSignatureSyncService = documentsSignatureSyncService;
        _employeesPositionValidationService = employeesPositionValidationService;
        _identityUserValidationService = identityUserValidationService;
        _invoker = invoker;
        _logger = logger;
        _recruitingInterviewLifecycleService = recruitingInterviewLifecycleService;
        _recruitingInterviewReportingService = recruitingInterviewReportingService;
        _recruitingJobOpeningLifecycleService = recruitingJobOpeningLifecycleService;
        _recruitingOfferApprovalService = recruitingOfferApprovalService;
        _repository = repository;
        _searchSearchIndexQueryService = searchSearchIndexQueryService;
        _settings = settings;
        _workplacesRoomBookingWorkflowService = workplacesRoomBookingWorkflowService;
        _sendAdvanceInterviewWorkflow0 = _benefitsEnrollmentApprovalService.EscalateEnrollmentAsync;
    }
    /// <summary>
    /// Advances an interview.
    /// </summary>
    public override async Task<ApiRecruiting.AdvanceInterviewWorkflowResponse> AdvanceInterviewWorkflow(ApiRecruiting.AdvanceInterviewWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling AdvanceInterviewWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Interview is { } record)
        {
            var snapshot = InterviewMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiBenefits.EscalateEnrollmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiBenefits.EscalateEnrollmentRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Interview?.Organizer?.DisplayName ?? ""),
                    Email = (request.Interview?.Organizer?.Email ?? ""),
                },
            },
            Enrollment = new ApiBenefits.Enrollment
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                    Phone = (request.Details?.Notify?.Phone ?? ""),
                },
            },
        };
        using var rpc0 = _sendAdvanceInterviewWorkflow0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.ScheduleJobOpeningExpiryRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.ScheduleJobOpeningExpiryRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                },
            },
            JobOpening = new ApiRecruiting.JobOpening
            {
                Bank = new ApiRecruiting.JobOpeningBankSettlement
                {
                    AccountHolder = (reply0.Enrollment ?? new ApiBenefits.Enrollment()).PaymentCase switch
                    {
                        ApiBenefits.Enrollment.FundingOneofCase.Bank => (reply0.Enrollment?.Bank?.AccountHolder ?? ""),
                        _ => ""
                    },
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Interview?.Location?.CountryCode ?? ""),
                },
                Counterparty = new ApiRecruiting.InterviewRef
                {
                    DisplayName = (reply0.Enrollment?.Holder?.DisplayName ?? ""),
                },
            },
        };
        using var rpc1 = _recruitingJobOpeningLifecycleService.ScheduleJobOpeningExpiryAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiSearch.FindSearchIndexesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiSearch.FindSearchIndexesRequest.Types.Filter
            {
                CreatedAfter = (reply1.JobOpening?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (reply0.Enrollment?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Region = (reply1.JobOpening?.BillingAddress?.Region ?? ""),
            },
        };
        var descriptor2 = new Method<ApiSearch.FindSearchIndexesRequest, ApiSearch.FindSearchIndexesResponse>(
            MethodType.Unary, "acme.search.v1" + ".SearchIndexQueryService", "FindSearchIndexes",
            Wire.Marshaller<ApiSearch.FindSearchIndexesRequest>(), Wire.Marshaller<ApiSearch.FindSearchIndexesResponse>());
        using var rpc2 = _invoker.AsyncUnaryCall(descriptor2, null,
            new CallOptions(cancellationToken: token), input2);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiRecruiting.AdvanceInterviewWorkflowResponse
        {
            Interview = new ApiRecruiting.Interview
            {
                InterviewId = (request.InterviewId ?? ""),
                Location = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply1.JobOpening?.BillingAddress?.PostalCode ?? ""),
                },
                Metadata = { (request.Interview?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Participants = { new ApiRecruiting.InterviewParticipant
                {
                    Contact = (reply0.Enrollment?.Contact ?? new ApiCommon.ContactInfo()),
                } },
            },
            Outcome = new ApiRecruiting.AdvanceInterviewWorkflowResponse.Types.Outcome
            {
                Message = (reply0.Outcome?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply2.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "interview",
            "audit", "AdvanceInterviewWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Cancels an interview.
    /// </summary>
    public override async Task<ApiRecruiting.CancelInterviewWorkflowResponse> CancelInterviewWorkflow(ApiRecruiting.CancelInterviewWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CancelInterviewWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Interview is { } record)
        {
            var snapshot = InterviewMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.GetInterviewLifecycleRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            InterviewId = (request.Interview?.InterviewId ?? ""),
        };
        var command0 = new DispatchCommand<ApiRecruiting.GetInterviewLifecycleRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _recruitingInterviewLifecycleService.GetInterviewLifecycleAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiRecruiting.CancelInterviewWorkflowResponse
        {
            Interview = new ApiRecruiting.Interview
            {
                InterviewId = (request.InterviewId ?? ""),
                Metadata = { (request.Interview?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Organizer = new ApiIdentity.UserRef
                {
                    Email = (reply0.Interview?.Organizer?.Email ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "interview",
            "audit", "CancelInterviewWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Lists interviews a page at a time.
    /// </summary>
    public override async Task<ApiRecruiting.ListInterviewWorkflowsResponse> ListInterviewWorkflows(ApiRecruiting.ListInterviewWorkflowsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ListInterviewWorkflows for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "interview", "batch",
            "ListInterviewWorkflows", "accepted", token);
        var profile0 = new MappingProfile<ApiRecruiting.ListInterviewWorkflowsRequest, ApiRecruiting.RecallOfferRequest>(value => new ApiRecruiting.RecallOfferRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Offer = new ApiRecruiting.Offer
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (value.Filter?.Region ?? ""),
                },
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _recruitingOfferApprovalService.RecallOfferAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiRecruiting.ListInterviewWorkflowsResponse
        {
            Interviews = { new ApiRecruiting.Interview
            {
                Organizer = new ApiIdentity.UserRef
                {
                    Email = (reply0.Offer?.Counterparty?.Email ?? ""),
                },
            } },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "interview",
            "audit", "ListInterviewWorkflows", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pauses an interview.
    /// </summary>
    public override async Task<ApiRecruiting.PauseInterviewWorkflowResponse> PauseInterviewWorkflow(ApiRecruiting.PauseInterviewWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling PauseInterviewWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Interview is { } record)
        {
            var snapshot = InterviewMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.DeleteInterviewReportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Etag = (request.Interview?.Audit?.Etag ?? ""),
            InterviewId = (request.Interview?.InterviewId ?? ""),
            Reason = (request.Reason ?? ""),
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _recruitingInterviewReportingService.DeleteInterviewReportAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = request.PauseInterviewWorkflowToStartRoomBookingWorkflow1();
        using var rpc1 = _workplacesRoomBookingWorkflowService.StartRoomBookingWorkflowAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiWorkplaces.StartRoomBookingWorkflowRequest>("acme.recruiting.v1.InterviewWorkflowService/PauseInterviewWorkflow/1", input1.Clone()), token);
        var response = new ApiRecruiting.PauseInterviewWorkflowResponse
        {
            Interview = new ApiRecruiting.Interview
            {
                InterviewId = (request.InterviewId ?? ""),
                Location = new ApiCommon.PostalAddress
                {
                    Locality = (reply1.RoomBooking?.Location?.Locality ?? ""),
                },
                Organizer = (request.Interview?.Organizer ?? new ApiIdentity.UserRef()),
            },
            Outcome = new ApiRecruiting.PauseInterviewWorkflowResponse.Types.Outcome
            {
                NextSteps = { (reply1.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Interview?.Title ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "interview",
            "audit", "PauseInterviewWorkflow", (request.Interview?.Title ?? ""), token);
        return response;
    }
    /// <summary>
    /// Signals an interview.
    /// </summary>
    public override async Task<ApiRecruiting.SignalInterviewWorkflowResponse> SignalInterviewWorkflow(ApiRecruiting.SignalInterviewWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SignalInterviewWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Interview is { } record)
        {
            var snapshot = InterviewMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiEmployees.CheckPositionEligibilityResponse();
        foreach (var item in (request.Interview?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiRecruiting.InterviewParticipant>()))
        {
            var input0 = new ApiEmployees.CheckPositionEligibilityRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiEmployees.CheckPositionEligibilityRequest.Types.Details
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Name = (item.Contact?.Name ?? new ApiCommon.PersonName()),
                    },
                },
                Position = new ApiEmployees.Position
                {
                    Owner = new ApiIdentity.UserRef
                    {
                        Email = (item.Contact?.Email ?? ""),
                    },
                },
            };
            using var rpc0 = _employeesPositionValidationService.CheckPositionEligibilityAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var input1 = new ApiRecruiting.RejectOfferRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.RejectOfferRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    Email = (reply0.Position?.Owner?.Email ?? ""),
                },
            },
            Offer = new ApiRecruiting.Offer
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.Position?.Audit?.CreatedBy ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.Interview?.Organizer?.DisplayName ?? ""),
                },
                Lines = { new ApiRecruiting.OfferLine
                {
                    Tax = new ApiRecruiting.OfferLineTax
                    {
                        Amount = (reply0.Position ?? new ApiEmployees.Position()).ActionCase switch
                        {
                            ApiEmployees.Position.ActionOneofCase.Discount => (reply0.Position?.Discount?.Amount ?? new ApiCommon.Money()),
                            _ => new ApiCommon.Money()
                        },
                    },
                } },
            },
        };
        using var rpc1 = _recruitingOfferApprovalService.RejectOfferAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.SignalInterviewWorkflowResponse
        {
            Interview = new ApiRecruiting.Interview
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply1.Offer?.Audit?.CreatedBy ?? ""),
                },
                InterviewId = (request.InterviewId ?? ""),
                Window = (request.Interview?.Window ?? new ApiCommon.TimeWindow()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "interview",
            "audit", "SignalInterviewWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Starts an interview.
    /// </summary>
    public override async Task<ApiRecruiting.StartInterviewWorkflowResponse> StartInterviewWorkflow(ApiRecruiting.StartInterviewWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling StartInterviewWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Interview is { } record)
        {
            var snapshot = InterviewMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiIdentity.CheckUserEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiIdentity.CheckUserEligibilityRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                },
            },
            User = new ApiIdentity.User
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Interview?.Audit?.CreatedBy ?? ""),
                },
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Interview?.Location?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Region = (request.Interview?.Location?.Region ?? ""),
                },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiIdentity.CheckUserEligibilityRequest>();
        using var rpc0 = _identityUserValidationService.CheckUserEligibilityAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiAudit.ExplainAuditEventDecisionRequest
        {
            AuditEvent = new ApiAudit.AuditEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiAudit.AuditTrailRef
                {
                    DisplayName = (request.Interview?.Organizer?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiAudit.ExplainAuditEventDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply0.User?.RelationshipManager?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _auditAuditEventValidationService.ExplainAuditEventDecisionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDocuments.PushSignatureRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.PushSignatureRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiDocuments.DocumentRef
                {
                    DisplayName = (reply1.AuditEvent?.Subject?.DisplayName ?? ""),
                },
            },
        };
        using var rpc2 = _documentsSignatureSyncService.PushSignatureAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiRecruiting.StartInterviewWorkflowResponse
        {
            Interview = new ApiRecruiting.Interview
            {
                InterviewId = (request.InterviewId ?? ""),
                Location = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply0.User?.PrimaryAddress?.PostalCode ?? ""),
                },
                Title = (request.Interview?.Title ?? ""),
            },
            Outcome = new ApiRecruiting.StartInterviewWorkflowResponse.Types.Outcome
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply2.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "interview",
            "audit", "StartInterviewWorkflow", (request.Reason ?? ""), token);
        return response;
    }
}
