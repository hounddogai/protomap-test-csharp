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
using ApiDeliveries = Acme.Deliveries.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDispatch = Acme.Dispatch.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSearch = Acme.Search.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Corporateevents;

public sealed partial class CorporateEventValidationEndpoint
{
    /// <summary>
    /// Checks a corporate event.
    /// </summary>
    public override async Task<ApiCorporateevents.CheckCorporateEventEligibilityResponse> CheckCorporateEventEligibility(ApiCorporateevents.CheckCorporateEventEligibilityRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CheckCorporateEventEligibility for {Tenant}", requestContext.TenantId);
        if (request.CorporateEvent is { } record)
        {
            var snapshot = CorporateEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiSearch.FindSearchIndexesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiSearch.FindSearchIndexesRequest.Types.Filter
            {
                Region = (request.CorporateEvent?.Location?.Region ?? ""),
            },
        };
        var descriptor0 = new Method<ApiSearch.FindSearchIndexesRequest, ApiSearch.FindSearchIndexesResponse>(
            MethodType.Unary, "acme.search.v1" + ".SearchIndexQueryService", "FindSearchIndexes",
            Wire.Marshaller<ApiSearch.FindSearchIndexesRequest>(), Wire.Marshaller<ApiSearch.FindSearchIndexesResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiCorporateevents.ExplainRegistrationCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.ExplainRegistrationCalculationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.Contact?.Phone ?? ""),
                },
            },
        };
        var command1 = new DispatchCommand<ApiCorporateevents.ExplainRegistrationCalculationRequest>(input1,
            "primary", requestContext.TenantId);
        var routed1 = command1 with { Region = _settings.Value.Region };
        using var rpc1 = _corporateeventsRegistrationCalculationService.ExplainRegistrationCalculationAsync(routed1.Payload, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiCorporateevents.CheckCorporateEventEligibilityResponse
        {
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                CorporateEventId = (request.CorporateEventId ?? ""),
                Metadata = { (request.CorporateEvent?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Participants = { new ApiCorporateevents.CorporateEventParticipant
                {
                    Contact = (reply1.Registration?.Contact ?? new ApiCommon.ContactInfo()),
                } },
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event",
            "audit", "CheckCorporateEventEligibility", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a corporate event.
    /// </summary>
    public override async Task<ApiCorporateevents.ExplainCorporateEventDecisionResponse> ExplainCorporateEventDecision(ApiCorporateevents.ExplainCorporateEventDecisionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExplainCorporateEventDecision for {Tenant}", requestContext.TenantId);
        if (request.CorporateEvent is { } record)
        {
            var snapshot = CorporateEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var profile0 = new MappingProfile<ApiCorporateevents.ExplainCorporateEventDecisionRequest, ApiVenues.CheckVenueEligibilityRequest>(value => new ApiVenues.CheckVenueEligibilityRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiVenues.CheckVenueEligibilityRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (value.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
                },
            },
            Venue = new ApiVenues.Venue
            {
                Address = new ApiCommon.PostalAddress
                {
                    Region = (value.CorporateEvent?.Location?.Region ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (value.CorporateEvent?.Audit?.Etag ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (value.CorporateEvent?.Organizer?.UserId ?? ""),
                },
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _venuesVenueValidationService.CheckVenueEligibilityAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiCorporateevents.CalculateCorporateEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.Venue?.Audit?.UpdatedBy ?? ""),
                },
                Participants = { new ApiCorporateevents.CorporateEventParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (reply0.Venue?.Owner?.Email ?? ""),
                        Phone = (request.Details?.Contact?.Phone ?? ""),
                    },
                } },
            },
            Details = new ApiCorporateevents.CalculateCorporateEventRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply0.Venue ?? new ApiVenues.Venue()).LocatorCase switch
                    {
                        ApiVenues.Venue.LocatorOneofCase.Address => (reply0.Venue?.Address?.CountryCode ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _corporateeventsCorporateEventCalculationService.CalculateCorporateEventAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiCorporateevents.ExplainCorporateEventDecisionResponse
        {
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.CorporateEvent?.Audit?.CreatedAt ?? new Timestamp()),
                },
                CorporateEventId = (request.CorporateEventId ?? ""),
                Location = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply0.Venue?.Address?.PostalCode ?? ""),
                },
                Window = (request.CorporateEvent?.Window ?? new ApiCommon.TimeWindow()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event",
            "audit", "ExplainCorporateEventDecision", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a corporate event.
    /// </summary>
    public override async Task<ApiCorporateevents.GetCorporateEventRulesResponse> GetCorporateEventRules(ApiCorporateevents.GetCorporateEventRulesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.CorporateEventId, "corporate_event_id");
        _logger.LogInformation("Handling GetCorporateEventRules for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event", request.CorporateEventId,
            "GetCorporateEventRules", "accepted", token);
        var input0 = request.GetCorporateEventRulesToSimulateCorporateEvent0();
        using var rpc0 = _corporateeventsCorporateEventCalculationService.SimulateCorporateEventAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDispatch.CountDispatchOrdersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDispatch.CountDispatchOrdersRequest.Types.Filter
            {
                CreatedAfter = (reply0.CorporateEvent?.Audit?.CreatedAt ?? new Timestamp()),
                OwnerId = (reply0.CorporateEvent?.Organizer?.UserId ?? ""),
                Region = (reply0.CorporateEvent?.Location?.Region ?? ""),
            },
        };
        var reply1 = await SendGetCorporateEventRules1Async(input1, token);
        var response = new ApiCorporateevents.GetCorporateEventRulesResponse
        {
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                CorporateEventId = (request.CorporateEventId ?? ""),
                Organizer = new ApiIdentity.UserRef
                {
                    Email = (reply0.CorporateEvent?.Organizer?.Email ?? ""),
                },
                Title = (reply0.CorporateEvent?.Title ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lints a corporate event.
    /// </summary>
    public override async Task<ApiCorporateevents.LintCorporateEventsResponse> LintCorporateEvents(ApiCorporateevents.LintCorporateEventsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling LintCorporateEvents for {Tenant}", requestContext.TenantId);
        if (request.CorporateEvent is { } record)
        {
            var snapshot = CorporateEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.ExplainRegistrationCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.ExplainRegistrationCalculationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
                },
            },
            Registration = new ApiCorporateevents.Registration
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                    Phone = (request.Details?.Contact?.Phone ?? ""),
                },
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _corporateeventsRegistrationCalculationService.ExplainRegistrationCalculationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDevices.ValidateComplianceScanRequest
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (reply0.Registration?.Holder?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.ValidateComplianceScanRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply0.Registration?.Contact?.Email ?? ""),
                    Name = (reply0.Registration?.Contact?.Name ?? new ApiCommon.PersonName()),
                    PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc1 = _devicesComplianceScanValidationService.ValidateComplianceScanAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var batch2 = (request.CorporateEvent?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiCorporateevents.CorporateEventParticipant>()).Select(async item =>
        {
            var input2 = new ApiHelpdesk.SuggestTicketsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiHelpdesk.SuggestTicketsRequest.Types.Filter
                {
                    Labels = { (reply0.Registration?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                    OwnerId = (item.ParticipantId ?? ""),
                    Region = (request.CorporateEvent?.Location?.Region ?? ""),
                },
            };
            using var rpc2 = _helpdeskTicketQueryService.SuggestTicketsAsync(input2, cancellationToken: token);
            var reply2 = await rpc2.ResponseAsync;
            return reply2;
        }).ToArray();
        var replies2 = await Task.WhenAll(batch2);
        var reply2 = replies2.LastOrDefault() ?? new ApiHelpdesk.SuggestTicketsResponse();
        var response = new ApiCorporateevents.LintCorporateEventsResponse
        {
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                CorporateEventId = (request.CorporateEventId ?? ""),
                Participants = { new ApiCorporateevents.CorporateEventParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (reply1.ComplianceScan?.Subject?.Email ?? ""),
                    },
                } },
                Status = (request.CorporateEvent?.Status ?? (ApiCorporateevents.CorporateEventStatus)0),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event",
            "audit", "LintCorporateEvents", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Normalizes a corporate event.
    /// </summary>
    public override async Task<ApiCorporateevents.NormalizeCorporateEventResponse> NormalizeCorporateEvent(ApiCorporateevents.NormalizeCorporateEventRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling NormalizeCorporateEvent for {Tenant}", requestContext.TenantId);
        if (request.CorporateEvent is { } record)
        {
            var snapshot = CorporateEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.CalculateRegistrationRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Details = new ApiCorporateevents.CalculateRegistrationRequest.Types.Details
        {
            Destination = new ApiCommon.PostalAddress
            {
                CountryCode = (request.CorporateEvent?.Location?.CountryCode ?? ""),
            },
        };
        input0.Registration = new ApiCorporateevents.Registration
        {
            Contact = new ApiCommon.ContactInfo
            {
                Email = (request.Details?.Contact?.Email ?? ""),
                Name = (request.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
            },
        };
        using var rpc0 = _corporateeventsRegistrationCalculationService.CalculateRegistrationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiHelpdesk.SummarizeTicketsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Reason ?? ""),
        };
        using var rpc1 = _helpdeskTicketQueryService.SummarizeTicketsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiCorporateevents.NormalizeCorporateEventResponse
        {
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                CorporateEventId = (request.CorporateEventId ?? ""),
                Metadata = { (request.CorporateEvent?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Organizer = new ApiIdentity.UserRef
                {
                    Email = (reply0.Registration?.Contact?.Email ?? ""),
                },
                Participants = { new ApiCorporateevents.CorporateEventParticipant
                {
                    Contact = (reply0.Registration?.Contact ?? new ApiCommon.ContactInfo()),
                } },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.Locale ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event",
            "audit", "NormalizeCorporateEvent", (request.Details?.Locale ?? ""), token);
        return response;
    }
    /// <summary>
    /// Previews a corporate event.
    /// </summary>
    public override async Task<ApiCorporateevents.PreviewCorporateEventResponse> PreviewCorporateEvent(ApiCorporateevents.PreviewCorporateEventRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PreviewCorporateEvent for {Tenant}", requestContext.TenantId);
        if (request.CorporateEvent is { } record)
        {
            var snapshot = CorporateEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.RecalculateRegistrationsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Registration = new ApiCorporateevents.Registration
            {
                Holder = new ApiCorporateevents.CorporateEventRef
                {
                    DisplayName = (request.CorporateEvent?.Organizer?.DisplayName ?? ""),
                },
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _corporateeventsRegistrationCalculationService.RecalculateRegistrationsAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var reply1 = new ApiCorporateevents.ExplainRegistrationCalculationResponse();
        foreach (var item in (request.CorporateEvent?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiCorporateevents.CorporateEventParticipant>()))
        {
            var input1 = new ApiCorporateevents.ExplainRegistrationCalculationRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Registration = new ApiCorporateevents.Registration
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (item.Contact?.Email ?? ""),
                        Phone = (item.Contact?.Phone ?? ""),
                        PreferredChannel = (item.Contact?.PreferredChannel ?? ""),
                    },
                },
            };
            using var rpc1 = _corporateeventsRegistrationCalculationService.ExplainRegistrationCalculationAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var reply2 = new ApiCorporateevents.CompareRegistrationOptionsResponse();
        foreach (var item in (request.CorporateEvent?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiCorporateevents.CorporateEventParticipant>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input2 = new ApiCorporateevents.CompareRegistrationOptionsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Registration = new ApiCorporateevents.Registration
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (item.Contact?.Email ?? ""),
                        PreferredChannel = (item.Contact?.PreferredChannel ?? ""),
                    },
                },
            };
            using var rpc2 = _corporateeventsRegistrationCalculationService.CompareRegistrationOptionsAsync(input2, cancellationToken: token);
            reply2 = await rpc2.ResponseAsync;
        }
        var input3 = new ApiDocuments.ResolveSignatureConflictRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ResolveSignatureConflictRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Signature = new ApiDocuments.Signature
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Measurements = { new ApiDocuments.SignatureMeasurement
                {
                    ObservedAt = (request.CorporateEvent?.Audit?.CreatedAt ?? new Timestamp()),
                } },
                Subject = new ApiDocuments.DocumentRef
                {
                    DisplayName = (request.CorporateEvent?.Organizer?.DisplayName ?? ""),
                },
            },
        };
        using var rpc3 = _documentsSignatureSyncService.ResolveSignatureConflictAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiCorporateevents.PreviewCorporateEventResponse
        {
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                CorporateEventId = (request.CorporateEventId ?? ""),
                Organizer = (request.CorporateEvent?.Organizer ?? new ApiIdentity.UserRef()),
            },
            Outcome = new ApiCorporateevents.PreviewCorporateEventResponse.Types.Outcome
            {
                EffectiveAt = (reply3.Outcome?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event",
            "audit", "PreviewCorporateEvent", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Scores a corporate event.
    /// </summary>
    public override async Task<ApiCorporateevents.ScoreCorporateEventResponse> ScoreCorporateEvent(ApiCorporateevents.ScoreCorporateEventRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScoreCorporateEvent for {Tenant}", requestContext.TenantId);
        if (request.CorporateEvent is { } record)
        {
            var snapshot = CorporateEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.CountTicketsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiHelpdesk.CountTicketsRequest.Types.Filter
            {
                CreatedAfter = (request.CorporateEvent?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (request.CorporateEvent?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.CorporateEvent?.Organizer?.UserId ?? ""),
                Region = (request.Details?.Destination?.Region ?? ""),
            },
        };
        var named0 = _registry.Get<ApiHelpdesk.TicketQueryService.TicketQueryServiceClient>("acme.helpdesk.v1.TicketQueryService");
        using var rpc0 = named0.CountTicketsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiCorporateevents.ExplainCorporateEventCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Location = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Details?.Destination?.CountryCode ?? ""),
                },
                Participants = { new ApiCorporateevents.CorporateEventParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.CorporateEvent?.Organizer?.Email ?? ""),
                    },
                } },
            },
        };
        var descriptor1 = new Method<ApiCorporateevents.ExplainCorporateEventCalculationRequest, ApiCorporateevents.ExplainCorporateEventCalculationResponse>(
            MethodType.Unary, "acme.corporateevents.v1" + ".CorporateEventCalculationService", "ExplainCorporateEventCalculation",
            Wire.Marshaller<ApiCorporateevents.ExplainCorporateEventCalculationRequest>(), Wire.Marshaller<ApiCorporateevents.ExplainCorporateEventCalculationResponse>());
        using var rpc1 = _invoker.AsyncUnaryCall(descriptor1, null,
            new CallOptions(cancellationToken: token), input1);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiCorporateevents.RecalculateRegistrationsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.RecalculateRegistrationsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Details?.Destination?.PostalCode ?? ""),
                },
            },
            Registration = new ApiCorporateevents.Registration
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.CorporateEvent?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedAt = (request.CorporateEvent?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply1.CorporateEvent?.Organizer?.Email ?? ""),
                },
            },
        };
        var command2 = new DispatchCommand<ApiCorporateevents.RecalculateRegistrationsRequest>(input2,
            "primary", requestContext.TenantId);
        var routed2 = command2 with { Region = _settings.Value.Region };
        using var rpc2 = _corporateeventsRegistrationCalculationService.RecalculateRegistrationsAsync(routed2.Payload, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiCorporateevents.ScoreCorporateEventResponse
        {
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                CorporateEventId = (reply2.Registration?.Holder?.CorporateEventId ?? ""),
                Location = new ApiCommon.PostalAddress
                {
                    Region = (reply1.CorporateEvent?.Location?.Region ?? ""),
                },
                Organizer = (request.CorporateEvent?.Organizer ?? new ApiIdentity.UserRef()),
                Title = (request.CorporateEvent?.Title ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event",
            "audit", "ScoreCorporateEvent", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Validates a corporate event.
    /// </summary>
    public override async Task<ApiCorporateevents.ValidateCorporateEventResponse> ValidateCorporateEvent(ApiCorporateevents.ValidateCorporateEventRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ValidateCorporateEvent for {Tenant}", requestContext.TenantId);
        if (request.CorporateEvent is { } record)
        {
            var snapshot = CorporateEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var profile0 = new MappingProfile<ApiCorporateevents.ValidateCorporateEventRequest, ApiDeliveries.ResolveDeliveriesRequest>(value => new ApiDeliveries.ResolveDeliveriesRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _deliveriesDeliveryQueryService.ResolveDeliveriesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiCorporateevents.RecalculateCorporateEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Participants = { new ApiCorporateevents.CorporateEventParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Name = (request.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
                        PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
                    },
                } },
            },
        };
        input1 = PropertyMap.Copy(input1);
        using var rpc1 = _corporateeventsCorporateEventCalculationService.RecalculateCorporateEventsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiCorporateevents.ValidateCorporateEventResponse
        {
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                CorporateEventId = (request.CorporateEventId ?? ""),
                Location = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply1.CorporateEvent?.Location?.PostalCode ?? ""),
                },
                Metadata = { (request.CorporateEvent?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Outcome = new ApiCorporateevents.ValidateCorporateEventResponse.Types.Outcome
            {
                Message = (reply1.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event",
            "audit", "ValidateCorporateEvent", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiDispatch.CountDispatchOrdersResponse> SendGetCorporateEventRules1Async(ApiDispatch.CountDispatchOrdersRequest request, CancellationToken token)
    {
        return await _corporateEventValidationEndpointSendGetCorporateEventRules1Port.SendAsync(request, token);
    }
}
