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
using ApiEmployees = Acme.Employees.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiPerformance = Acme.Performance.V1;

namespace Acme.Workforce.Learning;

/// <summary>
/// Checks courses against the rules before anything acts on them.
/// </summary>
public sealed partial class CourseValidationEndpoint : ApiLearning.CourseValidationService.CourseValidationServiceBase
{
    private readonly ICourseValidationEndpointSendPreviewCourse2Port _courseValidationEndpointSendPreviewCourse2Port;
    private readonly ApiFeatureflags.FeatureFlagValidationService.FeatureFlagValidationServiceClient _featureflagsFeatureFlagValidationService;
    private readonly ApiHelpdesk.KnowledgeArticleValidationService.KnowledgeArticleValidationServiceClient _helpdeskKnowledgeArticleValidationService;
    private readonly ApiIdentity.UserQueryService.UserQueryServiceClient _identityUserQueryService;
    private readonly CallInvoker _invoker;
    private readonly ApiLearning.LearningPathCalculationService.LearningPathCalculationServiceClient _learningLearningPathCalculationService;
    private readonly ILogger<CourseValidationEndpoint> _logger;
    private readonly ApiPerformance.FeedbackValidationService.FeedbackValidationServiceClient _performanceFeedbackValidationService;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiHelpdesk.ValidateKnowledgeArticleRequest, CallOptions, AsyncUnaryCall<ApiHelpdesk.ValidateKnowledgeArticleResponse>> _sendGetCourseRules0;

    public CourseValidationEndpoint(
        ICourseValidationEndpointSendPreviewCourse2Port courseValidationEndpointSendPreviewCourse2PortAdapter,
        ApiFeatureflags.FeatureFlagValidationService.FeatureFlagValidationServiceClient featureflagsFeatureFlagValidationService,
        ApiHelpdesk.KnowledgeArticleValidationService.KnowledgeArticleValidationServiceClient helpdeskKnowledgeArticleValidationService,
        ApiIdentity.UserQueryService.UserQueryServiceClient identityUserQueryService,
        CallInvoker invoker,
        ApiLearning.LearningPathCalculationService.LearningPathCalculationServiceClient learningLearningPathCalculationService,
        ILogger<CourseValidationEndpoint> logger,
        ApiPerformance.FeedbackValidationService.FeedbackValidationServiceClient performanceFeedbackValidationService,
        IRecordRepository repository)
    {
        _courseValidationEndpointSendPreviewCourse2Port = courseValidationEndpointSendPreviewCourse2PortAdapter;
        _featureflagsFeatureFlagValidationService = featureflagsFeatureFlagValidationService;
        _helpdeskKnowledgeArticleValidationService = helpdeskKnowledgeArticleValidationService;
        _identityUserQueryService = identityUserQueryService;
        _invoker = invoker;
        _learningLearningPathCalculationService = learningLearningPathCalculationService;
        _logger = logger;
        _performanceFeedbackValidationService = performanceFeedbackValidationService;
        _repository = repository;
        _sendGetCourseRules0 = _helpdeskKnowledgeArticleValidationService.ValidateKnowledgeArticleAsync;
    }
}
