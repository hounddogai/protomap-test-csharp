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
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Onboarding;

/// <summary>
/// Answers read-only questions about onboarding plans, for screens and reports.
/// </summary>
public sealed partial class OnboardingPlanQueryEndpoint : ApiOnboarding.OnboardingPlanQueryService.OnboardingPlanQueryServiceBase
{
    private readonly ApiDocuments.DocumentSyncService.DocumentSyncServiceClient _documentsDocumentSyncService;
    private readonly ApiHelpdesk.KnowledgeArticleValidationService.KnowledgeArticleValidationServiceClient _helpdeskKnowledgeArticleValidationService;
    private readonly CallInvoker _invoker;
    private readonly ILogger<OnboardingPlanQueryEndpoint> _logger;
    private readonly ApiNotifications.NotificationValidationService.NotificationValidationServiceClient _notificationsNotificationValidationService;
    private readonly ApiOnboarding.OnboardingTaskCalculationService.OnboardingTaskCalculationServiceClient _onboardingOnboardingTaskCalculationService;
    private readonly IRecordRepository _repository;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiWorkplaces.RoomBookingValidationService.RoomBookingValidationServiceClient _workplacesRoomBookingValidationService;

    public OnboardingPlanQueryEndpoint(
        ApiDocuments.DocumentSyncService.DocumentSyncServiceClient documentsDocumentSyncService,
        ApiHelpdesk.KnowledgeArticleValidationService.KnowledgeArticleValidationServiceClient helpdeskKnowledgeArticleValidationService,
        CallInvoker invoker,
        ILogger<OnboardingPlanQueryEndpoint> logger,
        ApiNotifications.NotificationValidationService.NotificationValidationServiceClient notificationsNotificationValidationService,
        ApiOnboarding.OnboardingTaskCalculationService.OnboardingTaskCalculationServiceClient onboardingOnboardingTaskCalculationService,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiWorkplaces.RoomBookingValidationService.RoomBookingValidationServiceClient workplacesRoomBookingValidationService)
    {
        _documentsDocumentSyncService = documentsDocumentSyncService;
        _helpdeskKnowledgeArticleValidationService = helpdeskKnowledgeArticleValidationService;
        _invoker = invoker;
        _logger = logger;
        _notificationsNotificationValidationService = notificationsNotificationValidationService;
        _onboardingOnboardingTaskCalculationService = onboardingOnboardingTaskCalculationService;
        _repository = repository;
        _settings = settings;
        _workplacesRoomBookingValidationService = workplacesRoomBookingValidationService;
    }
}
