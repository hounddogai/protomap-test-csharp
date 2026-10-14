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
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDatasets = Acme.Datasets.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;

namespace Acme.Workforce.Corporateevents;

/// <summary>
/// Answers read-only questions about registrations, for screens and reports.
/// </summary>
public sealed class RegistrationQueryEndpoint : ApiCorporateevents.RegistrationQueryService.RegistrationQueryServiceBase
{
    private readonly ApiCorporateevents.CorporateEventCalculationService.CorporateEventCalculationServiceClient _corporateeventsCorporateEventCalculationService;
    private readonly ApiCorporateevents.CorporateEventQueryService.CorporateEventQueryServiceClient _corporateeventsCorporateEventQueryService;
    private readonly ApiCorporateevents.RegistrationValidationService.RegistrationValidationServiceClient _corporateeventsRegistrationValidationService;
    private readonly ApiDatasets.DatasetQueryService.DatasetQueryServiceClient _datasetsDatasetQueryService;
    private readonly ApiDocuments.SignatureExportService.SignatureExportServiceClient _documentsSignatureExportService;
    private readonly ILogger<RegistrationQueryEndpoint> _logger;
    private readonly IRegistrationQueryEndpointSendPeekRegistration0Port _registrationQueryEndpointSendPeekRegistration0Port;
    private readonly IRecordRepository _repository;

