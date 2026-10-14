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
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Scheduling;

/// <summary>
/// Routes swap requests through approval, with escalation and delegation.
/// </summary>
public sealed partial class SwapRequestApprovalEndpoint : ApiScheduling.SwapRequestApprovalService.SwapRequestApprovalServiceBase
{
    private readonly ApiCorporateevents.VenueApprovalService.VenueApprovalServiceClient _corporateeventsVenueApprovalService;
    private readonly ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient _employeesEmploymentContractCalculationService;
    private readonly ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient _employeesEmploymentContractValidationService;
    private readonly ApiEmployees.PositionValidationService.PositionValidationServiceClient _employeesPositionValidationService;
    private readonly ApiLedger.LedgerAccountService.LedgerAccountServiceClient _ledgerLedgerAccountService;
    private readonly ApiLicensing.SoftwareLicenseWorkflowService.SoftwareLicenseWorkflowServiceClient _licensingSoftwareLicenseWorkflowService;
    private readonly ILogger<SwapRequestApprovalEndpoint> _logger;
    private readonly ApiPerformance.ReviewWorkflowService.ReviewWorkflowServiceClient _performanceReviewWorkflowService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiScheduling.AvailabilityWindowApprovalService.AvailabilityWindowApprovalServiceClient _schedulingAvailabilityWindowApprovalService;
    private readonly ApiScheduling.RosterApprovalService.RosterApprovalServiceClient _schedulingRosterApprovalService;
    private readonly ApiScheduling.ShiftWorkflowService.ShiftWorkflowServiceClient _schedulingShiftWorkflowService;
    private readonly ApiScheduling.SwapRequestCalculationService.SwapRequestCalculationServiceClient _schedulingSwapRequestCalculationService;
    private readonly ApiScheduling.SwapRequestValidationService.SwapRequestValidationServiceClient _schedulingSwapRequestValidationService;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ISwapRequestApprovalEndpointSendApproveSwapRequest0Port _swapRequestApprovalEndpointSendApproveSwapRequest0Port;
    private readonly ApiTravel.ItineraryReportingService.ItineraryReportingServiceClient _travelItineraryReportingService;

    public SwapRequestApprovalEndpoint(
        ApiCorporateevents.VenueApprovalService.VenueApprovalServiceClient corporateeventsVenueApprovalService,
        ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient employeesEmploymentContractCalculationService,
        ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient employeesEmploymentContractValidationService,
        ApiEmployees.PositionValidationService.PositionValidationServiceClient employeesPositionValidationService,
        ApiLedger.LedgerAccountService.LedgerAccountServiceClient ledgerLedgerAccountService,
        ApiLicensing.SoftwareLicenseWorkflowService.SoftwareLicenseWorkflowServiceClient licensingSoftwareLicenseWorkflowService,
        ILogger<SwapRequestApprovalEndpoint> logger,
        ApiPerformance.ReviewWorkflowService.ReviewWorkflowServiceClient performanceReviewWorkflowService,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiScheduling.AvailabilityWindowApprovalService.AvailabilityWindowApprovalServiceClient schedulingAvailabilityWindowApprovalService,
        ApiScheduling.RosterApprovalService.RosterApprovalServiceClient schedulingRosterApprovalService,
        ApiScheduling.ShiftWorkflowService.ShiftWorkflowServiceClient schedulingShiftWorkflowService,
        ApiScheduling.SwapRequestCalculationService.SwapRequestCalculationServiceClient schedulingSwapRequestCalculationService,
        ApiScheduling.SwapRequestValidationService.SwapRequestValidationServiceClient schedulingSwapRequestValidationService,
        IOptions<MeshOptions> settings,
        ISwapRequestApprovalEndpointSendApproveSwapRequest0Port swapRequestApprovalEndpointSendApproveSwapRequest0PortAdapter,
        ApiTravel.ItineraryReportingService.ItineraryReportingServiceClient travelItineraryReportingService)
    {
        _corporateeventsVenueApprovalService = corporateeventsVenueApprovalService;
        _employeesEmploymentContractCalculationService = employeesEmploymentContractCalculationService;
        _employeesEmploymentContractValidationService = employeesEmploymentContractValidationService;
        _employeesPositionValidationService = employeesPositionValidationService;
        _ledgerLedgerAccountService = ledgerLedgerAccountService;
        _licensingSoftwareLicenseWorkflowService = licensingSoftwareLicenseWorkflowService;
        _logger = logger;
        _performanceReviewWorkflowService = performanceReviewWorkflowService;
        _registry = registry;
        _repository = repository;
        _schedulingAvailabilityWindowApprovalService = schedulingAvailabilityWindowApprovalService;
        _schedulingRosterApprovalService = schedulingRosterApprovalService;
        _schedulingShiftWorkflowService = schedulingShiftWorkflowService;
        _schedulingSwapRequestCalculationService = schedulingSwapRequestCalculationService;
        _schedulingSwapRequestValidationService = schedulingSwapRequestValidationService;
        _settings = settings;
        _swapRequestApprovalEndpointSendApproveSwapRequest0Port = swapRequestApprovalEndpointSendApproveSwapRequest0PortAdapter;
        _travelItineraryReportingService = travelItineraryReportingService;
    }
}
