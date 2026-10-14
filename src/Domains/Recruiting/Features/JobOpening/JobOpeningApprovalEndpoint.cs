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
using ApiBanking = Acme.Banking.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Recruiting;

/// <summary>
/// Routes job openings through approval, with escalation and delegation.
/// </summary>
public sealed partial class JobOpeningApprovalEndpoint : ApiRecruiting.JobOpeningApprovalService.JobOpeningApprovalServiceBase
{
    private readonly ApiBanking.BankAccountQueryService.BankAccountQueryServiceClient _bankingBankAccountQueryService;
    private readonly ApiCorporateevents.RegistrationReportingService.RegistrationReportingServiceClient _corporateeventsRegistrationReportingService;
    private readonly ApiDocuments.DocumentService.DocumentServiceClient _documentsDocumentService;
    private readonly ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient _documentsDocumentTemplateSyncService;
    private readonly ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient _employeesEmployeeQueryService;
    private readonly ApiEmployees.EmployeeService.EmployeeServiceClient _employeesEmployeeService;
    private readonly ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient _employeesEmploymentContractValidationService;
    private readonly ApiEmployees.PositionValidationService.PositionValidationServiceClient _employeesPositionValidationService;
    private readonly CallInvoker _invoker;
    private readonly IJobOpeningApprovalEndpointSendListPendingJobOpenings0Port _jobOpeningApprovalEndpointSendListPendingJobOpenings0Port;
    private readonly ILogger<JobOpeningApprovalEndpoint> _logger;
    private readonly ApiMetrics.MetricSeriesValidationService.MetricSeriesValidationServiceClient _metricsMetricSeriesValidationService;
    private readonly ApiRecruiting.CandidateCalculationService.CandidateCalculationServiceClient _recruitingCandidateCalculationService;
    private readonly ApiRecruiting.CandidateReportingService.CandidateReportingServiceClient _recruitingCandidateReportingService;
    private readonly ApiRecruiting.CandidateValidationService.CandidateValidationServiceClient _recruitingCandidateValidationService;
    private readonly ApiRecruiting.InterviewCalculationService.InterviewCalculationServiceClient _recruitingInterviewCalculationService;
    private readonly ApiRecruiting.InterviewSyncService.InterviewSyncServiceClient _recruitingInterviewSyncService;
    private readonly ApiRecruiting.JobOpeningLifecycleService.JobOpeningLifecycleServiceClient _recruitingJobOpeningLifecycleService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiScheduling.ShiftApprovalService.ShiftApprovalServiceClient _schedulingShiftApprovalService;
    private readonly ApiScheduling.SwapRequestApprovalService.SwapRequestApprovalServiceClient _schedulingSwapRequestApprovalService;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTimekeeping.OvertimeWorkflowService.OvertimeWorkflowServiceClient _timekeepingOvertimeWorkflowService;

    public JobOpeningApprovalEndpoint(
        ApiBanking.BankAccountQueryService.BankAccountQueryServiceClient bankingBankAccountQueryService,
        ApiCorporateevents.RegistrationReportingService.RegistrationReportingServiceClient corporateeventsRegistrationReportingService,
        ApiDocuments.DocumentService.DocumentServiceClient documentsDocumentService,
        ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient documentsDocumentTemplateSyncService,
        ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient employeesEmployeeQueryService,
        ApiEmployees.EmployeeService.EmployeeServiceClient employeesEmployeeService,
        ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient employeesEmploymentContractValidationService,
        ApiEmployees.PositionValidationService.PositionValidationServiceClient employeesPositionValidationService,
        CallInvoker invoker,
        IJobOpeningApprovalEndpointSendListPendingJobOpenings0Port jobOpeningApprovalEndpointSendListPendingJobOpenings0PortAdapter,
        ILogger<JobOpeningApprovalEndpoint> logger,
        ApiMetrics.MetricSeriesValidationService.MetricSeriesValidationServiceClient metricsMetricSeriesValidationService,
        ApiRecruiting.CandidateCalculationService.CandidateCalculationServiceClient recruitingCandidateCalculationService,
        ApiRecruiting.CandidateReportingService.CandidateReportingServiceClient recruitingCandidateReportingService,
        ApiRecruiting.CandidateValidationService.CandidateValidationServiceClient recruitingCandidateValidationService,
        ApiRecruiting.InterviewCalculationService.InterviewCalculationServiceClient recruitingInterviewCalculationService,
        ApiRecruiting.InterviewSyncService.InterviewSyncServiceClient recruitingInterviewSyncService,
        ApiRecruiting.JobOpeningLifecycleService.JobOpeningLifecycleServiceClient recruitingJobOpeningLifecycleService,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiScheduling.ShiftApprovalService.ShiftApprovalServiceClient schedulingShiftApprovalService,
        ApiScheduling.SwapRequestApprovalService.SwapRequestApprovalServiceClient schedulingSwapRequestApprovalService,
        IOptions<MeshOptions> settings,
        ApiTimekeeping.OvertimeWorkflowService.OvertimeWorkflowServiceClient timekeepingOvertimeWorkflowService)
    {
        _bankingBankAccountQueryService = bankingBankAccountQueryService;
        _corporateeventsRegistrationReportingService = corporateeventsRegistrationReportingService;
        _documentsDocumentService = documentsDocumentService;
        _documentsDocumentTemplateSyncService = documentsDocumentTemplateSyncService;
        _employeesEmployeeQueryService = employeesEmployeeQueryService;
        _employeesEmployeeService = employeesEmployeeService;
        _employeesEmploymentContractValidationService = employeesEmploymentContractValidationService;
        _employeesPositionValidationService = employeesPositionValidationService;
        _invoker = invoker;
        _jobOpeningApprovalEndpointSendListPendingJobOpenings0Port = jobOpeningApprovalEndpointSendListPendingJobOpenings0PortAdapter;
        _logger = logger;
        _metricsMetricSeriesValidationService = metricsMetricSeriesValidationService;
        _recruitingCandidateCalculationService = recruitingCandidateCalculationService;
        _recruitingCandidateReportingService = recruitingCandidateReportingService;
        _recruitingCandidateValidationService = recruitingCandidateValidationService;
        _recruitingInterviewCalculationService = recruitingInterviewCalculationService;
        _recruitingInterviewSyncService = recruitingInterviewSyncService;
        _recruitingJobOpeningLifecycleService = recruitingJobOpeningLifecycleService;
        _registry = registry;
        _repository = repository;
        _schedulingShiftApprovalService = schedulingShiftApprovalService;
        _schedulingSwapRequestApprovalService = schedulingSwapRequestApprovalService;
        _settings = settings;
        _timekeepingOvertimeWorkflowService = timekeepingOvertimeWorkflowService;
    }
}
