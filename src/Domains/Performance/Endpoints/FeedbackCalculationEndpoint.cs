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
using ApiAnomalies = Acme.Anomalies.V1;
using ApiAudiences = Acme.Audiences.V1;
using ApiAudit = Acme.Audit.V1;
using ApiAuthorization = Acme.Authorization.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLabeling = Acme.Labeling.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTravel = Acme.Travel.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Performance;

/// <summary>
/// Computes the figures of a feedback, such as totals, taxes, and estimates.
/// </summary>
public sealed class FeedbackCalculationEndpoint : ApiPerformance.FeedbackCalculationService.FeedbackCalculationServiceBase
{
    private readonly ApiAnomalies.AnomalyService.AnomalyServiceClient _anomaliesAnomalyService;
    private readonly ApiAudiences.AudienceQueryService.AudienceQueryServiceClient _audiencesAudienceQueryService;
    private readonly ApiAudit.AuditEventQueryService.AuditEventQueryServiceClient _auditAuditEventQueryService;
    private readonly ApiAuthorization.AccessPolicyService.AccessPolicyServiceClient _authorizationAccessPolicyService;
    private readonly ApiCorporateevents.RegistrationValidationService.RegistrationValidationServiceClient _corporateeventsRegistrationValidationService;
    private readonly ApiDevices.ComplianceScanValidationService.ComplianceScanValidationServiceClient _devicesComplianceScanValidationService;
    private readonly ApiDocuments.DocumentExportService.DocumentExportServiceClient _documentsDocumentExportService;
    private readonly ApiDocuments.DocumentFolderQueryService.DocumentFolderQueryServiceClient _documentsDocumentFolderQueryService;
    private readonly ApiDocuments.DocumentValidationService.DocumentValidationServiceClient _documentsDocumentValidationService;
    private readonly ApiDocuments.SignatureSyncService.SignatureSyncServiceClient _documentsSignatureSyncService;
    private readonly ApiFeatureflags.FeatureFlagValidationService.FeatureFlagValidationServiceClient _featureflagsFeatureFlagValidationService;
    private readonly IFeedbackCalculationEndpointSendCompareFeedbackOptions0Port _feedbackCalculationEndpointSendCompareFeedbackOptions0Port;
    private readonly IFeedbackCalculationEndpointSendSimulateFeedback0Port _feedbackCalculationEndpointSendSimulateFeedback0Port;
    private readonly ApiHelpdesk.EscalationValidationService.EscalationValidationServiceClient _helpdeskEscalationValidationService;
    private readonly ApiHelpdesk.TicketQueryService.TicketQueryServiceClient _helpdeskTicketQueryService;
    private readonly ApiLabeling.LabelingTaskQueryService.LabelingTaskQueryServiceClient _labelingLabelingTaskQueryService;
    private readonly ILogger<FeedbackCalculationEndpoint> _logger;
    private readonly ApiMetrics.MetricSeriesService.MetricSeriesServiceClient _metricsMetricSeriesService;
    private readonly ApiNotifications.NotificationValidationService.NotificationValidationServiceClient _notificationsNotificationValidationService;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiAuthorization.BatchGetAccessPoliciesRequest, CallOptions, AsyncUnaryCall<ApiAuthorization.BatchGetAccessPoliciesResponse>> _sendQuoteFeedback0;
    private readonly ApiTravel.TravelBookingCalculationService.TravelBookingCalculationServiceClient _travelTravelBookingCalculationService;
    private readonly ApiVenues.VenueValidationService.VenueValidationServiceClient _venuesVenueValidationService;

