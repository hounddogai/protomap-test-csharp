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
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiShows = Acme.Shows.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Recruiting;

/// <summary>
/// Computes the figures of a candidate, such as totals, taxes, and estimates.
/// </summary>
public sealed class CandidateCalculationEndpoint : ApiRecruiting.CandidateCalculationService.CandidateCalculationServiceBase
{
    private readonly NotificationBus _bus;
    private readonly ICandidateCalculationEndpointSendCalculateCandidate0Port _candidateCalculationEndpointSendCalculateCandidate0Port;
    private readonly ICandidateCalculationEndpointSendQuoteCandidate1Port _candidateCalculationEndpointSendQuoteCandidate1Port;
    private readonly ApiDocuments.DocumentFolderHistoryService.DocumentFolderHistoryServiceClient _documentsDocumentFolderHistoryService;
    private readonly ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient _documentsDocumentTemplateSyncService;
    private readonly ApiHelpdesk.TicketService.TicketServiceClient _helpdeskTicketService;
    private readonly ApiLedger.LedgerAccountService.LedgerAccountServiceClient _ledgerLedgerAccountService;
    private readonly ApiLicensing.SoftwareLicenseValidationService.SoftwareLicenseValidationServiceClient _licensingSoftwareLicenseValidationService;
    private readonly ILogger<CandidateCalculationEndpoint> _logger;
    private readonly ApiPerformance.ReviewService.ReviewServiceClient _performanceReviewService;
    private readonly RetryPolicy _policy;
    private readonly ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient _recruitingJobOpeningCalculationService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiRecruiting.RecalculateJobOpeningsRequest, CallOptions, AsyncUnaryCall<ApiRecruiting.RecalculateJobOpeningsResponse>> _sendEstimateCandidate3;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiShows.ShowService.ShowServiceClient _showsShowService;
    private readonly ApiTravel.TripValidationService.TripValidationServiceClient _travelTripValidationService;

