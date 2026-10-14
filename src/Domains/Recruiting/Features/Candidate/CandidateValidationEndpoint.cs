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
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Recruiting;

/// <summary>
/// Checks candidates against the rules before anything acts on them.
/// </summary>
public sealed partial class CandidateValidationEndpoint : ApiRecruiting.CandidateValidationService.CandidateValidationServiceBase
{
    private readonly ApiAuthorization.AccessPolicyQueryService.AccessPolicyQueryServiceClient _authorizationAccessPolicyQueryService;
    private readonly NotificationBus _bus;
    private readonly ApiDocuments.DocumentFolderQueryService.DocumentFolderQueryServiceClient _documentsDocumentFolderQueryService;
    private readonly ApiDocuments.SignatureValidationService.SignatureValidationServiceClient _documentsSignatureValidationService;
    private readonly ILogger<CandidateValidationEndpoint> _logger;
    private readonly ApiPerformance.CalibrationValidationService.CalibrationValidationServiceClient _performanceCalibrationValidationService;
    private readonly RetryPolicy _policy;
    private readonly ApiRecruiting.CandidateCalculationService.CandidateCalculationServiceClient _recruitingCandidateCalculationService;
    private readonly ApiRecruiting.InterviewCalculationService.InterviewCalculationServiceClient _recruitingInterviewCalculationService;
    private readonly ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient _recruitingJobOpeningCalculationService;
    private readonly ApiRecruiting.JobOpeningValidationService.JobOpeningValidationServiceClient _recruitingJobOpeningValidationService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiRecruiting.ExplainJobOpeningDecisionRequest, CallOptions, AsyncUnaryCall<ApiRecruiting.ExplainJobOpeningDecisionResponse>> _sendValidateCandidate0;
    private readonly ApiTimekeeping.TimesheetService.TimesheetServiceClient _timekeepingTimesheetService;

    public CandidateValidationEndpoint(
        ApiAuthorization.AccessPolicyQueryService.AccessPolicyQueryServiceClient authorizationAccessPolicyQueryService,
        NotificationBus bus,
        ApiDocuments.DocumentFolderQueryService.DocumentFolderQueryServiceClient documentsDocumentFolderQueryService,
        ApiDocuments.SignatureValidationService.SignatureValidationServiceClient documentsSignatureValidationService,
        ILogger<CandidateValidationEndpoint> logger,
        ApiPerformance.CalibrationValidationService.CalibrationValidationServiceClient performanceCalibrationValidationService,
        RetryPolicy policy,
        ApiRecruiting.CandidateCalculationService.CandidateCalculationServiceClient recruitingCandidateCalculationService,
        ApiRecruiting.InterviewCalculationService.InterviewCalculationServiceClient recruitingInterviewCalculationService,
        ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient recruitingJobOpeningCalculationService,
        ApiRecruiting.JobOpeningValidationService.JobOpeningValidationServiceClient recruitingJobOpeningValidationService,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiTimekeeping.TimesheetService.TimesheetServiceClient timekeepingTimesheetService)
    {
        _authorizationAccessPolicyQueryService = authorizationAccessPolicyQueryService;
        _bus = bus;
        _documentsDocumentFolderQueryService = documentsDocumentFolderQueryService;
        _documentsSignatureValidationService = documentsSignatureValidationService;
        _logger = logger;
        _performanceCalibrationValidationService = performanceCalibrationValidationService;
        _policy = policy;
        _recruitingCandidateCalculationService = recruitingCandidateCalculationService;
        _recruitingInterviewCalculationService = recruitingInterviewCalculationService;
        _recruitingJobOpeningCalculationService = recruitingJobOpeningCalculationService;
        _recruitingJobOpeningValidationService = recruitingJobOpeningValidationService;
        _registry = registry;
        _repository = repository;
        _timekeepingTimesheetService = timekeepingTimesheetService;
        _sendValidateCandidate0 = _recruitingJobOpeningValidationService.ExplainJobOpeningDecisionAsync;
    }
}
