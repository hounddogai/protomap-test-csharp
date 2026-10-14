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
/// Manages documents: creates, reads, changes, and lists them.
/// </summary>
public sealed partial class DocumentEndpoint : ApiDocuments.DocumentService.DocumentServiceBase
{
    private readonly ApiDocuments.DocumentExportService.DocumentExportServiceClient _documentsDocumentExportService;
    private readonly ILogger<DocumentEndpoint> _logger;
    private readonly IRecordRepository _repository;
    private readonly IOptions<MeshOptions> _settings;

    public DocumentEndpoint(
        ApiDocuments.DocumentExportService.DocumentExportServiceClient documentsDocumentExportService,
        ILogger<DocumentEndpoint> logger,
        IRecordRepository repository,
        IOptions<MeshOptions> settings)
    {
        _documentsDocumentExportService = documentsDocumentExportService;
        _logger = logger;
        _repository = repository;
        _settings = settings;
    }
}
