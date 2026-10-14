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
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiTours = Acme.Tours.V1;

namespace Acme.Workforce.Licensing;

/// <summary>
/// Answers read-only questions about entitlements, for screens and reports.
/// </summary>
public sealed partial class EntitlementQueryEndpoint : ApiLicensing.EntitlementQueryService.EntitlementQueryServiceBase
{
    private readonly ApiDocuments.SignatureValidationService.SignatureValidationServiceClient _documentsSignatureValidationService;
    private readonly ApiFeatureflags.FeatureFlagValidationService.FeatureFlagValidationServiceClient _featureflagsFeatureFlagValidationService;
    private readonly CallInvoker _invoker;
    private readonly ApiLicensing.EntitlementCalculationService.EntitlementCalculationServiceClient _licensingEntitlementCalculationService;
    private readonly ApiLicensing.LicenseAssignmentCalculationService.LicenseAssignmentCalculationServiceClient _licensingLicenseAssignmentCalculationService;
    private readonly ApiLicensing.SoftwareLicenseCalculationService.SoftwareLicenseCalculationServiceClient _licensingSoftwareLicenseCalculationService;
    private readonly ILogger<EntitlementQueryEndpoint> _logger;
    private readonly ApiNotifications.NotificationService.NotificationServiceClient _notificationsNotificationService;
    private readonly RetryPolicy _policy;
    private readonly IRecordRepository _repository;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTours.TourQueryService.TourQueryServiceClient _toursTourQueryService;

    public EntitlementQueryEndpoint(
        ApiDocuments.SignatureValidationService.SignatureValidationServiceClient documentsSignatureValidationService,
        ApiFeatureflags.FeatureFlagValidationService.FeatureFlagValidationServiceClient featureflagsFeatureFlagValidationService,
        CallInvoker invoker,
        ApiLicensing.EntitlementCalculationService.EntitlementCalculationServiceClient licensingEntitlementCalculationService,
        ApiLicensing.LicenseAssignmentCalculationService.LicenseAssignmentCalculationServiceClient licensingLicenseAssignmentCalculationService,
        ApiLicensing.SoftwareLicenseCalculationService.SoftwareLicenseCalculationServiceClient licensingSoftwareLicenseCalculationService,
        ILogger<EntitlementQueryEndpoint> logger,
        ApiNotifications.NotificationService.NotificationServiceClient notificationsNotificationService,
        RetryPolicy policy,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiTours.TourQueryService.TourQueryServiceClient toursTourQueryService)
    {
        _documentsSignatureValidationService = documentsSignatureValidationService;
        _featureflagsFeatureFlagValidationService = featureflagsFeatureFlagValidationService;
        _invoker = invoker;
        _licensingEntitlementCalculationService = licensingEntitlementCalculationService;
        _licensingLicenseAssignmentCalculationService = licensingLicenseAssignmentCalculationService;
        _licensingSoftwareLicenseCalculationService = licensingSoftwareLicenseCalculationService;
        _logger = logger;
        _notificationsNotificationService = notificationsNotificationService;
        _policy = policy;
        _repository = repository;
        _settings = settings;
        _toursTourQueryService = toursTourQueryService;
    }
}
