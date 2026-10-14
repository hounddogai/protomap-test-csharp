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
/// Tells the people who follow a document template what changed.
/// </summary>
public sealed partial class DocumentTemplateNotificationEndpoint : ApiDocuments.DocumentTemplateNotificationService.DocumentTemplateNotificationServiceBase
{
    private readonly ApiDocuments.DocumentFolderHistoryService.DocumentFolderHistoryServiceClient _documentsDocumentFolderHistoryService;
    private readonly ApiDocuments.DocumentTemplateWorkflowService.DocumentTemplateWorkflowServiceClient _documentsDocumentTemplateWorkflowService;
    private readonly ILogger<DocumentTemplateNotificationEndpoint> _logger;
    private readonly ApiMetrics.MetricSeriesService.MetricSeriesServiceClient _metricsMetricSeriesService;
    private readonly IRecordRepository _repository;

    public DocumentTemplateNotificationEndpoint(
        ApiDocuments.DocumentFolderHistoryService.DocumentFolderHistoryServiceClient documentsDocumentFolderHistoryService,
        ApiDocuments.DocumentTemplateWorkflowService.DocumentTemplateWorkflowServiceClient documentsDocumentTemplateWorkflowService,
        ILogger<DocumentTemplateNotificationEndpoint> logger,
        ApiMetrics.MetricSeriesService.MetricSeriesServiceClient metricsMetricSeriesService,
        IRecordRepository repository)
    {
        _documentsDocumentFolderHistoryService = documentsDocumentFolderHistoryService;
        _documentsDocumentTemplateWorkflowService = documentsDocumentTemplateWorkflowService;
        _logger = logger;
        _metricsMetricSeriesService = metricsMetricSeriesService;
        _repository = repository;
    }
}
