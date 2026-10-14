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
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiRecommendations = Acme.Recommendations.V1;
using ApiSearch = Acme.Search.V1;

namespace Acme.Workforce.Licensing;

/// <summary>
/// Checks license assignments against the rules before anything acts on them.
/// </summary>
public sealed class LicenseAssignmentValidationEndpoint : ApiLicensing.LicenseAssignmentValidationService.LicenseAssignmentValidationServiceBase
{
    private readonly CallInvoker _invoker;
    private readonly ApiLedger.LedgerAccountQueryService.LedgerAccountQueryServiceClient _ledgerLedgerAccountQueryService;
    private readonly ApiLedger.LedgerAccountService.LedgerAccountServiceClient _ledgerLedgerAccountService;
    private readonly ApiLedger.LedgerAccountValidationService.LedgerAccountValidationServiceClient _ledgerLedgerAccountValidationService;
    private readonly ILicenseAssignmentValidationEndpointSendLintLicenseAssignments1Port _licenseAssignmentValidationEndpointSendLintLicenseAssignments1Port;
    private readonly ApiLicensing.EntitlementCalculationService.EntitlementCalculationServiceClient _licensingEntitlementCalculationService;
    private readonly ApiLicensing.LicenseAssignmentCalculationService.LicenseAssignmentCalculationServiceClient _licensingLicenseAssignmentCalculationService;
    private readonly ApiLicensing.SoftwareLicenseValidationService.SoftwareLicenseValidationServiceClient _licensingSoftwareLicenseValidationService;
    private readonly ILogger<LicenseAssignmentValidationEndpoint> _logger;
    private readonly ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient _metricsMetricSeriesQueryService;
    private readonly RetryPolicy _policy;
    private readonly ApiRecommendations.RecommendationSetService.RecommendationSetServiceClient _recommendationsRecommendationSetService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiSearch.SearchIndexValidationService.SearchIndexValidationServiceClient _searchSearchIndexValidationService;
    private readonly IOptions<MeshOptions> _settings;

