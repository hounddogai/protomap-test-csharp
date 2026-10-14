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
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFuel = Acme.Fuel.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSearch = Acme.Search.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Benefits;

/// <summary>
/// Routes benefit plans through approval, with escalation and delegation.
/// </summary>
public sealed class BenefitPlanApprovalEndpoint : ApiBenefits.BenefitPlanApprovalService.BenefitPlanApprovalServiceBase
{
    private readonly IBenefitPlanApprovalEndpointSendEscalateBenefitPlan3Port _benefitPlanApprovalEndpointSendEscalateBenefitPlan3Port;
    private readonly ApiBenefits.DependentValidationService.DependentValidationServiceClient _benefitsDependentValidationService;
    private readonly ApiBenefits.EnrollmentQueryService.EnrollmentQueryServiceClient _benefitsEnrollmentQueryService;
    private readonly ApiBenefits.LifeEventValidationService.LifeEventValidationServiceClient _benefitsLifeEventValidationService;
    private readonly ApiCorporateevents.CorporateEventApprovalService.CorporateEventApprovalServiceClient _corporateeventsCorporateEventApprovalService;
    private readonly ApiDocuments.DocumentHistoryService.DocumentHistoryServiceClient _documentsDocumentHistoryService;
    private readonly ApiDocuments.SignatureSyncService.SignatureSyncServiceClient _documentsSignatureSyncService;
    private readonly ApiDocuments.SignatureValidationService.SignatureValidationServiceClient _documentsSignatureValidationService;
    private readonly ApiFuel.FuelCardQueryService.FuelCardQueryServiceClient _fuelFuelCardQueryService;
    private readonly CallInvoker _invoker;
    private readonly ILogger<BenefitPlanApprovalEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly ApiRecruiting.CandidateQueryService.CandidateQueryServiceClient _recruitingCandidateQueryService;
    private readonly ApiRecruiting.CandidateValidationService.CandidateValidationServiceClient _recruitingCandidateValidationService;
    private readonly ApiRecruiting.InterviewCalculationService.InterviewCalculationServiceClient _recruitingInterviewCalculationService;
    private readonly ApiRecruiting.JobOpeningService.JobOpeningServiceClient _recruitingJobOpeningService;
    private readonly ApiRecruiting.JobOpeningValidationService.JobOpeningValidationServiceClient _recruitingJobOpeningValidationService;
    private readonly ApiRecruiting.OfferValidationService.OfferValidationServiceClient _recruitingOfferValidationService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiSearch.SearchIndexQueryService.SearchIndexQueryServiceClient _searchSearchIndexQueryService;
    private readonly Func<ApiDocuments.ListDocumentVersionsRequest, CallOptions, AsyncUnaryCall<ApiDocuments.ListDocumentVersionsResponse>> _sendApproveBenefitPlan3;
    private readonly ApiWorkplaces.RoomBookingWorkflowService.RoomBookingWorkflowServiceClient _workplacesRoomBookingWorkflowService;

