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
using ApiBanking = Acme.Banking.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;

namespace Acme.Workforce.Helpdesk;

/// <summary>
/// Routes knowledge articles through approval, with escalation and delegation.
/// </summary>
public sealed partial class KnowledgeArticleApprovalEndpoint : ApiHelpdesk.KnowledgeArticleApprovalService.KnowledgeArticleApprovalServiceBase
{
    private readonly ApiAuthorization.AccessPolicyQueryService.AccessPolicyQueryServiceClient _authorizationAccessPolicyQueryService;
    private readonly ApiBanking.BankAccountQueryService.BankAccountQueryServiceClient _bankingBankAccountQueryService;
    private readonly NotificationBus _bus;
    private readonly ApiDocuments.SignatureValidationService.SignatureValidationServiceClient _documentsSignatureValidationService;
    private readonly ApiEmployees.EmployeeService.EmployeeServiceClient _employeesEmployeeService;
    private readonly ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient _employeesOrgUnitQueryService;
    private readonly ApiHelpdesk.EscalationReportingService.EscalationReportingServiceClient _helpdeskEscalationReportingService;
    private readonly ApiHelpdesk.KnowledgeArticleCalculationService.KnowledgeArticleCalculationServiceClient _helpdeskKnowledgeArticleCalculationService;
    private readonly ApiHelpdesk.KnowledgeArticleLifecycleService.KnowledgeArticleLifecycleServiceClient _helpdeskKnowledgeArticleLifecycleService;
    private readonly ApiHelpdesk.KnowledgeArticleValidationService.KnowledgeArticleValidationServiceClient _helpdeskKnowledgeArticleValidationService;
    private readonly ApiHelpdesk.TicketApprovalService.TicketApprovalServiceClient _helpdeskTicketApprovalService;
    private readonly ApiHelpdesk.TicketCalculationService.TicketCalculationServiceClient _helpdeskTicketCalculationService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.IncidentApprovalService.IncidentApprovalServiceClient _itsmIncidentApprovalService;
    private readonly IKnowledgeArticleApprovalEndpointSendSubmitKnowledgeArticle1Port _knowledgeArticleApprovalEndpointSendSubmitKnowledgeArticle1Port;
    private readonly ApiLicensing.LicenseAssignmentWorkflowService.LicenseAssignmentWorkflowServiceClient _licensingLicenseAssignmentWorkflowService;
    private readonly ILogger<KnowledgeArticleApprovalEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiHelpdesk.RenewKnowledgeArticleRequest, CallOptions, AsyncUnaryCall<ApiHelpdesk.RenewKnowledgeArticleResponse>> _sendEscalateKnowledgeArticle1;
    private readonly IOptions<MeshOptions> _settings;

    public KnowledgeArticleApprovalEndpoint(
        ApiAuthorization.AccessPolicyQueryService.AccessPolicyQueryServiceClient authorizationAccessPolicyQueryService,
        ApiBanking.BankAccountQueryService.BankAccountQueryServiceClient bankingBankAccountQueryService,
        NotificationBus bus,
        ApiDocuments.SignatureValidationService.SignatureValidationServiceClient documentsSignatureValidationService,
        ApiEmployees.EmployeeService.EmployeeServiceClient employeesEmployeeService,
        ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient employeesOrgUnitQueryService,
        ApiHelpdesk.EscalationReportingService.EscalationReportingServiceClient helpdeskEscalationReportingService,
        ApiHelpdesk.KnowledgeArticleCalculationService.KnowledgeArticleCalculationServiceClient helpdeskKnowledgeArticleCalculationService,
        ApiHelpdesk.KnowledgeArticleLifecycleService.KnowledgeArticleLifecycleServiceClient helpdeskKnowledgeArticleLifecycleService,
        ApiHelpdesk.KnowledgeArticleValidationService.KnowledgeArticleValidationServiceClient helpdeskKnowledgeArticleValidationService,
        ApiHelpdesk.TicketApprovalService.TicketApprovalServiceClient helpdeskTicketApprovalService,
        ApiHelpdesk.TicketCalculationService.TicketCalculationServiceClient helpdeskTicketCalculationService,
        CallInvoker invoker,
        ApiItsm.IncidentApprovalService.IncidentApprovalServiceClient itsmIncidentApprovalService,
        IKnowledgeArticleApprovalEndpointSendSubmitKnowledgeArticle1Port knowledgeArticleApprovalEndpointSendSubmitKnowledgeArticle1PortAdapter,
        ApiLicensing.LicenseAssignmentWorkflowService.LicenseAssignmentWorkflowServiceClient licensingLicenseAssignmentWorkflowService,
        ILogger<KnowledgeArticleApprovalEndpoint> logger,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings)
    {
        _authorizationAccessPolicyQueryService = authorizationAccessPolicyQueryService;
        _bankingBankAccountQueryService = bankingBankAccountQueryService;
        _bus = bus;
        _documentsSignatureValidationService = documentsSignatureValidationService;
        _employeesEmployeeService = employeesEmployeeService;
        _employeesOrgUnitQueryService = employeesOrgUnitQueryService;
        _helpdeskEscalationReportingService = helpdeskEscalationReportingService;
        _helpdeskKnowledgeArticleCalculationService = helpdeskKnowledgeArticleCalculationService;
        _helpdeskKnowledgeArticleLifecycleService = helpdeskKnowledgeArticleLifecycleService;
        _helpdeskKnowledgeArticleValidationService = helpdeskKnowledgeArticleValidationService;
        _helpdeskTicketApprovalService = helpdeskTicketApprovalService;
        _helpdeskTicketCalculationService = helpdeskTicketCalculationService;
        _invoker = invoker;
        _itsmIncidentApprovalService = itsmIncidentApprovalService;
        _knowledgeArticleApprovalEndpointSendSubmitKnowledgeArticle1Port = knowledgeArticleApprovalEndpointSendSubmitKnowledgeArticle1PortAdapter;
        _licensingLicenseAssignmentWorkflowService = licensingLicenseAssignmentWorkflowService;
        _logger = logger;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _settings = settings;
        _sendEscalateKnowledgeArticle1 = _helpdeskKnowledgeArticleLifecycleService.RenewKnowledgeArticleAsync;
    }
}
