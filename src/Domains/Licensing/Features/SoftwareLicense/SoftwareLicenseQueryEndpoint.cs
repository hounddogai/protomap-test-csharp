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
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiKeymanagement = Acme.Keymanagement.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiNotifications = Acme.Notifications.V1;

namespace Acme.Workforce.Licensing;

/// <summary>
/// Answers read-only questions about software licenses, for screens and reports.
/// </summary>
public sealed partial class SoftwareLicenseQueryEndpoint : ApiLicensing.SoftwareLicenseQueryService.SoftwareLicenseQueryServiceBase
{
    private readonly ApiDocuments.DocumentHistoryService.DocumentHistoryServiceClient _documentsDocumentHistoryService;
    private readonly ApiDocuments.SignatureValidationService.SignatureValidationServiceClient _documentsSignatureValidationService;
    private readonly ApiKeymanagement.CryptoKeyQueryService.CryptoKeyQueryServiceClient _keymanagementCryptoKeyQueryService;
    private readonly ApiLicensing.EntitlementCalculationService.EntitlementCalculationServiceClient _licensingEntitlementCalculationService;
    private readonly ApiLicensing.EntitlementValidationService.EntitlementValidationServiceClient _licensingEntitlementValidationService;
    private readonly ApiLicensing.LicenseAssignmentCalculationService.LicenseAssignmentCalculationServiceClient _licensingLicenseAssignmentCalculationService;
    private readonly ApiLicensing.SoftwareLicenseCalculationService.SoftwareLicenseCalculationServiceClient _licensingSoftwareLicenseCalculationService;
    private readonly ILogger<SoftwareLicenseQueryEndpoint> _logger;
    private readonly ApiNotifications.NotificationValidationService.NotificationValidationServiceClient _notificationsNotificationValidationService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiLicensing.ExplainEntitlementDecisionRequest, CallOptions, AsyncUnaryCall<ApiLicensing.ExplainEntitlementDecisionResponse>> _sendExportSoftwareLicenses2;

    public SoftwareLicenseQueryEndpoint(
        ApiDocuments.DocumentHistoryService.DocumentHistoryServiceClient documentsDocumentHistoryService,
        ApiDocuments.SignatureValidationService.SignatureValidationServiceClient documentsSignatureValidationService,
        ApiKeymanagement.CryptoKeyQueryService.CryptoKeyQueryServiceClient keymanagementCryptoKeyQueryService,
        ApiLicensing.EntitlementCalculationService.EntitlementCalculationServiceClient licensingEntitlementCalculationService,
        ApiLicensing.EntitlementValidationService.EntitlementValidationServiceClient licensingEntitlementValidationService,
        ApiLicensing.LicenseAssignmentCalculationService.LicenseAssignmentCalculationServiceClient licensingLicenseAssignmentCalculationService,
        ApiLicensing.SoftwareLicenseCalculationService.SoftwareLicenseCalculationServiceClient licensingSoftwareLicenseCalculationService,
        ILogger<SoftwareLicenseQueryEndpoint> logger,
        ApiNotifications.NotificationValidationService.NotificationValidationServiceClient notificationsNotificationValidationService,
        ClientRegistry registry,
        IRecordRepository repository)
    {
        _documentsDocumentHistoryService = documentsDocumentHistoryService;
        _documentsSignatureValidationService = documentsSignatureValidationService;
        _keymanagementCryptoKeyQueryService = keymanagementCryptoKeyQueryService;
        _licensingEntitlementCalculationService = licensingEntitlementCalculationService;
        _licensingEntitlementValidationService = licensingEntitlementValidationService;
        _licensingLicenseAssignmentCalculationService = licensingLicenseAssignmentCalculationService;
        _licensingSoftwareLicenseCalculationService = licensingSoftwareLicenseCalculationService;
        _logger = logger;
        _notificationsNotificationValidationService = notificationsNotificationValidationService;
        _registry = registry;
        _repository = repository;
        _sendExportSoftwareLicenses2 = _licensingEntitlementValidationService.ExplainEntitlementDecisionAsync;
    }
}
