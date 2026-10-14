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
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiPatients = Acme.Patients.V1;
using ApiTelematics = Acme.Telematics.V1;

namespace Acme.Workforce.Corporateevents;

/// <summary>
/// Computes the figures of a registration, such as totals, taxes, and estimates.
/// </summary>
public sealed partial class RegistrationCalculationEndpoint : ApiCorporateevents.RegistrationCalculationService.RegistrationCalculationServiceBase
{
    private readonly ApiCorporateevents.CorporateEventCalculationService.CorporateEventCalculationServiceClient _corporateeventsCorporateEventCalculationService;
    private readonly ApiDocuments.DocumentTemplateNotificationService.DocumentTemplateNotificationServiceClient _documentsDocumentTemplateNotificationService;
    private readonly ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient _documentsDocumentTemplateSyncService;
    private readonly ApiHelpdesk.TicketService.TicketServiceClient _helpdeskTicketService;
    private readonly ILogger<RegistrationCalculationEndpoint> _logger;
    private readonly ApiPatients.PatientService.PatientServiceClient _patientsPatientService;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiCorporateevents.QuoteCorporateEventRequest, CallOptions, AsyncUnaryCall<ApiCorporateevents.QuoteCorporateEventResponse>> _sendExplainRegistrationCalculation2;
    private readonly ApiTelematics.TelemetryReadingCalculationService.TelemetryReadingCalculationServiceClient _telematicsTelemetryReadingCalculationService;

    public RegistrationCalculationEndpoint(
        ApiCorporateevents.CorporateEventCalculationService.CorporateEventCalculationServiceClient corporateeventsCorporateEventCalculationService,
        ApiDocuments.DocumentTemplateNotificationService.DocumentTemplateNotificationServiceClient documentsDocumentTemplateNotificationService,
        ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient documentsDocumentTemplateSyncService,
        ApiHelpdesk.TicketService.TicketServiceClient helpdeskTicketService,
        ILogger<RegistrationCalculationEndpoint> logger,
        ApiPatients.PatientService.PatientServiceClient patientsPatientService,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiTelematics.TelemetryReadingCalculationService.TelemetryReadingCalculationServiceClient telematicsTelemetryReadingCalculationService)
    {
        _corporateeventsCorporateEventCalculationService = corporateeventsCorporateEventCalculationService;
        _documentsDocumentTemplateNotificationService = documentsDocumentTemplateNotificationService;
        _documentsDocumentTemplateSyncService = documentsDocumentTemplateSyncService;
        _helpdeskTicketService = helpdeskTicketService;
        _logger = logger;
        _patientsPatientService = patientsPatientService;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _telematicsTelemetryReadingCalculationService = telematicsTelemetryReadingCalculationService;
        _sendExplainRegistrationCalculation2 = _corporateeventsCorporateEventCalculationService.QuoteCorporateEventAsync;
    }
}
