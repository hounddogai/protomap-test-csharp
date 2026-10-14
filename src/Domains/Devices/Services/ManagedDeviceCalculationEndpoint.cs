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
using ApiAnomalies = Acme.Anomalies.V1;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiPatients = Acme.Patients.V1;

namespace Acme.Workforce.Devices;

/// <summary>
/// Computes the figures of a managed device, such as totals, taxes, and estimates.
/// </summary>
public sealed partial class ManagedDeviceCalculationEndpoint : ApiDevices.ManagedDeviceCalculationService.ManagedDeviceCalculationServiceBase
{
    private readonly ApiAnomalies.AnomalyService.AnomalyServiceClient _anomaliesAnomalyService;
    private readonly ApiDocuments.DocumentFolderValidationService.DocumentFolderValidationServiceClient _documentsDocumentFolderValidationService;
    private readonly ApiDocuments.DocumentQueryService.DocumentQueryServiceClient _documentsDocumentQueryService;
    private readonly ApiIdentity.UserValidationService.UserValidationServiceClient _identityUserValidationService;
    private readonly CallInvoker _invoker;
    private readonly ApiLicensing.EntitlementCalculationService.EntitlementCalculationServiceClient _licensingEntitlementCalculationService;
    private readonly ApiLicensing.LicenseAssignmentCalculationService.LicenseAssignmentCalculationServiceClient _licensingLicenseAssignmentCalculationService;
    private readonly ApiLicensing.LicenseAssignmentValidationService.LicenseAssignmentValidationServiceClient _licensingLicenseAssignmentValidationService;
    private readonly ApiLicensing.SoftwareLicenseCalculationService.SoftwareLicenseCalculationServiceClient _licensingSoftwareLicenseCalculationService;
    private readonly ApiLicensing.SoftwareLicenseValidationService.SoftwareLicenseValidationServiceClient _licensingSoftwareLicenseValidationService;
    private readonly ILogger<ManagedDeviceCalculationEndpoint> _logger;
    private readonly IManagedDeviceCalculationEndpointSendSimulateManagedDevice0Port _managedDeviceCalculationEndpointSendSimulateManagedDevice0Port;
    private readonly ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient _metricsMetricSeriesQueryService;
    private readonly ApiPatients.PatientService.PatientServiceClient _patientsPatientService;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiPatients.ClonePatientRequest, CallOptions, AsyncUnaryCall<ApiPatients.ClonePatientResponse>> _sendEstimateManagedDevice1;
    private readonly IOptions<MeshOptions> _settings;

    public ManagedDeviceCalculationEndpoint(
        ApiAnomalies.AnomalyService.AnomalyServiceClient anomaliesAnomalyService,
        ApiDocuments.DocumentFolderValidationService.DocumentFolderValidationServiceClient documentsDocumentFolderValidationService,
        ApiDocuments.DocumentQueryService.DocumentQueryServiceClient documentsDocumentQueryService,
        ApiIdentity.UserValidationService.UserValidationServiceClient identityUserValidationService,
        CallInvoker invoker,
        ApiLicensing.EntitlementCalculationService.EntitlementCalculationServiceClient licensingEntitlementCalculationService,
        ApiLicensing.LicenseAssignmentCalculationService.LicenseAssignmentCalculationServiceClient licensingLicenseAssignmentCalculationService,
        ApiLicensing.LicenseAssignmentValidationService.LicenseAssignmentValidationServiceClient licensingLicenseAssignmentValidationService,
        ApiLicensing.SoftwareLicenseCalculationService.SoftwareLicenseCalculationServiceClient licensingSoftwareLicenseCalculationService,
        ApiLicensing.SoftwareLicenseValidationService.SoftwareLicenseValidationServiceClient licensingSoftwareLicenseValidationService,
        ILogger<ManagedDeviceCalculationEndpoint> logger,
        IManagedDeviceCalculationEndpointSendSimulateManagedDevice0Port managedDeviceCalculationEndpointSendSimulateManagedDevice0PortAdapter,
        ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient metricsMetricSeriesQueryService,
        ApiPatients.PatientService.PatientServiceClient patientsPatientService,
        IRecordRepository repository,
        IOptions<MeshOptions> settings)
    {
        _anomaliesAnomalyService = anomaliesAnomalyService;
        _documentsDocumentFolderValidationService = documentsDocumentFolderValidationService;
        _documentsDocumentQueryService = documentsDocumentQueryService;
        _identityUserValidationService = identityUserValidationService;
        _invoker = invoker;
        _licensingEntitlementCalculationService = licensingEntitlementCalculationService;
        _licensingLicenseAssignmentCalculationService = licensingLicenseAssignmentCalculationService;
        _licensingLicenseAssignmentValidationService = licensingLicenseAssignmentValidationService;
        _licensingSoftwareLicenseCalculationService = licensingSoftwareLicenseCalculationService;
        _licensingSoftwareLicenseValidationService = licensingSoftwareLicenseValidationService;
        _logger = logger;
        _managedDeviceCalculationEndpointSendSimulateManagedDevice0Port = managedDeviceCalculationEndpointSendSimulateManagedDevice0PortAdapter;
        _metricsMetricSeriesQueryService = metricsMetricSeriesQueryService;
        _patientsPatientService = patientsPatientService;
        _repository = repository;
        _settings = settings;
        _sendEstimateManagedDevice1 = _patientsPatientService.ClonePatientAsync;
    }
}
