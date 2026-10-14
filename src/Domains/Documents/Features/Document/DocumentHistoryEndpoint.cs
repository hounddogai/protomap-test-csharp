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
using ApiDocuments = Acme.Documents.V1;

namespace Acme.Workforce.Documents;

/// <summary>
/// Keeps every version of a document and restores earlier ones.
/// </summary>
public sealed partial class DocumentHistoryEndpoint : ApiDocuments.DocumentHistoryService.DocumentHistoryServiceBase
{
    private readonly ILogger<DocumentHistoryEndpoint> _logger;
    private readonly IRecordRepository _repository;

    public DocumentHistoryEndpoint(
        ILogger<DocumentHistoryEndpoint> logger,
        IRecordRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }
}
