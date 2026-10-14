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
using ApiCustomers = Acme.Customers.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Learning;

/// <summary>
/// Moves a course enrollment between the states of its lifecycle.
/// </summary>
public sealed class CourseEnrollmentLifecycleEndpoint : ApiLearning.CourseEnrollmentLifecycleService.CourseEnrollmentLifecycleServiceBase
{
    private readonly ICourseEnrollmentLifecycleEndpointSendActivateCourseEnrollment1Port _courseEnrollmentLifecycleEndpointSendActivateCourseEnrollment1Port;
    private readonly ApiCustomers.CustomerCalculationService.CustomerCalculationServiceClient _customersCustomerCalculationService;
    private readonly ApiDocuments.DocumentFolderExportService.DocumentFolderExportServiceClient _documentsDocumentFolderExportService;
    private readonly ApiDocuments.SignatureHistoryService.SignatureHistoryServiceClient _documentsSignatureHistoryService;
    private readonly ApiLearning.CertificationCalculationService.CertificationCalculationServiceClient _learningCertificationCalculationService;
    private readonly ApiLearning.CourseEnrollmentValidationService.CourseEnrollmentValidationServiceClient _learningCourseEnrollmentValidationService;
    private readonly ILogger<CourseEnrollmentLifecycleEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly ApiRecruiting.CandidateCalculationService.CandidateCalculationServiceClient _recruitingCandidateCalculationService;
    private readonly ApiRecruiting.InterviewValidationService.InterviewValidationServiceClient _recruitingInterviewValidationService;
    private readonly ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient _recruitingJobOpeningCalculationService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiLearning.CalculateCertificationRequest, CallOptions, AsyncUnaryCall<ApiLearning.CalculateCertificationResponse>> _sendSuspendCourseEnrollment1;

