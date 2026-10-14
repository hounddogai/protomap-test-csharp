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
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;

namespace Acme.Workforce.Devices;

/// <summary>
/// Checks device policies against the rules before anything acts on them.
/// </summary>
public sealed partial class DevicePolicyValidationEndpoint : ApiDevices.DevicePolicyValidationService.DevicePolicyValidationServiceBase
{
    private readonly ApiAuthorization.AccessPolicyQueryService.AccessPolicyQueryServiceClient _authorizationAccessPolicyQueryService;
    private readonly IDevicePolicyValidationEndpointSendValidateDevicePolicy0Port _devicePolicyValidationEndpointSendValidateDevicePolicy0Port;
    private readonly ApiDevices.DevicePolicyCalculationService.DevicePolicyCalculationServiceClient _devicesDevicePolicyCalculationService;
    private readonly ApiDevices.ManagedDeviceCalculationService.ManagedDeviceCalculationServiceClient _devicesManagedDeviceCalculationService;
    private readonly ApiDocuments.DocumentFolderExportService.DocumentFolderExportServiceClient _documentsDocumentFolderExportService;
    private readonly ApiLicensing.EntitlementCalculationService.EntitlementCalculationServiceClient _licensingEntitlementCalculationService;
    private readonly ApiLicensing.EntitlementValidationService.EntitlementValidationServiceClient _licensingEntitlementValidationService;
    private readonly ApiLicensing.SoftwareLicenseValidationService.SoftwareLicenseValidationServiceClient _licensingSoftwareLicenseValidationService;
    private readonly ILogger<DevicePolicyValidationEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiDevices.EstimateManagedDeviceRequest, CallOptions, AsyncUnaryCall<ApiDevices.EstimateManagedDeviceResponse>> _sendLintDevicePolicies1;

    public DevicePolicyValidationEndpoint(
        ApiAuthorization.AccessPolicyQueryService.AccessPolicyQueryServiceClient authorizationAccessPolicyQueryService,
        IDevicePolicyValidationEndpointSendValidateDevicePolicy0Port devicePolicyValidationEndpointSendValidateDevicePolicy0PortAdapter,
        ApiDevices.DevicePolicyCalculationService.DevicePolicyCalculationServiceClient devicesDevicePolicyCalculationService,
        ApiDevices.ManagedDeviceCalculationService.ManagedDeviceCalculationServiceClient devicesManagedDeviceCalculationService,
        ApiDocuments.DocumentFolderExportService.DocumentFolderExportServiceClient documentsDocumentFolderExportService,
        ApiLicensing.EntitlementCalculationService.EntitlementCalculationServiceClient licensingEntitlementCalculationService,
        ApiLicensing.EntitlementValidationService.EntitlementValidationServiceClient licensingEntitlementValidationService,
        ApiLicensing.SoftwareLicenseValidationService.SoftwareLicenseValidationServiceClient licensingSoftwareLicenseValidationService,
        ILogger<DevicePolicyValidationEndpoint> logger,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository)
    {
        _authorizationAccessPolicyQueryService = authorizationAccessPolicyQueryService;
        _devicePolicyValidationEndpointSendValidateDevicePolicy0Port = devicePolicyValidationEndpointSendValidateDevicePolicy0PortAdapter;
        _devicesDevicePolicyCalculationService = devicesDevicePolicyCalculationService;
        _devicesManagedDeviceCalculationService = devicesManagedDeviceCalculationService;
        _documentsDocumentFolderExportService = documentsDocumentFolderExportService;
        _licensingEntitlementCalculationService = licensingEntitlementCalculationService;
        _licensingEntitlementValidationService = licensingEntitlementValidationService;
        _licensingSoftwareLicenseValidationService = licensingSoftwareLicenseValidationService;
        _logger = logger;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _sendLintDevicePolicies1 = _devicesManagedDeviceCalculationService.EstimateManagedDeviceAsync;
    }
}
