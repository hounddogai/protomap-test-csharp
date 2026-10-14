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
using ApiLicensing = Acme.Licensing.V1;
using ApiMedia = Acme.Media.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Licensing;

/// <summary>
/// Checks entitlements against the rules before anything acts on them.
/// </summary>
public sealed class EntitlementValidationEndpoint : ApiLicensing.EntitlementValidationService.EntitlementValidationServiceBase
{
    private readonly ApiDocuments.SignatureNotificationService.SignatureNotificationServiceClient _documentsSignatureNotificationService;
    private readonly IEntitlementValidationEndpointSendExplainEntitlementDecision3Port _entitlementValidationEndpointSendExplainEntitlementDecision3Port;
    private readonly ApiIdentity.UserService.UserServiceClient _identityUserService;
    private readonly ApiLicensing.EntitlementCalculationService.EntitlementCalculationServiceClient _licensingEntitlementCalculationService;
    private readonly ApiLicensing.SoftwareLicenseCalculationService.SoftwareLicenseCalculationServiceClient _licensingSoftwareLicenseCalculationService;
    private readonly ApiLicensing.SoftwareLicenseValidationService.SoftwareLicenseValidationServiceClient _licensingSoftwareLicenseValidationService;
    private readonly ILogger<EntitlementValidationEndpoint> _logger;
    private readonly ApiMedia.MediaAssetCalculationService.MediaAssetCalculationServiceClient _mediaMediaAssetCalculationService;
    private readonly ApiNotifications.NotificationValidationService.NotificationValidationServiceClient _notificationsNotificationValidationService;
    private readonly RetryPolicy _policy;
    private readonly IRecordRepository _repository;
    private readonly ApiVenues.VenueService.VenueServiceClient _venuesVenueService;
    private readonly ApiVenues.VenueValidationService.VenueValidationServiceClient _venuesVenueValidationService;

