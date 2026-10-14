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
using ApiAdmissions = Acme.Admissions.V1;
using ApiAudiences = Acme.Audiences.V1;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiPrescriptions = Acme.Prescriptions.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiSearch = Acme.Search.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Scheduling;

/// <summary>
/// Computes the figures of a roster, such as totals, taxes, and estimates.
/// </summary>
public sealed partial class RosterCalculationEndpoint : ApiScheduling.RosterCalculationService.RosterCalculationServiceBase
{
    private readonly ApiAdmissions.AdmissionService.AdmissionServiceClient _admissionsAdmissionService;
    private readonly ApiAudiences.AudienceService.AudienceServiceClient _audiencesAudienceService;
    private readonly ApiDevices.ManagedDeviceService.ManagedDeviceServiceClient _devicesManagedDeviceService;
    private readonly ApiDocuments.DocumentSyncService.DocumentSyncServiceClient _documentsDocumentSyncService;
    private readonly ApiDocuments.DocumentTemplateNotificationService.DocumentTemplateNotificationServiceClient _documentsDocumentTemplateNotificationService;
    private readonly ApiDocuments.SignatureHistoryService.SignatureHistoryServiceClient _documentsSignatureHistoryService;
    private readonly ApiDocuments.SignatureSyncService.SignatureSyncServiceClient _documentsSignatureSyncService;
    private readonly ApiHelpdesk.TicketQueryService.TicketQueryServiceClient _helpdeskTicketQueryService;
    private readonly ApiHelpdesk.TicketService.TicketServiceClient _helpdeskTicketService;
    private readonly ApiItsm.ChangeRequestValidationService.ChangeRequestValidationServiceClient _itsmChangeRequestValidationService;
    private readonly ApiItsm.IncidentQueryService.IncidentQueryServiceClient _itsmIncidentQueryService;
    private readonly ApiLedger.LedgerAccountValidationService.LedgerAccountValidationServiceClient _ledgerLedgerAccountValidationService;
    private readonly ILogger<RosterCalculationEndpoint> _logger;
    private readonly ApiMetrics.MetricSeriesValidationService.MetricSeriesValidationServiceClient _metricsMetricSeriesValidationService;
    private readonly ApiPerformance.FeedbackValidationService.FeedbackValidationServiceClient _performanceFeedbackValidationService;
    private readonly ApiPrescriptions.PrescriptionCalculationService.PrescriptionCalculationServiceClient _prescriptionsPrescriptionCalculationService;
    private readonly IRecordRepository _repository;
    private readonly IRosterCalculationEndpointSendQuoteRoster2Port _rosterCalculationEndpointSendQuoteRoster2Port;
    private readonly ApiSearch.SearchIndexValidationService.SearchIndexValidationServiceClient _searchSearchIndexValidationService;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiWorkplaces.DeskCalculationService.DeskCalculationServiceClient _workplacesDeskCalculationService;

    public RosterCalculationEndpoint(
        ApiAdmissions.AdmissionService.AdmissionServiceClient admissionsAdmissionService,
        ApiAudiences.AudienceService.AudienceServiceClient audiencesAudienceService,
        ApiDevices.ManagedDeviceService.ManagedDeviceServiceClient devicesManagedDeviceService,
        ApiDocuments.DocumentSyncService.DocumentSyncServiceClient documentsDocumentSyncService,
        ApiDocuments.DocumentTemplateNotificationService.DocumentTemplateNotificationServiceClient documentsDocumentTemplateNotificationService,
        ApiDocuments.SignatureHistoryService.SignatureHistoryServiceClient documentsSignatureHistoryService,
        ApiDocuments.SignatureSyncService.SignatureSyncServiceClient documentsSignatureSyncService,
        ApiHelpdesk.TicketQueryService.TicketQueryServiceClient helpdeskTicketQueryService,
        ApiHelpdesk.TicketService.TicketServiceClient helpdeskTicketService,
        ApiItsm.ChangeRequestValidationService.ChangeRequestValidationServiceClient itsmChangeRequestValidationService,
        ApiItsm.IncidentQueryService.IncidentQueryServiceClient itsmIncidentQueryService,
        ApiLedger.LedgerAccountValidationService.LedgerAccountValidationServiceClient ledgerLedgerAccountValidationService,
        ILogger<RosterCalculationEndpoint> logger,
        ApiMetrics.MetricSeriesValidationService.MetricSeriesValidationServiceClient metricsMetricSeriesValidationService,
        ApiPerformance.FeedbackValidationService.FeedbackValidationServiceClient performanceFeedbackValidationService,
        ApiPrescriptions.PrescriptionCalculationService.PrescriptionCalculationServiceClient prescriptionsPrescriptionCalculationService,
        IRecordRepository repository,
        IRosterCalculationEndpointSendQuoteRoster2Port rosterCalculationEndpointSendQuoteRoster2PortAdapter,
        ApiSearch.SearchIndexValidationService.SearchIndexValidationServiceClient searchSearchIndexValidationService,
        IOptions<MeshOptions> settings,
        ApiWorkplaces.DeskCalculationService.DeskCalculationServiceClient workplacesDeskCalculationService)
    {
        _admissionsAdmissionService = admissionsAdmissionService;
        _audiencesAudienceService = audiencesAudienceService;
        _devicesManagedDeviceService = devicesManagedDeviceService;
        _documentsDocumentSyncService = documentsDocumentSyncService;
        _documentsDocumentTemplateNotificationService = documentsDocumentTemplateNotificationService;
        _documentsSignatureHistoryService = documentsSignatureHistoryService;
        _documentsSignatureSyncService = documentsSignatureSyncService;
        _helpdeskTicketQueryService = helpdeskTicketQueryService;
        _helpdeskTicketService = helpdeskTicketService;
        _itsmChangeRequestValidationService = itsmChangeRequestValidationService;
        _itsmIncidentQueryService = itsmIncidentQueryService;
        _ledgerLedgerAccountValidationService = ledgerLedgerAccountValidationService;
        _logger = logger;
        _metricsMetricSeriesValidationService = metricsMetricSeriesValidationService;
        _performanceFeedbackValidationService = performanceFeedbackValidationService;
        _prescriptionsPrescriptionCalculationService = prescriptionsPrescriptionCalculationService;
        _repository = repository;
        _rosterCalculationEndpointSendQuoteRoster2Port = rosterCalculationEndpointSendQuoteRoster2PortAdapter;
        _searchSearchIndexValidationService = searchSearchIndexValidationService;
        _settings = settings;
        _workplacesDeskCalculationService = workplacesDeskCalculationService;
    }
}
