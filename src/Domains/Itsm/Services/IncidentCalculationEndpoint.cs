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
using ApiAudit = Acme.Audit.V1;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Itsm;

/// <summary>
/// Computes the figures of an incident, such as totals, taxes, and estimates.
/// </summary>
public sealed partial class IncidentCalculationEndpoint : ApiItsm.IncidentCalculationService.IncidentCalculationServiceBase
{
    private readonly ApiAudit.AuditEventValidationService.AuditEventValidationServiceClient _auditAuditEventValidationService;
    private readonly NotificationBus _bus;
    private readonly ApiDevices.ComplianceScanValidationService.ComplianceScanValidationServiceClient _devicesComplianceScanValidationService;
    private readonly ApiDevices.ManagedDeviceQueryService.ManagedDeviceQueryServiceClient _devicesManagedDeviceQueryService;
    private readonly ApiDevices.ManagedDeviceService.ManagedDeviceServiceClient _devicesManagedDeviceService;
    private readonly ApiDocuments.DocumentService.DocumentServiceClient _documentsDocumentService;
    private readonly ApiDocuments.DocumentTemplateExportService.DocumentTemplateExportServiceClient _documentsDocumentTemplateExportService;
    private readonly ApiDocuments.DocumentValidationService.DocumentValidationServiceClient _documentsDocumentValidationService;
    private readonly ApiDocuments.SignatureCalculationService.SignatureCalculationServiceClient _documentsSignatureCalculationService;
    private readonly ApiLicensing.EntitlementQueryService.EntitlementQueryServiceClient _licensingEntitlementQueryService;
    private readonly ApiLicensing.LicenseAssignmentValidationService.LicenseAssignmentValidationServiceClient _licensingLicenseAssignmentValidationService;
    private readonly ApiLicensing.SoftwareLicenseQueryService.SoftwareLicenseQueryServiceClient _licensingSoftwareLicenseQueryService;
    private readonly ApiLicensing.SoftwareLicenseService.SoftwareLicenseServiceClient _licensingSoftwareLicenseService;
    private readonly ApiLicensing.SoftwareLicenseValidationService.SoftwareLicenseValidationServiceClient _licensingSoftwareLicenseValidationService;
    private readonly ILogger<IncidentCalculationEndpoint> _logger;
    private readonly ApiNotifications.NotificationService.NotificationServiceClient _notificationsNotificationService;
    private readonly RetryPolicy _policy;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiDocuments.RetryDocumentTemplateExportRequest, CallOptions, AsyncUnaryCall<ApiDocuments.RetryDocumentTemplateExportResponse>> _sendSimulateIncident3;

    public IncidentCalculationEndpoint(
        ApiAudit.AuditEventValidationService.AuditEventValidationServiceClient auditAuditEventValidationService,
        NotificationBus bus,
        ApiDevices.ComplianceScanValidationService.ComplianceScanValidationServiceClient devicesComplianceScanValidationService,
        ApiDevices.ManagedDeviceQueryService.ManagedDeviceQueryServiceClient devicesManagedDeviceQueryService,
        ApiDevices.ManagedDeviceService.ManagedDeviceServiceClient devicesManagedDeviceService,
        ApiDocuments.DocumentService.DocumentServiceClient documentsDocumentService,
        ApiDocuments.DocumentTemplateExportService.DocumentTemplateExportServiceClient documentsDocumentTemplateExportService,
        ApiDocuments.DocumentValidationService.DocumentValidationServiceClient documentsDocumentValidationService,
        ApiDocuments.SignatureCalculationService.SignatureCalculationServiceClient documentsSignatureCalculationService,
        ApiLicensing.EntitlementQueryService.EntitlementQueryServiceClient licensingEntitlementQueryService,
        ApiLicensing.LicenseAssignmentValidationService.LicenseAssignmentValidationServiceClient licensingLicenseAssignmentValidationService,
        ApiLicensing.SoftwareLicenseQueryService.SoftwareLicenseQueryServiceClient licensingSoftwareLicenseQueryService,
        ApiLicensing.SoftwareLicenseService.SoftwareLicenseServiceClient licensingSoftwareLicenseService,
        ApiLicensing.SoftwareLicenseValidationService.SoftwareLicenseValidationServiceClient licensingSoftwareLicenseValidationService,
        ILogger<IncidentCalculationEndpoint> logger,
        ApiNotifications.NotificationService.NotificationServiceClient notificationsNotificationService,
        RetryPolicy policy,
        IRecordRepository repository)
    {
        _auditAuditEventValidationService = auditAuditEventValidationService;
        _bus = bus;
        _devicesComplianceScanValidationService = devicesComplianceScanValidationService;
        _devicesManagedDeviceQueryService = devicesManagedDeviceQueryService;
        _devicesManagedDeviceService = devicesManagedDeviceService;
        _documentsDocumentService = documentsDocumentService;
        _documentsDocumentTemplateExportService = documentsDocumentTemplateExportService;
        _documentsDocumentValidationService = documentsDocumentValidationService;
        _documentsSignatureCalculationService = documentsSignatureCalculationService;
        _licensingEntitlementQueryService = licensingEntitlementQueryService;
        _licensingLicenseAssignmentValidationService = licensingLicenseAssignmentValidationService;
        _licensingSoftwareLicenseQueryService = licensingSoftwareLicenseQueryService;
        _licensingSoftwareLicenseService = licensingSoftwareLicenseService;
        _licensingSoftwareLicenseValidationService = licensingSoftwareLicenseValidationService;
        _logger = logger;
        _notificationsNotificationService = notificationsNotificationService;
        _policy = policy;
        _repository = repository;
        _sendSimulateIncident3 = _documentsDocumentTemplateExportService.RetryDocumentTemplateExportAsync;
    }
}
