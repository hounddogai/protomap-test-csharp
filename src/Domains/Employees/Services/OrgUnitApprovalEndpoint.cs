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
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiExperiments = Acme.Experiments.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLoyalty = Acme.Loyalty.V1;
using ApiScheduling = Acme.Scheduling.V1;

namespace Acme.Workforce.Employees;

/// <summary>
/// Routes org units through approval, with escalation and delegation.
/// </summary>
public sealed partial class OrgUnitApprovalEndpoint : ApiEmployees.OrgUnitApprovalService.OrgUnitApprovalServiceBase
{
    private readonly NotificationBus _bus;
    private readonly ApiDocuments.DocumentFolderSyncService.DocumentFolderSyncServiceClient _documentsDocumentFolderSyncService;
    private readonly ApiDocuments.DocumentQueryService.DocumentQueryServiceClient _documentsDocumentQueryService;
    private readonly ApiDocuments.DocumentTemplateHistoryService.DocumentTemplateHistoryServiceClient _documentsDocumentTemplateHistoryService;
    private readonly ApiEmployees.EmployeeApprovalService.EmployeeApprovalServiceClient _employeesEmployeeApprovalService;
    private readonly ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient _employeesEmployeeQueryService;
    private readonly ApiEmployees.EmployeeService.EmployeeServiceClient _employeesEmployeeService;
    private readonly ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient _employeesEmploymentContractCalculationService;
    private readonly ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient _employeesEmploymentContractValidationService;
    private readonly ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient _employeesOrgUnitQueryService;
    private readonly ApiEmployees.PositionValidationService.PositionValidationServiceClient _employeesPositionValidationService;
    private readonly ApiExperiments.ExperimentQueryService.ExperimentQueryServiceClient _experimentsExperimentQueryService;
    private readonly ApiFeatureflags.FeatureFlagQueryService.FeatureFlagQueryServiceClient _featureflagsFeatureFlagQueryService;
    private readonly ApiFeatureflags.FeatureFlagValidationService.FeatureFlagValidationServiceClient _featureflagsFeatureFlagValidationService;
    private readonly ApiHelpdesk.TicketWorkflowService.TicketWorkflowServiceClient _helpdeskTicketWorkflowService;
    private readonly ILogger<OrgUnitApprovalEndpoint> _logger;
    private readonly ApiLoyalty.LoyaltyMemberQueryService.LoyaltyMemberQueryServiceClient _loyaltyLoyaltyMemberQueryService;
    private readonly IOrgUnitApprovalEndpointSendEscalateOrgUnit2Port _orgUnitApprovalEndpointSendEscalateOrgUnit2Port;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiScheduling.AvailabilityWindowWorkflowService.AvailabilityWindowWorkflowServiceClient _schedulingAvailabilityWindowWorkflowService;
    private readonly ApiScheduling.RosterWorkflowService.RosterWorkflowServiceClient _schedulingRosterWorkflowService;
    private readonly Func<ApiEmployees.PreviewEmploymentContractRequest, CallOptions, AsyncUnaryCall<ApiEmployees.PreviewEmploymentContractResponse>> _sendCommentOrgUnitApproval0;
    private readonly IOptions<MeshOptions> _settings;

    public OrgUnitApprovalEndpoint(
        NotificationBus bus,
        ApiDocuments.DocumentFolderSyncService.DocumentFolderSyncServiceClient documentsDocumentFolderSyncService,
        ApiDocuments.DocumentQueryService.DocumentQueryServiceClient documentsDocumentQueryService,
        ApiDocuments.DocumentTemplateHistoryService.DocumentTemplateHistoryServiceClient documentsDocumentTemplateHistoryService,
        ApiEmployees.EmployeeApprovalService.EmployeeApprovalServiceClient employeesEmployeeApprovalService,
        ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient employeesEmployeeQueryService,
        ApiEmployees.EmployeeService.EmployeeServiceClient employeesEmployeeService,
        ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient employeesEmploymentContractCalculationService,
        ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient employeesEmploymentContractValidationService,
        ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient employeesOrgUnitQueryService,
        ApiEmployees.PositionValidationService.PositionValidationServiceClient employeesPositionValidationService,
        ApiExperiments.ExperimentQueryService.ExperimentQueryServiceClient experimentsExperimentQueryService,
        ApiFeatureflags.FeatureFlagQueryService.FeatureFlagQueryServiceClient featureflagsFeatureFlagQueryService,
        ApiFeatureflags.FeatureFlagValidationService.FeatureFlagValidationServiceClient featureflagsFeatureFlagValidationService,
        ApiHelpdesk.TicketWorkflowService.TicketWorkflowServiceClient helpdeskTicketWorkflowService,
        ILogger<OrgUnitApprovalEndpoint> logger,
        ApiLoyalty.LoyaltyMemberQueryService.LoyaltyMemberQueryServiceClient loyaltyLoyaltyMemberQueryService,
        IOrgUnitApprovalEndpointSendEscalateOrgUnit2Port orgUnitApprovalEndpointSendEscalateOrgUnit2PortAdapter,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiScheduling.AvailabilityWindowWorkflowService.AvailabilityWindowWorkflowServiceClient schedulingAvailabilityWindowWorkflowService,
        ApiScheduling.RosterWorkflowService.RosterWorkflowServiceClient schedulingRosterWorkflowService,
        IOptions<MeshOptions> settings)
    {
        _bus = bus;
        _documentsDocumentFolderSyncService = documentsDocumentFolderSyncService;
        _documentsDocumentQueryService = documentsDocumentQueryService;
        _documentsDocumentTemplateHistoryService = documentsDocumentTemplateHistoryService;
        _employeesEmployeeApprovalService = employeesEmployeeApprovalService;
        _employeesEmployeeQueryService = employeesEmployeeQueryService;
        _employeesEmployeeService = employeesEmployeeService;
        _employeesEmploymentContractCalculationService = employeesEmploymentContractCalculationService;
        _employeesEmploymentContractValidationService = employeesEmploymentContractValidationService;
        _employeesOrgUnitQueryService = employeesOrgUnitQueryService;
        _employeesPositionValidationService = employeesPositionValidationService;
        _experimentsExperimentQueryService = experimentsExperimentQueryService;
        _featureflagsFeatureFlagQueryService = featureflagsFeatureFlagQueryService;
        _featureflagsFeatureFlagValidationService = featureflagsFeatureFlagValidationService;
        _helpdeskTicketWorkflowService = helpdeskTicketWorkflowService;
        _logger = logger;
        _loyaltyLoyaltyMemberQueryService = loyaltyLoyaltyMemberQueryService;
        _orgUnitApprovalEndpointSendEscalateOrgUnit2Port = orgUnitApprovalEndpointSendEscalateOrgUnit2PortAdapter;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _schedulingAvailabilityWindowWorkflowService = schedulingAvailabilityWindowWorkflowService;
        _schedulingRosterWorkflowService = schedulingRosterWorkflowService;
        _settings = settings;
        _sendCommentOrgUnitApproval0 = _employeesEmploymentContractValidationService.PreviewEmploymentContractAsync;
    }
}
