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
using ApiCommon = Acme.Common.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSearch = Acme.Search.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Learning;

/// <summary>
/// Computes the figures of a certification, such as totals, taxes, and estimates.
/// </summary>
public sealed class CertificationCalculationEndpoint : ApiLearning.CertificationCalculationService.CertificationCalculationServiceBase
{
    private readonly ApiAudit.AuditEventValidationService.AuditEventValidationServiceClient _auditAuditEventValidationService;
    private readonly ApiHelpdesk.SlaPolicyCalculationService.SlaPolicyCalculationServiceClient _helpdeskSlaPolicyCalculationService;
    private readonly ApiHelpdesk.TicketQueryService.TicketQueryServiceClient _helpdeskTicketQueryService;
    private readonly CallInvoker _invoker;
    private readonly ApiLearning.CourseValidationService.CourseValidationServiceClient _learningCourseValidationService;
    private readonly ApiLedger.LedgerAccountValidationService.LedgerAccountValidationServiceClient _ledgerLedgerAccountValidationService;
    private readonly ILogger<CertificationCalculationEndpoint> _logger;
    private readonly ApiMetrics.MetricSeriesService.MetricSeriesServiceClient _metricsMetricSeriesService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiSearch.SearchIndexService.SearchIndexServiceClient _searchSearchIndexService;
    private readonly Func<ApiLearning.ExplainCourseDecisionRequest, CallOptions, AsyncUnaryCall<ApiLearning.ExplainCourseDecisionResponse>> _sendCompareCertificationOptions2;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTravel.ItineraryCalculationService.ItineraryCalculationServiceClient _travelItineraryCalculationService;
    private readonly ApiWorkplaces.DeskValidationService.DeskValidationServiceClient _workplacesDeskValidationService;

