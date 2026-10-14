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
using ApiLearning = Acme.Learning.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSearch = Acme.Search.V1;

namespace Acme.Workforce.Recruiting;

/// <summary>
/// Moves a candidate between the states of its lifecycle.
/// </summary>
public sealed partial class CandidateLifecycleEndpoint : ApiRecruiting.CandidateLifecycleService.CandidateLifecycleServiceBase
{
    private readonly ICandidateLifecycleEndpointSendSuspendCandidate0Port _candidateLifecycleEndpointSendSuspendCandidate0Port;
    private readonly ApiDocuments.DocumentTemplateCalculationService.DocumentTemplateCalculationServiceClient _documentsDocumentTemplateCalculationService;
    private readonly ApiDocuments.DocumentTemplateExportService.DocumentTemplateExportServiceClient _documentsDocumentTemplateExportService;
    private readonly ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient _documentsDocumentTemplateSyncService;
    private readonly CallInvoker _invoker;
    private readonly ApiLearning.CertificationApprovalService.CertificationApprovalServiceClient _learningCertificationApprovalService;
    private readonly ApiLedger.LedgerAccountService.LedgerAccountServiceClient _ledgerLedgerAccountService;
    private readonly ILogger<CandidateLifecycleEndpoint> _logger;
    private readonly ApiMetrics.MetricSeriesValidationService.MetricSeriesValidationServiceClient _metricsMetricSeriesValidationService;
    private readonly ApiNotifications.NotificationQueryService.NotificationQueryServiceClient _notificationsNotificationQueryService;
    private readonly ApiOnboarding.OnboardingTaskApprovalService.OnboardingTaskApprovalServiceClient _onboardingOnboardingTaskApprovalService;
    private readonly RetryPolicy _policy;
    private readonly ApiRecruiting.CandidateCalculationService.CandidateCalculationServiceClient _recruitingCandidateCalculationService;
    private readonly ApiRecruiting.CandidateReportingService.CandidateReportingServiceClient _recruitingCandidateReportingService;
    private readonly ApiRecruiting.InterviewReportingService.InterviewReportingServiceClient _recruitingInterviewReportingService;
    private readonly ApiRecruiting.JobOpeningLifecycleService.JobOpeningLifecycleServiceClient _recruitingJobOpeningLifecycleService;
    private readonly ApiRecruiting.JobOpeningService.JobOpeningServiceClient _recruitingJobOpeningService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiSearch.SearchIndexService.SearchIndexServiceClient _searchSearchIndexService;
    private readonly Func<ApiNotifications.ResolveNotificationsRequest, CallOptions, AsyncUnaryCall<ApiNotifications.ResolveNotificationsResponse>> _sendRenewCandidate0;

    public CandidateLifecycleEndpoint(
        ICandidateLifecycleEndpointSendSuspendCandidate0Port candidateLifecycleEndpointSendSuspendCandidate0PortAdapter,
        ApiDocuments.DocumentTemplateCalculationService.DocumentTemplateCalculationServiceClient documentsDocumentTemplateCalculationService,
        ApiDocuments.DocumentTemplateExportService.DocumentTemplateExportServiceClient documentsDocumentTemplateExportService,
        ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient documentsDocumentTemplateSyncService,
        CallInvoker invoker,
        ApiLearning.CertificationApprovalService.CertificationApprovalServiceClient learningCertificationApprovalService,
        ApiLedger.LedgerAccountService.LedgerAccountServiceClient ledgerLedgerAccountService,
        ILogger<CandidateLifecycleEndpoint> logger,
        ApiMetrics.MetricSeriesValidationService.MetricSeriesValidationServiceClient metricsMetricSeriesValidationService,
        ApiNotifications.NotificationQueryService.NotificationQueryServiceClient notificationsNotificationQueryService,
        ApiOnboarding.OnboardingTaskApprovalService.OnboardingTaskApprovalServiceClient onboardingOnboardingTaskApprovalService,
        RetryPolicy policy,
        ApiRecruiting.CandidateCalculationService.CandidateCalculationServiceClient recruitingCandidateCalculationService,
        ApiRecruiting.CandidateReportingService.CandidateReportingServiceClient recruitingCandidateReportingService,
        ApiRecruiting.InterviewReportingService.InterviewReportingServiceClient recruitingInterviewReportingService,
        ApiRecruiting.JobOpeningLifecycleService.JobOpeningLifecycleServiceClient recruitingJobOpeningLifecycleService,
        ApiRecruiting.JobOpeningService.JobOpeningServiceClient recruitingJobOpeningService,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiSearch.SearchIndexService.SearchIndexServiceClient searchSearchIndexService)
    {
        _candidateLifecycleEndpointSendSuspendCandidate0Port = candidateLifecycleEndpointSendSuspendCandidate0PortAdapter;
        _documentsDocumentTemplateCalculationService = documentsDocumentTemplateCalculationService;
        _documentsDocumentTemplateExportService = documentsDocumentTemplateExportService;
        _documentsDocumentTemplateSyncService = documentsDocumentTemplateSyncService;
        _invoker = invoker;
        _learningCertificationApprovalService = learningCertificationApprovalService;
        _ledgerLedgerAccountService = ledgerLedgerAccountService;
        _logger = logger;
        _metricsMetricSeriesValidationService = metricsMetricSeriesValidationService;
        _notificationsNotificationQueryService = notificationsNotificationQueryService;
        _onboardingOnboardingTaskApprovalService = onboardingOnboardingTaskApprovalService;
        _policy = policy;
        _recruitingCandidateCalculationService = recruitingCandidateCalculationService;
        _recruitingCandidateReportingService = recruitingCandidateReportingService;
        _recruitingInterviewReportingService = recruitingInterviewReportingService;
        _recruitingJobOpeningLifecycleService = recruitingJobOpeningLifecycleService;
        _recruitingJobOpeningService = recruitingJobOpeningService;
        _registry = registry;
        _repository = repository;
        _searchSearchIndexService = searchSearchIndexService;
        _sendRenewCandidate0 = _notificationsNotificationQueryService.ResolveNotificationsAsync;
    }
}
