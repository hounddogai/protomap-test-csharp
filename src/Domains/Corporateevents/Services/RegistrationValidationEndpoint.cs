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
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;

namespace Acme.Workforce.Corporateevents;

/// <summary>
/// Checks registrations against the rules before anything acts on them.
/// </summary>
public sealed partial class RegistrationValidationEndpoint : ApiCorporateevents.RegistrationValidationService.RegistrationValidationServiceBase
{
    private readonly ApiBoxoffice.BoxOfficeSaleService.BoxOfficeSaleServiceClient _boxofficeBoxOfficeSaleService;
    private readonly NotificationBus _bus;
    private readonly ApiCorporateevents.CorporateEventCalculationService.CorporateEventCalculationServiceClient _corporateeventsCorporateEventCalculationService;
    private readonly ApiCorporateevents.RegistrationCalculationService.RegistrationCalculationServiceClient _corporateeventsRegistrationCalculationService;
    private readonly ApiCorporateevents.VenueCalculationService.VenueCalculationServiceClient _corporateeventsVenueCalculationService;
    private readonly ApiHelpdesk.KnowledgeArticleCalculationService.KnowledgeArticleCalculationServiceClient _helpdeskKnowledgeArticleCalculationService;
    private readonly ApiIdentity.UserQueryService.UserQueryServiceClient _identityUserQueryService;
    private readonly ApiItsm.IncidentValidationService.IncidentValidationServiceClient _itsmIncidentValidationService;
    private readonly ApiItsm.ServiceAssetValidationService.ServiceAssetValidationServiceClient _itsmServiceAssetValidationService;
    private readonly ApiLicensing.LicenseAssignmentCalculationService.LicenseAssignmentCalculationServiceClient _licensingLicenseAssignmentCalculationService;
    private readonly ILogger<RegistrationValidationEndpoint> _logger;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly IOptions<MeshOptions> _settings;

    public RegistrationValidationEndpoint(
        ApiBoxoffice.BoxOfficeSaleService.BoxOfficeSaleServiceClient boxofficeBoxOfficeSaleService,
        NotificationBus bus,
        ApiCorporateevents.CorporateEventCalculationService.CorporateEventCalculationServiceClient corporateeventsCorporateEventCalculationService,
        ApiCorporateevents.RegistrationCalculationService.RegistrationCalculationServiceClient corporateeventsRegistrationCalculationService,
        ApiCorporateevents.VenueCalculationService.VenueCalculationServiceClient corporateeventsVenueCalculationService,
        ApiHelpdesk.KnowledgeArticleCalculationService.KnowledgeArticleCalculationServiceClient helpdeskKnowledgeArticleCalculationService,
        ApiIdentity.UserQueryService.UserQueryServiceClient identityUserQueryService,
        ApiItsm.IncidentValidationService.IncidentValidationServiceClient itsmIncidentValidationService,
        ApiItsm.ServiceAssetValidationService.ServiceAssetValidationServiceClient itsmServiceAssetValidationService,
        ApiLicensing.LicenseAssignmentCalculationService.LicenseAssignmentCalculationServiceClient licensingLicenseAssignmentCalculationService,
        ILogger<RegistrationValidationEndpoint> logger,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings)
    {
        _boxofficeBoxOfficeSaleService = boxofficeBoxOfficeSaleService;
        _bus = bus;
        _corporateeventsCorporateEventCalculationService = corporateeventsCorporateEventCalculationService;
        _corporateeventsRegistrationCalculationService = corporateeventsRegistrationCalculationService;
        _corporateeventsVenueCalculationService = corporateeventsVenueCalculationService;
        _helpdeskKnowledgeArticleCalculationService = helpdeskKnowledgeArticleCalculationService;
        _identityUserQueryService = identityUserQueryService;
        _itsmIncidentValidationService = itsmIncidentValidationService;
        _itsmServiceAssetValidationService = itsmServiceAssetValidationService;
        _licensingLicenseAssignmentCalculationService = licensingLicenseAssignmentCalculationService;
        _logger = logger;
        _registry = registry;
        _repository = repository;
        _settings = settings;
    }
}
