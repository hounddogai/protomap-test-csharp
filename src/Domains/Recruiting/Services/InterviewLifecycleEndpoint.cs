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
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Recruiting;

/// <summary>
/// Moves an interview between the states of its lifecycle.
/// </summary>
public sealed partial class InterviewLifecycleEndpoint : ApiRecruiting.InterviewLifecycleService.InterviewLifecycleServiceBase
{
    private readonly ApiDocuments.SignatureCalculationService.SignatureCalculationServiceClient _documentsSignatureCalculationService;
    private readonly ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient _employeesOrgUnitQueryService;
    private readonly ApiEmployees.PositionValidationService.PositionValidationServiceClient _employeesPositionValidationService;
    private readonly ApiHelpdesk.TicketWorkflowService.TicketWorkflowServiceClient _helpdeskTicketWorkflowService;
    private readonly ApiIdentity.UserQueryService.UserQueryServiceClient _identityUserQueryService;
    private readonly IInterviewLifecycleEndpointSendGetInterviewLifecycle0Port _interviewLifecycleEndpointSendGetInterviewLifecycle0Port;
    private readonly ApiLearning.CertificationWorkflowService.CertificationWorkflowServiceClient _learningCertificationWorkflowService;
    private readonly ILogger<InterviewLifecycleEndpoint> _logger;
    private readonly ApiNotifications.NotificationQueryService.NotificationQueryServiceClient _notificationsNotificationQueryService;
    private readonly ApiOnboarding.EquipmentWorkflowService.EquipmentWorkflowServiceClient _onboardingEquipmentWorkflowService;
    private readonly ApiOnboarding.OnboardingTaskReportingService.OnboardingTaskReportingServiceClient _onboardingOnboardingTaskReportingService;
    private readonly RetryPolicy _policy;
    private readonly ApiRecruiting.InterviewValidationService.InterviewValidationServiceClient _recruitingInterviewValidationService;
    private readonly ApiRecruiting.JobOpeningValidationService.JobOpeningValidationServiceClient _recruitingJobOpeningValidationService;
    private readonly ApiRecruiting.OfferCalculationService.OfferCalculationServiceClient _recruitingOfferCalculationService;
    private readonly ApiRecruiting.OfferSyncService.OfferSyncServiceClient _recruitingOfferSyncService;
    private readonly IRecordRepository _repository;

    public InterviewLifecycleEndpoint(
        ApiDocuments.SignatureCalculationService.SignatureCalculationServiceClient documentsSignatureCalculationService,
        ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient employeesOrgUnitQueryService,
        ApiEmployees.PositionValidationService.PositionValidationServiceClient employeesPositionValidationService,
        ApiHelpdesk.TicketWorkflowService.TicketWorkflowServiceClient helpdeskTicketWorkflowService,
        ApiIdentity.UserQueryService.UserQueryServiceClient identityUserQueryService,
        IInterviewLifecycleEndpointSendGetInterviewLifecycle0Port interviewLifecycleEndpointSendGetInterviewLifecycle0PortAdapter,
        ApiLearning.CertificationWorkflowService.CertificationWorkflowServiceClient learningCertificationWorkflowService,
        ILogger<InterviewLifecycleEndpoint> logger,
        ApiNotifications.NotificationQueryService.NotificationQueryServiceClient notificationsNotificationQueryService,
        ApiOnboarding.EquipmentWorkflowService.EquipmentWorkflowServiceClient onboardingEquipmentWorkflowService,
        ApiOnboarding.OnboardingTaskReportingService.OnboardingTaskReportingServiceClient onboardingOnboardingTaskReportingService,
        RetryPolicy policy,
        ApiRecruiting.InterviewValidationService.InterviewValidationServiceClient recruitingInterviewValidationService,
        ApiRecruiting.JobOpeningValidationService.JobOpeningValidationServiceClient recruitingJobOpeningValidationService,
        ApiRecruiting.OfferCalculationService.OfferCalculationServiceClient recruitingOfferCalculationService,
        ApiRecruiting.OfferSyncService.OfferSyncServiceClient recruitingOfferSyncService,
        IRecordRepository repository)
    {
        _documentsSignatureCalculationService = documentsSignatureCalculationService;
        _employeesOrgUnitQueryService = employeesOrgUnitQueryService;
        _employeesPositionValidationService = employeesPositionValidationService;
        _helpdeskTicketWorkflowService = helpdeskTicketWorkflowService;
        _identityUserQueryService = identityUserQueryService;
        _interviewLifecycleEndpointSendGetInterviewLifecycle0Port = interviewLifecycleEndpointSendGetInterviewLifecycle0PortAdapter;
        _learningCertificationWorkflowService = learningCertificationWorkflowService;
        _logger = logger;
        _notificationsNotificationQueryService = notificationsNotificationQueryService;
        _onboardingEquipmentWorkflowService = onboardingEquipmentWorkflowService;
        _onboardingOnboardingTaskReportingService = onboardingOnboardingTaskReportingService;
        _policy = policy;
        _recruitingInterviewValidationService = recruitingInterviewValidationService;
        _recruitingJobOpeningValidationService = recruitingJobOpeningValidationService;
        _recruitingOfferCalculationService = recruitingOfferCalculationService;
        _recruitingOfferSyncService = recruitingOfferSyncService;
        _repository = repository;
    }
}
