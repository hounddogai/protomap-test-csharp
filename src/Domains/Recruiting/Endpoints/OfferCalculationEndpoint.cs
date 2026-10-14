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
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWorkplaces = Acme.Workplaces.V1;
using ApiYards = Acme.Yards.V1;

namespace Acme.Workforce.Recruiting;

/// <summary>
/// Computes the figures of an offer, such as totals, taxes, and estimates.
/// </summary>
public sealed class OfferCalculationEndpoint : ApiRecruiting.OfferCalculationService.OfferCalculationServiceBase
{
    private readonly ApiFeatureflags.FeatureFlagValidationService.FeatureFlagValidationServiceClient _featureflagsFeatureFlagValidationService;
    private readonly ApiHelpdesk.TicketCalculationService.TicketCalculationServiceClient _helpdeskTicketCalculationService;
    private readonly CallInvoker _invoker;
    private readonly ApiLicensing.SoftwareLicenseQueryService.SoftwareLicenseQueryServiceClient _licensingSoftwareLicenseQueryService;
    private readonly ILogger<OfferCalculationEndpoint> _logger;
    private readonly IOfferCalculationEndpointSendSimulateOffer0Port _offerCalculationEndpointSendSimulateOffer0Port;
    private readonly RetryPolicy _policy;
    private readonly ApiRecruiting.CandidateCalculationService.CandidateCalculationServiceClient _recruitingCandidateCalculationService;
    private readonly ApiRecruiting.InterviewCalculationService.InterviewCalculationServiceClient _recruitingInterviewCalculationService;
    private readonly ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient _recruitingJobOpeningCalculationService;
    private readonly IRecordRepository _repository;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTravel.ItineraryCalculationService.ItineraryCalculationServiceClient _travelItineraryCalculationService;
    private readonly ApiWorkplaces.RoomBookingValidationService.RoomBookingValidationServiceClient _workplacesRoomBookingValidationService;
    private readonly ApiYards.YardCalculationService.YardCalculationServiceClient _yardsYardCalculationService;

