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
using ApiMetrics = Acme.Metrics.V1;

namespace Acme.Workforce.Documents;

/// <summary>
/// Operator actions on documents that support staff run by hand.
/// </summary>
public sealed partial class DocumentAdminEndpoint : ApiDocuments.DocumentAdminService.DocumentAdminServiceBase
{
    private readonly ApiDocuments.DocumentFolderSyncService.DocumentFolderSyncServiceClient _documentsDocumentFolderSyncService;
    private readonly ILogger<DocumentAdminEndpoint> _logger;
    private readonly ApiMetrics.MetricSeriesService.MetricSeriesServiceClient _metricsMetricSeriesService;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiMetrics.ListMetricSeriesesRequest, CallOptions, AsyncUnaryCall<ApiMetrics.ListMetricSeriesesResponse>> _sendArchiveDocument0;

    public DocumentAdminEndpoint(
        ApiDocuments.DocumentFolderSyncService.DocumentFolderSyncServiceClient documentsDocumentFolderSyncService,
        ILogger<DocumentAdminEndpoint> logger,
        ApiMetrics.MetricSeriesService.MetricSeriesServiceClient metricsMetricSeriesService,
        IRecordRepository repository)
    {
        _documentsDocumentFolderSyncService = documentsDocumentFolderSyncService;
        _logger = logger;
        _metricsMetricSeriesService = metricsMetricSeriesService;
        _repository = repository;
        _sendArchiveDocument0 = _metricsMetricSeriesService.ListMetricSeriesesAsync;
    }
}
