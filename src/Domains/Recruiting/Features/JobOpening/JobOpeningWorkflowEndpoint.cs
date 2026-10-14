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
using ApiEmployees = Acme.Employees.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLearning = Acme.Learning.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTravel = Acme.Travel.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Recruiting;

/// <summary>
/// Runs the long-lived workflow that takes a job opening from start to finish.
/// </summary>
public sealed partial class JobOpeningWorkflowEndpoint : ApiRecruiting.JobOpeningWorkflowService.JobOpeningWorkflowServiceBase
{
    private readonly ApiAuthorization.AccessPolicyService.AccessPolicyServiceClient _authorizationAccessPolicyService;
    private readonly ApiDevices.DevicePolicyApprovalService.DevicePolicyApprovalServiceClient _devicesDevicePolicyApprovalService;
    private readonly ApiDocuments.DocumentFolderExportService.DocumentFolderExportServiceClient _documentsDocumentFolderExportService;
    private readonly ApiDocuments.DocumentSyncService.DocumentSyncServiceClient _documentsDocumentSyncService;
    private readonly ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient _employeesEmploymentContractCalculationService;
    private readonly ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient _employeesEmploymentContractValidationService;
    private readonly ApiFeatureflags.FeatureFlagService.FeatureFlagServiceClient _featureflagsFeatureFlagService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.ServiceAssetApprovalService.ServiceAssetApprovalServiceClient _itsmServiceAssetApprovalService;
    private readonly ApiLearning.CourseEnrollmentApprovalService.CourseEnrollmentApprovalServiceClient _learningCourseEnrollmentApprovalService;
    private readonly ApiLearning.CourseWorkflowService.CourseWorkflowServiceClient _learningCourseWorkflowService;
    private readonly ILogger<JobOpeningWorkflowEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly ApiRecruiting.InterviewLifecycleService.InterviewLifecycleServiceClient _recruitingInterviewLifecycleService;
    private readonly ApiRecruiting.InterviewSyncService.InterviewSyncServiceClient _recruitingInterviewSyncService;
    private readonly ApiRecruiting.JobOpeningLifecycleService.JobOpeningLifecycleServiceClient _recruitingJobOpeningLifecycleService;
    private readonly ApiRecruiting.OfferCalculationService.OfferCalculationServiceClient _recruitingOfferCalculationService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiFeatureflags.BatchGetFeatureFlagsRequest, CallOptions, AsyncUnaryCall<ApiFeatureflags.BatchGetFeatureFlagsResponse>> _sendStartJobOpeningWorkflow1;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTravel.TravelPolicyWorkflowService.TravelPolicyWorkflowServiceClient _travelTravelPolicyWorkflowService;
    private readonly ApiVenues.VenueQueryService.VenueQueryServiceClient _venuesVenueQueryService;

    public JobOpeningWorkflowEndpoint(
        ApiAuthorization.AccessPolicyService.AccessPolicyServiceClient authorizationAccessPolicyService,
        ApiDevices.DevicePolicyApprovalService.DevicePolicyApprovalServiceClient devicesDevicePolicyApprovalService,
        ApiDocuments.DocumentFolderExportService.DocumentFolderExportServiceClient documentsDocumentFolderExportService,
        ApiDocuments.DocumentSyncService.DocumentSyncServiceClient documentsDocumentSyncService,
        ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient employeesEmploymentContractCalculationService,
        ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient employeesEmploymentContractValidationService,
        ApiFeatureflags.FeatureFlagService.FeatureFlagServiceClient featureflagsFeatureFlagService,
        CallInvoker invoker,
        ApiItsm.ServiceAssetApprovalService.ServiceAssetApprovalServiceClient itsmServiceAssetApprovalService,
        ApiLearning.CourseEnrollmentApprovalService.CourseEnrollmentApprovalServiceClient learningCourseEnrollmentApprovalService,
        ApiLearning.CourseWorkflowService.CourseWorkflowServiceClient learningCourseWorkflowService,
        ILogger<JobOpeningWorkflowEndpoint> logger,
        RetryPolicy policy,
        ApiRecruiting.InterviewLifecycleService.InterviewLifecycleServiceClient recruitingInterviewLifecycleService,
        ApiRecruiting.InterviewSyncService.InterviewSyncServiceClient recruitingInterviewSyncService,
        ApiRecruiting.JobOpeningLifecycleService.JobOpeningLifecycleServiceClient recruitingJobOpeningLifecycleService,
        ApiRecruiting.OfferCalculationService.OfferCalculationServiceClient recruitingOfferCalculationService,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiTravel.TravelPolicyWorkflowService.TravelPolicyWorkflowServiceClient travelTravelPolicyWorkflowService,
        ApiVenues.VenueQueryService.VenueQueryServiceClient venuesVenueQueryService)
    {
        _authorizationAccessPolicyService = authorizationAccessPolicyService;
        _devicesDevicePolicyApprovalService = devicesDevicePolicyApprovalService;
        _documentsDocumentFolderExportService = documentsDocumentFolderExportService;
        _documentsDocumentSyncService = documentsDocumentSyncService;
        _employeesEmploymentContractCalculationService = employeesEmploymentContractCalculationService;
        _employeesEmploymentContractValidationService = employeesEmploymentContractValidationService;
        _featureflagsFeatureFlagService = featureflagsFeatureFlagService;
        _invoker = invoker;
        _itsmServiceAssetApprovalService = itsmServiceAssetApprovalService;
        _learningCourseEnrollmentApprovalService = learningCourseEnrollmentApprovalService;
        _learningCourseWorkflowService = learningCourseWorkflowService;
        _logger = logger;
        _policy = policy;
        _recruitingInterviewLifecycleService = recruitingInterviewLifecycleService;
        _recruitingInterviewSyncService = recruitingInterviewSyncService;
        _recruitingJobOpeningLifecycleService = recruitingJobOpeningLifecycleService;
        _recruitingOfferCalculationService = recruitingOfferCalculationService;
        _registry = registry;
        _repository = repository;
        _settings = settings;
        _travelTravelPolicyWorkflowService = travelTravelPolicyWorkflowService;
        _venuesVenueQueryService = venuesVenueQueryService;
        _sendStartJobOpeningWorkflow1 = _featureflagsFeatureFlagService.BatchGetFeatureFlagsAsync;
    }
}
