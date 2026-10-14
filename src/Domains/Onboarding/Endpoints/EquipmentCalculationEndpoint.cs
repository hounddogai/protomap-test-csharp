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
using ApiLineage = Acme.Lineage.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Onboarding;

/// <summary>
/// Computes the figures of an equipment, such as totals, taxes, and estimates.
/// </summary>
public sealed class EquipmentCalculationEndpoint : ApiOnboarding.EquipmentCalculationService.EquipmentCalculationServiceBase
{
    private readonly ApiDocuments.DocumentTemplateHistoryService.DocumentTemplateHistoryServiceClient _documentsDocumentTemplateHistoryService;
    private readonly ApiFeatureflags.FeatureFlagValidationService.FeatureFlagValidationServiceClient _featureflagsFeatureFlagValidationService;
    private readonly ApiHelpdesk.SlaPolicyCalculationService.SlaPolicyCalculationServiceClient _helpdeskSlaPolicyCalculationService;
    private readonly ApiIdentity.UserService.UserServiceClient _identityUserService;
    private readonly ApiLineage.LineageNodeQueryService.LineageNodeQueryServiceClient _lineageLineageNodeQueryService;
    private readonly ILogger<EquipmentCalculationEndpoint> _logger;
    private readonly ApiOnboarding.OnboardingPlanCalculationService.OnboardingPlanCalculationServiceClient _onboardingOnboardingPlanCalculationService;
    private readonly ApiOnboarding.OnboardingPlanValidationService.OnboardingPlanValidationServiceClient _onboardingOnboardingPlanValidationService;
    private readonly ApiOnboarding.OnboardingTaskCalculationService.OnboardingTaskCalculationServiceClient _onboardingOnboardingTaskCalculationService;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiWorkplaces.DeskCalculationService.DeskCalculationServiceClient _workplacesDeskCalculationService;