    public EntitlementValidationEndpoint(
        ApiDocuments.SignatureNotificationService.SignatureNotificationServiceClient documentsSignatureNotificationService,
        IEntitlementValidationEndpointSendExplainEntitlementDecision3Port entitlementValidationEndpointSendExplainEntitlementDecision3PortAdapter,
        ApiIdentity.UserService.UserServiceClient identityUserService,
        ApiLicensing.EntitlementCalculationService.EntitlementCalculationServiceClient licensingEntitlementCalculationService,
        ApiLicensing.SoftwareLicenseCalculationService.SoftwareLicenseCalculationServiceClient licensingSoftwareLicenseCalculationService,
        ApiLicensing.SoftwareLicenseValidationService.SoftwareLicenseValidationServiceClient licensingSoftwareLicenseValidationService,
        ILogger<EntitlementValidationEndpoint> logger,
        ApiMedia.MediaAssetCalculationService.MediaAssetCalculationServiceClient mediaMediaAssetCalculationService,
        ApiNotifications.NotificationValidationService.NotificationValidationServiceClient notificationsNotificationValidationService,
        RetryPolicy policy,
        IRecordRepository repository,
        ApiVenues.VenueService.VenueServiceClient venuesVenueService,
        ApiVenues.VenueValidationService.VenueValidationServiceClient venuesVenueValidationService)
    {
        _documentsSignatureNotificationService = documentsSignatureNotificationService;
        _entitlementValidationEndpointSendExplainEntitlementDecision3Port = entitlementValidationEndpointSendExplainEntitlementDecision3PortAdapter;
        _identityUserService = identityUserService;
        _licensingEntitlementCalculationService = licensingEntitlementCalculationService;
        _licensingSoftwareLicenseCalculationService = licensingSoftwareLicenseCalculationService;
        _licensingSoftwareLicenseValidationService = licensingSoftwareLicenseValidationService;
        _logger = logger;
        _mediaMediaAssetCalculationService = mediaMediaAssetCalculationService;
        _notificationsNotificationValidationService = notificationsNotificationValidationService;
        _policy = policy;
        _repository = repository;
        _venuesVenueService = venuesVenueService;
        _venuesVenueValidationService = venuesVenueValidationService;
    }
    /// <summary>
    /// Explains an entitlement.
    /// </summary>
    public override async Task<ApiLicensing.ExplainEntitlementDecisionResponse> ExplainEntitlementDecision(ApiLicensing.ExplainEntitlementDecisionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ExplainEntitlementDecision for {Tenant}", requestContext.TenantId);
        if (request.Entitlement is { } record)
        {
            var snapshot = EntitlementMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.UnsubscribeSignatureUpdatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.UnsubscribeSignatureUpdatesRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                    PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc0 = _documentsSignatureNotificationService.UnsubscribeSignatureUpdatesAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiIdentity.CreateUserRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            User = new ApiIdentity.User
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Entitlement?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Entitlement?.Owner?.DisplayName ?? ""),
                },
            },
        };
        using var rpc1 = _identityUserService.CreateUserAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var input2 = request.ExplainEntitlementDecisionToSimulateSoftwareLicense2(reply1);
        using var rpc2 = _licensingSoftwareLicenseCalculationService.SimulateSoftwareLicenseAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiVenues.CheckVenueEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiVenues.CheckVenueEligibilityRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.Contact?.Phone ?? ""),
                },
            },
            Venue = new ApiVenues.Venue
            {
                Address = new ApiCommon.PostalAddress
                {
                    Region = (reply1.User?.PrimaryAddress?.Region ?? ""),
                },
            },
        };
        var reply3 = await SendExplainEntitlementDecision3Async(input3, token);
        var response = new ApiLicensing.ExplainEntitlementDecisionResponse
        {
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply2.SoftwareLicense?.Audit?.CreatedAt ?? new Timestamp()),
                },
                EntitlementId = (request.EntitlementId ?? ""),
                Name = (request.Entitlement?.Name ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply3.Venue?.Owner?.Email ?? ""),
                    UserId = (reply1.User?.UserId ?? ""),
                },
            },
            Outcome = new ApiLicensing.ExplainEntitlementDecisionResponse.Types.Outcome
            {
                ReferenceId = (reply3.Outcome?.ReferenceId ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply2.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "entitlement",
            "audit", "ExplainEntitlementDecision", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an entitlement.
    /// </summary>
    public override async Task<ApiLicensing.GetEntitlementRulesResponse> GetEntitlementRules(ApiLicensing.GetEntitlementRulesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.EntitlementId, "entitlement_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetEntitlementRules for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "entitlement", request.EntitlementId,
            "GetEntitlementRules", "accepted", token);
        var input0 = new ApiLicensing.ScoreSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                ExternalAccountId = (request.EntitlementId ?? ""),
                Labels = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _licensingSoftwareLicenseValidationService.ScoreSoftwareLicenseAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiLicensing.GetEntitlementRulesResponse
        {
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.SoftwareLicense?.Audit?.Etag ?? ""),
                    UpdatedBy = (reply0.SoftwareLicense?.Audit?.UpdatedBy ?? ""),
                },
                EntitlementId = (request.EntitlementId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lints an entitlement.
    /// </summary>
    public override async Task<ApiLicensing.LintEntitlementsResponse> LintEntitlements(ApiLicensing.LintEntitlementsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling LintEntitlements for {Tenant}", requestContext.TenantId);
        if (request.Entitlement is { } record)
        {
            var snapshot = EntitlementMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiLicensing.EstimateEntitlementResponse();
        foreach (var item in (request.Entitlement?.Conditions ?? new Google.Protobuf.Collections.RepeatedField<ApiLicensing.EntitlementCondition>()))
        {
            var input0 = new ApiLicensing.EstimateEntitlementRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Entitlement = new ApiLicensing.Entitlement
                {
                    Conditions = { new ApiLicensing.EntitlementCondition
                    {
                        Attribute = (item.Attribute ?? ""),
                        Value = (item.Value ?? ""),
                    } },
                },
            };
            using var rpc0 = _licensingEntitlementCalculationService.EstimateEntitlementAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var input1 = new ApiVenues.UpdateVenueRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Venue = new ApiVenues.Venue
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Entitlement?.Audit?.CreatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.Entitlement?.Owner?.UserId ?? ""),
                },
            },
        };
        var json1 = JsonFormatter.Default.Format(input1);
        input1 = JsonParser.Default.Parse<ApiVenues.UpdateVenueRequest>(json1);
        using var rpc1 = _venuesVenueService.UpdateVenueAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var batch2 = (request.Entitlement?.Conditions ?? new Google.Protobuf.Collections.RepeatedField<ApiLicensing.EntitlementCondition>()).Select(async item =>
        {
            var input2 = new ApiLicensing.SimulateEntitlementRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Entitlement = new ApiLicensing.Entitlement
                {
                    Conditions = { new ApiLicensing.EntitlementCondition
                    {
                        Attribute = (item.Attribute ?? ""),
                        Negate = item.Negate,
                        Operator = (item.Operator ?? ""),
                        Value = (item.Value ?? ""),
                    } },
                },
            };
            using var rpc2 = _licensingEntitlementCalculationService.SimulateEntitlementAsync(input2, cancellationToken: token);
            var reply2 = await rpc2.ResponseAsync;
            return reply2;
        }).ToArray();
        var replies2 = await Task.WhenAll(batch2);
        var reply2 = replies2.LastOrDefault() ?? new ApiLicensing.SimulateEntitlementResponse();
        var response = new ApiLicensing.LintEntitlementsResponse
        {
            Entitlement = new ApiLicensing.Entitlement
            {
                Effective = (request.Entitlement?.Effective ?? new ApiCommon.TimeWindow()),
                EntitlementId = (request.EntitlementId ?? ""),
            },
            Outcome = new ApiLicensing.LintEntitlementsResponse.Types.Outcome
            {
                Status = (reply1.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "entitlement",
            "audit", "LintEntitlements", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Normalizes an entitlement.
    /// </summary>
    public override async Task<ApiLicensing.NormalizeEntitlementResponse> NormalizeEntitlement(ApiLicensing.NormalizeEntitlementRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling NormalizeEntitlement for {Tenant}", requestContext.TenantId);
        if (request.Entitlement is { } record)
        {
            var snapshot = EntitlementMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiMedia.ExplainMediaAssetCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiMedia.ExplainMediaAssetCalculationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
            },
            MediaAsset = new ApiMedia.MediaAsset
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Entitlement?.Audit?.CreatedBy ?? ""),
                    Etag = (request.Entitlement?.Audit?.Etag ?? ""),
                },
            },
        };
        using var rpc0 = _mediaMediaAssetCalculationService.ExplainMediaAssetCalculationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.QuoteSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply0.MediaAsset?.Owner?.Email ?? ""),
                },
            },
        };
        var reply1 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _licensingSoftwareLicenseCalculationService.QuoteSoftwareLicenseAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiLicensing.NormalizeEntitlementResponse
        {
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.SoftwareLicense?.Audit?.Etag ?? ""),
                    UpdatedBy = (reply0.MediaAsset?.Audit?.UpdatedBy ?? ""),
                },
                EntitlementId = (request.EntitlementId ?? ""),
                Parameters = { (request.Entitlement?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "entitlement",
            "audit", "NormalizeEntitlement", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Previews an entitlement.
    /// </summary>
    public override async Task<ApiLicensing.PreviewEntitlementResponse> PreviewEntitlement(ApiLicensing.PreviewEntitlementRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling PreviewEntitlement for {Tenant}", requestContext.TenantId);
        if (request.Entitlement is { } record)
        {
            var snapshot = EntitlementMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiNotifications.ScoreNotificationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Notification = new ApiNotifications.Notification
            {
                Assignee = new ApiIdentity.UserRef
                {
                    UserId = (request.Entitlement?.Owner?.UserId ?? ""),
                },
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _notificationsNotificationValidationService.ScoreNotificationAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiLicensing.PreviewEntitlementResponse
        {
            Entitlement = new ApiLicensing.Entitlement
            {
                EntitlementId = (request.EntitlementId ?? ""),
                Status = (request.Entitlement?.Status ?? (ApiLicensing.EntitlementStatus)0),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "entitlement",
            "audit", "PreviewEntitlement", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Validates an entitlement.
    /// </summary>
    public override async Task<ApiLicensing.ValidateEntitlementResponse> ValidateEntitlement(ApiLicensing.ValidateEntitlementRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ValidateEntitlement for {Tenant}", requestContext.TenantId);
        if (request.Entitlement is { } record)
        {
            var snapshot = EntitlementMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.CalculateSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Entitlement?.Audit?.Etag ?? ""),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
                Holder = new ApiLicensing.LicenseAssignmentRef
                {
                    DisplayName = (request.Entitlement?.Owner?.DisplayName ?? ""),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _licensingSoftwareLicenseCalculationService.CalculateSoftwareLicenseAsync);
        var response = new ApiLicensing.ValidateEntitlementResponse
        {
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.SoftwareLicense?.Audit?.CreatedBy ?? ""),
                },
                EntitlementId = (request.EntitlementId ?? ""),
                Version = (request.Entitlement?.Version ?? 0L),
            },
            Outcome = new ApiLicensing.ValidateEntitlementResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "entitlement",
            "audit", "ValidateEntitlement", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiVenues.CheckVenueEligibilityResponse> SendExplainEntitlementDecision3Async(ApiVenues.CheckVenueEligibilityRequest request, CancellationToken token)
    {
        return await _entitlementValidationEndpointSendExplainEntitlementDecision3Port.SendAsync(request, token);
    }
}
