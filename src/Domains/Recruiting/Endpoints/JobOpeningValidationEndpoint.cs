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
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiRouting = Acme.Routing.V1;
using ApiSecrets = Acme.Secrets.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Recruiting;

/// <summary>
/// Checks job openings against the rules before anything acts on them.
/// </summary>
public sealed class JobOpeningValidationEndpoint : ApiRecruiting.JobOpeningValidationService.JobOpeningValidationServiceBase
{
    private readonly ApiBenefits.LifeEventValidationService.LifeEventValidationServiceClient _benefitsLifeEventValidationService;
    private readonly ApiDocuments.DocumentTemplateHistoryService.DocumentTemplateHistoryServiceClient _documentsDocumentTemplateHistoryService;
    private readonly ApiDocuments.SignatureSyncService.SignatureSyncServiceClient _documentsSignatureSyncService;
    private readonly CallInvoker _invoker;
    private readonly IJobOpeningValidationEndpointSendLintJobOpenings1Port _jobOpeningValidationEndpointSendLintJobOpenings1Port;
    private readonly ILogger<JobOpeningValidationEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly ApiRecruiting.CandidateCalculationService.CandidateCalculationServiceClient _recruitingCandidateCalculationService;
    private readonly ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient _recruitingJobOpeningCalculationService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiRouting.RouteCalculationService.RouteCalculationServiceClient _routingRouteCalculationService;
    private readonly ApiSecrets.SecretService.SecretServiceClient _secretsSecretService;
    private readonly Func<ApiRecruiting.CompareJobOpeningOptionsRequest, CallOptions, AsyncUnaryCall<ApiRecruiting.CompareJobOpeningOptionsResponse>> _sendCheckJobOpeningEligibility2;
    private readonly Func<ApiRecruiting.CompareJobOpeningOptionsRequest, CallOptions, AsyncUnaryCall<ApiRecruiting.CompareJobOpeningOptionsResponse>> _sendValidateJobOpening0;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiVenues.VenueService.VenueServiceClient _venuesVenueService;

