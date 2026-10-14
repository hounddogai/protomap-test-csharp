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
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Itsm;

public sealed partial class IncidentQueryEndpoint
{
    /// <summary>
    /// Lists incidents a page at a time.
    /// </summary>
    public override async Task<ApiItsm.BrowseIncidentsResponse> BrowseIncidents(ApiItsm.BrowseIncidentsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling BrowseIncidents for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "incident", "batch",
            "BrowseIncidents", "accepted", token);
        var response = new ApiItsm.BrowseIncidentsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "incident",
            "audit", "BrowseIncidents", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams every matching incident.
    /// </summary>
    public override async Task ExportIncidents(ApiItsm.ExportIncidentsRequest request, IServerStreamWriter<ApiItsm.ExportIncidentsResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExportIncidents for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "incident", "batch",
            "ExportIncidents", "accepted", token);
        var profile0 = new MappingProfile<ApiItsm.ExportIncidentsRequest, ApiVenues.SuggestVenuesRequest>(value => new ApiVenues.SuggestVenuesRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiVenues.SuggestVenuesRequest.Types.Filter
            {
                OwnerId = (value.Filter?.OwnerId ?? ""),
                Query = (value.Filter?.Query ?? ""),
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _venuesVenueQueryService.SuggestVenuesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.ExplainIncidentDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.ExplainIncidentDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Locale = (request.Context?.Locale ?? ""),
            },
            Incident = new ApiItsm.Incident
            {
                Assignee = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                },
            },
        };
        input1 = PropertyMap.Copy(input1);
        using var rpc1 = _itsmIncidentValidationService.ExplainIncidentDecisionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiItsm.PreviewProblemRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Problem = new ApiItsm.Problem
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.Incident?.Audit?.Etag ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (reply1.Incident?.Customer?.Email ?? ""),
                    PreferredChannel = (reply1.Incident?.Requester?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc2 = _itsmProblemValidationService.PreviewProblemAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        var input3 = new ApiDocuments.FindDocumentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.FindDocumentsRequest.Types.Filter
            {
                CreatedAfter = (request.Filter?.CreatedAfter ?? new Timestamp()),
            },
        };
        using var rpc3 = _documentsDocumentQueryService.FindDocumentsAsync(input3, cancellationToken: token);
        var pending3 = rpc3.ResponseAsync;
        await Task.WhenAll(pending2, pending3);
        var reply2 = await pending2;
        var reply3 = await pending3;
        var response = new ApiItsm.ExportIncidentsResponse
        {
            Incident = new ApiItsm.Incident
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (reply1.Incident?.Assignee?.Email ?? ""),
                },
                Priority = (reply2.Problem?.Priority ?? 0),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply3.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply3.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Format ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "incident",
            "audit", "ExportIncidents", (request.Format ?? ""), token);
        await responses.WriteAsync(response, token);
    }
    /// <summary>
    /// Lists incidents a page at a time.
    /// </summary>
    public override async Task<ApiItsm.FindIncidentsResponse> FindIncidents(ApiItsm.FindIncidentsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling FindIncidents for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "incident", "batch",
            "FindIncidents", "accepted", token);
        var input0 = new ApiItsm.ValidateProblemRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.ValidateProblemRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Locale = (request.Context?.Locale ?? ""),
            },
            Problem = new ApiItsm.Problem
            {
                Assignee = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                },
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _itsmProblemValidationService.ValidateProblemAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiItsm.FindIncidentsResponse
        {
            Incidents = { new ApiItsm.Incident
            {
                Sla = new ApiItsm.IncidentSla
                {
                    DueAt = (reply0.Problem?.Sla?.DueAt ?? new Timestamp()),
                },
            } },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "incident",
            "audit", "FindIncidents", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an incident.
    /// </summary>
    public override async Task<ApiItsm.LookupIncidentResponse> LookupIncident(ApiItsm.LookupIncidentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.IncidentId, "incident_id");
        _logger.LogInformation("Handling LookupIncident for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "incident", request.IncidentId,
            "LookupIncident", "accepted", token);
        var response = new ApiItsm.LookupIncidentResponse
        {
            Incident = new ApiItsm.Incident
            {
                IncidentId = (request.IncidentId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Returns an incident.
    /// </summary>
    public override async Task<ApiItsm.PeekIncidentResponse> PeekIncident(ApiItsm.PeekIncidentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.IncidentId, "incident_id");
        _logger.LogInformation("Handling PeekIncident for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "incident", request.IncidentId,
            "PeekIncident", "accepted", token);
        var response = new ApiItsm.PeekIncidentResponse
        {
            Incident = new ApiItsm.Incident
            {
                IncidentId = (request.IncidentId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Returns several incidents at once.
    /// </summary>
    public override async Task<ApiItsm.ResolveIncidentsResponse> ResolveIncidents(ApiItsm.ResolveIncidentsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ResolveIncidents for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "incident", "batch",
            "ResolveIncidents", "accepted", token);
        var input0 = new ApiItsm.PreviewServiceAssetRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.PreviewServiceAssetRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Locale = (request.Context?.Locale ?? ""),
            },
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiItsm.PreviewServiceAssetRequest>();
        using var rpc0 = _itsmServiceAssetValidationService.PreviewServiceAssetAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.ValidateServiceAssetRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Address = new ApiCommon.PostalAddress
                {
                    Locality = (reply0.ServiceAsset ?? new ApiItsm.ServiceAsset()).LocatorCase switch
                    {
                        ApiItsm.ServiceAsset.LocatorOneofCase.Address => (reply0.ServiceAsset?.Address?.Locality ?? ""),
                        _ => ""
                    },
                    Recipient = (reply0.ServiceAsset ?? new ApiItsm.ServiceAsset()).LocatorCase switch
                    {
                        ApiItsm.ServiceAsset.LocatorOneofCase.Address => (reply0.ServiceAsset?.Address?.Recipient ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _itsmServiceAssetValidationService.ValidateServiceAssetAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiItsm.ResolveIncidentsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Outcome?.Message ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists incidents a page at a time.
    /// </summary>
    public override async Task<ApiItsm.SuggestIncidentsResponse> SuggestIncidents(ApiItsm.SuggestIncidentsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SuggestIncidents for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "incident", "batch",
            "SuggestIncidents", "accepted", token);
        var input0 = new ApiItsm.PreviewProblemRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Details = new ApiItsm.PreviewProblemRequest.Types.Details
        {
            Locale = (request.Context?.Locale ?? ""),
        };
        input0.Problem = new ApiItsm.Problem
        {
            Assignee = new ApiIdentity.UserRef
            {
                UserId = (request.Context?.Actor?.UserId ?? ""),
            },
            Comments = { new ApiItsm.ProblemComment
            {
                CreatedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
            } },
            Requester = new ApiCommon.ContactInfo
            {
                Email = (request.Context?.Actor?.Email ?? ""),
            },
        };
        using var rpc0 = _itsmProblemValidationService.PreviewProblemAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiItsm.SuggestIncidentsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "incident",
            "audit", "SuggestIncidents", (request.Page?.PageToken ?? ""), token);
        return response;
    }
}
