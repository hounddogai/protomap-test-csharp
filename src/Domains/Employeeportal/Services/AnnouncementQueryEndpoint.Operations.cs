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
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiSurveys = Acme.Surveys.V1;

namespace Acme.Workforce.Employeeportal;

public sealed partial class AnnouncementQueryEndpoint
{
    /// <summary>
    /// Lists announcements a page at a time.
    /// </summary>
    public override async Task<ApiEmployeeportal.BrowseAnnouncementsResponse> BrowseAnnouncements(ApiEmployeeportal.BrowseAnnouncementsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling BrowseAnnouncements for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "announcement", "batch",
            "BrowseAnnouncements", "accepted", token);
        var input0 = new ApiEmployeeportal.FindSelfServiceRequestsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployeeportal.FindSelfServiceRequestsRequest.Types.Filter
            {
                OwnerId = (request.Filter?.OwnerId ?? ""),
                Region = (request.Filter?.Region ?? ""),
            },
            Page = new ApiCommon.PageRequest
            {
                PageSize = (request.Page?.PageSize ?? 0),
            },
        };
        var command0 = new DispatchCommand<ApiEmployeeportal.FindSelfServiceRequestsRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _employeeportalSelfServiceRequestQueryService.FindSelfServiceRequestsAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiEmployeeportal.BrowseAnnouncementsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "announcement",
            "audit", "BrowseAnnouncements", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Counts announcements by status.
    /// </summary>
    public override async Task<ApiEmployeeportal.CountAnnouncementsResponse> CountAnnouncements(ApiEmployeeportal.CountAnnouncementsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CountAnnouncements for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "announcement", "batch",
            "CountAnnouncements", "accepted", token);
        var input0 = new ApiDocuments.LintSignaturesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.LintSignaturesRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Locale = (request.Context?.Locale ?? ""),
            },
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                OccurredAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
            },
        };
        using var rpc0 = _documentsSignatureValidationService.LintSignaturesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiEmployeeportal.CountAnnouncementsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "announcement",
            "audit", "CountAnnouncements", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams every matching announcement.
    /// </summary>
    public override async Task ExportAnnouncements(ApiEmployeeportal.ExportAnnouncementsRequest request, IServerStreamWriter<ApiEmployeeportal.ExportAnnouncementsResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExportAnnouncements for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "announcement", "batch",
            "ExportAnnouncements", "accepted", token);
        var input0 = new ApiSurveys.ListSurveysRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiSurveys.ListSurveysRequest.Types.Filter
            {
                Labels = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Filter?.OwnerId ?? ""),
                Region = (request.Filter?.Region ?? ""),
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _surveysSurveyService.ListSurveysAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployeeportal.DisconnectPortalHomeSourceRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployeeportal.DisconnectPortalHomeSourceRequest.Types.Details
            {
                Envelope = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                MessageId = (reply0.Result?.Message ?? ""),
                ReceivedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
            },
        };
        using var rpc1 = _employeeportalPortalHomeIntegrationService.DisconnectPortalHomeSourceAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        var input2 = new ApiMetrics.ListMetricSeriesesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiMetrics.ListMetricSeriesesRequest.Types.Filter
            {
                OwnerId = (request.Filter?.OwnerId ?? ""),
            },
        };
        using var rpc2 = _metricsMetricSeriesService.ListMetricSeriesesAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        await Task.WhenAll(pending1, pending2);
        var reply1 = await pending1;
        var reply2 = await pending2;
        var response = new ApiEmployeeportal.ExportAnnouncementsResponse
        {
            Announcement = new ApiEmployeeportal.Announcement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.PortalHome?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply2.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Format ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "announcement",
            "audit", "ExportAnnouncements", (request.Format ?? ""), token);
        await responses.WriteAsync(response, token);
    }
    /// <summary>
    /// Lists announcements a page at a time.
    /// </summary>
    public override async Task<ApiEmployeeportal.FindAnnouncementsResponse> FindAnnouncements(ApiEmployeeportal.FindAnnouncementsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling FindAnnouncements for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "announcement", "batch",
            "FindAnnouncements", "accepted", token);
        var input0 = new ApiEmployeeportal.FindSelfServiceRequestsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployeeportal.FindSelfServiceRequestsRequest.Types.Filter
            {
                CreatedAfter = (request.Filter?.CreatedAfter ?? new Timestamp()),
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _employeeportalSelfServiceRequestQueryService.FindSelfServiceRequestsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiEmployeeportal.FindAnnouncementsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "announcement",
            "audit", "FindAnnouncements", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an announcement.
    /// </summary>
    public override async Task<ApiEmployeeportal.LookupAnnouncementResponse> LookupAnnouncement(ApiEmployeeportal.LookupAnnouncementRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.AnnouncementId, "announcement_id");
        _logger.LogInformation("Handling LookupAnnouncement for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "announcement", request.AnnouncementId,
            "LookupAnnouncement", "accepted", token);
        var input0 = new ApiEmployeeportal.ListAnnouncementSourcesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployeeportal.ListAnnouncementSourcesRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
                Labels = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.AnnouncementId ?? ""),
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiEmployeeportal.ListAnnouncementSourcesRequest>();
        using var rpc0 = _employeeportalAnnouncementIntegrationService.ListAnnouncementSourcesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiEmployeeportal.LookupAnnouncementResponse
        {
            Announcement = new ApiEmployeeportal.Announcement
            {
                AnnouncementId = (request.AnnouncementId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        return response;
    }
    /// <summary>
    /// Returns an announcement.
    /// </summary>
    public override async Task<ApiEmployeeportal.PeekAnnouncementResponse> PeekAnnouncement(ApiEmployeeportal.PeekAnnouncementRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.AnnouncementId, "announcement_id");
        _logger.LogInformation("Handling PeekAnnouncement for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "announcement", request.AnnouncementId,
            "PeekAnnouncement", "accepted", token);
        var input0 = new ApiEmployeeportal.AcknowledgeAnnouncementEventRequest
        {
            AnnouncementId = (request.AnnouncementId ?? ""),
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployeeportal.AcknowledgeAnnouncementEventRequest.Types.Details
            {
                Envelope = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                MessageId = (request.Context?.Actor?.UserId ?? ""),
                ReceivedAt = (request.Context?.Deadline ?? new Timestamp()),
            },
        };
        using var rpc0 = _employeeportalAnnouncementIntegrationService.AcknowledgeAnnouncementEventAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiEmployeeportal.PeekAnnouncementResponse
        {
            Announcement = new ApiEmployeeportal.Announcement
            {
                AnnouncementId = (request.AnnouncementId ?? ""),
                Total = (reply0.Announcement?.Total ?? new ApiCommon.Money()),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists announcements a page at a time.
    /// </summary>
    public override async Task<ApiEmployeeportal.SuggestAnnouncementsResponse> SuggestAnnouncements(ApiEmployeeportal.SuggestAnnouncementsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SuggestAnnouncements for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "announcement", "batch",
            "SuggestAnnouncements", "accepted", token);
        var input0 = new ApiEmployeeportal.SuggestPortalHomesRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Filter = new ApiEmployeeportal.SuggestPortalHomesRequest.Types.Filter
        {
            OwnerId = (request.Filter?.OwnerId ?? ""),
        };
        input0.Page = new ApiCommon.PageRequest
        {
            PageToken = (request.Page?.PageToken ?? ""),
        };
        using var rpc0 = _employeeportalPortalHomeQueryService.SuggestPortalHomesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiEmployeeportal.SuggestAnnouncementsResponse
        {
            PageInfo = new ApiCommon.PageInfo
            {
                NextPageToken = (reply0.PageInfo?.NextPageToken ?? ""),
                TotalSize = (reply0.PageInfo?.TotalSize ?? 0L),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.OrderBy ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "announcement",
            "audit", "SuggestAnnouncements", (request.OrderBy ?? ""), token);
        return response;
    }
    /// <summary>
    /// Summarizes an announcement.
    /// </summary>
    public override async Task<ApiEmployeeportal.SummarizeAnnouncementsResponse> SummarizeAnnouncements(ApiEmployeeportal.SummarizeAnnouncementsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SummarizeAnnouncements for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "announcement", request.AnnouncementId,
            "SummarizeAnnouncements", "accepted", token);
        var input0 = new ApiLedger.ExportLedgerAccountsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLedger.ExportLedgerAccountsRequest.Types.Filter
            {
                Labels = { (request.Details?.Filters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.AnnouncementId ?? ""),
            },
            Format = (request.Details?.Format ?? ""),
        };
        using var stream0 = _ledgerLedgerAccountQueryService.ExportLedgerAccounts(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportLedgerAccounts frame {Type}", change0.GetType().Name);
        }
        var input1 = new ApiEmployeeportal.FindSelfServiceRequestsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployeeportal.FindSelfServiceRequestsRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
                Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.AnnouncementId ?? ""),
            },
        };
        var reply1 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _employeeportalSelfServiceRequestQueryService.FindSelfServiceRequestsAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiEmployeeportal.SummarizeAnnouncementsResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "announcement",
            "audit", "SummarizeAnnouncements", (request.Reason ?? ""), token);
        return response;
    }
}
