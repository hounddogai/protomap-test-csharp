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
using ApiBroadcasts = Acme.Broadcasts.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;

namespace Acme.Workforce.Corporateevents;

public sealed partial class VenueCalculationEndpoint
{
    /// <summary>
    /// Calculates a venue.
    /// </summary>
    public override async Task<ApiCorporateevents.CalculateVenueResponse> CalculateVenue(ApiCorporateevents.CalculateVenueRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CalculateVenue for {Tenant}", requestContext.TenantId);
        if (request.Venue is { } record)
        {
            var snapshot = VenueMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.ListDocumentTemplateSubscriptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListDocumentTemplateSubscriptionsRequest.Types.Filter
            {
                OwnerId = (request.Venue?.Components?.GetValueOrDefault("primary")?.ComponentId ?? ""),
                Region = (request.Venue ?? new ApiCorporateevents.Venue()).LocatorCase switch
                {
                    ApiCorporateevents.Venue.LocatorOneofCase.Address => (request.Venue?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc0 = _documentsDocumentTemplateNotificationService.ListDocumentTemplateSubscriptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiCorporateevents.PreviewCorporateEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Location = new ApiCommon.PostalAddress
                {
                    Recipient = (request.Venue ?? new ApiCorporateevents.Venue()).LocatorCase switch
                    {
                        ApiCorporateevents.Venue.LocatorOneofCase.Address => (request.Venue?.Address?.Recipient ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _corporateeventsCorporateEventValidationService.PreviewCorporateEventAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiCorporateevents.ScoreCorporateEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Location = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Venue ?? new ApiCorporateevents.Venue()).LocatorCase switch
                    {
                        ApiCorporateevents.Venue.LocatorOneofCase.Address => (request.Venue?.Address?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()),
                        _ => new Google.Protobuf.Collections.RepeatedField<string>()
                    } },
                },
                Organizer = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Venue?.Owner?.DisplayName ?? ""),
                    UserId = (reply1.CorporateEvent?.Organizer?.UserId ?? ""),
                },
            },
        };
        using var rpc2 = _corporateeventsCorporateEventValidationService.ScoreCorporateEventAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiCorporateevents.CalculateVenueResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply2.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
            Venue = new ApiCorporateevents.Venue
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.CorporateEvent?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.CorporateEvent?.Organizer?.DisplayName ?? ""),
                },
                Specs = { (request.Venue?.Specs ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                VenueId = (request.VenueId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Venue?.VenueId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "venue",
            "audit", "CalculateVenue", (request.Venue?.VenueId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Compares a venue.
    /// </summary>
    public override async Task<ApiCorporateevents.CompareVenueOptionsResponse> CompareVenueOptions(ApiCorporateevents.CompareVenueOptionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CompareVenueOptions for {Tenant}", requestContext.TenantId);
        if (request.Venue is { } record)
        {
            var snapshot = VenueMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.PreviewCorporateEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Participants = { new ApiCorporateevents.CorporateEventParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Venue?.Owner?.Email ?? ""),
                    },
                } },
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _corporateeventsCorporateEventValidationService.PreviewCorporateEventAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiCorporateevents.CompareVenueOptionsResponse
        {
            Venue = new ApiCorporateevents.Venue
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.CorporateEvent?.Audit?.CreatedAt ?? new Timestamp()),
                    Etag = (reply0.CorporateEvent?.Audit?.Etag ?? ""),
                },
                Owner = (request.Venue?.Owner ?? new ApiIdentity.UserRef()),
                VenueId = (request.VenueId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Venue?.VenueId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "venue",
            "audit", "CompareVenueOptions", (request.Venue?.VenueId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Estimates a venue.
    /// </summary>
    public override async Task<ApiCorporateevents.EstimateVenueResponse> EstimateVenue(ApiCorporateevents.EstimateVenueRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EstimateVenue for {Tenant}", requestContext.TenantId);
        if (request.Venue is { } record)
        {
            var snapshot = VenueMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.ValidateCorporateEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Location = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Details?.Destination?.CountryCode ?? ""),
                    Locality = (request.Details?.Destination?.Locality ?? ""),
                    Region = (request.Details?.Destination?.Region ?? ""),
                },
                Participants = { new ApiCorporateevents.CorporateEventParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Venue?.Owner?.Email ?? ""),
                    },
                } },
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _corporateeventsCorporateEventValidationService.ValidateCorporateEventAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiCorporateevents.RecalculateRegistrationsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.RecalculateRegistrationsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Details?.Destination?.CountryCode ?? ""),
                    Lines = { (reply0.CorporateEvent?.Location?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Region = (request.Venue ?? new ApiCorporateevents.Venue()).LocatorCase switch
                    {
                        ApiCorporateevents.Venue.LocatorOneofCase.Address => (request.Venue?.Address?.Region ?? ""),
                        _ => ""
                    },
                },
            },
            Registration = new ApiCorporateevents.Registration
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Venue?.Owner?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _corporateeventsRegistrationCalculationService.RecalculateRegistrationsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiCorporateevents.EstimateVenueResponse
        {
            Venue = new ApiCorporateevents.Venue
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.Registration?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (reply0.CorporateEvent?.Organizer?.UserId ?? ""),
                },
                Status = (request.Venue?.Status ?? (ApiCorporateevents.VenueStatus)0),
                VenueId = (request.VenueId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Venue?.VenueId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "venue",
            "audit", "EstimateVenue", (request.Venue?.VenueId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a venue.
    /// </summary>
    public override async Task<ApiCorporateevents.ExplainVenueCalculationResponse> ExplainVenueCalculation(ApiCorporateevents.ExplainVenueCalculationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExplainVenueCalculation for {Tenant}", requestContext.TenantId);
        if (request.Venue is { } record)
        {
            var snapshot = VenueMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.LintCorporateEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.Venue?.Audit?.UpdatedBy ?? ""),
                },
                Location = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Venue ?? new ApiCorporateevents.Venue()).LocatorCase switch
                    {
                        ApiCorporateevents.Venue.LocatorOneofCase.Address => (request.Venue?.Address?.CountryCode ?? ""),
                        _ => ""
                    },
                },
            },
            Details = new ApiCorporateevents.LintCorporateEventsRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc0 = _corporateeventsCorporateEventValidationService.LintCorporateEventsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiIdentity.LintUsersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiIdentity.LintUsersRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.Contact?.Phone ?? ""),
                },
            },
            User = new ApiIdentity.User
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.CorporateEvent?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
                },
            },
        };
        var named1 = _registry.Get<ApiIdentity.UserValidationService.UserValidationServiceClient>("acme.identity.v1.UserValidationService");
        using var rpc1 = named1.LintUsersAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiCorporateevents.PreviewCorporateEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Organizer = new ApiIdentity.UserRef
                {
                    Email = (reply0.CorporateEvent?.Organizer?.Email ?? ""),
                },
                Participants = { new ApiCorporateevents.CorporateEventParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Venue?.Owner?.Email ?? ""),
                        PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
                    },
                } },
            },
        };
        using var rpc2 = _corporateeventsCorporateEventValidationService.PreviewCorporateEventAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        var input3 = new ApiCorporateevents.ExplainRegistrationCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Registration = new ApiCorporateevents.Registration
            {
                Holder = new ApiCorporateevents.CorporateEventRef
                {
                    DisplayName = (reply0.CorporateEvent?.Organizer?.DisplayName ?? ""),
                },
            },
        };
        using var rpc3 = _corporateeventsRegistrationCalculationService.ExplainRegistrationCalculationAsync(input3, cancellationToken: token);
        var pending3 = rpc3.ResponseAsync;
        await Task.WhenAll(pending2, pending3);
        var reply2 = await pending2;
        var reply3 = await pending3;
        var response = new ApiCorporateevents.ExplainVenueCalculationResponse
        {
            Outcome = new ApiCorporateevents.ExplainVenueCalculationResponse.Types.Outcome
            {
                Message = (reply0.Outcome?.Message ?? ""),
                ReferenceId = (reply1.Outcome?.ReferenceId ?? ""),
                Status = (reply2.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Outcome?.Status ?? ""),
            },
            Venue = new ApiCorporateevents.Venue
            {
                Capacity = (request.Venue?.Capacity ?? 0L),
                VenueId = (request.VenueId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Venue?.VenueId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "venue",
            "audit", "ExplainVenueCalculation", (request.Venue?.VenueId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a venue.
    /// </summary>
    public override async Task<ApiCorporateevents.GetVenueRatesResponse> GetVenueRates(ApiCorporateevents.GetVenueRatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.VenueId, "venue_id");
        _logger.LogInformation("Handling GetVenueRates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "venue", request.VenueId,
            "GetVenueRates", "accepted", token);
        var profile0 = new MappingProfile<ApiCorporateevents.GetVenueRatesRequest, ApiCorporateevents.NormalizeCorporateEventRequest>(value => new ApiCorporateevents.NormalizeCorporateEventRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Participants = { new ApiCorporateevents.CorporateEventParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (value.Context?.Actor?.Email ?? ""),
                    },
                } },
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _corporateeventsCorporateEventValidationService.NormalizeCorporateEventAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiCorporateevents.GetVenueRatesResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Outcome?.Message ?? ""),
            },
            Venue = new ApiCorporateevents.Venue
            {
                VenueId = (request.VenueId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Quotes a venue.
    /// </summary>
    public override async Task<ApiCorporateevents.QuoteVenueResponse> QuoteVenue(ApiCorporateevents.QuoteVenueRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling QuoteVenue for {Tenant}", requestContext.TenantId);
        if (request.Venue is { } record)
        {
            var snapshot = VenueMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.SimulateCorporateEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.SimulateCorporateEventRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.Venue ?? new ApiCorporateevents.Venue()).LocatorCase switch
                    {
                        ApiCorporateevents.Venue.LocatorOneofCase.Address => (request.Venue?.Address?.Region ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc0 = _corporateeventsCorporateEventCalculationService.SimulateCorporateEventAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiCorporateevents.SimulateCorporateEventRequest>("acme.corporateevents.v1.VenueCalculationService/QuoteVenue/0", input0.Clone()), token);
        var input1 = request.QuoteVenueToCheckCorporateEventEligibility1(reply0);
        using var rpc1 = _corporateeventsCorporateEventValidationService.CheckCorporateEventEligibilityAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiBroadcasts.BatchGetBroadcastsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply2 = await SendQuoteVenue2Async(input2, token);
        var input3 = new ApiCorporateevents.ScoreCorporateEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Location = new ApiCommon.PostalAddress
                {
                    Recipient = (request.Details?.Destination?.Recipient ?? ""),
                },
            },
        };
        var labels3 = input3.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input3.Context.Baggage.Clear();
        input3.Context.Baggage.Add(labels3);
        using var rpc3 = _corporateeventsCorporateEventValidationService.ScoreCorporateEventAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiCorporateevents.QuoteVenueResponse
        {
            Breakdown = new ApiCorporateevents.QuoteVenueResponse.Types.Breakdown
            {
                CurrencyCode = (request.Details?.CurrencyCode ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply2.Result?.Message ?? ""),
                Status = (reply2.Result?.Status ?? ""),
            },
            Venue = new ApiCorporateevents.Venue
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.CorporateEvent?.Audit?.Etag ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.CorporateEvent?.Organizer?.DisplayName ?? ""),
                },
                VenueId = (request.VenueId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Venue?.VenueId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "venue",
            "audit", "QuoteVenue", (request.Venue?.VenueId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalculates a venue.
    /// </summary>
    public override async Task<ApiCorporateevents.RecalculateVenuesResponse> RecalculateVenues(ApiCorporateevents.RecalculateVenuesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecalculateVenues for {Tenant}", requestContext.TenantId);
        if (request.Venue is { } record)
        {
            var snapshot = VenueMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.EstimateRegistrationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Registration = new ApiCorporateevents.Registration
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Venue?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (request.Venue?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiCorporateevents.EstimateRegistrationRequest>();
        using var rpc0 = _corporateeventsRegistrationCalculationService.EstimateRegistrationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiCorporateevents.NormalizeCorporateEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.NormalizeCorporateEventRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply0.Registration?.Contact?.Email ?? ""),
                },
            },
        };
        var json1 = JsonFormatter.Default.Format(input1);
        input1 = JsonParser.Default.Parse<ApiCorporateevents.NormalizeCorporateEventRequest>(json1);
        using var rpc1 = _corporateeventsCorporateEventValidationService.NormalizeCorporateEventAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiCorporateevents.RecalculateVenuesResponse
        {
            Breakdown = new ApiCorporateevents.RecalculateVenuesResponse.Types.Breakdown
            {
                CurrencyCode = (reply0.Registration?.CurrencyCode ?? ""),
            },
            Venue = new ApiCorporateevents.Venue
            {
                Name = (request.Venue?.Name ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply1.CorporateEvent?.Organizer?.Email ?? ""),
                },
                VenueId = (request.VenueId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Venue?.VenueId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "venue",
            "audit", "RecalculateVenues", (request.Venue?.VenueId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Simulates a venue.
    /// </summary>
    public override async Task<ApiCorporateevents.SimulateVenueResponse> SimulateVenue(ApiCorporateevents.SimulateVenueRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SimulateVenue for {Tenant}", requestContext.TenantId);
        if (request.Venue is { } record)
        {
            var snapshot = VenueMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiAudit.UpdateAuditEventRequest
        {
            AuditEvent = new ApiAudit.AuditEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Measurements = { new ApiAudit.AuditEventMeasurement
                {
                    Unit = (request.Venue?.Dimensions?.Unit ?? ""),
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _auditAuditEventService.UpdateAuditEventAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiCorporateevents.LintCorporateEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.Venue?.Audit?.UpdatedBy ?? ""),
                },
                Location = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Details?.Destination?.CountryCode ?? ""),
                },
            },
        };
        using var rpc1 = _corporateeventsCorporateEventValidationService.LintCorporateEventsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiCorporateevents.SimulateVenueResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.AuditEvent?.CorrelationId ?? ""),
            },
            Venue = new ApiCorporateevents.Venue
            {
                Code = (request.Venue?.Code ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.CorporateEvent?.Organizer?.DisplayName ?? ""),
                },
                VenueId = (request.VenueId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Venue?.VenueId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "venue",
            "audit", "SimulateVenue", (request.Venue?.VenueId ?? ""), token);
        return response;
    }
    private async Task<ApiBroadcasts.BatchGetBroadcastsResponse> SendQuoteVenue2Async(ApiBroadcasts.BatchGetBroadcastsRequest request, CancellationToken token)
    {
        return await _venueCalculationEndpointSendQuoteVenue2Port.SendAsync(request, token);
    }
}
