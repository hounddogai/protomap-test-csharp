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
using ApiDeliveries = Acme.Deliveries.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiGiftcards = Acme.Giftcards.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Recruiting;

/// <summary>
/// Keeps interviews in step with the systems of record in other regions.
/// </summary>
public sealed partial class InterviewSyncEndpoint : ApiRecruiting.InterviewSyncService.InterviewSyncServiceBase
{
    private readonly ApiAuthorization.AccessPolicyService.AccessPolicyServiceClient _authorizationAccessPolicyService;
    private readonly ApiDeliveries.DeliveryService.DeliveryServiceClient _deliveriesDeliveryService;
    private readonly ApiDocuments.DocumentTemplateNotificationService.DocumentTemplateNotificationServiceClient _documentsDocumentTemplateNotificationService;
    private readonly ApiEmployees.EmployeeService.EmployeeServiceClient _employeesEmployeeService;
    private readonly ApiGiftcards.GiftCardQueryService.GiftCardQueryServiceClient _giftcardsGiftCardQueryService;
    private readonly IInterviewSyncEndpointSendPushInterview0Port _interviewSyncEndpointSendPushInterview0Port;
    private readonly CallInvoker _invoker;
    private readonly ILogger<InterviewSyncEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly ApiRecruiting.CandidateQueryService.CandidateQueryServiceClient _recruitingCandidateQueryService;
    private readonly ApiRecruiting.InterviewCalculationService.InterviewCalculationServiceClient _recruitingInterviewCalculationService;
    private readonly ApiRecruiting.InterviewValidationService.InterviewValidationServiceClient _recruitingInterviewValidationService;
    private readonly ApiRecruiting.OfferCalculationService.OfferCalculationServiceClient _recruitingOfferCalculationService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiScheduling.ShiftWorkflowService.ShiftWorkflowServiceClient _schedulingShiftWorkflowService;
    private readonly ApiScheduling.SwapRequestApprovalService.SwapRequestApprovalServiceClient _schedulingSwapRequestApprovalService;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiWorkplaces.DeskWorkflowService.DeskWorkflowServiceClient _workplacesDeskWorkflowService;

    public InterviewSyncEndpoint(
        ApiAuthorization.AccessPolicyService.AccessPolicyServiceClient authorizationAccessPolicyService,
        ApiDeliveries.DeliveryService.DeliveryServiceClient deliveriesDeliveryService,
        ApiDocuments.DocumentTemplateNotificationService.DocumentTemplateNotificationServiceClient documentsDocumentTemplateNotificationService,
        ApiEmployees.EmployeeService.EmployeeServiceClient employeesEmployeeService,
        ApiGiftcards.GiftCardQueryService.GiftCardQueryServiceClient giftcardsGiftCardQueryService,
        IInterviewSyncEndpointSendPushInterview0Port interviewSyncEndpointSendPushInterview0PortAdapter,
        CallInvoker invoker,
        ILogger<InterviewSyncEndpoint> logger,
        RetryPolicy policy,
        ApiRecruiting.CandidateQueryService.CandidateQueryServiceClient recruitingCandidateQueryService,
        ApiRecruiting.InterviewCalculationService.InterviewCalculationServiceClient recruitingInterviewCalculationService,
        ApiRecruiting.InterviewValidationService.InterviewValidationServiceClient recruitingInterviewValidationService,
        ApiRecruiting.OfferCalculationService.OfferCalculationServiceClient recruitingOfferCalculationService,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiScheduling.ShiftWorkflowService.ShiftWorkflowServiceClient schedulingShiftWorkflowService,
        ApiScheduling.SwapRequestApprovalService.SwapRequestApprovalServiceClient schedulingSwapRequestApprovalService,
        IOptions<MeshOptions> settings,
        ApiWorkplaces.DeskWorkflowService.DeskWorkflowServiceClient workplacesDeskWorkflowService)
    {
        _authorizationAccessPolicyService = authorizationAccessPolicyService;
        _deliveriesDeliveryService = deliveriesDeliveryService;
        _documentsDocumentTemplateNotificationService = documentsDocumentTemplateNotificationService;
        _employeesEmployeeService = employeesEmployeeService;
        _giftcardsGiftCardQueryService = giftcardsGiftCardQueryService;
        _interviewSyncEndpointSendPushInterview0Port = interviewSyncEndpointSendPushInterview0PortAdapter;
        _invoker = invoker;
        _logger = logger;
        _policy = policy;
        _recruitingCandidateQueryService = recruitingCandidateQueryService;
        _recruitingInterviewCalculationService = recruitingInterviewCalculationService;
        _recruitingInterviewValidationService = recruitingInterviewValidationService;
        _recruitingOfferCalculationService = recruitingOfferCalculationService;
        _registry = registry;
        _repository = repository;
        _schedulingShiftWorkflowService = schedulingShiftWorkflowService;
        _schedulingSwapRequestApprovalService = schedulingSwapRequestApprovalService;
        _settings = settings;
        _workplacesDeskWorkflowService = workplacesDeskWorkflowService;
    }
}
