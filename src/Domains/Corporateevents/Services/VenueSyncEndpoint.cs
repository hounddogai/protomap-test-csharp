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
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;

namespace Acme.Workforce.Corporateevents;

/// <summary>
/// Keeps venues in step with the systems of record in other regions.
/// </summary>
public sealed partial class VenueSyncEndpoint : ApiCorporateevents.VenueSyncService.VenueSyncServiceBase
{
    private readonly ApiAudit.AuditEventQueryService.AuditEventQueryServiceClient _auditAuditEventQueryService;
    private readonly NotificationBus _bus;
    private readonly ApiCorporateevents.CorporateEventCalculationService.CorporateEventCalculationServiceClient _corporateeventsCorporateEventCalculationService;
    private readonly ApiCorporateevents.RegistrationCalculationService.RegistrationCalculationServiceClient _corporateeventsRegistrationCalculationService;
    private readonly ApiCorporateevents.RegistrationQueryService.RegistrationQueryServiceClient _corporateeventsRegistrationQueryService;
    private readonly ApiCorporateevents.RegistrationValidationService.RegistrationValidationServiceClient _corporateeventsRegistrationValidationService;
    private readonly ApiCorporateevents.VenueCalculationService.VenueCalculationServiceClient _corporateeventsVenueCalculationService;
    private readonly ApiDevices.DevicePolicyWorkflowService.DevicePolicyWorkflowServiceClient _devicesDevicePolicyWorkflowService;
    private readonly ApiDocuments.DocumentFolderExportService.DocumentFolderExportServiceClient _documentsDocumentFolderExportService;
    private readonly ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient _documentsDocumentTemplateSyncService;
    private readonly ApiDocuments.DocumentValidationService.DocumentValidationServiceClient _documentsDocumentValidationService;
    private readonly ApiFeatureflags.FeatureFlagQueryService.FeatureFlagQueryServiceClient _featureflagsFeatureFlagQueryService;
    private readonly CallInvoker _invoker;
    private readonly ApiLicensing.SoftwareLicenseApprovalService.SoftwareLicenseApprovalServiceClient _licensingSoftwareLicenseApprovalService;
    private readonly ILogger<VenueSyncEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly IRecordRepository _repository;
    private readonly IOptions<MeshOptions> _settings;

    public VenueSyncEndpoint(
        ApiAudit.AuditEventQueryService.AuditEventQueryServiceClient auditAuditEventQueryService,
        NotificationBus bus,
        ApiCorporateevents.CorporateEventCalculationService.CorporateEventCalculationServiceClient corporateeventsCorporateEventCalculationService,
        ApiCorporateevents.RegistrationCalculationService.RegistrationCalculationServiceClient corporateeventsRegistrationCalculationService,
        ApiCorporateevents.RegistrationQueryService.RegistrationQueryServiceClient corporateeventsRegistrationQueryService,
        ApiCorporateevents.RegistrationValidationService.RegistrationValidationServiceClient corporateeventsRegistrationValidationService,
        ApiCorporateevents.VenueCalculationService.VenueCalculationServiceClient corporateeventsVenueCalculationService,
        ApiDevices.DevicePolicyWorkflowService.DevicePolicyWorkflowServiceClient devicesDevicePolicyWorkflowService,
        ApiDocuments.DocumentFolderExportService.DocumentFolderExportServiceClient documentsDocumentFolderExportService,
        ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient documentsDocumentTemplateSyncService,
        ApiDocuments.DocumentValidationService.DocumentValidationServiceClient documentsDocumentValidationService,
        ApiFeatureflags.FeatureFlagQueryService.FeatureFlagQueryServiceClient featureflagsFeatureFlagQueryService,
        CallInvoker invoker,
        ApiLicensing.SoftwareLicenseApprovalService.SoftwareLicenseApprovalServiceClient licensingSoftwareLicenseApprovalService,
        ILogger<VenueSyncEndpoint> logger,
        RetryPolicy policy,
        IRecordRepository repository,
        IOptions<MeshOptions> settings)
    {
        _auditAuditEventQueryService = auditAuditEventQueryService;
        _bus = bus;
        _corporateeventsCorporateEventCalculationService = corporateeventsCorporateEventCalculationService;
        _corporateeventsRegistrationCalculationService = corporateeventsRegistrationCalculationService;
        _corporateeventsRegistrationQueryService = corporateeventsRegistrationQueryService;
        _corporateeventsRegistrationValidationService = corporateeventsRegistrationValidationService;
        _corporateeventsVenueCalculationService = corporateeventsVenueCalculationService;
        _devicesDevicePolicyWorkflowService = devicesDevicePolicyWorkflowService;
        _documentsDocumentFolderExportService = documentsDocumentFolderExportService;
        _documentsDocumentTemplateSyncService = documentsDocumentTemplateSyncService;
        _documentsDocumentValidationService = documentsDocumentValidationService;
        _featureflagsFeatureFlagQueryService = featureflagsFeatureFlagQueryService;
        _invoker = invoker;
        _licensingSoftwareLicenseApprovalService = licensingSoftwareLicenseApprovalService;
        _logger = logger;
        _policy = policy;
        _repository = repository;
        _settings = settings;
    }
}
