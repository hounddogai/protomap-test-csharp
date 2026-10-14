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
/// Checks signatures against the rules before anything acts on them.
/// </summary>
public sealed partial class SignatureValidationEndpoint : ApiDocuments.SignatureValidationService.SignatureValidationServiceBase
{
    private readonly ApiDocuments.SignatureHistoryService.SignatureHistoryServiceClient _documentsSignatureHistoryService;
    private readonly ILogger<SignatureValidationEndpoint> _logger;
    private readonly IRecordRepository _repository;

    public SignatureValidationEndpoint(
        ApiDocuments.SignatureHistoryService.SignatureHistoryServiceClient documentsSignatureHistoryService,
        ILogger<SignatureValidationEndpoint> logger,
        IRecordRepository repository)
    {
        _documentsSignatureHistoryService = documentsSignatureHistoryService;
        _logger = logger;
        _repository = repository;
    }
}
