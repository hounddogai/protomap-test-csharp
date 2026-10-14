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
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPersonalization = Acme.Personalization.V1;

namespace Acme.Workforce.Corporateevents;

/// <summary>
/// Runs the long-lived workflow that takes a venue from start to finish.
/// </summary>
public sealed partial class VenueWorkflowEndpoint : ApiCorporateevents.VenueWorkflowService.VenueWorkflowServiceBase
{
    private readonly ApiCorporateevents.CorporateEventApprovalService.CorporateEventApprovalServiceClient _corporateeventsCorporateEventApprovalService;
    private readonly ApiCorporateevents.CorporateEventReportingService.CorporateEventReportingServiceClient _corporateeventsCorporateEventReportingService;
    private readonly ApiCorporateevents.RegistrationApprovalService.RegistrationApprovalServiceClient _corporateeventsRegistrationApprovalService;
    private readonly ApiCorporateevents.VenueApprovalService.VenueApprovalServiceClient _corporateeventsVenueApprovalService;
    private readonly ApiCorporateevents.VenueCalculationService.VenueCalculationServiceClient _corporateeventsVenueCalculationService;
    private readonly ApiCorporateevents.VenueLifecycleService.VenueLifecycleServiceClient _corporateeventsVenueLifecycleService;
    private readonly ApiCorporateevents.VenueReportingService.VenueReportingServiceClient _corporateeventsVenueReportingService;
    private readonly ApiDocuments.SignatureExportService.SignatureExportServiceClient _documentsSignatureExportService;
    private readonly ApiDocuments.SignatureSyncService.SignatureSyncServiceClient _documentsSignatureSyncService;
    private readonly CallInvoker _invoker;
    private readonly ApiLedger.LedgerAccountService.LedgerAccountServiceClient _ledgerLedgerAccountService;
    private readonly ApiLicensing.LicenseAssignmentApprovalService.LicenseAssignmentApprovalServiceClient _licensingLicenseAssignmentApprovalService;
    private readonly ILogger<VenueWorkflowEndpoint> _logger;
    private readonly ApiPersonalization.ProfileCalculationService.ProfileCalculationServiceClient _personalizationProfileCalculationService;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiCorporateevents.RejectCorporateEventRequest, CallOptions, AsyncUnaryCall<ApiCorporateevents.RejectCorporateEventResponse>> _sendPauseVenueWorkflow0;
    private readonly IVenueWorkflowEndpointSendSignalVenueWorkflow0Port _venueWorkflowEndpointSendSignalVenueWorkflow0Port;

    public VenueWorkflowEndpoint(
        ApiCorporateevents.CorporateEventApprovalService.CorporateEventApprovalServiceClient corporateeventsCorporateEventApprovalService,
        ApiCorporateevents.CorporateEventReportingService.CorporateEventReportingServiceClient corporateeventsCorporateEventReportingService,
        ApiCorporateevents.RegistrationApprovalService.RegistrationApprovalServiceClient corporateeventsRegistrationApprovalService,
        ApiCorporateevents.VenueApprovalService.VenueApprovalServiceClient corporateeventsVenueApprovalService,
        ApiCorporateevents.VenueCalculationService.VenueCalculationServiceClient corporateeventsVenueCalculationService,
        ApiCorporateevents.VenueLifecycleService.VenueLifecycleServiceClient corporateeventsVenueLifecycleService,
        ApiCorporateevents.VenueReportingService.VenueReportingServiceClient corporateeventsVenueReportingService,
        ApiDocuments.SignatureExportService.SignatureExportServiceClient documentsSignatureExportService,
        ApiDocuments.SignatureSyncService.SignatureSyncServiceClient documentsSignatureSyncService,
        CallInvoker invoker,
        ApiLedger.LedgerAccountService.LedgerAccountServiceClient ledgerLedgerAccountService,
        ApiLicensing.LicenseAssignmentApprovalService.LicenseAssignmentApprovalServiceClient licensingLicenseAssignmentApprovalService,
        ILogger<VenueWorkflowEndpoint> logger,
        ApiPersonalization.ProfileCalculationService.ProfileCalculationServiceClient personalizationProfileCalculationService,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository,
        IVenueWorkflowEndpointSendSignalVenueWorkflow0Port venueWorkflowEndpointSendSignalVenueWorkflow0PortAdapter)
    {
        _corporateeventsCorporateEventApprovalService = corporateeventsCorporateEventApprovalService;
        _corporateeventsCorporateEventReportingService = corporateeventsCorporateEventReportingService;
        _corporateeventsRegistrationApprovalService = corporateeventsRegistrationApprovalService;
        _corporateeventsVenueApprovalService = corporateeventsVenueApprovalService;
        _corporateeventsVenueCalculationService = corporateeventsVenueCalculationService;
        _corporateeventsVenueLifecycleService = corporateeventsVenueLifecycleService;
        _corporateeventsVenueReportingService = corporateeventsVenueReportingService;
        _documentsSignatureExportService = documentsSignatureExportService;
        _documentsSignatureSyncService = documentsSignatureSyncService;
        _invoker = invoker;
        _ledgerLedgerAccountService = ledgerLedgerAccountService;
        _licensingLicenseAssignmentApprovalService = licensingLicenseAssignmentApprovalService;
        _logger = logger;
        _personalizationProfileCalculationService = personalizationProfileCalculationService;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _venueWorkflowEndpointSendSignalVenueWorkflow0Port = venueWorkflowEndpointSendSignalVenueWorkflow0PortAdapter;
        _sendPauseVenueWorkflow0 = _corporateeventsCorporateEventApprovalService.RejectCorporateEventAsync;
    }
}
