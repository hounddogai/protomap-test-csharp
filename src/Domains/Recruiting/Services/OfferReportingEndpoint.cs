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
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Recruiting;

/// <summary>
/// Builds and schedules reports about offers.
/// </summary>
public sealed partial class OfferReportingEndpoint : ApiRecruiting.OfferReportingService.OfferReportingServiceBase
{
    private readonly NotificationBus _bus;
    private readonly ApiCorporateevents.VenueApprovalService.VenueApprovalServiceClient _corporateeventsVenueApprovalService;
    private readonly ApiDocuments.DocumentService.DocumentServiceClient _documentsDocumentService;
    private readonly ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient _employeesEmploymentContractCalculationService;
    private readonly ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient _employeesOrgUnitQueryService;
    private readonly ApiIdentity.UserService.UserServiceClient _identityUserService;
    private readonly CallInvoker _invoker;
    private readonly ApiLedger.LedgerAccountValidationService.LedgerAccountValidationServiceClient _ledgerLedgerAccountValidationService;
    private readonly ILogger<OfferReportingEndpoint> _logger;
    private readonly ApiRecruiting.CandidateCalculationService.CandidateCalculationServiceClient _recruitingCandidateCalculationService;
    private readonly ApiRecruiting.InterviewReportingService.InterviewReportingServiceClient _recruitingInterviewReportingService;
    private readonly ApiRecruiting.InterviewSyncService.InterviewSyncServiceClient _recruitingInterviewSyncService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;

    public OfferReportingEndpoint(
        NotificationBus bus,
        ApiCorporateevents.VenueApprovalService.VenueApprovalServiceClient corporateeventsVenueApprovalService,
        ApiDocuments.DocumentService.DocumentServiceClient documentsDocumentService,
        ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient employeesEmploymentContractCalculationService,
        ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient employeesOrgUnitQueryService,
        ApiIdentity.UserService.UserServiceClient identityUserService,
        CallInvoker invoker,
        ApiLedger.LedgerAccountValidationService.LedgerAccountValidationServiceClient ledgerLedgerAccountValidationService,
        ILogger<OfferReportingEndpoint> logger,
        ApiRecruiting.CandidateCalculationService.CandidateCalculationServiceClient recruitingCandidateCalculationService,
        ApiRecruiting.InterviewReportingService.InterviewReportingServiceClient recruitingInterviewReportingService,
        ApiRecruiting.InterviewSyncService.InterviewSyncServiceClient recruitingInterviewSyncService,
        ClientRegistry registry,
        IRecordRepository repository)
    {
        _bus = bus;
        _corporateeventsVenueApprovalService = corporateeventsVenueApprovalService;
        _documentsDocumentService = documentsDocumentService;
        _employeesEmploymentContractCalculationService = employeesEmploymentContractCalculationService;
        _employeesOrgUnitQueryService = employeesOrgUnitQueryService;
        _identityUserService = identityUserService;
        _invoker = invoker;
        _ledgerLedgerAccountValidationService = ledgerLedgerAccountValidationService;
        _logger = logger;
        _recruitingCandidateCalculationService = recruitingCandidateCalculationService;
        _recruitingInterviewReportingService = recruitingInterviewReportingService;
        _recruitingInterviewSyncService = recruitingInterviewSyncService;
        _registry = registry;
        _repository = repository;
    }
}
