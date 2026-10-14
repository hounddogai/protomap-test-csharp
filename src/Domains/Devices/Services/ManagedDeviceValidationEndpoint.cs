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
using ApiDevices = Acme.Devices.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;

namespace Acme.Workforce.Devices;

/// <summary>
/// Checks managed devices against the rules before anything acts on them.
/// </summary>
public sealed partial class ManagedDeviceValidationEndpoint : ApiDevices.ManagedDeviceValidationService.ManagedDeviceValidationServiceBase
{
    private readonly NotificationBus _bus;
    private readonly ApiDevices.DevicePolicyCalculationService.DevicePolicyCalculationServiceClient _devicesDevicePolicyCalculationService;
    private readonly ApiDevices.ManagedDeviceCalculationService.ManagedDeviceCalculationServiceClient _devicesManagedDeviceCalculationService;
    private readonly ApiFeatureflags.FeatureFlagQueryService.FeatureFlagQueryServiceClient _featureflagsFeatureFlagQueryService;
    private readonly ApiIdentity.UserValidationService.UserValidationServiceClient _identityUserValidationService;
    private readonly ApiLicensing.EntitlementCalculationService.EntitlementCalculationServiceClient _licensingEntitlementCalculationService;
    private readonly ApiLicensing.EntitlementValidationService.EntitlementValidationServiceClient _licensingEntitlementValidationService;
    private readonly ApiLicensing.LicenseAssignmentValidationService.LicenseAssignmentValidationServiceClient _licensingLicenseAssignmentValidationService;
    private readonly ApiLicensing.SoftwareLicenseCalculationService.SoftwareLicenseCalculationServiceClient _licensingSoftwareLicenseCalculationService;
    private readonly ILogger<ManagedDeviceValidationEndpoint> _logger;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly IOptions<MeshOptions> _settings;

    public ManagedDeviceValidationEndpoint(
        NotificationBus bus,
        ApiDevices.DevicePolicyCalculationService.DevicePolicyCalculationServiceClient devicesDevicePolicyCalculationService,
        ApiDevices.ManagedDeviceCalculationService.ManagedDeviceCalculationServiceClient devicesManagedDeviceCalculationService,
        ApiFeatureflags.FeatureFlagQueryService.FeatureFlagQueryServiceClient featureflagsFeatureFlagQueryService,
        ApiIdentity.UserValidationService.UserValidationServiceClient identityUserValidationService,
        ApiLicensing.EntitlementCalculationService.EntitlementCalculationServiceClient licensingEntitlementCalculationService,
        ApiLicensing.EntitlementValidationService.EntitlementValidationServiceClient licensingEntitlementValidationService,
        ApiLicensing.LicenseAssignmentValidationService.LicenseAssignmentValidationServiceClient licensingLicenseAssignmentValidationService,
        ApiLicensing.SoftwareLicenseCalculationService.SoftwareLicenseCalculationServiceClient licensingSoftwareLicenseCalculationService,
        ILogger<ManagedDeviceValidationEndpoint> logger,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings)
    {
        _bus = bus;
        _devicesDevicePolicyCalculationService = devicesDevicePolicyCalculationService;
        _devicesManagedDeviceCalculationService = devicesManagedDeviceCalculationService;
        _featureflagsFeatureFlagQueryService = featureflagsFeatureFlagQueryService;
        _identityUserValidationService = identityUserValidationService;
        _licensingEntitlementCalculationService = licensingEntitlementCalculationService;
        _licensingEntitlementValidationService = licensingEntitlementValidationService;
        _licensingLicenseAssignmentValidationService = licensingLicenseAssignmentValidationService;
        _licensingSoftwareLicenseCalculationService = licensingSoftwareLicenseCalculationService;
        _logger = logger;
        _registry = registry;
        _repository = repository;
        _settings = settings;
    }
}
