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
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Onboarding;

/// <summary>
/// Routes onboarding plans through approval, with escalation and delegation.
/// </summary>
public sealed partial class OnboardingPlanApprovalEndpoint : ApiOnboarding.OnboardingPlanApprovalService.OnboardingPlanApprovalServiceBase
{
    private readonly ApiAudit.AuditEventQueryService.AuditEventQueryServiceClient _auditAuditEventQueryService;
    private readonly ApiCorporateevents.VenueLifecycleService.VenueLifecycleServiceClient _corporateeventsVenueLifecycleService;
    private readonly ApiDocuments.DocumentTemplateWorkflowService.DocumentTemplateWorkflowServiceClient _documentsDocumentTemplateWorkflowService;
    private readonly ApiIdentity.UserQueryService.UserQueryServiceClient _identityUserQueryService;
    private readonly ApiLearning.CourseWorkflowService.CourseWorkflowServiceClient _learningCourseWorkflowService;
    private readonly ILogger<OnboardingPlanApprovalEndpoint> _logger;
    private readonly ApiNotifications.NotificationValidationService.NotificationValidationServiceClient _notificationsNotificationValidationService;
    private readonly ApiOnboarding.EquipmentLifecycleService.EquipmentLifecycleServiceClient _onboardingEquipmentLifecycleService;
    private readonly ApiOnboarding.EquipmentReportingService.EquipmentReportingServiceClient _onboardingEquipmentReportingService;
    private readonly ApiOnboarding.OnboardingPlanLifecycleService.OnboardingPlanLifecycleServiceClient _onboardingOnboardingPlanLifecycleService;
    private readonly ApiOnboarding.OnboardingPlanQueryService.OnboardingPlanQueryServiceClient _onboardingOnboardingPlanQueryService;
    private readonly ApiOnboarding.OnboardingPlanService.OnboardingPlanServiceClient _onboardingOnboardingPlanService;
    private readonly ApiOnboarding.OnboardingPlanValidationService.OnboardingPlanValidationServiceClient _onboardingOnboardingPlanValidationService;
    private readonly ApiOnboarding.OnboardingTaskQueryService.OnboardingTaskQueryServiceClient _onboardingOnboardingTaskQueryService;
    private readonly ApiOnboarding.OnboardingTaskReportingService.OnboardingTaskReportingServiceClient _onboardingOnboardingTaskReportingService;
    private readonly IOnboardingPlanApprovalEndpointSendGetOnboardingPlanApprovalStatus0Port _onboardingPlanApprovalEndpointSendGetOnboardingPlanApprovalStatus0Port;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiOnboarding.CountOnboardingTaskTotalsRequest, CallOptions, AsyncUnaryCall<ApiOnboarding.CountOnboardingTaskTotalsResponse>> _sendCommentOnboardingPlanApproval0;
    private readonly Func<ApiAudit.FindAuditEventsRequest, CallOptions, AsyncUnaryCall<ApiAudit.FindAuditEventsResponse>> _sendRejectOnboardingPlan2;
    private readonly ApiTravel.TravelBookingApprovalService.TravelBookingApprovalServiceClient _travelTravelBookingApprovalService;
    private readonly ApiTravel.TravelBookingWorkflowService.TravelBookingWorkflowServiceClient _travelTravelBookingWorkflowService;

    public OnboardingPlanApprovalEndpoint(
        ApiAudit.AuditEventQueryService.AuditEventQueryServiceClient auditAuditEventQueryService,
        ApiCorporateevents.VenueLifecycleService.VenueLifecycleServiceClient corporateeventsVenueLifecycleService,
        ApiDocuments.DocumentTemplateWorkflowService.DocumentTemplateWorkflowServiceClient documentsDocumentTemplateWorkflowService,
        ApiIdentity.UserQueryService.UserQueryServiceClient identityUserQueryService,
        ApiLearning.CourseWorkflowService.CourseWorkflowServiceClient learningCourseWorkflowService,
        ILogger<OnboardingPlanApprovalEndpoint> logger,
        ApiNotifications.NotificationValidationService.NotificationValidationServiceClient notificationsNotificationValidationService,
        ApiOnboarding.EquipmentLifecycleService.EquipmentLifecycleServiceClient onboardingEquipmentLifecycleService,
        ApiOnboarding.EquipmentReportingService.EquipmentReportingServiceClient onboardingEquipmentReportingService,
        ApiOnboarding.OnboardingPlanLifecycleService.OnboardingPlanLifecycleServiceClient onboardingOnboardingPlanLifecycleService,
        ApiOnboarding.OnboardingPlanQueryService.OnboardingPlanQueryServiceClient onboardingOnboardingPlanQueryService,
        ApiOnboarding.OnboardingPlanService.OnboardingPlanServiceClient onboardingOnboardingPlanService,
        ApiOnboarding.OnboardingPlanValidationService.OnboardingPlanValidationServiceClient onboardingOnboardingPlanValidationService,
        ApiOnboarding.OnboardingTaskQueryService.OnboardingTaskQueryServiceClient onboardingOnboardingTaskQueryService,
        ApiOnboarding.OnboardingTaskReportingService.OnboardingTaskReportingServiceClient onboardingOnboardingTaskReportingService,
        IOnboardingPlanApprovalEndpointSendGetOnboardingPlanApprovalStatus0Port onboardingPlanApprovalEndpointSendGetOnboardingPlanApprovalStatus0PortAdapter,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiTravel.TravelBookingApprovalService.TravelBookingApprovalServiceClient travelTravelBookingApprovalService,
        ApiTravel.TravelBookingWorkflowService.TravelBookingWorkflowServiceClient travelTravelBookingWorkflowService)
    {
        _auditAuditEventQueryService = auditAuditEventQueryService;
        _corporateeventsVenueLifecycleService = corporateeventsVenueLifecycleService;
        _documentsDocumentTemplateWorkflowService = documentsDocumentTemplateWorkflowService;
        _identityUserQueryService = identityUserQueryService;
        _learningCourseWorkflowService = learningCourseWorkflowService;
        _logger = logger;
        _notificationsNotificationValidationService = notificationsNotificationValidationService;
        _onboardingEquipmentLifecycleService = onboardingEquipmentLifecycleService;
        _onboardingEquipmentReportingService = onboardingEquipmentReportingService;
        _onboardingOnboardingPlanLifecycleService = onboardingOnboardingPlanLifecycleService;
        _onboardingOnboardingPlanQueryService = onboardingOnboardingPlanQueryService;
        _onboardingOnboardingPlanService = onboardingOnboardingPlanService;
        _onboardingOnboardingPlanValidationService = onboardingOnboardingPlanValidationService;
        _onboardingOnboardingTaskQueryService = onboardingOnboardingTaskQueryService;
        _onboardingOnboardingTaskReportingService = onboardingOnboardingTaskReportingService;
        _onboardingPlanApprovalEndpointSendGetOnboardingPlanApprovalStatus0Port = onboardingPlanApprovalEndpointSendGetOnboardingPlanApprovalStatus0PortAdapter;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _travelTravelBookingApprovalService = travelTravelBookingApprovalService;
        _travelTravelBookingWorkflowService = travelTravelBookingWorkflowService;
        _sendCommentOnboardingPlanApproval0 = _onboardingOnboardingTaskReportingService.CountOnboardingTaskTotalsAsync;
        _sendRejectOnboardingPlan2 = _auditAuditEventQueryService.FindAuditEventsAsync;
    }
}
