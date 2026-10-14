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
using ApiCommon = Acme.Common.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiLineage = Acme.Lineage.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiSearch = Acme.Search.V1;

namespace Acme.Workforce.Licensing;

/// <summary>
/// Checks software licenses against the rules before anything acts on them.
/// </summary>
public sealed class SoftwareLicenseValidationEndpoint : ApiLicensing.SoftwareLicenseValidationService.SoftwareLicenseValidationServiceBase
{
    private readonly ApiAnomalies.AnomalyService.AnomalyServiceClient _anomaliesAnomalyService;
    private readonly CallInvoker _invoker;
    private readonly ApiLicensing.EntitlementCalculationService.EntitlementCalculationServiceClient _licensingEntitlementCalculationService;
    private readonly ApiLicensing.SoftwareLicenseCalculationService.SoftwareLicenseCalculationServiceClient _licensingSoftwareLicenseCalculationService;
    private readonly ApiLineage.LineageNodeCalculationService.LineageNodeCalculationServiceClient _lineageLineageNodeCalculationService;
    private readonly ILogger<SoftwareLicenseValidationEndpoint> _logger;
    private readonly ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient _metricsMetricSeriesQueryService;
    private readonly ApiNotifications.NotificationService.NotificationServiceClient _notificationsNotificationService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiSearch.SearchIndexService.SearchIndexServiceClient _searchSearchIndexService;
    private readonly Func<ApiLicensing.GetSoftwareLicenseRatesRequest, CallOptions, AsyncUnaryCall<ApiLicensing.GetSoftwareLicenseRatesResponse>> _sendNormalizeSoftwareLicense1;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ISoftwareLicenseValidationEndpointSendCheckSoftwareLicenseEligibility0Port _softwareLicenseValidationEndpointSendCheckSoftwareLicenseEligibility0Port;
    private readonly ISoftwareLicenseValidationEndpointSendValidateSoftwareLicense2Port _softwareLicenseValidationEndpointSendValidateSoftwareLicense2Port;

