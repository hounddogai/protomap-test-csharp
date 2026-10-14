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
using ApiDispatch = Acme.Dispatch.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Learning;

/// <summary>
/// Answers read-only questions about learning paths, for screens and reports.
/// </summary>
public sealed partial class LearningPathQueryEndpoint : ApiLearning.LearningPathQueryService.LearningPathQueryServiceBase
{
    private readonly NotificationBus _bus;
    private readonly ApiDispatch.DispatchOrderQueryService.DispatchOrderQueryServiceClient _dispatchDispatchOrderQueryService;
    private readonly CallInvoker _invoker;
    private readonly ApiLearning.CertificationCalculationService.CertificationCalculationServiceClient _learningCertificationCalculationService;
    private readonly ApiLearning.CertificationValidationService.CertificationValidationServiceClient _learningCertificationValidationService;
    private readonly ApiLearning.CourseValidationService.CourseValidationServiceClient _learningCourseValidationService;
    private readonly ApiLearning.LearningPathCalculationService.LearningPathCalculationServiceClient _learningLearningPathCalculationService;
    private readonly ILogger<LearningPathQueryEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly IRecordRepository _repository;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTravel.TravelBookingQueryService.TravelBookingQueryServiceClient _travelTravelBookingQueryService;

    public LearningPathQueryEndpoint(
        NotificationBus bus,
        ApiDispatch.DispatchOrderQueryService.DispatchOrderQueryServiceClient dispatchDispatchOrderQueryService,
        CallInvoker invoker,
        ApiLearning.CertificationCalculationService.CertificationCalculationServiceClient learningCertificationCalculationService,
        ApiLearning.CertificationValidationService.CertificationValidationServiceClient learningCertificationValidationService,
        ApiLearning.CourseValidationService.CourseValidationServiceClient learningCourseValidationService,
        ApiLearning.LearningPathCalculationService.LearningPathCalculationServiceClient learningLearningPathCalculationService,
        ILogger<LearningPathQueryEndpoint> logger,
        RetryPolicy policy,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiTravel.TravelBookingQueryService.TravelBookingQueryServiceClient travelTravelBookingQueryService)
    {
        _bus = bus;
        _dispatchDispatchOrderQueryService = dispatchDispatchOrderQueryService;
        _invoker = invoker;
        _learningCertificationCalculationService = learningCertificationCalculationService;
        _learningCertificationValidationService = learningCertificationValidationService;
        _learningCourseValidationService = learningCourseValidationService;
        _learningLearningPathCalculationService = learningLearningPathCalculationService;
        _logger = logger;
        _policy = policy;
        _repository = repository;
        _settings = settings;
        _travelTravelBookingQueryService = travelTravelBookingQueryService;
    }
}
