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
using ApiItsm = Acme.Itsm.V1;

namespace Acme.Workforce.Corporateevents;

/// <summary>
/// Answers read-only questions about corporate events, for screens and reports.
/// </summary>
public sealed partial class CorporateEventQueryEndpoint : ApiCorporateevents.CorporateEventQueryService.CorporateEventQueryServiceBase
{
    private readonly ICorporateEventQueryEndpointSendExportCorporateEvents2Port _corporateEventQueryEndpointSendExportCorporateEvents2Port;
    private readonly ApiCorporateevents.RegistrationCalculationService.RegistrationCalculationServiceClient _corporateeventsRegistrationCalculationService;
    private readonly ApiCorporateevents.VenueCalculationService.VenueCalculationServiceClient _corporateeventsVenueCalculationService;
    private readonly ApiCorporateevents.VenueValidationService.VenueValidationServiceClient _corporateeventsVenueValidationService;
    private readonly ApiDocuments.DocumentTemplateExportService.DocumentTemplateExportServiceClient _documentsDocumentTemplateExportService;
    private readonly ApiDocuments.SignatureSyncService.SignatureSyncServiceClient _documentsSignatureSyncService;
    private readonly ApiHelpdesk.EscalationCalculationService.EscalationCalculationServiceClient _helpdeskEscalationCalculationService;
    private readonly ApiItsm.IncidentQueryService.IncidentQueryServiceClient _itsmIncidentQueryService;
    private readonly ILogger<CorporateEventQueryEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly IRecordRepository _repository;

    public CorporateEventQueryEndpoint(
        ICorporateEventQueryEndpointSendExportCorporateEvents2Port corporateEventQueryEndpointSendExportCorporateEvents2PortAdapter,
        ApiCorporateevents.RegistrationCalculationService.RegistrationCalculationServiceClient corporateeventsRegistrationCalculationService,
        ApiCorporateevents.VenueCalculationService.VenueCalculationServiceClient corporateeventsVenueCalculationService,
        ApiCorporateevents.VenueValidationService.VenueValidationServiceClient corporateeventsVenueValidationService,
        ApiDocuments.DocumentTemplateExportService.DocumentTemplateExportServiceClient documentsDocumentTemplateExportService,
        ApiDocuments.SignatureSyncService.SignatureSyncServiceClient documentsSignatureSyncService,
        ApiHelpdesk.EscalationCalculationService.EscalationCalculationServiceClient helpdeskEscalationCalculationService,
        ApiItsm.IncidentQueryService.IncidentQueryServiceClient itsmIncidentQueryService,
        ILogger<CorporateEventQueryEndpoint> logger,
        RetryPolicy policy,
        IRecordRepository repository)
    {
        _corporateEventQueryEndpointSendExportCorporateEvents2Port = corporateEventQueryEndpointSendExportCorporateEvents2PortAdapter;
        _corporateeventsRegistrationCalculationService = corporateeventsRegistrationCalculationService;
        _corporateeventsVenueCalculationService = corporateeventsVenueCalculationService;
        _corporateeventsVenueValidationService = corporateeventsVenueValidationService;
        _documentsDocumentTemplateExportService = documentsDocumentTemplateExportService;
        _documentsSignatureSyncService = documentsSignatureSyncService;
        _helpdeskEscalationCalculationService = helpdeskEscalationCalculationService;
        _itsmIncidentQueryService = itsmIncidentQueryService;
        _logger = logger;
        _policy = policy;
        _repository = repository;
    }
}
