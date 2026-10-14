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
using ApiAuthorization = Acme.Authorization.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiExperiments = Acme.Experiments.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiSearch = Acme.Search.V1;

namespace Acme.Workforce.Corporateevents;

public sealed partial class CorporateEventCalculationEndpoint
{
    /// <summary>
    /// Calculates a corporate event.
    /// </summary>
    public override async Task<ApiCorporateevents.CalculateCorporateEventResponse> CalculateCorporateEvent(ApiCorporateevents.CalculateCorporateEventRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CalculateCorporateEvent for {Tenant}", requestContext.TenantId);
        if (request.CorporateEvent is { } record)
        {
            var snapshot = CorporateEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.UnsubscribeDocumentFolderUpdatesRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.UnsubscribeDocumentFolderUpdatesRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.CorporateEvent?.Organizer?.Email ?? ""),
                },
            },
            Reason = (request.Reason ?? ""),
        };
        using var rpc0 = _documentsDocumentFolderNotificationService.UnsubscribeDocumentFolderUpdatesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiAuthorization.BrowseAccessPoliciesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiAuthorization.BrowseAccessPoliciesRequest.Types.Filter
            {
                CreatedAfter = (request.CorporateEvent?.Audit?.UpdatedAt ?? new Timestamp()),
                OwnerId = (reply0.DocumentFolder?.Manager?.UserId ?? ""),
                Region = (request.Details?.Destination?.Region ?? ""),
            },
        };
        var reply1 = await Workflow.SendAsync(input1, token, _authorizationAccessPolicyQueryService.BrowseAccessPoliciesAsync);
        var batch2 = (request.CorporateEvent?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiCorporateevents.CorporateEventParticipant>()).Select(async item =>
        {
            var input2 = new ApiItsm.CompareIncidentOptionsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Incident = new ApiItsm.Incident
                {
                    Customer = new ApiEmployees.EmployeeRef
                    {
                        Email = (item.Contact?.Email ?? ""),
                    },
                },
            };
            using var rpc2 = _itsmIncidentCalculationService.CompareIncidentOptionsAsync(input2, cancellationToken: token);
            var reply2 = await rpc2.ResponseAsync;
            return reply2;
        }).ToArray();
        var replies2 = await Task.WhenAll(batch2);
        var reply2 = replies2.LastOrDefault() ?? new ApiItsm.CompareIncidentOptionsResponse();
        var input3 = new ApiHelpdesk.UpdateTicketRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Ticket = new ApiHelpdesk.Ticket
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply2.Incident?.Audit?.UpdatedBy ?? ""),
                },
                Customer = new ApiHelpdesk.KnowledgeArticleRef
                {
                    DisplayName = (reply2.Incident?.Assignee?.DisplayName ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (reply0.DocumentFolder?.Manager?.Email ?? ""),
                    PreferredChannel = (reply2.Incident?.Requester?.PreferredChannel ?? ""),
                },
            },
        };
        var named3 = _registry.Get<ApiHelpdesk.TicketService.TicketServiceClient>("acme.helpdesk.v1.TicketService");
        using var rpc3 = named3.UpdateTicketAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiCorporateevents.CalculateCorporateEventResponse
        {
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply3.Ticket?.Audit?.CreatedAt ?? new Timestamp()),
                },
                CorporateEventId = (request.CorporateEventId ?? ""),
                Location = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.DocumentFolder?.Location?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
                Organizer = new ApiIdentity.UserRef
                {
                    Email = (reply3.Ticket?.Requester?.Email ?? ""),
                },
                Window = (request.CorporateEvent?.Window ?? new ApiCommon.TimeWindow()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.Destination?.Recipient ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event",
            "audit", "CalculateCorporateEvent", (request.Details?.Destination?.Recipient ?? ""), token);
        return response;
    }
    /// <summary>
    /// Compares a corporate event.
    /// </summary>
    public override async Task<ApiCorporateevents.CompareCorporateEventOptionsResponse> CompareCorporateEventOptions(ApiCorporateevents.CompareCorporateEventOptionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CompareCorporateEventOptions for {Tenant}", requestContext.TenantId);
        if (request.CorporateEvent is { } record)
        {
            var snapshot = CorporateEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiNotifications.SearchNotificationsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiNotifications.SearchNotificationsRequest.Types.Filter
            {
                CreatedAfter = (request.CorporateEvent?.Audit?.CreatedAt ?? new Timestamp()),
                Region = (request.Details?.Destination?.Region ?? ""),
            },
        };
        var descriptor0 = new Method<ApiNotifications.SearchNotificationsRequest, ApiNotifications.SearchNotificationsResponse>(
            MethodType.Unary, "acme.notifications.v1" + ".NotificationService", "SearchNotifications",
            Wire.Marshaller<ApiNotifications.SearchNotificationsRequest>(), Wire.Marshaller<ApiNotifications.SearchNotificationsResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiExperiments.RecalculateExperimentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Experiment = new ApiExperiments.Experiment
            {
                Location = new ApiCommon.PostalAddress
                {
                    Locality = (request.Details?.Destination?.Locality ?? ""),
                },
                Organizer = new ApiIdentity.UserRef
                {
                    Email = (request.CorporateEvent?.Organizer?.Email ?? ""),
                },
                Weekly = new ApiExperiments.ExperimentWeeklyRule
                {
                    Interval = (request.CorporateEvent ?? new ApiCorporateevents.CorporateEvent()).RecurrenceCase switch
                    {
                        ApiCorporateevents.CorporateEvent.RecurrenceOneofCase.Weekly => (request.CorporateEvent?.Weekly?.Interval ?? 0),
                        _ => 0
                    },
                },
            },
        };
        using var rpc1 = _experimentsExperimentCalculationService.RecalculateExperimentsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiCorporateevents.CompareCorporateEventOptionsResponse
        {
            Breakdown = new ApiCorporateevents.CompareCorporateEventOptionsResponse.Types.Breakdown
            {
                CurrencyCode = (request.Details?.CurrencyCode ?? ""),
            },
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                CorporateEventId = (request.CorporateEventId ?? ""),
                Location = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply1.Experiment?.Location?.CountryCode ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event",
            "audit", "CompareCorporateEventOptions", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Estimates a corporate event.
    /// </summary>
    public override async Task<ApiCorporateevents.EstimateCorporateEventResponse> EstimateCorporateEvent(ApiCorporateevents.EstimateCorporateEventRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EstimateCorporateEvent for {Tenant}", requestContext.TenantId);
        if (request.CorporateEvent is { } record)
        {
            var snapshot = CorporateEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var profile0 = new MappingProfile<ApiCorporateevents.EstimateCorporateEventRequest, ApiItsm.ExplainChangeRequestDecisionRequest>(value => new ApiItsm.ExplainChangeRequestDecisionRequest
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (value.CorporateEvent?.Audit?.UpdatedBy ?? ""),
                },
            },
            Context = (value.Context ?? new ApiCommon.RequestContext()),
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _itsmChangeRequestValidationService.ExplainChangeRequestDecisionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var reply1 = new ApiExperiments.CompareExperimentOptionsResponse();
        foreach (var item in (request.CorporateEvent?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiCorporateevents.CorporateEventParticipant>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input1 = new ApiExperiments.CompareExperimentOptionsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Experiment = new ApiExperiments.Experiment
                {
                    Participants = { new ApiExperiments.ExperimentParticipant
                    {
                        Contact = new ApiCommon.ContactInfo
                        {
                            Email = (item.Contact?.Email ?? ""),
                        },
                    } },
                },
            };
            using var rpc1 = _experimentsExperimentCalculationService.CompareExperimentOptionsAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var input2 = new ApiItsm.CheckChangeRequestEligibilityRequest
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Customer = new ApiItsm.IncidentRef
                {
                    IncidentId = (reply0.ChangeRequest ?? new ApiItsm.ChangeRequest()).OriginCase switch
                    {
                        ApiItsm.ChangeRequest.OriginOneofCase.Customer => (reply0.ChangeRequest?.Customer?.IncidentId ?? ""),
                        _ => ""
                    },
                },
                Requester = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (reply0.ChangeRequest?.Requester?.PreferredChannel ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc2 = _itsmChangeRequestValidationService.CheckChangeRequestEligibilityAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiCorporateevents.EstimateCorporateEventResponse
        {
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply2.ChangeRequest?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                CorporateEventId = (request.CorporateEventId ?? ""),
                Participants = { new ApiCorporateevents.CorporateEventParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (reply2.ChangeRequest?.Requester?.Email ?? ""),
                        PreferredChannel = (reply0.ChangeRequest?.Requester?.PreferredChannel ?? ""),
                    },
                } },
                Window = (request.CorporateEvent?.Window ?? new ApiCommon.TimeWindow()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event",
            "audit", "EstimateCorporateEvent", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a corporate event.
    /// </summary>
    public override async Task<ApiCorporateevents.ExplainCorporateEventCalculationResponse> ExplainCorporateEventCalculation(ApiCorporateevents.ExplainCorporateEventCalculationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExplainCorporateEventCalculation for {Tenant}", requestContext.TenantId);
        if (request.CorporateEvent is { } record)
        {
            var snapshot = CorporateEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiHelpdesk.PreviewEscalationResponse();
        foreach (var item in (request.CorporateEvent?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiCorporateevents.CorporateEventParticipant>()))
        {
            var input0 = new ApiHelpdesk.PreviewEscalationRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiHelpdesk.PreviewEscalationRequest.Types.Details
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (item.Contact?.Email ?? ""),
                        Phone = (item.Contact?.Phone ?? ""),
                    },
                },
                Escalation = new ApiHelpdesk.Escalation
                {
                    Requester = new ApiCommon.ContactInfo
                    {
                        PreferredChannel = (item.Contact?.PreferredChannel ?? ""),
                    },
                },
            };
            using var rpc0 = _helpdeskEscalationValidationService.PreviewEscalationAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var input1 = new ApiSearch.ExplainSearchIndexDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiSearch.ExplainSearchIndexDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (reply0.Escalation?.Requester?.Phone ?? ""),
                },
            },
            SearchIndex = new ApiSearch.SearchIndex
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.CorporateEvent?.Audit?.Etag ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Escalation ?? new ApiHelpdesk.Escalation()).OriginCase switch
                    {
                        ApiHelpdesk.Escalation.OriginOneofCase.Customer => (reply0.Escalation?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                    Email = (reply0.Escalation?.Assignee?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _searchSearchIndexValidationService.ExplainSearchIndexDecisionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiCorporateevents.ExplainCorporateEventCalculationResponse
        {
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                CorporateEventId = (request.CorporateEventId ?? ""),
                Location = (request.CorporateEvent?.Location ?? new ApiCommon.PostalAddress()),
                Organizer = new ApiIdentity.UserRef
                {
                    UserId = (reply1.SearchIndex?.Owner?.UserId ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event",
            "audit", "ExplainCorporateEventCalculation", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a corporate event.
    /// </summary>
    public override async Task<ApiCorporateevents.GetCorporateEventRatesResponse> GetCorporateEventRates(ApiCorporateevents.GetCorporateEventRatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.CorporateEventId, "corporate_event_id");
        _logger.LogInformation("Handling GetCorporateEventRates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event", request.CorporateEventId,
            "GetCorporateEventRates", "accepted", token);
        var response = new ApiCorporateevents.GetCorporateEventRatesResponse
        {
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                CorporateEventId = (request.CorporateEventId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Quotes a corporate event.
    /// </summary>
    public override async Task<ApiCorporateevents.QuoteCorporateEventResponse> QuoteCorporateEvent(ApiCorporateevents.QuoteCorporateEventRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling QuoteCorporateEvent for {Tenant}", requestContext.TenantId);
        if (request.CorporateEvent is { } record)
        {
            var snapshot = CorporateEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiAuthorization.FindAccessPoliciesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiAuthorization.FindAccessPoliciesRequest.Types.Filter
            {
                CreatedAfter = (request.CorporateEvent?.Audit?.UpdatedAt ?? new Timestamp()),
                Region = (request.Details?.Destination?.Region ?? ""),
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiAuthorization.FindAccessPoliciesRequest>();
        using var rpc0 = _authorizationAccessPolicyQueryService.FindAccessPoliciesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDevices.EstimateDevicePolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.EstimateDevicePolicyRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Locality = (request.Details?.Destination?.Locality ?? ""),
                },
            },
        };
        var json1 = JsonFormatter.Default.Format(input1);
        input1 = JsonParser.Default.Parse<ApiDevices.EstimateDevicePolicyRequest>(json1);
        using var rpc1 = _devicesDevicePolicyCalculationService.EstimateDevicePolicyAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDocuments.UnsubscribeDocumentFolderUpdatesRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.UnsubscribeDocumentFolderUpdatesRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (reply1.DevicePolicy?.Owner?.Email ?? ""),
                },
                Variables = { (reply1.DevicePolicy?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Reason = (reply1.DevicePolicy?.Block?.Reason ?? ""),
        };
        using var rpc2 = _documentsDocumentFolderNotificationService.UnsubscribeDocumentFolderUpdatesAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        var input3 = new ApiItsm.PreviewServiceAssetRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Address = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.CorporateEvent?.Location?.CountryCode ?? ""),
                    Lines = { (request.CorporateEvent?.Location?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.CorporateEvent?.Audit?.Etag ?? ""),
                },
            },
        };
        using var rpc3 = _itsmServiceAssetValidationService.PreviewServiceAssetAsync(input3, cancellationToken: token);
        var pending3 = rpc3.ResponseAsync;
        await Task.WhenAll(pending2, pending3);
        var reply2 = await pending2;
        var reply3 = await pending3;
        var response = new ApiCorporateevents.QuoteCorporateEventResponse
        {
            Breakdown = new ApiCorporateevents.QuoteCorporateEventResponse.Types.Breakdown
            {
                Lines = { new ApiCorporateevents.QuoteCorporateEventResponse.Types.Breakdown.Types.Line
                {
                    Amount = (reply1.DevicePolicy?.Discount?.Amount ?? new ApiCommon.Money()),
                } },
                Total = (reply1.Breakdown?.Total ?? new ApiCommon.Money()),
            },
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                CorporateEventId = (request.CorporateEventId ?? ""),
                Location = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply2.DocumentFolder?.Location?.CountryCode ?? ""),
                    Lines = { (reply3.ServiceAsset?.Address?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Recipient = (reply3.ServiceAsset?.Address?.Recipient ?? ""),
                },
                Metadata = { (request.CorporateEvent?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event",
            "audit", "QuoteCorporateEvent", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalculates a corporate event.
    /// </summary>
    public override async Task<ApiCorporateevents.RecalculateCorporateEventsResponse> RecalculateCorporateEvents(ApiCorporateevents.RecalculateCorporateEventsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecalculateCorporateEvents for {Tenant}", requestContext.TenantId);
        if (request.CorporateEvent is { } record)
        {
            var snapshot = CorporateEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiMetrics.CheckMetricSeriesEligibilityResponse();
        foreach (var item in (request.CorporateEvent?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiCorporateevents.CorporateEventParticipant>()))
        {
            var input0 = new ApiMetrics.CheckMetricSeriesEligibilityRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiMetrics.CheckMetricSeriesEligibilityRequest.Types.Details
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (item.Contact?.Email ?? ""),
                        Phone = (item.Contact?.Phone ?? ""),
                    },
                },
            };
            using var rpc0 = _metricsMetricSeriesValidationService.CheckMetricSeriesEligibilityAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var input1 = new ApiItsm.EstimateIncidentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.EstimateIncidentRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.CorporateEvent?.Location?.CountryCode ?? ""),
                    Lines = { (reply0.MetricSeries ?? new ApiMetrics.MetricSeries()).LocatorCase switch
                    {
                        ApiMetrics.MetricSeries.LocatorOneofCase.Address => (reply0.MetricSeries?.Address?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()),
                        _ => new Google.Protobuf.Collections.RepeatedField<string>()
                    } },
                },
            },
            Incident = new ApiItsm.Incident
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.CorporateEvent?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        using var rpc1 = _itsmIncidentCalculationService.EstimateIncidentAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var batch2 = (request.CorporateEvent?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiCorporateevents.CorporateEventParticipant>()).Select(async item =>
        {
            var input2 = new ApiLicensing.NormalizeLicenseAssignmentRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiLicensing.NormalizeLicenseAssignmentRequest.Types.Details
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (item.Contact?.Email ?? ""),
                        Name = (item.Contact?.Name ?? new ApiCommon.PersonName()),
                        Phone = (item.Contact?.Phone ?? ""),
                        PreferredChannel = (item.Contact?.PreferredChannel ?? ""),
                    },
                },
            };
            using var rpc2 = _licensingLicenseAssignmentValidationService.NormalizeLicenseAssignmentAsync(input2, cancellationToken: token);
            var reply2 = await rpc2.ResponseAsync;
            return reply2;
        }).ToArray();
        var replies2 = await Task.WhenAll(batch2);
        var reply2 = replies2.LastOrDefault() ?? new ApiLicensing.NormalizeLicenseAssignmentResponse();
        var input3 = new ApiItsm.SimulateProblemRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Problem = new ApiItsm.Problem
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.MetricSeries?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Customer = new ApiEmployees.EmployeeRef
                {
                    Email = (reply0.MetricSeries?.Owner?.Email ?? ""),
                },
                Sla = new ApiItsm.ProblemSla
                {
                    DueAt = (reply1.Incident?.Sla?.DueAt ?? new Timestamp()),
                },
            },
        };
        using var rpc3 = _sendRecalculateCorporateEvents3(input3, new CallOptions(cancellationToken: token));
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiCorporateevents.RecalculateCorporateEventsResponse
        {
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                CorporateEventId = (request.CorporateEventId ?? ""),
                Title = (request.CorporateEvent?.Title ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply3.Result?.CorrelationId ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event",
            "audit", "RecalculateCorporateEvents", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Simulates a corporate event.
    /// </summary>
    public override async Task<ApiCorporateevents.SimulateCorporateEventResponse> SimulateCorporateEvent(ApiCorporateevents.SimulateCorporateEventRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SimulateCorporateEvent for {Tenant}", requestContext.TenantId);
        if (request.CorporateEvent is { } record)
        {
            var snapshot = CorporateEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiAuthorization.FindAccessPoliciesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiAuthorization.FindAccessPoliciesRequest.Types.Filter
            {
                CreatedAfter = (request.CorporateEvent?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Region = (request.CorporateEvent?.Location?.Region ?? ""),
            },
        };
        var named0 = _registry.Get<ApiAuthorization.AccessPolicyQueryService.AccessPolicyQueryServiceClient>("acme.authorization.v1.AccessPolicyQueryService");
        using var rpc0 = named0.FindAccessPoliciesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.SearchSoftwareLicensesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLicensing.SearchSoftwareLicensesRequest.Types.Filter
            {
                CreatedAfter = (request.CorporateEvent?.Audit?.UpdatedAt ?? new Timestamp()),
                OwnerId = (request.CorporateEvent?.Organizer?.UserId ?? ""),
                Region = (request.CorporateEvent?.Location?.Region ?? ""),
            },
        };
        var descriptor1 = new Method<ApiLicensing.SearchSoftwareLicensesRequest, ApiLicensing.SearchSoftwareLicensesResponse>(
            MethodType.Unary, "acme.licensing.v1" + ".SoftwareLicenseService", "SearchSoftwareLicenses",
            Wire.Marshaller<ApiLicensing.SearchSoftwareLicensesRequest>(), Wire.Marshaller<ApiLicensing.SearchSoftwareLicensesResponse>());
        using var rpc1 = _invoker.AsyncUnaryCall(descriptor1, null,
            new CallOptions(cancellationToken: token), input1);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiCorporateevents.SimulateCorporateEventResponse
        {
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                CorporateEventId = (request.CorporateEventId ?? ""),
                Window = (request.CorporateEvent?.Window ?? new ApiCommon.TimeWindow()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event",
            "audit", "SimulateCorporateEvent", (request.Reason ?? ""), token);
        return response;
    }
}
