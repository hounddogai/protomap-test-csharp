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
using ApiAdmissions = Acme.Admissions.V1;
using ApiBenefits = Acme.Benefits.V1;
using ApiCommon = Acme.Common.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiSellers = Acme.Sellers.V1;

namespace Acme.Workforce.Scheduling;

/// <summary>
/// Moves a swap request between the states of its lifecycle.
/// </summary>
public sealed partial class SwapRequestLifecycleEndpoint : ApiScheduling.SwapRequestLifecycleService.SwapRequestLifecycleServiceBase
{
    private readonly ApiAdmissions.AdmissionService.AdmissionServiceClient _admissionsAdmissionService;
    private readonly ApiBenefits.EnrollmentWorkflowService.EnrollmentWorkflowServiceClient _benefitsEnrollmentWorkflowService;
    private readonly ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient _employeesEmploymentContractCalculationService;
    private readonly ApiIdentity.UserValidationService.UserValidationServiceClient _identityUserValidationService;
    private readonly CallInvoker _invoker;
    private readonly ApiLedger.LedgerAccountService.LedgerAccountServiceClient _ledgerLedgerAccountService;
    private readonly ILogger<SwapRequestLifecycleEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly IRecordRepository _repository;
    private readonly ApiScheduling.AvailabilityWindowCalculationService.AvailabilityWindowCalculationServiceClient _schedulingAvailabilityWindowCalculationService;
    private readonly ApiScheduling.ShiftQueryService.ShiftQueryServiceClient _schedulingShiftQueryService;
    private readonly ApiScheduling.ShiftService.ShiftServiceClient _schedulingShiftService;
    private readonly ApiSellers.SellerService.SellerServiceClient _sellersSellerService;
    private readonly Func<ApiLedger.CloneLedgerAccountRequest, CallOptions, AsyncUnaryCall<ApiLedger.CloneLedgerAccountResponse>> _sendScheduleSwapRequestExpiry1;
    private readonly IOptions<MeshOptions> _settings;

    public SwapRequestLifecycleEndpoint(
        ApiAdmissions.AdmissionService.AdmissionServiceClient admissionsAdmissionService,
        ApiBenefits.EnrollmentWorkflowService.EnrollmentWorkflowServiceClient benefitsEnrollmentWorkflowService,
        ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient employeesEmploymentContractCalculationService,
        ApiIdentity.UserValidationService.UserValidationServiceClient identityUserValidationService,
        CallInvoker invoker,
        ApiLedger.LedgerAccountService.LedgerAccountServiceClient ledgerLedgerAccountService,
        ILogger<SwapRequestLifecycleEndpoint> logger,
        RetryPolicy policy,
        IRecordRepository repository,
        ApiScheduling.AvailabilityWindowCalculationService.AvailabilityWindowCalculationServiceClient schedulingAvailabilityWindowCalculationService,
        ApiScheduling.ShiftQueryService.ShiftQueryServiceClient schedulingShiftQueryService,
        ApiScheduling.ShiftService.ShiftServiceClient schedulingShiftService,
        ApiSellers.SellerService.SellerServiceClient sellersSellerService,
        IOptions<MeshOptions> settings)
    {
        _admissionsAdmissionService = admissionsAdmissionService;
        _benefitsEnrollmentWorkflowService = benefitsEnrollmentWorkflowService;
        _employeesEmploymentContractCalculationService = employeesEmploymentContractCalculationService;
        _identityUserValidationService = identityUserValidationService;
        _invoker = invoker;
        _ledgerLedgerAccountService = ledgerLedgerAccountService;
        _logger = logger;
        _policy = policy;
        _repository = repository;
        _schedulingAvailabilityWindowCalculationService = schedulingAvailabilityWindowCalculationService;
        _schedulingShiftQueryService = schedulingShiftQueryService;
        _schedulingShiftService = schedulingShiftService;
        _sellersSellerService = sellersSellerService;
        _settings = settings;
        _sendScheduleSwapRequestExpiry1 = _ledgerLedgerAccountService.CloneLedgerAccountAsync;
    }
}
