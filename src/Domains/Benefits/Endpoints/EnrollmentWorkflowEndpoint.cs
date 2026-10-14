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
using ApiBenefits = Acme.Benefits.V1;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSearch = Acme.Search.V1;

namespace Acme.Workforce.Benefits;

/// <summary>
/// Runs the long-lived workflow that takes an enrollment from start to finish.
/// </summary>
public sealed class EnrollmentWorkflowEndpoint : ApiBenefits.EnrollmentWorkflowService.EnrollmentWorkflowServiceBase
{
    private readonly ApiBenefits.BenefitPlanApprovalService.BenefitPlanApprovalServiceClient _benefitsBenefitPlanApprovalService;
    private readonly ApiBenefits.BenefitPlanQueryService.BenefitPlanQueryServiceClient _benefitsBenefitPlanQueryService;
    private readonly ApiBenefits.DependentValidationService.DependentValidationServiceClient _benefitsDependentValidationService;
    private readonly ApiBenefits.EnrollmentApprovalService.EnrollmentApprovalServiceClient _benefitsEnrollmentApprovalService;
    private readonly ApiBenefits.EnrollmentQueryService.EnrollmentQueryServiceClient _benefitsEnrollmentQueryService;
    private readonly ApiDevices.ManagedDeviceApprovalService.ManagedDeviceApprovalServiceClient _devicesManagedDeviceApprovalService;
    private readonly IEnrollmentWorkflowEndpointSendSignalEnrollmentWorkflow0Port _enrollmentWorkflowEndpointSendSignalEnrollmentWorkflow0Port;
    private readonly ApiIdentity.UserService.UserServiceClient _identityUserService;
    private readonly CallInvoker _invoker;
    private readonly ILogger<EnrollmentWorkflowEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly ApiRecruiting.InterviewCalculationService.InterviewCalculationServiceClient _recruitingInterviewCalculationService;
    private readonly ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient _recruitingJobOpeningCalculationService;
    private readonly ApiRecruiting.OfferValidationService.OfferValidationServiceClient _recruitingOfferValidationService;
    private readonly IRecordRepository _repository;
    private readonly ApiSearch.SearchIndexValidationService.SearchIndexValidationServiceClient _searchSearchIndexValidationService;
    private readonly Func<ApiBenefits.LookupEnrollmentRequest, CallOptions, AsyncUnaryCall<ApiBenefits.LookupEnrollmentResponse>> _sendResumeEnrollmentWorkflow0;
    private readonly IOptions<MeshOptions> _settings;