    public EquipmentCalculationEndpoint(
        ApiDocuments.DocumentTemplateHistoryService.DocumentTemplateHistoryServiceClient documentsDocumentTemplateHistoryService,
        ApiFeatureflags.FeatureFlagValidationService.FeatureFlagValidationServiceClient featureflagsFeatureFlagValidationService,
        ApiHelpdesk.SlaPolicyCalculationService.SlaPolicyCalculationServiceClient helpdeskSlaPolicyCalculationService,
        ApiIdentity.UserService.UserServiceClient identityUserService,
        ApiLineage.LineageNodeQueryService.LineageNodeQueryServiceClient lineageLineageNodeQueryService,
        ILogger<EquipmentCalculationEndpoint> logger,
        ApiOnboarding.OnboardingPlanCalculationService.OnboardingPlanCalculationServiceClient onboardingOnboardingPlanCalculationService,
        ApiOnboarding.OnboardingPlanValidationService.OnboardingPlanValidationServiceClient onboardingOnboardingPlanValidationService,
        ApiOnboarding.OnboardingTaskCalculationService.OnboardingTaskCalculationServiceClient onboardingOnboardingTaskCalculationService,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiWorkplaces.DeskCalculationService.DeskCalculationServiceClient workplacesDeskCalculationService)
    {
        _documentsDocumentTemplateHistoryService = documentsDocumentTemplateHistoryService;
        _featureflagsFeatureFlagValidationService = featureflagsFeatureFlagValidationService;
        _helpdeskSlaPolicyCalculationService = helpdeskSlaPolicyCalculationService;
        _identityUserService = identityUserService;
        _lineageLineageNodeQueryService = lineageLineageNodeQueryService;
        _logger = logger;
        _onboardingOnboardingPlanCalculationService = onboardingOnboardingPlanCalculationService;
        _onboardingOnboardingPlanValidationService = onboardingOnboardingPlanValidationService;
        _onboardingOnboardingTaskCalculationService = onboardingOnboardingTaskCalculationService;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _workplacesDeskCalculationService = workplacesDeskCalculationService;
    }
    /// <summary>
    /// Calculates an equipment.
    /// </summary>
    public override async Task<ApiOnboarding.CalculateEquipmentResponse> CalculateEquipment(ApiOnboarding.CalculateEquipmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CalculateEquipment for {Tenant}", requestContext.TenantId);
        if (request.Equipment is { } record)
        {
            var snapshot = EquipmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiIdentity.ListUsersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiIdentity.ListUsersRequest.Types.Filter
            {
                CreatedAfter = (request.Equipment?.Audit?.UpdatedAt ?? new Timestamp()),
                OwnerId = (request.Equipment?.Components?.GetValueOrDefault("primary")?.ComponentId ?? ""),
                Region = (request.Details?.Destination?.Region ?? ""),
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _identityUserService.ListUsersAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiOnboarding.CalculateEquipmentResponse
        {
            Equipment = new ApiOnboarding.Equipment
            {
                EquipmentId = (request.EquipmentId ?? ""),
                Status = (request.Equipment?.Status ?? (ApiOnboarding.EquipmentStatus)0),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "equipment",
            "audit", "CalculateEquipment", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Compares an equipment.
    /// </summary>
    public override async Task<ApiOnboarding.CompareEquipmentOptionsResponse> CompareEquipmentOptions(ApiOnboarding.CompareEquipmentOptionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CompareEquipmentOptions for {Tenant}", requestContext.TenantId);
        if (request.Equipment is { } record)
        {
            var snapshot = EquipmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiOnboarding.CheckOnboardingPlanEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Equipment?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedBy = (request.Equipment?.Audit?.UpdatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Details?.Destination?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Recipient = (request.Equipment ?? new ApiOnboarding.Equipment()).LocatorCase switch
                    {
                        ApiOnboarding.Equipment.LocatorOneofCase.Address => (request.Equipment?.Address?.Recipient ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc0 = _onboardingOnboardingPlanValidationService.CheckOnboardingPlanEligibilityAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiOnboarding.ValidateOnboardingPlanRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Equipment?.Audit?.Etag ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Equipment ?? new ApiOnboarding.Equipment()).LocatorCase switch
                    {
                        ApiOnboarding.Equipment.LocatorOneofCase.Address => (request.Equipment?.Address?.CountryCode ?? ""),
                        _ => ""
                    },
                    Locality = (request.Equipment ?? new ApiOnboarding.Equipment()).LocatorCase switch
                    {
                        ApiOnboarding.Equipment.LocatorOneofCase.Address => (request.Equipment?.Address?.Locality ?? ""),
                        _ => ""
                    },
                },
                Counterparty = new ApiOnboarding.EquipmentRef
                {
                    DisplayName = (request.Equipment?.Owner?.DisplayName ?? ""),
                },
            },
        };
        using var rpc1 = _onboardingOnboardingPlanValidationService.ValidateOnboardingPlanAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiHelpdesk.EstimateSlaPolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Equipment?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
        };
        using var rpc2 = _helpdeskSlaPolicyCalculationService.EstimateSlaPolicyAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        var input3 = new ApiWorkplaces.ExplainDeskCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Desk = new ApiWorkplaces.Desk
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.OnboardingPlan?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Components = { { "primary", new ApiWorkplaces.DeskComponent
                {
                    Description = (request.Equipment?.Components?.GetValueOrDefault("primary")?.Description ?? ""),
                } } },
            },
        };
        using var rpc3 = _workplacesDeskCalculationService.ExplainDeskCalculationAsync(input3, cancellationToken: token);
        var pending3 = rpc3.ResponseAsync;
        await Task.WhenAll(pending2, pending3);
        var reply2 = await pending2;
        var reply3 = await pending3;
        var response = new ApiOnboarding.CompareEquipmentOptionsResponse
        {
            Breakdown = new ApiOnboarding.CompareEquipmentOptionsResponse.Types.Breakdown
            {
                Total = (reply2.Breakdown?.Total ?? new ApiCommon.Money()),
            },
            Equipment = new ApiOnboarding.Equipment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.OnboardingPlan?.Audit?.CreatedAt ?? new Timestamp()),
                    CreatedBy = (reply1.OnboardingPlan?.Audit?.CreatedBy ?? ""),
                },
                Capacity = (request.Equipment?.Capacity ?? 0L),
                EquipmentId = (request.EquipmentId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply3.Desk?.Owner?.Email ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Outcome?.Message ?? ""),
                ReferenceIds = { (reply2.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "equipment",
            "audit", "CompareEquipmentOptions", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Estimates an equipment.
    /// </summary>
    public override async Task<ApiOnboarding.EstimateEquipmentResponse> EstimateEquipment(ApiOnboarding.EstimateEquipmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling EstimateEquipment for {Tenant}", requestContext.TenantId);
        if (request.Equipment is { } record)
        {
            var snapshot = EquipmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiOnboarding.SimulateOnboardingTaskRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.SimulateOnboardingTaskRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Locality = (request.Equipment ?? new ApiOnboarding.Equipment()).LocatorCase switch
                    {
                        ApiOnboarding.Equipment.LocatorOneofCase.Address => (request.Equipment?.Address?.Locality ?? ""),
                        _ => ""
                    },
                    PostalCode = (request.Details?.Destination?.PostalCode ?? ""),
                    Region = (request.Equipment ?? new ApiOnboarding.Equipment()).LocatorCase switch
                    {
                        ApiOnboarding.Equipment.LocatorOneofCase.Address => (request.Equipment?.Address?.Region ?? ""),
                        _ => ""
                    },
                },
            },
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Equipment?.Owner?.DisplayName ?? ""),
                },
            },
        };
        using var rpc0 = _onboardingOnboardingTaskCalculationService.SimulateOnboardingTaskAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiOnboarding.EstimateEquipmentResponse
        {
            Equipment = new ApiOnboarding.Equipment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.OnboardingTask?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedAt = (reply0.OnboardingTask?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                EquipmentId = (request.EquipmentId ?? ""),
                Owner = (request.Equipment?.Owner ?? new ApiIdentity.UserRef()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "equipment",
            "audit", "EstimateEquipment", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains an equipment.
    /// </summary>
    public override async Task<ApiOnboarding.ExplainEquipmentCalculationResponse> ExplainEquipmentCalculation(ApiOnboarding.ExplainEquipmentCalculationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ExplainEquipmentCalculation for {Tenant}", requestContext.TenantId);
        if (request.Equipment is { } record)
        {
            var snapshot = EquipmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiOnboarding.ValidateOnboardingPlanRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.ValidateOnboardingPlanRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                    PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
                },
            },
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Lines = { new ApiOnboarding.OnboardingPlanLine
                {
                    Quantity = (request.Equipment?.Components?.GetValueOrDefault("primary")?.Quantity ?? 0L),
                } },
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _onboardingOnboardingPlanValidationService.ValidateOnboardingPlanAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiOnboarding.ExplainEquipmentCalculationResponse
        {
            Equipment = new ApiOnboarding.Equipment
            {
                EquipmentId = (request.EquipmentId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.OnboardingPlan?.Counterparty?.DisplayName ?? ""),
                },
                Status = (request.Equipment?.Status ?? (ApiOnboarding.EquipmentStatus)0),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "equipment",
            "audit", "ExplainEquipmentCalculation", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an equipment.
    /// </summary>
    public override async Task<ApiOnboarding.GetEquipmentRatesResponse> GetEquipmentRates(ApiOnboarding.GetEquipmentRatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.EquipmentId, "equipment_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetEquipmentRates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "equipment", request.EquipmentId,
            "GetEquipmentRates", "accepted", token);
        var input0 = new ApiOnboarding.QuoteOnboardingPlanRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
                Counterparty = new ApiOnboarding.EquipmentRef
                {
                    EquipmentId = (request.EquipmentId ?? ""),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _onboardingOnboardingPlanCalculationService.QuoteOnboardingPlanAsync);
        var input1 = new ApiOnboarding.ExplainOnboardingPlanDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.OnboardingPlan?.Audit?.Etag ?? ""),
                    UpdatedBy = (reply0.OnboardingPlan?.Audit?.UpdatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (reply0.OnboardingPlan?.BillingAddress?.Recipient ?? ""),
                },
                Card = new ApiOnboarding.OnboardingPlanCardSettlement
                {
                    Token = (reply0.OnboardingPlan ?? new ApiOnboarding.OnboardingPlan()).PaymentCase switch
                    {
                        ApiOnboarding.OnboardingPlan.SettlementOneofCase.Card => (reply0.OnboardingPlan?.Card?.Token ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _onboardingOnboardingPlanValidationService.ExplainOnboardingPlanDecisionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiOnboarding.GetEquipmentRatesResponse
        {
            Equipment = new ApiOnboarding.Equipment
            {
                EquipmentId = (request.EquipmentId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.OnboardingPlan?.Counterparty?.DisplayName ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Quotes an equipment.
    /// </summary>
    public override async Task<ApiOnboarding.QuoteEquipmentResponse> QuoteEquipment(ApiOnboarding.QuoteEquipmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling QuoteEquipment for {Tenant}", requestContext.TenantId);
        if (request.Equipment is { } record)
        {
            var snapshot = EquipmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLineage.CountLineageNodesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLineage.CountLineageNodesRequest.Types.Filter
            {
                CreatedAfter = (request.Equipment?.Audit?.CreatedAt ?? new Timestamp()),
                OwnerId = (request.Equipment?.Components?.GetValueOrDefault("primary")?.ComponentId ?? ""),
                Region = (request.Details?.Destination?.Region ?? ""),
            },
        };
        var named0 = _registry.Get<ApiLineage.LineageNodeQueryService.LineageNodeQueryServiceClient>("acme.lineage.v1.LineageNodeQueryService");
        using var rpc0 = named0.CountLineageNodesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiOnboarding.QuoteEquipmentResponse
        {
            Breakdown = new ApiOnboarding.QuoteEquipmentResponse.Types.Breakdown
            {
                CurrencyCode = (request.Details?.CurrencyCode ?? ""),
            },
            Equipment = new ApiOnboarding.Equipment
            {
                EquipmentId = (request.EquipmentId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "equipment",
            "audit", "QuoteEquipment", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalculates an equipment.
    /// </summary>
    public override async Task<ApiOnboarding.RecalculateEquipmentsResponse> RecalculateEquipments(ApiOnboarding.RecalculateEquipmentsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RecalculateEquipments for {Tenant}", requestContext.TenantId);
        if (request.Equipment is { } record)
        {
            var snapshot = EquipmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.RecordDocumentTemplateVersionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.RecordDocumentTemplateVersionRequest.Types.Details
            {
                Snapshot = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Details?.Destination?.PostalCode ?? ""),
                },
                Lines = { new ApiDocuments.DocumentTemplateLine
                {
                    Quantity = (request.Equipment?.Components?.GetValueOrDefault("primary")?.Quantity ?? 0L),
                } },
            },
        };
        using var rpc0 = _documentsDocumentTemplateHistoryService.RecordDocumentTemplateVersionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiOnboarding.RecalculateEquipmentsResponse
        {
            Equipment = new ApiOnboarding.Equipment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.DocumentTemplate?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Code = (request.Equipment?.Code ?? ""),
                EquipmentId = (request.EquipmentId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "equipment",
            "audit", "RecalculateEquipments", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Simulates an equipment.
    /// </summary>
    public override async Task<ApiOnboarding.SimulateEquipmentResponse> SimulateEquipment(ApiOnboarding.SimulateEquipmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SimulateEquipment for {Tenant}", requestContext.TenantId);
        if (request.Equipment is { } record)
        {
            var snapshot = EquipmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiOnboarding.ValidateOnboardingPlanRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Equipment?.Audit?.Etag ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Equipment ?? new ApiOnboarding.Equipment()).LocatorCase switch
                    {
                        ApiOnboarding.Equipment.LocatorOneofCase.Address => (request.Equipment?.Address?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()),
                        _ => new Google.Protobuf.Collections.RepeatedField<string>()
                    } },
                    Locality = (request.Details?.Destination?.Locality ?? ""),
                    Recipient = (request.Details?.Destination?.Recipient ?? ""),
                },
            },
        };
        using var rpc0 = _onboardingOnboardingPlanValidationService.ValidateOnboardingPlanAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var profile1 = new MappingProfile<ApiOnboarding.SimulateEquipmentRequest, ApiFeatureflags.ExplainFeatureFlagDecisionRequest>(value => new ApiFeatureflags.ExplainFeatureFlagDecisionRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            FeatureFlag = new ApiFeatureflags.FeatureFlag
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (value.Equipment?.Audit?.CreatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (value.Equipment?.Owner?.UserId ?? ""),
                },
            },
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _featureflagsFeatureFlagValidationService.ExplainFeatureFlagDecisionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiOnboarding.SimulateEquipmentResponse
        {
            Breakdown = new ApiOnboarding.SimulateEquipmentResponse.Types.Breakdown
            {
                Total = (reply0.OnboardingPlan?.Total ?? new ApiCommon.Money()),
            },
            Equipment = new ApiOnboarding.Equipment
            {
                Components = { { "primary", new ApiOnboarding.EquipmentComponent
                {
                    Description = (reply1.FeatureFlag?.Description ?? ""),
                } } },
                EquipmentId = (request.EquipmentId ?? ""),
                Status = (request.Equipment?.Status ?? (ApiOnboarding.EquipmentStatus)0),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "equipment",
            "audit", "SimulateEquipment", (request.Reason ?? ""), token);
        return response;
    }
}
