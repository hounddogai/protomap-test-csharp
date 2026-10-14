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
using ApiIdentity = Acme.Identity.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Recruiting;

/// <summary>
/// Runs the long-lived workflow that takes a candidate from start to finish.
/// </summary>
public sealed partial class CandidateWorkflowEndpoint : ApiRecruiting.CandidateWorkflowService.CandidateWorkflowServiceBase
{
    private readonly ICandidateWorkflowEndpointSendListCandidateWorkflows0Port _candidateWorkflowEndpointSendListCandidateWorkflows0Port;
    private readonly ApiDocuments.SignatureHistoryService.SignatureHistoryServiceClient _documentsSignatureHistoryService;
    private readonly ApiEmployees.EmployeeService.EmployeeServiceClient _employeesEmployeeService;
    private readonly ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient _employeesEmploymentContractCalculationService;
    private readonly ApiEmployees.PositionValidationService.PositionValidationServiceClient _employeesPositionValidationService;
    private readonly ApiIdentity.UserQueryService.UserQueryServiceClient _identityUserQueryService;
    private readonly CallInvoker _invoker;
    private readonly ILogger<CandidateWorkflowEndpoint> _logger;
    private readonly ApiRecruiting.CandidateApprovalService.CandidateApprovalServiceClient _recruitingCandidateApprovalService;
    private readonly ApiRecruiting.CandidateLifecycleService.CandidateLifecycleServiceClient _recruitingCandidateLifecycleService;
    private readonly ApiRecruiting.InterviewSyncService.InterviewSyncServiceClient _recruitingInterviewSyncService;
    private readonly ApiRecruiting.JobOpeningWorkflowService.JobOpeningWorkflowServiceClient _recruitingJobOpeningWorkflowService;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiEmployees.CheckPositionEligibilityRequest, CallOptions, AsyncUnaryCall<ApiEmployees.CheckPositionEligibilityResponse>> _sendStartCandidateWorkflow3;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTimekeeping.OvertimeWorkflowService.OvertimeWorkflowServiceClient _timekeepingOvertimeWorkflowService;
    private readonly ApiVenues.VenueService.VenueServiceClient _venuesVenueService;

    public CandidateWorkflowEndpoint(
        ICandidateWorkflowEndpointSendListCandidateWorkflows0Port candidateWorkflowEndpointSendListCandidateWorkflows0PortAdapter,
        ApiDocuments.SignatureHistoryService.SignatureHistoryServiceClient documentsSignatureHistoryService,
        ApiEmployees.EmployeeService.EmployeeServiceClient employeesEmployeeService,
        ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient employeesEmploymentContractCalculationService,
        ApiEmployees.PositionValidationService.PositionValidationServiceClient employeesPositionValidationService,
        ApiIdentity.UserQueryService.UserQueryServiceClient identityUserQueryService,
        CallInvoker invoker,
        ILogger<CandidateWorkflowEndpoint> logger,
        ApiRecruiting.CandidateApprovalService.CandidateApprovalServiceClient recruitingCandidateApprovalService,
        ApiRecruiting.CandidateLifecycleService.CandidateLifecycleServiceClient recruitingCandidateLifecycleService,
        ApiRecruiting.InterviewSyncService.InterviewSyncServiceClient recruitingInterviewSyncService,
        ApiRecruiting.JobOpeningWorkflowService.JobOpeningWorkflowServiceClient recruitingJobOpeningWorkflowService,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiTimekeeping.OvertimeWorkflowService.OvertimeWorkflowServiceClient timekeepingOvertimeWorkflowService,
        ApiVenues.VenueService.VenueServiceClient venuesVenueService)
    {
        _candidateWorkflowEndpointSendListCandidateWorkflows0Port = candidateWorkflowEndpointSendListCandidateWorkflows0PortAdapter;
        _documentsSignatureHistoryService = documentsSignatureHistoryService;
        _employeesEmployeeService = employeesEmployeeService;
        _employeesEmploymentContractCalculationService = employeesEmploymentContractCalculationService;
        _employeesPositionValidationService = employeesPositionValidationService;
        _identityUserQueryService = identityUserQueryService;
        _invoker = invoker;
        _logger = logger;
        _recruitingCandidateApprovalService = recruitingCandidateApprovalService;
        _recruitingCandidateLifecycleService = recruitingCandidateLifecycleService;
        _recruitingInterviewSyncService = recruitingInterviewSyncService;
        _recruitingJobOpeningWorkflowService = recruitingJobOpeningWorkflowService;
        _repository = repository;
        _settings = settings;
        _timekeepingOvertimeWorkflowService = timekeepingOvertimeWorkflowService;
        _venuesVenueService = venuesVenueService;
        _sendStartCandidateWorkflow3 = _employeesPositionValidationService.CheckPositionEligibilityAsync;
    }
}
