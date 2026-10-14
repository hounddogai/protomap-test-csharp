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
using ApiIdentity = Acme.Identity.V1;

namespace Acme.Workforce.Documents;

/// <summary>
/// Exports document folders to files and other systems.
/// </summary>
public sealed partial class DocumentFolderExportEndpoint : ApiDocuments.DocumentFolderExportService.DocumentFolderExportServiceBase
{
    private readonly ApiDocuments.SignatureCalculationService.SignatureCalculationServiceClient _documentsSignatureCalculationService;
    private readonly ILogger<DocumentFolderExportEndpoint> _logger;
    private readonly IRecordRepository _repository;

    public DocumentFolderExportEndpoint(
        ApiDocuments.SignatureCalculationService.SignatureCalculationServiceClient documentsSignatureCalculationService,
        ILogger<DocumentFolderExportEndpoint> logger,
        IRecordRepository repository)
    {
        _documentsSignatureCalculationService = documentsSignatureCalculationService;
        _logger = logger;
        _repository = repository;
    }
}
