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
using ApiCareplans = Acme.Careplans.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Recruiting;

/// <summary>
/// Answers read-only questions about candidates, for screens and reports.
/// </summary>
public sealed partial class CandidateQueryEndpoint : ApiRecruiting.CandidateQueryService.CandidateQueryServiceBase
{
    private readonly ApiCareplans.CarePlanService.CarePlanServiceClient _careplansCarePlanService;
    private readonly ApiDocuments.DocumentTemplateWorkflowService.DocumentTemplateWorkflowServiceClient _documentsDocumentTemplateWorkflowService;
    private readonly ApiFeatureflags.FeatureFlagValidationService.FeatureFlagValidationServiceClient _featureflagsFeatureFlagValidationService;
    private readonly ApiIdentity.UserQueryService.UserQueryServiceClient _identityUserQueryService;
    private readonly CallInvoker _invoker;
    private readonly ApiLearning.CourseQueryService.CourseQueryServiceClient _learningCourseQueryService;
    private readonly ILogger<CandidateQueryEndpoint> _logger;
    private readonly ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient _metricsMetricSeriesQueryService;
    private readonly ApiOnboarding.OnboardingTaskCalculationService.OnboardingTaskCalculationServiceClient _onboardingOnboardingTaskCalculationService;
    private readonly RetryPolicy _policy;
    private readonly ApiRecruiting.CandidateCalculationService.CandidateCalculationServiceClient _recruitingCandidateCalculationService;
    private readonly ApiRecruiting.CandidateValidationService.CandidateValidationServiceClient _recruitingCandidateValidationService;
    private readonly ApiRecruiting.InterviewCalculationService.InterviewCalculationServiceClient _recruitingInterviewCalculationService;
    private readonly ApiRecruiting.JobOpeningValidationService.JobOpeningValidationServiceClient _recruitingJobOpeningValidationService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiCareplans.ListCarePlansRequest, CallOptions, AsyncUnaryCall<ApiCareplans.ListCarePlansResponse>> _sendLookupCandidate1;
    private readonly IOptions<MeshOptions> _settings;

    public CandidateQueryEndpoint(
        ApiCareplans.CarePlanService.CarePlanServiceClient careplansCarePlanService,
        ApiDocuments.DocumentTemplateWorkflowService.DocumentTemplateWorkflowServiceClient documentsDocumentTemplateWorkflowService,
        ApiFeatureflags.FeatureFlagValidationService.FeatureFlagValidationServiceClient featureflagsFeatureFlagValidationService,
        ApiIdentity.UserQueryService.UserQueryServiceClient identityUserQueryService,
        CallInvoker invoker,
        ApiLearning.CourseQueryService.CourseQueryServiceClient learningCourseQueryService,
        ILogger<CandidateQueryEndpoint> logger,
        ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient metricsMetricSeriesQueryService,
        ApiOnboarding.OnboardingTaskCalculationService.OnboardingTaskCalculationServiceClient onboardingOnboardingTaskCalculationService,
        RetryPolicy policy,
        ApiRecruiting.CandidateCalculationService.CandidateCalculationServiceClient recruitingCandidateCalculationService,
        ApiRecruiting.CandidateValidationService.CandidateValidationServiceClient recruitingCandidateValidationService,
        ApiRecruiting.InterviewCalculationService.InterviewCalculationServiceClient recruitingInterviewCalculationService,
        ApiRecruiting.JobOpeningValidationService.JobOpeningValidationServiceClient recruitingJobOpeningValidationService,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings)
    {
        _careplansCarePlanService = careplansCarePlanService;
        _documentsDocumentTemplateWorkflowService = documentsDocumentTemplateWorkflowService;
        _featureflagsFeatureFlagValidationService = featureflagsFeatureFlagValidationService;
        _identityUserQueryService = identityUserQueryService;
        _invoker = invoker;
        _learningCourseQueryService = learningCourseQueryService;
        _logger = logger;
        _metricsMetricSeriesQueryService = metricsMetricSeriesQueryService;
        _onboardingOnboardingTaskCalculationService = onboardingOnboardingTaskCalculationService;
        _policy = policy;
        _recruitingCandidateCalculationService = recruitingCandidateCalculationService;
        _recruitingCandidateValidationService = recruitingCandidateValidationService;
        _recruitingInterviewCalculationService = recruitingInterviewCalculationService;
        _recruitingJobOpeningValidationService = recruitingJobOpeningValidationService;
        _registry = registry;
        _repository = repository;
        _settings = settings;
        _sendLookupCandidate1 = _careplansCarePlanService.ListCarePlansAsync;
    }
}
