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
using ApiFuel = Acme.Fuel.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Itsm;

/// <summary>
/// Manages incidents: creates, reads, changes, and lists them.
/// </summary>
public sealed partial class IncidentEndpoint : ApiItsm.IncidentService.IncidentServiceBase
{
    private readonly ApiDevices.ComplianceScanValidationService.ComplianceScanValidationServiceClient _devicesComplianceScanValidationService;
    private readonly ApiDocuments.DocumentFolderQueryService.DocumentFolderQueryServiceClient _documentsDocumentFolderQueryService;
    private readonly ApiFuel.FuelCardCalculationService.FuelCardCalculationServiceClient _fuelFuelCardCalculationService;
    private readonly IIncidentEndpointSendListIncidents0Port _incidentEndpointSendListIncidents0Port;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.ChangeRequestQueryService.ChangeRequestQueryServiceClient _itsmChangeRequestQueryService;
    private readonly ApiItsm.ChangeRequestValidationService.ChangeRequestValidationServiceClient _itsmChangeRequestValidationService;
    private readonly ApiItsm.IncidentCalculationService.IncidentCalculationServiceClient _itsmIncidentCalculationService;
    private readonly ApiItsm.ProblemCalculationService.ProblemCalculationServiceClient _itsmProblemCalculationService;
    private readonly ApiItsm.ProblemValidationService.ProblemValidationServiceClient _itsmProblemValidationService;
    private readonly ApiLicensing.LicenseAssignmentValidationService.LicenseAssignmentValidationServiceClient _licensingLicenseAssignmentValidationService;
    private readonly ApiLicensing.SoftwareLicenseCalculationService.SoftwareLicenseCalculationServiceClient _licensingSoftwareLicenseCalculationService;
    private readonly ApiLicensing.SoftwareLicenseValidationService.SoftwareLicenseValidationServiceClient _licensingSoftwareLicenseValidationService;
    private readonly ILogger<IncidentEndpoint> _logger;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly IOptions<MeshOptions> _settings;

    public IncidentEndpoint(
        ApiDevices.ComplianceScanValidationService.ComplianceScanValidationServiceClient devicesComplianceScanValidationService,
        ApiDocuments.DocumentFolderQueryService.DocumentFolderQueryServiceClient documentsDocumentFolderQueryService,
        ApiFuel.FuelCardCalculationService.FuelCardCalculationServiceClient fuelFuelCardCalculationService,
        IIncidentEndpointSendListIncidents0Port incidentEndpointSendListIncidents0PortAdapter,
        CallInvoker invoker,
        ApiItsm.ChangeRequestQueryService.ChangeRequestQueryServiceClient itsmChangeRequestQueryService,
        ApiItsm.ChangeRequestValidationService.ChangeRequestValidationServiceClient itsmChangeRequestValidationService,
        ApiItsm.IncidentCalculationService.IncidentCalculationServiceClient itsmIncidentCalculationService,
        ApiItsm.ProblemCalculationService.ProblemCalculationServiceClient itsmProblemCalculationService,
        ApiItsm.ProblemValidationService.ProblemValidationServiceClient itsmProblemValidationService,
        ApiLicensing.LicenseAssignmentValidationService.LicenseAssignmentValidationServiceClient licensingLicenseAssignmentValidationService,
        ApiLicensing.SoftwareLicenseCalculationService.SoftwareLicenseCalculationServiceClient licensingSoftwareLicenseCalculationService,
        ApiLicensing.SoftwareLicenseValidationService.SoftwareLicenseValidationServiceClient licensingSoftwareLicenseValidationService,
        ILogger<IncidentEndpoint> logger,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings)
    {
        _devicesComplianceScanValidationService = devicesComplianceScanValidationService;
        _documentsDocumentFolderQueryService = documentsDocumentFolderQueryService;
        _fuelFuelCardCalculationService = fuelFuelCardCalculationService;
        _incidentEndpointSendListIncidents0Port = incidentEndpointSendListIncidents0PortAdapter;
        _invoker = invoker;
        _itsmChangeRequestQueryService = itsmChangeRequestQueryService;
        _itsmChangeRequestValidationService = itsmChangeRequestValidationService;
        _itsmIncidentCalculationService = itsmIncidentCalculationService;
        _itsmProblemCalculationService = itsmProblemCalculationService;
        _itsmProblemValidationService = itsmProblemValidationService;
        _licensingLicenseAssignmentValidationService = licensingLicenseAssignmentValidationService;
        _licensingSoftwareLicenseCalculationService = licensingSoftwareLicenseCalculationService;
        _licensingSoftwareLicenseValidationService = licensingSoftwareLicenseValidationService;
        _logger = logger;
        _registry = registry;
        _repository = repository;
        _settings = settings;
    }
}
