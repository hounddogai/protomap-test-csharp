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
using ApiAuthorization = Acme.Authorization.V1;
using ApiCareplans = Acme.Careplans.V1;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiForecasting = Acme.Forecasting.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPatients = Acme.Patients.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Devices;

/// <summary>
/// Checks compliance scans against the rules before anything acts on them.
/// </summary>
public sealed partial class ComplianceScanValidationEndpoint : ApiDevices.ComplianceScanValidationService.ComplianceScanValidationServiceBase
{
    private readonly ApiAuthorization.AccessPolicyQueryService.AccessPolicyQueryServiceClient _authorizationAccessPolicyQueryService;
    private readonly ApiCareplans.CarePlanCalculationService.CarePlanCalculationServiceClient _careplansCarePlanCalculationService;
    private readonly IComplianceScanValidationEndpointSendValidateComplianceScan1Port _complianceScanValidationEndpointSendValidateComplianceScan1Port;
    private readonly ApiDevices.ComplianceScanCalculationService.ComplianceScanCalculationServiceClient _devicesComplianceScanCalculationService;
    private readonly ApiDevices.DevicePolicyValidationService.DevicePolicyValidationServiceClient _devicesDevicePolicyValidationService;
    private readonly ApiDevices.ManagedDeviceCalculationService.ManagedDeviceCalculationServiceClient _devicesManagedDeviceCalculationService;
    private readonly ApiDocuments.DocumentService.DocumentServiceClient _documentsDocumentService;
    private readonly ApiForecasting.DemandForecastService.DemandForecastServiceClient _forecastingDemandForecastService;
    private readonly CallInvoker _invoker;
    private readonly ApiLicensing.EntitlementQueryService.EntitlementQueryServiceClient _licensingEntitlementQueryService;
    private readonly ApiLicensing.LicenseAssignmentValidationService.LicenseAssignmentValidationServiceClient _licensingLicenseAssignmentValidationService;
    private readonly ApiLicensing.SoftwareLicenseQueryService.SoftwareLicenseQueryServiceClient _licensingSoftwareLicenseQueryService;
    private readonly ILogger<ComplianceScanValidationEndpoint> _logger;
    private readonly ApiPatients.PatientCalculationService.PatientCalculationServiceClient _patientsPatientCalculationService;
    private readonly IRecordRepository _repository;
    private readonly IOptions<MeshOptions> _settings;

    public ComplianceScanValidationEndpoint(
        ApiAuthorization.AccessPolicyQueryService.AccessPolicyQueryServiceClient authorizationAccessPolicyQueryService,
        ApiCareplans.CarePlanCalculationService.CarePlanCalculationServiceClient careplansCarePlanCalculationService,
        IComplianceScanValidationEndpointSendValidateComplianceScan1Port complianceScanValidationEndpointSendValidateComplianceScan1PortAdapter,
        ApiDevices.ComplianceScanCalculationService.ComplianceScanCalculationServiceClient devicesComplianceScanCalculationService,
        ApiDevices.DevicePolicyValidationService.DevicePolicyValidationServiceClient devicesDevicePolicyValidationService,
        ApiDevices.ManagedDeviceCalculationService.ManagedDeviceCalculationServiceClient devicesManagedDeviceCalculationService,
        ApiDocuments.DocumentService.DocumentServiceClient documentsDocumentService,
        ApiForecasting.DemandForecastService.DemandForecastServiceClient forecastingDemandForecastService,
        CallInvoker invoker,
        ApiLicensing.EntitlementQueryService.EntitlementQueryServiceClient licensingEntitlementQueryService,
        ApiLicensing.LicenseAssignmentValidationService.LicenseAssignmentValidationServiceClient licensingLicenseAssignmentValidationService,
        ApiLicensing.SoftwareLicenseQueryService.SoftwareLicenseQueryServiceClient licensingSoftwareLicenseQueryService,
        ILogger<ComplianceScanValidationEndpoint> logger,
        ApiPatients.PatientCalculationService.PatientCalculationServiceClient patientsPatientCalculationService,
        IRecordRepository repository,
        IOptions<MeshOptions> settings)
    {
        _authorizationAccessPolicyQueryService = authorizationAccessPolicyQueryService;
        _careplansCarePlanCalculationService = careplansCarePlanCalculationService;
        _complianceScanValidationEndpointSendValidateComplianceScan1Port = complianceScanValidationEndpointSendValidateComplianceScan1PortAdapter;
        _devicesComplianceScanCalculationService = devicesComplianceScanCalculationService;
        _devicesDevicePolicyValidationService = devicesDevicePolicyValidationService;
        _devicesManagedDeviceCalculationService = devicesManagedDeviceCalculationService;
        _documentsDocumentService = documentsDocumentService;
        _forecastingDemandForecastService = forecastingDemandForecastService;
        _invoker = invoker;
        _licensingEntitlementQueryService = licensingEntitlementQueryService;
        _licensingLicenseAssignmentValidationService = licensingLicenseAssignmentValidationService;
        _licensingSoftwareLicenseQueryService = licensingSoftwareLicenseQueryService;
        _logger = logger;
        _patientsPatientCalculationService = patientsPatientCalculationService;
        _repository = repository;
        _settings = settings;
    }
}
