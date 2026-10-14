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
/// Keeps signatures in step with the systems of record in other regions.
/// </summary>
public sealed partial class SignatureSyncEndpoint : ApiDocuments.SignatureSyncService.SignatureSyncServiceBase
{
    private readonly ILogger<SignatureSyncEndpoint> _logger;
    private readonly IRecordRepository _repository;

    public SignatureSyncEndpoint(
        ILogger<SignatureSyncEndpoint> logger,
        IRecordRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }
}
