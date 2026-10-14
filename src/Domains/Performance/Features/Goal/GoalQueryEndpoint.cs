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
using ApiContent = Acme.Content.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLineage = Acme.Lineage.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Performance;

/// <summary>
/// Answers read-only questions about goals, for screens and reports.
/// </summary>
public sealed partial class GoalQueryEndpoint : ApiPerformance.GoalQueryService.GoalQueryServiceBase
{
    private readonly NotificationBus _bus;
    private readonly ApiContent.ArticleQueryService.ArticleQueryServiceClient _contentArticleQueryService;
    private readonly ApiDocuments.DocumentTemplateNotificationService.DocumentTemplateNotificationServiceClient _documentsDocumentTemplateNotificationService;
    private readonly ApiDocuments.SignatureExportService.SignatureExportServiceClient _documentsSignatureExportService;
    private readonly IGoalQueryEndpointSendLookupGoal0Port _goalQueryEndpointSendLookupGoal0Port;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.ChangeRequestQueryService.ChangeRequestQueryServiceClient _itsmChangeRequestQueryService;
    private readonly ApiLineage.LineageNodeQueryService.LineageNodeQueryServiceClient _lineageLineageNodeQueryService;
    private readonly ILogger<GoalQueryEndpoint> _logger;
    private readonly ApiPerformance.CalibrationValidationService.CalibrationValidationServiceClient _performanceCalibrationValidationService;
    private readonly ApiPerformance.FeedbackCalculationService.FeedbackCalculationServiceClient _performanceFeedbackCalculationService;
    private readonly ApiPerformance.FeedbackValidationService.FeedbackValidationServiceClient _performanceFeedbackValidationService;
    private readonly ApiPerformance.ReviewQueryService.ReviewQueryServiceClient _performanceReviewQueryService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiPerformance.NormalizeCalibrationRequest, CallOptions, AsyncUnaryCall<ApiPerformance.NormalizeCalibrationResponse>> _sendBrowseGoals0;
    private readonly ApiWorkplaces.VisitorPassCalculationService.VisitorPassCalculationServiceClient _workplacesVisitorPassCalculationService;

    public GoalQueryEndpoint(
        NotificationBus bus,
        ApiContent.ArticleQueryService.ArticleQueryServiceClient contentArticleQueryService,
        ApiDocuments.DocumentTemplateNotificationService.DocumentTemplateNotificationServiceClient documentsDocumentTemplateNotificationService,
        ApiDocuments.SignatureExportService.SignatureExportServiceClient documentsSignatureExportService,
        IGoalQueryEndpointSendLookupGoal0Port goalQueryEndpointSendLookupGoal0PortAdapter,
        CallInvoker invoker,
        ApiItsm.ChangeRequestQueryService.ChangeRequestQueryServiceClient itsmChangeRequestQueryService,
        ApiLineage.LineageNodeQueryService.LineageNodeQueryServiceClient lineageLineageNodeQueryService,
        ILogger<GoalQueryEndpoint> logger,
        ApiPerformance.CalibrationValidationService.CalibrationValidationServiceClient performanceCalibrationValidationService,
        ApiPerformance.FeedbackCalculationService.FeedbackCalculationServiceClient performanceFeedbackCalculationService,
        ApiPerformance.FeedbackValidationService.FeedbackValidationServiceClient performanceFeedbackValidationService,
        ApiPerformance.ReviewQueryService.ReviewQueryServiceClient performanceReviewQueryService,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiWorkplaces.VisitorPassCalculationService.VisitorPassCalculationServiceClient workplacesVisitorPassCalculationService)
    {
        _bus = bus;
        _contentArticleQueryService = contentArticleQueryService;
        _documentsDocumentTemplateNotificationService = documentsDocumentTemplateNotificationService;
        _documentsSignatureExportService = documentsSignatureExportService;
        _goalQueryEndpointSendLookupGoal0Port = goalQueryEndpointSendLookupGoal0PortAdapter;
        _invoker = invoker;
        _itsmChangeRequestQueryService = itsmChangeRequestQueryService;
        _lineageLineageNodeQueryService = lineageLineageNodeQueryService;
        _logger = logger;
        _performanceCalibrationValidationService = performanceCalibrationValidationService;
        _performanceFeedbackCalculationService = performanceFeedbackCalculationService;
        _performanceFeedbackValidationService = performanceFeedbackValidationService;
        _performanceReviewQueryService = performanceReviewQueryService;
        _registry = registry;
        _repository = repository;
        _workplacesVisitorPassCalculationService = workplacesVisitorPassCalculationService;
        _sendBrowseGoals0 = _performanceCalibrationValidationService.NormalizeCalibrationAsync;
    }
}
