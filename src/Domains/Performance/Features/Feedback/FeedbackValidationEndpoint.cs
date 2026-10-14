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
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiKeymanagement = Acme.Keymanagement.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Performance;

/// <summary>
/// Checks feedbacks against the rules before anything acts on them.
/// </summary>
public sealed partial class FeedbackValidationEndpoint : ApiPerformance.FeedbackValidationService.FeedbackValidationServiceBase
{
    private readonly ApiAuthorization.AccessPolicyValidationService.AccessPolicyValidationServiceClient _authorizationAccessPolicyValidationService;
    private readonly ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient _documentsDocumentTemplateSyncService;
    private readonly ApiDocuments.SignatureValidationService.SignatureValidationServiceClient _documentsSignatureValidationService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.ChangeRequestQueryService.ChangeRequestQueryServiceClient _itsmChangeRequestQueryService;
    private readonly ApiKeymanagement.CryptoKeyCalculationService.CryptoKeyCalculationServiceClient _keymanagementCryptoKeyCalculationService;
    private readonly ILogger<FeedbackValidationEndpoint> _logger;
    private readonly ApiMetrics.MetricSeriesValidationService.MetricSeriesValidationServiceClient _metricsMetricSeriesValidationService;
    private readonly ApiNotifications.NotificationQueryService.NotificationQueryServiceClient _notificationsNotificationQueryService;
    private readonly ApiPerformance.CalibrationCalculationService.CalibrationCalculationServiceClient _performanceCalibrationCalculationService;
    private readonly ApiPerformance.FeedbackCalculationService.FeedbackCalculationServiceClient _performanceFeedbackCalculationService;
    private readonly ApiPerformance.GoalValidationService.GoalValidationServiceClient _performanceGoalValidationService;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;

    public FeedbackValidationEndpoint(
        ApiAuthorization.AccessPolicyValidationService.AccessPolicyValidationServiceClient authorizationAccessPolicyValidationService,
        ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient documentsDocumentTemplateSyncService,
        ApiDocuments.SignatureValidationService.SignatureValidationServiceClient documentsSignatureValidationService,
        CallInvoker invoker,
        ApiItsm.ChangeRequestQueryService.ChangeRequestQueryServiceClient itsmChangeRequestQueryService,
        ApiKeymanagement.CryptoKeyCalculationService.CryptoKeyCalculationServiceClient keymanagementCryptoKeyCalculationService,
        ILogger<FeedbackValidationEndpoint> logger,
        ApiMetrics.MetricSeriesValidationService.MetricSeriesValidationServiceClient metricsMetricSeriesValidationService,
        ApiNotifications.NotificationQueryService.NotificationQueryServiceClient notificationsNotificationQueryService,
        ApiPerformance.CalibrationCalculationService.CalibrationCalculationServiceClient performanceCalibrationCalculationService,
        ApiPerformance.FeedbackCalculationService.FeedbackCalculationServiceClient performanceFeedbackCalculationService,
        ApiPerformance.GoalValidationService.GoalValidationServiceClient performanceGoalValidationService,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository)
    {
        _authorizationAccessPolicyValidationService = authorizationAccessPolicyValidationService;
        _documentsDocumentTemplateSyncService = documentsDocumentTemplateSyncService;
        _documentsSignatureValidationService = documentsSignatureValidationService;
        _invoker = invoker;
        _itsmChangeRequestQueryService = itsmChangeRequestQueryService;
        _keymanagementCryptoKeyCalculationService = keymanagementCryptoKeyCalculationService;
        _logger = logger;
        _metricsMetricSeriesValidationService = metricsMetricSeriesValidationService;
        _notificationsNotificationQueryService = notificationsNotificationQueryService;
        _performanceCalibrationCalculationService = performanceCalibrationCalculationService;
        _performanceFeedbackCalculationService = performanceFeedbackCalculationService;
        _performanceGoalValidationService = performanceGoalValidationService;
        _policy = policy;
        _registry = registry;
        _repository = repository;
    }
}
