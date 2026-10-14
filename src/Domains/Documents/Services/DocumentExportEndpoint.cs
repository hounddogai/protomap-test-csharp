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
/// Exports documents to files and other systems.
/// </summary>
public sealed partial class DocumentExportEndpoint : ApiDocuments.DocumentExportService.DocumentExportServiceBase
{
    private readonly ApiDocuments.DocumentSyncService.DocumentSyncServiceClient _documentsDocumentSyncService;
    private readonly ILogger<DocumentExportEndpoint> _logger;
    private readonly IRecordRepository _repository;

    public DocumentExportEndpoint(
        ApiDocuments.DocumentSyncService.DocumentSyncServiceClient documentsDocumentSyncService,
        ILogger<DocumentExportEndpoint> logger,
        IRecordRepository repository)
    {
        _documentsDocumentSyncService = documentsDocumentSyncService;
        _logger = logger;
        _repository = repository;
    }
}
