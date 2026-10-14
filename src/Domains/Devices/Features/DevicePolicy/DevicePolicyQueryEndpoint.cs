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
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiRatelimiting = Acme.Ratelimiting.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSearch = Acme.Search.V1;
using ApiSeating = Acme.Seating.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Devices;

/// <summary>
/// Answers read-only questions about device policies, for screens and reports.
/// </summary>
public sealed partial class DevicePolicyQueryEndpoint : ApiDevices.DevicePolicyQueryService.DevicePolicyQueryServiceBase
{
    private readonly NotificationBus _bus;
    private readonly IDevicePolicyQueryEndpointSendExportDevicePolicies2Port _devicePolicyQueryEndpointSendExportDevicePolicies2Port;
    private readonly ApiDevices.ComplianceScanValidationService.ComplianceScanValidationServiceClient _devicesComplianceScanValidationService;
    private readonly ApiDevices.DevicePolicyCalculationService.DevicePolicyCalculationServiceClient _devicesDevicePolicyCalculationService;
    private readonly ApiDevices.ManagedDeviceQueryService.ManagedDeviceQueryServiceClient _devicesManagedDeviceQueryService;
    private readonly ApiDocuments.DocumentValidationService.DocumentValidationServiceClient _documentsDocumentValidationService;
    private readonly ApiLicensing.SoftwareLicenseQueryService.SoftwareLicenseQueryServiceClient _licensingSoftwareLicenseQueryService;
    private readonly ILogger<DevicePolicyQueryEndpoint> _logger;
    private readonly ApiRatelimiting.RateLimitCalculationService.RateLimitCalculationServiceClient _ratelimitingRateLimitCalculationService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiSearch.SearchIndexValidationService.SearchIndexValidationServiceClient _searchSearchIndexValidationService;
    private readonly ApiSeating.SeatMapQueryService.SeatMapQueryServiceClient _seatingSeatMapQueryService;
    private readonly ApiVenues.VenueValidationService.VenueValidationServiceClient _venuesVenueValidationService;

    public DevicePolicyQueryEndpoint(
        NotificationBus bus,
        IDevicePolicyQueryEndpointSendExportDevicePolicies2Port devicePolicyQueryEndpointSendExportDevicePolicies2PortAdapter,
        ApiDevices.ComplianceScanValidationService.ComplianceScanValidationServiceClient devicesComplianceScanValidationService,
        ApiDevices.DevicePolicyCalculationService.DevicePolicyCalculationServiceClient devicesDevicePolicyCalculationService,
        ApiDevices.ManagedDeviceQueryService.ManagedDeviceQueryServiceClient devicesManagedDeviceQueryService,
        ApiDocuments.DocumentValidationService.DocumentValidationServiceClient documentsDocumentValidationService,
        ApiLicensing.SoftwareLicenseQueryService.SoftwareLicenseQueryServiceClient licensingSoftwareLicenseQueryService,
        ILogger<DevicePolicyQueryEndpoint> logger,
        ApiRatelimiting.RateLimitCalculationService.RateLimitCalculationServiceClient ratelimitingRateLimitCalculationService,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiSearch.SearchIndexValidationService.SearchIndexValidationServiceClient searchSearchIndexValidationService,
        ApiSeating.SeatMapQueryService.SeatMapQueryServiceClient seatingSeatMapQueryService,
        ApiVenues.VenueValidationService.VenueValidationServiceClient venuesVenueValidationService)
    {
        _bus = bus;
        _devicePolicyQueryEndpointSendExportDevicePolicies2Port = devicePolicyQueryEndpointSendExportDevicePolicies2PortAdapter;
        _devicesComplianceScanValidationService = devicesComplianceScanValidationService;
        _devicesDevicePolicyCalculationService = devicesDevicePolicyCalculationService;
        _devicesManagedDeviceQueryService = devicesManagedDeviceQueryService;
        _documentsDocumentValidationService = documentsDocumentValidationService;
        _licensingSoftwareLicenseQueryService = licensingSoftwareLicenseQueryService;
        _logger = logger;
        _ratelimitingRateLimitCalculationService = ratelimitingRateLimitCalculationService;
        _registry = registry;
        _repository = repository;
        _searchSearchIndexValidationService = searchSearchIndexValidationService;
        _seatingSeatMapQueryService = seatingSeatMapQueryService;
        _venuesVenueValidationService = venuesVenueValidationService;
    }
}
