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
using ApiFuel = Acme.Fuel.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiSearch = Acme.Search.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Helpdesk;

/// <summary>
/// Computes the figures of an escalation, such as totals, taxes, and estimates.
/// </summary>
public sealed class EscalationCalculationEndpoint : ApiHelpdesk.EscalationCalculationService.EscalationCalculationServiceBase
{
    private readonly ApiDocuments.DocumentFolderNotificationService.DocumentFolderNotificationServiceClient _documentsDocumentFolderNotificationService;
    private readonly ApiDocuments.DocumentNotificationService.DocumentNotificationServiceClient _documentsDocumentNotificationService;
    private readonly IEscalationCalculationEndpointSendQuoteEscalation3Port _escalationCalculationEndpointSendQuoteEscalation3Port;
    private readonly ApiFuel.FuelCardCalculationService.FuelCardCalculationServiceClient _fuelFuelCardCalculationService;
    private readonly ApiHelpdesk.KnowledgeArticleCalculationService.KnowledgeArticleCalculationServiceClient _helpdeskKnowledgeArticleCalculationService;
    private readonly ApiHelpdesk.KnowledgeArticleValidationService.KnowledgeArticleValidationServiceClient _helpdeskKnowledgeArticleValidationService;
    private readonly ApiHelpdesk.SlaPolicyCalculationService.SlaPolicyCalculationServiceClient _helpdeskSlaPolicyCalculationService;
    private readonly ApiHelpdesk.TicketCalculationService.TicketCalculationServiceClient _helpdeskTicketCalculationService;
    private readonly ApiHelpdesk.TicketValidationService.TicketValidationServiceClient _helpdeskTicketValidationService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.ChangeRequestValidationService.ChangeRequestValidationServiceClient _itsmChangeRequestValidationService;
    private readonly ApiItsm.IncidentService.IncidentServiceClient _itsmIncidentService;
    private readonly ApiItsm.ServiceAssetValidationService.ServiceAssetValidationServiceClient _itsmServiceAssetValidationService;
    private readonly ApiLicensing.EntitlementValidationService.EntitlementValidationServiceClient _licensingEntitlementValidationService;
    private readonly ILogger<EscalationCalculationEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiSearch.SearchIndexQueryService.SearchIndexQueryServiceClient _searchSearchIndexQueryService;
    private readonly Func<ApiItsm.ValidateChangeRequestRequest, CallOptions, AsyncUnaryCall<ApiItsm.ValidateChangeRequestResponse>> _sendExplainEscalationCalculation0;
    private readonly ApiVenues.VenueValidationService.VenueValidationServiceClient _venuesVenueValidationService;

