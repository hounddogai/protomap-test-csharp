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
using ApiDocuments = Acme.Documents.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;

namespace Acme.Workforce.Corporateevents;

public sealed partial class CorporateEventQueryEndpoint
{
    /// <summary>
    /// Lists corporate events a page at a time.
    /// </summary>
    public override async Task<ApiCorporateevents.BrowseCorporateEventsResponse> BrowseCorporateEvents(ApiCorporateevents.BrowseCorporateEventsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling BrowseCorporateEvents for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event", "batch",
            "BrowseCorporateEvents", "accepted", token);
        var profile0 = new MappingProfile<ApiCorporateevents.BrowseCorporateEventsRequest, ApiCorporateevents.CalculateRegistrationRequest>(value => new ApiCorporateevents.CalculateRegistrationRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.CalculateRegistrationRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (value.Filter?.Region ?? ""),
                },
            },
            Registration = new ApiCorporateevents.Registration
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (value.Context?.Actor?.Email ?? ""),
                },
                Labels = { (value.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _corporateeventsRegistrationCalculationService.CalculateRegistrationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiCorporateevents.BrowseCorporateEventsResponse
        {
            CorporateEvents = { new ApiCorporateevents.CorporateEvent
            {
                Participants = { new ApiCorporateevents.CorporateEventParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        PreferredChannel = (reply0.Registration?.Contact?.PreferredChannel ?? ""),
                    },
                } },
            } },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event",
            "audit", "BrowseCorporateEvents", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Counts corporate events by status.
    /// </summary>
    public override async Task<ApiCorporateevents.CountCorporateEventsResponse> CountCorporateEvents(ApiCorporateevents.CountCorporateEventsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CountCorporateEvents for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event", "batch",
            "CountCorporateEvents", "accepted", token);
        var response = new ApiCorporateevents.CountCorporateEventsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event",
            "audit", "CountCorporateEvents", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams every matching corporate event.
    /// </summary>
    public override async Task ExportCorporateEvents(ApiCorporateevents.ExportCorporateEventsRequest request, IServerStreamWriter<ApiCorporateevents.ExportCorporateEventsResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExportCorporateEvents for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event", "batch",
            "ExportCorporateEvents", "accepted", token);
        var input0 = new ApiCorporateevents.CompareVenueOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.CompareVenueOptionsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _corporateeventsVenueCalculationService.CompareVenueOptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiHelpdesk.CompareEscalationOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.CompareEscalationOptionsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply0.Venue ?? new ApiCorporateevents.Venue()).LocatorCase switch
                    {
                        ApiCorporateevents.Venue.LocatorOneofCase.Address => (reply0.Venue?.Address?.CountryCode ?? ""),
                        _ => ""
                    },
                    Lines = { (reply0.Venue ?? new ApiCorporateevents.Venue()).LocatorCase switch
                    {
                        ApiCorporateevents.Venue.LocatorOneofCase.Address => (reply0.Venue?.Address?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()),
                        _ => new Google.Protobuf.Collections.RepeatedField<string>()
                    } },
                },
            },
            Escalation = new ApiHelpdesk.Escalation
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.Venue?.Audit?.UpdatedAt ?? new Timestamp()),
                    UpdatedBy = (reply0.Venue?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        using var rpc1 = _helpdeskEscalationCalculationService.CompareEscalationOptionsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiCorporateevents.EstimateVenueRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Venue = new ApiCorporateevents.Venue
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.Escalation?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedAt = (reply0.Venue?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        var reply2 = await SendExportCorporateEvents2Async(input2, token);
        var input3 = new ApiCorporateevents.ExplainVenueDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Venue = new ApiCorporateevents.Venue
            {
                Address = new ApiCommon.PostalAddress
                {
                    Locality = (reply0.Venue ?? new ApiCorporateevents.Venue()).LocatorCase switch
                    {
                        ApiCorporateevents.Venue.LocatorOneofCase.Address => (reply0.Venue?.Address?.Locality ?? ""),
                        _ => ""
                    },
                },
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply1.Escalation?.Audit?.UpdatedBy ?? ""),
                },
                Components = { { "primary", new ApiCorporateevents.VenueComponent
                {
                    ComponentId = (reply2.Venue?.Components?.GetValueOrDefault("primary")?.ComponentId ?? ""),
                } } },
            },
        };
        using var rpc3 = _corporateeventsVenueValidationService.ExplainVenueDecisionAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiCorporateevents.ExportCorporateEventsResponse
        {
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Location = new ApiCommon.PostalAddress
                {
                    Recipient = (reply0.Venue?.Address?.Recipient ?? ""),
                    Region = (reply2.Venue?.Address?.Region ?? ""),
                },
                Participants = { new ApiCorporateevents.CorporateEventParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (reply2.Venue?.Owner?.Email ?? ""),
                        Phone = (reply1.Escalation?.Requester?.Phone ?? ""),
                        PreferredChannel = (reply1.Escalation?.Requester?.PreferredChannel ?? ""),
                    },
                } },
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply3.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Format ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event",
            "audit", "ExportCorporateEvents", (request.Format ?? ""), token);
        await responses.WriteAsync(response, token);
    }
    /// <summary>
    /// Lists corporate events a page at a time.
    /// </summary>
    public override async Task<ApiCorporateevents.FindCorporateEventsResponse> FindCorporateEvents(ApiCorporateevents.FindCorporateEventsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling FindCorporateEvents for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event", "batch",
            "FindCorporateEvents", "accepted", token);
        var input0 = new ApiItsm.ExportIncidentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiItsm.ExportIncidentsRequest.Types.Filter
            {
                Query = (request.Filter?.Query ?? ""),
            },
        };
        using var stream0 = _itsmIncidentQueryService.ExportIncidents(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportIncidents frame {Type}", change0.GetType().Name);
        }
        var response = new ApiCorporateevents.FindCorporateEventsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event",
            "audit", "FindCorporateEvents", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a corporate event.
    /// </summary>
    public override async Task<ApiCorporateevents.LookupCorporateEventResponse> LookupCorporateEvent(ApiCorporateevents.LookupCorporateEventRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.CorporateEventId, "corporate_event_id");
        _logger.LogInformation("Handling LookupCorporateEvent for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event", request.CorporateEventId,
            "LookupCorporateEvent", "accepted", token);
        var input0 = new ApiCorporateevents.CompareVenueOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Venue = new ApiCorporateevents.Venue
            {
                Components = { { "primary", new ApiCorporateevents.VenueComponent
                {
                    ComponentId = (request.CorporateEventId ?? ""),
                } } },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiCorporateevents.CompareVenueOptionsRequest>(json0);
        using var rpc0 = _corporateeventsVenueCalculationService.CompareVenueOptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiCorporateevents.LookupCorporateEventResponse
        {
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                CorporateEventId = (request.CorporateEventId ?? ""),
                Organizer = new ApiIdentity.UserRef
                {
                    UserId = (reply0.Venue?.Owner?.UserId ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        return response;
    }
    /// <summary>
    /// Returns a corporate event.
    /// </summary>
    public override async Task<ApiCorporateevents.PeekCorporateEventResponse> PeekCorporateEvent(ApiCorporateevents.PeekCorporateEventRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.CorporateEventId, "corporate_event_id");
        _logger.LogInformation("Handling PeekCorporateEvent for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event", request.CorporateEventId,
            "PeekCorporateEvent", "accepted", token);
        var response = new ApiCorporateevents.PeekCorporateEventResponse
        {
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                CorporateEventId = (request.CorporateEventId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Returns several corporate events at once.
    /// </summary>
    public override async Task<ApiCorporateevents.ResolveCorporateEventsResponse> ResolveCorporateEvents(ApiCorporateevents.ResolveCorporateEventsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ResolveCorporateEvents for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event", "batch",
            "ResolveCorporateEvents", "accepted", token);
        var input0 = new ApiCorporateevents.ExplainVenueDecisionRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Venue = new ApiCorporateevents.Venue
        {
            Owner = new ApiIdentity.UserRef
            {
                UserId = (request.Context?.Actor?.UserId ?? ""),
            },
        };
        using var rpc0 = _corporateeventsVenueValidationService.ExplainVenueDecisionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.ReconcileSignaturesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ReconcileSignaturesRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Signature = new ApiDocuments.Signature
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Measurements = { new ApiDocuments.SignatureMeasurement
                {
                    Unit = (reply0.Venue?.Dimensions?.Unit ?? ""),
                } },
                Subject = new ApiDocuments.DocumentRef
                {
                    DisplayName = (reply0.Venue?.Owner?.DisplayName ?? ""),
                },
            },
        };
        using var rpc1 = _documentsSignatureSyncService.ReconcileSignaturesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiCorporateevents.ResolveCorporateEventsResponse
        {
            CorporateEvents = { new ApiCorporateevents.CorporateEvent
            {
                Organizer = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.Signature?.Subject?.DisplayName ?? ""),
                    Email = (reply0.Venue?.Owner?.Email ?? ""),
                },
            } },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists corporate events a page at a time.
    /// </summary>
    public override async Task<ApiCorporateevents.SuggestCorporateEventsResponse> SuggestCorporateEvents(ApiCorporateevents.SuggestCorporateEventsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SuggestCorporateEvents for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event", "batch",
            "SuggestCorporateEvents", "accepted", token);
        var input0 = new ApiDocuments.ListDocumentTemplateExportsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListDocumentTemplateExportsRequest.Types.Filter
            {
                CreatedAfter = (request.Filter?.CreatedAfter ?? new Timestamp()),
                Region = (request.Filter?.Region ?? ""),
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _documentsDocumentTemplateExportService.ListDocumentTemplateExportsAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiCorporateevents.SuggestCorporateEventsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event",
            "audit", "SuggestCorporateEvents", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Summarizes a corporate event.
    /// </summary>
    public override async Task<ApiCorporateevents.SummarizeCorporateEventsResponse> SummarizeCorporateEvents(ApiCorporateevents.SummarizeCorporateEventsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SummarizeCorporateEvents for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event", request.CorporateEventId,
            "SummarizeCorporateEvents", "accepted", token);
        var input0 = new ApiCorporateevents.QuoteVenueRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Reason ?? ""),
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _corporateeventsVenueCalculationService.QuoteVenueAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiCorporateevents.SummarizeCorporateEventsResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.Format ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event",
            "audit", "SummarizeCorporateEvents", (request.Details?.Format ?? ""), token);
        return response;
    }
    private async Task<ApiCorporateevents.EstimateVenueResponse> SendExportCorporateEvents2Async(ApiCorporateevents.EstimateVenueRequest request, CancellationToken token)
    {
        return await _corporateEventQueryEndpointSendExportCorporateEvents2Port.SendAsync(request, token);
    }
}
