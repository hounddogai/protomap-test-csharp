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
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiTelematics = Acme.Telematics.V1;

namespace Acme.Workforce.Licensing;

/// <summary>
/// Routes software licenses through approval, with escalation and delegation.
/// </summary>
public sealed partial class SoftwareLicenseApprovalEndpoint : ApiLicensing.SoftwareLicenseApprovalService.SoftwareLicenseApprovalServiceBase
{
    private readonly NotificationBus _bus;
    private readonly ApiDocuments.SignatureNotificationService.SignatureNotificationServiceClient _documentsSignatureNotificationService;
    private readonly ApiIdentity.UserService.UserServiceClient _identityUserService;
    private readonly ApiLearning.CourseValidationService.CourseValidationServiceClient _learningCourseValidationService;
    private readonly ApiLearning.LearningPathCalculationService.LearningPathCalculationServiceClient _learningLearningPathCalculationService;
    private readonly ApiLicensing.EntitlementCalculationService.EntitlementCalculationServiceClient _licensingEntitlementCalculationService;
    private readonly ApiLicensing.LicenseAssignmentLifecycleService.LicenseAssignmentLifecycleServiceClient _licensingLicenseAssignmentLifecycleService;
    private readonly ILogger<SoftwareLicenseApprovalEndpoint> _logger;
    private readonly ApiNotifications.NotificationValidationService.NotificationValidationServiceClient _notificationsNotificationValidationService;
    private readonly IRecordRepository _repository;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ISoftwareLicenseApprovalEndpointSendRecallSoftwareLicense2Port _softwareLicenseApprovalEndpointSendRecallSoftwareLicense2Port;
    private readonly ApiTelematics.TelemetryReadingQueryService.TelemetryReadingQueryServiceClient _telematicsTelemetryReadingQueryService;

    public SoftwareLicenseApprovalEndpoint(
        NotificationBus bus,
        ApiDocuments.SignatureNotificationService.SignatureNotificationServiceClient documentsSignatureNotificationService,
        ApiIdentity.UserService.UserServiceClient identityUserService,
        ApiLearning.CourseValidationService.CourseValidationServiceClient learningCourseValidationService,
        ApiLearning.LearningPathCalculationService.LearningPathCalculationServiceClient learningLearningPathCalculationService,
        ApiLicensing.EntitlementCalculationService.EntitlementCalculationServiceClient licensingEntitlementCalculationService,
        ApiLicensing.LicenseAssignmentLifecycleService.LicenseAssignmentLifecycleServiceClient licensingLicenseAssignmentLifecycleService,
        ILogger<SoftwareLicenseApprovalEndpoint> logger,
        ApiNotifications.NotificationValidationService.NotificationValidationServiceClient notificationsNotificationValidationService,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ISoftwareLicenseApprovalEndpointSendRecallSoftwareLicense2Port softwareLicenseApprovalEndpointSendRecallSoftwareLicense2PortAdapter,
        ApiTelematics.TelemetryReadingQueryService.TelemetryReadingQueryServiceClient telematicsTelemetryReadingQueryService)
    {
        _bus = bus;
        _documentsSignatureNotificationService = documentsSignatureNotificationService;
        _identityUserService = identityUserService;
        _learningCourseValidationService = learningCourseValidationService;
        _learningLearningPathCalculationService = learningLearningPathCalculationService;
        _licensingEntitlementCalculationService = licensingEntitlementCalculationService;
        _licensingLicenseAssignmentLifecycleService = licensingLicenseAssignmentLifecycleService;
        _logger = logger;
        _notificationsNotificationValidationService = notificationsNotificationValidationService;
        _repository = repository;
        _settings = settings;
        _softwareLicenseApprovalEndpointSendRecallSoftwareLicense2Port = softwareLicenseApprovalEndpointSendRecallSoftwareLicense2PortAdapter;
        _telematicsTelemetryReadingQueryService = telematicsTelemetryReadingQueryService;
    }
}
