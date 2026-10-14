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
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Itsm;

/// <summary>
/// Moves a service asset between the states of its lifecycle.
/// </summary>
public sealed partial class ServiceAssetLifecycleEndpoint : ApiItsm.ServiceAssetLifecycleService.ServiceAssetLifecycleServiceBase
{
    private readonly ApiDevices.DevicePolicyWorkflowService.DevicePolicyWorkflowServiceClient _devicesDevicePolicyWorkflowService;
    private readonly ApiDocuments.DocumentService.DocumentServiceClient _documentsDocumentService;
    private readonly ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient _employeesEmployeeQueryService;
    private readonly ApiEmployees.EmployeeService.EmployeeServiceClient _employeesEmployeeService;
    private readonly ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient _employeesOrgUnitQueryService;
    private readonly ApiItsm.ChangeRequestReportingService.ChangeRequestReportingServiceClient _itsmChangeRequestReportingService;
    private readonly ApiItsm.ProblemSyncService.ProblemSyncServiceClient _itsmProblemSyncService;
    private readonly ApiItsm.ServiceAssetCalculationService.ServiceAssetCalculationServiceClient _itsmServiceAssetCalculationService;
    private readonly ApiItsm.ServiceAssetReportingService.ServiceAssetReportingServiceClient _itsmServiceAssetReportingService;
    private readonly ApiItsm.ServiceAssetValidationService.ServiceAssetValidationServiceClient _itsmServiceAssetValidationService;
    private readonly ILogger<ServiceAssetLifecycleEndpoint> _logger;
    private readonly ApiNotifications.NotificationService.NotificationServiceClient _notificationsNotificationService;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiNotifications.CloneNotificationRequest, CallOptions, AsyncUnaryCall<ApiNotifications.CloneNotificationResponse>> _sendRenewServiceAsset0;
    private readonly IServiceAssetLifecycleEndpointSendTerminateServiceAsset0Port _serviceAssetLifecycleEndpointSendTerminateServiceAsset0Port;
    private readonly ApiVenues.VenueValidationService.VenueValidationServiceClient _venuesVenueValidationService;

    public ServiceAssetLifecycleEndpoint(
        ApiDevices.DevicePolicyWorkflowService.DevicePolicyWorkflowServiceClient devicesDevicePolicyWorkflowService,
        ApiDocuments.DocumentService.DocumentServiceClient documentsDocumentService,
        ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient employeesEmployeeQueryService,
        ApiEmployees.EmployeeService.EmployeeServiceClient employeesEmployeeService,
        ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient employeesOrgUnitQueryService,
        ApiItsm.ChangeRequestReportingService.ChangeRequestReportingServiceClient itsmChangeRequestReportingService,
        ApiItsm.ProblemSyncService.ProblemSyncServiceClient itsmProblemSyncService,
        ApiItsm.ServiceAssetCalculationService.ServiceAssetCalculationServiceClient itsmServiceAssetCalculationService,
        ApiItsm.ServiceAssetReportingService.ServiceAssetReportingServiceClient itsmServiceAssetReportingService,
        ApiItsm.ServiceAssetValidationService.ServiceAssetValidationServiceClient itsmServiceAssetValidationService,
        ILogger<ServiceAssetLifecycleEndpoint> logger,
        ApiNotifications.NotificationService.NotificationServiceClient notificationsNotificationService,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository,
        IServiceAssetLifecycleEndpointSendTerminateServiceAsset0Port serviceAssetLifecycleEndpointSendTerminateServiceAsset0PortAdapter,
        ApiVenues.VenueValidationService.VenueValidationServiceClient venuesVenueValidationService)
    {
        _devicesDevicePolicyWorkflowService = devicesDevicePolicyWorkflowService;
        _documentsDocumentService = documentsDocumentService;
        _employeesEmployeeQueryService = employeesEmployeeQueryService;
        _employeesEmployeeService = employeesEmployeeService;
        _employeesOrgUnitQueryService = employeesOrgUnitQueryService;
        _itsmChangeRequestReportingService = itsmChangeRequestReportingService;
        _itsmProblemSyncService = itsmProblemSyncService;
        _itsmServiceAssetCalculationService = itsmServiceAssetCalculationService;
        _itsmServiceAssetReportingService = itsmServiceAssetReportingService;
        _itsmServiceAssetValidationService = itsmServiceAssetValidationService;
        _logger = logger;
        _notificationsNotificationService = notificationsNotificationService;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _serviceAssetLifecycleEndpointSendTerminateServiceAsset0Port = serviceAssetLifecycleEndpointSendTerminateServiceAsset0PortAdapter;
        _venuesVenueValidationService = venuesVenueValidationService;
        _sendRenewServiceAsset0 = _notificationsNotificationService.CloneNotificationAsync;
    }
}