    public LicenseAssignmentValidationEndpoint(
        CallInvoker invoker,
        ApiLedger.LedgerAccountQueryService.LedgerAccountQueryServiceClient ledgerLedgerAccountQueryService,
        ApiLedger.LedgerAccountService.LedgerAccountServiceClient ledgerLedgerAccountService,
        ApiLedger.LedgerAccountValidationService.LedgerAccountValidationServiceClient ledgerLedgerAccountValidationService,
        ILicenseAssignmentValidationEndpointSendLintLicenseAssignments1Port licenseAssignmentValidationEndpointSendLintLicenseAssignments1PortAdapter,
        ApiLicensing.EntitlementCalculationService.EntitlementCalculationServiceClient licensingEntitlementCalculationService,
        ApiLicensing.LicenseAssignmentCalculationService.LicenseAssignmentCalculationServiceClient licensingLicenseAssignmentCalculationService,
        ApiLicensing.SoftwareLicenseValidationService.SoftwareLicenseValidationServiceClient licensingSoftwareLicenseValidationService,
        ILogger<LicenseAssignmentValidationEndpoint> logger,
        ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient metricsMetricSeriesQueryService,
        RetryPolicy policy,
        ApiRecommendations.RecommendationSetService.RecommendationSetServiceClient recommendationsRecommendationSetService,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiSearch.SearchIndexValidationService.SearchIndexValidationServiceClient searchSearchIndexValidationService,
        IOptions<MeshOptions> settings)
    {
        _invoker = invoker;
        _ledgerLedgerAccountQueryService = ledgerLedgerAccountQueryService;
        _ledgerLedgerAccountService = ledgerLedgerAccountService;
        _ledgerLedgerAccountValidationService = ledgerLedgerAccountValidationService;
        _licenseAssignmentValidationEndpointSendLintLicenseAssignments1Port = licenseAssignmentValidationEndpointSendLintLicenseAssignments1PortAdapter;
        _licensingEntitlementCalculationService = licensingEntitlementCalculationService;
        _licensingLicenseAssignmentCalculationService = licensingLicenseAssignmentCalculationService;
        _licensingSoftwareLicenseValidationService = licensingSoftwareLicenseValidationService;
        _logger = logger;
        _metricsMetricSeriesQueryService = metricsMetricSeriesQueryService;
        _policy = policy;
        _recommendationsRecommendationSetService = recommendationsRecommendationSetService;
        _registry = registry;
        _repository = repository;
        _searchSearchIndexValidationService = searchSearchIndexValidationService;
        _settings = settings;
    }
    /// <summary>
    /// Explains a license assignment.
    /// </summary>
    public override async Task<ApiLicensing.ExplainLicenseAssignmentDecisionResponse> ExplainLicenseAssignmentDecision(ApiLicensing.ExplainLicenseAssignmentDecisionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ExplainLicenseAssignmentDecision for {Tenant}", requestContext.TenantId);
        if (request.LicenseAssignment is { } record)
        {
            var snapshot = LicenseAssignmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.LintSoftwareLicensesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.LicenseAssignment?.Audit?.Etag ?? ""),
                    UpdatedBy = (request.LicenseAssignment?.Audit?.UpdatedBy ?? ""),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
                },
            },
        };
        var command0 = new DispatchCommand<ApiLicensing.LintSoftwareLicensesRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _licensingSoftwareLicenseValidationService.LintSoftwareLicensesAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiLicensing.ExplainLicenseAssignmentDecisionResponse
        {
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.SoftwareLicense?.Audit?.CreatedBy ?? ""),
                },
                IssuedAt = (request.LicenseAssignment?.IssuedAt ?? new Timestamp()),
                LicenseAssignmentId = (request.LicenseAssignmentId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment",
            "audit", "ExplainLicenseAssignmentDecision", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a license assignment.
    /// </summary>
    public override async Task<ApiLicensing.GetLicenseAssignmentRulesResponse> GetLicenseAssignmentRules(ApiLicensing.GetLicenseAssignmentRulesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.LicenseAssignmentId, "license_assignment_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetLicenseAssignmentRules for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment", request.LicenseAssignmentId,
            "GetLicenseAssignmentRules", "accepted", token);
        var profile0 = new MappingProfile<ApiLicensing.GetLicenseAssignmentRulesRequest, ApiLicensing.CompareLicenseAssignmentOptionsRequest>(value => new ApiLicensing.CompareLicenseAssignmentOptionsRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            LicenseAssignmentId = (value.LicenseAssignmentId ?? ""),
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _licensingLicenseAssignmentCalculationService.CompareLicenseAssignmentOptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecommendations.BatchGetRecommendationSetsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        input1 = PropertyMap.Copy(input1);
        using var rpc1 = _recommendationsRecommendationSetService.BatchGetRecommendationSetsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLicensing.GetLicenseAssignmentRulesResponse
        {
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (reply0.LicenseAssignment?.BillingAddress?.Region ?? ""),
                },
                LicenseAssignmentId = (request.LicenseAssignmentId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lints a license assignment.
    /// </summary>
    public override async Task<ApiLicensing.LintLicenseAssignmentsResponse> LintLicenseAssignments(ApiLicensing.LintLicenseAssignmentsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling LintLicenseAssignments for {Tenant}", requestContext.TenantId);
        if (request.LicenseAssignment is { } record)
        {
            var snapshot = LicenseAssignmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiLicensing.ExplainEntitlementCalculationResponse();
        foreach (var item in (request.LicenseAssignment?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiLicensing.LicenseAssignmentLine>()))
        {
            var input0 = new ApiLicensing.ExplainEntitlementCalculationRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiLicensing.ExplainEntitlementCalculationRequest.Types.Details
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Details?.Contact?.Email ?? ""),
                        Name = (request.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
                    },
                },
                Entitlement = new ApiLicensing.Entitlement
                {
                    Description = (item.Description ?? ""),
                    Discount = new ApiLicensing.EntitlementDiscountAction
                    {
                        Amount = (item.Amount ?? new ApiCommon.Money()),
                    },
                },
            };
            using var rpc0 = _licensingEntitlementCalculationService.ExplainEntitlementCalculationAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var input1 = new ApiLedger.NormalizeLedgerAccountRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LedgerAccount = new ApiLedger.LedgerAccount
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.LicenseAssignment?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        var reply1 = await SendLintLicenseAssignments1Async(input1, token);
        var batch2 = (request.LicenseAssignment?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiLicensing.LicenseAssignmentLine>()).Select(async item =>
        {
            var input2 = new ApiLicensing.SimulateEntitlementRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Entitlement = new ApiLicensing.Entitlement
                {
                    Description = (item.Description ?? ""),
                    Discount = new ApiLicensing.EntitlementDiscountAction
                    {
                        Amount = (item.Amount ?? new ApiCommon.Money()),
                    },
                    Owner = new ApiIdentity.UserRef
                    {
                        DisplayName = (request.LicenseAssignment?.Counterparty?.DisplayName ?? ""),
                        Email = (request.Details?.Contact?.Email ?? ""),
                    },
                },
            };
            using var rpc2 = _licensingEntitlementCalculationService.SimulateEntitlementAsync(input2, cancellationToken: token);
            var reply2 = await rpc2.ResponseAsync;
            return reply2;
        }).ToArray();
        var replies2 = await Task.WhenAll(batch2);
        var reply2 = replies2.LastOrDefault() ?? new ApiLicensing.SimulateEntitlementResponse();
        var input3 = new ApiLedger.CountLedgerAccountsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLedger.CountLedgerAccountsRequest.Types.Filter
            {
                Labels = { (reply1.LedgerAccount?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        var packed3 = Any.Pack(input3);
        input3 = packed3.Unpack<ApiLedger.CountLedgerAccountsRequest>();
        using var rpc3 = _ledgerLedgerAccountQueryService.CountLedgerAccountsAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiLicensing.LintLicenseAssignmentsResponse
        {
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                LicenseAssignmentId = (request.LicenseAssignmentId ?? ""),
                Number = (request.LicenseAssignment?.Number ?? ""),
            },
            Outcome = new ApiLicensing.LintLicenseAssignmentsResponse.Types.Outcome
            {
                Message = (reply1.Outcome?.Message ?? ""),
                Status = (reply3.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment",
            "audit", "LintLicenseAssignments", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Normalizes a license assignment.
    /// </summary>
    public override async Task<ApiLicensing.NormalizeLicenseAssignmentResponse> NormalizeLicenseAssignment(ApiLicensing.NormalizeLicenseAssignmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling NormalizeLicenseAssignment for {Tenant}", requestContext.TenantId);
        if (request.LicenseAssignment is { } record)
        {
            var snapshot = LicenseAssignmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiSearch.ExplainSearchIndexDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SearchIndex = new ApiSearch.SearchIndex
            {
                Address = new ApiCommon.PostalAddress
                {
                    Lines = { (request.LicenseAssignment?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Locality = (request.LicenseAssignment?.BillingAddress?.Locality ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.LicenseAssignment?.Audit?.CreatedBy ?? ""),
                    UpdatedBy = (request.LicenseAssignment?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiSearch.ExplainSearchIndexDecisionRequest>(json0);
        using var rpc0 = _searchSearchIndexValidationService.ExplainSearchIndexDecisionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.CompareEntitlementOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.CompareEntitlementOptionsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.SearchIndex ?? new ApiSearch.SearchIndex()).LocatorCase switch
                    {
                        ApiSearch.SearchIndex.LocatorOneofCase.Address => (reply0.SearchIndex?.Address?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()),
                        _ => new Google.Protobuf.Collections.RepeatedField<string>()
                    } },
                },
            },
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.LicenseAssignment?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        using var rpc1 = _licensingEntitlementCalculationService.CompareEntitlementOptionsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLicensing.NormalizeLicenseAssignmentResponse
        {
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.Entitlement?.Audit?.Etag ?? ""),
                },
                Counterparty = (request.LicenseAssignment?.Counterparty ?? new ApiLicensing.EntitlementRef()),
                LicenseAssignmentId = (request.LicenseAssignmentId ?? ""),
            },
            Outcome = new ApiLicensing.NormalizeLicenseAssignmentResponse.Types.Outcome
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment",
            "audit", "NormalizeLicenseAssignment", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Previews a license assignment.
    /// </summary>
    public override async Task<ApiLicensing.PreviewLicenseAssignmentResponse> PreviewLicenseAssignment(ApiLicensing.PreviewLicenseAssignmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling PreviewLicenseAssignment for {Tenant}", requestContext.TenantId);
        if (request.LicenseAssignment is { } record)
        {
            var snapshot = LicenseAssignmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiMetrics.SummarizeMetricSeriesesRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiMetrics.SummarizeMetricSeriesesRequest.Types.Details
            {
                Filters = { (request.LicenseAssignment?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Reason = (request.Reason ?? ""),
        };
        using var rpc0 = _metricsMetricSeriesQueryService.SummarizeMetricSeriesesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.GetLicenseAssignmentRatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LicenseAssignmentId = (request.LicenseAssignment?.LicenseAssignmentId ?? ""),
        };
        var reply1 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _licensingLicenseAssignmentCalculationService.GetLicenseAssignmentRatesAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiLicensing.PreviewLicenseAssignmentResponse
        {
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply1.LicenseAssignment?.BillingAddress?.CountryCode ?? ""),
                    Region = (reply1.LicenseAssignment?.BillingAddress?.Region ?? ""),
                },
                LicenseAssignmentId = (request.LicenseAssignmentId ?? ""),
                Metadata = { (request.LicenseAssignment?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.LicenseAssignment?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment",
            "audit", "PreviewLicenseAssignment", (request.LicenseAssignment?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Scores a license assignment.
    /// </summary>
    public override async Task<ApiLicensing.ScoreLicenseAssignmentResponse> ScoreLicenseAssignment(ApiLicensing.ScoreLicenseAssignmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ScoreLicenseAssignment for {Tenant}", requestContext.TenantId);
        if (request.LicenseAssignment is { } record)
        {
            var snapshot = LicenseAssignmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.CalculateLicenseAssignmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.CalculateLicenseAssignmentRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.Details?.Destination?.Region ?? ""),
                },
            },
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (request.Details?.Destination?.Locality ?? ""),
                },
                Counterparty = new ApiLicensing.EntitlementRef
                {
                    DisplayName = (request.LicenseAssignment?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _licensingLicenseAssignmentCalculationService.CalculateLicenseAssignmentAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiLicensing.ScoreLicenseAssignmentResponse
        {
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                LicenseAssignmentId = (request.LicenseAssignmentId ?? ""),
                Status = (request.LicenseAssignment?.Status ?? (ApiLicensing.LicenseAssignmentStatus)0),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment",
            "audit", "ScoreLicenseAssignment", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Validates a license assignment.
    /// </summary>
    public override async Task<ApiLicensing.ValidateLicenseAssignmentResponse> ValidateLicenseAssignment(ApiLicensing.ValidateLicenseAssignmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ValidateLicenseAssignment for {Tenant}", requestContext.TenantId);
        if (request.LicenseAssignment is { } record)
        {
            var snapshot = LicenseAssignmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.EstimateEntitlementRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.LicenseAssignment?.Audit?.Etag ?? ""),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _licensingEntitlementCalculationService.EstimateEntitlementAsync);
        var input1 = new ApiLicensing.ExplainEntitlementCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.Entitlement?.Audit?.CreatedBy ?? ""),
                    UpdatedBy = (request.LicenseAssignment?.Audit?.UpdatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.LicenseAssignment?.Counterparty?.DisplayName ?? ""),
                },
                Route = new ApiLicensing.EntitlementRouteAction
                {
                    Target = (reply0.Entitlement ?? new ApiLicensing.Entitlement()).ActionCase switch
                    {
                        ApiLicensing.Entitlement.ActionOneofCase.Route => (reply0.Entitlement?.Route?.Target ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _licensingEntitlementCalculationService.ExplainEntitlementCalculationAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiLedger.CreateLedgerAccountRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LedgerAccount = new ApiLedger.LedgerAccount
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.Entitlement?.Audit?.CreatedAt ?? new Timestamp()),
                    CreatedBy = (reply1.Entitlement?.Audit?.CreatedBy ?? ""),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.Contact?.Phone ?? ""),
                },
                Holder = new ApiLedger.PostingBatchRef
                {
                    DisplayName = (reply1.Entitlement?.Owner?.DisplayName ?? ""),
                },
            },
        };
        var named2 = _registry.Get<ApiLedger.LedgerAccountService.LedgerAccountServiceClient>("acme.ledger.v1.LedgerAccountService");
        using var rpc2 = named2.CreateLedgerAccountAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiLicensing.QuoteEntitlementRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.LicenseAssignment?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
            },
        };
        var descriptor3 = new Method<ApiLicensing.QuoteEntitlementRequest, ApiLicensing.QuoteEntitlementResponse>(
            MethodType.Unary, "acme.licensing.v1" + ".EntitlementCalculationService", "QuoteEntitlement",
            Wire.Marshaller<ApiLicensing.QuoteEntitlementRequest>(), Wire.Marshaller<ApiLicensing.QuoteEntitlementResponse>());
        using var rpc3 = _invoker.AsyncUnaryCall(descriptor3, null,
            new CallOptions(cancellationToken: token), input3);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiLicensing.ValidateLicenseAssignmentResponse
        {
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply3.Entitlement?.Audit?.Etag ?? ""),
                    UpdatedAt = (reply1.Entitlement?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Counterparty = new ApiLicensing.EntitlementRef
                {
                    DisplayName = (reply0.Entitlement?.Owner?.DisplayName ?? ""),
                },
                LicenseAssignmentId = (request.LicenseAssignmentId ?? ""),
                Total = (request.LicenseAssignment?.Total ?? new ApiCommon.Money()),
            },
            Outcome = new ApiLicensing.ValidateLicenseAssignmentResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply2.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment",
            "audit", "ValidateLicenseAssignment", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiLedger.NormalizeLedgerAccountResponse> SendLintLicenseAssignments1Async(ApiLedger.NormalizeLedgerAccountRequest request, CancellationToken token)
    {
        return await _licenseAssignmentValidationEndpointSendLintLicenseAssignments1Port.SendAsync(request, token);
    }
}