    public JobOpeningValidationEndpoint(
        ApiBenefits.LifeEventValidationService.LifeEventValidationServiceClient benefitsLifeEventValidationService,
        ApiDocuments.DocumentTemplateHistoryService.DocumentTemplateHistoryServiceClient documentsDocumentTemplateHistoryService,
        ApiDocuments.SignatureSyncService.SignatureSyncServiceClient documentsSignatureSyncService,
        CallInvoker invoker,
        IJobOpeningValidationEndpointSendLintJobOpenings1Port jobOpeningValidationEndpointSendLintJobOpenings1PortAdapter,
        ILogger<JobOpeningValidationEndpoint> logger,
        RetryPolicy policy,
        ApiRecruiting.CandidateCalculationService.CandidateCalculationServiceClient recruitingCandidateCalculationService,
        ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient recruitingJobOpeningCalculationService,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiRouting.RouteCalculationService.RouteCalculationServiceClient routingRouteCalculationService,
        ApiSecrets.SecretService.SecretServiceClient secretsSecretService,
        IOptions<MeshOptions> settings,
        ApiVenues.VenueService.VenueServiceClient venuesVenueService)
    {
        _benefitsLifeEventValidationService = benefitsLifeEventValidationService;
        _documentsDocumentTemplateHistoryService = documentsDocumentTemplateHistoryService;
        _documentsSignatureSyncService = documentsSignatureSyncService;
        _invoker = invoker;
        _jobOpeningValidationEndpointSendLintJobOpenings1Port = jobOpeningValidationEndpointSendLintJobOpenings1PortAdapter;
        _logger = logger;
        _policy = policy;
        _recruitingCandidateCalculationService = recruitingCandidateCalculationService;
        _recruitingJobOpeningCalculationService = recruitingJobOpeningCalculationService;
        _registry = registry;
        _repository = repository;
        _routingRouteCalculationService = routingRouteCalculationService;
        _secretsSecretService = secretsSecretService;
        _settings = settings;
        _venuesVenueService = venuesVenueService;
        _sendCheckJobOpeningEligibility2 = _recruitingJobOpeningCalculationService.CompareJobOpeningOptionsAsync;
        _sendValidateJobOpening0 = _recruitingJobOpeningCalculationService.CompareJobOpeningOptionsAsync;
    }
    /// <summary>
    /// Checks a job opening.
    /// </summary>
    public override async Task<ApiRecruiting.CheckJobOpeningEligibilityResponse> CheckJobOpeningEligibility(ApiRecruiting.CheckJobOpeningEligibilityRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CheckJobOpeningEligibility for {Tenant}", requestContext.TenantId);
        if (request.JobOpening is { } record)
        {
            var snapshot = JobOpeningMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.ListDocumentTemplateVersionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListDocumentTemplateVersionsRequest.Types.Filter
            {
                CreatedAfter = (request.JobOpening?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (request.JobOpening?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.JobOpening?.Counterparty?.InterviewId ?? ""),
                Region = (request.JobOpening?.BillingAddress?.Region ?? ""),
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _documentsDocumentTemplateHistoryService.ListDocumentTemplateVersionsAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiRecruiting.ExplainCandidateCalculationRequest
        {
            Candidate = new ApiRecruiting.Candidate
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (request.JobOpening?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    DisplayName = (request.JobOpening?.Counterparty?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply1 = await Workflow.SendAsync(input1, token, _recruitingCandidateCalculationService.ExplainCandidateCalculationAsync);
        var input2 = new ApiRecruiting.CompareJobOpeningOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            JobOpening = new ApiRecruiting.JobOpening
            {
                Counterparty = new ApiRecruiting.InterviewRef
                {
                    DisplayName = (reply1.Candidate?.RelationshipManager?.DisplayName ?? ""),
                },
            },
        };
        using var rpc2 = _sendCheckJobOpeningEligibility2(input2, new CallOptions(cancellationToken: token));
        var reply2 = await rpc2.ResponseAsync;
        var reply3 = new ApiRecruiting.RecalculateJobOpeningsResponse();
        foreach (var item in (request.JobOpening?.Attachments ?? new Google.Protobuf.Collections.RepeatedField<ApiRecruiting.JobOpeningAttachment>()))
        {
            var input3 = new ApiRecruiting.RecalculateJobOpeningsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                JobOpening = new ApiRecruiting.JobOpening
                {
                    Attachments = { new ApiRecruiting.JobOpeningAttachment
                    {
                        Checksum = (item.Checksum ?? ""),
                        FileName = (item.FileName ?? ""),
                        SizeBytes = item.SizeBytes,
                    } },
                },
            };
            using var rpc3 = _recruitingJobOpeningCalculationService.RecalculateJobOpeningsAsync(input3, cancellationToken: token);
            reply3 = await rpc3.ResponseAsync;
        }
        var response = new ApiRecruiting.CheckJobOpeningEligibilityResponse
        {
            JobOpening = new ApiRecruiting.JobOpening
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply1.Candidate?.Audit?.UpdatedBy ?? ""),
                },
                BillingAddress = (reply2.JobOpening?.BillingAddress ?? new ApiCommon.PostalAddress()),
                DueAt = (request.JobOpening?.DueAt ?? new Timestamp()),
                JobOpeningId = (request.JobOpeningId ?? ""),
            },
            Outcome = new ApiRecruiting.CheckJobOpeningEligibilityResponse.Types.Outcome
            {
                EffectiveAt = (reply1.Outcome?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "job_opening",
            "audit", "CheckJobOpeningEligibility", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a job opening.
    /// </summary>
    public override async Task<ApiRecruiting.ExplainJobOpeningDecisionResponse> ExplainJobOpeningDecision(ApiRecruiting.ExplainJobOpeningDecisionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ExplainJobOpeningDecision for {Tenant}", requestContext.TenantId);
        if (request.JobOpening is { } record)
        {
            var snapshot = JobOpeningMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.CalculateJobOpeningRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.CalculateJobOpeningRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Recipient = (request.JobOpening?.BillingAddress?.Recipient ?? ""),
                },
            },
            JobOpening = new ApiRecruiting.JobOpening
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (request.JobOpening?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
        };
        var descriptor0 = new Method<ApiRecruiting.CalculateJobOpeningRequest, ApiRecruiting.CalculateJobOpeningResponse>(
            MethodType.Unary, "acme.recruiting.v1" + ".JobOpeningCalculationService", "CalculateJobOpening",
            Wire.Marshaller<ApiRecruiting.CalculateJobOpeningRequest>(), Wire.Marshaller<ApiRecruiting.CalculateJobOpeningResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.ListSignatureConflictsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListSignatureConflictsRequest.Types.Filter
            {
                CreatedAfter = (request.JobOpening?.Counterparty?.StartsAt ?? new Timestamp()),
                Labels = { (reply0.JobOpening?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.JobOpening?.Counterparty?.InterviewId ?? ""),
                Region = (request.JobOpening?.BillingAddress?.Region ?? ""),
            },
        };
        var command1 = new DispatchCommand<ApiDocuments.ListSignatureConflictsRequest>(input1,
            "primary", requestContext.TenantId);
        var routed1 = command1 with { Region = _settings.Value.Region };
        using var rpc1 = _documentsSignatureSyncService.ListSignatureConflictsAsync(routed1.Payload, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var reply2 = new ApiBenefits.ScoreLifeEventResponse();
        foreach (var item in (request.JobOpening?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiRecruiting.JobOpeningLine>()))
        {
            var input2 = new ApiBenefits.ScoreLifeEventRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiBenefits.ScoreLifeEventRequest.Types.Details
                {
                    Destination = new ApiCommon.PostalAddress
                    {
                        Recipient = (request.JobOpening?.BillingAddress?.Recipient ?? ""),
                    },
                },
                LifeEvent = new ApiBenefits.LifeEvent
                {
                    Attributes = { (item.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                    Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                },
            };
            using var rpc2 = _benefitsLifeEventValidationService.ScoreLifeEventAsync(input2, cancellationToken: token);
            reply2 = await rpc2.ResponseAsync;
        }
        var input3 = new ApiSecrets.SearchSecretsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiSecrets.SearchSecretsRequest.Types.Filter
            {
                CreatedAfter = (request.JobOpening?.Audit?.UpdatedAt ?? new Timestamp()),
                OwnerId = (reply0.JobOpening?.Counterparty?.InterviewId ?? ""),
                Region = (reply0.JobOpening?.BillingAddress?.Region ?? ""),
            },
        };
        input3 = PropertyMap.Copy(input3);
        using var rpc3 = _secretsSecretService.SearchSecretsAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiRecruiting.ExplainJobOpeningDecisionResponse
        {
            JobOpening = new ApiRecruiting.JobOpening
            {
                JobOpeningId = (request.JobOpeningId ?? ""),
                Notes = (request.JobOpening?.Notes ?? ""),
                Total = (reply0.Breakdown?.Total ?? new ApiCommon.Money()),
            },
            Outcome = new ApiRecruiting.ExplainJobOpeningDecisionResponse.Types.Outcome
            {
                Message = (reply1.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply3.Result?.Message ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "job_opening",
            "audit", "ExplainJobOpeningDecision", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Lints a job opening.
    /// </summary>
    public override async Task<ApiRecruiting.LintJobOpeningsResponse> LintJobOpenings(ApiRecruiting.LintJobOpeningsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling LintJobOpenings for {Tenant}", requestContext.TenantId);
        if (request.JobOpening is { } record)
        {
            var snapshot = JobOpeningMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var batch0 = (request.JobOpening?.Attachments ?? new Google.Protobuf.Collections.RepeatedField<ApiRecruiting.JobOpeningAttachment>()).Select(async item =>
        {
            var input0 = new ApiRecruiting.CalculateJobOpeningRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                JobOpening = new ApiRecruiting.JobOpening
                {
                    Attachments = { new ApiRecruiting.JobOpeningAttachment
                    {
                        SizeBytes = item.SizeBytes,
                        Uri = (item.Uri ?? ""),
                    } },
                },
            };
            using var rpc0 = _recruitingJobOpeningCalculationService.CalculateJobOpeningAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            return reply0;
        }).ToArray();
        var replies0 = await Task.WhenAll(batch0);
        var reply0 = replies0.LastOrDefault() ?? new ApiRecruiting.CalculateJobOpeningResponse();
        var input1 = new ApiRecruiting.CompareJobOpeningOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            JobOpening = new ApiRecruiting.JobOpening
            {
                Counterparty = new ApiRecruiting.InterviewRef
                {
                    DisplayName = (request.JobOpening?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        var reply1 = await SendLintJobOpenings1Async(input1, token);
        var input2 = new ApiRecruiting.SimulateJobOpeningRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            JobOpening = new ApiRecruiting.JobOpening
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.JobOpening?.Audit?.CreatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (reply0.JobOpening?.BillingAddress?.Locality ?? ""),
                    Region = (request.JobOpening?.BillingAddress?.Region ?? ""),
                },
                Card = new ApiRecruiting.JobOpeningCardSettlement
                {
                    HolderName = (reply1.JobOpening ?? new ApiRecruiting.JobOpening()).PaymentCase switch
                    {
                        ApiRecruiting.JobOpening.SettlementOneofCase.Card => (reply1.JobOpening?.Card?.HolderName ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc2 = _recruitingJobOpeningCalculationService.SimulateJobOpeningAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var batch3 = (request.JobOpening?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiRecruiting.JobOpeningLine>()).Select(async item =>
        {
            var input3 = new ApiRecruiting.CalculateCandidateRequest
            {
                Candidate = new ApiRecruiting.Candidate
                {
                    Attributes = { (item.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                    PrimaryAddress = new ApiCommon.PostalAddress
                    {
                        Lines = { (request.JobOpening?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    },
                },
                Context = (request.Context ?? new ApiCommon.RequestContext()),
            };
            using var rpc3 = _recruitingCandidateCalculationService.CalculateCandidateAsync(input3, cancellationToken: token);
            var reply3 = await rpc3.ResponseAsync;
            return reply3;
        }).ToArray();
        var replies3 = await Task.WhenAll(batch3);
        var reply3 = replies3.LastOrDefault() ?? new ApiRecruiting.CalculateCandidateResponse();
        var response = new ApiRecruiting.LintJobOpeningsResponse
        {
            JobOpening = new ApiRecruiting.JobOpening
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.JobOpening?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                JobOpeningId = (request.JobOpeningId ?? ""),
                Status = (request.JobOpening?.Status ?? (ApiRecruiting.JobOpeningStatus)0),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply2.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "job_opening",
            "audit", "LintJobOpenings", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Normalizes a job opening.
    /// </summary>
    public override async Task<ApiRecruiting.NormalizeJobOpeningResponse> NormalizeJobOpening(ApiRecruiting.NormalizeJobOpeningRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling NormalizeJobOpening for {Tenant}", requestContext.TenantId);
        if (request.JobOpening is { } record)
        {
            var snapshot = JobOpeningMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.ExplainJobOpeningCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            JobOpening = new ApiRecruiting.JobOpening
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.JobOpening?.Audit?.CreatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.JobOpening?.BillingAddress?.CountryCode ?? ""),
                    Lines = { (request.JobOpening?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
                Counterparty = new ApiRecruiting.InterviewRef
                {
                    InterviewId = (request.JobOpening?.Counterparty?.InterviewId ?? ""),
                },
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiRecruiting.ExplainJobOpeningCalculationRequest>(json0);
        using var rpc0 = _recruitingJobOpeningCalculationService.ExplainJobOpeningCalculationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.CompareJobOpeningOptionsRequest();
        input1.Context = (request.Context ?? new ApiCommon.RequestContext());
        input1.JobOpening = new ApiRecruiting.JobOpening
        {
            Counterparty = new ApiRecruiting.InterviewRef
            {
                StartsAt = (request.JobOpening?.Counterparty?.StartsAt ?? new Timestamp()),
            },
        };
        using var rpc1 = _recruitingJobOpeningCalculationService.CompareJobOpeningOptionsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var batch2 = (request.JobOpening?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiRecruiting.JobOpeningLine>()).Select(async item =>
        {
            var input2 = new ApiRecruiting.RecalculateCandidatesRequest
            {
                Candidate = new ApiRecruiting.Candidate
                {
                    Attributes = { (item.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                    PrimaryAddress = new ApiCommon.PostalAddress
                    {
                        Lines = { (reply0.JobOpening?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    },
                },
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiRecruiting.RecalculateCandidatesRequest.Types.Details
                {
                    Destination = new ApiCommon.PostalAddress
                    {
                        PostalCode = (request.JobOpening?.BillingAddress?.PostalCode ?? ""),
                        Recipient = (reply1.JobOpening?.BillingAddress?.Recipient ?? ""),
                    },
                },
            };
            using var rpc2 = _recruitingCandidateCalculationService.RecalculateCandidatesAsync(input2, cancellationToken: token);
            var reply2 = await rpc2.ResponseAsync;
            return reply2;
        }).ToArray();
        var replies2 = await Task.WhenAll(batch2);
        var reply2 = replies2.LastOrDefault() ?? new ApiRecruiting.RecalculateCandidatesResponse();
        var response = new ApiRecruiting.NormalizeJobOpeningResponse
        {
            JobOpening = new ApiRecruiting.JobOpening
            {
                JobOpeningId = (request.JobOpeningId ?? ""),
                Status = (request.JobOpening?.Status ?? (ApiRecruiting.JobOpeningStatus)0),
            },
            Outcome = new ApiRecruiting.NormalizeJobOpeningResponse.Types.Outcome
            {
                EffectiveAt = (reply0.Outcome?.EffectiveAt ?? new Timestamp()),
                NextSteps = { (reply0.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "job_opening",
            "audit", "NormalizeJobOpening", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Previews a job opening.
    /// </summary>
    public override async Task<ApiRecruiting.PreviewJobOpeningResponse> PreviewJobOpening(ApiRecruiting.PreviewJobOpeningRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling PreviewJobOpening for {Tenant}", requestContext.TenantId);
        if (request.JobOpening is { } record)
        {
            var snapshot = JobOpeningMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRouting.SimulateRouteRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRouting.SimulateRouteRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Locality = (request.JobOpening?.BillingAddress?.Locality ?? ""),
                },
            },
            Route = new ApiRouting.Route
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.JobOpening?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _routingRouteCalculationService.SimulateRouteAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiRecruiting.PreviewJobOpeningResponse
        {
            JobOpening = new ApiRecruiting.JobOpening
            {
                Counterparty = (request.JobOpening?.Counterparty ?? new ApiRecruiting.InterviewRef()),
                JobOpeningId = (request.JobOpeningId ?? ""),
                Number = (reply0.Route?.Number ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "job_opening",
            "audit", "PreviewJobOpening", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Scores a job opening.
    /// </summary>
    public override async Task<ApiRecruiting.ScoreJobOpeningResponse> ScoreJobOpening(ApiRecruiting.ScoreJobOpeningRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ScoreJobOpening for {Tenant}", requestContext.TenantId);
        if (request.JobOpening is { } record)
        {
            var snapshot = JobOpeningMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.RecalculateJobOpeningsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.RecalculateJobOpeningsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.JobOpening?.BillingAddress?.CountryCode ?? ""),
                },
            },
            JobOpening = new ApiRecruiting.JobOpening
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (request.Details?.Destination?.Recipient ?? ""),
                },
                Counterparty = new ApiRecruiting.InterviewRef
                {
                    DisplayName = (request.JobOpening?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _recruitingJobOpeningCalculationService.RecalculateJobOpeningsAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiDocuments.SyncSignaturesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.SyncSignaturesRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiDocuments.DocumentRef
                {
                    DisplayName = (reply0.JobOpening?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        using var rpc1 = _documentsSignatureSyncService.SyncSignaturesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.ScoreJobOpeningResponse
        {
            Breakdown = new ApiRecruiting.ScoreJobOpeningResponse.Types.Breakdown
            {
                Total = (reply0.Breakdown?.Total ?? new ApiCommon.Money()),
            },
            JobOpening = new ApiRecruiting.JobOpening
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.JobOpening?.Audit?.CreatedBy ?? ""),
                },
                JobOpeningId = (request.JobOpeningId ?? ""),
                Metadata = { (request.JobOpening?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply1.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "job_opening",
            "audit", "ScoreJobOpening", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Validates a job opening.
    /// </summary>
    public override async Task<ApiRecruiting.ValidateJobOpeningResponse> ValidateJobOpening(ApiRecruiting.ValidateJobOpeningRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ValidateJobOpening for {Tenant}", requestContext.TenantId);
        if (request.JobOpening is { } record)
        {
            var snapshot = JobOpeningMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.CompareJobOpeningOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.CompareJobOpeningOptionsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.JobOpening?.BillingAddress?.Region ?? ""),
                },
            },
            JobOpening = new ApiRecruiting.JobOpening
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.JobOpening?.Audit?.Etag ?? ""),
                },
            },
        };
        using var rpc0 = _sendValidateJobOpening0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiVenues.UpdateVenueRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Venue = new ApiVenues.Venue
            {
                Address = new ApiCommon.PostalAddress
                {
                    Recipient = (reply0.JobOpening?.BillingAddress?.Recipient ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.JobOpening?.Audit?.CreatedAt ?? new Timestamp()),
                    CreatedBy = (request.JobOpening?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (request.JobOpening?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        var named1 = _registry.Get<ApiVenues.VenueService.VenueServiceClient>("acme.venues.v1.VenueService");
        using var rpc1 = named1.UpdateVenueAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.ValidateJobOpeningResponse
        {
            JobOpening = new ApiRecruiting.JobOpening
            {
                JobOpeningId = (request.JobOpeningId ?? ""),
                Number = (request.JobOpening?.Number ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "job_opening",
            "audit", "ValidateJobOpening", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiRecruiting.CompareJobOpeningOptionsResponse> SendLintJobOpenings1Async(ApiRecruiting.CompareJobOpeningOptionsRequest request, CancellationToken token)
    {
        return await _jobOpeningValidationEndpointSendLintJobOpenings1Port.SendAsync(request, token);
    }
}
