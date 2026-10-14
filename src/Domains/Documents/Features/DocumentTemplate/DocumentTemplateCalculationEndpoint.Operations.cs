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
using ApiAuthorization = Acme.Authorization.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;

namespace Acme.Workforce.Documents;

public sealed partial class DocumentTemplateCalculationEndpoint
{
    /// <summary>
    /// Calculates a document template.
    /// </summary>
    public override async Task<ApiDocuments.CalculateDocumentTemplateResponse> CalculateDocumentTemplate(ApiDocuments.CalculateDocumentTemplateRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CalculateDocumentTemplate for {Tenant}", requestContext.TenantId);
        if (request.DocumentTemplate is { } record)
        {
            var snapshot = DocumentTemplateMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.CalculateDocumentTemplateResponse
        {
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                DocumentTemplateId = (request.DocumentTemplateId ?? ""),
                Metadata = { (request.DocumentTemplate?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_template",
            "audit", "CalculateDocumentTemplate", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Compares a document template.
    /// </summary>
    public override async Task<ApiDocuments.CompareDocumentTemplateOptionsResponse> CompareDocumentTemplateOptions(ApiDocuments.CompareDocumentTemplateOptionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CompareDocumentTemplateOptions for {Tenant}", requestContext.TenantId);
        if (request.DocumentTemplate is { } record)
        {
            var snapshot = DocumentTemplateMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.CompareDocumentTemplateOptionsResponse
        {
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                DocumentTemplateId = (request.DocumentTemplateId ?? ""),
                Metadata = { (request.DocumentTemplate?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_template",
            "audit", "CompareDocumentTemplateOptions", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Estimates a document template.
    /// </summary>
    public override async Task<ApiDocuments.EstimateDocumentTemplateResponse> EstimateDocumentTemplate(ApiDocuments.EstimateDocumentTemplateRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EstimateDocumentTemplate for {Tenant}", requestContext.TenantId);
        if (request.DocumentTemplate is { } record)
        {
            var snapshot = DocumentTemplateMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.ListDocumentFolderConflictsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListDocumentFolderConflictsRequest.Types.Filter
            {
                CreatedAfter = (request.DocumentTemplate?.Audit?.UpdatedAt ?? new Timestamp()),
                OwnerId = (request.DocumentTemplate?.Counterparty?.DocumentFolderId ?? ""),
                Region = (request.Details?.Destination?.Region ?? ""),
            },
        };
        var reply0 = await SendEstimateDocumentTemplate0Async(input0, token);
        var response = new ApiDocuments.EstimateDocumentTemplateResponse
        {
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                Counterparty = (request.DocumentTemplate?.Counterparty ?? new ApiDocuments.DocumentFolderRef()),
                DocumentTemplateId = (request.DocumentTemplateId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_template",
            "audit", "EstimateDocumentTemplate", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a document template.
    /// </summary>
    public override async Task<ApiDocuments.ExplainDocumentTemplateCalculationResponse> ExplainDocumentTemplateCalculation(ApiDocuments.ExplainDocumentTemplateCalculationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExplainDocumentTemplateCalculation for {Tenant}", requestContext.TenantId);
        if (request.DocumentTemplate is { } record)
        {
            var snapshot = DocumentTemplateMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.ExplainDocumentTemplateCalculationResponse
        {
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                DocumentTemplateId = (request.DocumentTemplateId ?? ""),
                Notes = (request.DocumentTemplate?.Notes ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_template",
            "audit", "ExplainDocumentTemplateCalculation", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a document template.
    /// </summary>
    public override async Task<ApiDocuments.GetDocumentTemplateRatesResponse> GetDocumentTemplateRates(ApiDocuments.GetDocumentTemplateRatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DocumentTemplateId, "document_template_id");
        _logger.LogInformation("Handling GetDocumentTemplateRates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_template", request.DocumentTemplateId,
            "GetDocumentTemplateRates", "accepted", token);
        var response = new ApiDocuments.GetDocumentTemplateRatesResponse
        {
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                DocumentTemplateId = (request.DocumentTemplateId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Quotes a document template.
    /// </summary>
    public override async Task<ApiDocuments.QuoteDocumentTemplateResponse> QuoteDocumentTemplate(ApiDocuments.QuoteDocumentTemplateRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling QuoteDocumentTemplate for {Tenant}", requestContext.TenantId);
        if (request.DocumentTemplate is { } record)
        {
            var snapshot = DocumentTemplateMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.ResolveDocumentConflictRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ResolveDocumentConflictRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Document = new ApiDocuments.Document
            {
                Counterparty = new ApiDocuments.DocumentFolderRef
                {
                    Path = (request.DocumentTemplate?.Counterparty?.Path ?? ""),
                },
            },
        };
        using var rpc0 = _documentsDocumentSyncService.ResolveDocumentConflictAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDocuments.QuoteDocumentTemplateResponse
        {
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                DocumentTemplateId = (request.DocumentTemplateId ?? ""),
                DueAt = (request.DocumentTemplate?.DueAt ?? new Timestamp()),
                IssuedAt = (reply0.Document?.IssuedAt ?? new Timestamp()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_template",
            "audit", "QuoteDocumentTemplate", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalculates a document template.
    /// </summary>
    public override async Task<ApiDocuments.RecalculateDocumentTemplatesResponse> RecalculateDocumentTemplates(ApiDocuments.RecalculateDocumentTemplatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecalculateDocumentTemplates for {Tenant}", requestContext.TenantId);
        if (request.DocumentTemplate is { } record)
        {
            var snapshot = DocumentTemplateMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiDocuments.ExplainSignatureCalculationResponse();
        foreach (var item in (request.DocumentTemplate?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiDocuments.DocumentTemplateLine>()))
        {
            var input0 = new ApiDocuments.ExplainSignatureCalculationRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Signature = new ApiDocuments.Signature
                {
                    Attributes = { (item.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                    Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                    Subject = new ApiDocuments.DocumentRef
                    {
                        DisplayName = (request.DocumentTemplate?.Counterparty?.DisplayName ?? ""),
                        Number = (request.DocumentTemplate?.Number ?? ""),
                    },
                },
            };
            using var rpc0 = _documentsSignatureCalculationService.ExplainSignatureCalculationAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var response = new ApiDocuments.RecalculateDocumentTemplatesResponse
        {
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                DocumentTemplateId = (request.DocumentTemplateId ?? ""),
                Number = (request.DocumentTemplate?.Number ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_template",
            "audit", "RecalculateDocumentTemplates", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Simulates a document template.
    /// </summary>
    public override async Task<ApiDocuments.SimulateDocumentTemplateResponse> SimulateDocumentTemplate(ApiDocuments.SimulateDocumentTemplateRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SimulateDocumentTemplate for {Tenant}", requestContext.TenantId);
        if (request.DocumentTemplate is { } record)
        {
            var snapshot = DocumentTemplateMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiAuthorization.BrowseAccessPoliciesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiAuthorization.BrowseAccessPoliciesRequest.Types.Filter
            {
                CreatedAfter = (request.DocumentTemplate?.Audit?.CreatedAt ?? new Timestamp()),
                Region = (request.Details?.Destination?.Region ?? ""),
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiAuthorization.BrowseAccessPoliciesRequest>(json0);
        using var rpc0 = _authorizationAccessPolicyQueryService.BrowseAccessPoliciesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDocuments.SimulateDocumentTemplateResponse
        {
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                DocumentTemplateId = (request.DocumentTemplateId ?? ""),
                IssuedAt = (request.DocumentTemplate?.IssuedAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_template",
            "audit", "SimulateDocumentTemplate", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiDocuments.ListDocumentFolderConflictsResponse> SendEstimateDocumentTemplate0Async(ApiDocuments.ListDocumentFolderConflictsRequest request, CancellationToken token)
    {
        return await _documentTemplateCalculationEndpointSendEstimateDocumentTemplate0Port.SendAsync(request, token);
    }
}
