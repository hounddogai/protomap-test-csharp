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
using ApiAuthorization = Acme.Authorization.V1;
using ApiCommon = Acme.Common.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiMessaging = Acme.Messaging.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Licensing;

/// <summary>
/// Runs the long-lived workflow that takes an entitlement from start to finish.
/// </summary>
public sealed partial class EntitlementWorkflowEndpoint : ApiLicensing.EntitlementWorkflowService.EntitlementWorkflowServiceBase
{
    private readonly ApiAudit.AuditEventService.AuditEventServiceClient _auditAuditEventService;
    private readonly ApiAuthorization.AccessPolicyValidationService.AccessPolicyValidationServiceClient _authorizationAccessPolicyValidationService;
    private readonly CallInvoker _invoker;
    private readonly ApiLicensing.EntitlementApprovalService.EntitlementApprovalServiceClient _licensingEntitlementApprovalService;
    private readonly ApiLicensing.LicenseAssignmentApprovalService.LicenseAssignmentApprovalServiceClient _licensingLicenseAssignmentApprovalService;
    private readonly ApiLicensing.LicenseAssignmentCalculationService.LicenseAssignmentCalculationServiceClient _licensingLicenseAssignmentCalculationService;
    private readonly ApiLicensing.SoftwareLicenseApprovalService.SoftwareLicenseApprovalServiceClient _licensingSoftwareLicenseApprovalService;
    private readonly ApiLicensing.SoftwareLicenseService.SoftwareLicenseServiceClient _licensingSoftwareLicenseService;
    private readonly ApiLicensing.SoftwareLicenseValidationService.SoftwareLicenseValidationServiceClient _licensingSoftwareLicenseValidationService;
    private readonly ILogger<EntitlementWorkflowEndpoint> _logger;
    private readonly ApiMessaging.ConversationQueryService.ConversationQueryServiceClient _messagingConversationQueryService;
    private readonly ApiNotifications.NotificationService.NotificationServiceClient _notificationsNotificationService;
    private readonly IRecordRepository _repository;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTravel.TripCalculationService.TripCalculationServiceClient _travelTripCalculationService;

    public EntitlementWorkflowEndpoint(
        ApiAudit.AuditEventService.AuditEventServiceClient auditAuditEventService,
        ApiAuthorization.AccessPolicyValidationService.AccessPolicyValidationServiceClient authorizationAccessPolicyValidationService,
        CallInvoker invoker,
        ApiLicensing.EntitlementApprovalService.EntitlementApprovalServiceClient licensingEntitlementApprovalService,
        ApiLicensing.LicenseAssignmentApprovalService.LicenseAssignmentApprovalServiceClient licensingLicenseAssignmentApprovalService,
        ApiLicensing.LicenseAssignmentCalculationService.LicenseAssignmentCalculationServiceClient licensingLicenseAssignmentCalculationService,
        ApiLicensing.SoftwareLicenseApprovalService.SoftwareLicenseApprovalServiceClient licensingSoftwareLicenseApprovalService,
        ApiLicensing.SoftwareLicenseService.SoftwareLicenseServiceClient licensingSoftwareLicenseService,
        ApiLicensing.SoftwareLicenseValidationService.SoftwareLicenseValidationServiceClient licensingSoftwareLicenseValidationService,
        ILogger<EntitlementWorkflowEndpoint> logger,
        ApiMessaging.ConversationQueryService.ConversationQueryServiceClient messagingConversationQueryService,
        ApiNotifications.NotificationService.NotificationServiceClient notificationsNotificationService,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiTravel.TripCalculationService.TripCalculationServiceClient travelTripCalculationService)
    {
        _auditAuditEventService = auditAuditEventService;
        _authorizationAccessPolicyValidationService = authorizationAccessPolicyValidationService;
        _invoker = invoker;
        _licensingEntitlementApprovalService = licensingEntitlementApprovalService;
        _licensingLicenseAssignmentApprovalService = licensingLicenseAssignmentApprovalService;
        _licensingLicenseAssignmentCalculationService = licensingLicenseAssignmentCalculationService;
        _licensingSoftwareLicenseApprovalService = licensingSoftwareLicenseApprovalService;
        _licensingSoftwareLicenseService = licensingSoftwareLicenseService;
        _licensingSoftwareLicenseValidationService = licensingSoftwareLicenseValidationService;
        _logger = logger;
        _messagingConversationQueryService = messagingConversationQueryService;
        _notificationsNotificationService = notificationsNotificationService;
        _repository = repository;
        _settings = settings;
        _travelTripCalculationService = travelTripCalculationService;
    }
}
