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
/// Checks documents against the rules before anything acts on them.
/// </summary>
public sealed partial class DocumentValidationEndpoint : ApiDocuments.DocumentValidationService.DocumentValidationServiceBase
{
    private readonly ApiDocuments.DocumentAdminService.DocumentAdminServiceClient _documentsDocumentAdminService;
    private readonly ApiDocuments.SignatureSyncService.SignatureSyncServiceClient _documentsSignatureSyncService;
    private readonly ILogger<DocumentValidationEndpoint> _logger;
    private readonly IRecordRepository _repository;

    public DocumentValidationEndpoint(
        ApiDocuments.DocumentAdminService.DocumentAdminServiceClient documentsDocumentAdminService,
        ApiDocuments.SignatureSyncService.SignatureSyncServiceClient documentsSignatureSyncService,
        ILogger<DocumentValidationEndpoint> logger,
        IRecordRepository repository)
    {
        _documentsDocumentAdminService = documentsDocumentAdminService;
        _documentsSignatureSyncService = documentsSignatureSyncService;
        _logger = logger;
        _repository = repository;
    }
}
