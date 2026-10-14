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
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiPersonalization = Acme.Personalization.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiSearch = Acme.Search.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Recruiting;

/// <summary>
/// Computes the figures of a job opening, such as totals, taxes, and estimates.
/// </summary>
public sealed partial class JobOpeningCalculationEndpoint : ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceBase
{
    private readonly ApiDevices.DevicePolicyValidationService.DevicePolicyValidationServiceClient _devicesDevicePolicyValidationService;
    private readonly ApiDevices.ManagedDeviceValidationService.ManagedDeviceValidationServiceClient _devicesManagedDeviceValidationService;
    private readonly ApiDocuments.DocumentTemplateAdminService.DocumentTemplateAdminServiceClient _documentsDocumentTemplateAdminService;
    private readonly ApiDocuments.DocumentTemplateNotificationService.DocumentTemplateNotificationServiceClient _documentsDocumentTemplateNotificationService;
    private readonly ApiFeatureflags.FeatureFlagQueryService.FeatureFlagQueryServiceClient _featureflagsFeatureFlagQueryService;
    private readonly ApiHelpdesk.SlaPolicyValidationService.SlaPolicyValidationServiceClient _helpdeskSlaPolicyValidationService;
    private readonly ApiHelpdesk.TicketQueryService.TicketQueryServiceClient _helpdeskTicketQueryService;
    private readonly ILogger<JobOpeningCalculationEndpoint> _logger;
    private readonly ApiNotifications.NotificationService.NotificationServiceClient _notificationsNotificationService;
    private readonly ApiPersonalization.ProfileQueryService.ProfileQueryServiceClient _personalizationProfileQueryService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiScheduling.RosterQueryService.RosterQueryServiceClient _schedulingRosterQueryService;
    private readonly ApiSearch.SearchIndexQueryService.SearchIndexQueryServiceClient _searchSearchIndexQueryService;
    private readonly Func<ApiPersonalization.BrowseProfilesRequest, CallOptions, AsyncUnaryCall<ApiPersonalization.BrowseProfilesResponse>> _sendCalculateJobOpening1;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTimekeeping.TimesheetQueryService.TimesheetQueryServiceClient _timekeepingTimesheetQueryService;
    private readonly ApiWorkplaces.BuildingCalculationService.BuildingCalculationServiceClient _workplacesBuildingCalculationService;

    public JobOpeningCalculationEndpoint(
        ApiDevices.DevicePolicyValidationService.DevicePolicyValidationServiceClient devicesDevicePolicyValidationService,
        ApiDevices.ManagedDeviceValidationService.ManagedDeviceValidationServiceClient devicesManagedDeviceValidationService,
        ApiDocuments.DocumentTemplateAdminService.DocumentTemplateAdminServiceClient documentsDocumentTemplateAdminService,
        ApiDocuments.DocumentTemplateNotificationService.DocumentTemplateNotificationServiceClient documentsDocumentTemplateNotificationService,
        ApiFeatureflags.FeatureFlagQueryService.FeatureFlagQueryServiceClient featureflagsFeatureFlagQueryService,
        ApiHelpdesk.SlaPolicyValidationService.SlaPolicyValidationServiceClient helpdeskSlaPolicyValidationService,
        ApiHelpdesk.TicketQueryService.TicketQueryServiceClient helpdeskTicketQueryService,
        ILogger<JobOpeningCalculationEndpoint> logger,
        ApiNotifications.NotificationService.NotificationServiceClient notificationsNotificationService,
        ApiPersonalization.ProfileQueryService.ProfileQueryServiceClient personalizationProfileQueryService,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiScheduling.RosterQueryService.RosterQueryServiceClient schedulingRosterQueryService,
        ApiSearch.SearchIndexQueryService.SearchIndexQueryServiceClient searchSearchIndexQueryService,
        IOptions<MeshOptions> settings,
        ApiTimekeeping.TimesheetQueryService.TimesheetQueryServiceClient timekeepingTimesheetQueryService,
        ApiWorkplaces.BuildingCalculationService.BuildingCalculationServiceClient workplacesBuildingCalculationService)
    {
        _devicesDevicePolicyValidationService = devicesDevicePolicyValidationService;
        _devicesManagedDeviceValidationService = devicesManagedDeviceValidationService;
        _documentsDocumentTemplateAdminService = documentsDocumentTemplateAdminService;
        _documentsDocumentTemplateNotificationService = documentsDocumentTemplateNotificationService;
        _featureflagsFeatureFlagQueryService = featureflagsFeatureFlagQueryService;
        _helpdeskSlaPolicyValidationService = helpdeskSlaPolicyValidationService;
        _helpdeskTicketQueryService = helpdeskTicketQueryService;
        _logger = logger;
        _notificationsNotificationService = notificationsNotificationService;
        _personalizationProfileQueryService = personalizationProfileQueryService;
        _registry = registry;
        _repository = repository;
        _schedulingRosterQueryService = schedulingRosterQueryService;
        _searchSearchIndexQueryService = searchSearchIndexQueryService;
        _settings = settings;
        _timekeepingTimesheetQueryService = timekeepingTimesheetQueryService;
        _workplacesBuildingCalculationService = workplacesBuildingCalculationService;
        _sendCalculateJobOpening1 = _personalizationProfileQueryService.BrowseProfilesAsync;
    }
}
