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
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLearning = Acme.Learning.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Learning;

/// <summary>
/// Answers read-only questions about courses, for screens and reports.
/// </summary>
public sealed partial class CourseQueryEndpoint : ApiLearning.CourseQueryService.CourseQueryServiceBase
{
    private readonly ICourseQueryEndpointSendCountCourses0Port _courseQueryEndpointSendCountCourses0Port;
    private readonly ApiDocuments.DocumentFolderQueryService.DocumentFolderQueryServiceClient _documentsDocumentFolderQueryService;
    private readonly ApiDocuments.DocumentValidationService.DocumentValidationServiceClient _documentsDocumentValidationService;
    private readonly ApiItsm.IncidentQueryService.IncidentQueryServiceClient _itsmIncidentQueryService;
    private readonly ApiLearning.CertificationCalculationService.CertificationCalculationServiceClient _learningCertificationCalculationService;
    private readonly ApiLearning.CourseValidationService.CourseValidationServiceClient _learningCourseValidationService;
    private readonly ILogger<CourseQueryEndpoint> _logger;
    private readonly IRecordRepository _repository;
    private readonly ApiTravel.TripCalculationService.TripCalculationServiceClient _travelTripCalculationService;

    public CourseQueryEndpoint(
        ICourseQueryEndpointSendCountCourses0Port courseQueryEndpointSendCountCourses0PortAdapter,
        ApiDocuments.DocumentFolderQueryService.DocumentFolderQueryServiceClient documentsDocumentFolderQueryService,
        ApiDocuments.DocumentValidationService.DocumentValidationServiceClient documentsDocumentValidationService,
        ApiItsm.IncidentQueryService.IncidentQueryServiceClient itsmIncidentQueryService,
        ApiLearning.CertificationCalculationService.CertificationCalculationServiceClient learningCertificationCalculationService,
        ApiLearning.CourseValidationService.CourseValidationServiceClient learningCourseValidationService,
        ILogger<CourseQueryEndpoint> logger,
        IRecordRepository repository,
        ApiTravel.TripCalculationService.TripCalculationServiceClient travelTripCalculationService)
    {
        _courseQueryEndpointSendCountCourses0Port = courseQueryEndpointSendCountCourses0PortAdapter;
        _documentsDocumentFolderQueryService = documentsDocumentFolderQueryService;
        _documentsDocumentValidationService = documentsDocumentValidationService;
        _itsmIncidentQueryService = itsmIncidentQueryService;
        _learningCertificationCalculationService = learningCertificationCalculationService;
        _learningCourseValidationService = learningCourseValidationService;
        _logger = logger;
        _repository = repository;
        _travelTripCalculationService = travelTripCalculationService;
    }
}
