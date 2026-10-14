using Acme.Workforce.Platform;
using Google.Protobuf;
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
/// Keeps document folders in step with the systems of record in other regions.
/// </summary>
public sealed partial class DocumentFolderSyncEndpoint : ApiDocuments.DocumentFolderSyncService.DocumentFolderSyncServiceBase
{
    private readonly ApiDocuments.DocumentSyncService.DocumentSyncServiceClient _documentsDocumentSyncService;
    private readonly ILogger<DocumentFolderSyncEndpoint> _logger;
    private readonly IRecordRepository _repository;

    public DocumentFolderSyncEndpoint(
        ApiDocuments.DocumentSyncService.DocumentSyncServiceClient documentsDocumentSyncService,
        ILogger<DocumentFolderSyncEndpoint> logger,
        IRecordRepository repository)
    {
        _documentsDocumentSyncService = documentsDocumentSyncService;
        _logger = logger;
        _repository = repository;
    }
}