    public EnrollmentWorkflowEndpoint(
        ApiBenefits.BenefitPlanApprovalService.BenefitPlanApprovalServiceClient benefitsBenefitPlanApprovalService,
        ApiBenefits.BenefitPlanQueryService.BenefitPlanQueryServiceClient benefitsBenefitPlanQueryService,
        ApiBenefits.DependentValidationService.DependentValidationServiceClient benefitsDependentValidationService,
        ApiBenefits.EnrollmentApprovalService.EnrollmentApprovalServiceClient benefitsEnrollmentApprovalService,
        ApiBenefits.EnrollmentQueryService.EnrollmentQueryServiceClient benefitsEnrollmentQueryService,
        ApiDevices.ManagedDeviceApprovalService.ManagedDeviceApprovalServiceClient devicesManagedDeviceApprovalService,
        IEnrollmentWorkflowEndpointSendSignalEnrollmentWorkflow0Port enrollmentWorkflowEndpointSendSignalEnrollmentWorkflow0PortAdapter,
        ApiIdentity.UserService.UserServiceClient identityUserService,
        CallInvoker invoker,
        ILogger<EnrollmentWorkflowEndpoint> logger,
        RetryPolicy policy,
        ApiRecruiting.InterviewCalculationService.InterviewCalculationServiceClient recruitingInterviewCalculationService,
        ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient recruitingJobOpeningCalculationService,
        ApiRecruiting.OfferValidationService.OfferValidationServiceClient recruitingOfferValidationService,
        IRecordRepository repository,
        ApiSearch.SearchIndexValidationService.SearchIndexValidationServiceClient searchSearchIndexValidationService,
        IOptions<MeshOptions> settings)
    {
        _benefitsBenefitPlanApprovalService = benefitsBenefitPlanApprovalService;
        _benefitsBenefitPlanQueryService = benefitsBenefitPlanQueryService;
        _benefitsDependentValidationService = benefitsDependentValidationService;
        _benefitsEnrollmentApprovalService = benefitsEnrollmentApprovalService;
        _benefitsEnrollmentQueryService = benefitsEnrollmentQueryService;
        _devicesManagedDeviceApprovalService = devicesManagedDeviceApprovalService;
        _enrollmentWorkflowEndpointSendSignalEnrollmentWorkflow0Port = enrollmentWorkflowEndpointSendSignalEnrollmentWorkflow0PortAdapter;
        _identityUserService = identityUserService;
        _invoker = invoker;
        _logger = logger;
        _policy = policy;
        _recruitingInterviewCalculationService = recruitingInterviewCalculationService;
        _recruitingJobOpeningCalculationService = recruitingJobOpeningCalculationService;
        _recruitingOfferValidationService = recruitingOfferValidationService;
        _repository = repository;
        _searchSearchIndexValidationService = searchSearchIndexValidationService;
        _settings = settings;
        _sendResumeEnrollmentWorkflow0 = _benefitsEnrollmentQueryService.LookupEnrollmentAsync;
    }
    /// <summary>
    /// Advances an enrollment.
    /// </summary>
    public override async Task<ApiBenefits.AdvanceEnrollmentWorkflowResponse> AdvanceEnrollmentWorkflow(ApiBenefits.AdvanceEnrollmentWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling AdvanceEnrollmentWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Enrollment is { } record)
        {
            var snapshot = EnrollmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiIdentity.CloneUserRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            User = new ApiIdentity.User
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Enrollment?.Audit?.CreatedBy ?? ""),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Enrollment?.Contact?.Name ?? new ApiCommon.PersonName()),
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _identityUserService.CloneUserAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiBenefits.AdvanceEnrollmentWorkflowResponse
        {
            Enrollment = new ApiBenefits.Enrollment
            {
                Contact = (request.Enrollment?.Contact ?? new ApiCommon.ContactInfo()),
                EnrollmentId = (request.EnrollmentId ?? ""),
                Holder = new ApiRecruiting.CandidateRef
                {
                    Email = (reply0.User?.RelationshipManager?.Email ?? ""),
                },
            },
            Outcome = new ApiBenefits.AdvanceEnrollmentWorkflowResponse.Types.Outcome
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "enrollment",
            "audit", "AdvanceEnrollmentWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an enrollment.
    /// </summary>
    public override async Task<ApiBenefits.InspectEnrollmentWorkflowResponse> InspectEnrollmentWorkflow(ApiBenefits.InspectEnrollmentWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.EnrollmentId, "enrollment_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling InspectEnrollmentWorkflow for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "enrollment", request.EnrollmentId,
            "InspectEnrollmentWorkflow", "accepted", token);
        var input0 = new ApiBenefits.ScoreDependentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Dependent = new ApiBenefits.Dependent
            {
                RelationshipManager = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _benefitsDependentValidationService.ScoreDependentAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiBenefits.InspectEnrollmentWorkflowResponse
        {
            Enrollment = new ApiBenefits.Enrollment
            {
                EnrollmentId = (request.EnrollmentId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists enrollments a page at a time.
    /// </summary>
    public override async Task<ApiBenefits.ListEnrollmentWorkflowsResponse> ListEnrollmentWorkflows(ApiBenefits.ListEnrollmentWorkflowsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ListEnrollmentWorkflows for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "enrollment", "batch",
            "ListEnrollmentWorkflows", "accepted", token);
        var response = new ApiBenefits.ListEnrollmentWorkflowsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "enrollment",
            "audit", "ListEnrollmentWorkflows", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pauses an enrollment.
    /// </summary>
    public override async Task<ApiBenefits.PauseEnrollmentWorkflowResponse> PauseEnrollmentWorkflow(ApiBenefits.PauseEnrollmentWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling PauseEnrollmentWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Enrollment is { } record)
        {
            var snapshot = EnrollmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiBenefits.ResolveBenefitPlansRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _benefitsBenefitPlanQueryService.ResolveBenefitPlansAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiBenefits.DelegateEnrollmentApprovalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiBenefits.DelegateEnrollmentApprovalRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    Email = (request.Enrollment?.Holder?.Email ?? ""),
                },
            },
        };
        var reply1 = await Workflow.SendAsync(input1, token, _benefitsEnrollmentApprovalService.DelegateEnrollmentApprovalAsync);
        var response = new ApiBenefits.PauseEnrollmentWorkflowResponse
        {
            Enrollment = new ApiBenefits.Enrollment
            {
                Contact = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (reply1.Enrollment?.Contact?.PreferredChannel ?? ""),
                },
                CurrencyCode = (request.Enrollment?.CurrencyCode ?? ""),
                EnrollmentId = (request.EnrollmentId ?? ""),
                Holder = new ApiRecruiting.CandidateRef
                {
                    Email = (reply1.Enrollment?.Contact?.Email ?? ""),
                },
            },
            Outcome = new ApiBenefits.PauseEnrollmentWorkflowResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "enrollment",
            "audit", "PauseEnrollmentWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Resumes an enrollment.
    /// </summary>
    public override async Task<ApiBenefits.ResumeEnrollmentWorkflowResponse> ResumeEnrollmentWorkflow(ApiBenefits.ResumeEnrollmentWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ResumeEnrollmentWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Enrollment is { } record)
        {
            var snapshot = EnrollmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiBenefits.LookupEnrollmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EnrollmentId = (request.EnrollmentId ?? ""),
        };
        using var rpc0 = _sendResumeEnrollmentWorkflow0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var reply1 = new ApiBenefits.RejectBenefitPlanResponse();
        foreach (var item in (request.Enrollment?.Limits ?? new Google.Protobuf.Collections.RepeatedField<ApiBenefits.EnrollmentLimit>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input1 = new ApiBenefits.RejectBenefitPlanRequest
            {
                BenefitPlan = new ApiBenefits.BenefitPlan
                {
                    Discount = new ApiBenefits.BenefitPlanDiscountAction
                    {
                        Amount = (item.Amount ?? new ApiCommon.Money()),
                    },
                    Name = (item.Name ?? ""),
                    Owner = new ApiIdentity.UserRef
                    {
                        Email = (reply0.Enrollment?.Contact?.Email ?? ""),
                    },
                },
                Context = (request.Context ?? new ApiCommon.RequestContext()),
            };
            using var rpc1 = _benefitsBenefitPlanApprovalService.RejectBenefitPlanAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var input2 = new ApiBenefits.LintDependentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Dependent = new ApiBenefits.Dependent
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Enrollment?.Contact?.Email ?? ""),
                    PreferredChannel = (request.Enrollment?.Contact?.PreferredChannel ?? ""),
                },
            },
        };
        var descriptor2 = new Method<ApiBenefits.LintDependentsRequest, ApiBenefits.LintDependentsResponse>(
            MethodType.Unary, "acme.benefits.v1" + ".DependentValidationService", "LintDependents",
            Wire.Marshaller<ApiBenefits.LintDependentsRequest>(), Wire.Marshaller<ApiBenefits.LintDependentsResponse>());
        using var rpc2 = _invoker.AsyncUnaryCall(descriptor2, null,
            new CallOptions(cancellationToken: token), input2);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiDevices.RecallManagedDeviceRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply0.Enrollment?.Contact?.Email ?? ""),
                },
            },
        };
        var command3 = new DispatchCommand<ApiDevices.RecallManagedDeviceRequest>(input3,
            "primary", requestContext.TenantId);
        var routed3 = command3 with { Region = _settings.Value.Region };
        using var rpc3 = _devicesManagedDeviceApprovalService.RecallManagedDeviceAsync(routed3.Payload, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiBenefits.ResumeEnrollmentWorkflowResponse
        {
            Enrollment = new ApiBenefits.Enrollment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply3.ManagedDevice?.Audit?.CreatedBy ?? ""),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (reply2.Dependent?.Contact?.Phone ?? ""),
                },
                EnrollmentId = (request.EnrollmentId ?? ""),
                Holder = new ApiRecruiting.CandidateRef
                {
                    Email = (reply0.Enrollment?.Holder?.Email ?? ""),
                },
                Labels = { (request.Enrollment?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply3.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "enrollment",
            "audit", "ResumeEnrollmentWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Retrys an enrollment.
    /// </summary>
    public override async Task<ApiBenefits.RetryEnrollmentWorkflowResponse> RetryEnrollmentWorkflow(ApiBenefits.RetryEnrollmentWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RetryEnrollmentWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Enrollment is { } record)
        {
            var snapshot = EnrollmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var profile0 = new MappingProfile<ApiBenefits.RetryEnrollmentWorkflowRequest, ApiSearch.LintSearchIndexesRequest>(value => new ApiSearch.LintSearchIndexesRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiSearch.LintSearchIndexesRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (value.Enrollment?.Holder?.Email ?? ""),
                    Phone = (value.Details?.Notify?.Phone ?? ""),
                },
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _searchSearchIndexValidationService.LintSearchIndexesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.ExplainInterviewCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Interview = new ApiRecruiting.Interview
            {
                Participants = { new ApiRecruiting.InterviewParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Enrollment?.Contact?.Email ?? ""),
                        Phone = (request.Enrollment?.Contact?.Phone ?? ""),
                    },
                } },
            },
        };
        input1 = PropertyMap.Copy(input1);
        using var rpc1 = _recruitingInterviewCalculationService.ExplainInterviewCalculationAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = request.RetryEnrollmentWorkflowToNormalizeOffer2();
        using var rpc2 = _recruitingOfferValidationService.NormalizeOfferAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiBenefits.RetryEnrollmentWorkflowResponse
        {
            Enrollment = new ApiBenefits.Enrollment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.SearchIndex?.Audit?.CreatedBy ?? ""),
                    Etag = (reply1.Interview?.Audit?.Etag ?? ""),
                },
                CurrencyCode = (request.Enrollment?.CurrencyCode ?? ""),
                EnrollmentId = (request.EnrollmentId ?? ""),
            },
            Outcome = new ApiBenefits.RetryEnrollmentWorkflowResponse.Types.Outcome
            {
                Status = (reply2.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "enrollment",
            "audit", "RetryEnrollmentWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Signals an enrollment.
    /// </summary>
    public override async Task<ApiBenefits.SignalEnrollmentWorkflowResponse> SignalEnrollmentWorkflow(ApiBenefits.SignalEnrollmentWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SignalEnrollmentWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Enrollment is { } record)
        {
            var snapshot = EnrollmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiBenefits.EscalateEnrollmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiBenefits.EscalateEnrollmentRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Enrollment?.Holder?.DisplayName ?? ""),
                },
            },
            Enrollment = new ApiBenefits.Enrollment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Enrollment?.Audit?.Etag ?? ""),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Details?.Notify?.PreferredChannel ?? ""),
                },
            },
        };
        var reply0 = await SendSignalEnrollmentWorkflow0Async(input0, token);
        var batch1 = (request.Enrollment?.Limits ?? new Google.Protobuf.Collections.RepeatedField<ApiBenefits.EnrollmentLimit>()).Select(async item =>
        {
            var input1 = new ApiRecruiting.QuoteJobOpeningRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                JobOpening = new ApiRecruiting.JobOpening
                {
                    Card = new ApiRecruiting.JobOpeningCardSettlement
                    {
                        HolderName = (reply0.Enrollment?.Card?.HolderName ?? ""),
                    },
                    Lines = { new ApiRecruiting.JobOpeningLine
                    {
                        Amount = (item.Amount ?? new ApiCommon.Money()),
                    } },
                },
            };
            using var rpc1 = _recruitingJobOpeningCalculationService.QuoteJobOpeningAsync(input1, cancellationToken: token);
            var reply1 = await rpc1.ResponseAsync;
            return reply1;
        }).ToArray();
        var replies1 = await Task.WhenAll(batch1);
        var reply1 = replies1.LastOrDefault() ?? new ApiRecruiting.QuoteJobOpeningResponse();
        var response = new ApiBenefits.SignalEnrollmentWorkflowResponse
        {
            Enrollment = new ApiBenefits.Enrollment
            {
                Available = (request.Enrollment?.Available ?? new ApiCommon.Money()),
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (reply0.Enrollment?.Contact?.Phone ?? ""),
                },
                EnrollmentId = (request.EnrollmentId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "enrollment",
            "audit", "SignalEnrollmentWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Starts an enrollment.
    /// </summary>
    public override async Task<ApiBenefits.StartEnrollmentWorkflowResponse> StartEnrollmentWorkflow(ApiBenefits.StartEnrollmentWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling StartEnrollmentWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Enrollment is { } record)
        {
            var snapshot = EnrollmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.RejectManagedDeviceRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Enrollment?.Holder?.Email ?? ""),
                },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiDevices.RejectManagedDeviceRequest>();
        using var rpc0 = _devicesManagedDeviceApprovalService.RejectManagedDeviceAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiBenefits.StartEnrollmentWorkflowResponse
        {
            Enrollment = new ApiBenefits.Enrollment
            {
                Contact = (request.Enrollment?.Contact ?? new ApiCommon.ContactInfo()),
                EnrollmentId = (request.EnrollmentId ?? ""),
                Limits = { new ApiBenefits.EnrollmentLimit
                {
                    Name = (reply0.ManagedDevice?.Name ?? ""),
                } },
            },
            Outcome = new ApiBenefits.StartEnrollmentWorkflowResponse.Types.Outcome
            {
                NextSteps = { (reply0.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "enrollment",
            "audit", "StartEnrollmentWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiBenefits.EscalateEnrollmentResponse> SendSignalEnrollmentWorkflow0Async(ApiBenefits.EscalateEnrollmentRequest request, CancellationToken token)
    {
        return await _enrollmentWorkflowEndpointSendSignalEnrollmentWorkflow0Port.SendAsync(request, token);
    }
}
