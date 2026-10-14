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

/// <summary>
/// Operator actions on document templates that support staff run by hand.
/// </summary>
public sealed class DocumentTemplateAdminEndpoint : ApiDocuments.DocumentTemplateAdminService.DocumentTemplateAdminServiceBase
{
    private readonly ApiAuthorization.AccessPolicyQueryService.AccessPolicyQueryServiceClient _authorizationAccessPolicyQueryService;
    private readonly ILogger<DocumentTemplateAdminEndpoint> _logger;
    private readonly IRecordRepository _repository;

    public DocumentTemplateAdminEndpoint(
        ApiAuthorization.AccessPolicyQueryService.AccessPolicyQueryServiceClient authorizationAccessPolicyQueryService,
        ILogger<DocumentTemplateAdminEndpoint> logger,
        IRecordRepository repository)
    {
        _authorizationAccessPolicyQueryService = authorizationAccessPolicyQueryService;
        _logger = logger;
        _repository = repository;
    }
    /// <summary>
    /// Archives a document template.
    /// </summary>
    public override async Task<ApiDocuments.ArchiveDocumentTemplateResponse> ArchiveDocumentTemplate(ApiDocuments.ArchiveDocumentTemplateRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DocumentTemplateId, "document_template_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ArchiveDocumentTemplate for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_template", request.DocumentTemplateId,
            "ArchiveDocumentTemplate", "accepted", token);
        var response = new ApiDocuments.ArchiveDocumentTemplateResponse
        {
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                DocumentTemplateId = (request.DocumentTemplateId ?? ""),
            },
            Outcome = new ApiDocuments.ArchiveDocumentTemplateResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_template",
            "audit", "ArchiveDocumentTemplate", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a document template.
    /// </summary>
    public override async Task<ApiDocuments.InspectDocumentTemplateResponse> InspectDocumentTemplate(ApiDocuments.InspectDocumentTemplateRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DocumentTemplateId, "document_template_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling InspectDocumentTemplate for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_template", request.DocumentTemplateId,
            "InspectDocumentTemplate", "accepted", token);
        var response = new ApiDocuments.InspectDocumentTemplateResponse
        {
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                DocumentTemplateId = (request.DocumentTemplateId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Locks a document template.
    /// </summary>
    public override async Task<ApiDocuments.LockDocumentTemplateResponse> LockDocumentTemplate(ApiDocuments.LockDocumentTemplateRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DocumentTemplateId, "document_template_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling LockDocumentTemplate for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_template", request.DocumentTemplateId,
            "LockDocumentTemplate", "accepted", token);
        var response = new ApiDocuments.LockDocumentTemplateResponse
        {
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                DocumentTemplateId = (request.DocumentTemplateId ?? ""),
            },
            Outcome = new ApiDocuments.LockDocumentTemplateResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_template",
            "audit", "LockDocumentTemplate", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Deletes a document template.
    /// </summary>
    public override async Task<ApiDocuments.PurgeDocumentTemplateResponse> PurgeDocumentTemplate(ApiDocuments.PurgeDocumentTemplateRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DocumentTemplateId, "document_template_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling PurgeDocumentTemplate for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_template", request.DocumentTemplateId,
            "PurgeDocumentTemplate", "accepted", token);
        var response = new ApiDocuments.PurgeDocumentTemplateResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_template",
            "audit", "PurgeDocumentTemplate", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Reassigns a document template.
    /// </summary>
    public override async Task<ApiDocuments.ReassignDocumentTemplateResponse> ReassignDocumentTemplate(ApiDocuments.ReassignDocumentTemplateRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DocumentTemplateId, "document_template_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ReassignDocumentTemplate for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_template", request.DocumentTemplateId,
            "ReassignDocumentTemplate", "accepted", token);
        var input0 = request.ReassignDocumentTemplateToResolveAccessPolicies0();
        using var rpc0 = _authorizationAccessPolicyQueryService.ResolveAccessPoliciesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDocuments.ReassignDocumentTemplateResponse
        {
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                Counterparty = new ApiDocuments.DocumentFolderRef
                {
                    DisplayName = (request.Details?.Assignee?.DisplayName ?? ""),
                },
                DocumentTemplateId = (request.DocumentTemplateId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_template",
            "audit", "ReassignDocumentTemplate", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Repairs a document template.
    /// </summary>
    public override async Task<ApiDocuments.RepairDocumentTemplateResponse> RepairDocumentTemplate(ApiDocuments.RepairDocumentTemplateRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RepairDocumentTemplate for {Tenant}", requestContext.TenantId);
        if (request.DocumentTemplate is { } record)
        {
            var snapshot = DocumentTemplateMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.RepairDocumentTemplateResponse
        {
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                DocumentTemplateId = (request.DocumentTemplateId ?? ""),
                Status = (request.DocumentTemplate?.Status ?? (ApiDocuments.DocumentTemplateStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_template",
            "audit", "RepairDocumentTemplate", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Restores a document template.
    /// </summary>
    public override async Task<ApiDocuments.RestoreDocumentTemplateResponse> RestoreDocumentTemplate(ApiDocuments.RestoreDocumentTemplateRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DocumentTemplateId, "document_template_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RestoreDocumentTemplate for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_template", request.DocumentTemplateId,
            "RestoreDocumentTemplate", "accepted", token);
        var response = new ApiDocuments.RestoreDocumentTemplateResponse
        {
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                DocumentTemplateId = (request.DocumentTemplateId ?? ""),
            },
            Outcome = new ApiDocuments.RestoreDocumentTemplateResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_template",
            "audit", "RestoreDocumentTemplate", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Transfers a document template.
    /// </summary>
    public override async Task<ApiDocuments.TransferDocumentTemplateResponse> TransferDocumentTemplate(ApiDocuments.TransferDocumentTemplateRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DocumentTemplateId, "document_template_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling TransferDocumentTemplate for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_template", request.DocumentTemplateId,
            "TransferDocumentTemplate", "accepted", token);
        var response = new ApiDocuments.TransferDocumentTemplateResponse
        {
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                DocumentTemplateId = (request.DocumentTemplateId ?? ""),
            },
            Outcome = new ApiDocuments.TransferDocumentTemplateResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_template",
            "audit", "TransferDocumentTemplate", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Unlocks a document template.
    /// </summary>
    public override async Task<ApiDocuments.UnlockDocumentTemplateResponse> UnlockDocumentTemplate(ApiDocuments.UnlockDocumentTemplateRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DocumentTemplateId, "document_template_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling UnlockDocumentTemplate for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_template", request.DocumentTemplateId,
            "UnlockDocumentTemplate", "accepted", token);
        var response = new ApiDocuments.UnlockDocumentTemplateResponse
        {
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                DocumentTemplateId = (request.DocumentTemplateId ?? ""),
            },
            Outcome = new ApiDocuments.UnlockDocumentTemplateResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_template",
            "audit", "UnlockDocumentTemplate", (request.Reason ?? ""), token);
        return response;
    }
}
