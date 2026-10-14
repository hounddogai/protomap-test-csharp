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
using ApiAuthorization = Acme.Authorization.V1;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiSearch = Acme.Search.V1;

namespace Acme.Workforce.Helpdesk;

/// <summary>
/// Answers read-only questions about knowledge articles, for screens and reports.
/// </summary>
public sealed partial class KnowledgeArticleQueryEndpoint : ApiHelpdesk.KnowledgeArticleQueryService.KnowledgeArticleQueryServiceBase
{
    private readonly ApiAuthorization.AccessPolicyService.AccessPolicyServiceClient _authorizationAccessPolicyService;
    private readonly NotificationBus _bus;
    private readonly ApiDevices.ManagedDeviceValidationService.ManagedDeviceValidationServiceClient _devicesManagedDeviceValidationService;
    private readonly ApiHelpdesk.EscalationValidationService.EscalationValidationServiceClient _helpdeskEscalationValidationService;
    private readonly ApiHelpdesk.KnowledgeArticleCalculationService.KnowledgeArticleCalculationServiceClient _helpdeskKnowledgeArticleCalculationService;
    private readonly ApiHelpdesk.SlaPolicyCalculationService.SlaPolicyCalculationServiceClient _helpdeskSlaPolicyCalculationService;
    private readonly ApiHelpdesk.TicketQueryService.TicketQueryServiceClient _helpdeskTicketQueryService;
    private readonly ApiItsm.ChangeRequestValidationService.ChangeRequestValidationServiceClient _itsmChangeRequestValidationService;
    private readonly ApiItsm.ServiceAssetValidationService.ServiceAssetValidationServiceClient _itsmServiceAssetValidationService;
    private readonly ApiLicensing.LicenseAssignmentValidationService.LicenseAssignmentValidationServiceClient _licensingLicenseAssignmentValidationService;
    private readonly ILogger<KnowledgeArticleQueryEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiSearch.SearchIndexQueryService.SearchIndexQueryServiceClient _searchSearchIndexQueryService;

    public KnowledgeArticleQueryEndpoint(
        ApiAuthorization.AccessPolicyService.AccessPolicyServiceClient authorizationAccessPolicyService,
        NotificationBus bus,
        ApiDevices.ManagedDeviceValidationService.ManagedDeviceValidationServiceClient devicesManagedDeviceValidationService,
        ApiHelpdesk.EscalationValidationService.EscalationValidationServiceClient helpdeskEscalationValidationService,
        ApiHelpdesk.KnowledgeArticleCalculationService.KnowledgeArticleCalculationServiceClient helpdeskKnowledgeArticleCalculationService,
        ApiHelpdesk.SlaPolicyCalculationService.SlaPolicyCalculationServiceClient helpdeskSlaPolicyCalculationService,
        ApiHelpdesk.TicketQueryService.TicketQueryServiceClient helpdeskTicketQueryService,
        ApiItsm.ChangeRequestValidationService.ChangeRequestValidationServiceClient itsmChangeRequestValidationService,
        ApiItsm.ServiceAssetValidationService.ServiceAssetValidationServiceClient itsmServiceAssetValidationService,
        ApiLicensing.LicenseAssignmentValidationService.LicenseAssignmentValidationServiceClient licensingLicenseAssignmentValidationService,
        ILogger<KnowledgeArticleQueryEndpoint> logger,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiSearch.SearchIndexQueryService.SearchIndexQueryServiceClient searchSearchIndexQueryService)
    {
        _authorizationAccessPolicyService = authorizationAccessPolicyService;
        _bus = bus;
        _devicesManagedDeviceValidationService = devicesManagedDeviceValidationService;
        _helpdeskEscalationValidationService = helpdeskEscalationValidationService;
        _helpdeskKnowledgeArticleCalculationService = helpdeskKnowledgeArticleCalculationService;
        _helpdeskSlaPolicyCalculationService = helpdeskSlaPolicyCalculationService;
        _helpdeskTicketQueryService = helpdeskTicketQueryService;
        _itsmChangeRequestValidationService = itsmChangeRequestValidationService;
        _itsmServiceAssetValidationService = itsmServiceAssetValidationService;
        _licensingLicenseAssignmentValidationService = licensingLicenseAssignmentValidationService;
        _logger = logger;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _searchSearchIndexQueryService = searchSearchIndexQueryService;
    }
}
