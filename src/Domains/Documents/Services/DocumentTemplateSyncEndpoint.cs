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
/// Keeps document templates in step with the systems of record in other regions.
/// </summary>
public sealed partial class DocumentTemplateSyncEndpoint : ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceBase
{
    private readonly ApiDocuments.DocumentSyncService.DocumentSyncServiceClient _documentsDocumentSyncService;
    private readonly ILogger<DocumentTemplateSyncEndpoint> _logger;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiDocuments.ListDocumentConflictsRequest, CallOptions, AsyncUnaryCall<ApiDocuments.ListDocumentConflictsResponse>> _sendResolveDocumentTemplateConflict0;

    public DocumentTemplateSyncEndpoint(
        ApiDocuments.DocumentSyncService.DocumentSyncServiceClient documentsDocumentSyncService,
        ILogger<DocumentTemplateSyncEndpoint> logger,
        IRecordRepository repository)
    {
        _documentsDocumentSyncService = documentsDocumentSyncService;
        _logger = logger;
        _repository = repository;
        _sendResolveDocumentTemplateConflict0 = _documentsDocumentSyncService.ListDocumentConflictsAsync;
    }
}
