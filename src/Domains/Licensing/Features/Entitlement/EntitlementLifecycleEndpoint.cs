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
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Licensing;

/// <summary>
/// Moves an entitlement between the states of its lifecycle.
/// </summary>
public sealed partial class EntitlementLifecycleEndpoint : ApiLicensing.EntitlementLifecycleService.EntitlementLifecycleServiceBase
{
    private readonly ApiBenefits.EnrollmentQueryService.EnrollmentQueryServiceClient _benefitsEnrollmentQueryService;
    private readonly ApiDocuments.DocumentFolderExportService.DocumentFolderExportServiceClient _documentsDocumentFolderExportService;
    private readonly ApiDocuments.DocumentFolderHistoryService.DocumentFolderHistoryServiceClient _documentsDocumentFolderHistoryService;
    private readonly ApiDocuments.SignatureSyncService.SignatureSyncServiceClient _documentsSignatureSyncService;
    private readonly CallInvoker _invoker;
    private readonly ApiLedger.LedgerAccountValidationService.LedgerAccountValidationServiceClient _ledgerLedgerAccountValidationService;
    private readonly ApiLicensing.EntitlementValidationService.EntitlementValidationServiceClient _licensingEntitlementValidationService;
    private readonly ApiLicensing.LicenseAssignmentValidationService.LicenseAssignmentValidationServiceClient _licensingLicenseAssignmentValidationService;
    private readonly ApiLicensing.SoftwareLicenseCalculationService.SoftwareLicenseCalculationServiceClient _licensingSoftwareLicenseCalculationService;
    private readonly ApiLicensing.SoftwareLicenseService.SoftwareLicenseServiceClient _licensingSoftwareLicenseService;
    private readonly ILogger<EntitlementLifecycleEndpoint> _logger;
    private readonly ApiNotifications.NotificationValidationService.NotificationValidationServiceClient _notificationsNotificationValidationService;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiLicensing.CompareSoftwareLicenseOptionsRequest, CallOptions, AsyncUnaryCall<ApiLicensing.CompareSoftwareLicenseOptionsResponse>> _sendExpireEntitlement1;
    private readonly ApiWorkplaces.BuildingValidationService.BuildingValidationServiceClient _workplacesBuildingValidationService;

    public EntitlementLifecycleEndpoint(
        ApiBenefits.EnrollmentQueryService.EnrollmentQueryServiceClient benefitsEnrollmentQueryService,
        ApiDocuments.DocumentFolderExportService.DocumentFolderExportServiceClient documentsDocumentFolderExportService,
        ApiDocuments.DocumentFolderHistoryService.DocumentFolderHistoryServiceClient documentsDocumentFolderHistoryService,
        ApiDocuments.SignatureSyncService.SignatureSyncServiceClient documentsSignatureSyncService,
        CallInvoker invoker,
        ApiLedger.LedgerAccountValidationService.LedgerAccountValidationServiceClient ledgerLedgerAccountValidationService,
        ApiLicensing.EntitlementValidationService.EntitlementValidationServiceClient licensingEntitlementValidationService,
        ApiLicensing.LicenseAssignmentValidationService.LicenseAssignmentValidationServiceClient licensingLicenseAssignmentValidationService,
        ApiLicensing.SoftwareLicenseCalculationService.SoftwareLicenseCalculationServiceClient licensingSoftwareLicenseCalculationService,
        ApiLicensing.SoftwareLicenseService.SoftwareLicenseServiceClient licensingSoftwareLicenseService,
        ILogger<EntitlementLifecycleEndpoint> logger,
        ApiNotifications.NotificationValidationService.NotificationValidationServiceClient notificationsNotificationValidationService,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiWorkplaces.BuildingValidationService.BuildingValidationServiceClient workplacesBuildingValidationService)
    {
        _benefitsEnrollmentQueryService = benefitsEnrollmentQueryService;
        _documentsDocumentFolderExportService = documentsDocumentFolderExportService;
        _documentsDocumentFolderHistoryService = documentsDocumentFolderHistoryService;
        _documentsSignatureSyncService = documentsSignatureSyncService;
        _invoker = invoker;
        _ledgerLedgerAccountValidationService = ledgerLedgerAccountValidationService;
        _licensingEntitlementValidationService = licensingEntitlementValidationService;
        _licensingLicenseAssignmentValidationService = licensingLicenseAssignmentValidationService;
        _licensingSoftwareLicenseCalculationService = licensingSoftwareLicenseCalculationService;
        _licensingSoftwareLicenseService = licensingSoftwareLicenseService;
        _logger = logger;
        _notificationsNotificationValidationService = notificationsNotificationValidationService;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _workplacesBuildingValidationService = workplacesBuildingValidationService;
        _sendExpireEntitlement1 = _licensingSoftwareLicenseCalculationService.CompareSoftwareLicenseOptionsAsync;
    }
}
