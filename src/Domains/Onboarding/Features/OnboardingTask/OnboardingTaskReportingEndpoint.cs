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
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiMedia = Acme.Media.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiReviews = Acme.Reviews.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Onboarding;

/// <summary>
/// Builds and schedules reports about onboarding tasks.
/// </summary>
public sealed partial class OnboardingTaskReportingEndpoint : ApiOnboarding.OnboardingTaskReportingService.OnboardingTaskReportingServiceBase
{
    private readonly ApiDocuments.DocumentQueryService.DocumentQueryServiceClient _documentsDocumentQueryService;
    private readonly ApiDocuments.SignatureCalculationService.SignatureCalculationServiceClient _documentsSignatureCalculationService;
    private readonly ApiHelpdesk.SlaPolicyWorkflowService.SlaPolicyWorkflowServiceClient _helpdeskSlaPolicyWorkflowService;
    private readonly ApiLearning.LearningPathWorkflowService.LearningPathWorkflowServiceClient _learningLearningPathWorkflowService;
    private readonly ILogger<OnboardingTaskReportingEndpoint> _logger;
    private readonly ApiMedia.MediaAssetQueryService.MediaAssetQueryServiceClient _mediaMediaAssetQueryService;
    private readonly ApiOnboarding.OnboardingPlanCalculationService.OnboardingPlanCalculationServiceClient _onboardingOnboardingPlanCalculationService;
    private readonly ApiOnboarding.OnboardingTaskValidationService.OnboardingTaskValidationServiceClient _onboardingOnboardingTaskValidationService;
    private readonly IOnboardingTaskReportingEndpointSendGenerateOnboardingTaskReport2Port _onboardingTaskReportingEndpointSendGenerateOnboardingTaskReport2Port;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiReviews.ProductReviewQueryService.ProductReviewQueryServiceClient _reviewsProductReviewQueryService;
    private readonly Func<ApiWorkplaces.ResumeBuildingWorkflowRequest, CallOptions, AsyncUnaryCall<ApiWorkplaces.ResumeBuildingWorkflowResponse>> _sendStreamOnboardingTaskMetrics0;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiWorkplaces.BuildingWorkflowService.BuildingWorkflowServiceClient _workplacesBuildingWorkflowService;

    public OnboardingTaskReportingEndpoint(
        ApiDocuments.DocumentQueryService.DocumentQueryServiceClient documentsDocumentQueryService,
        ApiDocuments.SignatureCalculationService.SignatureCalculationServiceClient documentsSignatureCalculationService,
        ApiHelpdesk.SlaPolicyWorkflowService.SlaPolicyWorkflowServiceClient helpdeskSlaPolicyWorkflowService,
        ApiLearning.LearningPathWorkflowService.LearningPathWorkflowServiceClient learningLearningPathWorkflowService,
        ILogger<OnboardingTaskReportingEndpoint> logger,
        ApiMedia.MediaAssetQueryService.MediaAssetQueryServiceClient mediaMediaAssetQueryService,
        ApiOnboarding.OnboardingPlanCalculationService.OnboardingPlanCalculationServiceClient onboardingOnboardingPlanCalculationService,
        ApiOnboarding.OnboardingTaskValidationService.OnboardingTaskValidationServiceClient onboardingOnboardingTaskValidationService,
        IOnboardingTaskReportingEndpointSendGenerateOnboardingTaskReport2Port onboardingTaskReportingEndpointSendGenerateOnboardingTaskReport2PortAdapter,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiReviews.ProductReviewQueryService.ProductReviewQueryServiceClient reviewsProductReviewQueryService,
        IOptions<MeshOptions> settings,
        ApiWorkplaces.BuildingWorkflowService.BuildingWorkflowServiceClient workplacesBuildingWorkflowService)
    {
        _documentsDocumentQueryService = documentsDocumentQueryService;
        _documentsSignatureCalculationService = documentsSignatureCalculationService;
        _helpdeskSlaPolicyWorkflowService = helpdeskSlaPolicyWorkflowService;
        _learningLearningPathWorkflowService = learningLearningPathWorkflowService;
        _logger = logger;
        _mediaMediaAssetQueryService = mediaMediaAssetQueryService;
        _onboardingOnboardingPlanCalculationService = onboardingOnboardingPlanCalculationService;
        _onboardingOnboardingTaskValidationService = onboardingOnboardingTaskValidationService;
        _onboardingTaskReportingEndpointSendGenerateOnboardingTaskReport2Port = onboardingTaskReportingEndpointSendGenerateOnboardingTaskReport2PortAdapter;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _reviewsProductReviewQueryService = reviewsProductReviewQueryService;
        _settings = settings;
        _workplacesBuildingWorkflowService = workplacesBuildingWorkflowService;
        _sendStreamOnboardingTaskMetrics0 = _workplacesBuildingWorkflowService.ResumeBuildingWorkflowAsync;
    }
}