    public BenefitPlanApprovalEndpoint(
        IBenefitPlanApprovalEndpointSendEscalateBenefitPlan3Port benefitPlanApprovalEndpointSendEscalateBenefitPlan3PortAdapter,
        ApiBenefits.DependentValidationService.DependentValidationServiceClient benefitsDependentValidationService,
        ApiBenefits.EnrollmentQueryService.EnrollmentQueryServiceClient benefitsEnrollmentQueryService,
        ApiBenefits.LifeEventValidationService.LifeEventValidationServiceClient benefitsLifeEventValidationService,
        ApiCorporateevents.CorporateEventApprovalService.CorporateEventApprovalServiceClient corporateeventsCorporateEventApprovalService,
        ApiDocuments.DocumentHistoryService.DocumentHistoryServiceClient documentsDocumentHistoryService,
        ApiDocuments.SignatureSyncService.SignatureSyncServiceClient documentsSignatureSyncService,
        ApiDocuments.SignatureValidationService.SignatureValidationServiceClient documentsSignatureValidationService,
        ApiFuel.FuelCardQueryService.FuelCardQueryServiceClient fuelFuelCardQueryService,
        CallInvoker invoker,
        ILogger<BenefitPlanApprovalEndpoint> logger,
        RetryPolicy policy,
        ApiRecruiting.CandidateQueryService.CandidateQueryServiceClient recruitingCandidateQueryService,
        ApiRecruiting.CandidateValidationService.CandidateValidationServiceClient recruitingCandidateValidationService,
        ApiRecruiting.InterviewCalculationService.InterviewCalculationServiceClient recruitingInterviewCalculationService,
        ApiRecruiting.JobOpeningService.JobOpeningServiceClient recruitingJobOpeningService,
        ApiRecruiting.JobOpeningValidationService.JobOpeningValidationServiceClient recruitingJobOpeningValidationService,
        ApiRecruiting.OfferValidationService.OfferValidationServiceClient recruitingOfferValidationService,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiSearch.SearchIndexQueryService.SearchIndexQueryServiceClient searchSearchIndexQueryService,
        ApiWorkplaces.RoomBookingWorkflowService.RoomBookingWorkflowServiceClient workplacesRoomBookingWorkflowService)
    {
        _benefitPlanApprovalEndpointSendEscalateBenefitPlan3Port = benefitPlanApprovalEndpointSendEscalateBenefitPlan3PortAdapter;
        _benefitsDependentValidationService = benefitsDependentValidationService;
        _benefitsEnrollmentQueryService = benefitsEnrollmentQueryService;
        _benefitsLifeEventValidationService = benefitsLifeEventValidationService;
        _corporateeventsCorporateEventApprovalService = corporateeventsCorporateEventApprovalService;
        _documentsDocumentHistoryService = documentsDocumentHistoryService;
        _documentsSignatureSyncService = documentsSignatureSyncService;
        _documentsSignatureValidationService = documentsSignatureValidationService;
        _fuelFuelCardQueryService = fuelFuelCardQueryService;
        _invoker = invoker;
        _logger = logger;
        _policy = policy;
        _recruitingCandidateQueryService = recruitingCandidateQueryService;
        _recruitingCandidateValidationService = recruitingCandidateValidationService;
        _recruitingInterviewCalculationService = recruitingInterviewCalculationService;
        _recruitingJobOpeningService = recruitingJobOpeningService;
        _recruitingJobOpeningValidationService = recruitingJobOpeningValidationService;
        _recruitingOfferValidationService = recruitingOfferValidationService;
        _registry = registry;
        _repository = repository;
        _searchSearchIndexQueryService = searchSearchIndexQueryService;
        _workplacesRoomBookingWorkflowService = workplacesRoomBookingWorkflowService;
        _sendApproveBenefitPlan3 = _documentsDocumentHistoryService.ListDocumentVersionsAsync;
    }
    /// <summary>
    /// Approves a benefit plan.
    /// </summary>
    public override async Task<ApiBenefits.ApproveBenefitPlanResponse> ApproveBenefitPlan(ApiBenefits.ApproveBenefitPlanRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ApproveBenefitPlan for {Tenant}", requestContext.TenantId);
        if (request.BenefitPlan is { } record)
        {
            var snapshot = BenefitPlanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiFuel.FindFuelCardsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiFuel.FindFuelCardsRequest.Types.Filter
            {
                CreatedAfter = (request.Details?.EffectiveAt ?? new Timestamp()),
                OwnerId = (request.Details?.Approver?.UserId ?? ""),
            },
        };
        using var rpc0 = _fuelFuelCardQueryService.FindFuelCardsAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiSearch.BrowseSearchIndexesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiSearch.BrowseSearchIndexesRequest.Types.Filter
            {
                CreatedAfter = (request.BenefitPlan?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (request.BenefitPlan?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.BenefitPlan?.Owner?.UserId ?? ""),
            },
        };
        using var rpc1 = _searchSearchIndexQueryService.BrowseSearchIndexesAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var input2 = new ApiRecruiting.LintJobOpeningsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            JobOpening = new ApiRecruiting.JobOpening
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.BenefitPlan?.Audit?.CreatedAt ?? new Timestamp()),
                    Etag = (request.BenefitPlan?.Audit?.Etag ?? ""),
                },
                Counterparty = new ApiRecruiting.InterviewRef
                {
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                },
            },
        };
        var reply2 = await Workflow.SendAsync(input2, token, _recruitingJobOpeningValidationService.LintJobOpeningsAsync);
        var input3 = new ApiDocuments.ListDocumentVersionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListDocumentVersionsRequest.Types.Filter
            {
                CreatedAfter = (request.BenefitPlan?.Audit?.CreatedAt ?? new Timestamp()),
                OwnerId = (reply2.JobOpening?.Counterparty?.InterviewId ?? ""),
                Region = (reply2.JobOpening?.BillingAddress?.Region ?? ""),
            },
        };
        using var rpc3 = _sendApproveBenefitPlan3(input3, new CallOptions(cancellationToken: token));
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiBenefits.ApproveBenefitPlanResponse
        {
            BenefitPlan = new ApiBenefits.BenefitPlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply2.JobOpening?.Audit?.UpdatedBy ?? ""),
                },
                BenefitPlanId = (request.BenefitPlanId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply2.JobOpening?.Counterparty?.DisplayName ?? ""),
                },
            },
            Outcome = new ApiBenefits.ApproveBenefitPlanResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Message = (reply3.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply1.Result?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "benefit_plan",
            "audit", "ApproveBenefitPlan", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Delegates a benefit plan.
    /// </summary>
    public override async Task<ApiBenefits.DelegateBenefitPlanApprovalResponse> DelegateBenefitPlanApproval(ApiBenefits.DelegateBenefitPlanApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling DelegateBenefitPlanApproval for {Tenant}", requestContext.TenantId);
        if (request.BenefitPlan is { } record)
        {
            var snapshot = BenefitPlanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.EstimateInterviewRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Interview = new ApiRecruiting.Interview
            {
                Organizer = new ApiIdentity.UserRef
                {
                    UserId = (request.BenefitPlan?.Owner?.UserId ?? ""),
                },
                Participants = { new ApiRecruiting.InterviewParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.BenefitPlan?.Owner?.Email ?? ""),
                    },
                } },
            },
        };
        var named0 = _registry.Get<ApiRecruiting.InterviewCalculationService.InterviewCalculationServiceClient>("acme.recruiting.v1.InterviewCalculationService");
        using var rpc0 = named0.EstimateInterviewAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.ValidateOfferRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Offer = new ApiRecruiting.Offer
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.BenefitPlan?.Audit?.CreatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply0.Interview?.Location?.PostalCode ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.BenefitPlan?.Owner?.DisplayName ?? ""),
                },
            },
        };
        var descriptor1 = new Method<ApiRecruiting.ValidateOfferRequest, ApiRecruiting.ValidateOfferResponse>(
            MethodType.Unary, "acme.recruiting.v1" + ".OfferValidationService", "ValidateOffer",
            Wire.Marshaller<ApiRecruiting.ValidateOfferRequest>(), Wire.Marshaller<ApiRecruiting.ValidateOfferResponse>());
        using var rpc1 = _invoker.AsyncUnaryCall(descriptor1, null,
            new CallOptions(cancellationToken: token), input1);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiRecruiting.CheckJobOpeningEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            JobOpening = new ApiRecruiting.JobOpening
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.Offer?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Lines = { new ApiRecruiting.JobOpeningLine
                {
                    Tax = new ApiRecruiting.JobOpeningLineTax
                    {
                        Amount = (request.BenefitPlan ?? new ApiBenefits.BenefitPlan()).ActionCase switch
                        {
                            ApiBenefits.BenefitPlan.ActionOneofCase.Discount => (request.BenefitPlan?.Discount?.Amount ?? new ApiCommon.Money()),
                            _ => new ApiCommon.Money()
                        },
                    },
                } },
            },
        };
        using var rpc2 = _recruitingJobOpeningValidationService.CheckJobOpeningEligibilityAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiBenefits.DelegateBenefitPlanApprovalResponse
        {
            BenefitPlan = new ApiBenefits.BenefitPlan
            {
                BenefitPlanId = (request.BenefitPlanId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Interview?.Organizer?.DisplayName ?? ""),
                },
                Status = (request.BenefitPlan?.Status ?? (ApiBenefits.BenefitPlanStatus)0),
            },
            Outcome = new ApiBenefits.DelegateBenefitPlanApprovalResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "benefit_plan",
            "audit", "DelegateBenefitPlanApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Escalates a benefit plan.
    /// </summary>
    public override async Task<ApiBenefits.EscalateBenefitPlanResponse> EscalateBenefitPlan(ApiBenefits.EscalateBenefitPlanRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling EscalateBenefitPlan for {Tenant}", requestContext.TenantId);
        if (request.BenefitPlan is { } record)
        {
            var snapshot = BenefitPlanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.DelegateCorporateEventApprovalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Participants = { new ApiCorporateevents.CorporateEventParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Details?.Approver?.Email ?? ""),
                    },
                } },
            },
            Details = new ApiCorporateevents.DelegateCorporateEventApprovalRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    UserId = (request.BenefitPlan?.Owner?.UserId ?? ""),
                },
            },
        };
        using var rpc0 = _corporateeventsCorporateEventApprovalService.DelegateCorporateEventApprovalAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiDocuments.PushSignatureRequest
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
                    DisplayName = (request.BenefitPlan?.Owner?.DisplayName ?? ""),
                },
            },
        };
        using var rpc1 = _documentsSignatureSyncService.PushSignatureAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var input2 = new ApiBenefits.SummarizeEnrollmentsRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.BenefitPlan ?? new ApiBenefits.BenefitPlan()).ActionCase switch
            {
                ApiBenefits.BenefitPlan.ActionOneofCase.Block => (request.BenefitPlan?.Block?.Reason ?? ""),
                _ => ""
            },
        };
        using var rpc2 = _benefitsEnrollmentQueryService.SummarizeEnrollmentsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiBenefits.LintDependentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiBenefits.LintDependentsRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.BenefitPlan?.Owner?.Email ?? ""),
                },
            },
        };
        var reply3 = await SendEscalateBenefitPlan3Async(input3, token);
        var response = new ApiBenefits.EscalateBenefitPlanResponse
        {
            BenefitPlan = new ApiBenefits.BenefitPlan
            {
                BenefitPlanId = (request.BenefitPlanId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.Signature?.Subject?.DisplayName ?? ""),
                },
                Version = (request.BenefitPlan?.Version ?? 0L),
            },
            Outcome = new ApiBenefits.EscalateBenefitPlanResponse.Types.Outcome
            {
                EffectiveAt = (reply3.Outcome?.EffectiveAt ?? new Timestamp()),
                NextSteps = { (reply0.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply2.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                ReferenceIds = { (reply2.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "benefit_plan",
            "audit", "EscalateBenefitPlan", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a benefit plan.
    /// </summary>
    public override async Task<ApiBenefits.GetBenefitPlanApprovalStatusResponse> GetBenefitPlanApprovalStatus(ApiBenefits.GetBenefitPlanApprovalStatusRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.BenefitPlanId, "benefit_plan_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetBenefitPlanApprovalStatus for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "benefit_plan", request.BenefitPlanId,
            "GetBenefitPlanApprovalStatus", "accepted", token);
        var input0 = new ApiRecruiting.LintJobOpeningsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.LintJobOpeningsRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Locale = (request.Context?.Locale ?? ""),
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _recruitingJobOpeningValidationService.LintJobOpeningsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiBenefits.GetBenefitPlanApprovalStatusResponse
        {
            BenefitPlan = new ApiBenefits.BenefitPlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.JobOpening?.Audit?.Etag ?? ""),
                },
                BenefitPlanId = (request.BenefitPlanId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists benefit plans a page at a time.
    /// </summary>
    public override async Task<ApiBenefits.ListPendingBenefitPlansResponse> ListPendingBenefitPlans(ApiBenefits.ListPendingBenefitPlansRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ListPendingBenefitPlans for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "benefit_plan", "batch",
            "ListPendingBenefitPlans", "accepted", token);
        var input0 = new ApiBenefits.ValidateLifeEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiBenefits.ValidateLifeEventRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Locale = (request.Context?.Locale ?? ""),
            },
            LifeEvent = new ApiBenefits.LifeEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Measurements = { new ApiBenefits.LifeEventMeasurement
                {
                    ObservedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                } },
                Source = (request.Filter?.OwnerId ?? ""),
            },
        };
        using var rpc0 = _benefitsLifeEventValidationService.ValidateLifeEventAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiBenefits.ListPendingBenefitPlansResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "benefit_plan",
            "audit", "ListPendingBenefitPlans", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalls a benefit plan.
    /// </summary>
    public override async Task<ApiBenefits.RecallBenefitPlanResponse> RecallBenefitPlan(ApiBenefits.RecallBenefitPlanRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RecallBenefitPlan for {Tenant}", requestContext.TenantId);
        if (request.BenefitPlan is { } record)
        {
            var snapshot = BenefitPlanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiBenefits.LintLifeEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiBenefits.LintLifeEventsRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.BenefitPlan?.Owner?.Email ?? ""),
                },
            },
            LifeEvent = new ApiBenefits.LifeEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _benefitsLifeEventValidationService.LintLifeEventsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiBenefits.PreviewLifeEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LifeEvent = new ApiBenefits.LifeEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    Email = (reply0.LifeEvent?.Subject?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _benefitsLifeEventValidationService.PreviewLifeEventAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiRecruiting.CheckCandidateEligibilityRequest
        {
            Candidate = new ApiRecruiting.Candidate
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply1.LifeEvent?.Subject?.Email ?? ""),
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.LifeEvent?.Subject?.DisplayName ?? ""),
                    Email = (request.BenefitPlan?.Owner?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc2 = _recruitingCandidateValidationService.CheckCandidateEligibilityAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiBenefits.RecallBenefitPlanResponse
        {
            BenefitPlan = new ApiBenefits.BenefitPlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply2.Candidate?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                BenefitPlanId = (request.BenefitPlanId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply0.LifeEvent?.Subject?.Email ?? ""),
                },
                Status = (request.BenefitPlan?.Status ?? (ApiBenefits.BenefitPlanStatus)0),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.LifeEvent?.CorrelationId ?? ""),
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "benefit_plan",
            "audit", "RecallBenefitPlan", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Rejects a benefit plan.
    /// </summary>
    public override async Task<ApiBenefits.RejectBenefitPlanResponse> RejectBenefitPlan(ApiBenefits.RejectBenefitPlanRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RejectBenefitPlan for {Tenant}", requestContext.TenantId);
        if (request.BenefitPlan is { } record)
        {
            var snapshot = BenefitPlanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.CloneJobOpeningRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            JobOpening = new ApiRecruiting.JobOpening
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.BenefitPlan?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (request.BenefitPlan?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Counterparty = new ApiRecruiting.InterviewRef
                {
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                },
                Lines = { new ApiRecruiting.JobOpeningLine
                {
                    Tax = new ApiRecruiting.JobOpeningLineTax
                    {
                        Amount = (request.BenefitPlan ?? new ApiBenefits.BenefitPlan()).ActionCase switch
                        {
                            ApiBenefits.BenefitPlan.ActionOneofCase.Discount => (request.BenefitPlan?.Discount?.Amount ?? new ApiCommon.Money()),
                            _ => new ApiCommon.Money()
                        },
                    },
                } },
            },
        };
        using var rpc0 = _recruitingJobOpeningService.CloneJobOpeningAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiBenefits.RejectBenefitPlanResponse
        {
            BenefitPlan = new ApiBenefits.BenefitPlan
            {
                BenefitPlanId = (request.BenefitPlanId ?? ""),
                Version = (request.BenefitPlan?.Version ?? 0L),
            },
            Outcome = new ApiBenefits.RejectBenefitPlanResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "benefit_plan",
            "audit", "RejectBenefitPlan", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Submits a benefit plan.
    /// </summary>
    public override async Task<ApiBenefits.SubmitBenefitPlanResponse> SubmitBenefitPlan(ApiBenefits.SubmitBenefitPlanRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SubmitBenefitPlan for {Tenant}", requestContext.TenantId);
        if (request.BenefitPlan is { } record)
        {
            var snapshot = BenefitPlanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiWorkplaces.StartRoomBookingWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiWorkplaces.StartRoomBookingWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.BenefitPlan?.Owner?.Email ?? ""),
                },
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _workplacesRoomBookingWorkflowService.StartRoomBookingWorkflowAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiWorkplaces.SignalRoomBookingWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                Participants = { new ApiWorkplaces.RoomBookingParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (reply0.RoomBooking?.Organizer?.Email ?? ""),
                    },
                } },
            },
        };
        var reply1 = await Workflow.SendAsync(input1, token, _workplacesRoomBookingWorkflowService.SignalRoomBookingWorkflowAsync);
        var input2 = new ApiDocuments.ScoreSignatureRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ScoreSignatureRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (reply1.RoomBooking?.Location?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Locality = (reply0.RoomBooking?.Location?.Locality ?? ""),
                    Region = (reply1.RoomBooking?.Location?.Region ?? ""),
                },
            },
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
        };
        using var rpc2 = _documentsSignatureValidationService.ScoreSignatureAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiRecruiting.SuggestCandidatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiRecruiting.SuggestCandidatesRequest.Types.Filter
            {
                Region = (reply1.RoomBooking?.Location?.Region ?? ""),
            },
        };
        var named3 = _registry.Get<ApiRecruiting.CandidateQueryService.CandidateQueryServiceClient>("acme.recruiting.v1.CandidateQueryService");
        using var rpc3 = named3.SuggestCandidatesAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiBenefits.SubmitBenefitPlanResponse
        {
            BenefitPlan = new ApiBenefits.BenefitPlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.RoomBooking?.Audit?.UpdatedAt ?? new Timestamp()),
                    UpdatedBy = (reply1.RoomBooking?.Audit?.UpdatedBy ?? ""),
                },
                BenefitPlanId = (request.BenefitPlanId ?? ""),
                Priority = (request.BenefitPlan?.Priority ?? 0),
            },
            Outcome = new ApiBenefits.SubmitBenefitPlanResponse.Types.Outcome
            {
                EffectiveAt = (reply0.Outcome?.EffectiveAt ?? new Timestamp()),
                Message = (reply2.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Signature?.CorrelationId ?? ""),
                Status = (reply3.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "benefit_plan",
            "audit", "SubmitBenefitPlan", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiBenefits.LintDependentsResponse> SendEscalateBenefitPlan3Async(ApiBenefits.LintDependentsRequest request, CancellationToken token)
    {
        return await _benefitPlanApprovalEndpointSendEscalateBenefitPlan3Port.SendAsync(request, token);
    }
}
