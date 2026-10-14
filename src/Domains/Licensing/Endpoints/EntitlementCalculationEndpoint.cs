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
using ApiPipelines = Acme.Pipelines.V1;

namespace Acme.Workforce.Licensing;

/// <summary>
/// Computes the figures of an entitlement, such as totals, taxes, and estimates.
/// </summary>
public sealed class EntitlementCalculationEndpoint : ApiLicensing.EntitlementCalculationService.EntitlementCalculationServiceBase
{
    private readonly ApiDocuments.DocumentSyncService.DocumentSyncServiceClient _documentsDocumentSyncService;
    private readonly ApiDocuments.DocumentTemplateCalculationService.DocumentTemplateCalculationServiceClient _documentsDocumentTemplateCalculationService;
    private readonly IEntitlementCalculationEndpointSendExplainEntitlementCalculation0Port _entitlementCalculationEndpointSendExplainEntitlementCalculation0Port;
    private readonly ApiFeatureflags.FeatureFlagService.FeatureFlagServiceClient _featureflagsFeatureFlagService;
    private readonly ApiLicensing.SoftwareLicenseCalculationService.SoftwareLicenseCalculationServiceClient _licensingSoftwareLicenseCalculationService;
    private readonly ILogger<EntitlementCalculationEndpoint> _logger;
    private readonly ApiPipelines.PipelineCalculationService.PipelineCalculationServiceClient _pipelinesPipelineCalculationService;
    private readonly IRecordRepository _repository;

