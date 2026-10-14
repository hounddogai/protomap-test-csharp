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
using ApiBroadcasts = Acme.Broadcasts.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;

namespace Acme.Workforce.Corporateevents;

/// <summary>
/// Computes the figures of a venue, such as totals, taxes, and estimates.
/// </summary>
public sealed partial class VenueCalculationEndpoint : ApiCorporateevents.VenueCalculationService.VenueCalculationServiceBase
{
    private readonly ApiAudit.AuditEventService.AuditEventServiceClient _auditAuditEventService;
    private readonly ApiBroadcasts.BroadcastService.BroadcastServiceClient _broadcastsBroadcastService;
    private readonly NotificationBus _bus;
    private readonly ApiCorporateevents.CorporateEventCalculationService.CorporateEventCalculationServiceClient _corporateeventsCorporateEventCalculationService;
    private readonly ApiCorporateevents.CorporateEventValidationService.CorporateEventValidationServiceClient _corporateeventsCorporateEventValidationService;
    private readonly ApiCorporateevents.RegistrationCalculationService.RegistrationCalculationServiceClient _corporateeventsRegistrationCalculationService;
    private readonly ApiDocuments.DocumentTemplateNotificationService.DocumentTemplateNotificationServiceClient _documentsDocumentTemplateNotificationService;
    private readonly ApiIdentity.UserValidationService.UserValidationServiceClient _identityUserValidationService;
    private readonly ILogger<VenueCalculationEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly IVenueCalculationEndpointSendQuoteVenue2Port _venueCalculationEndpointSendQuoteVenue2Port;

    public VenueCalculationEndpoint(
        ApiAudit.AuditEventService.AuditEventServiceClient auditAuditEventService,
        ApiBroadcasts.BroadcastService.BroadcastServiceClient broadcastsBroadcastService,
        NotificationBus bus,
        ApiCorporateevents.CorporateEventCalculationService.CorporateEventCalculationServiceClient corporateeventsCorporateEventCalculationService,
        ApiCorporateevents.CorporateEventValidationService.CorporateEventValidationServiceClient corporateeventsCorporateEventValidationService,
        ApiCorporateevents.RegistrationCalculationService.RegistrationCalculationServiceClient corporateeventsRegistrationCalculationService,
        ApiDocuments.DocumentTemplateNotificationService.DocumentTemplateNotificationServiceClient documentsDocumentTemplateNotificationService,
        ApiIdentity.UserValidationService.UserValidationServiceClient identityUserValidationService,
        ILogger<VenueCalculationEndpoint> logger,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository,
        IVenueCalculationEndpointSendQuoteVenue2Port venueCalculationEndpointSendQuoteVenue2PortAdapter)
    {
        _auditAuditEventService = auditAuditEventService;
        _broadcastsBroadcastService = broadcastsBroadcastService;
        _bus = bus;
        _corporateeventsCorporateEventCalculationService = corporateeventsCorporateEventCalculationService;
        _corporateeventsCorporateEventValidationService = corporateeventsCorporateEventValidationService;
        _corporateeventsRegistrationCalculationService = corporateeventsRegistrationCalculationService;
        _documentsDocumentTemplateNotificationService = documentsDocumentTemplateNotificationService;
        _identityUserValidationService = identityUserValidationService;
        _logger = logger;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _venueCalculationEndpointSendQuoteVenue2Port = venueCalculationEndpointSendQuoteVenue2PortAdapter;
    }
}