    public RegistrationQueryEndpoint(
        ApiCorporateevents.CorporateEventCalculationService.CorporateEventCalculationServiceClient corporateeventsCorporateEventCalculationService,
        ApiCorporateevents.CorporateEventQueryService.CorporateEventQueryServiceClient corporateeventsCorporateEventQueryService,
        ApiCorporateevents.RegistrationValidationService.RegistrationValidationServiceClient corporateeventsRegistrationValidationService,
        ApiDatasets.DatasetQueryService.DatasetQueryServiceClient datasetsDatasetQueryService,
        ApiDocuments.SignatureExportService.SignatureExportServiceClient documentsSignatureExportService,
        ILogger<RegistrationQueryEndpoint> logger,
        IRegistrationQueryEndpointSendPeekRegistration0Port registrationQueryEndpointSendPeekRegistration0PortAdapter,
        IRecordRepository repository)
    {
        _corporateeventsCorporateEventCalculationService = corporateeventsCorporateEventCalculationService;
        _corporateeventsCorporateEventQueryService = corporateeventsCorporateEventQueryService;
        _corporateeventsRegistrationValidationService = corporateeventsRegistrationValidationService;
        _datasetsDatasetQueryService = datasetsDatasetQueryService;
        _documentsSignatureExportService = documentsSignatureExportService;
        _logger = logger;
        _registrationQueryEndpointSendPeekRegistration0Port = registrationQueryEndpointSendPeekRegistration0PortAdapter;
        _repository = repository;
    }
    /// <summary>
    /// Lists registrations a page at a time.
    /// </summary>
    public override async Task<ApiCorporateevents.BrowseRegistrationsResponse> BrowseRegistrations(ApiCorporateevents.BrowseRegistrationsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling BrowseRegistrations for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "registration", "batch",
            "BrowseRegistrations", "accepted", token);
        var response = new ApiCorporateevents.BrowseRegistrationsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "registration",
            "audit", "BrowseRegistrations", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Counts registrations by status.
    /// </summary>
    public override async Task<ApiCorporateevents.CountRegistrationsResponse> CountRegistrations(ApiCorporateevents.CountRegistrationsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CountRegistrations for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "registration", "batch",
            "CountRegistrations", "accepted", token);
        var response = new ApiCorporateevents.CountRegistrationsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "registration",
            "audit", "CountRegistrations", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams every matching registration.
    /// </summary>
    public override async Task ExportRegistrations(ApiCorporateevents.ExportRegistrationsRequest request, IServerStreamWriter<ApiCorporateevents.ExportRegistrationsResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ExportRegistrations for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "registration", "batch",
            "ExportRegistrations", "accepted", token);
        var input0 = new ApiDocuments.ListSignatureExportsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListSignatureExportsRequest.Types.Filter
            {
                CreatedAfter = (request.Filter?.CreatedAfter ?? new Timestamp()),
                Labels = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Filter?.OwnerId ?? ""),
                Query = (request.Filter?.Query ?? ""),
            },
        };
        using var rpc0 = _documentsSignatureExportService.ListSignatureExportsAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiCorporateevents.RecalculateCorporateEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Location = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
                Organizer = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _corporateeventsCorporateEventCalculationService.RecalculateCorporateEventsAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var response = new ApiCorporateevents.ExportRegistrationsResponse
        {
            Registration = new ApiCorporateevents.Registration
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply1.CorporateEvent?.Audit?.UpdatedBy ?? ""),
                },
                CurrencyCode = (reply1.Breakdown?.CurrencyCode ?? ""),
                Labels = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Format ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "registration",
            "audit", "ExportRegistrations", (request.Format ?? ""), token);
        await responses.WriteAsync(response, token);
    }
    /// <summary>
    /// Lists registrations a page at a time.
    /// </summary>
    public override async Task<ApiCorporateevents.FindRegistrationsResponse> FindRegistrations(ApiCorporateevents.FindRegistrationsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling FindRegistrations for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "registration", "batch",
            "FindRegistrations", "accepted", token);
        var response = new ApiCorporateevents.FindRegistrationsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "registration",
            "audit", "FindRegistrations", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a registration.
    /// </summary>
    public override async Task<ApiCorporateevents.LookupRegistrationResponse> LookupRegistration(ApiCorporateevents.LookupRegistrationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.RegistrationId, "registration_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling LookupRegistration for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "registration", request.RegistrationId,
            "LookupRegistration", "accepted", token);
        var response = new ApiCorporateevents.LookupRegistrationResponse
        {
            Registration = new ApiCorporateevents.Registration
            {
                RegistrationId = (request.RegistrationId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Returns a registration.
    /// </summary>
    public override async Task<ApiCorporateevents.PeekRegistrationResponse> PeekRegistration(ApiCorporateevents.PeekRegistrationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.RegistrationId, "registration_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling PeekRegistration for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "registration", request.RegistrationId,
            "PeekRegistration", "accepted", token);
        var input0 = new ApiCorporateevents.ExplainRegistrationDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.ExplainRegistrationDecisionRequest.Types.Details
            {
                Locale = (request.Context?.Locale ?? ""),
            },
            Registration = new ApiCorporateevents.Registration
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Holder = new ApiCorporateevents.CorporateEventRef
                {
                    StartsAt = (request.Context?.Deadline ?? new Timestamp()),
                },
                RegistrationId = (request.RegistrationId ?? ""),
            },
        };
        var reply0 = await SendPeekRegistration0Async(input0, token);
        var response = new ApiCorporateevents.PeekRegistrationResponse
        {
            Registration = new ApiCorporateevents.Registration
            {
                Balance = (reply0.Registration?.Balance ?? new ApiCommon.Money()),
                RegistrationId = (request.RegistrationId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Returns several registrations at once.
    /// </summary>
    public override async Task<ApiCorporateevents.ResolveRegistrationsResponse> ResolveRegistrations(ApiCorporateevents.ResolveRegistrationsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ResolveRegistrations for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "registration", "batch",
            "ResolveRegistrations", "accepted", token);
        var input0 = new ApiDatasets.CountDatasetsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDatasets.CountDatasetsRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
                Labels = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Context?.TenantId ?? ""),
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _datasetsDatasetQueryService.CountDatasetsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiCorporateevents.ResolveRegistrationsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists registrations a page at a time.
    /// </summary>
    public override async Task<ApiCorporateevents.SuggestRegistrationsResponse> SuggestRegistrations(ApiCorporateevents.SuggestRegistrationsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SuggestRegistrations for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "registration", "batch",
            "SuggestRegistrations", "accepted", token);
        var response = new ApiCorporateevents.SuggestRegistrationsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "registration",
            "audit", "SuggestRegistrations", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Summarizes a registration.
    /// </summary>
    public override async Task<ApiCorporateevents.SummarizeRegistrationsResponse> SummarizeRegistrations(ApiCorporateevents.SummarizeRegistrationsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SummarizeRegistrations for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "registration", request.RegistrationId,
            "SummarizeRegistrations", "accepted", token);
        var input0 = new ApiCorporateevents.SuggestCorporateEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiCorporateevents.SuggestCorporateEventsRequest.Types.Filter
            {
                Labels = { (request.Details?.Filters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.RegistrationId ?? ""),
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiCorporateevents.SuggestCorporateEventsRequest>();
        using var rpc0 = _corporateeventsCorporateEventQueryService.SuggestCorporateEventsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiCorporateevents.LintRegistrationsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Registration = new ApiCorporateevents.Registration
            {
                RegistrationId = (request.RegistrationId ?? ""),
            },
        };
        var json1 = JsonFormatter.Default.Format(input1);
        input1 = JsonParser.Default.Parse<ApiCorporateevents.LintRegistrationsRequest>(json1);
        using var rpc1 = _corporateeventsRegistrationValidationService.LintRegistrationsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiCorporateevents.SummarizeRegistrationsResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "registration",
            "audit", "SummarizeRegistrations", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiCorporateevents.ExplainRegistrationDecisionResponse> SendPeekRegistration0Async(ApiCorporateevents.ExplainRegistrationDecisionRequest request, CancellationToken token)
    {
        return await _registrationQueryEndpointSendPeekRegistration0Port.SendAsync(request, token);
    }
}
