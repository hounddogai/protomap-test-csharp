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
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSearch = Acme.Search.V1;

namespace Acme.Workforce.Learning;

/// <summary>
/// Routes course enrollments through approval, with escalation and delegation.
/// </summary>
public sealed partial class CourseEnrollmentApprovalEndpoint : ApiLearning.CourseEnrollmentApprovalService.CourseEnrollmentApprovalServiceBase
{
    private readonly ApiCorporateevents.CorporateEventWorkflowService.CorporateEventWorkflowServiceClient _corporateeventsCorporateEventWorkflowService;
    private readonly ICourseEnrollmentApprovalEndpointSendRejectCourseEnrollment3Port _courseEnrollmentApprovalEndpointSendRejectCourseEnrollment3Port;
    private readonly ApiDocuments.DocumentService.DocumentServiceClient _documentsDocumentService;
    private readonly ApiLearning.CertificationValidationService.CertificationValidationServiceClient _learningCertificationValidationService;
    private readonly ApiLearning.CourseEnrollmentCalculationService.CourseEnrollmentCalculationServiceClient _learningCourseEnrollmentCalculationService;
    private readonly ApiLearning.LearningPathApprovalService.LearningPathApprovalServiceClient _learningLearningPathApprovalService;
    private readonly ApiLearning.LearningPathCalculationService.LearningPathCalculationServiceClient _learningLearningPathCalculationService;
    private readonly ApiLicensing.LicenseAssignmentApprovalService.LicenseAssignmentApprovalServiceClient _licensingLicenseAssignmentApprovalService;
    private readonly ILogger<CourseEnrollmentApprovalEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly ApiRecruiting.CandidateCalculationService.CandidateCalculationServiceClient _recruitingCandidateCalculationService;
    private readonly ApiRecruiting.CandidateValidationService.CandidateValidationServiceClient _recruitingCandidateValidationService;
    private readonly ApiRecruiting.InterviewCalculationService.InterviewCalculationServiceClient _recruitingInterviewCalculationService;
    private readonly ApiRecruiting.InterviewValidationService.InterviewValidationServiceClient _recruitingInterviewValidationService;
    private readonly ApiRecruiting.JobOpeningQueryService.JobOpeningQueryServiceClient _recruitingJobOpeningQueryService;
    private readonly ApiRecruiting.JobOpeningValidationService.JobOpeningValidationServiceClient _recruitingJobOpeningValidationService;
    private readonly IRecordRepository _repository;
    private readonly ApiSearch.SearchIndexValidationService.SearchIndexValidationServiceClient _searchSearchIndexValidationService;
    private readonly Func<ApiLicensing.DelegateLicenseAssignmentApprovalRequest, CallOptions, AsyncUnaryCall<ApiLicensing.DelegateLicenseAssignmentApprovalResponse>> _sendListPendingCourseEnrollments0;
    private readonly IOptions<MeshOptions> _settings;

    public CourseEnrollmentApprovalEndpoint(
        ApiCorporateevents.CorporateEventWorkflowService.CorporateEventWorkflowServiceClient corporateeventsCorporateEventWorkflowService,
        ICourseEnrollmentApprovalEndpointSendRejectCourseEnrollment3Port courseEnrollmentApprovalEndpointSendRejectCourseEnrollment3PortAdapter,
        ApiDocuments.DocumentService.DocumentServiceClient documentsDocumentService,
        ApiLearning.CertificationValidationService.CertificationValidationServiceClient learningCertificationValidationService,
        ApiLearning.CourseEnrollmentCalculationService.CourseEnrollmentCalculationServiceClient learningCourseEnrollmentCalculationService,
        ApiLearning.LearningPathApprovalService.LearningPathApprovalServiceClient learningLearningPathApprovalService,
        ApiLearning.LearningPathCalculationService.LearningPathCalculationServiceClient learningLearningPathCalculationService,
        ApiLicensing.LicenseAssignmentApprovalService.LicenseAssignmentApprovalServiceClient licensingLicenseAssignmentApprovalService,
        ILogger<CourseEnrollmentApprovalEndpoint> logger,
        RetryPolicy policy,
        ApiRecruiting.CandidateCalculationService.CandidateCalculationServiceClient recruitingCandidateCalculationService,
        ApiRecruiting.CandidateValidationService.CandidateValidationServiceClient recruitingCandidateValidationService,
        ApiRecruiting.InterviewCalculationService.InterviewCalculationServiceClient recruitingInterviewCalculationService,
        ApiRecruiting.InterviewValidationService.InterviewValidationServiceClient recruitingInterviewValidationService,
        ApiRecruiting.JobOpeningQueryService.JobOpeningQueryServiceClient recruitingJobOpeningQueryService,
        ApiRecruiting.JobOpeningValidationService.JobOpeningValidationServiceClient recruitingJobOpeningValidationService,
        IRecordRepository repository,
        ApiSearch.SearchIndexValidationService.SearchIndexValidationServiceClient searchSearchIndexValidationService,
        IOptions<MeshOptions> settings)
    {
        _corporateeventsCorporateEventWorkflowService = corporateeventsCorporateEventWorkflowService;
        _courseEnrollmentApprovalEndpointSendRejectCourseEnrollment3Port = courseEnrollmentApprovalEndpointSendRejectCourseEnrollment3PortAdapter;
        _documentsDocumentService = documentsDocumentService;
        _learningCertificationValidationService = learningCertificationValidationService;
        _learningCourseEnrollmentCalculationService = learningCourseEnrollmentCalculationService;
        _learningLearningPathApprovalService = learningLearningPathApprovalService;
        _learningLearningPathCalculationService = learningLearningPathCalculationService;
        _licensingLicenseAssignmentApprovalService = licensingLicenseAssignmentApprovalService;
        _logger = logger;
        _policy = policy;
        _recruitingCandidateCalculationService = recruitingCandidateCalculationService;
        _recruitingCandidateValidationService = recruitingCandidateValidationService;
        _recruitingInterviewCalculationService = recruitingInterviewCalculationService;
        _recruitingInterviewValidationService = recruitingInterviewValidationService;
        _recruitingJobOpeningQueryService = recruitingJobOpeningQueryService;
        _recruitingJobOpeningValidationService = recruitingJobOpeningValidationService;
        _repository = repository;
        _searchSearchIndexValidationService = searchSearchIndexValidationService;
        _settings = settings;
        _sendListPendingCourseEnrollments0 = _licensingLicenseAssignmentApprovalService.DelegateLicenseAssignmentApprovalAsync;
    }
}