    public CandidateCalculationEndpoint(
        NotificationBus bus,
        ICandidateCalculationEndpointSendCalculateCandidate0Port candidateCalculationEndpointSendCalculateCandidate0PortAdapter,
        ICandidateCalculationEndpointSendQuoteCandidate1Port candidateCalculationEndpointSendQuoteCandidate1PortAdapter,
        ApiDocuments.DocumentFolderHistoryService.DocumentFolderHistoryServiceClient documentsDocumentFolderHistoryService,
        ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient documentsDocumentTemplateSyncService,
        ApiHelpdesk.TicketService.TicketServiceClient helpdeskTicketService,
        ApiLedger.LedgerAccountService.LedgerAccountServiceClient ledgerLedgerAccountService,
        ApiLicensing.SoftwareLicenseValidationService.SoftwareLicenseValidationServiceClient licensingSoftwareLicenseValidationService,
        ILogger<CandidateCalculationEndpoint> logger,
        ApiPerformance.ReviewService.ReviewServiceClient performanceReviewService,
        RetryPolicy policy,
        ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient recruitingJobOpeningCalculationService,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiShows.ShowService.ShowServiceClient showsShowService,
        ApiTravel.TripValidationService.TripValidationServiceClient travelTripValidationService)
    {
        _bus = bus;
        _candidateCalculationEndpointSendCalculateCandidate0Port = candidateCalculationEndpointSendCalculateCandidate0PortAdapter;
        _candidateCalculationEndpointSendQuoteCandidate1Port = candidateCalculationEndpointSendQuoteCandidate1PortAdapter;
        _documentsDocumentFolderHistoryService = documentsDocumentFolderHistoryService;
        _documentsDocumentTemplateSyncService = documentsDocumentTemplateSyncService;
        _helpdeskTicketService = helpdeskTicketService;
        _ledgerLedgerAccountService = ledgerLedgerAccountService;
        _licensingSoftwareLicenseValidationService = licensingSoftwareLicenseValidationService;
        _logger = logger;
        _performanceReviewService = performanceReviewService;
        _policy = policy;
        _recruitingJobOpeningCalculationService = recruitingJobOpeningCalculationService;
        _registry = registry;
        _repository = repository;
        _settings = settings;
        _showsShowService = showsShowService;
        _travelTripValidationService = travelTripValidationService;
        _sendEstimateCandidate3 = _recruitingJobOpeningCalculationService.RecalculateJobOpeningsAsync;
    }
    /// <summary>
    /// Calculates a candidate.
    /// </summary>
    public override async Task<ApiRecruiting.CalculateCandidateResponse> CalculateCandidate(ApiRecruiting.CalculateCandidateRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CalculateCandidate for {Tenant}", requestContext.TenantId);
        if (request.Candidate is { } record)
        {
            var snapshot = CandidateMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.RecalculateJobOpeningsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.RecalculateJobOpeningsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Candidate?.PrimaryAddress?.CountryCode ?? ""),
                    Region = (request.Details?.Destination?.Region ?? ""),
                },
            },
            JobOpening = new ApiRecruiting.JobOpening
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Candidate?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (request.Details?.Destination?.Recipient ?? ""),
                },
            },
        };
        var reply0 = await SendCalculateCandidate0Async(input0, token);
        var input1 = new ApiRecruiting.QuoteJobOpeningRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.QuoteJobOpeningRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Locality = (request.Details?.Destination?.Locality ?? ""),
                    Recipient = (reply0.JobOpening?.BillingAddress?.Recipient ?? ""),
                },
            },
            JobOpening = new ApiRecruiting.JobOpening
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Candidate?.Audit?.Etag ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Candidate?.PrimaryAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
        };
        var labels1 = input1.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input1.Context.Baggage.Clear();
        input1.Context.Baggage.Add(labels1);
        using var rpc1 = _recruitingJobOpeningCalculationService.QuoteJobOpeningAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.CalculateCandidateResponse
        {
            Candidate = new ApiRecruiting.Candidate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.JobOpening?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedBy = (reply1.JobOpening?.Audit?.UpdatedBy ?? ""),
                },
                CandidateId = (request.CandidateId ?? ""),
                PrimaryAddress = (request.Candidate?.PrimaryAddress ?? new ApiCommon.PostalAddress()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "candidate",
            "audit", "CalculateCandidate", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Compares a candidate.
    /// </summary>
    public override async Task<ApiRecruiting.CompareCandidateOptionsResponse> CompareCandidateOptions(ApiRecruiting.CompareCandidateOptionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CompareCandidateOptions for {Tenant}", requestContext.TenantId);
        if (request.Candidate is { } record)
        {
            var snapshot = CandidateMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.QuoteJobOpeningRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            JobOpening = new ApiRecruiting.JobOpening
            {
                Counterparty = new ApiRecruiting.InterviewRef
                {
                    DisplayName = (request.Candidate?.RelationshipManager?.DisplayName ?? ""),
                },
            },
        };
        using var rpc0 = _recruitingJobOpeningCalculationService.QuoteJobOpeningAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiPerformance.CreateReviewRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Review = new ApiPerformance.Review
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (request.Candidate?.PrimaryAddress?.Locality ?? ""),
                    Recipient = (request.Candidate?.PrimaryAddress?.Recipient ?? ""),
                },
            },
        };
        using var rpc1 = _performanceReviewService.CreateReviewAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var input2 = new ApiDocuments.ListDocumentTemplateConflictsRequest();
        input2.Context = (request.Context ?? new ApiCommon.RequestContext());
        input2.Filter = new ApiDocuments.ListDocumentTemplateConflictsRequest.Types.Filter
        {
            CreatedAfter = (reply0.JobOpening?.Audit?.CreatedAt ?? new Timestamp()),
            OwnerId = (reply0.JobOpening?.JobOpeningId ?? ""),
            Region = (reply0.JobOpening?.BillingAddress?.Region ?? ""),
        };
        using var rpc2 = _documentsDocumentTemplateSyncService.ListDocumentTemplateConflictsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiRecruiting.CalculateJobOpeningRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            JobOpening = new ApiRecruiting.JobOpening
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (request.Details?.Destination?.Locality ?? ""),
                    Region = (reply0.JobOpening?.BillingAddress?.Region ?? ""),
                },
            },
        };
        using var rpc3 = _recruitingJobOpeningCalculationService.CalculateJobOpeningAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiRecruiting.CompareCandidateOptionsResponse
        {
            Breakdown = new ApiRecruiting.CompareCandidateOptionsResponse.Types.Breakdown
            {
                CurrencyCode = (request.Details?.CurrencyCode ?? ""),
                Total = (reply3.Breakdown?.Total ?? new ApiCommon.Money()),
            },
            Candidate = new ApiRecruiting.Candidate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.JobOpening?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                CandidateId = (request.CandidateId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "candidate",
            "audit", "CompareCandidateOptions", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Estimates a candidate.
    /// </summary>
    public override async Task<ApiRecruiting.EstimateCandidateResponse> EstimateCandidate(ApiRecruiting.EstimateCandidateRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling EstimateCandidate for {Tenant}", requestContext.TenantId);
        if (request.Candidate is { } record)
        {
            var snapshot = CandidateMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.NormalizeSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Holder = new ApiLicensing.LicenseAssignmentRef
                {
                    Number = (request.Candidate ?? new ApiRecruiting.Candidate()).IdentityDocumentCase switch
                    {
                        ApiRecruiting.Candidate.IdentityDocumentOneofCase.Passport => (request.Candidate?.Passport?.Number ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc0 = _licensingSoftwareLicenseValidationService.NormalizeSoftwareLicenseAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.EstimateJobOpeningRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.EstimateJobOpeningRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Details?.Destination?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
            JobOpening = new ApiRecruiting.JobOpening
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.SoftwareLicense?.Audit?.Etag ?? ""),
                },
                Counterparty = new ApiRecruiting.InterviewRef
                {
                    DisplayName = (reply0.SoftwareLicense?.Holder?.DisplayName ?? ""),
                },
            },
        };
        var reply1 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _recruitingJobOpeningCalculationService.EstimateJobOpeningAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input2 = new ApiShows.BatchGetShowsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply2 = await Workflow.SendAsync(input2, token, _showsShowService.BatchGetShowsAsync);
        var input3 = new ApiRecruiting.RecalculateJobOpeningsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.RecalculateJobOpeningsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Details?.Destination?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    PostalCode = (request.Details?.Destination?.PostalCode ?? ""),
                },
            },
            JobOpening = new ApiRecruiting.JobOpening
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (request.Details?.Destination?.Locality ?? ""),
                    PostalCode = (request.Candidate?.PrimaryAddress?.PostalCode ?? ""),
                },
            },
        };
        using var rpc3 = _sendEstimateCandidate3(input3, new CallOptions(cancellationToken: token));
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiRecruiting.EstimateCandidateResponse
        {
            Candidate = new ApiRecruiting.Candidate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply3.JobOpening?.Audit?.Etag ?? ""),
                },
                CandidateId = (request.CandidateId ?? ""),
                RelationshipManager = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.SoftwareLicense?.Holder?.DisplayName ?? ""),
                    Email = (reply0.SoftwareLicense?.Contact?.Email ?? ""),
                },
                Status = (request.Candidate?.Status ?? (ApiRecruiting.CandidateStatus)0),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply2.Result?.Message ?? ""),
                ReferenceIds = { (reply2.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "candidate",
            "audit", "EstimateCandidate", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a candidate.
    /// </summary>
    public override async Task<ApiRecruiting.ExplainCandidateCalculationResponse> ExplainCandidateCalculation(ApiRecruiting.ExplainCandidateCalculationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ExplainCandidateCalculation for {Tenant}", requestContext.TenantId);
        if (request.Candidate is { } record)
        {
            var snapshot = CandidateMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.RecalculateJobOpeningsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            JobOpening = new ApiRecruiting.JobOpening
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.Candidate?.Audit?.UpdatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Candidate?.PrimaryAddress?.CountryCode ?? ""),
                    Recipient = (request.Candidate?.PrimaryAddress?.Recipient ?? ""),
                },
            },
        };
        var named0 = _registry.Get<ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient>("acme.recruiting.v1.JobOpeningCalculationService");
        using var rpc0 = named0.RecalculateJobOpeningsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLedger.WatchLedgerAccountsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Subscriber = new ApiCommon.ContactInfo
            {
                Email = (request.Candidate?.Contact?.Email ?? ""),
                Name = (request.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
                Phone = (request.Candidate?.Contact?.Phone ?? ""),
                PreferredChannel = (request.Candidate?.Contact?.PreferredChannel ?? ""),
            },
        };
        using var stream1 = _ledgerLedgerAccountService.WatchLedgerAccounts(input1, cancellationToken: token);
        await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received WatchLedgerAccounts frame {Type}", change1.GetType().Name);
        }
        var input2 = new ApiHelpdesk.CloneTicketRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Ticket = new ApiHelpdesk.Ticket
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Candidate?.Audit?.Etag ?? ""),
                    UpdatedAt = (reply0.JobOpening?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
            },
        };
        var command2 = new DispatchCommand<ApiHelpdesk.CloneTicketRequest>(input2,
            "primary", requestContext.TenantId);
        var routed2 = command2 with { Region = _settings.Value.Region };
        using var rpc2 = _helpdeskTicketService.CloneTicketAsync(routed2.Payload, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiTravel.PreviewTripRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Trip = new ApiTravel.Trip
            {
                Card = new ApiTravel.TripCardSettlement
                {
                    Network = (reply0.JobOpening ?? new ApiRecruiting.JobOpening()).PaymentCase switch
                    {
                        ApiRecruiting.JobOpening.SettlementOneofCase.Card => (reply0.JobOpening?.Card?.Network ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc3 = _travelTripValidationService.PreviewTripAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiRecruiting.ExplainCandidateCalculationResponse
        {
            Candidate = new ApiRecruiting.Candidate
            {
                Attributes = { (request.Candidate?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.JobOpening?.Audit?.Etag ?? ""),
                },
                CandidateId = (request.CandidateId ?? ""),
            },
            Outcome = new ApiRecruiting.ExplainCandidateCalculationResponse.Types.Outcome
            {
                Status = (reply2.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "candidate",
            "audit", "ExplainCandidateCalculation", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a candidate.
    /// </summary>
    public override async Task<ApiRecruiting.GetCandidateRatesResponse> GetCandidateRates(ApiRecruiting.GetCandidateRatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.CandidateId, "candidate_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetCandidateRates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "candidate", request.CandidateId,
            "GetCandidateRates", "accepted", token);
        var input0 = new ApiRecruiting.EstimateJobOpeningRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            JobOpening = new ApiRecruiting.JobOpening
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
                Lines = { new ApiRecruiting.JobOpeningLine
                {
                    Attributes = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                } },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _recruitingJobOpeningCalculationService.EstimateJobOpeningAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiRecruiting.GetCandidateRatesResponse
        {
            Candidate = new ApiRecruiting.Candidate
            {
                CandidateId = (request.CandidateId ?? ""),
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply0.JobOpening?.BillingAddress?.PostalCode ?? ""),
                },
            },
        };
        return response;
    }
    /// <summary>
    /// Quotes a candidate.
    /// </summary>
    public override async Task<ApiRecruiting.QuoteCandidateResponse> QuoteCandidate(ApiRecruiting.QuoteCandidateRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling QuoteCandidate for {Tenant}", requestContext.TenantId);
        if (request.Candidate is { } record)
        {
            var snapshot = CandidateMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = request.QuoteCandidateToExplainJobOpeningCalculation0();
        using var rpc0 = _recruitingJobOpeningCalculationService.ExplainJobOpeningCalculationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.EstimateJobOpeningRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.EstimateJobOpeningRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Details?.Destination?.PostalCode ?? ""),
                },
            },
            JobOpening = new ApiRecruiting.JobOpening
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Candidate?.PrimaryAddress?.PostalCode ?? ""),
                },
            },
        };
        var reply1 = await SendQuoteCandidate1Async(input1, token);
        var response = new ApiRecruiting.QuoteCandidateResponse
        {
            Breakdown = new ApiRecruiting.QuoteCandidateResponse.Types.Breakdown
            {
                Total = (reply1.Breakdown?.Total ?? new ApiCommon.Money()),
            },
            Candidate = new ApiRecruiting.Candidate
            {
                CandidateId = (request.CandidateId ?? ""),
                RelationshipManager = (request.Candidate?.RelationshipManager ?? new ApiIdentity.UserRef()),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "candidate",
            "audit", "QuoteCandidate", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalculates a candidate.
    /// </summary>
    public override async Task<ApiRecruiting.RecalculateCandidatesResponse> RecalculateCandidates(ApiRecruiting.RecalculateCandidatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RecalculateCandidates for {Tenant}", requestContext.TenantId);
        if (request.Candidate is { } record)
        {
            var snapshot = CandidateMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.LintTripsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Trip = new ApiTravel.Trip
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Details?.Destination?.CountryCode ?? ""),
                },
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _travelTripValidationService.LintTripsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.RecalculateJobOpeningsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            JobOpening = new ApiRecruiting.JobOpening
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (request.Candidate?.PrimaryAddress?.Locality ?? ""),
                },
            },
        };
        var packed1 = Any.Pack(input1);
        input1 = packed1.Unpack<ApiRecruiting.RecalculateJobOpeningsRequest>();
        using var rpc1 = _recruitingJobOpeningCalculationService.RecalculateJobOpeningsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.RecalculateCandidatesResponse
        {
            Candidate = new ApiRecruiting.Candidate
            {
                CandidateId = (request.CandidateId ?? ""),
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply1.JobOpening?.BillingAddress?.PostalCode ?? ""),
                    Recipient = (reply0.Trip?.BillingAddress?.Recipient ?? ""),
                },
                Status = (request.Candidate?.Status ?? (ApiRecruiting.CandidateStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "candidate",
            "audit", "RecalculateCandidates", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Simulates a candidate.
    /// </summary>
    public override async Task<ApiRecruiting.SimulateCandidateResponse> SimulateCandidate(ApiRecruiting.SimulateCandidateRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SimulateCandidate for {Tenant}", requestContext.TenantId);
        if (request.Candidate is { } record)
        {
            var snapshot = CandidateMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.SimulateJobOpeningRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.SimulateJobOpeningRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.Candidate?.PrimaryAddress?.Region ?? ""),
                },
            },
            JobOpening = new ApiRecruiting.JobOpening
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (request.Candidate?.PrimaryAddress?.Locality ?? ""),
                },
                Counterparty = new ApiRecruiting.InterviewRef
                {
                    DisplayName = (request.Candidate?.RelationshipManager?.DisplayName ?? ""),
                },
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiRecruiting.SimulateJobOpeningRequest>(json0);
        using var rpc0 = _recruitingJobOpeningCalculationService.SimulateJobOpeningAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiRecruiting.SimulateJobOpeningRequest>("acme.recruiting.v1.CandidateCalculationService/SimulateCandidate/0", input0.Clone()), token);
        var input1 = new ApiDocuments.AnnotateDocumentFolderVersionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.AnnotateDocumentFolderVersionRequest.Types.Details
            {
                Snapshot = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                Children = { new ApiDocuments.DocumentFolder
                {
                    Children = { new ApiDocuments.DocumentFolder
                    {
                        Location = new ApiCommon.PostalAddress
                        {
                            CountryCode = (request.Details?.Destination?.CountryCode ?? ""),
                        },
                    } },
                } },
            },
        };
        using var rpc1 = _documentsDocumentFolderHistoryService.AnnotateDocumentFolderVersionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.SimulateCandidateResponse
        {
            Breakdown = new ApiRecruiting.SimulateCandidateResponse.Types.Breakdown
            {
                CurrencyCode = (reply0.Breakdown?.CurrencyCode ?? ""),
            },
            Candidate = new ApiRecruiting.Candidate
            {
                Attributes = { (request.Candidate?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                CandidateId = (request.CandidateId ?? ""),
                RelationshipManager = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.DocumentFolder?.Manager?.DisplayName ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "candidate",
            "audit", "SimulateCandidate", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiRecruiting.RecalculateJobOpeningsResponse> SendCalculateCandidate0Async(ApiRecruiting.RecalculateJobOpeningsRequest request, CancellationToken token)
    {
        return await _candidateCalculationEndpointSendCalculateCandidate0Port.SendAsync(request, token);
    }
    private async Task<ApiRecruiting.EstimateJobOpeningResponse> SendQuoteCandidate1Async(ApiRecruiting.EstimateJobOpeningRequest request, CancellationToken token)
    {
        return await _candidateCalculationEndpointSendQuoteCandidate1Port.SendAsync(request, token);
    }
}
