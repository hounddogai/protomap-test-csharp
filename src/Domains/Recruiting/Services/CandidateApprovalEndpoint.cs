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
using ApiAudit = Acme.Audit.V1;
using ApiCommon = Acme.Common.V1;
using ApiCompliance = Acme.Compliance.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiPipelines = Acme.Pipelines.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Recruiting;

/// <summary>
/// Routes candidates through approval, with escalation and delegation.
/// </summary>
public sealed partial class CandidateApprovalEndpoint : ApiRecruiting.CandidateApprovalService.CandidateApprovalServiceBase
{
    private readonly ApiAudit.AuditEventQueryService.AuditEventQueryServiceClient _auditAuditEventQueryService;
    private readonly ICandidateApprovalEndpointSendApproveCandidate0Port _candidateApprovalEndpointSendApproveCandidate0Port;
    private readonly ApiCompliance.ComplianceCaseService.ComplianceCaseServiceClient _complianceComplianceCaseService;
    private readonly ApiDocuments.DocumentFolderExportService.DocumentFolderExportServiceClient _documentsDocumentFolderExportService;
    private readonly ApiDocuments.DocumentTemplateCalculationService.DocumentTemplateCalculationServiceClient _documentsDocumentTemplateCalculationService;
    private readonly ApiEmployees.EmployeeService.EmployeeServiceClient _employeesEmployeeService;
    private readonly ApiEmployees.PositionValidationService.PositionValidationServiceClient _employeesPositionValidationService;
    private readonly ApiIdentity.UserValidationService.UserValidationServiceClient _identityUserValidationService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.ProblemLifecycleService.ProblemLifecycleServiceClient _itsmProblemLifecycleService;
    private readonly ApiItsm.ProblemSyncService.ProblemSyncServiceClient _itsmProblemSyncService;
    private readonly ApiLedger.LedgerAccountService.LedgerAccountServiceClient _ledgerLedgerAccountService;
    private readonly ILogger<CandidateApprovalEndpoint> _logger;
    private readonly ApiPipelines.PipelineService.PipelineServiceClient _pipelinesPipelineService;
    private readonly ApiRecruiting.CandidateReportingService.CandidateReportingServiceClient _recruitingCandidateReportingService;
    private readonly ApiRecruiting.InterviewReportingService.InterviewReportingServiceClient _recruitingInterviewReportingService;
    private readonly ApiRecruiting.JobOpeningApprovalService.JobOpeningApprovalServiceClient _recruitingJobOpeningApprovalService;
    private readonly ApiRecruiting.OfferLifecycleService.OfferLifecycleServiceClient _recruitingOfferLifecycleService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiRecruiting.ScheduleCandidateReportRequest, CallOptions, AsyncUnaryCall<ApiRecruiting.ScheduleCandidateReportResponse>> _sendRejectCandidate0;
    private readonly IOptions<MeshOptions> _settings;

    public CandidateApprovalEndpoint(
        ApiAudit.AuditEventQueryService.AuditEventQueryServiceClient auditAuditEventQueryService,
        ICandidateApprovalEndpointSendApproveCandidate0Port candidateApprovalEndpointSendApproveCandidate0PortAdapter,
        ApiCompliance.ComplianceCaseService.ComplianceCaseServiceClient complianceComplianceCaseService,
        ApiDocuments.DocumentFolderExportService.DocumentFolderExportServiceClient documentsDocumentFolderExportService,
        ApiDocuments.DocumentTemplateCalculationService.DocumentTemplateCalculationServiceClient documentsDocumentTemplateCalculationService,
        ApiEmployees.EmployeeService.EmployeeServiceClient employeesEmployeeService,
        ApiEmployees.PositionValidationService.PositionValidationServiceClient employeesPositionValidationService,
        ApiIdentity.UserValidationService.UserValidationServiceClient identityUserValidationService,
        CallInvoker invoker,
        ApiItsm.ProblemLifecycleService.ProblemLifecycleServiceClient itsmProblemLifecycleService,
        ApiItsm.ProblemSyncService.ProblemSyncServiceClient itsmProblemSyncService,
        ApiLedger.LedgerAccountService.LedgerAccountServiceClient ledgerLedgerAccountService,
        ILogger<CandidateApprovalEndpoint> logger,
        ApiPipelines.PipelineService.PipelineServiceClient pipelinesPipelineService,
        ApiRecruiting.CandidateReportingService.CandidateReportingServiceClient recruitingCandidateReportingService,
        ApiRecruiting.InterviewReportingService.InterviewReportingServiceClient recruitingInterviewReportingService,
        ApiRecruiting.JobOpeningApprovalService.JobOpeningApprovalServiceClient recruitingJobOpeningApprovalService,
        ApiRecruiting.OfferLifecycleService.OfferLifecycleServiceClient recruitingOfferLifecycleService,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings)
    {
        _auditAuditEventQueryService = auditAuditEventQueryService;
        _candidateApprovalEndpointSendApproveCandidate0Port = candidateApprovalEndpointSendApproveCandidate0PortAdapter;
        _complianceComplianceCaseService = complianceComplianceCaseService;
        _documentsDocumentFolderExportService = documentsDocumentFolderExportService;
        _documentsDocumentTemplateCalculationService = documentsDocumentTemplateCalculationService;
        _employeesEmployeeService = employeesEmployeeService;
        _employeesPositionValidationService = employeesPositionValidationService;
        _identityUserValidationService = identityUserValidationService;
        _invoker = invoker;
        _itsmProblemLifecycleService = itsmProblemLifecycleService;
        _itsmProblemSyncService = itsmProblemSyncService;
        _ledgerLedgerAccountService = ledgerLedgerAccountService;
        _logger = logger;
        _pipelinesPipelineService = pipelinesPipelineService;
        _recruitingCandidateReportingService = recruitingCandidateReportingService;
        _recruitingInterviewReportingService = recruitingInterviewReportingService;
        _recruitingJobOpeningApprovalService = recruitingJobOpeningApprovalService;
        _recruitingOfferLifecycleService = recruitingOfferLifecycleService;
        _registry = registry;
        _repository = repository;
        _settings = settings;
        _sendRejectCandidate0 = _recruitingCandidateReportingService.ScheduleCandidateReportAsync;
    }
}
