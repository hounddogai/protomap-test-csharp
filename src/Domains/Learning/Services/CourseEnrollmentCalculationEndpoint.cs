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
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTicketing = Acme.Ticketing.V1;

namespace Acme.Workforce.Learning;

/// <summary>
/// Computes the figures of a course enrollment, such as totals, taxes, and estimates.
/// </summary>
public sealed partial class CourseEnrollmentCalculationEndpoint : ApiLearning.CourseEnrollmentCalculationService.CourseEnrollmentCalculationServiceBase
{
    private readonly ApiAuthorization.AccessPolicyQueryService.AccessPolicyQueryServiceClient _authorizationAccessPolicyQueryService;
    private readonly ApiDevices.DevicePolicyQueryService.DevicePolicyQueryServiceClient _devicesDevicePolicyQueryService;
    private readonly ApiDocuments.DocumentHistoryService.DocumentHistoryServiceClient _documentsDocumentHistoryService;
    private readonly ApiHelpdesk.SlaPolicyValidationService.SlaPolicyValidationServiceClient _helpdeskSlaPolicyValidationService;
    private readonly ApiLearning.CertificationCalculationService.CertificationCalculationServiceClient _learningCertificationCalculationService;
    private readonly ApiLearning.LearningPathCalculationService.LearningPathCalculationServiceClient _learningLearningPathCalculationService;
    private readonly ApiLearning.LearningPathValidationService.LearningPathValidationServiceClient _learningLearningPathValidationService;
    private readonly ILogger<CourseEnrollmentCalculationEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiLearning.ScoreLearningPathRequest, CallOptions, AsyncUnaryCall<ApiLearning.ScoreLearningPathResponse>> _sendExplainCourseEnrollmentCalculation0;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTicketing.TicketService.TicketServiceClient _ticketingTicketService;

    public CourseEnrollmentCalculationEndpoint(
        ApiAuthorization.AccessPolicyQueryService.AccessPolicyQueryServiceClient authorizationAccessPolicyQueryService,
        ApiDevices.DevicePolicyQueryService.DevicePolicyQueryServiceClient devicesDevicePolicyQueryService,
        ApiDocuments.DocumentHistoryService.DocumentHistoryServiceClient documentsDocumentHistoryService,
        ApiHelpdesk.SlaPolicyValidationService.SlaPolicyValidationServiceClient helpdeskSlaPolicyValidationService,
        ApiLearning.CertificationCalculationService.CertificationCalculationServiceClient learningCertificationCalculationService,
        ApiLearning.LearningPathCalculationService.LearningPathCalculationServiceClient learningLearningPathCalculationService,
        ApiLearning.LearningPathValidationService.LearningPathValidationServiceClient learningLearningPathValidationService,
        ILogger<CourseEnrollmentCalculationEndpoint> logger,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiTicketing.TicketService.TicketServiceClient ticketingTicketService)
    {
        _authorizationAccessPolicyQueryService = authorizationAccessPolicyQueryService;
        _devicesDevicePolicyQueryService = devicesDevicePolicyQueryService;
        _documentsDocumentHistoryService = documentsDocumentHistoryService;
        _helpdeskSlaPolicyValidationService = helpdeskSlaPolicyValidationService;
        _learningCertificationCalculationService = learningCertificationCalculationService;
        _learningLearningPathCalculationService = learningLearningPathCalculationService;
        _learningLearningPathValidationService = learningLearningPathValidationService;
        _logger = logger;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _settings = settings;
        _ticketingTicketService = ticketingTicketService;
        _sendExplainCourseEnrollmentCalculation0 = _learningLearningPathValidationService.ScoreLearningPathAsync;
    }
}
