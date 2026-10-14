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
using ApiCustomers = Acme.Customers.V1;
using ApiDevices = Acme.Devices.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiInvoicing = Acme.Invoicing.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Devices;

/// <summary>
/// Routes device policies through approval, with escalation and delegation.
/// </summary>
public sealed partial class DevicePolicyApprovalEndpoint : ApiDevices.DevicePolicyApprovalService.DevicePolicyApprovalServiceBase
{
    private readonly ApiAuthorization.AccessPolicyQueryService.AccessPolicyQueryServiceClient _authorizationAccessPolicyQueryService;
    private readonly IDevicePolicyApprovalEndpointSendDelegateDevicePolicyApproval0Port _devicePolicyApprovalEndpointSendDelegateDevicePolicyApproval0Port;
    private readonly ApiDevices.ComplianceScanValidationService.ComplianceScanValidationServiceClient _devicesComplianceScanValidationService;
    private readonly ApiDevices.DevicePolicyLifecycleService.DevicePolicyLifecycleServiceClient _devicesDevicePolicyLifecycleService;
    private readonly ApiDevices.ManagedDeviceService.ManagedDeviceServiceClient _devicesManagedDeviceService;
    private readonly ApiInvoicing.InvoiceBatchService.InvoiceBatchServiceClient _invoicingInvoiceBatchService;
    private readonly ApiItsm.ChangeRequestCalculationService.ChangeRequestCalculationServiceClient _itsmChangeRequestCalculationService;
    private readonly ApiLicensing.EntitlementWorkflowService.EntitlementWorkflowServiceClient _licensingEntitlementWorkflowService;
    private readonly ApiLicensing.SoftwareLicenseApprovalService.SoftwareLicenseApprovalServiceClient _licensingSoftwareLicenseApprovalService;
    private readonly ILogger<DevicePolicyApprovalEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly ApiRecruiting.JobOpeningQueryService.JobOpeningQueryServiceClient _recruitingJobOpeningQueryService;
    private readonly IRecordRepository _repository;

    public DevicePolicyApprovalEndpoint(
        ApiAuthorization.AccessPolicyQueryService.AccessPolicyQueryServiceClient authorizationAccessPolicyQueryService,
        IDevicePolicyApprovalEndpointSendDelegateDevicePolicyApproval0Port devicePolicyApprovalEndpointSendDelegateDevicePolicyApproval0PortAdapter,
        ApiDevices.ComplianceScanValidationService.ComplianceScanValidationServiceClient devicesComplianceScanValidationService,
        ApiDevices.DevicePolicyLifecycleService.DevicePolicyLifecycleServiceClient devicesDevicePolicyLifecycleService,
        ApiDevices.ManagedDeviceService.ManagedDeviceServiceClient devicesManagedDeviceService,
        ApiInvoicing.InvoiceBatchService.InvoiceBatchServiceClient invoicingInvoiceBatchService,
        ApiItsm.ChangeRequestCalculationService.ChangeRequestCalculationServiceClient itsmChangeRequestCalculationService,
        ApiLicensing.EntitlementWorkflowService.EntitlementWorkflowServiceClient licensingEntitlementWorkflowService,
        ApiLicensing.SoftwareLicenseApprovalService.SoftwareLicenseApprovalServiceClient licensingSoftwareLicenseApprovalService,
        ILogger<DevicePolicyApprovalEndpoint> logger,
        RetryPolicy policy,
        ApiRecruiting.JobOpeningQueryService.JobOpeningQueryServiceClient recruitingJobOpeningQueryService,
        IRecordRepository repository)
    {
        _authorizationAccessPolicyQueryService = authorizationAccessPolicyQueryService;
        _devicePolicyApprovalEndpointSendDelegateDevicePolicyApproval0Port = devicePolicyApprovalEndpointSendDelegateDevicePolicyApproval0PortAdapter;
        _devicesComplianceScanValidationService = devicesComplianceScanValidationService;
        _devicesDevicePolicyLifecycleService = devicesDevicePolicyLifecycleService;
        _devicesManagedDeviceService = devicesManagedDeviceService;
        _invoicingInvoiceBatchService = invoicingInvoiceBatchService;
        _itsmChangeRequestCalculationService = itsmChangeRequestCalculationService;
        _licensingEntitlementWorkflowService = licensingEntitlementWorkflowService;
        _licensingSoftwareLicenseApprovalService = licensingSoftwareLicenseApprovalService;
        _logger = logger;
        _policy = policy;
        _recruitingJobOpeningQueryService = recruitingJobOpeningQueryService;
        _repository = repository;
    }
}
