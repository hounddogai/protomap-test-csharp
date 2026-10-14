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
using ApiConcessions = Acme.Concessions.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSearch = Acme.Search.V1;

namespace Acme.Workforce.Helpdesk;

/// <summary>
/// Computes the figures of a ticket, such as totals, taxes, and estimates.
/// </summary>
public sealed partial class TicketCalculationEndpoint : ApiHelpdesk.TicketCalculationService.TicketCalculationServiceBase
{
    private readonly ApiConcessions.ConcessionStandQueryService.ConcessionStandQueryServiceClient _concessionsConcessionStandQueryService;
    private readonly ApiDevices.ComplianceScanCalculationService.ComplianceScanCalculationServiceClient _devicesComplianceScanCalculationService;
    private readonly ApiDevices.DevicePolicyCalculationService.DevicePolicyCalculationServiceClient _devicesDevicePolicyCalculationService;
    private readonly ApiDevices.ManagedDeviceCalculationService.ManagedDeviceCalculationServiceClient _devicesManagedDeviceCalculationService;
    private readonly ApiDevices.ManagedDeviceQueryService.ManagedDeviceQueryServiceClient _devicesManagedDeviceQueryService;
    private readonly ApiDocuments.DocumentFolderNotificationService.DocumentFolderNotificationServiceClient _documentsDocumentFolderNotificationService;
    private readonly ApiFeatureflags.FeatureFlagService.FeatureFlagServiceClient _featureflagsFeatureFlagService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.ChangeRequestCalculationService.ChangeRequestCalculationServiceClient _itsmChangeRequestCalculationService;
    private readonly ApiItsm.ServiceAssetValidationService.ServiceAssetValidationServiceClient _itsmServiceAssetValidationService;
    private readonly ILogger<TicketCalculationEndpoint> _logger;
    private readonly ApiNotifications.NotificationValidationService.NotificationValidationServiceClient _notificationsNotificationValidationService;
    private readonly IRecordRepository _repository;
    private readonly ApiSearch.SearchIndexService.SearchIndexServiceClient _searchSearchIndexService;
    private readonly IOptions<MeshOptions> _settings;

    public TicketCalculationEndpoint(
        ApiConcessions.ConcessionStandQueryService.ConcessionStandQueryServiceClient concessionsConcessionStandQueryService,
        ApiDevices.ComplianceScanCalculationService.ComplianceScanCalculationServiceClient devicesComplianceScanCalculationService,
        ApiDevices.DevicePolicyCalculationService.DevicePolicyCalculationServiceClient devicesDevicePolicyCalculationService,
        ApiDevices.ManagedDeviceCalculationService.ManagedDeviceCalculationServiceClient devicesManagedDeviceCalculationService,
        ApiDevices.ManagedDeviceQueryService.ManagedDeviceQueryServiceClient devicesManagedDeviceQueryService,
        ApiDocuments.DocumentFolderNotificationService.DocumentFolderNotificationServiceClient documentsDocumentFolderNotificationService,
        ApiFeatureflags.FeatureFlagService.FeatureFlagServiceClient featureflagsFeatureFlagService,
        CallInvoker invoker,
        ApiItsm.ChangeRequestCalculationService.ChangeRequestCalculationServiceClient itsmChangeRequestCalculationService,
        ApiItsm.ServiceAssetValidationService.ServiceAssetValidationServiceClient itsmServiceAssetValidationService,
        ILogger<TicketCalculationEndpoint> logger,
        ApiNotifications.NotificationValidationService.NotificationValidationServiceClient notificationsNotificationValidationService,
        IRecordRepository repository,
        ApiSearch.SearchIndexService.SearchIndexServiceClient searchSearchIndexService,
        IOptions<MeshOptions> settings)
    {
        _concessionsConcessionStandQueryService = concessionsConcessionStandQueryService;
        _devicesComplianceScanCalculationService = devicesComplianceScanCalculationService;
        _devicesDevicePolicyCalculationService = devicesDevicePolicyCalculationService;
        _devicesManagedDeviceCalculationService = devicesManagedDeviceCalculationService;
        _devicesManagedDeviceQueryService = devicesManagedDeviceQueryService;
        _documentsDocumentFolderNotificationService = documentsDocumentFolderNotificationService;
        _featureflagsFeatureFlagService = featureflagsFeatureFlagService;
        _invoker = invoker;
        _itsmChangeRequestCalculationService = itsmChangeRequestCalculationService;
        _itsmServiceAssetValidationService = itsmServiceAssetValidationService;
        _logger = logger;
        _notificationsNotificationValidationService = notificationsNotificationValidationService;
        _repository = repository;
        _searchSearchIndexService = searchSearchIndexService;
        _settings = settings;
    }
}
