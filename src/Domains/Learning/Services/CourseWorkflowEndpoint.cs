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
using ApiAudit = Acme.Audit.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Learning;

/// <summary>
/// Runs the long-lived workflow that takes a course from start to finish.
/// </summary>
public sealed partial class CourseWorkflowEndpoint : ApiLearning.CourseWorkflowService.CourseWorkflowServiceBase
{
    private readonly ApiAudit.AuditEventValidationService.AuditEventValidationServiceClient _auditAuditEventValidationService;
    private readonly NotificationBus _bus;
    private readonly ICourseWorkflowEndpointSendStartCourseWorkflow1Port _courseWorkflowEndpointSendStartCourseWorkflow1Port;
    private readonly ApiDocuments.SignatureCalculationService.SignatureCalculationServiceClient _documentsSignatureCalculationService;
    private readonly ApiLearning.CourseApprovalService.CourseApprovalServiceClient _learningCourseApprovalService;
    private readonly ApiLearning.LearningPathApprovalService.LearningPathApprovalServiceClient _learningLearningPathApprovalService;
    private readonly ApiLearning.LearningPathQueryService.LearningPathQueryServiceClient _learningLearningPathQueryService;
    private readonly ApiLearning.LearningPathValidationService.LearningPathValidationServiceClient _learningLearningPathValidationService;
    private readonly ApiLicensing.LicenseAssignmentReportingService.LicenseAssignmentReportingServiceClient _licensingLicenseAssignmentReportingService;
    private readonly ILogger<CourseWorkflowEndpoint> _logger;
    private readonly ApiPerformance.GoalApprovalService.GoalApprovalServiceClient _performanceGoalApprovalService;
    private readonly ApiRecruiting.CandidateCalculationService.CandidateCalculationServiceClient _recruitingCandidateCalculationService;
    private readonly ApiRecruiting.CandidateValidationService.CandidateValidationServiceClient _recruitingCandidateValidationService;
    private readonly IRecordRepository _repository;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiWorkplaces.VisitorPassReportingService.VisitorPassReportingServiceClient _workplacesVisitorPassReportingService;

    public CourseWorkflowEndpoint(
        ApiAudit.AuditEventValidationService.AuditEventValidationServiceClient auditAuditEventValidationService,
        NotificationBus bus,
        ICourseWorkflowEndpointSendStartCourseWorkflow1Port courseWorkflowEndpointSendStartCourseWorkflow1PortAdapter,
        ApiDocuments.SignatureCalculationService.SignatureCalculationServiceClient documentsSignatureCalculationService,
        ApiLearning.CourseApprovalService.CourseApprovalServiceClient learningCourseApprovalService,
        ApiLearning.LearningPathApprovalService.LearningPathApprovalServiceClient learningLearningPathApprovalService,
        ApiLearning.LearningPathQueryService.LearningPathQueryServiceClient learningLearningPathQueryService,
        ApiLearning.LearningPathValidationService.LearningPathValidationServiceClient learningLearningPathValidationService,
        ApiLicensing.LicenseAssignmentReportingService.LicenseAssignmentReportingServiceClient licensingLicenseAssignmentReportingService,
        ILogger<CourseWorkflowEndpoint> logger,
        ApiPerformance.GoalApprovalService.GoalApprovalServiceClient performanceGoalApprovalService,
        ApiRecruiting.CandidateCalculationService.CandidateCalculationServiceClient recruitingCandidateCalculationService,
        ApiRecruiting.CandidateValidationService.CandidateValidationServiceClient recruitingCandidateValidationService,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiWorkplaces.VisitorPassReportingService.VisitorPassReportingServiceClient workplacesVisitorPassReportingService)
    {
        _auditAuditEventValidationService = auditAuditEventValidationService;
        _bus = bus;
        _courseWorkflowEndpointSendStartCourseWorkflow1Port = courseWorkflowEndpointSendStartCourseWorkflow1PortAdapter;
        _documentsSignatureCalculationService = documentsSignatureCalculationService;
        _learningCourseApprovalService = learningCourseApprovalService;
        _learningLearningPathApprovalService = learningLearningPathApprovalService;
        _learningLearningPathQueryService = learningLearningPathQueryService;
        _learningLearningPathValidationService = learningLearningPathValidationService;
        _licensingLicenseAssignmentReportingService = licensingLicenseAssignmentReportingService;
        _logger = logger;
        _performanceGoalApprovalService = performanceGoalApprovalService;
        _recruitingCandidateCalculationService = recruitingCandidateCalculationService;
        _recruitingCandidateValidationService = recruitingCandidateValidationService;
        _repository = repository;
        _settings = settings;
        _workplacesVisitorPassReportingService = workplacesVisitorPassReportingService;
    }
}
