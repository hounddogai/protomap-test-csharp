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
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiSubscriptions = Acme.Subscriptions.V1;
using ApiVenues = Acme.Venues.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Employees;

/// <summary>
/// Routes positions through approval, with escalation and delegation.
/// </summary>
public sealed partial class PositionApprovalEndpoint : ApiEmployees.PositionApprovalService.PositionApprovalServiceBase
{
    private readonly ApiDocuments.DocumentFolderValidationService.DocumentFolderValidationServiceClient _documentsDocumentFolderValidationService;
    private readonly ApiDocuments.DocumentSyncService.DocumentSyncServiceClient _documentsDocumentSyncService;
    private readonly ApiDocuments.DocumentTemplateNotificationService.DocumentTemplateNotificationServiceClient _documentsDocumentTemplateNotificationService;
    private readonly ApiEmployees.EmployeeWorkflowService.EmployeeWorkflowServiceClient _employeesEmployeeWorkflowService;
    private readonly ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient _employeesEmploymentContractValidationService;
    private readonly ApiEmployees.OrgUnitApprovalService.OrgUnitApprovalServiceClient _employeesOrgUnitApprovalService;
    private readonly ApiEmployees.PositionValidationService.PositionValidationServiceClient _employeesPositionValidationService;
    private readonly ApiHelpdesk.KnowledgeArticleApprovalService.KnowledgeArticleApprovalServiceClient _helpdeskKnowledgeArticleApprovalService;
    private readonly CallInvoker _invoker;
    private readonly ILogger<PositionApprovalEndpoint> _logger;
    private readonly ApiOnboarding.EquipmentReportingService.EquipmentReportingServiceClient _onboardingEquipmentReportingService;
    private readonly IPositionApprovalEndpointSendRejectPosition0Port _positionApprovalEndpointSendRejectPosition0Port;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiEmployees.RecallOrgUnitRequest, CallOptions, AsyncUnaryCall<ApiEmployees.RecallOrgUnitResponse>> _sendApprovePosition0;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiSubscriptions.SubscriptionService.SubscriptionServiceClient _subscriptionsSubscriptionService;
    private readonly ApiVenues.VenueService.VenueServiceClient _venuesVenueService;
    private readonly ApiWorkplaces.VisitorPassApprovalService.VisitorPassApprovalServiceClient _workplacesVisitorPassApprovalService;

    public PositionApprovalEndpoint(
        ApiDocuments.DocumentFolderValidationService.DocumentFolderValidationServiceClient documentsDocumentFolderValidationService,
        ApiDocuments.DocumentSyncService.DocumentSyncServiceClient documentsDocumentSyncService,
        ApiDocuments.DocumentTemplateNotificationService.DocumentTemplateNotificationServiceClient documentsDocumentTemplateNotificationService,
        ApiEmployees.EmployeeWorkflowService.EmployeeWorkflowServiceClient employeesEmployeeWorkflowService,
        ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient employeesEmploymentContractValidationService,
        ApiEmployees.OrgUnitApprovalService.OrgUnitApprovalServiceClient employeesOrgUnitApprovalService,
        ApiEmployees.PositionValidationService.PositionValidationServiceClient employeesPositionValidationService,
        ApiHelpdesk.KnowledgeArticleApprovalService.KnowledgeArticleApprovalServiceClient helpdeskKnowledgeArticleApprovalService,
        CallInvoker invoker,
        ILogger<PositionApprovalEndpoint> logger,
        ApiOnboarding.EquipmentReportingService.EquipmentReportingServiceClient onboardingEquipmentReportingService,
        IPositionApprovalEndpointSendRejectPosition0Port positionApprovalEndpointSendRejectPosition0PortAdapter,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiSubscriptions.SubscriptionService.SubscriptionServiceClient subscriptionsSubscriptionService,
        ApiVenues.VenueService.VenueServiceClient venuesVenueService,
        ApiWorkplaces.VisitorPassApprovalService.VisitorPassApprovalServiceClient workplacesVisitorPassApprovalService)
    {
        _documentsDocumentFolderValidationService = documentsDocumentFolderValidationService;
        _documentsDocumentSyncService = documentsDocumentSyncService;
        _documentsDocumentTemplateNotificationService = documentsDocumentTemplateNotificationService;
        _employeesEmployeeWorkflowService = employeesEmployeeWorkflowService;
        _employeesEmploymentContractValidationService = employeesEmploymentContractValidationService;
        _employeesOrgUnitApprovalService = employeesOrgUnitApprovalService;
        _employeesPositionValidationService = employeesPositionValidationService;
        _helpdeskKnowledgeArticleApprovalService = helpdeskKnowledgeArticleApprovalService;
        _invoker = invoker;
        _logger = logger;
        _onboardingEquipmentReportingService = onboardingEquipmentReportingService;
        _positionApprovalEndpointSendRejectPosition0Port = positionApprovalEndpointSendRejectPosition0PortAdapter;
        _repository = repository;
        _settings = settings;
        _subscriptionsSubscriptionService = subscriptionsSubscriptionService;
        _venuesVenueService = venuesVenueService;
        _workplacesVisitorPassApprovalService = workplacesVisitorPassApprovalService;
        _sendApprovePosition0 = _employeesOrgUnitApprovalService.RecallOrgUnitAsync;
    }
}
