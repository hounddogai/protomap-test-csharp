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
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiWebhooks = Acme.Webhooks.V1;

namespace Acme.Workforce.Helpdesk;

/// <summary>
/// Checks knowledge articles against the rules before anything acts on them.
/// </summary>
public sealed partial class KnowledgeArticleValidationEndpoint : ApiHelpdesk.KnowledgeArticleValidationService.KnowledgeArticleValidationServiceBase
{
    private readonly ApiAuthorization.AccessPolicyService.AccessPolicyServiceClient _authorizationAccessPolicyService;
    private readonly ApiDocuments.DocumentFolderQueryService.DocumentFolderQueryServiceClient _documentsDocumentFolderQueryService;
    private readonly ApiDocuments.DocumentNotificationService.DocumentNotificationServiceClient _documentsDocumentNotificationService;
    private readonly ApiDocuments.DocumentTemplateCalculationService.DocumentTemplateCalculationServiceClient _documentsDocumentTemplateCalculationService;
    private readonly ApiHelpdesk.KnowledgeArticleCalculationService.KnowledgeArticleCalculationServiceClient _helpdeskKnowledgeArticleCalculationService;
    private readonly ApiHelpdesk.SlaPolicyCalculationService.SlaPolicyCalculationServiceClient _helpdeskSlaPolicyCalculationService;
    private readonly ApiHelpdesk.TicketCalculationService.TicketCalculationServiceClient _helpdeskTicketCalculationService;
    private readonly ApiHelpdesk.TicketValidationService.TicketValidationServiceClient _helpdeskTicketValidationService;
    private readonly ApiItsm.IncidentValidationService.IncidentValidationServiceClient _itsmIncidentValidationService;
    private readonly IKnowledgeArticleValidationEndpointSendLintKnowledgeArticles1Port _knowledgeArticleValidationEndpointSendLintKnowledgeArticles1Port;
    private readonly ILogger<KnowledgeArticleValidationEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly IRecordRepository _repository;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiWebhooks.WebhookEndpointQueryService.WebhookEndpointQueryServiceClient _webhooksWebhookEndpointQueryService;

    public KnowledgeArticleValidationEndpoint(
        ApiAuthorization.AccessPolicyService.AccessPolicyServiceClient authorizationAccessPolicyService,
        ApiDocuments.DocumentFolderQueryService.DocumentFolderQueryServiceClient documentsDocumentFolderQueryService,
        ApiDocuments.DocumentNotificationService.DocumentNotificationServiceClient documentsDocumentNotificationService,
        ApiDocuments.DocumentTemplateCalculationService.DocumentTemplateCalculationServiceClient documentsDocumentTemplateCalculationService,
        ApiHelpdesk.KnowledgeArticleCalculationService.KnowledgeArticleCalculationServiceClient helpdeskKnowledgeArticleCalculationService,
        ApiHelpdesk.SlaPolicyCalculationService.SlaPolicyCalculationServiceClient helpdeskSlaPolicyCalculationService,
        ApiHelpdesk.TicketCalculationService.TicketCalculationServiceClient helpdeskTicketCalculationService,
        ApiHelpdesk.TicketValidationService.TicketValidationServiceClient helpdeskTicketValidationService,
        ApiItsm.IncidentValidationService.IncidentValidationServiceClient itsmIncidentValidationService,
        IKnowledgeArticleValidationEndpointSendLintKnowledgeArticles1Port knowledgeArticleValidationEndpointSendLintKnowledgeArticles1PortAdapter,
        ILogger<KnowledgeArticleValidationEndpoint> logger,
        RetryPolicy policy,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiWebhooks.WebhookEndpointQueryService.WebhookEndpointQueryServiceClient webhooksWebhookEndpointQueryService)
    {
        _authorizationAccessPolicyService = authorizationAccessPolicyService;
        _documentsDocumentFolderQueryService = documentsDocumentFolderQueryService;
        _documentsDocumentNotificationService = documentsDocumentNotificationService;
        _documentsDocumentTemplateCalculationService = documentsDocumentTemplateCalculationService;
        _helpdeskKnowledgeArticleCalculationService = helpdeskKnowledgeArticleCalculationService;
        _helpdeskSlaPolicyCalculationService = helpdeskSlaPolicyCalculationService;
        _helpdeskTicketCalculationService = helpdeskTicketCalculationService;
        _helpdeskTicketValidationService = helpdeskTicketValidationService;
        _itsmIncidentValidationService = itsmIncidentValidationService;
        _knowledgeArticleValidationEndpointSendLintKnowledgeArticles1Port = knowledgeArticleValidationEndpointSendLintKnowledgeArticles1PortAdapter;
        _logger = logger;
        _policy = policy;
        _repository = repository;
        _settings = settings;
        _webhooksWebhookEndpointQueryService = webhooksWebhookEndpointQueryService;
    }
}
