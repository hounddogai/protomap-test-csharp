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
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiReturns = Acme.Returns.V1;

namespace Acme.Workforce.Corporateevents;

public sealed partial class VenueReportingEndpoint
{
    /// <summary>
    /// Deletes a venue.
    /// </summary>
    public override async Task<ApiCorporateevents.DeleteVenueReportResponse> DeleteVenueReport(ApiCorporateevents.DeleteVenueReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.VenueId, "venue_id");
        _logger.LogInformation("Handling DeleteVenueReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "venue", request.VenueId,
            "DeleteVenueReport", "accepted", token);
        var input0 = new ApiReturns.SuggestReturnRequestsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiReturns.SuggestReturnRequestsRequest.Types.Filter
            {
                OwnerId = (request.VenueId ?? ""),
            },
        };
        using var rpc0 = _sendDeleteVenueReport0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.UnsubscribeDocumentFolderUpdatesRequest
        {
            Annotations = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.UnsubscribeDocumentFolderUpdatesRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Variables = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Reason = (request.Reason ?? ""),
        };
        using var rpc1 = _documentsDocumentFolderNotificationService.UnsubscribeDocumentFolderUpdatesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDocuments.ListDocumentConflictsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListDocumentConflictsRequest.Types.Filter
            {
                CreatedAfter = (reply1.DocumentFolder?.Audit?.CreatedAt ?? new Timestamp()),
                Region = (reply1.DocumentFolder?.Location?.Region ?? ""),
            },
        };
        var descriptor2 = new Method<ApiDocuments.ListDocumentConflictsRequest, ApiDocuments.ListDocumentConflictsResponse>(
            MethodType.Unary, "acme.documents.v1" + ".DocumentSyncService", "ListDocumentConflicts",
            Wire.Marshaller<ApiDocuments.ListDocumentConflictsRequest>(), Wire.Marshaller<ApiDocuments.ListDocumentConflictsResponse>());
        using var rpc2 = _invoker.AsyncUnaryCall(descriptor2, null,
            new CallOptions(cancellationToken: token), input2);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiDocuments.ValidateDocumentFolderRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                Children = { new ApiDocuments.DocumentFolder
                {
                    Children = { new ApiDocuments.DocumentFolder
                    {
                        Manager = new ApiIdentity.UserRef
                        {
                            DisplayName = (reply1.DocumentFolder?.Manager?.DisplayName ?? ""),
                        },
                    } },
                } },
            },
        };
        var command3 = new DispatchCommand<ApiDocuments.ValidateDocumentFolderRequest>(input3,
            "primary", requestContext.TenantId);
        var routed3 = command3 with { Region = _settings.Value.Region };
        using var rpc3 = _documentsDocumentFolderValidationService.ValidateDocumentFolderAsync(routed3.Payload, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiCorporateevents.DeleteVenueReportResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply2.Result?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Etag ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "venue",
            "audit", "DeleteVenueReport", (request.Etag ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a venue.
    /// </summary>
    public override async Task<ApiCorporateevents.FetchVenueReportResponse> FetchVenueReport(ApiCorporateevents.FetchVenueReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.VenueId, "venue_id");
        _logger.LogInformation("Handling FetchVenueReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "venue", request.VenueId,
            "FetchVenueReport", "accepted", token);
        var response = new ApiCorporateevents.FetchVenueReportResponse
        {
            Venue = new ApiCorporateevents.Venue
            {
                VenueId = (request.VenueId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Generates a venue.
    /// </summary>
    public override async Task<ApiCorporateevents.GenerateVenueReportResponse> GenerateVenueReport(ApiCorporateevents.GenerateVenueReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling GenerateVenueReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "venue", request.VenueId,
            "GenerateVenueReport", "accepted", token);
        var profile0 = new MappingProfile<ApiCorporateevents.GenerateVenueReportRequest, ApiCorporateevents.FindCorporateEventsRequest>(value => new ApiCorporateevents.FindCorporateEventsRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiCorporateevents.FindCorporateEventsRequest.Types.Filter
            {
                Labels = { (value.Details?.Filters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (value.VenueId ?? ""),
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _corporateeventsCorporateEventQueryService.FindCorporateEventsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiCorporateevents.GenerateVenueReportResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "venue",
            "audit", "GenerateVenueReport", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Lists venues a page at a time.
    /// </summary>
    public override async Task<ApiCorporateevents.ListVenueReportsResponse> ListVenueReports(ApiCorporateevents.ListVenueReportsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListVenueReports for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "venue", "batch",
            "ListVenueReports", "accepted", token);
        var response = new ApiCorporateevents.ListVenueReportsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "venue",
            "audit", "ListVenueReports", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Schedules a venue.
    /// </summary>
    public override async Task<ApiCorporateevents.ScheduleVenueReportResponse> ScheduleVenueReport(ApiCorporateevents.ScheduleVenueReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScheduleVenueReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "venue", request.VenueId,
            "ScheduleVenueReport", "accepted", token);
        var input0 = new ApiLicensing.StreamLicenseAssignmentMetricsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Subscriber = new ApiCommon.ContactInfo
            {
                Email = (request.Context?.Actor?.Email ?? ""),
            },
        };
        using var stream0 = _licensingLicenseAssignmentReportingService.StreamLicenseAssignmentMetrics(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received StreamLicenseAssignmentMetrics frame {Type}", change0.GetType().Name);
        }
        var response = new ApiCorporateevents.ScheduleVenueReportResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.VenueId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "venue",
            "audit", "ScheduleVenueReport", (request.VenueId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Shares a venue.
    /// </summary>
    public override async Task<ApiCorporateevents.ShareVenueReportResponse> ShareVenueReport(ApiCorporateevents.ShareVenueReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ShareVenueReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "venue", request.VenueId,
            "ShareVenueReport", "accepted", token);
        var input0 = new ApiReturns.WatchReturnRequestsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Subscriber = new ApiCommon.ContactInfo
            {
                Phone = (request.Details?.Recipient?.Phone ?? ""),
            },
        };
        using var stream0 = _returnsReturnRequestService.WatchReturnRequests(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received WatchReturnRequests frame {Type}", change0.GetType().Name);
        }
        var input1 = new ApiDocuments.ExplainDocumentFolderDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ExplainDocumentFolderDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.Recipient?.Phone ?? ""),
                },
            },
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                Children = { new ApiDocuments.DocumentFolder
                {
                    Manager = new ApiIdentity.UserRef
                    {
                        Email = (request.Details?.Recipient?.Email ?? ""),
                    },
                } },
            },
        };
        var reply1 = await SendShareVenueReport1Async(input1, token);
        var response = new ApiCorporateevents.ShareVenueReportResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Venue = new ApiCorporateevents.Venue
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Recipient?.Email ?? ""),
                },
                VenueId = (request.VenueId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "venue",
            "audit", "ShareVenueReport", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams the changes of venues as they happen.
    /// </summary>
    public override async Task StreamVenueMetrics(ApiCorporateevents.StreamVenueMetricsRequest request, IServerStreamWriter<ApiCorporateevents.StreamVenueMetricsResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling StreamVenueMetrics for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "venue", "batch",
            "StreamVenueMetrics", "accepted", token);
        var input0 = new ApiFeatureflags.BatchGetFeatureFlagsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _featureflagsFeatureFlagService.BatchGetFeatureFlagsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiCorporateevents.ExplainVenueDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Venue = new ApiCorporateevents.Venue
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Subscriber?.Email ?? ""),
                },
            },
        };
        var packed1 = Any.Pack(input1);
        input1 = packed1.Unpack<ApiCorporateevents.ExplainVenueDecisionRequest>();
        using var rpc1 = _corporateeventsVenueValidationService.ExplainVenueDecisionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiCorporateevents.StreamVenueMetricsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            Venue = new ApiCorporateevents.Venue
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.Venue?.Audit?.Etag ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Subscriber?.Email ?? ""),
                },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Subscriber?.PreferredChannel ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "venue",
            "audit", "StreamVenueMetrics", (request.Subscriber?.PreferredChannel ?? ""), token);
        await responses.WriteAsync(response, token);
    }
    private async Task<ApiDocuments.ExplainDocumentFolderDecisionResponse> SendShareVenueReport1Async(ApiDocuments.ExplainDocumentFolderDecisionRequest request, CancellationToken token)
    {
        return await _venueReportingEndpointSendShareVenueReport1Port.SendAsync(request, token);
    }
}
