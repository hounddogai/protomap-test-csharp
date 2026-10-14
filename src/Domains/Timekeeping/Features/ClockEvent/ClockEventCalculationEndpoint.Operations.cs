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
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiShows = Acme.Shows.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Timekeeping;

public sealed partial class ClockEventCalculationEndpoint
{
    /// <summary>
    /// Calculates a clock event.
    /// </summary>
    public override async Task<ApiTimekeeping.CalculateClockEventResponse> CalculateClockEvent(ApiTimekeeping.CalculateClockEventRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CalculateClockEvent for {Tenant}", requestContext.TenantId);
        if (request.ClockEvent is { } record)
        {
            var snapshot = ClockEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiFeatureflags.ExportFeatureFlagsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiFeatureflags.ExportFeatureFlagsRequest.Types.Filter
            {
                Region = (request.Details?.Destination?.Region ?? ""),
            },
        };
        using var stream0 = _featureflagsFeatureFlagQueryService.ExportFeatureFlags(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportFeatureFlags frame {Type}", change0.GetType().Name);
        }
        var response = new ApiTimekeeping.CalculateClockEventResponse
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                ClockEventId = (request.ClockEventId ?? ""),
                Subject = (request.ClockEvent?.Subject ?? new ApiTimekeeping.TimesheetRef()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "clock_event",
            "audit", "CalculateClockEvent", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Estimates a clock event.
    /// </summary>
    public override async Task<ApiTimekeeping.EstimateClockEventResponse> EstimateClockEvent(ApiTimekeeping.EstimateClockEventRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EstimateClockEvent for {Tenant}", requestContext.TenantId);
        if (request.ClockEvent is { } record)
        {
            var snapshot = ClockEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        foreach (var item in (request.ClockEvent?.Measurements ?? new Google.Protobuf.Collections.RepeatedField<ApiTimekeeping.ClockEventMeasurement>()))
        {
            var input0 = new ApiShows.ExportShowsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiShows.ExportShowsRequest.Types.Filter
                {
                    CreatedAfter = (item.ObservedAt ?? new Timestamp()),
                    Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                    OwnerId = (request.ClockEvent?.Subject?.TimesheetId ?? ""),
                    Region = (request.Details?.Destination?.Region ?? ""),
                },
            };
            using var stream0 = _showsShowQueryService.ExportShows(input0, cancellationToken: token);
            await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
            {
                _logger.LogDebug("Received ExportShows frame {Type}", change0.GetType().Name);
            }
        }
        var input1 = new ApiNotifications.ScoreNotificationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiNotifications.ScoreNotificationRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Details?.Destination?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    PostalCode = (request.Details?.Destination?.PostalCode ?? ""),
                },
            },
            Notification = new ApiNotifications.Notification
            {
                Customer = new ApiNotifications.NotificationTemplateRef
                {
                    DisplayName = (request.ClockEvent?.Subject?.DisplayName ?? ""),
                },
            },
        };
        var reply1 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _notificationsNotificationValidationService.ScoreNotificationAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiTimekeeping.EstimateClockEventResponse
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                Attributes = { (request.ClockEvent?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ClockEventId = (request.ClockEventId ?? ""),
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "clock_event",
            "audit", "EstimateClockEvent", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a clock event.
    /// </summary>
    public override async Task<ApiTimekeeping.GetClockEventRatesResponse> GetClockEventRates(ApiTimekeeping.GetClockEventRatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ClockEventId, "clock_event_id");
        _logger.LogInformation("Handling GetClockEventRates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "clock_event", request.ClockEventId,
            "GetClockEventRates", "accepted", token);
        var response = new ApiTimekeeping.GetClockEventRatesResponse
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                ClockEventId = (request.ClockEventId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Quotes a clock event.
    /// </summary>
    public override async Task<ApiTimekeeping.QuoteClockEventResponse> QuoteClockEvent(ApiTimekeeping.QuoteClockEventRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling QuoteClockEvent for {Tenant}", requestContext.TenantId);
        if (request.ClockEvent is { } record)
        {
            var snapshot = ClockEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.PreviewKnowledgeArticleRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Details?.Destination?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _helpdeskKnowledgeArticleValidationService.PreviewKnowledgeArticleAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiLicensing.ValidateEntitlementRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.ValidateEntitlementRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply0.KnowledgeArticle?.Counterparty?.Email ?? ""),
                },
            },
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.KnowledgeArticle?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedAt = (reply0.KnowledgeArticle?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.ClockEvent?.Subject?.DisplayName ?? ""),
                },
            },
        };
        var reply1 = await Workflow.SendAsync(input1, token, _licensingEntitlementValidationService.ValidateEntitlementAsync);
        var response = new ApiTimekeeping.QuoteClockEventResponse
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                ClockEventId = (request.ClockEventId ?? ""),
                Sequence = (request.ClockEvent?.Sequence ?? 0L),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Message = (reply1.Outcome?.Message ?? ""),
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "clock_event",
            "audit", "QuoteClockEvent", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalculates a clock event.
    /// </summary>
    public override async Task<ApiTimekeeping.RecalculateClockEventsResponse> RecalculateClockEvents(ApiTimekeeping.RecalculateClockEventsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecalculateClockEvents for {Tenant}", requestContext.TenantId);
        if (request.ClockEvent is { } record)
        {
            var snapshot = ClockEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiLicensing.PreviewEntitlementResponse();
        foreach (var item in (request.ClockEvent?.Measurements ?? new Google.Protobuf.Collections.RepeatedField<ApiTimekeeping.ClockEventMeasurement>()))
        {
            var input0 = new ApiLicensing.PreviewEntitlementRequest
            {
                Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Entitlement = new ApiLicensing.Entitlement
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedAt = (item.ObservedAt ?? new Timestamp()),
                    },
                    Block = new ApiLicensing.EntitlementBlockAction
                    {
                        Reason = (request.Reason ?? ""),
                    },
                    Owner = new ApiIdentity.UserRef
                    {
                        DisplayName = (request.ClockEvent?.Subject?.DisplayName ?? ""),
                    },
                },
            };
            using var rpc0 = _licensingEntitlementValidationService.PreviewEntitlementAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var response = new ApiTimekeeping.RecalculateClockEventsResponse
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                ClockEventId = (request.ClockEventId ?? ""),
                Source = (request.ClockEvent?.Source ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.Destination?.Region ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "clock_event",
            "audit", "RecalculateClockEvents", (request.Details?.Destination?.Region ?? ""), token);
        return response;
    }
    /// <summary>
    /// Simulates a clock event.
    /// </summary>
    public override async Task<ApiTimekeeping.SimulateClockEventResponse> SimulateClockEvent(ApiTimekeeping.SimulateClockEventRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SimulateClockEvent for {Tenant}", requestContext.TenantId);
        if (request.ClockEvent is { } record)
        {
            var snapshot = ClockEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.RevertDocumentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.RevertDocumentRequest.Types.Details
            {
                Snapshot = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Document = new ApiDocuments.Document
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Details?.Destination?.Region ?? ""),
                },
                Lines = { new ApiDocuments.DocumentLine
                {
                    Attributes = { (request.ClockEvent?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                } },
            },
        };
        using var rpc0 = _documentsDocumentHistoryService.RevertDocumentAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var batch1 = (request.ClockEvent?.Measurements ?? new Google.Protobuf.Collections.RepeatedField<ApiTimekeeping.ClockEventMeasurement>()).Select(async item =>
        {
            var input1 = new ApiDocuments.ListDocumentTemplateConflictsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiDocuments.ListDocumentTemplateConflictsRequest.Types.Filter
                {
                    CreatedAfter = (item.ObservedAt ?? new Timestamp()),
                },
            };
            using var rpc1 = _documentsDocumentTemplateSyncService.ListDocumentTemplateConflictsAsync(input1, cancellationToken: token);
            var reply1 = await rpc1.ResponseAsync;
            return reply1;
        }).ToArray();
        var replies1 = await Task.WhenAll(batch1);
        var reply1 = replies1.LastOrDefault() ?? new ApiDocuments.ListDocumentTemplateConflictsResponse();
        var response = new ApiTimekeeping.SimulateClockEventResponse
        {
            Breakdown = new ApiTimekeeping.SimulateClockEventResponse.Types.Breakdown
            {
                CurrencyCode = (request.Details?.CurrencyCode ?? ""),
            },
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                ClockEventId = (request.ClockEventId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "clock_event",
            "audit", "SimulateClockEvent", (request.Reason ?? ""), token);
        return response;
    }
}
