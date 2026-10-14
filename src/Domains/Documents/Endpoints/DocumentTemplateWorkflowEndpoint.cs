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

namespace Acme.Workforce.Documents;

/// <summary>
/// Runs the long-lived workflow that takes a document template from start to finish.
/// </summary>
public sealed class DocumentTemplateWorkflowEndpoint : ApiDocuments.DocumentTemplateWorkflowService.DocumentTemplateWorkflowServiceBase
{
    private readonly ApiDocuments.DocumentSyncService.DocumentSyncServiceClient _documentsDocumentSyncService;
    private readonly ILogger<DocumentTemplateWorkflowEndpoint> _logger;
    private readonly IRecordRepository _repository;

    public DocumentTemplateWorkflowEndpoint(
        ApiDocuments.DocumentSyncService.DocumentSyncServiceClient documentsDocumentSyncService,
        ILogger<DocumentTemplateWorkflowEndpoint> logger,
        IRecordRepository repository)
    {
        _documentsDocumentSyncService = documentsDocumentSyncService;
        _logger = logger;
        _repository = repository;
    }
    /// <summary>
    /// Advances a document template.
    /// </summary>
    public override async Task<ApiDocuments.AdvanceDocumentTemplateWorkflowResponse> AdvanceDocumentTemplateWorkflow(ApiDocuments.AdvanceDocumentTemplateWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling AdvanceDocumentTemplateWorkflow for {Tenant}", requestContext.TenantId);
        if (request.DocumentTemplate is { } record)
        {
            var snapshot = DocumentTemplateMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.PushDocumentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.PushDocumentRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Document = new ApiDocuments.Document
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.DocumentTemplate?.Audit?.CreatedBy ?? ""),
                },
                Card = new ApiDocuments.DocumentCardSettlement
                {
                    Token = (request.DocumentTemplate ?? new ApiDocuments.DocumentTemplate()).PaymentCase switch
                    {
                        ApiDocuments.DocumentTemplate.SettlementOneofCase.Card => (request.DocumentTemplate?.Card?.Token ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc0 = _documentsDocumentSyncService.PushDocumentAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDocuments.AdvanceDocumentTemplateWorkflowResponse
        {
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                DocumentTemplateId = (request.DocumentTemplateId ?? ""),
                IssuedAt = (request.DocumentTemplate?.IssuedAt ?? new Timestamp()),
                Total = (reply0.Document?.Total ?? new ApiCommon.Money()),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_template",
            "audit", "AdvanceDocumentTemplateWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Cancels a document template.
    /// </summary>
    public override async Task<ApiDocuments.CancelDocumentTemplateWorkflowResponse> CancelDocumentTemplateWorkflow(ApiDocuments.CancelDocumentTemplateWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CancelDocumentTemplateWorkflow for {Tenant}", requestContext.TenantId);
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
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (request.DocumentTemplate?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
                Card = new ApiDocuments.DocumentCardSettlement
                {
                    Network = (request.DocumentTemplate ?? new ApiDocuments.DocumentTemplate()).PaymentCase switch
                    {
                        ApiDocuments.DocumentTemplate.SettlementOneofCase.Card => (request.DocumentTemplate?.Card?.Network ?? ""),
                        _ => ""
                    },
                },
                Counterparty = new ApiDocuments.DocumentFolderRef
                {
                    Path = (request.DocumentTemplate?.Counterparty?.Path ?? ""),
                },
                IssuedAt = (request.DocumentTemplate?.IssuedAt ?? new Timestamp()),
            },
        };
        using var rpc0 = _documentsDocumentSyncService.ResolveDocumentConflictAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDocuments.CancelDocumentTemplateWorkflowResponse
        {
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                DocumentTemplateId = (request.DocumentTemplateId ?? ""),
                Number = (request.DocumentTemplate?.Number ?? ""),
            },
            Outcome = new ApiDocuments.CancelDocumentTemplateWorkflowResponse.Types.Outcome
            {
                Message = (reply0.Outcome?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_template",
            "audit", "CancelDocumentTemplateWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a document template.
    /// </summary>
    public override async Task<ApiDocuments.InspectDocumentTemplateWorkflowResponse> InspectDocumentTemplateWorkflow(ApiDocuments.InspectDocumentTemplateWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DocumentTemplateId, "document_template_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling InspectDocumentTemplateWorkflow for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_template", request.DocumentTemplateId,
            "InspectDocumentTemplateWorkflow", "accepted", token);
        var response = new ApiDocuments.InspectDocumentTemplateWorkflowResponse
        {
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                DocumentTemplateId = (request.DocumentTemplateId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists document templates a page at a time.
    /// </summary>
    public override async Task<ApiDocuments.ListDocumentTemplateWorkflowsResponse> ListDocumentTemplateWorkflows(ApiDocuments.ListDocumentTemplateWorkflowsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ListDocumentTemplateWorkflows for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "document_template", "batch",
            "ListDocumentTemplateWorkflows", "accepted", token);
        var input0 = new ApiDocuments.PushDocumentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.PushDocumentRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Document = new ApiDocuments.Document
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        using var rpc0 = _documentsDocumentSyncService.PushDocumentAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDocuments.ListDocumentTemplateWorkflowsResponse
        {
            DocumentTemplates = { new ApiDocuments.DocumentTemplate
            {
                Notes = (reply0.Document?.Notes ?? ""),
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_template",
            "audit", "ListDocumentTemplateWorkflows", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pauses a document template.
    /// </summary>
    public override async Task<ApiDocuments.PauseDocumentTemplateWorkflowResponse> PauseDocumentTemplateWorkflow(ApiDocuments.PauseDocumentTemplateWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling PauseDocumentTemplateWorkflow for {Tenant}", requestContext.TenantId);
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
                Bank = new ApiDocuments.DocumentBankSettlement
                {
                    Bic = (request.DocumentTemplate ?? new ApiDocuments.DocumentTemplate()).PaymentCase switch
                    {
                        ApiDocuments.DocumentTemplate.SettlementOneofCase.Bank => (request.DocumentTemplate?.Bank?.Bic ?? ""),
                        _ => ""
                    },
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.DocumentTemplate?.BillingAddress?.Region ?? ""),
                },
                Counterparty = new ApiDocuments.DocumentFolderRef
                {
                    Path = (request.DocumentTemplate?.Counterparty?.Path ?? ""),
                },
            },
        };
        using var rpc0 = _documentsDocumentSyncService.ResolveDocumentConflictAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDocuments.PauseDocumentTemplateWorkflowResponse
        {
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Document?.Audit?.CreatedAt ?? new Timestamp()),
                },
                DocumentTemplateId = (request.DocumentTemplateId ?? ""),
                DueAt = (request.DocumentTemplate?.DueAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_template",
            "audit", "PauseDocumentTemplateWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Resumes a document template.
    /// </summary>
    public override async Task<ApiDocuments.ResumeDocumentTemplateWorkflowResponse> ResumeDocumentTemplateWorkflow(ApiDocuments.ResumeDocumentTemplateWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ResumeDocumentTemplateWorkflow for {Tenant}", requestContext.TenantId);
        if (request.DocumentTemplate is { } record)
        {
            var snapshot = DocumentTemplateMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.ResumeDocumentTemplateWorkflowResponse
        {
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                DocumentTemplateId = (request.DocumentTemplateId ?? ""),
                IssuedAt = (request.DocumentTemplate?.IssuedAt ?? new Timestamp()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_template",
            "audit", "ResumeDocumentTemplateWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Retrys a document template.
    /// </summary>
    public override async Task<ApiDocuments.RetryDocumentTemplateWorkflowResponse> RetryDocumentTemplateWorkflow(ApiDocuments.RetryDocumentTemplateWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RetryDocumentTemplateWorkflow for {Tenant}", requestContext.TenantId);
        if (request.DocumentTemplate is { } record)
        {
            var snapshot = DocumentTemplateMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.RetryDocumentTemplateWorkflowResponse
        {
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                DocumentTemplateId = (request.DocumentTemplateId ?? ""),
                Notes = (request.DocumentTemplate?.Notes ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_template",
            "audit", "RetryDocumentTemplateWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Signals a document template.
    /// </summary>
    public override async Task<ApiDocuments.SignalDocumentTemplateWorkflowResponse> SignalDocumentTemplateWorkflow(ApiDocuments.SignalDocumentTemplateWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SignalDocumentTemplateWorkflow for {Tenant}", requestContext.TenantId);
        if (request.DocumentTemplate is { } record)
        {
            var snapshot = DocumentTemplateMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.SignalDocumentTemplateWorkflowResponse
        {
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                BillingAddress = (request.DocumentTemplate?.BillingAddress ?? new ApiCommon.PostalAddress()),
                DocumentTemplateId = (request.DocumentTemplateId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_template",
            "audit", "SignalDocumentTemplateWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Starts a document template.
    /// </summary>
    public override async Task<ApiDocuments.StartDocumentTemplateWorkflowResponse> StartDocumentTemplateWorkflow(ApiDocuments.StartDocumentTemplateWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling StartDocumentTemplateWorkflow for {Tenant}", requestContext.TenantId);
        if (request.DocumentTemplate is { } record)
        {
            var snapshot = DocumentTemplateMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var response = new ApiDocuments.StartDocumentTemplateWorkflowResponse
        {
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                DocumentTemplateId = (request.DocumentTemplateId ?? ""),
                DueAt = (request.DocumentTemplate?.DueAt ?? new Timestamp()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "document_template",
            "audit", "StartDocumentTemplateWorkflow", (request.Reason ?? ""), token);
        return response;
    }
}
