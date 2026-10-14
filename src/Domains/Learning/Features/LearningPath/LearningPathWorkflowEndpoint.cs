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
using ApiDevices = Acme.Devices.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLearning = Acme.Learning.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Learning;

/// <summary>
/// Runs the long-lived workflow that takes a learning path from start to finish.
/// </summary>
public sealed partial class LearningPathWorkflowEndpoint : ApiLearning.LearningPathWorkflowService.LearningPathWorkflowServiceBase
{
    private readonly ApiDevices.ManagedDeviceWorkflowService.ManagedDeviceWorkflowServiceClient _devicesManagedDeviceWorkflowService;
    private readonly ApiHelpdesk.EscalationApprovalService.EscalationApprovalServiceClient _helpdeskEscalationApprovalService;
    private readonly ApiHelpdesk.TicketApprovalService.TicketApprovalServiceClient _helpdeskTicketApprovalService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.ServiceAssetApprovalService.ServiceAssetApprovalServiceClient _itsmServiceAssetApprovalService;
    private readonly ApiLearning.CertificationApprovalService.CertificationApprovalServiceClient _learningCertificationApprovalService;
    private readonly ApiLearning.CourseValidationService.CourseValidationServiceClient _learningCourseValidationService;
    private readonly ApiLearning.CourseWorkflowService.CourseWorkflowServiceClient _learningCourseWorkflowService;
    private readonly ApiLearning.LearningPathApprovalService.LearningPathApprovalServiceClient _learningLearningPathApprovalService;
    private readonly ApiLearning.LearningPathValidationService.LearningPathValidationServiceClient _learningLearningPathValidationService;
    private readonly ILearningPathWorkflowEndpointSendCancelLearningPathWorkflow0Port _learningPathWorkflowEndpointSendCancelLearningPathWorkflow0Port;
    private readonly ILogger<LearningPathWorkflowEndpoint> _logger;
    private readonly ApiPerformance.CalibrationApprovalService.CalibrationApprovalServiceClient _performanceCalibrationApprovalService;
    private readonly RetryPolicy _policy;
    private readonly ApiRecruiting.InterviewCalculationService.InterviewCalculationServiceClient _recruitingInterviewCalculationService;
    private readonly ApiRecruiting.InterviewValidationService.InterviewValidationServiceClient _recruitingInterviewValidationService;
    private readonly ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient _recruitingJobOpeningCalculationService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiHelpdesk.RejectTicketRequest, CallOptions, AsyncUnaryCall<ApiHelpdesk.RejectTicketResponse>> _sendRetryLearningPathWorkflow1;
    private readonly IOptions<MeshOptions> _settings;

    public LearningPathWorkflowEndpoint(
        ApiDevices.ManagedDeviceWorkflowService.ManagedDeviceWorkflowServiceClient devicesManagedDeviceWorkflowService,
        ApiHelpdesk.EscalationApprovalService.EscalationApprovalServiceClient helpdeskEscalationApprovalService,
        ApiHelpdesk.TicketApprovalService.TicketApprovalServiceClient helpdeskTicketApprovalService,
        CallInvoker invoker,
        ApiItsm.ServiceAssetApprovalService.ServiceAssetApprovalServiceClient itsmServiceAssetApprovalService,
        ApiLearning.CertificationApprovalService.CertificationApprovalServiceClient learningCertificationApprovalService,
        ApiLearning.CourseValidationService.CourseValidationServiceClient learningCourseValidationService,
        ApiLearning.CourseWorkflowService.CourseWorkflowServiceClient learningCourseWorkflowService,
        ApiLearning.LearningPathApprovalService.LearningPathApprovalServiceClient learningLearningPathApprovalService,
        ApiLearning.LearningPathValidationService.LearningPathValidationServiceClient learningLearningPathValidationService,
        ILearningPathWorkflowEndpointSendCancelLearningPathWorkflow0Port learningPathWorkflowEndpointSendCancelLearningPathWorkflow0PortAdapter,
        ILogger<LearningPathWorkflowEndpoint> logger,
        ApiPerformance.CalibrationApprovalService.CalibrationApprovalServiceClient performanceCalibrationApprovalService,
        RetryPolicy policy,
        ApiRecruiting.InterviewCalculationService.InterviewCalculationServiceClient recruitingInterviewCalculationService,
        ApiRecruiting.InterviewValidationService.InterviewValidationServiceClient recruitingInterviewValidationService,
        ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient recruitingJobOpeningCalculationService,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings)
    {
        _devicesManagedDeviceWorkflowService = devicesManagedDeviceWorkflowService;
        _helpdeskEscalationApprovalService = helpdeskEscalationApprovalService;
        _helpdeskTicketApprovalService = helpdeskTicketApprovalService;
        _invoker = invoker;
        _itsmServiceAssetApprovalService = itsmServiceAssetApprovalService;
        _learningCertificationApprovalService = learningCertificationApprovalService;
        _learningCourseValidationService = learningCourseValidationService;
        _learningCourseWorkflowService = learningCourseWorkflowService;
        _learningLearningPathApprovalService = learningLearningPathApprovalService;
        _learningLearningPathValidationService = learningLearningPathValidationService;
        _learningPathWorkflowEndpointSendCancelLearningPathWorkflow0Port = learningPathWorkflowEndpointSendCancelLearningPathWorkflow0PortAdapter;
        _logger = logger;
        _performanceCalibrationApprovalService = performanceCalibrationApprovalService;
        _policy = policy;
        _recruitingInterviewCalculationService = recruitingInterviewCalculationService;
        _recruitingInterviewValidationService = recruitingInterviewValidationService;
        _recruitingJobOpeningCalculationService = recruitingJobOpeningCalculationService;
        _registry = registry;
        _repository = repository;
        _settings = settings;
        _sendRetryLearningPathWorkflow1 = _helpdeskTicketApprovalService.RejectTicketAsync;
    }
}