    public EntitlementCalculationEndpoint(
        ApiDocuments.DocumentSyncService.DocumentSyncServiceClient documentsDocumentSyncService,
        ApiDocuments.DocumentTemplateCalculationService.DocumentTemplateCalculationServiceClient documentsDocumentTemplateCalculationService,
        IEntitlementCalculationEndpointSendExplainEntitlementCalculation0Port entitlementCalculationEndpointSendExplainEntitlementCalculation0PortAdapter,
        ApiFeatureflags.FeatureFlagService.FeatureFlagServiceClient featureflagsFeatureFlagService,
        ApiLicensing.SoftwareLicenseCalculationService.SoftwareLicenseCalculationServiceClient licensingSoftwareLicenseCalculationService,
        ILogger<EntitlementCalculationEndpoint> logger,
        ApiPipelines.PipelineCalculationService.PipelineCalculationServiceClient pipelinesPipelineCalculationService,
        IRecordRepository repository)
    {
        _documentsDocumentSyncService = documentsDocumentSyncService;
        _documentsDocumentTemplateCalculationService = documentsDocumentTemplateCalculationService;
        _entitlementCalculationEndpointSendExplainEntitlementCalculation0Port = entitlementCalculationEndpointSendExplainEntitlementCalculation0PortAdapter;
        _featureflagsFeatureFlagService = featureflagsFeatureFlagService;
        _licensingSoftwareLicenseCalculationService = licensingSoftwareLicenseCalculationService;
        _logger = logger;
        _pipelinesPipelineCalculationService = pipelinesPipelineCalculationService;
        _repository = repository;
    }
    /// <summary>
    /// Calculates an entitlement.
    /// </summary>
    public override async Task<ApiLicensing.CalculateEntitlementResponse> CalculateEntitlement(ApiLicensing.CalculateEntitlementRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CalculateEntitlement for {Tenant}", requestContext.TenantId);
        if (request.Entitlement is { } record)
        {
            var snapshot = EntitlementMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.CalculateDocumentTemplateRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                Lines = { new ApiDocuments.DocumentTemplateLine
                {
                    Tax = new ApiDocuments.DocumentTemplateLineTax
                    {
                        Amount = (request.Entitlement ?? new ApiLicensing.Entitlement()).ActionCase switch
                        {
                            ApiLicensing.Entitlement.ActionOneofCase.Discount => (request.Entitlement?.Discount?.Amount ?? new ApiCommon.Money()),
                            _ => new ApiCommon.Money()
                        },
                    },
                } },
            },
        };
        using var rpc0 = _documentsDocumentTemplateCalculationService.CalculateDocumentTemplateAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.RecalculateSoftwareLicensesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.DocumentTemplate?.Audit?.CreatedBy ?? ""),
                },
                Bank = new ApiLicensing.SoftwareLicenseBankFunding
                {
                    Bic = (reply0.DocumentTemplate ?? new ApiDocuments.DocumentTemplate()).PaymentCase switch
                    {
                        ApiDocuments.DocumentTemplate.SettlementOneofCase.Bank => (reply0.DocumentTemplate?.Bank?.Bic ?? ""),
                        _ => ""
                    },
                },
                Holder = new ApiLicensing.LicenseAssignmentRef
                {
                    DisplayName = (request.Entitlement?.Owner?.DisplayName ?? ""),
                },
            },
        };
        using var rpc1 = _licensingSoftwareLicenseCalculationService.RecalculateSoftwareLicensesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLicensing.CalculateEntitlementResponse
        {
            Breakdown = new ApiLicensing.CalculateEntitlementResponse.Types.Breakdown
            {
                CurrencyCode = (reply1.Breakdown?.CurrencyCode ?? ""),
                Total = (reply1.Breakdown?.Total ?? new ApiCommon.Money()),
            },
            Entitlement = new ApiLicensing.Entitlement
            {
                EntitlementId = (request.EntitlementId ?? ""),
                Name = (request.Entitlement?.Name ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "entitlement",
            "audit", "CalculateEntitlement", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Compares an entitlement.
    /// </summary>
    public override async Task<ApiLicensing.CompareEntitlementOptionsResponse> CompareEntitlementOptions(ApiLicensing.CompareEntitlementOptionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CompareEntitlementOptions for {Tenant}", requestContext.TenantId);
        if (request.Entitlement is { } record)
        {
            var snapshot = EntitlementMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var profile0 = new MappingProfile<ApiLicensing.CompareEntitlementOptionsRequest, ApiLicensing.SimulateSoftwareLicenseRequest>(value => new ApiLicensing.SimulateSoftwareLicenseRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.SimulateSoftwareLicenseRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (value.Details?.Destination?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (value.Entitlement?.Audit?.UpdatedBy ?? ""),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (value.Entitlement?.Owner?.Email ?? ""),
                },
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _licensingSoftwareLicenseCalculationService.SimulateSoftwareLicenseAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.ReconcileDocumentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ReconcileDocumentsRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Document = new ApiDocuments.Document
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.SoftwareLicense?.Audit?.CreatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Details?.Destination?.CountryCode ?? ""),
                    Locality = (request.Details?.Destination?.Locality ?? ""),
                },
            },
        };
        using var rpc1 = _documentsDocumentSyncService.ReconcileDocumentsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLicensing.CompareEntitlementOptionsResponse
        {
            Breakdown = new ApiLicensing.CompareEntitlementOptionsResponse.Types.Breakdown
            {
                CurrencyCode = (request.Details?.CurrencyCode ?? ""),
            },
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.Document?.Audit?.Etag ?? ""),
                    UpdatedAt = (reply0.SoftwareLicense?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                EntitlementId = (request.EntitlementId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "entitlement",
            "audit", "CompareEntitlementOptions", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Estimates an entitlement.
    /// </summary>
    public override async Task<ApiLicensing.EstimateEntitlementResponse> EstimateEntitlement(ApiLicensing.EstimateEntitlementRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling EstimateEntitlement for {Tenant}", requestContext.TenantId);
        if (request.Entitlement is { } record)
        {
            var snapshot = EntitlementMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = request.EstimateEntitlementToExplainSoftwareLicenseCalculation0();
        using var rpc0 = _licensingSoftwareLicenseCalculationService.ExplainSoftwareLicenseCalculationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiLicensing.EstimateEntitlementResponse
        {
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.SoftwareLicense?.Audit?.CreatedBy ?? ""),
                },
                Description = (request.Entitlement?.Description ?? ""),
                EntitlementId = (request.EntitlementId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "entitlement",
            "audit", "EstimateEntitlement", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains an entitlement.
    /// </summary>
    public override async Task<ApiLicensing.ExplainEntitlementCalculationResponse> ExplainEntitlementCalculation(ApiLicensing.ExplainEntitlementCalculationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ExplainEntitlementCalculation for {Tenant}", requestContext.TenantId);
        if (request.Entitlement is { } record)
        {
            var snapshot = EntitlementMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiFeatureflags.ListFeatureFlagsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiFeatureflags.ListFeatureFlagsRequest.Types.Filter
            {
                CreatedAfter = (request.Entitlement?.Audit?.CreatedAt ?? new Timestamp()),
            },
        };
        var reply0 = await SendExplainEntitlementCalculation0Async(input0, token);
        var input1 = new ApiLicensing.RecalculateSoftwareLicensesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Entitlement?.Audit?.CreatedBy ?? ""),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.Contact?.Phone ?? ""),
                    PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc1 = _licensingSoftwareLicenseCalculationService.RecalculateSoftwareLicensesAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        var input2 = new ApiFeatureflags.BatchGetFeatureFlagsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc2 = _featureflagsFeatureFlagService.BatchGetFeatureFlagsAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        await Task.WhenAll(pending1, pending2);
        var reply1 = await pending1;
        var reply2 = await pending2;
        var input3 = new ApiPipelines.CalculatePipelineRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Pipeline = new ApiPipelines.Pipeline
            {
                Organizer = new ApiIdentity.UserRef
                {
                    UserId = (request.Entitlement?.Owner?.UserId ?? ""),
                },
                Participants = { new ApiPipelines.PipelineParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (reply1.SoftwareLicense?.Contact?.Email ?? ""),
                        Phone = (request.Details?.Contact?.Phone ?? ""),
                    },
                } },
            },
        };
        var json3 = JsonFormatter.Default.Format(input3);
        input3 = JsonParser.Default.Parse<ApiPipelines.CalculatePipelineRequest>(json3);
        using var rpc3 = _pipelinesPipelineCalculationService.CalculatePipelineAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiLicensing.ExplainEntitlementCalculationResponse
        {
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply3.Pipeline?.Audit?.UpdatedBy ?? ""),
                },
                Description = (request.Entitlement?.Description ?? ""),
                EntitlementId = (request.EntitlementId ?? ""),
            },
            Outcome = new ApiLicensing.ExplainEntitlementCalculationResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                ReferenceIds = { (reply2.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "entitlement",
            "audit", "ExplainEntitlementCalculation", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Quotes an entitlement.
    /// </summary>
    public override async Task<ApiLicensing.QuoteEntitlementResponse> QuoteEntitlement(ApiLicensing.QuoteEntitlementRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling QuoteEntitlement for {Tenant}", requestContext.TenantId);
        if (request.Entitlement is { } record)
        {
            var snapshot = EntitlementMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.ListDocumentConflictsRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Filter = new ApiDocuments.ListDocumentConflictsRequest.Types.Filter
        {
            CreatedAfter = (request.Entitlement?.Audit?.CreatedAt ?? new Timestamp()),
            Region = (request.Details?.Destination?.Region ?? ""),
        };
        using var rpc0 = _documentsDocumentSyncService.ListDocumentConflictsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiLicensing.QuoteEntitlementResponse
        {
            Entitlement = new ApiLicensing.Entitlement
            {
                EntitlementId = (request.EntitlementId ?? ""),
                Version = (request.Entitlement?.Version ?? 0L),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "entitlement",
            "audit", "QuoteEntitlement", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Simulates an entitlement.
    /// </summary>
    public override async Task<ApiLicensing.SimulateEntitlementResponse> SimulateEntitlement(ApiLicensing.SimulateEntitlementRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SimulateEntitlement for {Tenant}", requestContext.TenantId);
        if (request.Entitlement is { } record)
        {
            var snapshot = EntitlementMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.ExplainSoftwareLicenseCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Entitlement?.Owner?.Email ?? ""),
                },
                Limits = { new ApiLicensing.SoftwareLicenseLimit
                {
                    Amount = (request.Entitlement ?? new ApiLicensing.Entitlement()).ActionCase switch
                    {
                        ApiLicensing.Entitlement.ActionOneofCase.Discount => (request.Entitlement?.Discount?.Amount ?? new ApiCommon.Money()),
                        _ => new ApiCommon.Money()
                    },
                } },
            },
        };
        using var rpc0 = _licensingSoftwareLicenseCalculationService.ExplainSoftwareLicenseCalculationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.CompareSoftwareLicenseOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.SoftwareLicense?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Entitlement?.Owner?.Email ?? ""),
                },
                Holder = new ApiLicensing.LicenseAssignmentRef
                {
                    LicenseAssignmentId = (reply0.SoftwareLicense?.Holder?.LicenseAssignmentId ?? ""),
                },
            },
        };
        var reply1 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _licensingSoftwareLicenseCalculationService.CompareSoftwareLicenseOptionsAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiLicensing.SimulateEntitlementResponse
        {
            Entitlement = new ApiLicensing.Entitlement
            {
                Effective = (request.Entitlement?.Effective ?? new ApiCommon.TimeWindow()),
                EntitlementId = (request.EntitlementId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "entitlement",
            "audit", "SimulateEntitlement", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiFeatureflags.ListFeatureFlagsResponse> SendExplainEntitlementCalculation0Async(ApiFeatureflags.ListFeatureFlagsRequest request, CancellationToken token)
    {
        return await _entitlementCalculationEndpointSendExplainEntitlementCalculation0Port.SendAsync(request, token);
    }
}
