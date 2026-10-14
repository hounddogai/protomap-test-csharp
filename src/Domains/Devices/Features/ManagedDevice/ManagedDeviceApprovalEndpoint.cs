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
using ApiAppointments = Acme.Appointments.V1;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSearch = Acme.Search.V1;

namespace Acme.Workforce.Devices;

/// <summary>
/// Routes managed devices through approval, with escalation and delegation.
/// </summary>
public sealed partial class ManagedDeviceApprovalEndpoint : ApiDevices.ManagedDeviceApprovalService.ManagedDeviceApprovalServiceBase
{
    private readonly ApiAppointments.AppointmentService.AppointmentServiceClient _appointmentsAppointmentService;
    private readonly NotificationBus _bus;
    private readonly ApiDevices.ComplianceScanCalculationService.ComplianceScanCalculationServiceClient _devicesComplianceScanCalculationService;
    private readonly ApiDevices.DevicePolicyCalculationService.DevicePolicyCalculationServiceClient _devicesDevicePolicyCalculationService;
    private readonly ApiDevices.DevicePolicyQueryService.DevicePolicyQueryServiceClient _devicesDevicePolicyQueryService;
    private readonly ApiDevices.DevicePolicyValidationService.DevicePolicyValidationServiceClient _devicesDevicePolicyValidationService;
    private readonly ApiDevices.ManagedDeviceCalculationService.ManagedDeviceCalculationServiceClient _devicesManagedDeviceCalculationService;
    private readonly ApiDevices.ManagedDeviceQueryService.ManagedDeviceQueryServiceClient _devicesManagedDeviceQueryService;
    private readonly ApiDevices.ManagedDeviceService.ManagedDeviceServiceClient _devicesManagedDeviceService;
    private readonly ApiDocuments.DocumentFolderNotificationService.DocumentFolderNotificationServiceClient _documentsDocumentFolderNotificationService;
    private readonly CallInvoker _invoker;
    private readonly ApiLicensing.LicenseAssignmentApprovalService.LicenseAssignmentApprovalServiceClient _licensingLicenseAssignmentApprovalService;
    private readonly ApiLicensing.SoftwareLicenseApprovalService.SoftwareLicenseApprovalServiceClient _licensingSoftwareLicenseApprovalService;
    private readonly ILogger<ManagedDeviceApprovalEndpoint> _logger;
    private readonly IManagedDeviceApprovalEndpointSendRejectManagedDevice2Port _managedDeviceApprovalEndpointSendRejectManagedDevice2Port;
    private readonly ApiRecruiting.InterviewValidationService.InterviewValidationServiceClient _recruitingInterviewValidationService;
    private readonly ApiRecruiting.JobOpeningQueryService.JobOpeningQueryServiceClient _recruitingJobOpeningQueryService;
    private readonly ApiRecruiting.JobOpeningService.JobOpeningServiceClient _recruitingJobOpeningService;
    private readonly ApiRecruiting.JobOpeningValidationService.JobOpeningValidationServiceClient _recruitingJobOpeningValidationService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiSearch.SearchIndexValidationService.SearchIndexValidationServiceClient _searchSearchIndexValidationService;
    private readonly Func<ApiLicensing.SubmitLicenseAssignmentRequest, CallOptions, AsyncUnaryCall<ApiLicensing.SubmitLicenseAssignmentResponse>> _sendEscalateManagedDevice1;

    public ManagedDeviceApprovalEndpoint(
        ApiAppointments.AppointmentService.AppointmentServiceClient appointmentsAppointmentService,
        NotificationBus bus,
        ApiDevices.ComplianceScanCalculationService.ComplianceScanCalculationServiceClient devicesComplianceScanCalculationService,
        ApiDevices.DevicePolicyCalculationService.DevicePolicyCalculationServiceClient devicesDevicePolicyCalculationService,
        ApiDevices.DevicePolicyQueryService.DevicePolicyQueryServiceClient devicesDevicePolicyQueryService,
        ApiDevices.DevicePolicyValidationService.DevicePolicyValidationServiceClient devicesDevicePolicyValidationService,
        ApiDevices.ManagedDeviceCalculationService.ManagedDeviceCalculationServiceClient devicesManagedDeviceCalculationService,
        ApiDevices.ManagedDeviceQueryService.ManagedDeviceQueryServiceClient devicesManagedDeviceQueryService,
        ApiDevices.ManagedDeviceService.ManagedDeviceServiceClient devicesManagedDeviceService,
        ApiDocuments.DocumentFolderNotificationService.DocumentFolderNotificationServiceClient documentsDocumentFolderNotificationService,
        CallInvoker invoker,
        ApiLicensing.LicenseAssignmentApprovalService.LicenseAssignmentApprovalServiceClient licensingLicenseAssignmentApprovalService,
        ApiLicensing.SoftwareLicenseApprovalService.SoftwareLicenseApprovalServiceClient licensingSoftwareLicenseApprovalService,
        ILogger<ManagedDeviceApprovalEndpoint> logger,
        IManagedDeviceApprovalEndpointSendRejectManagedDevice2Port managedDeviceApprovalEndpointSendRejectManagedDevice2PortAdapter,
        ApiRecruiting.InterviewValidationService.InterviewValidationServiceClient recruitingInterviewValidationService,
        ApiRecruiting.JobOpeningQueryService.JobOpeningQueryServiceClient recruitingJobOpeningQueryService,
        ApiRecruiting.JobOpeningService.JobOpeningServiceClient recruitingJobOpeningService,
        ApiRecruiting.JobOpeningValidationService.JobOpeningValidationServiceClient recruitingJobOpeningValidationService,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiSearch.SearchIndexValidationService.SearchIndexValidationServiceClient searchSearchIndexValidationService)
    {
        _appointmentsAppointmentService = appointmentsAppointmentService;
        _bus = bus;
        _devicesComplianceScanCalculationService = devicesComplianceScanCalculationService;
        _devicesDevicePolicyCalculationService = devicesDevicePolicyCalculationService;
        _devicesDevicePolicyQueryService = devicesDevicePolicyQueryService;
        _devicesDevicePolicyValidationService = devicesDevicePolicyValidationService;
        _devicesManagedDeviceCalculationService = devicesManagedDeviceCalculationService;
        _devicesManagedDeviceQueryService = devicesManagedDeviceQueryService;
        _devicesManagedDeviceService = devicesManagedDeviceService;
        _documentsDocumentFolderNotificationService = documentsDocumentFolderNotificationService;
        _invoker = invoker;
        _licensingLicenseAssignmentApprovalService = licensingLicenseAssignmentApprovalService;
        _licensingSoftwareLicenseApprovalService = licensingSoftwareLicenseApprovalService;
        _logger = logger;
        _managedDeviceApprovalEndpointSendRejectManagedDevice2Port = managedDeviceApprovalEndpointSendRejectManagedDevice2PortAdapter;
        _recruitingInterviewValidationService = recruitingInterviewValidationService;
        _recruitingJobOpeningQueryService = recruitingJobOpeningQueryService;
        _recruitingJobOpeningService = recruitingJobOpeningService;
        _recruitingJobOpeningValidationService = recruitingJobOpeningValidationService;
        _registry = registry;
        _repository = repository;
        _searchSearchIndexValidationService = searchSearchIndexValidationService;
        _sendEscalateManagedDevice1 = _licensingLicenseAssignmentApprovalService.SubmitLicenseAssignmentAsync;
    }
}
