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
using ApiFuel = Acme.Fuel.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSurveys = Acme.Surveys.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Learning;

/// <summary>
/// Checks course enrollments against the rules before anything acts on them.
/// </summary>
public sealed partial class CourseEnrollmentValidationEndpoint : ApiLearning.CourseEnrollmentValidationService.CourseEnrollmentValidationServiceBase
{
    private readonly ICourseEnrollmentValidationEndpointSendGetCourseEnrollmentRules0Port _courseEnrollmentValidationEndpointSendGetCourseEnrollmentRules0Port;
    private readonly ApiFuel.FuelCardService.FuelCardServiceClient _fuelFuelCardService;
    private readonly ApiHelpdesk.KnowledgeArticleValidationService.KnowledgeArticleValidationServiceClient _helpdeskKnowledgeArticleValidationService;
    private readonly CallInvoker _invoker;
    private readonly ApiLearning.CertificationValidationService.CertificationValidationServiceClient _learningCertificationValidationService;
    private readonly ApiLearning.CourseEnrollmentCalculationService.CourseEnrollmentCalculationServiceClient _learningCourseEnrollmentCalculationService;
    private readonly ApiLearning.CourseValidationService.CourseValidationServiceClient _learningCourseValidationService;
    private readonly ApiLearning.LearningPathCalculationService.LearningPathCalculationServiceClient _learningLearningPathCalculationService;
    private readonly ApiLearning.LearningPathValidationService.LearningPathValidationServiceClient _learningLearningPathValidationService;
    private readonly ILogger<CourseEnrollmentValidationEndpoint> _logger;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiLearning.SimulateLearningPathRequest, CallOptions, AsyncUnaryCall<ApiLearning.SimulateLearningPathResponse>> _sendPreviewCourseEnrollment3;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiSurveys.SurveyQueryService.SurveyQueryServiceClient _surveysSurveyQueryService;
    private readonly ApiTravel.TravelBookingCalculationService.TravelBookingCalculationServiceClient _travelTravelBookingCalculationService;
    private readonly ApiTravel.TravelBookingQueryService.TravelBookingQueryServiceClient _travelTravelBookingQueryService;
    private readonly ApiTravel.TripQueryService.TripQueryServiceClient _travelTripQueryService;
    private readonly ApiWorkplaces.BuildingService.BuildingServiceClient _workplacesBuildingService;

    public CourseEnrollmentValidationEndpoint(
        ICourseEnrollmentValidationEndpointSendGetCourseEnrollmentRules0Port courseEnrollmentValidationEndpointSendGetCourseEnrollmentRules0PortAdapter,
        ApiFuel.FuelCardService.FuelCardServiceClient fuelFuelCardService,
        ApiHelpdesk.KnowledgeArticleValidationService.KnowledgeArticleValidationServiceClient helpdeskKnowledgeArticleValidationService,
        CallInvoker invoker,
        ApiLearning.CertificationValidationService.CertificationValidationServiceClient learningCertificationValidationService,
        ApiLearning.CourseEnrollmentCalculationService.CourseEnrollmentCalculationServiceClient learningCourseEnrollmentCalculationService,
        ApiLearning.CourseValidationService.CourseValidationServiceClient learningCourseValidationService,
        ApiLearning.LearningPathCalculationService.LearningPathCalculationServiceClient learningLearningPathCalculationService,
        ApiLearning.LearningPathValidationService.LearningPathValidationServiceClient learningLearningPathValidationService,
        ILogger<CourseEnrollmentValidationEndpoint> logger,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiSurveys.SurveyQueryService.SurveyQueryServiceClient surveysSurveyQueryService,
        ApiTravel.TravelBookingCalculationService.TravelBookingCalculationServiceClient travelTravelBookingCalculationService,
        ApiTravel.TravelBookingQueryService.TravelBookingQueryServiceClient travelTravelBookingQueryService,
        ApiTravel.TripQueryService.TripQueryServiceClient travelTripQueryService,
        ApiWorkplaces.BuildingService.BuildingServiceClient workplacesBuildingService)
    {
        _courseEnrollmentValidationEndpointSendGetCourseEnrollmentRules0Port = courseEnrollmentValidationEndpointSendGetCourseEnrollmentRules0PortAdapter;
        _fuelFuelCardService = fuelFuelCardService;
        _helpdeskKnowledgeArticleValidationService = helpdeskKnowledgeArticleValidationService;
        _invoker = invoker;
        _learningCertificationValidationService = learningCertificationValidationService;
        _learningCourseEnrollmentCalculationService = learningCourseEnrollmentCalculationService;
        _learningCourseValidationService = learningCourseValidationService;
        _learningLearningPathCalculationService = learningLearningPathCalculationService;
        _learningLearningPathValidationService = learningLearningPathValidationService;
        _logger = logger;
        _registry = registry;
        _repository = repository;
        _settings = settings;
        _surveysSurveyQueryService = surveysSurveyQueryService;
        _travelTravelBookingCalculationService = travelTravelBookingCalculationService;
        _travelTravelBookingQueryService = travelTravelBookingQueryService;
        _travelTripQueryService = travelTripQueryService;
        _workplacesBuildingService = workplacesBuildingService;
        _sendPreviewCourseEnrollment3 = _learningLearningPathCalculationService.SimulateLearningPathAsync;
    }
}
