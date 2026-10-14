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
using ApiDeliveries = Acme.Deliveries.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLabeling = Acme.Labeling.V1;
using ApiLearning = Acme.Learning.V1;
using ApiMedia = Acme.Media.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Learning;

/// <summary>
/// Routes certifications through approval, with escalation and delegation.
/// </summary>
public sealed partial class CertificationApprovalEndpoint : ApiLearning.CertificationApprovalService.CertificationApprovalServiceBase
{
    private readonly ApiAuthorization.AccessPolicyValidationService.AccessPolicyValidationServiceClient _authorizationAccessPolicyValidationService;
    private readonly NotificationBus _bus;
    private readonly ApiDeliveries.DeliveryService.DeliveryServiceClient _deliveriesDeliveryService;
    private readonly ApiDevices.ManagedDeviceWorkflowService.ManagedDeviceWorkflowServiceClient _devicesManagedDeviceWorkflowService;
    private readonly ApiDocuments.DocumentTemplateHistoryService.DocumentTemplateHistoryServiceClient _documentsDocumentTemplateHistoryService;
    private readonly ApiDocuments.SignatureCalculationService.SignatureCalculationServiceClient _documentsSignatureCalculationService;
    private readonly ApiFeatureflags.FeatureFlagValidationService.FeatureFlagValidationServiceClient _featureflagsFeatureFlagValidationService;
    private readonly CallInvoker _invoker;
    private readonly ApiLabeling.LabelingTaskQueryService.LabelingTaskQueryServiceClient _labelingLabelingTaskQueryService;
    private readonly ApiLearning.CourseApprovalService.CourseApprovalServiceClient _learningCourseApprovalService;
    private readonly ApiLearning.CourseWorkflowService.CourseWorkflowServiceClient _learningCourseWorkflowService;
    private readonly ApiLearning.LearningPathApprovalService.LearningPathApprovalServiceClient _learningLearningPathApprovalService;
    private readonly ApiLearning.LearningPathQueryService.LearningPathQueryServiceClient _learningLearningPathQueryService;
    private readonly ILogger<CertificationApprovalEndpoint> _logger;
    private readonly ApiMedia.MediaAssetQueryService.MediaAssetQueryServiceClient _mediaMediaAssetQueryService;
    private readonly ApiRecruiting.CandidateQueryService.CandidateQueryServiceClient _recruitingCandidateQueryService;
    private readonly ApiRecruiting.InterviewValidationService.InterviewValidationServiceClient _recruitingInterviewValidationService;
    private readonly ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient _recruitingJobOpeningCalculationService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiFeatureflags.PreviewFeatureFlagRequest, CallOptions, AsyncUnaryCall<ApiFeatureflags.PreviewFeatureFlagResponse>> _sendEscalateCertification1;
    private readonly ApiWorkplaces.RoomBookingReportingService.RoomBookingReportingServiceClient _workplacesRoomBookingReportingService;

    public CertificationApprovalEndpoint(
        ApiAuthorization.AccessPolicyValidationService.AccessPolicyValidationServiceClient authorizationAccessPolicyValidationService,
        NotificationBus bus,
        ApiDeliveries.DeliveryService.DeliveryServiceClient deliveriesDeliveryService,
        ApiDevices.ManagedDeviceWorkflowService.ManagedDeviceWorkflowServiceClient devicesManagedDeviceWorkflowService,
        ApiDocuments.DocumentTemplateHistoryService.DocumentTemplateHistoryServiceClient documentsDocumentTemplateHistoryService,
        ApiDocuments.SignatureCalculationService.SignatureCalculationServiceClient documentsSignatureCalculationService,
        ApiFeatureflags.FeatureFlagValidationService.FeatureFlagValidationServiceClient featureflagsFeatureFlagValidationService,
        CallInvoker invoker,
        ApiLabeling.LabelingTaskQueryService.LabelingTaskQueryServiceClient labelingLabelingTaskQueryService,
        ApiLearning.CourseApprovalService.CourseApprovalServiceClient learningCourseApprovalService,
        ApiLearning.CourseWorkflowService.CourseWorkflowServiceClient learningCourseWorkflowService,
        ApiLearning.LearningPathApprovalService.LearningPathApprovalServiceClient learningLearningPathApprovalService,
        ApiLearning.LearningPathQueryService.LearningPathQueryServiceClient learningLearningPathQueryService,
        ILogger<CertificationApprovalEndpoint> logger,
        ApiMedia.MediaAssetQueryService.MediaAssetQueryServiceClient mediaMediaAssetQueryService,
        ApiRecruiting.CandidateQueryService.CandidateQueryServiceClient recruitingCandidateQueryService,
        ApiRecruiting.InterviewValidationService.InterviewValidationServiceClient recruitingInterviewValidationService,
        ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient recruitingJobOpeningCalculationService,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiWorkplaces.RoomBookingReportingService.RoomBookingReportingServiceClient workplacesRoomBookingReportingService)
    {
        _authorizationAccessPolicyValidationService = authorizationAccessPolicyValidationService;
        _bus = bus;
        _deliveriesDeliveryService = deliveriesDeliveryService;
        _devicesManagedDeviceWorkflowService = devicesManagedDeviceWorkflowService;
        _documentsDocumentTemplateHistoryService = documentsDocumentTemplateHistoryService;
        _documentsSignatureCalculationService = documentsSignatureCalculationService;
        _featureflagsFeatureFlagValidationService = featureflagsFeatureFlagValidationService;
        _invoker = invoker;
        _labelingLabelingTaskQueryService = labelingLabelingTaskQueryService;
        _learningCourseApprovalService = learningCourseApprovalService;
        _learningCourseWorkflowService = learningCourseWorkflowService;
        _learningLearningPathApprovalService = learningLearningPathApprovalService;
        _learningLearningPathQueryService = learningLearningPathQueryService;
        _logger = logger;
        _mediaMediaAssetQueryService = mediaMediaAssetQueryService;
        _recruitingCandidateQueryService = recruitingCandidateQueryService;
        _recruitingInterviewValidationService = recruitingInterviewValidationService;
        _recruitingJobOpeningCalculationService = recruitingJobOpeningCalculationService;
        _registry = registry;
        _repository = repository;
        _workplacesRoomBookingReportingService = workplacesRoomBookingReportingService;
        _sendEscalateCertification1 = _featureflagsFeatureFlagValidationService.PreviewFeatureFlagAsync;
    }
}