    public OfferCalculationEndpoint(
        ApiFeatureflags.FeatureFlagValidationService.FeatureFlagValidationServiceClient featureflagsFeatureFlagValidationService,
        ApiHelpdesk.TicketCalculationService.TicketCalculationServiceClient helpdeskTicketCalculationService,
        CallInvoker invoker,
        ApiLicensing.SoftwareLicenseQueryService.SoftwareLicenseQueryServiceClient licensingSoftwareLicenseQueryService,
        ILogger<OfferCalculationEndpoint> logger,
        IOfferCalculationEndpointSendSimulateOffer0Port offerCalculationEndpointSendSimulateOffer0PortAdapter,
        RetryPolicy policy,
        ApiRecruiting.CandidateCalculationService.CandidateCalculationServiceClient recruitingCandidateCalculationService,
        ApiRecruiting.InterviewCalculationService.InterviewCalculationServiceClient recruitingInterviewCalculationService,
        ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient recruitingJobOpeningCalculationService,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiTravel.ItineraryCalculationService.ItineraryCalculationServiceClient travelItineraryCalculationService,
        ApiWorkplaces.RoomBookingValidationService.RoomBookingValidationServiceClient workplacesRoomBookingValidationService,
        ApiYards.YardCalculationService.YardCalculationServiceClient yardsYardCalculationService)
    {
        _featureflagsFeatureFlagValidationService = featureflagsFeatureFlagValidationService;
        _helpdeskTicketCalculationService = helpdeskTicketCalculationService;
        _invoker = invoker;
        _licensingSoftwareLicenseQueryService = licensingSoftwareLicenseQueryService;
        _logger = logger;
        _offerCalculationEndpointSendSimulateOffer0Port = offerCalculationEndpointSendSimulateOffer0PortAdapter;
        _policy = policy;
        _recruitingCandidateCalculationService = recruitingCandidateCalculationService;
        _recruitingInterviewCalculationService = recruitingInterviewCalculationService;
        _recruitingJobOpeningCalculationService = recruitingJobOpeningCalculationService;
        _repository = repository;
        _settings = settings;
        _travelItineraryCalculationService = travelItineraryCalculationService;
        _workplacesRoomBookingValidationService = workplacesRoomBookingValidationService;
        _yardsYardCalculationService = yardsYardCalculationService;
    }
    /// <summary>
    /// Calculates an offer.
    /// </summary>
    public override async Task<ApiRecruiting.CalculateOfferResponse> CalculateOffer(ApiRecruiting.CalculateOfferRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CalculateOffer for {Tenant}", requestContext.TenantId);
        if (request.Offer is { } record)
        {
            var snapshot = OfferMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.RecalculateInterviewsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.RecalculateInterviewsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Details?.Destination?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Recipient = (request.Offer?.BillingAddress?.Recipient ?? ""),
                },
            },
            Interview = new ApiRecruiting.Interview
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Offer?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Location = new ApiCommon.PostalAddress
                {
                    Locality = (request.Offer?.BillingAddress?.Locality ?? ""),
                },
            },
        };
        using var rpc0 = _recruitingInterviewCalculationService.RecalculateInterviewsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiFeatureflags.ValidateFeatureFlagRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            FeatureFlag = new ApiFeatureflags.FeatureFlag
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Offer?.Audit?.CreatedBy ?? ""),
                    Etag = (request.Offer?.Audit?.Etag ?? ""),
                    UpdatedAt = (reply0.Interview?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Offer?.Counterparty?.Email ?? ""),
                },
            },
        };
        var reply1 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _featureflagsFeatureFlagValidationService.ValidateFeatureFlagAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input2 = new ApiRecruiting.QuoteCandidateRequest
        {
            Candidate = new ApiRecruiting.Candidate
            {
                RelationshipManager = new ApiIdentity.UserRef
                {
                    Email = (request.Offer?.Counterparty?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply2 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _recruitingCandidateCalculationService.QuoteCandidateAsync(input2, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiRecruiting.CalculateOfferResponse
        {
            Offer = new ApiRecruiting.Offer
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply2.Candidate?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedBy = (reply1.FeatureFlag?.Audit?.UpdatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.Interview?.Location?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (reply0.Interview?.Organizer?.Email ?? ""),
                },
                IssuedAt = (request.Offer?.IssuedAt ?? new Timestamp()),
                OfferId = (request.OfferId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "offer",
            "audit", "CalculateOffer", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Estimates an offer.
    /// </summary>
    public override async Task<ApiRecruiting.EstimateOfferResponse> EstimateOffer(ApiRecruiting.EstimateOfferRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling EstimateOffer for {Tenant}", requestContext.TenantId);
        if (request.Offer is { } record)
        {
            var snapshot = OfferMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.CalculateTicketRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.CalculateTicketRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Details?.Destination?.PostalCode ?? ""),
                },
            },
            Ticket = new ApiHelpdesk.Ticket
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Offer?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _helpdeskTicketCalculationService.CalculateTicketAsync);
        var input1 = new ApiRecruiting.QuoteJobOpeningRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.QuoteJobOpeningRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Details?.Destination?.PostalCode ?? ""),
                },
            },
            JobOpening = new ApiRecruiting.JobOpening
            {
                Bank = new ApiRecruiting.JobOpeningBankSettlement
                {
                    AccountHolder = (request.Offer ?? new ApiRecruiting.Offer()).PaymentCase switch
                    {
                        ApiRecruiting.Offer.SettlementOneofCase.Bank => (request.Offer?.Bank?.AccountHolder ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _recruitingJobOpeningCalculationService.QuoteJobOpeningAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var reply2 = new ApiRecruiting.CompareJobOpeningOptionsResponse();
        foreach (var item in (request.Offer?.Attachments ?? new Google.Protobuf.Collections.RepeatedField<ApiRecruiting.OfferAttachment>()))
        {
            var input2 = new ApiRecruiting.CompareJobOpeningOptionsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                JobOpening = new ApiRecruiting.JobOpening
                {
                    Attachments = { new ApiRecruiting.JobOpeningAttachment
                    {
                        ContentType = (item.ContentType ?? ""),
                        FileName = (item.FileName ?? ""),
                        SizeBytes = item.SizeBytes,
                    } },
                },
            };
            using var rpc2 = _recruitingJobOpeningCalculationService.CompareJobOpeningOptionsAsync(input2, cancellationToken: token);
            reply2 = await rpc2.ResponseAsync;
        }
        var response = new ApiRecruiting.EstimateOfferResponse
        {
            Breakdown = new ApiRecruiting.EstimateOfferResponse.Types.Breakdown
            {
                Total = (request.Offer?.Total ?? new ApiCommon.Money()),
            },
            Offer = new ApiRecruiting.Offer
            {
                DueAt = (reply0.Ticket?.Sla?.DueAt ?? new Timestamp()),
                Metadata = { (reply1.JobOpening?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OfferId = (request.OfferId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "offer",
            "audit", "EstimateOffer", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains an offer.
    /// </summary>
    public override async Task<ApiRecruiting.ExplainOfferCalculationResponse> ExplainOfferCalculation(ApiRecruiting.ExplainOfferCalculationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ExplainOfferCalculation for {Tenant}", requestContext.TenantId);
        if (request.Offer is { } record)
        {
            var snapshot = OfferMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiWorkplaces.ValidateRoomBookingRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Offer?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Location = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Offer?.BillingAddress?.CountryCode ?? ""),
                },
            },
        };
        var descriptor0 = new Method<ApiWorkplaces.ValidateRoomBookingRequest, ApiWorkplaces.ValidateRoomBookingResponse>(
            MethodType.Unary, "acme.workplaces.v1" + ".RoomBookingValidationService", "ValidateRoomBooking",
            Wire.Marshaller<ApiWorkplaces.ValidateRoomBookingRequest>(), Wire.Marshaller<ApiWorkplaces.ValidateRoomBookingResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.RecalculateInterviewsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Interview = new ApiRecruiting.Interview
            {
                Participants = { new ApiRecruiting.InterviewParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (reply0.RoomBooking?.Organizer?.Email ?? ""),
                        Phone = (request.Details?.Contact?.Phone ?? ""),
                        PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
                    },
                } },
            },
        };
        var command1 = new DispatchCommand<ApiRecruiting.RecalculateInterviewsRequest>(input1,
            "primary", requestContext.TenantId);
        var routed1 = command1 with { Region = _settings.Value.Region };
        using var rpc1 = _recruitingInterviewCalculationService.RecalculateInterviewsAsync(routed1.Payload, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.ExplainOfferCalculationResponse
        {
            Offer = new ApiRecruiting.Offer
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply1.Interview?.Audit?.UpdatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.RoomBooking?.Location?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
                OfferId = (request.OfferId ?? ""),
                Total = (request.Offer?.Total ?? new ApiCommon.Money()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "offer",
            "audit", "ExplainOfferCalculation", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Quotes an offer.
    /// </summary>
    public override async Task<ApiRecruiting.QuoteOfferResponse> QuoteOffer(ApiRecruiting.QuoteOfferRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling QuoteOffer for {Tenant}", requestContext.TenantId);
        if (request.Offer is { } record)
        {
            var snapshot = OfferMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var profile0 = new MappingProfile<ApiRecruiting.QuoteOfferRequest, ApiTravel.EstimateItineraryRequest>(value => new ApiTravel.EstimateItineraryRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Itinerary = new ApiTravel.Itinerary
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (value.Offer?.Audit?.CreatedBy ?? ""),
                },
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _travelItineraryCalculationService.EstimateItineraryAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiRecruiting.QuoteOfferResponse
        {
            Offer = new ApiRecruiting.Offer
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (reply0.Itinerary?.BillingAddress?.Region ?? ""),
                },
                DueAt = (request.Offer?.DueAt ?? new Timestamp()),
                IssuedAt = (reply0.Itinerary?.IssuedAt ?? new Timestamp()),
                OfferId = (request.OfferId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "offer",
            "audit", "QuoteOffer", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalculates an offer.
    /// </summary>
    public override async Task<ApiRecruiting.RecalculateOffersResponse> RecalculateOffers(ApiRecruiting.RecalculateOffersRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RecalculateOffers for {Tenant}", requestContext.TenantId);
        if (request.Offer is { } record)
        {
            var snapshot = OfferMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.ExplainInterviewCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Interview = new ApiRecruiting.Interview
            {
                Location = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Offer?.BillingAddress?.CountryCode ?? ""),
                    Locality = (request.Details?.Destination?.Locality ?? ""),
                },
                Organizer = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Offer?.Counterparty?.DisplayName ?? ""),
                    Email = (request.Offer?.Counterparty?.Email ?? ""),
                },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _recruitingInterviewCalculationService.ExplainInterviewCalculationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.ExportSoftwareLicensesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLicensing.ExportSoftwareLicensesRequest.Types.Filter
            {
                CreatedAfter = (reply0.Interview?.Audit?.CreatedAt ?? new Timestamp()),
                Region = (reply0.Interview?.Location?.Region ?? ""),
            },
        };
        using var stream1 = _licensingSoftwareLicenseQueryService.ExportSoftwareLicenses(input1, cancellationToken: token);
        await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportSoftwareLicenses frame {Type}", change1.GetType().Name);
        }
        var response = new ApiRecruiting.RecalculateOffersResponse
        {
            Offer = new ApiRecruiting.Offer
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (reply0.Interview?.Organizer?.DisplayName ?? ""),
                },
                OfferId = (request.OfferId ?? ""),
                Total = (request.Offer?.Total ?? new ApiCommon.Money()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "offer",
            "audit", "RecalculateOffers", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Simulates an offer.
    /// </summary>
    public override async Task<ApiRecruiting.SimulateOfferResponse> SimulateOffer(ApiRecruiting.SimulateOfferRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SimulateOffer for {Tenant}", requestContext.TenantId);
        if (request.Offer is { } record)
        {
            var snapshot = OfferMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiYards.ExplainYardCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Yard = new ApiYards.Yard
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Offer?.Counterparty?.Email ?? ""),
                },
            },
        };
        var reply0 = await SendSimulateOffer0Async(input0, token);
        var input1 = new ApiRecruiting.EstimateInterviewRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.EstimateInterviewRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Locality = (request.Offer?.BillingAddress?.Locality ?? ""),
                },
            },
            Interview = new ApiRecruiting.Interview
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.Offer?.Audit?.UpdatedBy ?? ""),
                },
                Location = new ApiCommon.PostalAddress
                {
                    Region = (request.Offer?.BillingAddress?.Region ?? ""),
                },
                Organizer = new ApiIdentity.UserRef
                {
                    Email = (request.Offer?.Counterparty?.Email ?? ""),
                },
            },
        };
        var labels1 = input1.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input1.Context.Baggage.Clear();
        input1.Context.Baggage.Add(labels1);
        using var rpc1 = _recruitingInterviewCalculationService.EstimateInterviewAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.SimulateOfferResponse
        {
            Offer = new ApiRecruiting.Offer
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply1.Interview?.Audit?.UpdatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (reply1.Interview?.Location?.Region ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (reply0.Yard?.Owner?.DisplayName ?? ""),
                },
                OfferId = (request.OfferId ?? ""),
                Status = (request.Offer?.Status ?? (ApiRecruiting.OfferStatus)0),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "offer",
            "audit", "SimulateOffer", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiYards.ExplainYardCalculationResponse> SendSimulateOffer0Async(ApiYards.ExplainYardCalculationRequest request, CancellationToken token)
    {
        return await _offerCalculationEndpointSendSimulateOffer0Port.SendAsync(request, token);
    }
}
