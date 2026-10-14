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
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiFleet = Acme.Fleet.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

/// <summary>
/// Routes desks through approval, with escalation and delegation.
/// </summary>
public sealed partial class DeskApprovalEndpoint : ApiWorkplaces.DeskApprovalService.DeskApprovalServiceBase
{
    private readonly ApiCorporateevents.CorporateEventReportingService.CorporateEventReportingServiceClient _corporateeventsCorporateEventReportingService;
    private readonly ApiDocuments.SignatureSyncService.SignatureSyncServiceClient _documentsSignatureSyncService;
    private readonly ApiEmployees.EmployeeService.EmployeeServiceClient _employeesEmployeeService;
    private readonly ApiFeatureflags.FeatureFlagQueryService.FeatureFlagQueryServiceClient _featureflagsFeatureFlagQueryService;
    private readonly ApiFleet.VehicleService.VehicleServiceClient _fleetVehicleService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.IncidentWorkflowService.IncidentWorkflowServiceClient _itsmIncidentWorkflowService;
    private readonly ApiLicensing.LicenseAssignmentApprovalService.LicenseAssignmentApprovalServiceClient _licensingLicenseAssignmentApprovalService;
    private readonly ILogger<DeskApprovalEndpoint> _logger;
    private readonly ApiNotifications.NotificationValidationService.NotificationValidationServiceClient _notificationsNotificationValidationService;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiWorkplaces.BuildingQueryService.BuildingQueryServiceClient _workplacesBuildingQueryService;
    private readonly ApiWorkplaces.DeskCalculationService.DeskCalculationServiceClient _workplacesDeskCalculationService;
    private readonly ApiWorkplaces.DeskLifecycleService.DeskLifecycleServiceClient _workplacesDeskLifecycleService;
    private readonly ApiWorkplaces.RoomBookingReportingService.RoomBookingReportingServiceClient _workplacesRoomBookingReportingService;
    private readonly ApiWorkplaces.VisitorPassCalculationService.VisitorPassCalculationServiceClient _workplacesVisitorPassCalculationService;

    public DeskApprovalEndpoint(
        ApiCorporateevents.CorporateEventReportingService.CorporateEventReportingServiceClient corporateeventsCorporateEventReportingService,
        ApiDocuments.SignatureSyncService.SignatureSyncServiceClient documentsSignatureSyncService,
        ApiEmployees.EmployeeService.EmployeeServiceClient employeesEmployeeService,
        ApiFeatureflags.FeatureFlagQueryService.FeatureFlagQueryServiceClient featureflagsFeatureFlagQueryService,
        ApiFleet.VehicleService.VehicleServiceClient fleetVehicleService,
        CallInvoker invoker,
        ApiItsm.IncidentWorkflowService.IncidentWorkflowServiceClient itsmIncidentWorkflowService,
        ApiLicensing.LicenseAssignmentApprovalService.LicenseAssignmentApprovalServiceClient licensingLicenseAssignmentApprovalService,
        ILogger<DeskApprovalEndpoint> logger,
        ApiNotifications.NotificationValidationService.NotificationValidationServiceClient notificationsNotificationValidationService,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiWorkplaces.BuildingQueryService.BuildingQueryServiceClient workplacesBuildingQueryService,
        ApiWorkplaces.DeskCalculationService.DeskCalculationServiceClient workplacesDeskCalculationService,
        ApiWorkplaces.DeskLifecycleService.DeskLifecycleServiceClient workplacesDeskLifecycleService,
        ApiWorkplaces.RoomBookingReportingService.RoomBookingReportingServiceClient workplacesRoomBookingReportingService,
        ApiWorkplaces.VisitorPassCalculationService.VisitorPassCalculationServiceClient workplacesVisitorPassCalculationService)
    {
        _corporateeventsCorporateEventReportingService = corporateeventsCorporateEventReportingService;
        _documentsSignatureSyncService = documentsSignatureSyncService;
        _employeesEmployeeService = employeesEmployeeService;
        _featureflagsFeatureFlagQueryService = featureflagsFeatureFlagQueryService;
        _fleetVehicleService = fleetVehicleService;
        _invoker = invoker;
        _itsmIncidentWorkflowService = itsmIncidentWorkflowService;
        _licensingLicenseAssignmentApprovalService = licensingLicenseAssignmentApprovalService;
        _logger = logger;
        _notificationsNotificationValidationService = notificationsNotificationValidationService;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _settings = settings;
        _workplacesBuildingQueryService = workplacesBuildingQueryService;
        _workplacesDeskCalculationService = workplacesDeskCalculationService;
        _workplacesDeskLifecycleService = workplacesDeskLifecycleService;
        _workplacesRoomBookingReportingService = workplacesRoomBookingReportingService;
        _workplacesVisitorPassCalculationService = workplacesVisitorPassCalculationService;
    }
}