    public FeedbackCalculationEndpoint(
        ApiAnomalies.AnomalyService.AnomalyServiceClient anomaliesAnomalyService,
        ApiAudiences.AudienceQueryService.AudienceQueryServiceClient audiencesAudienceQueryService,
        ApiAudit.AuditEventQueryService.AuditEventQueryServiceClient auditAuditEventQueryService,
        ApiAuthorization.AccessPolicyService.AccessPolicyServiceClient authorizationAccessPolicyService,
        ApiCorporateevents.RegistrationValidationService.RegistrationValidationServiceClient corporateeventsRegistrationValidationService,
        ApiDevices.ComplianceScanValidationService.ComplianceScanValidationServiceClient devicesComplianceScanValidationService,
        ApiDocuments.DocumentExportService.DocumentExportServiceClient documentsDocumentExportService,
        ApiDocuments.DocumentFolderQueryService.DocumentFolderQueryServiceClient documentsDocumentFolderQueryService,
        ApiDocuments.DocumentValidationService.DocumentValidationServiceClient documentsDocumentValidationService,
        ApiDocuments.SignatureSyncService.SignatureSyncServiceClient documentsSignatureSyncService,
        ApiFeatureflags.FeatureFlagValidationService.FeatureFlagValidationServiceClient featureflagsFeatureFlagValidationService,
        IFeedbackCalculationEndpointSendCompareFeedbackOptions0Port feedbackCalculationEndpointSendCompareFeedbackOptions0PortAdapter,
        IFeedbackCalculationEndpointSendSimulateFeedback0Port feedbackCalculationEndpointSendSimulateFeedback0PortAdapter,
        ApiHelpdesk.EscalationValidationService.EscalationValidationServiceClient helpdeskEscalationValidationService,
        ApiHelpdesk.TicketQueryService.TicketQueryServiceClient helpdeskTicketQueryService,
        ApiLabeling.LabelingTaskQueryService.LabelingTaskQueryServiceClient labelingLabelingTaskQueryService,
        ILogger<FeedbackCalculationEndpoint> logger,
        ApiMetrics.MetricSeriesService.MetricSeriesServiceClient metricsMetricSeriesService,
        ApiNotifications.NotificationValidationService.NotificationValidationServiceClient notificationsNotificationValidationService,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiTravel.TravelBookingCalculationService.TravelBookingCalculationServiceClient travelTravelBookingCalculationService,
        ApiVenues.VenueValidationService.VenueValidationServiceClient venuesVenueValidationService)
    {
        _anomaliesAnomalyService = anomaliesAnomalyService;
        _audiencesAudienceQueryService = audiencesAudienceQueryService;
        _auditAuditEventQueryService = auditAuditEventQueryService;
        _authorizationAccessPolicyService = authorizationAccessPolicyService;
        _corporateeventsRegistrationValidationService = corporateeventsRegistrationValidationService;
        _devicesComplianceScanValidationService = devicesComplianceScanValidationService;
        _documentsDocumentExportService = documentsDocumentExportService;
        _documentsDocumentFolderQueryService = documentsDocumentFolderQueryService;
        _documentsDocumentValidationService = documentsDocumentValidationService;
        _documentsSignatureSyncService = documentsSignatureSyncService;
        _featureflagsFeatureFlagValidationService = featureflagsFeatureFlagValidationService;
        _feedbackCalculationEndpointSendCompareFeedbackOptions0Port = feedbackCalculationEndpointSendCompareFeedbackOptions0PortAdapter;
        _feedbackCalculationEndpointSendSimulateFeedback0Port = feedbackCalculationEndpointSendSimulateFeedback0PortAdapter;
        _helpdeskEscalationValidationService = helpdeskEscalationValidationService;
        _helpdeskTicketQueryService = helpdeskTicketQueryService;
        _labelingLabelingTaskQueryService = labelingLabelingTaskQueryService;
        _logger = logger;
        _metricsMetricSeriesService = metricsMetricSeriesService;
        _notificationsNotificationValidationService = notificationsNotificationValidationService;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _travelTravelBookingCalculationService = travelTravelBookingCalculationService;
        _venuesVenueValidationService = venuesVenueValidationService;
        _sendQuoteFeedback0 = _authorizationAccessPolicyService.BatchGetAccessPoliciesAsync;
    }
    /// <summary>
    /// Calculates a feedback.
    /// </summary>
    public override async Task<ApiPerformance.CalculateFeedbackResponse> CalculateFeedback(ApiPerformance.CalculateFeedbackRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CalculateFeedback for {Tenant}", requestContext.TenantId);
        if (request.Feedback is { } record)
        {
            var snapshot = FeedbackMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiAudiences.ExportAudiencesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiAudiences.ExportAudiencesRequest.Types.Filter
            {
                CreatedAfter = (request.Feedback?.OccurredAt ?? new Timestamp()),
                Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Feedback?.CorrelationId ?? ""),
                Region = (request.Details?.Destination?.Region ?? ""),
            },
        };
        using var stream0 = _audiencesAudienceQueryService.ExportAudiences(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportAudiences frame {Type}", change0.GetType().Name);
        }
        var input1 = new ApiDocuments.PushSignatureRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.PushSignatureRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Signature = new ApiDocuments.Signature
            {
                CorrelationId = (request.Feedback?.CorrelationId ?? ""),
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Sequence = (request.Feedback?.Sequence ?? 0L),
                Subject = new ApiDocuments.DocumentRef
                {
                    DisplayName = (request.Feedback?.Subject?.DisplayName ?? ""),
                },
            },
        };
        using var rpc1 = _documentsSignatureSyncService.PushSignatureAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiAudit.ResolveAuditEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc2 = _auditAuditEventQueryService.ResolveAuditEventsAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        var input3 = new ApiAuthorization.CloneAccessPolicyRequest
        {
            AccessPolicy = new ApiAuthorization.AccessPolicy
            {
                Block = new ApiAuthorization.AccessPolicyBlockAction
                {
                    Reason = (request.Reason ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.Signature?.Subject?.DisplayName ?? ""),
                    Email = (request.Feedback?.Subject?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc3 = _authorizationAccessPolicyService.CloneAccessPolicyAsync(input3, cancellationToken: token);
        var pending3 = rpc3.ResponseAsync;
        await Task.WhenAll(pending2, pending3);
        var reply2 = await pending2;
        var reply3 = await pending3;
        var response = new ApiPerformance.CalculateFeedbackResponse
        {
            Breakdown = new ApiPerformance.CalculateFeedbackResponse.Types.Breakdown
            {
                CurrencyCode = (request.Details?.CurrencyCode ?? ""),
            },
            Feedback = new ApiPerformance.Feedback
            {
                Attributes = { (reply1.Signature?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
                FeedbackId = (request.FeedbackId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Feedback?.Subject?.CandidateId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "feedback",
            "audit", "CalculateFeedback", (request.Feedback?.Subject?.CandidateId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Compares a feedback.
    /// </summary>
    public override async Task<ApiPerformance.CompareFeedbackOptionsResponse> CompareFeedbackOptions(ApiPerformance.CompareFeedbackOptionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CompareFeedbackOptions for {Tenant}", requestContext.TenantId);
        if (request.Feedback is { } record)
        {
            var snapshot = FeedbackMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiFeatureflags.ValidateFeatureFlagRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiFeatureflags.ValidateFeatureFlagRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Feedback?.Subject?.Email ?? ""),
                },
            },
        };
        var reply0 = await SendCompareFeedbackOptions0Async(input0, token);
        var input1 = new ApiMetrics.ListMetricSeriesesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiMetrics.ListMetricSeriesesRequest.Types.Filter
            {
                CreatedAfter = (reply0.Outcome?.EffectiveAt ?? new Timestamp()),
                Labels = { (request.Feedback?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply0.Outcome?.ReferenceId ?? ""),
                Region = (request.Details?.Destination?.Region ?? ""),
            },
        };
        var labels1 = input1.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input1.Context.Baggage.Clear();
        input1.Context.Baggage.Add(labels1);
        using var rpc1 = _metricsMetricSeriesService.ListMetricSeriesesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiPerformance.CompareFeedbackOptionsResponse
        {
            Feedback = new ApiPerformance.Feedback
            {
                FeedbackId = (request.FeedbackId ?? ""),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (reply0.FeatureFlag?.Owner?.DisplayName ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (request.Feedback?.CorrelationId ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "feedback",
            "audit", "CompareFeedbackOptions", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Estimates a feedback.
    /// </summary>
    public override async Task<ApiPerformance.EstimateFeedbackResponse> EstimateFeedback(ApiPerformance.EstimateFeedbackRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling EstimateFeedback for {Tenant}", requestContext.TenantId);
        if (request.Feedback is { } record)
        {
            var snapshot = FeedbackMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        foreach (var item in (request.Feedback?.Measurements ?? new Google.Protobuf.Collections.RepeatedField<ApiPerformance.FeedbackMeasurement>()))
        {
            var input0 = new ApiDocuments.ExportDocumentFoldersRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiDocuments.ExportDocumentFoldersRequest.Types.Filter
                {
                    CreatedAfter = (item.ObservedAt ?? new Timestamp()),
                    OwnerId = (request.Feedback?.Subject?.CandidateId ?? ""),
                    Region = (request.Details?.Destination?.Region ?? ""),
                },
            };
            using var stream0 = _documentsDocumentFolderQueryService.ExportDocumentFolders(input0, cancellationToken: token);
            await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
            {
                _logger.LogDebug("Received ExportDocumentFolders frame {Type}", change0.GetType().Name);
            }
        }
        var input1 = new ApiNotifications.NormalizeNotificationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Notification = new ApiNotifications.Notification
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Feedback?.Subject?.DisplayName ?? ""),
                    Email = (request.Feedback?.Subject?.Email ?? ""),
                },
            },
            Reason = (request.Reason ?? ""),
        };
        var json1 = JsonFormatter.Default.Format(input1);
        input1 = JsonParser.Default.Parse<ApiNotifications.NormalizeNotificationRequest>(json1);
        using var rpc1 = _notificationsNotificationValidationService.NormalizeNotificationAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiTravel.RecalculateTravelBookingsRequest();
        input2.Context = (request.Context ?? new ApiCommon.RequestContext());
        input2.Details = new ApiTravel.RecalculateTravelBookingsRequest.Types.Details
        {
            Destination = new ApiCommon.PostalAddress
            {
                CountryCode = (request.Details?.Destination?.CountryCode ?? ""),
            },
        };
        input2.TravelBooking = new ApiTravel.TravelBooking
        {
            BillingAddress = new ApiCommon.PostalAddress
            {
                Lines = { (request.Details?.Destination?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Region = (request.Details?.Destination?.Region ?? ""),
            },
        };
        using var rpc2 = _travelTravelBookingCalculationService.RecalculateTravelBookingsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiPerformance.EstimateFeedbackResponse
        {
            Feedback = new ApiPerformance.Feedback
            {
                FeedbackId = (request.FeedbackId ?? ""),
                OccurredAt = (request.Feedback?.OccurredAt ?? new Timestamp()),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (reply2.TravelBooking?.Counterparty?.DisplayName ?? ""),
                    Email = (reply1.Notification?.Requester?.Email ?? ""),
                },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Feedback?.Source ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "feedback",
            "audit", "EstimateFeedback", (request.Feedback?.Source ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a feedback.
    /// </summary>
    public override async Task<ApiPerformance.ExplainFeedbackCalculationResponse> ExplainFeedbackCalculation(ApiPerformance.ExplainFeedbackCalculationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ExplainFeedbackCalculation for {Tenant}", requestContext.TenantId);
        if (request.Feedback is { } record)
        {
            var snapshot = FeedbackMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiAudit.SuggestAuditEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiAudit.SuggestAuditEventsRequest.Types.Filter
            {
                OwnerId = (request.Feedback?.Subject?.CandidateId ?? ""),
            },
        };
        using var rpc0 = _auditAuditEventQueryService.SuggestAuditEventsAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiHelpdesk.CountTicketsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiHelpdesk.CountTicketsRequest.Types.Filter
            {
                CreatedAfter = (request.Feedback?.OccurredAt ?? new Timestamp()),
                Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Feedback?.Subject?.CandidateId ?? ""),
            },
        };
        using var rpc1 = _helpdeskTicketQueryService.CountTicketsAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var input2 = new ApiDocuments.ListDocumentExportsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListDocumentExportsRequest.Types.Filter
            {
                CreatedAfter = (request.Feedback?.OccurredAt ?? new Timestamp()),
            },
        };
        var reply2 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _documentsDocumentExportService.ListDocumentExportsAsync(input2, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiPerformance.ExplainFeedbackCalculationResponse
        {
            Feedback = new ApiPerformance.Feedback
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                FeedbackId = (request.FeedbackId ?? ""),
                OccurredAt = (request.Feedback?.OccurredAt ?? new Timestamp()),
            },
            Outcome = new ApiPerformance.ExplainFeedbackCalculationResponse.Types.Outcome
            {
                Status = (reply2.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "feedback",
            "audit", "ExplainFeedbackCalculation", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a feedback.
    /// </summary>
    public override async Task<ApiPerformance.GetFeedbackRatesResponse> GetFeedbackRates(ApiPerformance.GetFeedbackRatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.FeedbackId, "feedback_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetFeedbackRates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "feedback", request.FeedbackId,
            "GetFeedbackRates", "accepted", token);
        var input0 = new ApiDevices.NormalizeComplianceScanRequest
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Measurements = { new ApiDevices.ComplianceScanMeasurement
                {
                    ObservedAt = (request.Context?.Deadline ?? new Timestamp()),
                } },
                Subject = new ApiRecruiting.CandidateRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.NormalizeComplianceScanRequest.Types.Details
            {
                Locale = (request.Context?.Locale ?? ""),
            },
        };
        using var rpc0 = _devicesComplianceScanValidationService.NormalizeComplianceScanAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiPerformance.GetFeedbackRatesResponse
        {
            Feedback = new ApiPerformance.Feedback
            {
                FeedbackId = (request.FeedbackId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.ComplianceScan?.CorrelationId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Quotes a feedback.
    /// </summary>
    public override async Task<ApiPerformance.QuoteFeedbackResponse> QuoteFeedback(ApiPerformance.QuoteFeedbackRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling QuoteFeedback for {Tenant}", requestContext.TenantId);
        if (request.Feedback is { } record)
        {
            var snapshot = FeedbackMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiAuthorization.BatchGetAccessPoliciesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _sendQuoteFeedback0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiVenues.CheckVenueEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiVenues.CheckVenueEligibilityRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Feedback?.Subject?.Email ?? ""),
                },
            },
            Venue = new ApiVenues.Venue
            {
                Address = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Details?.Destination?.CountryCode ?? ""),
                },
            },
        };
        var named1 = _registry.Get<ApiVenues.VenueValidationService.VenueValidationServiceClient>("acme.venues.v1.VenueValidationService");
        using var rpc1 = named1.CheckVenueEligibilityAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var batch2 = (request.Feedback?.Measurements ?? new Google.Protobuf.Collections.RepeatedField<ApiPerformance.FeedbackMeasurement>()).Select(async item =>
        {
            var input2 = new ApiHelpdesk.NormalizeEscalationRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiHelpdesk.NormalizeEscalationRequest.Types.Details
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (reply1.Venue?.Owner?.Email ?? ""),
                    },
                },
                Escalation = new ApiHelpdesk.Escalation
                {
                    Assignee = new ApiIdentity.UserRef
                    {
                        UserId = (reply1.Venue?.Owner?.UserId ?? ""),
                    },
                    Audit = new ApiCommon.AuditInfo
                    {
                        UpdatedAt = (reply1.Venue?.Audit?.UpdatedAt ?? new Timestamp()),
                    },
                    Sla = new ApiHelpdesk.EscalationSla
                    {
                        DueAt = (item.ObservedAt ?? new Timestamp()),
                    },
                },
            };
            using var rpc2 = _helpdeskEscalationValidationService.NormalizeEscalationAsync(input2, cancellationToken: token);
            var reply2 = await rpc2.ResponseAsync;
            return reply2;
        }).ToArray();
        var replies2 = await Task.WhenAll(batch2);
        var reply2 = replies2.LastOrDefault() ?? new ApiHelpdesk.NormalizeEscalationResponse();
        var response = new ApiPerformance.QuoteFeedbackResponse
        {
            Breakdown = new ApiPerformance.QuoteFeedbackResponse.Types.Breakdown
            {
                CurrencyCode = (request.Details?.CurrencyCode ?? ""),
            },
            Feedback = new ApiPerformance.Feedback
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                FeedbackId = (request.FeedbackId ?? ""),
                Subject = new ApiRecruiting.CandidateRef
                {
                    Email = (reply1.Venue?.Owner?.Email ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "feedback",
            "audit", "QuoteFeedback", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalculates a feedback.
    /// </summary>
    public override async Task<ApiPerformance.RecalculateFeedbacksResponse> RecalculateFeedbacks(ApiPerformance.RecalculateFeedbacksRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RecalculateFeedbacks for {Tenant}", requestContext.TenantId);
        if (request.Feedback is { } record)
        {
            var snapshot = FeedbackMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiMetrics.WatchMetricSeriesesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Subscriber = new ApiCommon.ContactInfo
            {
                Email = (request.Feedback?.Subject?.Email ?? ""),
            },
        };
        using var stream0 = _metricsMetricSeriesService.WatchMetricSerieses(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received WatchMetricSerieses frame {Type}", change0.GetType().Name);
        }
        var reply1 = new ApiAnomalies.UpdateAnomalyResponse();
        foreach (var item in (request.Feedback?.Measurements ?? new Google.Protobuf.Collections.RepeatedField<ApiPerformance.FeedbackMeasurement>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input1 = new ApiAnomalies.UpdateAnomalyRequest
            {
                Anomaly = new ApiAnomalies.Anomaly
                {
                    Assignee = new ApiIdentity.UserRef
                    {
                        DisplayName = (request.Feedback?.Subject?.DisplayName ?? ""),
                        UserId = (request.Context?.Actor?.UserId ?? ""),
                    },
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedAt = (item.ObservedAt ?? new Timestamp()),
                    },
                    Requester = new ApiCommon.ContactInfo
                    {
                        Email = (request.Feedback?.Subject?.Email ?? ""),
                    },
                },
                Context = (request.Context ?? new ApiCommon.RequestContext()),
            };
            using var rpc1 = _anomaliesAnomalyService.UpdateAnomalyAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var input2 = new ApiAuthorization.UpdateAccessPolicyRequest
        {
            AccessPolicy = new ApiAuthorization.AccessPolicy
            {
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.Anomaly?.Assignee?.DisplayName ?? ""),
                    Email = (reply1.Anomaly?.Assignee?.Email ?? ""),
                    UserId = (reply1.Anomaly?.Assignee?.UserId ?? ""),
                },
                Version = (reply1.Anomaly ?? new ApiAnomalies.Anomaly()).OriginCase switch
                {
                    ApiAnomalies.Anomaly.OriginOneofCase.Customer => (reply1.Anomaly?.Customer?.Version ?? 0L),
                    _ => 0L
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc2 = _authorizationAccessPolicyService.UpdateAccessPolicyAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = request.RecalculateFeedbacksToCountLabelingTasks3();
        using var rpc3 = _labelingLabelingTaskQueryService.CountLabelingTasksAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiPerformance.RecalculateFeedbacksResponse
        {
            Feedback = new ApiPerformance.Feedback
            {
                CorrelationId = (request.Feedback?.CorrelationId ?? ""),
                FeedbackId = (request.FeedbackId ?? ""),
                Subject = new ApiRecruiting.CandidateRef
                {
                    Email = (reply2.AccessPolicy?.Owner?.Email ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply3.Result?.CorrelationId ?? ""),
                Message = (reply3.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "feedback",
            "audit", "RecalculateFeedbacks", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Simulates a feedback.
    /// </summary>
    public override async Task<ApiPerformance.SimulateFeedbackResponse> SimulateFeedback(ApiPerformance.SimulateFeedbackRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SimulateFeedback for {Tenant}", requestContext.TenantId);
        if (request.Feedback is { } record)
        {
            var snapshot = FeedbackMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.EstimateTravelBookingRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.EstimateTravelBookingRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Recipient = (request.Details?.Destination?.Recipient ?? ""),
                    Region = (request.Details?.Destination?.Region ?? ""),
                },
            },
            TravelBooking = new ApiTravel.TravelBooking
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Details?.Destination?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
        };
        var reply0 = await SendSimulateFeedback0Async(input0, token);
        var input1 = new ApiCorporateevents.NormalizeRegistrationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.NormalizeRegistrationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply0.TravelBooking?.Counterparty?.Email ?? ""),
                },
            },
            Registration = new ApiCorporateevents.Registration
            {
                Card = new ApiCorporateevents.RegistrationCardFunding
                {
                    HolderName = (reply0.TravelBooking ?? new ApiTravel.TravelBooking()).PaymentCase switch
                    {
                        ApiTravel.TravelBooking.SettlementOneofCase.Card => (reply0.TravelBooking?.Card?.HolderName ?? ""),
                        _ => ""
                    },
                    Token = (reply0.TravelBooking ?? new ApiTravel.TravelBooking()).PaymentCase switch
                    {
                        ApiTravel.TravelBooking.SettlementOneofCase.Card => (reply0.TravelBooking?.Card?.Token ?? ""),
                        _ => ""
                    },
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Feedback?.Subject?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _corporateeventsRegistrationValidationService.NormalizeRegistrationAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDocuments.NormalizeDocumentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Document = new ApiDocuments.Document
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply1.Registration?.Audit?.CreatedBy ?? ""),
                    UpdatedBy = (reply0.TravelBooking?.Audit?.UpdatedBy ?? ""),
                },
                Bank = new ApiDocuments.DocumentBankSettlement
                {
                    Bic = (reply0.TravelBooking ?? new ApiTravel.TravelBooking()).PaymentCase switch
                    {
                        ApiTravel.TravelBooking.SettlementOneofCase.Bank => (reply0.TravelBooking?.Bank?.Bic ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc2 = _documentsDocumentValidationService.NormalizeDocumentAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiPerformance.SimulateFeedbackResponse
        {
            Feedback = new ApiPerformance.Feedback
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                FeedbackId = (request.FeedbackId ?? ""),
                Source = (request.Feedback?.Source ?? ""),
                Subject = new ApiRecruiting.CandidateRef
                {
                    Email = (reply1.Registration?.Contact?.Email ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply2.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "feedback",
            "audit", "SimulateFeedback", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiFeatureflags.ValidateFeatureFlagResponse> SendCompareFeedbackOptions0Async(ApiFeatureflags.ValidateFeatureFlagRequest request, CancellationToken token)
    {
        return await _feedbackCalculationEndpointSendCompareFeedbackOptions0Port.SendAsync(request, token);
    }
    private async Task<ApiTravel.EstimateTravelBookingResponse> SendSimulateFeedback0Async(ApiTravel.EstimateTravelBookingRequest request, CancellationToken token)
    {
        return await _feedbackCalculationEndpointSendSimulateFeedback0Port.SendAsync(request, token);
    }
}
