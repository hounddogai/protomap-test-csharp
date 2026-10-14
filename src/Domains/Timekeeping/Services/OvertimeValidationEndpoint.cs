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
using ApiBenefits = Acme.Benefits.V1;
using ApiCommon = Acme.Common.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiMessaging = Acme.Messaging.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Timekeeping;

/// <summary>
/// Checks overtimes against the rules before anything acts on them.
/// </summary>
public sealed partial class OvertimeValidationEndpoint : ApiTimekeeping.OvertimeValidationService.OvertimeValidationServiceBase
{
    private readonly ApiBenefits.BenefitPlanQueryService.BenefitPlanQueryServiceClient _benefitsBenefitPlanQueryService;
    private readonly ILogger<OvertimeValidationEndpoint> _logger;
    private readonly ApiMessaging.ConversationCalculationService.ConversationCalculationServiceClient _messagingConversationCalculationService;
    private readonly ApiNotifications.NotificationQueryService.NotificationQueryServiceClient _notificationsNotificationQueryService;
    private readonly ApiNotifications.NotificationValidationService.NotificationValidationServiceClient _notificationsNotificationValidationService;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiTimekeeping.ExplainTimesheetDecisionRequest, CallOptions, AsyncUnaryCall<ApiTimekeeping.ExplainTimesheetDecisionResponse>> _sendValidateOvertime1;
    private readonly ApiTimekeeping.ClockEventCalculationService.ClockEventCalculationServiceClient _timekeepingClockEventCalculationService;
    private readonly ApiTimekeeping.ClockEventValidationService.ClockEventValidationServiceClient _timekeepingClockEventValidationService;
    private readonly ApiTimekeeping.OvertimeCalculationService.OvertimeCalculationServiceClient _timekeepingOvertimeCalculationService;
    private readonly ApiTimekeeping.TimesheetValidationService.TimesheetValidationServiceClient _timekeepingTimesheetValidationService;
    private readonly ApiTravel.TravelPolicyCalculationService.TravelPolicyCalculationServiceClient _travelTravelPolicyCalculationService;

    public OvertimeValidationEndpoint(
        ApiBenefits.BenefitPlanQueryService.BenefitPlanQueryServiceClient benefitsBenefitPlanQueryService,
        ILogger<OvertimeValidationEndpoint> logger,
        ApiMessaging.ConversationCalculationService.ConversationCalculationServiceClient messagingConversationCalculationService,
        ApiNotifications.NotificationQueryService.NotificationQueryServiceClient notificationsNotificationQueryService,
        ApiNotifications.NotificationValidationService.NotificationValidationServiceClient notificationsNotificationValidationService,
        IRecordRepository repository,
        ApiTimekeeping.ClockEventCalculationService.ClockEventCalculationServiceClient timekeepingClockEventCalculationService,
        ApiTimekeeping.ClockEventValidationService.ClockEventValidationServiceClient timekeepingClockEventValidationService,
        ApiTimekeeping.OvertimeCalculationService.OvertimeCalculationServiceClient timekeepingOvertimeCalculationService,
        ApiTimekeeping.TimesheetValidationService.TimesheetValidationServiceClient timekeepingTimesheetValidationService,
        ApiTravel.TravelPolicyCalculationService.TravelPolicyCalculationServiceClient travelTravelPolicyCalculationService)
    {
        _benefitsBenefitPlanQueryService = benefitsBenefitPlanQueryService;
        _logger = logger;
        _messagingConversationCalculationService = messagingConversationCalculationService;
        _notificationsNotificationQueryService = notificationsNotificationQueryService;
        _notificationsNotificationValidationService = notificationsNotificationValidationService;
        _repository = repository;
        _timekeepingClockEventCalculationService = timekeepingClockEventCalculationService;
        _timekeepingClockEventValidationService = timekeepingClockEventValidationService;
        _timekeepingOvertimeCalculationService = timekeepingOvertimeCalculationService;
        _timekeepingTimesheetValidationService = timekeepingTimesheetValidationService;
        _travelTravelPolicyCalculationService = travelTravelPolicyCalculationService;
        _sendValidateOvertime1 = _timekeepingTimesheetValidationService.ExplainTimesheetDecisionAsync;
    }
}
