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
using ApiCampaigns = Acme.Campaigns.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDevices = Acme.Devices.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiSellers = Acme.Sellers.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Corporateevents;

public sealed partial class CorporateEventApprovalEndpoint
{
    /// <summary>
    /// Approves a corporate event.
    /// </summary>
    public override async Task<ApiCorporateevents.ApproveCorporateEventResponse> ApproveCorporateEvent(ApiCorporateevents.ApproveCorporateEventRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ApproveCorporateEvent for {Tenant}", requestContext.TenantId);
        if (request.CorporateEvent is { } record)
        {
            var snapshot = CorporateEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiVenues.SummarizeVenuesRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _venuesVenueQueryService.SummarizeVenuesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiCorporateevents.StreamRegistrationMetricsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiCorporateevents.StreamRegistrationMetricsRequest.Types.Filter
            {
                Region = (request.CorporateEvent?.Location?.Region ?? ""),
            },
            Subscriber = new ApiCommon.ContactInfo
            {
                Email = (request.Details?.Approver?.Email ?? ""),
            },
        };
        using var stream1 = _corporateeventsRegistrationReportingService.StreamRegistrationMetrics(input1, cancellationToken: token);
        await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received StreamRegistrationMetrics frame {Type}", change1.GetType().Name);
        }
        var response = new ApiCorporateevents.ApproveCorporateEventResponse
        {
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                CorporateEventId = (request.CorporateEventId ?? ""),
                Title = (request.CorporateEvent?.Title ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event",
            "audit", "ApproveCorporateEvent", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Delegates a corporate event.
    /// </summary>
    public override async Task<ApiCorporateevents.DelegateCorporateEventApprovalResponse> DelegateCorporateEventApproval(ApiCorporateevents.DelegateCorporateEventApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling DelegateCorporateEventApproval for {Tenant}", requestContext.TenantId);
        if (request.CorporateEvent is { } record)
        {
            var snapshot = CorporateEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.EscalateEntitlementRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.EscalateEntitlementRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    DisplayName = (request.CorporateEvent?.Organizer?.DisplayName ?? ""),
                },
            },
            Entitlement = new ApiLicensing.Entitlement
            {
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.Details?.Approver?.UserId ?? ""),
                },
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _licensingEntitlementApprovalService.EscalateEntitlementAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var reply1 = new ApiDevices.StartDevicePolicyWorkflowResponse();
        foreach (var item in (request.CorporateEvent?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiCorporateevents.CorporateEventParticipant>()))
        {
            var input1 = new ApiDevices.StartDevicePolicyWorkflowRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiDevices.StartDevicePolicyWorkflowRequest.Types.Details
                {
                    Notify = new ApiCommon.ContactInfo
                    {
                        Email = (item.Contact?.Email ?? ""),
                        Name = (item.Contact?.Name ?? new ApiCommon.PersonName()),
                        Phone = (item.Contact?.Phone ?? ""),
                        PreferredChannel = (item.Contact?.PreferredChannel ?? ""),
                    },
                },
            };
            using var rpc1 = _devicesDevicePolicyWorkflowService.StartDevicePolicyWorkflowAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var input2 = new ApiSellers.BrowseSellersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiSellers.BrowseSellersRequest.Types.Filter
            {
                CreatedAfter = (reply1.DevicePolicy?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (request.CorporateEvent?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.CorporateEvent?.CorporateEventId ?? ""),
                Region = (request.CorporateEvent?.Location?.Region ?? ""),
            },
        };
        using var rpc2 = _sendDelegateCorporateEventApproval2(input2, new CallOptions(cancellationToken: token));
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiCorporateevents.DelegateCorporateEventApprovalResponse
        {
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                CorporateEventId = (request.CorporateEventId ?? ""),
            },
            Outcome = new ApiCorporateevents.DelegateCorporateEventApprovalResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                NextSteps = { (reply0.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply2.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply2.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event",
            "audit", "DelegateCorporateEventApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Escalates a corporate event.
    /// </summary>
    public override async Task<ApiCorporateevents.EscalateCorporateEventResponse> EscalateCorporateEvent(ApiCorporateevents.EscalateCorporateEventRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EscalateCorporateEvent for {Tenant}", requestContext.TenantId);
        if (request.CorporateEvent is { } record)
        {
            var snapshot = CorporateEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.CreateCorporateEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Location = new ApiCommon.PostalAddress
                {
                    Locality = (request.CorporateEvent?.Location?.Locality ?? ""),
                    Recipient = (request.CorporateEvent?.Location?.Recipient ?? ""),
                },
                Participants = { new ApiCorporateevents.CorporateEventParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Details?.Approver?.Email ?? ""),
                    },
                } },
            },
        };
        var named0 = _registry.Get<ApiCorporateevents.CorporateEventService.CorporateEventServiceClient>("acme.corporateevents.v1.CorporateEventService");
        using var rpc0 = named0.CreateCorporateEventAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.CommentServiceAssetApprovalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.CommentServiceAssetApprovalRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    Email = (reply0.CorporateEvent?.Organizer?.Email ?? ""),
                },
            },
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Address = new ApiCommon.PostalAddress
                {
                    Locality = (reply0.CorporateEvent?.Location?.Locality ?? ""),
                    Region = (reply0.CorporateEvent?.Location?.Region ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (reply0.CorporateEvent?.Organizer?.UserId ?? ""),
                },
            },
        };
        var descriptor1 = new Method<ApiItsm.CommentServiceAssetApprovalRequest, ApiItsm.CommentServiceAssetApprovalResponse>(
            MethodType.Unary, "acme.itsm.v1" + ".ServiceAssetApprovalService", "CommentServiceAssetApproval",
            Wire.Marshaller<ApiItsm.CommentServiceAssetApprovalRequest>(), Wire.Marshaller<ApiItsm.CommentServiceAssetApprovalResponse>());
        using var rpc1 = _invoker.AsyncUnaryCall(descriptor1, null,
            new CallOptions(cancellationToken: token), input1);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiCorporateevents.EscalateCorporateEventResponse
        {
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                CorporateEventId = (request.CorporateEventId ?? ""),
                Location = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply0.CorporateEvent?.Location?.CountryCode ?? ""),
                },
                Status = (request.CorporateEvent?.Status ?? (ApiCorporateevents.CorporateEventStatus)0),
            },
            Outcome = new ApiCorporateevents.EscalateCorporateEventResponse.Types.Outcome
            {
                Message = (reply1.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event",
            "audit", "EscalateCorporateEvent", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a corporate event.
    /// </summary>
    public override async Task<ApiCorporateevents.GetCorporateEventApprovalStatusResponse> GetCorporateEventApprovalStatus(ApiCorporateevents.GetCorporateEventApprovalStatusRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.CorporateEventId, "corporate_event_id");
        _logger.LogInformation("Handling GetCorporateEventApprovalStatus for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event", request.CorporateEventId,
            "GetCorporateEventApprovalStatus", "accepted", token);
        var input0 = new ApiCorporateevents.ScheduleRegistrationExpiryRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Registration = new ApiCorporateevents.Registration
            {
                Holder = new ApiCorporateevents.CorporateEventRef
                {
                    CorporateEventId = (request.CorporateEventId ?? ""),
                },
            },
        };
        var command0 = new DispatchCommand<ApiCorporateevents.ScheduleRegistrationExpiryRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _corporateeventsRegistrationLifecycleService.ScheduleRegistrationExpiryAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiCorporateevents.ScheduleRegistrationExpiryRequest>("acme.corporateevents.v1.CorporateEventApprovalService/GetCorporateEventApprovalStatus/0", routed0.Payload.Clone()), token);
        var profile1 = new MappingProfile<ApiCorporateevents.GetCorporateEventApprovalStatusRequest, ApiCorporateevents.TerminateCorporateEventRequest>(value => new ApiCorporateevents.TerminateCorporateEventRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            CorporateEventId = (value.CorporateEventId ?? ""),
            Details = new ApiCorporateevents.TerminateCorporateEventRequest.Types.Details
            {
                NotifyContact = new ApiCommon.ContactInfo
                {
                    Email = (reply0.Registration?.Contact?.Email ?? ""),
                    Name = (reply0.Registration?.Contact?.Name ?? new ApiCommon.PersonName()),
                    PreferredChannel = (reply0.Registration?.Contact?.PreferredChannel ?? ""),
                },
            },
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _corporateeventsCorporateEventLifecycleService.TerminateCorporateEventAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiCorporateevents.GetCorporateEventApprovalStatusResponse
        {
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply1.CorporateEvent?.Audit?.CreatedBy ?? ""),
                },
                CorporateEventId = (request.CorporateEventId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists corporate events a page at a time.
    /// </summary>
    public override async Task<ApiCorporateevents.ListPendingCorporateEventsResponse> ListPendingCorporateEvents(ApiCorporateevents.ListPendingCorporateEventsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListPendingCorporateEvents for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event", "batch",
            "ListPendingCorporateEvents", "accepted", token);
        var input0 = new ApiFeatureflags.NormalizeFeatureFlagRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiFeatureflags.NormalizeFeatureFlagRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Locale = (request.Context?.Locale ?? ""),
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _featureflagsFeatureFlagValidationService.NormalizeFeatureFlagAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiCorporateevents.ListPendingCorporateEventsResponse
        {
            CorporateEvents = { new ApiCorporateevents.CorporateEvent
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.FeatureFlag?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Participants = { new ApiCorporateevents.CorporateEventParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (reply0.FeatureFlag?.Owner?.Email ?? ""),
                    },
                } },
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event",
            "audit", "ListPendingCorporateEvents", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Rejects a corporate event.
    /// </summary>
    public override async Task<ApiCorporateevents.RejectCorporateEventResponse> RejectCorporateEvent(ApiCorporateevents.RejectCorporateEventRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RejectCorporateEvent for {Tenant}", requestContext.TenantId);
        if (request.CorporateEvent is { } record)
        {
            var snapshot = CorporateEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCampaigns.BatchGetMarketingCampaignsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _campaignsMarketingCampaignService.BatchGetMarketingCampaignsAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiLicensing.SubmitEntitlementRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.SubmitEntitlementRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    DisplayName = (request.CorporateEvent?.Organizer?.DisplayName ?? ""),
                },
            },
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.CorporateEvent?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (request.CorporateEvent?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        using var rpc1 = _licensingEntitlementApprovalService.SubmitEntitlementAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var reply2 = new ApiCorporateevents.CountCorporateEventTotalsResponse();
        foreach (var item in (request.CorporateEvent?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiCorporateevents.CorporateEventParticipant>()))
        {
            var input2 = new ApiCorporateevents.CountCorporateEventTotalsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiCorporateevents.CountCorporateEventTotalsRequest.Types.Filter
                {
                    CreatedAfter = (reply1.Entitlement?.Audit?.CreatedAt ?? new Timestamp()),
                    OwnerId = (item.ParticipantId ?? ""),
                    Region = (request.CorporateEvent?.Location?.Region ?? ""),
                },
            };
            using var rpc2 = _corporateeventsCorporateEventReportingService.CountCorporateEventTotalsAsync(input2, cancellationToken: token);
            reply2 = await rpc2.ResponseAsync;
        }
        var input3 = new ApiCorporateevents.PullVenuesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.PullVenuesRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Venue = new ApiCorporateevents.Venue
            {
                Address = new ApiCommon.PostalAddress
                {
                    Recipient = (request.CorporateEvent?.Location?.Recipient ?? ""),
                },
            },
        };
        using var rpc3 = _corporateeventsVenueSyncService.PullVenuesAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiCorporateevents.RejectCorporateEventResponse
        {
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                CorporateEventId = (request.CorporateEventId ?? ""),
                Organizer = new ApiIdentity.UserRef
                {
                    Email = (reply3.Venue?.Owner?.Email ?? ""),
                },
                Window = (request.CorporateEvent?.Window ?? new ApiCommon.TimeWindow()),
            },
            Outcome = new ApiCorporateevents.RejectCorporateEventResponse.Types.Outcome
            {
                ReferenceId = (reply1.Outcome?.ReferenceId ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply3.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event",
            "audit", "RejectCorporateEvent", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Submits a corporate event.
    /// </summary>
    public override async Task<ApiCorporateevents.SubmitCorporateEventResponse> SubmitCorporateEvent(ApiCorporateevents.SubmitCorporateEventRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SubmitCorporateEvent for {Tenant}", requestContext.TenantId);
        if (request.CorporateEvent is { } record)
        {
            var snapshot = CorporateEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.PeekCorporateEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEventId = (request.CorporateEvent?.CorporateEventId ?? ""),
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiCorporateevents.PeekCorporateEventRequest>(json0);
        using var rpc0 = _corporateeventsCorporateEventQueryService.PeekCorporateEventAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiCampaigns.UpdateMarketingCampaignRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            MarketingCampaign = new ApiCampaigns.MarketingCampaign
            {
                Organizer = new ApiIdentity.UserRef
                {
                    UserId = (request.Details?.Approver?.UserId ?? ""),
                },
                Participants = { new ApiCampaigns.MarketingCampaignParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.CorporateEvent?.Organizer?.Email ?? ""),
                    },
                } },
                Weekly = new ApiCampaigns.MarketingCampaignWeeklyRule
                {
                    Days = { (request.CorporateEvent ?? new ApiCorporateevents.CorporateEvent()).RecurrenceCase switch
                    {
                        ApiCorporateevents.CorporateEvent.RecurrenceOneofCase.Weekly => (request.CorporateEvent?.Weekly?.Days ?? new Google.Protobuf.Collections.RepeatedField<string>()),
                        _ => new Google.Protobuf.Collections.RepeatedField<string>()
                    } },
                    Interval = (reply0.CorporateEvent ?? new ApiCorporateevents.CorporateEvent()).RecurrenceCase switch
                    {
                        ApiCorporateevents.CorporateEvent.RecurrenceOneofCase.Weekly => (reply0.CorporateEvent?.Weekly?.Interval ?? 0),
                        _ => 0
                    },
                },
            },
        };
        using var rpc1 = _campaignsMarketingCampaignService.UpdateMarketingCampaignAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiVenues.SuggestVenuesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiVenues.SuggestVenuesRequest.Types.Filter
            {
                Region = (request.CorporateEvent?.Location?.Region ?? ""),
            },
        };
        using var rpc2 = _venuesVenueQueryService.SuggestVenuesAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiHelpdesk.ResumeSlaPolicyWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.MarketingCampaign?.Audit?.CreatedAt ?? new Timestamp()),
                    Etag = (reply1.MarketingCampaign?.Audit?.Etag ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.CorporateEvent?.Organizer?.UserId ?? ""),
                },
            },
        };
        var reply3 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _helpdeskSlaPolicyWorkflowService.ResumeSlaPolicyWorkflowAsync(input3, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiCorporateevents.SubmitCorporateEventResponse
        {
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply1.MarketingCampaign?.Audit?.CreatedBy ?? ""),
                    UpdatedBy = (reply1.MarketingCampaign?.Audit?.UpdatedBy ?? ""),
                },
                CorporateEventId = (request.CorporateEventId ?? ""),
                Location = (request.CorporateEvent?.Location ?? new ApiCommon.PostalAddress()),
                Status = (reply0.CorporateEvent?.Status ?? (ApiCorporateevents.CorporateEventStatus)0),
            },
            Outcome = new ApiCorporateevents.SubmitCorporateEventResponse.Types.Outcome
            {
                Message = (reply3.Outcome?.Message ?? ""),
                Status = (reply3.Outcome?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply2.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply2.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event",
            "audit", "SubmitCorporateEvent", (request.Reason ?? ""), token);
        return response;
    }
}
