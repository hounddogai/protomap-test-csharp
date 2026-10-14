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
using ApiCertificates = Acme.Certificates.V1;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;

namespace Acme.Workforce.Helpdesk;

/// <summary>
/// Answers read-only questions about tickets, for screens and reports.
/// </summary>
public sealed partial class TicketQueryEndpoint : ApiHelpdesk.TicketQueryService.TicketQueryServiceBase
{
    private readonly ApiCertificates.CertificateCalculationService.CertificateCalculationServiceClient _certificatesCertificateCalculationService;
    private readonly ApiDevices.DevicePolicyCalculationService.DevicePolicyCalculationServiceClient _devicesDevicePolicyCalculationService;
    private readonly ApiDocuments.DocumentFolderSyncService.DocumentFolderSyncServiceClient _documentsDocumentFolderSyncService;
    private readonly ApiDocuments.DocumentTemplateExportService.DocumentTemplateExportServiceClient _documentsDocumentTemplateExportService;
    private readonly ApiDocuments.SignatureValidationService.SignatureValidationServiceClient _documentsSignatureValidationService;
    private readonly ApiFeatureflags.FeatureFlagService.FeatureFlagServiceClient _featureflagsFeatureFlagService;
    private readonly ApiHelpdesk.EscalationCalculationService.EscalationCalculationServiceClient _helpdeskEscalationCalculationService;
    private readonly ApiHelpdesk.KnowledgeArticleValidationService.KnowledgeArticleValidationServiceClient _helpdeskKnowledgeArticleValidationService;
    private readonly ApiHelpdesk.SlaPolicyCalculationService.SlaPolicyCalculationServiceClient _helpdeskSlaPolicyCalculationService;
    private readonly ApiHelpdesk.SlaPolicyValidationService.SlaPolicyValidationServiceClient _helpdeskSlaPolicyValidationService;
    private readonly ApiHelpdesk.TicketCalculationService.TicketCalculationServiceClient _helpdeskTicketCalculationService;
    private readonly ApiIdentity.UserQueryService.UserQueryServiceClient _identityUserQueryService;
    private readonly CallInvoker _invoker;
    private readonly ILogger<TicketQueryEndpoint> _logger;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiHelpdesk.CalculateTicketRequest, CallOptions, AsyncUnaryCall<ApiHelpdesk.CalculateTicketResponse>> _sendExportTickets3;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ITicketQueryEndpointSendSummarizeTickets0Port _ticketQueryEndpointSendSummarizeTickets0Port;

    public TicketQueryEndpoint(
        ApiCertificates.CertificateCalculationService.CertificateCalculationServiceClient certificatesCertificateCalculationService,
        ApiDevices.DevicePolicyCalculationService.DevicePolicyCalculationServiceClient devicesDevicePolicyCalculationService,
        ApiDocuments.DocumentFolderSyncService.DocumentFolderSyncServiceClient documentsDocumentFolderSyncService,
        ApiDocuments.DocumentTemplateExportService.DocumentTemplateExportServiceClient documentsDocumentTemplateExportService,
        ApiDocuments.SignatureValidationService.SignatureValidationServiceClient documentsSignatureValidationService,
        ApiFeatureflags.FeatureFlagService.FeatureFlagServiceClient featureflagsFeatureFlagService,
        ApiHelpdesk.EscalationCalculationService.EscalationCalculationServiceClient helpdeskEscalationCalculationService,
        ApiHelpdesk.KnowledgeArticleValidationService.KnowledgeArticleValidationServiceClient helpdeskKnowledgeArticleValidationService,
        ApiHelpdesk.SlaPolicyCalculationService.SlaPolicyCalculationServiceClient helpdeskSlaPolicyCalculationService,
        ApiHelpdesk.SlaPolicyValidationService.SlaPolicyValidationServiceClient helpdeskSlaPolicyValidationService,
        ApiHelpdesk.TicketCalculationService.TicketCalculationServiceClient helpdeskTicketCalculationService,
        ApiIdentity.UserQueryService.UserQueryServiceClient identityUserQueryService,
        CallInvoker invoker,
        ILogger<TicketQueryEndpoint> logger,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ITicketQueryEndpointSendSummarizeTickets0Port ticketQueryEndpointSendSummarizeTickets0PortAdapter)
    {
        _certificatesCertificateCalculationService = certificatesCertificateCalculationService;
        _devicesDevicePolicyCalculationService = devicesDevicePolicyCalculationService;
        _documentsDocumentFolderSyncService = documentsDocumentFolderSyncService;
        _documentsDocumentTemplateExportService = documentsDocumentTemplateExportService;
        _documentsSignatureValidationService = documentsSignatureValidationService;
        _featureflagsFeatureFlagService = featureflagsFeatureFlagService;
        _helpdeskEscalationCalculationService = helpdeskEscalationCalculationService;
        _helpdeskKnowledgeArticleValidationService = helpdeskKnowledgeArticleValidationService;
        _helpdeskSlaPolicyCalculationService = helpdeskSlaPolicyCalculationService;
        _helpdeskSlaPolicyValidationService = helpdeskSlaPolicyValidationService;
        _helpdeskTicketCalculationService = helpdeskTicketCalculationService;
        _identityUserQueryService = identityUserQueryService;
        _invoker = invoker;
        _logger = logger;
        _registry = registry;
        _repository = repository;
        _settings = settings;
        _ticketQueryEndpointSendSummarizeTickets0Port = ticketQueryEndpointSendSummarizeTickets0PortAdapter;
        _sendExportTickets3 = _helpdeskTicketCalculationService.CalculateTicketAsync;
    }
}
