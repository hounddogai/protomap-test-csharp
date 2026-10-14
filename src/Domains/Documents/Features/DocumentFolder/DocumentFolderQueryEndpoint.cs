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
/// Answers read-only questions about document folders, for screens and reports.
/// </summary>
public sealed partial class DocumentFolderQueryEndpoint : ApiDocuments.DocumentFolderQueryService.DocumentFolderQueryServiceBase
{
    private readonly ILogger<DocumentFolderQueryEndpoint> _logger;
    private readonly IRecordRepository _repository;

    public DocumentFolderQueryEndpoint(
        ILogger<DocumentFolderQueryEndpoint> logger,
        IRecordRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }
}