    public SoftwareLicenseValidationEndpoint(
        ApiAnomalies.AnomalyService.AnomalyServiceClient anomaliesAnomalyService,
        CallInvoker invoker,
        ApiLicensing.EntitlementCalculationService.EntitlementCalculationServiceClient licensingEntitlementCalculationService,
        ApiLicensing.SoftwareLicenseCalculationService.SoftwareLicenseCalculationServiceClient licensingSoftwareLicenseCalculationService,
        ApiLineage.LineageNodeCalculationService.LineageNodeCalculationServiceClient lineageLineageNodeCalculationService,
        ILogger<SoftwareLicenseValidationEndpoint> logger,
        ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient metricsMetricSeriesQueryService,
        ApiNotifications.NotificationService.NotificationServiceClient notificationsNotificationService,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiSearch.SearchIndexService.SearchIndexServiceClient searchSearchIndexService,
        IOptions<MeshOptions> settings,
        ISoftwareLicenseValidationEndpointSendCheckSoftwareLicenseEligibility0Port softwareLicenseValidationEndpointSendCheckSoftwareLicenseEligibility0PortAdapter,
        ISoftwareLicenseValidationEndpointSendValidateSoftwareLicense2Port softwareLicenseValidationEndpointSendValidateSoftwareLicense2PortAdapter)
    {
        _anomaliesAnomalyService = anomaliesAnomalyService;
        _invoker = invoker;
        _licensingEntitlementCalculationService = licensingEntitlementCalculationService;
        _licensingSoftwareLicenseCalculationService = licensingSoftwareLicenseCalculationService;
        _lineageLineageNodeCalculationService = lineageLineageNodeCalculationService;
        _logger = logger;
        _metricsMetricSeriesQueryService = metricsMetricSeriesQueryService;
        _notificationsNotificationService = notificationsNotificationService;
        _registry = registry;
        _repository = repository;
        _searchSearchIndexService = searchSearchIndexService;
        _settings = settings;
        _softwareLicenseValidationEndpointSendCheckSoftwareLicenseEligibility0Port = softwareLicenseValidationEndpointSendCheckSoftwareLicenseEligibility0PortAdapter;
        _softwareLicenseValidationEndpointSendValidateSoftwareLicense2Port = softwareLicenseValidationEndpointSendValidateSoftwareLicense2PortAdapter;
        _sendNormalizeSoftwareLicense1 = _licensingSoftwareLicenseCalculationService.GetSoftwareLicenseRatesAsync;
    }
    /// <summary>
    /// Checks a software license.
    /// </summary>
    public override async Task<ApiLicensing.CheckSoftwareLicenseEligibilityResponse> CheckSoftwareLicenseEligibility(ApiLicensing.CheckSoftwareLicenseEligibilityRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CheckSoftwareLicenseEligibility for {Tenant}", requestContext.TenantId);
        if (request.SoftwareLicense is { } record)
        {
            var snapshot = SoftwareLicenseMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiMetrics.CountMetricSeriesesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiMetrics.CountMetricSeriesesRequest.Types.Filter
            {
                CreatedAfter = (request.SoftwareLicense?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (request.SoftwareLicense?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        var reply0 = await SendCheckSoftwareLicenseEligibility0Async(input0, token);
        var input1 = new ApiLicensing.QuoteSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.SoftwareLicense?.Audit?.CreatedBy ?? ""),
                },
                Bank = new ApiLicensing.SoftwareLicenseBankFunding
                {
                    AccountHolder = (request.SoftwareLicense ?? new ApiLicensing.SoftwareLicense()).PaymentCase switch
                    {
                        ApiLicensing.SoftwareLicense.FundingOneofCase.Bank => (request.SoftwareLicense?.Bank?.AccountHolder ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _licensingSoftwareLicenseCalculationService.QuoteSoftwareLicenseAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLicensing.CheckSoftwareLicenseEligibilityResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                CurrencyCode = (request.SoftwareLicense?.CurrencyCode ?? ""),
                Holder = (reply1.SoftwareLicense?.Holder ?? new ApiLicensing.LicenseAssignmentRef()),
                SoftwareLicenseId = (request.SoftwareLicenseId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SoftwareLicense?.SoftwareLicenseId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "software_license",
            "audit", "CheckSoftwareLicenseEligibility", (request.SoftwareLicense?.SoftwareLicenseId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a software license.
    /// </summary>
    public override async Task<ApiLicensing.ExplainSoftwareLicenseDecisionResponse> ExplainSoftwareLicenseDecision(ApiLicensing.ExplainSoftwareLicenseDecisionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ExplainSoftwareLicenseDecision for {Tenant}", requestContext.TenantId);
        if (request.SoftwareLicense is { } record)
        {
            var snapshot = SoftwareLicenseMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiMetrics.SuggestMetricSeriesesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiMetrics.SuggestMetricSeriesesRequest.Types.Filter
            {
                CreatedAfter = (request.SoftwareLicense?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (request.SoftwareLicense?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiMetrics.SuggestMetricSeriesesRequest>();
        using var rpc0 = _metricsMetricSeriesQueryService.SuggestMetricSeriesesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiAnomalies.SearchAnomaliesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiAnomalies.SearchAnomaliesRequest.Types.Filter
            {
                CreatedAfter = (request.SoftwareLicense?.OpenedAt ?? new Timestamp()),
                Labels = { (request.SoftwareLicense?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        var json1 = JsonFormatter.Default.Format(input1);
        input1 = JsonParser.Default.Parse<ApiAnomalies.SearchAnomaliesRequest>(json1);
        using var rpc1 = _anomaliesAnomalyService.SearchAnomaliesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLicensing.ExplainSoftwareLicenseDecisionResponse
        {
            Outcome = new ApiLicensing.ExplainSoftwareLicenseDecisionResponse.Types.Outcome
            {
                Message = (reply1.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                SoftwareLicenseId = (request.SoftwareLicenseId ?? ""),
                Status = (request.SoftwareLicense?.Status ?? (ApiLicensing.SoftwareLicenseStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SoftwareLicense?.SoftwareLicenseId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "software_license",
            "audit", "ExplainSoftwareLicenseDecision", (request.SoftwareLicense?.SoftwareLicenseId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a software license.
    /// </summary>
    public override async Task<ApiLicensing.GetSoftwareLicenseRulesResponse> GetSoftwareLicenseRules(ApiLicensing.GetSoftwareLicenseRulesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.SoftwareLicenseId, "software_license_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetSoftwareLicenseRules for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "software_license", request.SoftwareLicenseId,
            "GetSoftwareLicenseRules", "accepted", token);
        var response = new ApiLicensing.GetSoftwareLicenseRulesResponse
        {
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                SoftwareLicenseId = (request.SoftwareLicenseId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lints a software license.
    /// </summary>
    public override async Task<ApiLicensing.LintSoftwareLicensesResponse> LintSoftwareLicenses(ApiLicensing.LintSoftwareLicensesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling LintSoftwareLicenses for {Tenant}", requestContext.TenantId);
        if (request.SoftwareLicense is { } record)
        {
            var snapshot = SoftwareLicenseMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.CalculateSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Card = new ApiLicensing.SoftwareLicenseCardFunding
                {
                    Network = (request.SoftwareLicense ?? new ApiLicensing.SoftwareLicense()).PaymentCase switch
                    {
                        ApiLicensing.SoftwareLicense.FundingOneofCase.Card => (request.SoftwareLicense?.Card?.Network ?? ""),
                        _ => ""
                    },
                },
                Contact = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc0 = _licensingSoftwareLicenseCalculationService.CalculateSoftwareLicenseAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.RecalculateSoftwareLicensesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.SoftwareLicense?.Audit?.Etag ?? ""),
                },
                Bank = new ApiLicensing.SoftwareLicenseBankFunding
                {
                    Iban = (reply0.SoftwareLicense ?? new ApiLicensing.SoftwareLicense()).PaymentCase switch
                    {
                        ApiLicensing.SoftwareLicense.FundingOneofCase.Bank => (reply0.SoftwareLicense?.Bank?.Iban ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _licensingSoftwareLicenseCalculationService.RecalculateSoftwareLicensesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiSearch.BatchGetSearchIndexesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply2 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _searchSearchIndexService.BatchGetSearchIndexesAsync(input2, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input3 = new ApiLicensing.SimulateSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.SoftwareLicense?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Bank = new ApiLicensing.SoftwareLicenseBankFunding
                {
                    Iban = (reply1.SoftwareLicense ?? new ApiLicensing.SoftwareLicense()).PaymentCase switch
                    {
                        ApiLicensing.SoftwareLicense.FundingOneofCase.Bank => (reply1.SoftwareLicense?.Bank?.Iban ?? ""),
                        _ => ""
                    },
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (reply1.SoftwareLicense?.Contact?.Name ?? new ApiCommon.PersonName()),
                },
                Holder = new ApiLicensing.LicenseAssignmentRef
                {
                    LicenseAssignmentId = (reply0.SoftwareLicense?.Holder?.LicenseAssignmentId ?? ""),
                },
            },
        };
        using var rpc3 = _licensingSoftwareLicenseCalculationService.SimulateSoftwareLicenseAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiLicensing.LintSoftwareLicensesResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
                Message = (reply1.Result?.Message ?? ""),
            },
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Available = (request.SoftwareLicense?.Available ?? new ApiCommon.Money()),
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (reply0.SoftwareLicense?.Contact?.Name ?? new ApiCommon.PersonName()),
                },
                SoftwareLicenseId = (request.SoftwareLicenseId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SoftwareLicense?.SoftwareLicenseId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "software_license",
            "audit", "LintSoftwareLicenses", (request.SoftwareLicense?.SoftwareLicenseId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Normalizes a software license.
    /// </summary>
    public override async Task<ApiLicensing.NormalizeSoftwareLicenseResponse> NormalizeSoftwareLicense(ApiLicensing.NormalizeSoftwareLicenseRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling NormalizeSoftwareLicense for {Tenant}", requestContext.TenantId);
        if (request.SoftwareLicense is { } record)
        {
            var snapshot = SoftwareLicenseMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.RecalculateSoftwareLicensesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.SoftwareLicense?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.SoftwareLicense?.Contact?.Phone ?? ""),
                    PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _licensingSoftwareLicenseCalculationService.RecalculateSoftwareLicensesAsync);
        var input1 = new ApiLicensing.GetSoftwareLicenseRatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicenseId = (request.SoftwareLicense?.SoftwareLicenseId ?? ""),
        };
        using var rpc1 = _sendNormalizeSoftwareLicense1(input1, new CallOptions(cancellationToken: token));
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiNotifications.CreateNotificationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Notification = new ApiNotifications.Notification
            {
                Requester = new ApiCommon.ContactInfo
                {
                    Phone = (reply0.SoftwareLicense?.Contact?.Phone ?? ""),
                    PreferredChannel = (reply1.SoftwareLicense?.Contact?.PreferredChannel ?? ""),
                },
            },
        };
        var named2 = _registry.Get<ApiNotifications.NotificationService.NotificationServiceClient>("acme.notifications.v1.NotificationService");
        using var rpc2 = named2.CreateNotificationAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiLicensing.NormalizeSoftwareLicenseResponse
        {
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply2.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply2.Notification?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedBy = (reply1.SoftwareLicense?.Audit?.UpdatedBy ?? ""),
                },
                Holder = (request.SoftwareLicense?.Holder ?? new ApiLicensing.LicenseAssignmentRef()),
                SoftwareLicenseId = (reply0.SoftwareLicense?.SoftwareLicenseId ?? ""),
                Status = (request.SoftwareLicense?.Status ?? (ApiLicensing.SoftwareLicenseStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SoftwareLicenseId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "software_license",
            "audit", "NormalizeSoftwareLicense", (request.SoftwareLicenseId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Previews a software license.
    /// </summary>
    public override async Task<ApiLicensing.PreviewSoftwareLicenseResponse> PreviewSoftwareLicense(ApiLicensing.PreviewSoftwareLicenseRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling PreviewSoftwareLicense for {Tenant}", requestContext.TenantId);
        if (request.SoftwareLicense is { } record)
        {
            var snapshot = SoftwareLicenseMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.SimulateEntitlementRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.SoftwareLicense?.Audit?.CreatedAt ?? new Timestamp()),
                    CreatedBy = (request.SoftwareLicense?.Audit?.CreatedBy ?? ""),
                    UpdatedBy = (request.SoftwareLicense?.Audit?.UpdatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.SoftwareLicense?.Holder?.DisplayName ?? ""),
                },
            },
        };
        var descriptor0 = new Method<ApiLicensing.SimulateEntitlementRequest, ApiLicensing.SimulateEntitlementResponse>(
            MethodType.Unary, "acme.licensing.v1" + ".EntitlementCalculationService", "SimulateEntitlement",
            Wire.Marshaller<ApiLicensing.SimulateEntitlementRequest>(), Wire.Marshaller<ApiLicensing.SimulateEntitlementResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiLicensing.PreviewSoftwareLicenseResponse
        {
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Balance = (request.SoftwareLicense?.Balance ?? new ApiCommon.Money()),
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply0.Entitlement?.Owner?.Email ?? ""),
                },
                CurrencyCode = (reply0.Breakdown?.CurrencyCode ?? ""),
                SoftwareLicenseId = (request.SoftwareLicenseId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SoftwareLicense?.SoftwareLicenseId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "software_license",
            "audit", "PreviewSoftwareLicense", (request.SoftwareLicense?.SoftwareLicenseId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Scores a software license.
    /// </summary>
    public override async Task<ApiLicensing.ScoreSoftwareLicenseResponse> ScoreSoftwareLicense(ApiLicensing.ScoreSoftwareLicenseRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ScoreSoftwareLicense for {Tenant}", requestContext.TenantId);
        if (request.SoftwareLicense is { } record)
        {
            var snapshot = SoftwareLicenseMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLineage.CalculateLineageNodeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LineageNode = new ApiLineage.LineageNode
            {
                Children = { new ApiLineage.LineageNode
                {
                    Children = { new ApiLineage.LineageNode
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            Etag = (request.SoftwareLicense?.Audit?.Etag ?? ""),
                        },
                        Manager = new ApiIdentity.UserRef
                        {
                            DisplayName = (request.SoftwareLicense?.Holder?.DisplayName ?? ""),
                        },
                    } },
                } },
            },
        };
        var command0 = new DispatchCommand<ApiLineage.CalculateLineageNodeRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _lineageLineageNodeCalculationService.CalculateLineageNodeAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var reply1 = new ApiLicensing.EstimateEntitlementResponse();
        foreach (var item in (request.SoftwareLicense?.Limits ?? new Google.Protobuf.Collections.RepeatedField<ApiLicensing.SoftwareLicenseLimit>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input1 = new ApiLicensing.EstimateEntitlementRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiLicensing.EstimateEntitlementRequest.Types.Details
                {
                    Destination = new ApiCommon.PostalAddress
                    {
                        Lines = { (request.Details?.Destination?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    },
                },
                Entitlement = new ApiLicensing.Entitlement
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedAt = (reply0.LineageNode?.Audit?.CreatedAt ?? new Timestamp()),
                    },
                    Discount = new ApiLicensing.EntitlementDiscountAction
                    {
                        Amount = (item.Amount ?? new ApiCommon.Money()),
                    },
                    Name = (item.Name ?? ""),
                },
            };
            using var rpc1 = _licensingEntitlementCalculationService.EstimateEntitlementAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var response = new ApiLicensing.ScoreSoftwareLicenseResponse
        {
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.LineageNode?.Audit?.CreatedBy ?? ""),
                },
                Holder = (request.SoftwareLicense?.Holder ?? new ApiLicensing.LicenseAssignmentRef()),
                SoftwareLicenseId = (request.SoftwareLicenseId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SoftwareLicense?.SoftwareLicenseId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "software_license",
            "audit", "ScoreSoftwareLicense", (request.SoftwareLicense?.SoftwareLicenseId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Validates a software license.
    /// </summary>
    public override async Task<ApiLicensing.ValidateSoftwareLicenseResponse> ValidateSoftwareLicense(ApiLicensing.ValidateSoftwareLicenseRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ValidateSoftwareLicense for {Tenant}", requestContext.TenantId);
        if (request.SoftwareLicense is { } record)
        {
            var snapshot = SoftwareLicenseMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var batch0 = (request.SoftwareLicense?.Limits ?? new Google.Protobuf.Collections.RepeatedField<ApiLicensing.SoftwareLicenseLimit>()).Select(async item =>
        {
            var input0 = new ApiLicensing.ExplainSoftwareLicenseCalculationRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                SoftwareLicense = new ApiLicensing.SoftwareLicense
                {
                    Limits = { new ApiLicensing.SoftwareLicenseLimit
                    {
                        Amount = (item.Amount ?? new ApiCommon.Money()),
                        Name = (item.Name ?? ""),
                        Period = (item.Period ?? new Duration()),
                    } },
                },
            };
            using var rpc0 = _licensingSoftwareLicenseCalculationService.ExplainSoftwareLicenseCalculationAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            return reply0;
        }).ToArray();
        var replies0 = await Task.WhenAll(batch0);
        var reply0 = replies0.LastOrDefault() ?? new ApiLicensing.ExplainSoftwareLicenseCalculationResponse();
        var input1 = new ApiLicensing.SimulateSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Card = new ApiLicensing.SoftwareLicenseCardFunding
                {
                    Last4 = (reply0.SoftwareLicense ?? new ApiLicensing.SoftwareLicense()).PaymentCase switch
                    {
                        ApiLicensing.SoftwareLicense.FundingOneofCase.Card => (reply0.SoftwareLicense?.Card?.Last4 ?? ""),
                        _ => ""
                    },
                },
                Holder = new ApiLicensing.LicenseAssignmentRef
                {
                    LicenseAssignmentId = (request.SoftwareLicense?.Holder?.LicenseAssignmentId ?? ""),
                },
            },
        };
        using var rpc1 = _licensingSoftwareLicenseCalculationService.SimulateSoftwareLicenseAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiLicensing.ExplainEntitlementCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.ExplainEntitlementCalculationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (reply1.SoftwareLicense?.Contact?.Phone ?? ""),
                    PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
                },
            },
            Entitlement = new ApiLicensing.Entitlement
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply0.SoftwareLicense?.Contact?.Email ?? ""),
                },
            },
        };
        var reply2 = await SendValidateSoftwareLicense2Async(input2, token);
        var response = new ApiLicensing.ValidateSoftwareLicenseResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply2.Outcome?.Status ?? ""),
            },
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Available = (request.SoftwareLicense?.Available ?? new ApiCommon.Money()),
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply2.Entitlement?.Owner?.Email ?? ""),
                },
                Holder = new ApiLicensing.LicenseAssignmentRef
                {
                    DisplayName = (reply1.SoftwareLicense?.Holder?.DisplayName ?? ""),
                },
                Labels = { (reply1.SoftwareLicense?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                SoftwareLicenseId = (request.SoftwareLicenseId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SoftwareLicense?.SoftwareLicenseId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "software_license",
            "audit", "ValidateSoftwareLicense", (request.SoftwareLicense?.SoftwareLicenseId ?? ""), token);
        return response;
    }
    private async Task<ApiMetrics.CountMetricSeriesesResponse> SendCheckSoftwareLicenseEligibility0Async(ApiMetrics.CountMetricSeriesesRequest request, CancellationToken token)
    {
        return await _softwareLicenseValidationEndpointSendCheckSoftwareLicenseEligibility0Port.SendAsync(request, token);
    }
    private async Task<ApiLicensing.ExplainEntitlementCalculationResponse> SendValidateSoftwareLicense2Async(ApiLicensing.ExplainEntitlementCalculationRequest request, CancellationToken token)
    {
        return await _softwareLicenseValidationEndpointSendValidateSoftwareLicense2Port.SendAsync(request, token);
    }
}
