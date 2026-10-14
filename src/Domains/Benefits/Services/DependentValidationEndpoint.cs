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
using ApiAuthorization = Acme.Authorization.V1;
using ApiBenefits = Acme.Benefits.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiWebhooks = Acme.Webhooks.V1;

namespace Acme.Workforce.Benefits;

/// <summary>
/// Checks dependents against the rules before anything acts on them.
/// </summary>
public sealed partial class DependentValidationEndpoint : ApiBenefits.DependentValidationService.DependentValidationServiceBase
{
    private readonly ApiAudit.AuditEventService.AuditEventServiceClient _auditAuditEventService;
    private readonly ApiAuthorization.AccessPolicyService.AccessPolicyServiceClient _authorizationAccessPolicyService;
    private readonly ApiBenefits.LifeEventCalculationService.LifeEventCalculationServiceClient _benefitsLifeEventCalculationService;
    private readonly IDependentValidationEndpointSendScoreDependent0Port _dependentValidationEndpointSendScoreDependent0Port;
    private readonly ApiDocuments.DocumentQueryService.DocumentQueryServiceClient _documentsDocumentQueryService;
    private readonly ApiHelpdesk.KnowledgeArticleCalculationService.KnowledgeArticleCalculationServiceClient _helpdeskKnowledgeArticleCalculationService;
    private readonly ApiLearning.CourseQueryService.CourseQueryServiceClient _learningCourseQueryService;
    private readonly ApiLedger.LedgerAccountQueryService.LedgerAccountQueryServiceClient _ledgerLedgerAccountQueryService;
    private readonly ILogger<DependentValidationEndpoint> _logger;
    private readonly ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient _metricsMetricSeriesQueryService;
    private readonly ApiPerformance.CalibrationCalculationService.CalibrationCalculationServiceClient _performanceCalibrationCalculationService;
    private readonly RetryPolicy _policy;
    private readonly IRecordRepository _repository;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiWebhooks.WebhookEndpointQueryService.WebhookEndpointQueryServiceClient _webhooksWebhookEndpointQueryService;

    public DependentValidationEndpoint(
        ApiAudit.AuditEventService.AuditEventServiceClient auditAuditEventService,
        ApiAuthorization.AccessPolicyService.AccessPolicyServiceClient authorizationAccessPolicyService,
        ApiBenefits.LifeEventCalculationService.LifeEventCalculationServiceClient benefitsLifeEventCalculationService,
        IDependentValidationEndpointSendScoreDependent0Port dependentValidationEndpointSendScoreDependent0PortAdapter,
        ApiDocuments.DocumentQueryService.DocumentQueryServiceClient documentsDocumentQueryService,
        ApiHelpdesk.KnowledgeArticleCalculationService.KnowledgeArticleCalculationServiceClient helpdeskKnowledgeArticleCalculationService,
        ApiLearning.CourseQueryService.CourseQueryServiceClient learningCourseQueryService,
        ApiLedger.LedgerAccountQueryService.LedgerAccountQueryServiceClient ledgerLedgerAccountQueryService,
        ILogger<DependentValidationEndpoint> logger,
        ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient metricsMetricSeriesQueryService,
        ApiPerformance.CalibrationCalculationService.CalibrationCalculationServiceClient performanceCalibrationCalculationService,
        RetryPolicy policy,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiWebhooks.WebhookEndpointQueryService.WebhookEndpointQueryServiceClient webhooksWebhookEndpointQueryService)
    {
        _auditAuditEventService = auditAuditEventService;
        _authorizationAccessPolicyService = authorizationAccessPolicyService;
        _benefitsLifeEventCalculationService = benefitsLifeEventCalculationService;
        _dependentValidationEndpointSendScoreDependent0Port = dependentValidationEndpointSendScoreDependent0PortAdapter;
        _documentsDocumentQueryService = documentsDocumentQueryService;
        _helpdeskKnowledgeArticleCalculationService = helpdeskKnowledgeArticleCalculationService;
        _learningCourseQueryService = learningCourseQueryService;
        _ledgerLedgerAccountQueryService = ledgerLedgerAccountQueryService;
        _logger = logger;
        _metricsMetricSeriesQueryService = metricsMetricSeriesQueryService;
        _performanceCalibrationCalculationService = performanceCalibrationCalculationService;
        _policy = policy;
        _repository = repository;
        _settings = settings;
        _webhooksWebhookEndpointQueryService = webhooksWebhookEndpointQueryService;
    }
}
