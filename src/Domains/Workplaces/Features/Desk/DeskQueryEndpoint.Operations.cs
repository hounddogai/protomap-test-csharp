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
using ApiMedia = Acme.Media.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiRatelimiting = Acme.Ratelimiting.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

public sealed partial class DeskQueryEndpoint
{
    /// <summary>
    /// Lists desks a page at a time.
    /// </summary>
    public override async Task<ApiWorkplaces.BrowseDesksResponse> BrowseDesks(ApiWorkplaces.BrowseDesksRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling BrowseDesks for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "desk", "batch",
            "BrowseDesks", "accepted", token);
        var input0 = new ApiRatelimiting.SuggestRateLimitsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiRatelimiting.SuggestRateLimitsRequest.Types.Filter
            {
                OwnerId = (request.Filter?.OwnerId ?? ""),
                Region = (request.Filter?.Region ?? ""),
            },
            Page = new ApiCommon.PageRequest
            {
                PageSize = (request.Page?.PageSize ?? 0),
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiRatelimiting.SuggestRateLimitsRequest>();
        using var rpc0 = _ratelimitingRateLimitQueryService.SuggestRateLimitsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiMetrics.ValidateMetricSeriesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            MetricSeries = new ApiMetrics.MetricSeries
            {
                Address = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        var json1 = JsonFormatter.Default.Format(input1);
        input1 = JsonParser.Default.Parse<ApiMetrics.ValidateMetricSeriesRequest>(json1);
        using var rpc1 = _metricsMetricSeriesValidationService.ValidateMetricSeriesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiWorkplaces.BrowseDesksResponse
        {
            Desks = { new ApiWorkplaces.Desk
            {
                Dimensions = new ApiWorkplaces.DeskDimensions
                {
                    Width = (reply1.MetricSeries?.Dimensions?.Width ?? 0d),
                },
            } },
            PageInfo = new ApiCommon.PageInfo
            {
                TotalSize = (reply0.PageInfo?.TotalSize ?? 0L),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "desk",
            "audit", "BrowseDesks", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Counts desks by status.
    /// </summary>
    public override async Task<ApiWorkplaces.CountDesksResponse> CountDesks(ApiWorkplaces.CountDesksRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CountDesks for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "desk", "batch",
            "CountDesks", "accepted", token);
        var input0 = new ApiTravel.SuggestTripsRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Filter = new ApiTravel.SuggestTripsRequest.Types.Filter
        {
            Region = (request.Filter?.Region ?? ""),
        };
        using var rpc0 = _travelTripQueryService.SuggestTripsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiWorkplaces.CountDesksResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "desk",
            "audit", "CountDesks", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams every matching desk.
    /// </summary>
    public override async Task ExportDesks(ApiWorkplaces.ExportDesksRequest request, IServerStreamWriter<ApiWorkplaces.ExportDesksResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExportDesks for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "desk", "batch",
            "ExportDesks", "accepted", token);
        var input0 = new ApiDocuments.SimulateDocumentTemplateRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
                DueAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                Metadata = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        using var rpc0 = _documentsDocumentTemplateCalculationService.SimulateDocumentTemplateAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiWorkplaces.ExportDesksResponse
        {
            Desk = new ApiWorkplaces.Desk
            {
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.DocumentTemplate?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Format ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "desk",
            "audit", "ExportDesks", (request.Format ?? ""), token);
        await responses.WriteAsync(response, token);
    }
    /// <summary>
    /// Returns a desk.
    /// </summary>
    public override async Task<ApiWorkplaces.LookupDeskResponse> LookupDesk(ApiWorkplaces.LookupDeskRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DeskId, "desk_id");
        _logger.LogInformation("Handling LookupDesk for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "desk", request.DeskId,
            "LookupDesk", "accepted", token);
        var input0 = new ApiDocuments.LintDocumentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.LintDocumentsRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Locale = (request.Context?.Locale ?? ""),
            },
            Document = new ApiDocuments.Document
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
                Metadata = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _documentsDocumentValidationService.LintDocumentsAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiWorkplaces.LookupDeskResponse
        {
            Desk = new ApiWorkplaces.Desk
            {
                DeskId = (request.DeskId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Returns a desk.
    /// </summary>
    public override async Task<ApiWorkplaces.PeekDeskResponse> PeekDesk(ApiWorkplaces.PeekDeskRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DeskId, "desk_id");
        _logger.LogInformation("Handling PeekDesk for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "desk", request.DeskId,
            "PeekDesk", "accepted", token);
        var input0 = new ApiWorkplaces.ExplainVisitorPassDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiWorkplaces.ExplainVisitorPassDecisionRequest.Types.Details
            {
                Locale = (request.Context?.Locale ?? ""),
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _workplacesVisitorPassValidationService.ExplainVisitorPassDecisionAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiIdentity.CloneUserRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            User = new ApiIdentity.User
            {
                Passport = new ApiIdentity.UserPassport
                {
                    Number = (reply0.VisitorPass?.Number ?? ""),
                },
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply0.VisitorPass?.BillingAddress?.CountryCode ?? ""),
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.VisitorPass?.Counterparty?.DisplayName ?? ""),
                    Email = (reply0.VisitorPass?.Counterparty?.Email ?? ""),
                },
            },
        };
        var reply1 = await Workflow.SendAsync(input1, token, _identityUserService.CloneUserAsync);
        var response = new ApiWorkplaces.PeekDeskResponse
        {
            Desk = new ApiWorkplaces.Desk
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.VisitorPass?.Audit?.CreatedAt ?? new Timestamp()),
                },
                DeskId = (request.DeskId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (reply1.User?.UserId ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Returns several desks at once.
    /// </summary>
    public override async Task<ApiWorkplaces.ResolveDesksResponse> ResolveDesks(ApiWorkplaces.ResolveDesksRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ResolveDesks for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "desk", "batch",
            "ResolveDesks", "accepted", token);
        var input0 = new ApiMedia.UpdateMediaAssetRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            MediaAsset = new ApiMedia.MediaAsset
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        using var rpc0 = _sendResolveDesks0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiWorkplaces.QuoteBuildingRequest
        {
            Building = new ApiWorkplaces.Building
            {
                Children = { new ApiWorkplaces.Building
                {
                    Children = { new ApiWorkplaces.Building
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            CreatedAt = (reply0.MediaAsset?.Audit?.CreatedAt ?? new Timestamp()),
                            CreatedBy = (reply0.MediaAsset?.Audit?.CreatedBy ?? ""),
                            UpdatedBy = (reply0.MediaAsset?.Audit?.UpdatedBy ?? ""),
                        },
                        Manager = new ApiIdentity.UserRef
                        {
                            DisplayName = (reply0.MediaAsset?.Owner?.DisplayName ?? ""),
                        },
                    } },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var named1 = _registry.Get<ApiWorkplaces.BuildingCalculationService.BuildingCalculationServiceClient>("acme.workplaces.v1.BuildingCalculationService");
        using var rpc1 = named1.QuoteBuildingAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiWorkplaces.ResolveDesksResponse
        {
            Desks = { new ApiWorkplaces.Desk
            {
                Dimensions = new ApiWorkplaces.DeskDimensions
                {
                    Weight = (reply0.MediaAsset?.Dimensions?.Weight ?? 0d),
                },
                Name = (reply1.Building?.Name ?? ""),
            } },
        };
        return response;
    }
    /// <summary>
    /// Lists desks a page at a time.
    /// </summary>
    public override async Task<ApiWorkplaces.SuggestDesksResponse> SuggestDesks(ApiWorkplaces.SuggestDesksRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SuggestDesks for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "desk", "batch",
            "SuggestDesks", "accepted", token);
        var input0 = new ApiWorkplaces.NormalizeBuildingRequest
        {
            Building = new ApiWorkplaces.Building
            {
                Children = { new ApiWorkplaces.Building
                {
                    Children = { new ApiWorkplaces.Building
                    {
                        Location = new ApiCommon.PostalAddress
                        {
                            Region = (request.Filter?.Region ?? ""),
                        },
                        Manager = new ApiIdentity.UserRef
                        {
                            UserId = (request.Context?.Actor?.UserId ?? ""),
                        },
                    } },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var descriptor0 = new Method<ApiWorkplaces.NormalizeBuildingRequest, ApiWorkplaces.NormalizeBuildingResponse>(
            MethodType.Unary, "acme.workplaces.v1" + ".BuildingValidationService", "NormalizeBuilding",
            Wire.Marshaller<ApiWorkplaces.NormalizeBuildingRequest>(), Wire.Marshaller<ApiWorkplaces.NormalizeBuildingResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiWorkplaces.SuggestDesksResponse
        {
            Desks = { new ApiWorkplaces.Desk
            {
                Name = (reply0.Building?.Name ?? ""),
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "desk",
            "audit", "SuggestDesks", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Summarizes a desk.
    /// </summary>
    public override async Task<ApiWorkplaces.SummarizeDesksResponse> SummarizeDesks(ApiWorkplaces.SummarizeDesksRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SummarizeDesks for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "desk", request.DeskId,
            "SummarizeDesks", "accepted", token);
        var input0 = new ApiIdentity.NormalizeUserRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Reason ?? ""),
            User = new ApiIdentity.User
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        var command0 = new DispatchCommand<ApiIdentity.NormalizeUserRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _identityUserValidationService.NormalizeUserAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiWorkplaces.SummarizeDesksResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.Format ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "desk",
            "audit", "SummarizeDesks", (request.Details?.Format ?? ""), token);
        return response;
    }
}