    public CertificationCalculationEndpoint(
        ApiAudit.AuditEventValidationService.AuditEventValidationServiceClient auditAuditEventValidationService,
        ApiHelpdesk.SlaPolicyCalculationService.SlaPolicyCalculationServiceClient helpdeskSlaPolicyCalculationService,
        ApiHelpdesk.TicketQueryService.TicketQueryServiceClient helpdeskTicketQueryService,
        CallInvoker invoker,
        ApiLearning.CourseValidationService.CourseValidationServiceClient learningCourseValidationService,
        ApiLedger.LedgerAccountValidationService.LedgerAccountValidationServiceClient ledgerLedgerAccountValidationService,
        ILogger<CertificationCalculationEndpoint> logger,
        ApiMetrics.MetricSeriesService.MetricSeriesServiceClient metricsMetricSeriesService,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiSearch.SearchIndexService.SearchIndexServiceClient searchSearchIndexService,
        IOptions<MeshOptions> settings,
        ApiTravel.ItineraryCalculationService.ItineraryCalculationServiceClient travelItineraryCalculationService,
        ApiWorkplaces.DeskValidationService.DeskValidationServiceClient workplacesDeskValidationService)
    {
        _auditAuditEventValidationService = auditAuditEventValidationService;
        _helpdeskSlaPolicyCalculationService = helpdeskSlaPolicyCalculationService;
        _helpdeskTicketQueryService = helpdeskTicketQueryService;
        _invoker = invoker;
        _learningCourseValidationService = learningCourseValidationService;
        _ledgerLedgerAccountValidationService = ledgerLedgerAccountValidationService;
        _logger = logger;
        _metricsMetricSeriesService = metricsMetricSeriesService;
        _registry = registry;
        _repository = repository;
        _searchSearchIndexService = searchSearchIndexService;
        _settings = settings;
        _travelItineraryCalculationService = travelItineraryCalculationService;
        _workplacesDeskValidationService = workplacesDeskValidationService;
        _sendCompareCertificationOptions2 = _learningCourseValidationService.ExplainCourseDecisionAsync;
    }
    /// <summary>
    /// Calculates a certification.
    /// </summary>
    public override async Task<ApiLearning.CalculateCertificationResponse> CalculateCertification(ApiLearning.CalculateCertificationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CalculateCertification for {Tenant}", requestContext.TenantId);
        if (request.Certification is { } record)
        {
            var snapshot = CertificationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiAudit.ScoreAuditEventRequest
        {
            AuditEvent = new ApiAudit.AuditEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiAudit.ScoreAuditEventRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Details?.Destination?.CountryCode ?? ""),
                    Lines = { (request.Certification?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Locality = (request.Details?.Destination?.Locality ?? ""),
                    Region = (request.Certification?.BillingAddress?.Region ?? ""),
                },
            },
        };
        using var rpc0 = _auditAuditEventValidationService.ScoreAuditEventAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiLearning.CalculateCertificationResponse
        {
            Breakdown = new ApiLearning.CalculateCertificationResponse.Types.Breakdown
            {
                CurrencyCode = (reply0.Breakdown?.CurrencyCode ?? ""),
            },
            Certification = new ApiLearning.Certification
            {
                CertificationId = (request.CertificationId ?? ""),
                Number = (request.Certification?.Number ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "certification",
            "audit", "CalculateCertification", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Compares a certification.
    /// </summary>
    public override async Task<ApiLearning.CompareCertificationOptionsResponse> CompareCertificationOptions(ApiLearning.CompareCertificationOptionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CompareCertificationOptions for {Tenant}", requestContext.TenantId);
        if (request.Certification is { } record)
        {
            var snapshot = CertificationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLearning.ValidateCourseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Course = new ApiLearning.Course
            {
                Address = new ApiCommon.PostalAddress
                {
                    Region = (request.Certification?.BillingAddress?.Region ?? ""),
                },
            },
        };
        using var rpc0 = _learningCourseValidationService.ValidateCourseAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiHelpdesk.SuggestTicketsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiHelpdesk.SuggestTicketsRequest.Types.Filter
            {
                Region = (request.Certification?.BillingAddress?.Region ?? ""),
            },
        };
        using var rpc1 = _helpdeskTicketQueryService.SuggestTicketsAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var input2 = new ApiLearning.ExplainCourseDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Course = new ApiLearning.Course
            {
                Address = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Certification?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Locality = (request.Details?.Destination?.Locality ?? ""),
                    Recipient = (request.Details?.Destination?.Recipient ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Course?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
        };
        using var rpc2 = _sendCompareCertificationOptions2(input2, new CallOptions(cancellationToken: token));
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiHelpdesk.CalculateSlaPolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.CalculateSlaPolicyRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Details?.Destination?.CountryCode ?? ""),
                },
            },
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply2.Course?.Audit?.CreatedAt ?? new Timestamp()),
                    Etag = (reply2.Course?.Audit?.Etag ?? ""),
                },
            },
        };
        var named3 = _registry.Get<ApiHelpdesk.SlaPolicyCalculationService.SlaPolicyCalculationServiceClient>("acme.helpdesk.v1.SlaPolicyCalculationService");
        using var rpc3 = named3.CalculateSlaPolicyAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiLearning.CompareCertificationOptionsResponse
        {
            Breakdown = new ApiLearning.CompareCertificationOptionsResponse.Types.Breakdown
            {
                Lines = { new ApiLearning.CompareCertificationOptionsResponse.Types.Breakdown.Types.Line
                {
                    Amount = (reply3.SlaPolicy?.Discount?.Amount ?? new ApiCommon.Money()),
                } },
            },
            Certification = new ApiLearning.Certification
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply3.SlaPolicy?.Audit?.CreatedBy ?? ""),
                },
                BillingAddress = (request.Certification?.BillingAddress ?? new ApiCommon.PostalAddress()),
                CertificationId = (request.CertificationId ?? ""),
                Counterparty = new ApiRecruiting.CandidateRef
                {
                    Email = (reply2.Course?.Owner?.Email ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "certification",
            "audit", "CompareCertificationOptions", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Estimates a certification.
    /// </summary>
    public override async Task<ApiLearning.EstimateCertificationResponse> EstimateCertification(ApiLearning.EstimateCertificationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling EstimateCertification for {Tenant}", requestContext.TenantId);
        if (request.Certification is { } record)
        {
            var snapshot = CertificationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLearning.ExplainCourseDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Course = new ApiLearning.Course
            {
                Address = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Certification?.BillingAddress?.CountryCode ?? ""),
                    Lines = { (request.Certification?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    PostalCode = (request.Certification?.BillingAddress?.PostalCode ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Certification?.Counterparty?.Email ?? ""),
                },
            },
        };
        var descriptor0 = new Method<ApiLearning.ExplainCourseDecisionRequest, ApiLearning.ExplainCourseDecisionResponse>(
            MethodType.Unary, "acme.learning.v1" + ".CourseValidationService", "ExplainCourseDecision",
            Wire.Marshaller<ApiLearning.ExplainCourseDecisionRequest>(), Wire.Marshaller<ApiLearning.ExplainCourseDecisionResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLearning.CheckCourseEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Course = new ApiLearning.Course
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.Certification?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        var command1 = new DispatchCommand<ApiLearning.CheckCourseEligibilityRequest>(input1,
            "primary", requestContext.TenantId);
        var routed1 = command1 with { Region = _settings.Value.Region };
        using var rpc1 = _learningCourseValidationService.CheckCourseEligibilityAsync(routed1.Payload, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var profile2 = new MappingProfile<ApiLearning.EstimateCertificationRequest, ApiLedger.LintLedgerAccountsRequest>(value => new ApiLedger.LintLedgerAccountsRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLedger.LintLedgerAccountsRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply1.Course?.Owner?.Email ?? ""),
                },
            },
            LedgerAccount = new ApiLedger.LedgerAccount
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.Course?.Audit?.Etag ?? ""),
                    UpdatedAt = (reply1.Course?.Audit?.UpdatedAt ?? new Timestamp()),
                    UpdatedBy = (reply1.Course?.Audit?.UpdatedBy ?? ""),
                },
            },
        });
        var input2 = profile2.Map(request);
        using var rpc2 = _ledgerLedgerAccountValidationService.LintLedgerAccountsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiMetrics.UpdateMetricSeriesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            MetricSeries = new ApiMetrics.MetricSeries
            {
                Address = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply0.Course ?? new ApiLearning.Course()).LocatorCase switch
                    {
                        ApiLearning.Course.LocatorOneofCase.Address => (reply0.Course?.Address?.CountryCode ?? ""),
                        _ => ""
                    },
                    PostalCode = (reply1.Course ?? new ApiLearning.Course()).LocatorCase switch
                    {
                        ApiLearning.Course.LocatorOneofCase.Address => (reply1.Course?.Address?.PostalCode ?? ""),
                        _ => ""
                    },
                },
                Dimensions = new ApiMetrics.MetricSeriesDimensions
                {
                    Width = (reply1.Course?.Dimensions?.Width ?? 0d),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply2.LedgerAccount?.Holder?.DisplayName ?? ""),
                },
            },
        };
        using var rpc3 = _metricsMetricSeriesService.UpdateMetricSeriesAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiLearning.EstimateCertificationResponse
        {
            Breakdown = new ApiLearning.EstimateCertificationResponse.Types.Breakdown
            {
                CurrencyCode = (reply2.LedgerAccount?.CurrencyCode ?? ""),
            },
            Certification = new ApiLearning.Certification
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (reply1.Course?.Address?.Locality ?? ""),
                    Recipient = (reply0.Course?.Address?.Recipient ?? ""),
                },
                CertificationId = (request.CertificationId ?? ""),
                Counterparty = (request.Certification?.Counterparty ?? new ApiRecruiting.CandidateRef()),
                Lines = { new ApiLearning.CertificationLine
                {
                    Description = (reply0.Course?.Components?.GetValueOrDefault("primary")?.Description ?? ""),
                    Quantity = (reply3.MetricSeries?.Components?.GetValueOrDefault("primary")?.Quantity ?? 0L),
                } },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "certification",
            "audit", "EstimateCertification", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a certification.
    /// </summary>
    public override async Task<ApiLearning.GetCertificationRatesResponse> GetCertificationRates(ApiLearning.GetCertificationRatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.CertificationId, "certification_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetCertificationRates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "certification", request.CertificationId,
            "GetCertificationRates", "accepted", token);
        var response = new ApiLearning.GetCertificationRatesResponse
        {
            Certification = new ApiLearning.Certification
            {
                CertificationId = (request.CertificationId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Quotes a certification.
    /// </summary>
    public override async Task<ApiLearning.QuoteCertificationResponse> QuoteCertification(ApiLearning.QuoteCertificationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling QuoteCertification for {Tenant}", requestContext.TenantId);
        if (request.Certification is { } record)
        {
            var snapshot = CertificationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = request.QuoteCertificationToExplainCourseDecision0();
        using var rpc0 = _learningCourseValidationService.ExplainCourseDecisionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTravel.QuoteItineraryRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Itinerary = new ApiTravel.Itinerary
            {
                Card = new ApiTravel.ItineraryCardSettlement
                {
                    Network = (request.Certification ?? new ApiLearning.Certification()).PaymentCase switch
                    {
                        ApiLearning.Certification.SettlementOneofCase.Card => (request.Certification?.Card?.Network ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _travelItineraryCalculationService.QuoteItineraryAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLearning.QuoteCertificationResponse
        {
            Certification = new ApiLearning.Certification
            {
                CertificationId = (request.CertificationId ?? ""),
                Counterparty = new ApiRecruiting.CandidateRef
                {
                    Email = (reply0.Course?.Owner?.Email ?? ""),
                },
                Metadata = { (request.Certification?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Total = (reply1.Itinerary?.Total ?? new ApiCommon.Money()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "certification",
            "audit", "QuoteCertification", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalculates a certification.
    /// </summary>
    public override async Task<ApiLearning.RecalculateCertificationsResponse> RecalculateCertifications(ApiLearning.RecalculateCertificationsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RecalculateCertifications for {Tenant}", requestContext.TenantId);
        if (request.Certification is { } record)
        {
            var snapshot = CertificationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiSearch.BatchGetSearchIndexesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _searchSearchIndexService.BatchGetSearchIndexesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLearning.ValidateCourseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Course = new ApiLearning.Course
            {
                Address = new ApiCommon.PostalAddress
                {
                    Recipient = (request.Certification?.BillingAddress?.Recipient ?? ""),
                    Region = (request.Details?.Destination?.Region ?? ""),
                },
            },
        };
        var packed1 = Any.Pack(input1);
        input1 = packed1.Unpack<ApiLearning.ValidateCourseRequest>();
        using var rpc1 = _learningCourseValidationService.ValidateCourseAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLearning.RecalculateCertificationsResponse
        {
            Breakdown = new ApiLearning.RecalculateCertificationsResponse.Types.Breakdown
            {
                Total = (request.Certification?.Total ?? new ApiCommon.Money()),
            },
            Certification = new ApiLearning.Certification
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.Course?.Audit?.CreatedAt ?? new Timestamp()),
                },
                CertificationId = (request.CertificationId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "certification",
            "audit", "RecalculateCertifications", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Simulates a certification.
    /// </summary>
    public override async Task<ApiLearning.SimulateCertificationResponse> SimulateCertification(ApiLearning.SimulateCertificationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SimulateCertification for {Tenant}", requestContext.TenantId);
        if (request.Certification is { } record)
        {
            var snapshot = CertificationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiWorkplaces.NormalizeDeskRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Desk = new ApiWorkplaces.Desk
            {
                Address = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Certification?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Locality = (request.Details?.Destination?.Locality ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Certification?.Audit?.CreatedBy ?? ""),
                },
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiWorkplaces.NormalizeDeskRequest>(json0);
        using var rpc0 = _workplacesDeskValidationService.NormalizeDeskAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiLearning.SimulateCertificationResponse
        {
            Certification = new ApiLearning.Certification
            {
                BillingAddress = (request.Certification?.BillingAddress ?? new ApiCommon.PostalAddress()),
                CertificationId = (request.CertificationId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "certification",
            "audit", "SimulateCertification", (request.Reason ?? ""), token);
        return response;
    }
}
