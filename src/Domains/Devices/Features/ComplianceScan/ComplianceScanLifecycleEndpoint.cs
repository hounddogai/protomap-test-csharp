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
using ApiBenefits = Acme.Benefits.V1;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Devices;

/// <summary>
/// Moves a compliance scan between the states of its lifecycle.
/// </summary>
public sealed partial class ComplianceScanLifecycleEndpoint : ApiDevices.ComplianceScanLifecycleService.ComplianceScanLifecycleServiceBase
{
    private readonly ApiBenefits.BenefitPlanService.BenefitPlanServiceClient _benefitsBenefitPlanService;
    private readonly NotificationBus _bus;
    private readonly IComplianceScanLifecycleEndpointSendTerminateComplianceScan0Port _complianceScanLifecycleEndpointSendTerminateComplianceScan0Port;
    private readonly ApiDevices.ComplianceScanCalculationService.ComplianceScanCalculationServiceClient _devicesComplianceScanCalculationService;
    private readonly ApiDevices.ComplianceScanValidationService.ComplianceScanValidationServiceClient _devicesComplianceScanValidationService;
    private readonly ApiDevices.DevicePolicyQueryService.DevicePolicyQueryServiceClient _devicesDevicePolicyQueryService;
    private readonly ApiDevices.ManagedDeviceQueryService.ManagedDeviceQueryServiceClient _devicesManagedDeviceQueryService;
    private readonly ApiDevices.ManagedDeviceService.ManagedDeviceServiceClient _devicesManagedDeviceService;
    private readonly ApiDevices.ManagedDeviceValidationService.ManagedDeviceValidationServiceClient _devicesManagedDeviceValidationService;
    private readonly ApiDocuments.DocumentFolderExportService.DocumentFolderExportServiceClient _documentsDocumentFolderExportService;
    private readonly ApiLedger.LedgerAccountValidationService.LedgerAccountValidationServiceClient _ledgerLedgerAccountValidationService;
    private readonly ApiLicensing.SoftwareLicenseWorkflowService.SoftwareLicenseWorkflowServiceClient _licensingSoftwareLicenseWorkflowService;
    private readonly ILogger<ComplianceScanLifecycleEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly ApiRecruiting.CandidateCalculationService.CandidateCalculationServiceClient _recruitingCandidateCalculationService;
    private readonly ApiRecruiting.CandidateQueryService.CandidateQueryServiceClient _recruitingCandidateQueryService;
    private readonly ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient _recruitingJobOpeningCalculationService;
    private readonly ApiRecruiting.JobOpeningService.JobOpeningServiceClient _recruitingJobOpeningService;
    private readonly ApiRecruiting.JobOpeningValidationService.JobOpeningValidationServiceClient _recruitingJobOpeningValidationService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiRecruiting.UpdateJobOpeningRequest, CallOptions, AsyncUnaryCall<ApiRecruiting.UpdateJobOpeningResponse>> _sendReinstateComplianceScan1;
    private readonly IOptions<MeshOptions> _settings;

    public ComplianceScanLifecycleEndpoint(
        ApiBenefits.BenefitPlanService.BenefitPlanServiceClient benefitsBenefitPlanService,
        NotificationBus bus,
        IComplianceScanLifecycleEndpointSendTerminateComplianceScan0Port complianceScanLifecycleEndpointSendTerminateComplianceScan0PortAdapter,
        ApiDevices.ComplianceScanCalculationService.ComplianceScanCalculationServiceClient devicesComplianceScanCalculationService,
        ApiDevices.ComplianceScanValidationService.ComplianceScanValidationServiceClient devicesComplianceScanValidationService,
        ApiDevices.DevicePolicyQueryService.DevicePolicyQueryServiceClient devicesDevicePolicyQueryService,
        ApiDevices.ManagedDeviceQueryService.ManagedDeviceQueryServiceClient devicesManagedDeviceQueryService,
        ApiDevices.ManagedDeviceService.ManagedDeviceServiceClient devicesManagedDeviceService,
        ApiDevices.ManagedDeviceValidationService.ManagedDeviceValidationServiceClient devicesManagedDeviceValidationService,
        ApiDocuments.DocumentFolderExportService.DocumentFolderExportServiceClient documentsDocumentFolderExportService,
        ApiLedger.LedgerAccountValidationService.LedgerAccountValidationServiceClient ledgerLedgerAccountValidationService,
        ApiLicensing.SoftwareLicenseWorkflowService.SoftwareLicenseWorkflowServiceClient licensingSoftwareLicenseWorkflowService,
        ILogger<ComplianceScanLifecycleEndpoint> logger,
        RetryPolicy policy,
        ApiRecruiting.CandidateCalculationService.CandidateCalculationServiceClient recruitingCandidateCalculationService,
        ApiRecruiting.CandidateQueryService.CandidateQueryServiceClient recruitingCandidateQueryService,
        ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient recruitingJobOpeningCalculationService,
        ApiRecruiting.JobOpeningService.JobOpeningServiceClient recruitingJobOpeningService,
        ApiRecruiting.JobOpeningValidationService.JobOpeningValidationServiceClient recruitingJobOpeningValidationService,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings)
    {
        _benefitsBenefitPlanService = benefitsBenefitPlanService;
        _bus = bus;
        _complianceScanLifecycleEndpointSendTerminateComplianceScan0Port = complianceScanLifecycleEndpointSendTerminateComplianceScan0PortAdapter;
        _devicesComplianceScanCalculationService = devicesComplianceScanCalculationService;
        _devicesComplianceScanValidationService = devicesComplianceScanValidationService;
        _devicesDevicePolicyQueryService = devicesDevicePolicyQueryService;
        _devicesManagedDeviceQueryService = devicesManagedDeviceQueryService;
        _devicesManagedDeviceService = devicesManagedDeviceService;
        _devicesManagedDeviceValidationService = devicesManagedDeviceValidationService;
        _documentsDocumentFolderExportService = documentsDocumentFolderExportService;
        _ledgerLedgerAccountValidationService = ledgerLedgerAccountValidationService;
        _licensingSoftwareLicenseWorkflowService = licensingSoftwareLicenseWorkflowService;
        _logger = logger;
        _policy = policy;
        _recruitingCandidateCalculationService = recruitingCandidateCalculationService;
        _recruitingCandidateQueryService = recruitingCandidateQueryService;
        _recruitingJobOpeningCalculationService = recruitingJobOpeningCalculationService;
        _recruitingJobOpeningService = recruitingJobOpeningService;
        _recruitingJobOpeningValidationService = recruitingJobOpeningValidationService;
        _registry = registry;
        _repository = repository;
        _settings = settings;
        _sendReinstateComplianceScan1 = _recruitingJobOpeningService.UpdateJobOpeningAsync;
    }
}