    public EscalationCalculationEndpoint(
        ApiDocuments.DocumentFolderNotificationService.DocumentFolderNotificationServiceClient documentsDocumentFolderNotificationService,
        ApiDocuments.DocumentNotificationService.DocumentNotificationServiceClient documentsDocumentNotificationService,
        IEscalationCalculationEndpointSendQuoteEscalation3Port escalationCalculationEndpointSendQuoteEscalation3PortAdapter,
        ApiFuel.FuelCardCalculationService.FuelCardCalculationServiceClient fuelFuelCardCalculationService,
        ApiHelpdesk.KnowledgeArticleCalculationService.KnowledgeArticleCalculationServiceClient helpdeskKnowledgeArticleCalculationService,
        ApiHelpdesk.KnowledgeArticleValidationService.KnowledgeArticleValidationServiceClient helpdeskKnowledgeArticleValidationService,
        ApiHelpdesk.SlaPolicyCalculationService.SlaPolicyCalculationServiceClient helpdeskSlaPolicyCalculationService,
        ApiHelpdesk.TicketCalculationService.TicketCalculationServiceClient helpdeskTicketCalculationService,
        ApiHelpdesk.TicketValidationService.TicketValidationServiceClient helpdeskTicketValidationService,
        CallInvoker invoker,
        ApiItsm.ChangeRequestValidationService.ChangeRequestValidationServiceClient itsmChangeRequestValidationService,
        ApiItsm.IncidentService.IncidentServiceClient itsmIncidentService,
        ApiItsm.ServiceAssetValidationService.ServiceAssetValidationServiceClient itsmServiceAssetValidationService,
        ApiLicensing.EntitlementValidationService.EntitlementValidationServiceClient licensingEntitlementValidationService,
        ILogger<EscalationCalculationEndpoint> logger,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiSearch.SearchIndexQueryService.SearchIndexQueryServiceClient searchSearchIndexQueryService,
        ApiVenues.VenueValidationService.VenueValidationServiceClient venuesVenueValidationService)
    {
        _documentsDocumentFolderNotificationService = documentsDocumentFolderNotificationService;
        _documentsDocumentNotificationService = documentsDocumentNotificationService;
        _escalationCalculationEndpointSendQuoteEscalation3Port = escalationCalculationEndpointSendQuoteEscalation3PortAdapter;
        _fuelFuelCardCalculationService = fuelFuelCardCalculationService;
        _helpdeskKnowledgeArticleCalculationService = helpdeskKnowledgeArticleCalculationService;
        _helpdeskKnowledgeArticleValidationService = helpdeskKnowledgeArticleValidationService;
        _helpdeskSlaPolicyCalculationService = helpdeskSlaPolicyCalculationService;
        _helpdeskTicketCalculationService = helpdeskTicketCalculationService;
        _helpdeskTicketValidationService = helpdeskTicketValidationService;
        _invoker = invoker;
        _itsmChangeRequestValidationService = itsmChangeRequestValidationService;
        _itsmIncidentService = itsmIncidentService;
        _itsmServiceAssetValidationService = itsmServiceAssetValidationService;
        _licensingEntitlementValidationService = licensingEntitlementValidationService;
        _logger = logger;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _searchSearchIndexQueryService = searchSearchIndexQueryService;
        _venuesVenueValidationService = venuesVenueValidationService;
        _sendExplainEscalationCalculation0 = _itsmChangeRequestValidationService.ValidateChangeRequestAsync;
    }
    /// <summary>
    /// Calculates an escalation.
    /// </summary>
    public override async Task<ApiHelpdesk.CalculateEscalationResponse> CalculateEscalation(ApiHelpdesk.CalculateEscalationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CalculateEscalation for {Tenant}", requestContext.TenantId);
        if (request.Escalation is { } record)
        {
            var snapshot = EscalationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiHelpdesk.CalculateSlaPolicyResponse();
        foreach (var item in (request.Escalation?.History ?? new Google.Protobuf.Collections.RepeatedField<ApiHelpdesk.EscalationStatusChange>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input0 = new ApiHelpdesk.CalculateSlaPolicyRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                SlaPolicy = new ApiHelpdesk.SlaPolicy
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        Etag = (request.Escalation?.Audit?.Etag ?? ""),
                        UpdatedAt = (item.ChangedAt ?? new Timestamp()),
                    },
                    Owner = new ApiIdentity.UserRef
                    {
                        DisplayName = (request.Escalation?.Customer?.DisplayName ?? ""),
                        Email = (request.Escalation?.Requester?.Email ?? ""),
                    },
                },
            };
            using var rpc0 = _helpdeskSlaPolicyCalculationService.CalculateSlaPolicyAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var input1 = new ApiItsm.ListIncidentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiItsm.ListIncidentsRequest.Types.Filter
            {
                CreatedAfter = (reply0.SlaPolicy?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (reply0.SlaPolicy?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply0.SlaPolicy?.Owner?.UserId ?? ""),
                Region = (request.Details?.Destination?.Region ?? ""),
            },
        };
        var labels1 = input1.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input1.Context.Baggage.Clear();
        input1.Context.Baggage.Add(labels1);
        using var rpc1 = _itsmIncidentService.ListIncidentsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiHelpdesk.CalculateEscalationResponse
        {
            Escalation = new ApiHelpdesk.Escalation
            {
                CustomFields = { (request.Escalation?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                EscalationId = (request.EscalationId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "escalation",
            "audit", "CalculateEscalation", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Compares an escalation.
    /// </summary>
    public override async Task<ApiHelpdesk.CompareEscalationOptionsResponse> CompareEscalationOptions(ApiHelpdesk.CompareEscalationOptionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CompareEscalationOptions for {Tenant}", requestContext.TenantId);
        if (request.Escalation is { } record)
        {
            var snapshot = EscalationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.SimulateKnowledgeArticleRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.SimulateKnowledgeArticleRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Locality = (request.Details?.Destination?.Locality ?? ""),
                    PostalCode = (request.Details?.Destination?.PostalCode ?? ""),
                },
            },
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Details?.Destination?.CountryCode ?? ""),
                    Lines = { (request.Details?.Destination?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiHelpdesk.SimulateKnowledgeArticleRequest>();
        using var rpc0 = _helpdeskKnowledgeArticleCalculationService.SimulateKnowledgeArticleAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.ValidateEntitlementRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.KnowledgeArticle?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.Escalation?.Assignee?.UserId ?? ""),
                },
            },
        };
        using var rpc1 = _licensingEntitlementValidationService.ValidateEntitlementAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        var input2 = new ApiHelpdesk.EstimateTicketRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Ticket = new ApiHelpdesk.Ticket
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Escalation?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
        };
        using var rpc2 = _helpdeskTicketCalculationService.EstimateTicketAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        await Task.WhenAll(pending1, pending2);
        var reply1 = await pending1;
        var reply2 = await pending2;
        var input3 = new ApiHelpdesk.ExplainKnowledgeArticleDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.ExplainKnowledgeArticleDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply2.Ticket?.Assignee?.Email ?? ""),
                },
            },
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply2.Ticket?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (reply2.Ticket?.Requester?.Email ?? ""),
                },
            },
        };
        using var rpc3 = _helpdeskKnowledgeArticleValidationService.ExplainKnowledgeArticleDecisionAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiHelpdesk.CompareEscalationOptionsResponse
        {
            Escalation = new ApiHelpdesk.Escalation
            {
                Assignee = (request.Escalation?.Assignee ?? new ApiIdentity.UserRef()),
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply3.KnowledgeArticle?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedBy = (reply0.KnowledgeArticle?.Audit?.UpdatedBy ?? ""),
                },
                EscalationId = (request.EscalationId ?? ""),
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (reply2.Ticket?.Requester?.Email ?? ""),
                    PreferredChannel = (reply2.Ticket?.Requester?.PreferredChannel ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Message = (reply1.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "escalation",
            "audit", "CompareEscalationOptions", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Estimates an escalation.
    /// </summary>
    public override async Task<ApiHelpdesk.EstimateEscalationResponse> EstimateEscalation(ApiHelpdesk.EstimateEscalationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling EstimateEscalation for {Tenant}", requestContext.TenantId);
        if (request.Escalation is { } record)
        {
            var snapshot = EscalationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiFuel.QuoteFuelCardRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiFuel.QuoteFuelCardRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Details?.Destination?.CountryCode ?? ""),
                },
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _fuelFuelCardCalculationService.QuoteFuelCardAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiVenues.ScoreVenueRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Venue = new ApiVenues.Venue
            {
                Address = new ApiCommon.PostalAddress
                {
                    Locality = (request.Details?.Destination?.Locality ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Escalation?.Audit?.CreatedBy ?? ""),
                    UpdatedBy = (request.Escalation?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        var reply1 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _venuesVenueValidationService.ScoreVenueAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input2 = new ApiHelpdesk.ExplainSlaPolicyCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.ExplainSlaPolicyCalculationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Escalation?.Requester?.Phone ?? ""),
                },
            },
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.Venue?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedBy = (reply0.FuelCard?.Audit?.UpdatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply0.FuelCard?.Holder?.Email ?? ""),
                },
            },
        };
        var reply2 = await Workflow.SendAsync(input2, token, _helpdeskSlaPolicyCalculationService.ExplainSlaPolicyCalculationAsync);
        var response = new ApiHelpdesk.EstimateEscalationResponse
        {
            Escalation = new ApiHelpdesk.Escalation
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.FuelCard?.Audit?.Etag ?? ""),
                },
                Comments = { new ApiHelpdesk.EscalationComment
                {
                    CreatedAt = (reply1.Venue?.Audit?.CreatedAt ?? new Timestamp()),
                } },
                EscalationId = (request.EscalationId ?? ""),
                Subject = (request.Escalation?.Subject ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply2.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "escalation",
            "audit", "EstimateEscalation", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains an escalation.
    /// </summary>
    public override async Task<ApiHelpdesk.ExplainEscalationCalculationResponse> ExplainEscalationCalculation(ApiHelpdesk.ExplainEscalationCalculationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ExplainEscalationCalculation for {Tenant}", requestContext.TenantId);
        if (request.Escalation is { } record)
        {
            var snapshot = EscalationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.ValidateChangeRequestRequest
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Comments = { new ApiItsm.ChangeRequestComment
                {
                    CreatedAt = (request.Escalation?.Audit?.CreatedAt ?? new Timestamp()),
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _sendExplainEscalationCalculation0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiHelpdesk.ExplainKnowledgeArticleCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Escalation?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (request.Escalation?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.Escalation?.Assignee?.DisplayName ?? ""),
                    Email = (request.Escalation?.Assignee?.Email ?? ""),
                },
            },
        };
        var named1 = _registry.Get<ApiHelpdesk.KnowledgeArticleCalculationService.KnowledgeArticleCalculationServiceClient>("acme.helpdesk.v1.KnowledgeArticleCalculationService");
        using var rpc1 = named1.ExplainKnowledgeArticleCalculationAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiHelpdesk.CompareSlaPolicyOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.ChangeRequest?.Audit?.Etag ?? ""),
                    UpdatedAt = (reply1.KnowledgeArticle?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        var descriptor2 = new Method<ApiHelpdesk.CompareSlaPolicyOptionsRequest, ApiHelpdesk.CompareSlaPolicyOptionsResponse>(
            MethodType.Unary, "acme.helpdesk.v1" + ".SlaPolicyCalculationService", "CompareSlaPolicyOptions",
            Wire.Marshaller<ApiHelpdesk.CompareSlaPolicyOptionsRequest>(), Wire.Marshaller<ApiHelpdesk.CompareSlaPolicyOptionsResponse>());
        using var rpc2 = _invoker.AsyncUnaryCall(descriptor2, null,
            new CallOptions(cancellationToken: token), input2);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiHelpdesk.ExplainEscalationCalculationResponse
        {
            Escalation = new ApiHelpdesk.Escalation
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply2.SlaPolicy?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Comments = { new ApiHelpdesk.EscalationComment
                {
                    CreatedAt = (reply1.KnowledgeArticle?.Audit?.CreatedAt ?? new Timestamp()),
                } },
                EscalationId = (request.EscalationId ?? ""),
                Priority = (request.Escalation?.Priority ?? 0),
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (reply1.KnowledgeArticle?.Counterparty?.Email ?? ""),
                },
                Sla = new ApiHelpdesk.EscalationSla
                {
                    Breached = (reply0.ChangeRequest?.Sla?.Breached ?? false),
                },
            },
            Outcome = new ApiHelpdesk.ExplainEscalationCalculationResponse.Types.Outcome
            {
                Status = (reply2.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "escalation",
            "audit", "ExplainEscalationCalculation", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an escalation.
    /// </summary>
    public override async Task<ApiHelpdesk.GetEscalationRatesResponse> GetEscalationRates(ApiHelpdesk.GetEscalationRatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.EscalationId, "escalation_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetEscalationRates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "escalation", request.EscalationId,
            "GetEscalationRates", "accepted", token);
        var input0 = new ApiDocuments.SubscribeDocumentFolderUpdatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.SubscribeDocumentFolderUpdatesRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Variables = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        using var rpc0 = _documentsDocumentFolderNotificationService.SubscribeDocumentFolderUpdatesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiHelpdesk.GetEscalationRatesResponse
        {
            Escalation = new ApiHelpdesk.Escalation
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.DocumentFolder?.Audit?.Etag ?? ""),
                },
                EscalationId = (request.EscalationId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Quotes an escalation.
    /// </summary>
    public override async Task<ApiHelpdesk.QuoteEscalationResponse> QuoteEscalation(ApiHelpdesk.QuoteEscalationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling QuoteEscalation for {Tenant}", requestContext.TenantId);
        if (request.Escalation is { } record)
        {
            var snapshot = EscalationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var batch0 = (request.Escalation?.History ?? new Google.Protobuf.Collections.RepeatedField<ApiHelpdesk.EscalationStatusChange>()).Select(async item =>
        {
            var input0 = new ApiHelpdesk.ValidateTicketRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Ticket = new ApiHelpdesk.Ticket
                {
                    History = { new ApiHelpdesk.TicketStatusChange
                    {
                        ChangedAt = (item.ChangedAt ?? new Timestamp()),
                    } },
                },
            };
            using var rpc0 = _helpdeskTicketValidationService.ValidateTicketAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            return reply0;
        }).ToArray();
        var replies0 = await Task.WhenAll(batch0);
        var reply0 = replies0.LastOrDefault() ?? new ApiHelpdesk.ValidateTicketResponse();
        var reply1 = new ApiSearch.FindSearchIndexesResponse();
        foreach (var item in (request.Escalation?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiHelpdesk.EscalationComment>()))
        {
            var input1 = new ApiSearch.FindSearchIndexesRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiSearch.FindSearchIndexesRequest.Types.Filter
                {
                    CreatedAfter = (item.CreatedAt ?? new Timestamp()),
                    Labels = { (reply0.Ticket?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                    OwnerId = (item.CommentId ?? ""),
                    Region = (request.Details?.Destination?.Region ?? ""),
                },
            };
            using var rpc1 = _searchSearchIndexQueryService.FindSearchIndexesAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var input2 = request.QuoteEscalationToPreviewServiceAsset2(reply0);
        using var rpc2 = _itsmServiceAssetValidationService.PreviewServiceAssetAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiHelpdesk.RecalculateKnowledgeArticlesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                Lines = { new ApiHelpdesk.KnowledgeArticleLine
                {
                    Quantity = (reply2.ServiceAsset?.Components?.GetValueOrDefault("primary")?.Quantity ?? 0L),
                } },
            },
        };
        var reply3 = await SendQuoteEscalation3Async(input3, token);
        var response = new ApiHelpdesk.QuoteEscalationResponse
        {
            Escalation = new ApiHelpdesk.Escalation
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply2.ServiceAsset?.Audit?.CreatedAt ?? new Timestamp()),
                },
                EscalationId = (request.EscalationId ?? ""),
                Status = (request.Escalation?.Status ?? (ApiHelpdesk.EscalationStatus)0),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply3.Result?.CorrelationId ?? ""),
                Details = { (reply3.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply2.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "escalation",
            "audit", "QuoteEscalation", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalculates an escalation.
    /// </summary>
    public override async Task<ApiHelpdesk.RecalculateEscalationsResponse> RecalculateEscalations(ApiHelpdesk.RecalculateEscalationsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RecalculateEscalations for {Tenant}", requestContext.TenantId);
        if (request.Escalation is { } record)
        {
            var snapshot = EscalationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.SubscribeDocumentUpdatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.SubscribeDocumentUpdatesRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.Escalation?.Requester?.Email ?? ""),
                    Phone = (request.Escalation?.Requester?.Phone ?? ""),
                    PreferredChannel = (request.Escalation?.Requester?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc0 = _documentsDocumentNotificationService.SubscribeDocumentUpdatesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiSearch.FindSearchIndexesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiSearch.FindSearchIndexesRequest.Types.Filter
            {
                CreatedAfter = (request.Escalation?.Audit?.CreatedAt ?? new Timestamp()),
                Region = (reply0.Document?.BillingAddress?.Region ?? ""),
            },
        };
        var packed1 = Any.Pack(input1);
        input1 = packed1.Unpack<ApiSearch.FindSearchIndexesRequest>();
        using var rpc1 = _searchSearchIndexQueryService.FindSearchIndexesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiHelpdesk.RecalculateEscalationsResponse
        {
            Escalation = new ApiHelpdesk.Escalation
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Document?.Audit?.CreatedAt ?? new Timestamp()),
                    Etag = (reply0.Document?.Audit?.Etag ?? ""),
                },
                EscalationId = (request.EscalationId ?? ""),
                Priority = (request.Escalation?.Priority ?? 0),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "escalation",
            "audit", "RecalculateEscalations", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Simulates an escalation.
    /// </summary>
    public override async Task<ApiHelpdesk.SimulateEscalationResponse> SimulateEscalation(ApiHelpdesk.SimulateEscalationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SimulateEscalation for {Tenant}", requestContext.TenantId);
        if (request.Escalation is { } record)
        {
            var snapshot = EscalationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.EstimateKnowledgeArticleRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.EstimateKnowledgeArticleRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Recipient = (request.Details?.Destination?.Recipient ?? ""),
                },
            },
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (request.Details?.Destination?.Locality ?? ""),
                },
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiHelpdesk.EstimateKnowledgeArticleRequest>(json0);
        using var rpc0 = _helpdeskKnowledgeArticleCalculationService.EstimateKnowledgeArticleAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiVenues.LintVenuesRequest();
        input1.Context = (request.Context ?? new ApiCommon.RequestContext());
        input1.Venue = new ApiVenues.Venue
        {
            Address = new ApiCommon.PostalAddress
            {
                PostalCode = (request.Details?.Destination?.PostalCode ?? ""),
            },
            Audit = new ApiCommon.AuditInfo
            {
                Etag = (request.Escalation?.Audit?.Etag ?? ""),
            },
            Owner = new ApiIdentity.UserRef
            {
                Email = (reply0.KnowledgeArticle?.Counterparty?.Email ?? ""),
            },
        };
        using var rpc1 = _venuesVenueValidationService.LintVenuesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiHelpdesk.SimulateEscalationResponse
        {
            Breakdown = new ApiHelpdesk.SimulateEscalationResponse.Types.Breakdown
            {
                CurrencyCode = (reply0.Breakdown?.CurrencyCode ?? ""),
            },
            Escalation = new ApiHelpdesk.Escalation
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.KnowledgeArticle?.Counterparty?.DisplayName ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.Venue?.Audit?.Etag ?? ""),
                    UpdatedBy = (reply1.Venue?.Audit?.UpdatedBy ?? ""),
                },
                CustomFields = { (request.Escalation?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                EscalationId = (request.EscalationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "escalation",
            "audit", "SimulateEscalation", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiHelpdesk.RecalculateKnowledgeArticlesResponse> SendQuoteEscalation3Async(ApiHelpdesk.RecalculateKnowledgeArticlesRequest request, CancellationToken token)
    {
        return await _escalationCalculationEndpointSendQuoteEscalation3Port.SendAsync(request, token);
    }
}
