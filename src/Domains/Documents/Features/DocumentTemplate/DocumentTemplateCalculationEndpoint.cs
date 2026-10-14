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
using ApiAuthorization = Acme.Authorization.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;

namespace Acme.Workforce.Documents;

/// <summary>
/// Computes the figures of a document template, such as totals, taxes, and estimates.
/// </summary>
public sealed partial class DocumentTemplateCalculationEndpoint : ApiDocuments.DocumentTemplateCalculationService.DocumentTemplateCalculationServiceBase
{
    private readonly ApiAuthorization.AccessPolicyQueryService.AccessPolicyQueryServiceClient _authorizationAccessPolicyQueryService;
    private readonly IDocumentTemplateCalculationEndpointSendEstimateDocumentTemplate0Port _documentTemplateCalculationEndpointSendEstimateDocumentTemplate0Port;
    private readonly ApiDocuments.DocumentFolderSyncService.DocumentFolderSyncServiceClient _documentsDocumentFolderSyncService;
    private readonly ApiDocuments.DocumentSyncService.DocumentSyncServiceClient _documentsDocumentSyncService;
    private readonly ApiDocuments.SignatureCalculationService.SignatureCalculationServiceClient _documentsSignatureCalculationService;
    private readonly ILogger<DocumentTemplateCalculationEndpoint> _logger;
    private readonly IRecordRepository _repository;

    public DocumentTemplateCalculationEndpoint(
        ApiAuthorization.AccessPolicyQueryService.AccessPolicyQueryServiceClient authorizationAccessPolicyQueryService,
        IDocumentTemplateCalculationEndpointSendEstimateDocumentTemplate0Port documentTemplateCalculationEndpointSendEstimateDocumentTemplate0PortAdapter,
        ApiDocuments.DocumentFolderSyncService.DocumentFolderSyncServiceClient documentsDocumentFolderSyncService,
        ApiDocuments.DocumentSyncService.DocumentSyncServiceClient documentsDocumentSyncService,
        ApiDocuments.SignatureCalculationService.SignatureCalculationServiceClient documentsSignatureCalculationService,
        ILogger<DocumentTemplateCalculationEndpoint> logger,
        IRecordRepository repository)
    {
        _authorizationAccessPolicyQueryService = authorizationAccessPolicyQueryService;
        _documentTemplateCalculationEndpointSendEstimateDocumentTemplate0Port = documentTemplateCalculationEndpointSendEstimateDocumentTemplate0PortAdapter;
        _documentsDocumentFolderSyncService = documentsDocumentFolderSyncService;
        _documentsDocumentSyncService = documentsDocumentSyncService;
        _documentsSignatureCalculationService = documentsSignatureCalculationService;
        _logger = logger;
        _repository = repository;
    }
}
