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
using ApiBoxoffice = Acme.Boxoffice.V1;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTelehealth = Acme.Telehealth.V1;

namespace Acme.Workforce.Itsm;

/// <summary>
/// Checks change requests against the rules before anything acts on them.
/// </summary>
public sealed partial class ChangeRequestValidationEndpoint : ApiItsm.ChangeRequestValidationService.ChangeRequestValidationServiceBase
{
    private readonly ApiBoxoffice.BoxOfficeSaleService.BoxOfficeSaleServiceClient _boxofficeBoxOfficeSaleService;
    private readonly ApiDevices.ComplianceScanCalculationService.ComplianceScanCalculationServiceClient _devicesComplianceScanCalculationService;
    private readonly ApiDevices.DevicePolicyCalculationService.DevicePolicyCalculationServiceClient _devicesDevicePolicyCalculationService;
    private readonly ApiDevices.DevicePolicyQueryService.DevicePolicyQueryServiceClient _devicesDevicePolicyQueryService;
    private readonly ApiDocuments.DocumentHistoryService.DocumentHistoryServiceClient _documentsDocumentHistoryService;
    private readonly ApiDocuments.DocumentSyncService.DocumentSyncServiceClient _documentsDocumentSyncService;
    private readonly ApiDocuments.SignatureNotificationService.SignatureNotificationServiceClient _documentsSignatureNotificationService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.IncidentCalculationService.IncidentCalculationServiceClient _itsmIncidentCalculationService;
    private readonly ApiItsm.IncidentValidationService.IncidentValidationServiceClient _itsmIncidentValidationService;
    private readonly ApiItsm.ServiceAssetCalculationService.ServiceAssetCalculationServiceClient _itsmServiceAssetCalculationService;
    private readonly ApiLicensing.LicenseAssignmentValidationService.LicenseAssignmentValidationServiceClient _licensingLicenseAssignmentValidationService;
    private readonly ApiLicensing.SoftwareLicenseValidationService.SoftwareLicenseValidationServiceClient _licensingSoftwareLicenseValidationService;
    private readonly ILogger<ChangeRequestValidationEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiTelehealth.VideoVisitCalculationService.VideoVisitCalculationServiceClient _telehealthVideoVisitCalculationService;

    public ChangeRequestValidationEndpoint(
        ApiBoxoffice.BoxOfficeSaleService.BoxOfficeSaleServiceClient boxofficeBoxOfficeSaleService,
        ApiDevices.ComplianceScanCalculationService.ComplianceScanCalculationServiceClient devicesComplianceScanCalculationService,
        ApiDevices.DevicePolicyCalculationService.DevicePolicyCalculationServiceClient devicesDevicePolicyCalculationService,
        ApiDevices.DevicePolicyQueryService.DevicePolicyQueryServiceClient devicesDevicePolicyQueryService,
        ApiDocuments.DocumentHistoryService.DocumentHistoryServiceClient documentsDocumentHistoryService,
        ApiDocuments.DocumentSyncService.DocumentSyncServiceClient documentsDocumentSyncService,
        ApiDocuments.SignatureNotificationService.SignatureNotificationServiceClient documentsSignatureNotificationService,
        CallInvoker invoker,
        ApiItsm.IncidentCalculationService.IncidentCalculationServiceClient itsmIncidentCalculationService,
        ApiItsm.IncidentValidationService.IncidentValidationServiceClient itsmIncidentValidationService,
        ApiItsm.ServiceAssetCalculationService.ServiceAssetCalculationServiceClient itsmServiceAssetCalculationService,
        ApiLicensing.LicenseAssignmentValidationService.LicenseAssignmentValidationServiceClient licensingLicenseAssignmentValidationService,
        ApiLicensing.SoftwareLicenseValidationService.SoftwareLicenseValidationServiceClient licensingSoftwareLicenseValidationService,
        ILogger<ChangeRequestValidationEndpoint> logger,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiTelehealth.VideoVisitCalculationService.VideoVisitCalculationServiceClient telehealthVideoVisitCalculationService)
    {
        _boxofficeBoxOfficeSaleService = boxofficeBoxOfficeSaleService;
        _devicesComplianceScanCalculationService = devicesComplianceScanCalculationService;
        _devicesDevicePolicyCalculationService = devicesDevicePolicyCalculationService;
        _devicesDevicePolicyQueryService = devicesDevicePolicyQueryService;
        _documentsDocumentHistoryService = documentsDocumentHistoryService;
        _documentsDocumentSyncService = documentsDocumentSyncService;
        _documentsSignatureNotificationService = documentsSignatureNotificationService;
        _invoker = invoker;
        _itsmIncidentCalculationService = itsmIncidentCalculationService;
        _itsmIncidentValidationService = itsmIncidentValidationService;
        _itsmServiceAssetCalculationService = itsmServiceAssetCalculationService;
        _licensingLicenseAssignmentValidationService = licensingLicenseAssignmentValidationService;
        _licensingSoftwareLicenseValidationService = licensingSoftwareLicenseValidationService;
        _logger = logger;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _telehealthVideoVisitCalculationService = telehealthVideoVisitCalculationService;
    }
}
