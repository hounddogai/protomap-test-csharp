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
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiTours = Acme.Tours.V1;

namespace Acme.Workforce.Licensing;

public sealed partial class EntitlementQueryEndpoint
{
    /// <summary>
    /// Lists entitlements a page at a time.
    /// </summary>
    public override async Task<ApiLicensing.BrowseEntitlementsResponse> BrowseEntitlements(ApiLicensing.BrowseEntitlementsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling BrowseEntitlements for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "entitlement", "batch",
            "BrowseEntitlements", "accepted", token);
        var response = new ApiLicensing.BrowseEntitlementsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "entitlement",
            "audit", "BrowseEntitlements", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Counts entitlements by status.
    /// </summary>
    public override async Task<ApiLicensing.CountEntitlementsResponse> CountEntitlements(ApiLicensing.CountEntitlementsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CountEntitlements for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "entitlement", "batch",
            "CountEntitlements", "accepted", token);
        var input0 = new ApiLicensing.CalculateSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.CalculateSoftwareLicenseRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Labels = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        using var rpc0 = _licensingSoftwareLicenseCalculationService.CalculateSoftwareLicenseAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.RecalculateLicenseAssignmentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.SoftwareLicense?.Audit?.UpdatedBy ?? ""),
                },
                Counterparty = new ApiLicensing.EntitlementRef
                {
                    DisplayName = (reply0.SoftwareLicense?.Holder?.DisplayName ?? ""),
                },
            },
        };
        var reply1 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _licensingLicenseAssignmentCalculationService.RecalculateLicenseAssignmentsAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiLicensing.CountEntitlementsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Message = (reply1.Result?.Message ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "entitlement",
            "audit", "CountEntitlements", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams every matching entitlement.
    /// </summary>
    public override async Task ExportEntitlements(ApiLicensing.ExportEntitlementsRequest request, IServerStreamWriter<ApiLicensing.ExportEntitlementsResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExportEntitlements for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "entitlement", "batch",
            "ExportEntitlements", "accepted", token);
        var input0 = new ApiNotifications.BatchGetNotificationsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _notificationsNotificationService.BatchGetNotificationsAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiLicensing.ExportEntitlementsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Format ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "entitlement",
            "audit", "ExportEntitlements", (request.Format ?? ""), token);
        await responses.WriteAsync(response, token);
    }
    /// <summary>
    /// Lists entitlements a page at a time.
    /// </summary>
    public override async Task<ApiLicensing.FindEntitlementsResponse> FindEntitlements(ApiLicensing.FindEntitlementsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling FindEntitlements for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "entitlement", "batch",
            "FindEntitlements", "accepted", token);
        var input0 = new ApiFeatureflags.NormalizeFeatureFlagRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            FeatureFlag = new ApiFeatureflags.FeatureFlag
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _featureflagsFeatureFlagValidationService.NormalizeFeatureFlagAsync);
        var input1 = new ApiLicensing.SimulateEntitlementRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.FeatureFlag?.Audit?.Etag ?? ""),
                    UpdatedBy = (reply0.FeatureFlag?.Audit?.UpdatedBy ?? ""),
                },
                Discount = new ApiLicensing.EntitlementDiscountAction
                {
                    Percent = (reply0.FeatureFlag ?? new ApiFeatureflags.FeatureFlag()).ActionCase switch
                    {
                        ApiFeatureflags.FeatureFlag.ActionOneofCase.Discount => (reply0.FeatureFlag?.Discount?.Percent ?? 0d),
                        _ => 0d
                    },
                },
            },
        };
        using var rpc1 = _licensingEntitlementCalculationService.SimulateEntitlementAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLicensing.FindEntitlementsResponse
        {
            Entitlements = { new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.Entitlement?.Audit?.Etag ?? ""),
                    UpdatedAt = (reply0.FeatureFlag?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Version = (reply0.FeatureFlag?.Version ?? 0L),
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "entitlement",
            "audit", "FindEntitlements", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an entitlement.
    /// </summary>
    public override async Task<ApiLicensing.LookupEntitlementResponse> LookupEntitlement(ApiLicensing.LookupEntitlementRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.EntitlementId, "entitlement_id");
        _logger.LogInformation("Handling LookupEntitlement for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "entitlement", request.EntitlementId,
            "LookupEntitlement", "accepted", token);
        var input0 = new ApiDocuments.ExplainSignatureDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ExplainSignatureDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
        };
        using var rpc0 = _documentsSignatureValidationService.ExplainSignatureDecisionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiLicensing.LookupEntitlementResponse
        {
            Entitlement = new ApiLicensing.Entitlement
            {
                EntitlementId = (request.EntitlementId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Signature?.Subject?.DisplayName ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        return response;
    }
    /// <summary>
    /// Returns an entitlement.
    /// </summary>
    public override async Task<ApiLicensing.PeekEntitlementResponse> PeekEntitlement(ApiLicensing.PeekEntitlementRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.EntitlementId, "entitlement_id");
        _logger.LogInformation("Handling PeekEntitlement for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "entitlement", request.EntitlementId,
            "PeekEntitlement", "accepted", token);
        var input0 = new ApiLicensing.CalculateSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        var descriptor0 = new Method<ApiLicensing.CalculateSoftwareLicenseRequest, ApiLicensing.CalculateSoftwareLicenseResponse>(
            MethodType.Unary, "acme.licensing.v1" + ".SoftwareLicenseCalculationService", "CalculateSoftwareLicense",
            Wire.Marshaller<ApiLicensing.CalculateSoftwareLicenseRequest>(), Wire.Marshaller<ApiLicensing.CalculateSoftwareLicenseResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiLicensing.PeekEntitlementResponse
        {
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.SoftwareLicense?.Audit?.Etag ?? ""),
                },
                EntitlementId = (request.EntitlementId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply0.SoftwareLicense?.Contact?.Email ?? ""),
                },
            },
        };
        return response;
    }
    /// <summary>
    /// Returns several entitlements at once.
    /// </summary>
    public override async Task<ApiLicensing.ResolveEntitlementsResponse> ResolveEntitlements(ApiLicensing.ResolveEntitlementsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ResolveEntitlements for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "entitlement", "batch",
            "ResolveEntitlements", "accepted", token);
        var response = new ApiLicensing.ResolveEntitlementsResponse();
        return response;
    }
    /// <summary>
    /// Lists entitlements a page at a time.
    /// </summary>
    public override async Task<ApiLicensing.SuggestEntitlementsResponse> SuggestEntitlements(ApiLicensing.SuggestEntitlementsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SuggestEntitlements for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "entitlement", "batch",
            "SuggestEntitlements", "accepted", token);
        var input0 = new ApiTours.CountToursRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiTours.CountToursRequest.Types.Filter
            {
                Query = (request.Filter?.Query ?? ""),
            },
            Page = new ApiCommon.PageRequest
            {
                PageSize = (request.Page?.PageSize ?? 0),
            },
        };
        var command0 = new DispatchCommand<ApiTours.CountToursRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _toursTourQueryService.CountToursAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiLicensing.SuggestEntitlementsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "entitlement",
            "audit", "SuggestEntitlements", (request.Page?.PageToken ?? ""), token);
        return response;
    }
}