    public CourseEnrollmentLifecycleEndpoint(
        ICourseEnrollmentLifecycleEndpointSendActivateCourseEnrollment1Port courseEnrollmentLifecycleEndpointSendActivateCourseEnrollment1PortAdapter,
        ApiCustomers.CustomerCalculationService.CustomerCalculationServiceClient customersCustomerCalculationService,
        ApiDocuments.DocumentFolderExportService.DocumentFolderExportServiceClient documentsDocumentFolderExportService,
        ApiDocuments.SignatureHistoryService.SignatureHistoryServiceClient documentsSignatureHistoryService,
        ApiLearning.CertificationCalculationService.CertificationCalculationServiceClient learningCertificationCalculationService,
        ApiLearning.CourseEnrollmentValidationService.CourseEnrollmentValidationServiceClient learningCourseEnrollmentValidationService,
        ILogger<CourseEnrollmentLifecycleEndpoint> logger,
        RetryPolicy policy,
        ApiRecruiting.CandidateCalculationService.CandidateCalculationServiceClient recruitingCandidateCalculationService,
        ApiRecruiting.InterviewValidationService.InterviewValidationServiceClient recruitingInterviewValidationService,
        ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient recruitingJobOpeningCalculationService,
        ClientRegistry registry,
        IRecordRepository repository)
    {
        _courseEnrollmentLifecycleEndpointSendActivateCourseEnrollment1Port = courseEnrollmentLifecycleEndpointSendActivateCourseEnrollment1PortAdapter;
        _customersCustomerCalculationService = customersCustomerCalculationService;
        _documentsDocumentFolderExportService = documentsDocumentFolderExportService;
        _documentsSignatureHistoryService = documentsSignatureHistoryService;
        _learningCertificationCalculationService = learningCertificationCalculationService;
        _learningCourseEnrollmentValidationService = learningCourseEnrollmentValidationService;
        _logger = logger;
        _policy = policy;
        _recruitingCandidateCalculationService = recruitingCandidateCalculationService;
        _recruitingInterviewValidationService = recruitingInterviewValidationService;
        _recruitingJobOpeningCalculationService = recruitingJobOpeningCalculationService;
        _registry = registry;
        _repository = repository;
        _sendSuspendCourseEnrollment1 = _learningCertificationCalculationService.CalculateCertificationAsync;
    }
    /// <summary>
    /// Activates a course enrollment.
    /// </summary>
    public override async Task<ApiLearning.ActivateCourseEnrollmentResponse> ActivateCourseEnrollment(ApiLearning.ActivateCourseEnrollmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.CourseEnrollmentId, "course_enrollment_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ActivateCourseEnrollment for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "course_enrollment", request.CourseEnrollmentId,
            "ActivateCourseEnrollment", "accepted", token);
        var input0 = request.ActivateCourseEnrollmentToSimulateCertification0();
        using var rpc0 = _learningCertificationCalculationService.SimulateCertificationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLearning.GetCourseEnrollmentRulesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CourseEnrollmentId = (request.CourseEnrollmentId ?? ""),
        };
        var reply1 = await SendActivateCourseEnrollment1Async(input1, token);
        var response = new ApiLearning.ActivateCourseEnrollmentResponse
        {
            CourseEnrollment = new ApiLearning.CourseEnrollment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Certification?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedAt = (reply0.Certification?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Contact = (reply1.CourseEnrollment?.Contact ?? new ApiCommon.ContactInfo()),
                CourseEnrollmentId = (request.CourseEnrollmentId ?? ""),
                Holder = (reply1.CourseEnrollment?.Holder ?? new ApiRecruiting.CandidateRef()),
            },
            Outcome = new ApiLearning.ActivateCourseEnrollmentResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.ReasonCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "course_enrollment",
            "audit", "ActivateCourseEnrollment", (request.Details?.ReasonCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Expires a course enrollment.
    /// </summary>
    public override async Task<ApiLearning.ExpireCourseEnrollmentResponse> ExpireCourseEnrollment(ApiLearning.ExpireCourseEnrollmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.CourseEnrollmentId, "course_enrollment_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ExpireCourseEnrollment for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "course_enrollment", request.CourseEnrollmentId,
            "ExpireCourseEnrollment", "accepted", token);
        var input0 = new ApiRecruiting.PreviewInterviewRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Interview = new ApiRecruiting.Interview
            {
                Participants = { new ApiRecruiting.InterviewParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                    },
                } },
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _recruitingInterviewValidationService.PreviewInterviewAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.CompareSignatureVersionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.CompareSignatureVersionsRequest.Types.Details
            {
                AuthorEmail = (reply0.Interview?.Organizer?.Email ?? ""),
                Snapshot = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Signature = new ApiDocuments.Signature
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiDocuments.DocumentRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                },
            },
        };
        using var rpc1 = _documentsSignatureHistoryService.CompareSignatureVersionsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLearning.ExpireCourseEnrollmentResponse
        {
            CourseEnrollment = new ApiLearning.CourseEnrollment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.Interview?.Audit?.Etag ?? ""),
                },
                CourseEnrollmentId = (request.CourseEnrollmentId ?? ""),
            },
            Outcome = new ApiLearning.ExpireCourseEnrollmentResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Signature?.CorrelationId ?? ""),
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "course_enrollment",
            "audit", "ExpireCourseEnrollment", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a course enrollment.
    /// </summary>
    public override async Task<ApiLearning.GetCourseEnrollmentLifecycleResponse> GetCourseEnrollmentLifecycle(ApiLearning.GetCourseEnrollmentLifecycleRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.CourseEnrollmentId, "course_enrollment_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetCourseEnrollmentLifecycle for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "course_enrollment", request.CourseEnrollmentId,
            "GetCourseEnrollmentLifecycle", "accepted", token);
        var input0 = new ApiDocuments.ListDocumentFolderExportsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListDocumentFolderExportsRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
                Labels = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.CourseEnrollmentId ?? ""),
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiDocuments.ListDocumentFolderExportsRequest>(json0);
        using var rpc0 = _documentsDocumentFolderExportService.ListDocumentFolderExportsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiLearning.GetCourseEnrollmentLifecycleResponse
        {
            CourseEnrollment = new ApiLearning.CourseEnrollment
            {
                CourseEnrollmentId = (request.CourseEnrollmentId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Reinstates a course enrollment.
    /// </summary>
    public override async Task<ApiLearning.ReinstateCourseEnrollmentResponse> ReinstateCourseEnrollment(ApiLearning.ReinstateCourseEnrollmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.CourseEnrollmentId, "course_enrollment_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ReinstateCourseEnrollment for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "course_enrollment", request.CourseEnrollmentId,
            "ReinstateCourseEnrollment", "accepted", token);
        var input0 = new ApiLearning.PreviewCourseEnrollmentRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.CourseEnrollment = new ApiLearning.CourseEnrollment
        {
            Holder = new ApiRecruiting.CandidateRef
            {
                Email = (request.Details?.NewOwner?.Email ?? ""),
            },
        };
        using var rpc0 = _learningCourseEnrollmentValidationService.PreviewCourseEnrollmentAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLearning.CheckCourseEnrollmentEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CourseEnrollment = new ApiLearning.CourseEnrollment
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                },
                Holder = new ApiRecruiting.CandidateRef
                {
                    Email = (reply0.CourseEnrollment?.Contact?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _learningCourseEnrollmentValidationService.CheckCourseEnrollmentEligibilityAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLearning.ReinstateCourseEnrollmentResponse
        {
            CourseEnrollment = new ApiLearning.CourseEnrollment
            {
                CourseEnrollmentId = (reply1.CourseEnrollment?.CourseEnrollmentId ?? ""),
                Holder = new ApiRecruiting.CandidateRef
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                },
            },
            Outcome = new ApiLearning.ReinstateCourseEnrollmentResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Message = (reply0.Outcome?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "course_enrollment",
            "audit", "ReinstateCourseEnrollment", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Renews a course enrollment.
    /// </summary>
    public override async Task<ApiLearning.RenewCourseEnrollmentResponse> RenewCourseEnrollment(ApiLearning.RenewCourseEnrollmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.CourseEnrollmentId, "course_enrollment_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RenewCourseEnrollment for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "course_enrollment", request.CourseEnrollmentId,
            "RenewCourseEnrollment", "accepted", token);
        var input0 = new ApiCustomers.EstimateCustomerRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Customer = new ApiCustomers.Customer
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                    Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                },
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _customersCustomerCalculationService.EstimateCustomerAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiDocuments.RetryDocumentFolderExportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                Children = { new ApiDocuments.DocumentFolder
                {
                    Children = { new ApiDocuments.DocumentFolder
                    {
                        Location = new ApiCommon.PostalAddress
                        {
                            Lines = { (reply0.Customer?.PrimaryAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                            Recipient = (reply0.Customer?.PrimaryAddress?.Recipient ?? ""),
                            Region = (reply0.Customer?.PrimaryAddress?.Region ?? ""),
                        },
                        Manager = new ApiIdentity.UserRef
                        {
                            UserId = (reply0.Customer?.RelationshipManager?.UserId ?? ""),
                        },
                    } },
                } },
            },
        };
        var reply1 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _documentsDocumentFolderExportService.RetryDocumentFolderExportAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiLearning.RenewCourseEnrollmentResponse
        {
            CourseEnrollment = new ApiLearning.CourseEnrollment
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                    Name = (reply0.Customer?.Name ?? new ApiCommon.PersonName()),
                },
                CourseEnrollmentId = (request.CourseEnrollmentId ?? ""),
            },
            Outcome = new ApiLearning.RenewCourseEnrollmentResponse.Types.Outcome
            {
                EffectiveAt = (reply1.Outcome?.EffectiveAt ?? new Timestamp()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "course_enrollment",
            "audit", "RenewCourseEnrollment", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Suspends a course enrollment.
    /// </summary>
    public override async Task<ApiLearning.SuspendCourseEnrollmentResponse> SuspendCourseEnrollment(ApiLearning.SuspendCourseEnrollmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.CourseEnrollmentId, "course_enrollment_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SuspendCourseEnrollment for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "course_enrollment", request.CourseEnrollmentId,
            "SuspendCourseEnrollment", "accepted", token);
        var input0 = new ApiRecruiting.SimulateCandidateRequest
        {
            Candidate = new ApiRecruiting.Candidate
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply0 = await Workflow.SendAsync(input0, token, _recruitingCandidateCalculationService.SimulateCandidateAsync);
        var input1 = new ApiLearning.CalculateCertificationRequest
        {
            Certification = new ApiLearning.Certification
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.Candidate?.Audit?.Etag ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLearning.CalculateCertificationRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply0.Candidate?.PrimaryAddress?.PostalCode ?? ""),
                    Recipient = (reply0.Candidate?.PrimaryAddress?.Recipient ?? ""),
                },
            },
        };
        using var rpc1 = _sendSuspendCourseEnrollment1(input1, new CallOptions(cancellationToken: token));
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLearning.SuspendCourseEnrollmentResponse
        {
            CourseEnrollment = new ApiLearning.CourseEnrollment
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (reply0.Candidate?.Name ?? new ApiCommon.PersonName()),
                },
                CourseEnrollmentId = (request.CourseEnrollmentId ?? ""),
            },
            Outcome = new ApiLearning.SuspendCourseEnrollmentResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "course_enrollment",
            "audit", "SuspendCourseEnrollment", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Terminates a course enrollment.
    /// </summary>
    public override async Task<ApiLearning.TerminateCourseEnrollmentResponse> TerminateCourseEnrollment(ApiLearning.TerminateCourseEnrollmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.CourseEnrollmentId, "course_enrollment_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling TerminateCourseEnrollment for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "course_enrollment", request.CourseEnrollmentId,
            "TerminateCourseEnrollment", "accepted", token);
        var input0 = new ApiRecruiting.RecalculateJobOpeningsRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            JobOpening = new ApiRecruiting.JobOpening
            {
                Counterparty = new ApiRecruiting.InterviewRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                    StartsAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                },
            },
            Reason = (request.Reason ?? ""),
        };
        var named0 = _registry.Get<ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient>("acme.recruiting.v1.JobOpeningCalculationService");
        using var rpc0 = named0.RecalculateJobOpeningsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiLearning.TerminateCourseEnrollmentResponse
        {
            CourseEnrollment = new ApiLearning.CourseEnrollment
            {
                CourseEnrollmentId = (request.CourseEnrollmentId ?? ""),
            },
            Outcome = new ApiLearning.TerminateCourseEnrollmentResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Message = (reply0.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.ReasonCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "course_enrollment",
            "audit", "TerminateCourseEnrollment", (request.Details?.ReasonCode ?? ""), token);
        return response;
    }
    private async Task<ApiLearning.GetCourseEnrollmentRulesResponse> SendActivateCourseEnrollment1Async(ApiLearning.GetCourseEnrollmentRulesRequest request, CancellationToken token)
    {
        return await _courseEnrollmentLifecycleEndpointSendActivateCourseEnrollment1Port.SendAsync(request, token);
    }
}
