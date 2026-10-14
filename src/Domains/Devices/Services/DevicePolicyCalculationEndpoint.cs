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
using ApiConcessions = Acme.Concessions.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiNotifications = Acme.Notifications.V1;

namespace Acme.Workforce.Devices;

/// <summary>
/// Computes the figures of a device policy, such as totals, taxes, and estimates.
/// </summary>
public sealed partial class DevicePolicyCalculationEndpoint : ApiDevices.DevicePolicyCalculationService.DevicePolicyCalculationServiceBase
{
    private readonly ApiAuthorization.AccessPolicyService.AccessPolicyServiceClient _authorizationAccessPolicyService;
    private readonly NotificationBus _bus;
    private readonly ApiConcessions.ConcessionStandService.ConcessionStandServiceClient _concessionsConcessionStandService;
    private readonly ApiDevices.ManagedDeviceCalculationService.ManagedDeviceCalculationServiceClient _devicesManagedDeviceCalculationService;
    private readonly ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient _documentsDocumentTemplateSyncService;
    private readonly ApiLicensing.SoftwareLicenseQueryService.SoftwareLicenseQueryServiceClient _licensingSoftwareLicenseQueryService;
    private readonly ApiLicensing.SoftwareLicenseValidationService.SoftwareLicenseValidationServiceClient _licensingSoftwareLicenseValidationService;
    private readonly ILogger<DevicePolicyCalculationEndpoint> _logger;
    private readonly ApiNotifications.NotificationService.NotificationServiceClient _notificationsNotificationService;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiNotifications.BatchGetNotificationsRequest, CallOptions, AsyncUnaryCall<ApiNotifications.BatchGetNotificationsResponse>> _sendCalculateDevicePolicy0;
    private readonly IOptions<MeshOptions> _settings;

    public DevicePolicyCalculationEndpoint(
        ApiAuthorization.AccessPolicyService.AccessPolicyServiceClient authorizationAccessPolicyService,
        NotificationBus bus,
        ApiConcessions.ConcessionStandService.ConcessionStandServiceClient concessionsConcessionStandService,
        ApiDevices.ManagedDeviceCalculationService.ManagedDeviceCalculationServiceClient devicesManagedDeviceCalculationService,
        ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient documentsDocumentTemplateSyncService,
        ApiLicensing.SoftwareLicenseQueryService.SoftwareLicenseQueryServiceClient licensingSoftwareLicenseQueryService,
        ApiLicensing.SoftwareLicenseValidationService.SoftwareLicenseValidationServiceClient licensingSoftwareLicenseValidationService,
        ILogger<DevicePolicyCalculationEndpoint> logger,
        ApiNotifications.NotificationService.NotificationServiceClient notificationsNotificationService,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings)
    {
        _authorizationAccessPolicyService = authorizationAccessPolicyService;
        _bus = bus;
        _concessionsConcessionStandService = concessionsConcessionStandService;
        _devicesManagedDeviceCalculationService = devicesManagedDeviceCalculationService;
        _documentsDocumentTemplateSyncService = documentsDocumentTemplateSyncService;
        _licensingSoftwareLicenseQueryService = licensingSoftwareLicenseQueryService;
        _licensingSoftwareLicenseValidationService = licensingSoftwareLicenseValidationService;
        _logger = logger;
        _notificationsNotificationService = notificationsNotificationService;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _settings = settings;
        _sendCalculateDevicePolicy0 = _notificationsNotificationService.BatchGetNotificationsAsync;
    }
}
