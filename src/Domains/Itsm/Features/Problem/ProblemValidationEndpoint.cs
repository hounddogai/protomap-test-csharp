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
using ApiAudit = Acme.Audit.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiSearch = Acme.Search.V1;

namespace Acme.Workforce.Itsm;

/// <summary>
/// Checks problems against the rules before anything acts on them.
/// </summary>
public sealed partial class ProblemValidationEndpoint : ApiItsm.ProblemValidationService.ProblemValidationServiceBase
{
    private readonly ApiAudit.AuditEventValidationService.AuditEventValidationServiceClient _auditAuditEventValidationService;
    private readonly ApiDocuments.DocumentExportService.DocumentExportServiceClient _documentsDocumentExportService;
    private readonly ApiDocuments.DocumentFolderSyncService.DocumentFolderSyncServiceClient _documentsDocumentFolderSyncService;
    private readonly ApiIdentity.UserValidationService.UserValidationServiceClient _identityUserValidationService;
    private readonly ApiItsm.ChangeRequestCalculationService.ChangeRequestCalculationServiceClient _itsmChangeRequestCalculationService;
    private readonly ApiItsm.ChangeRequestValidationService.ChangeRequestValidationServiceClient _itsmChangeRequestValidationService;
    private readonly ApiItsm.ProblemCalculationService.ProblemCalculationServiceClient _itsmProblemCalculationService;
    private readonly ApiItsm.ServiceAssetCalculationService.ServiceAssetCalculationServiceClient _itsmServiceAssetCalculationService;
    private readonly ApiItsm.ServiceAssetValidationService.ServiceAssetValidationServiceClient _itsmServiceAssetValidationService;
    private readonly ApiLicensing.SoftwareLicenseQueryService.SoftwareLicenseQueryServiceClient _licensingSoftwareLicenseQueryService;
    private readonly ILogger<ProblemValidationEndpoint> _logger;
    private readonly ApiMetrics.MetricSeriesValidationService.MetricSeriesValidationServiceClient _metricsMetricSeriesValidationService;
    private readonly IRecordRepository _repository;
    private readonly ApiSearch.SearchIndexValidationService.SearchIndexValidationServiceClient _searchSearchIndexValidationService;
    private readonly Func<ApiItsm.QuoteServiceAssetRequest, CallOptions, AsyncUnaryCall<ApiItsm.QuoteServiceAssetResponse>> _sendPreviewProblem2;

    public ProblemValidationEndpoint(
        ApiAudit.AuditEventValidationService.AuditEventValidationServiceClient auditAuditEventValidationService,
        ApiDocuments.DocumentExportService.DocumentExportServiceClient documentsDocumentExportService,
        ApiDocuments.DocumentFolderSyncService.DocumentFolderSyncServiceClient documentsDocumentFolderSyncService,
        ApiIdentity.UserValidationService.UserValidationServiceClient identityUserValidationService,
        ApiItsm.ChangeRequestCalculationService.ChangeRequestCalculationServiceClient itsmChangeRequestCalculationService,
        ApiItsm.ChangeRequestValidationService.ChangeRequestValidationServiceClient itsmChangeRequestValidationService,
        ApiItsm.ProblemCalculationService.ProblemCalculationServiceClient itsmProblemCalculationService,
        ApiItsm.ServiceAssetCalculationService.ServiceAssetCalculationServiceClient itsmServiceAssetCalculationService,
        ApiItsm.ServiceAssetValidationService.ServiceAssetValidationServiceClient itsmServiceAssetValidationService,
        ApiLicensing.SoftwareLicenseQueryService.SoftwareLicenseQueryServiceClient licensingSoftwareLicenseQueryService,
        ILogger<ProblemValidationEndpoint> logger,
        ApiMetrics.MetricSeriesValidationService.MetricSeriesValidationServiceClient metricsMetricSeriesValidationService,
        IRecordRepository repository,
        ApiSearch.SearchIndexValidationService.SearchIndexValidationServiceClient searchSearchIndexValidationService)
    {
        _auditAuditEventValidationService = auditAuditEventValidationService;
        _documentsDocumentExportService = documentsDocumentExportService;
        _documentsDocumentFolderSyncService = documentsDocumentFolderSyncService;
        _identityUserValidationService = identityUserValidationService;
        _itsmChangeRequestCalculationService = itsmChangeRequestCalculationService;
        _itsmChangeRequestValidationService = itsmChangeRequestValidationService;
        _itsmProblemCalculationService = itsmProblemCalculationService;
        _itsmServiceAssetCalculationService = itsmServiceAssetCalculationService;
        _itsmServiceAssetValidationService = itsmServiceAssetValidationService;
        _licensingSoftwareLicenseQueryService = licensingSoftwareLicenseQueryService;
        _logger = logger;
        _metricsMetricSeriesValidationService = metricsMetricSeriesValidationService;
        _repository = repository;
        _searchSearchIndexValidationService = searchSearchIndexValidationService;
        _sendPreviewProblem2 = _itsmServiceAssetCalculationService.QuoteServiceAssetAsync;
    }
}
