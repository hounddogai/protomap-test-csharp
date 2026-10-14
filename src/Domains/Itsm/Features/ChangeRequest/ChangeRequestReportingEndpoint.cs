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
using ApiLicensing = Acme.Licensing.V1;
using ApiMaintenance = Acme.Maintenance.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Itsm;

/// <summary>
/// Builds and schedules reports about change requests.
/// </summary>
public sealed partial class ChangeRequestReportingEndpoint : ApiItsm.ChangeRequestReportingService.ChangeRequestReportingServiceBase
{
    private readonly IChangeRequestReportingEndpointSendScheduleChangeRequestReport1Port _changeRequestReportingEndpointSendScheduleChangeRequestReport1Port;
    private readonly ApiDevices.ComplianceScanLifecycleService.ComplianceScanLifecycleServiceClient _devicesComplianceScanLifecycleService;
    private readonly ApiDocuments.DocumentService.DocumentServiceClient _documentsDocumentService;
    private readonly ApiDocuments.SignatureExportService.SignatureExportServiceClient _documentsSignatureExportService;
    private readonly ApiEmployees.PositionValidationService.PositionValidationServiceClient _employeesPositionValidationService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.IncidentCalculationService.IncidentCalculationServiceClient _itsmIncidentCalculationService;
    private readonly ApiItsm.IncidentQueryService.IncidentQueryServiceClient _itsmIncidentQueryService;
    private readonly ApiItsm.IncidentService.IncidentServiceClient _itsmIncidentService;
    private readonly ApiItsm.ProblemCalculationService.ProblemCalculationServiceClient _itsmProblemCalculationService;
    private readonly ApiItsm.ProblemValidationService.ProblemValidationServiceClient _itsmProblemValidationService;
    private readonly ApiItsm.ServiceAssetCalculationService.ServiceAssetCalculationServiceClient _itsmServiceAssetCalculationService;
    private readonly ApiItsm.ServiceAssetValidationService.ServiceAssetValidationServiceClient _itsmServiceAssetValidationService;
    private readonly ApiLicensing.LicenseAssignmentReportingService.LicenseAssignmentReportingServiceClient _licensingLicenseAssignmentReportingService;
    private readonly ILogger<ChangeRequestReportingEndpoint> _logger;
    private readonly ApiMaintenance.WorkOrderQueryService.WorkOrderQueryServiceClient _maintenanceWorkOrderQueryService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiItsm.CheckServiceAssetEligibilityRequest, CallOptions, AsyncUnaryCall<ApiItsm.CheckServiceAssetEligibilityResponse>> _sendDeleteChangeRequestReport0;
    private readonly IOptions<MeshOptions> _settings;

    public ChangeRequestReportingEndpoint(
        IChangeRequestReportingEndpointSendScheduleChangeRequestReport1Port changeRequestReportingEndpointSendScheduleChangeRequestReport1PortAdapter,
        ApiDevices.ComplianceScanLifecycleService.ComplianceScanLifecycleServiceClient devicesComplianceScanLifecycleService,
        ApiDocuments.DocumentService.DocumentServiceClient documentsDocumentService,
        ApiDocuments.SignatureExportService.SignatureExportServiceClient documentsSignatureExportService,
        ApiEmployees.PositionValidationService.PositionValidationServiceClient employeesPositionValidationService,
        CallInvoker invoker,
        ApiItsm.IncidentCalculationService.IncidentCalculationServiceClient itsmIncidentCalculationService,
        ApiItsm.IncidentQueryService.IncidentQueryServiceClient itsmIncidentQueryService,
        ApiItsm.IncidentService.IncidentServiceClient itsmIncidentService,
        ApiItsm.ProblemCalculationService.ProblemCalculationServiceClient itsmProblemCalculationService,
        ApiItsm.ProblemValidationService.ProblemValidationServiceClient itsmProblemValidationService,
        ApiItsm.ServiceAssetCalculationService.ServiceAssetCalculationServiceClient itsmServiceAssetCalculationService,
        ApiItsm.ServiceAssetValidationService.ServiceAssetValidationServiceClient itsmServiceAssetValidationService,
        ApiLicensing.LicenseAssignmentReportingService.LicenseAssignmentReportingServiceClient licensingLicenseAssignmentReportingService,
        ILogger<ChangeRequestReportingEndpoint> logger,
        ApiMaintenance.WorkOrderQueryService.WorkOrderQueryServiceClient maintenanceWorkOrderQueryService,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings)
    {
        _changeRequestReportingEndpointSendScheduleChangeRequestReport1Port = changeRequestReportingEndpointSendScheduleChangeRequestReport1PortAdapter;
        _devicesComplianceScanLifecycleService = devicesComplianceScanLifecycleService;
        _documentsDocumentService = documentsDocumentService;
        _documentsSignatureExportService = documentsSignatureExportService;
        _employeesPositionValidationService = employeesPositionValidationService;
        _invoker = invoker;
        _itsmIncidentCalculationService = itsmIncidentCalculationService;
        _itsmIncidentQueryService = itsmIncidentQueryService;
        _itsmIncidentService = itsmIncidentService;
        _itsmProblemCalculationService = itsmProblemCalculationService;
        _itsmProblemValidationService = itsmProblemValidationService;
        _itsmServiceAssetCalculationService = itsmServiceAssetCalculationService;
        _itsmServiceAssetValidationService = itsmServiceAssetValidationService;
        _licensingLicenseAssignmentReportingService = licensingLicenseAssignmentReportingService;
        _logger = logger;
        _maintenanceWorkOrderQueryService = maintenanceWorkOrderQueryService;
        _registry = registry;
        _repository = repository;
        _settings = settings;
        _sendDeleteChangeRequestReport0 = _itsmServiceAssetValidationService.CheckServiceAssetEligibilityAsync;
    }
}
