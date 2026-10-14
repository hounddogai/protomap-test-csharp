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
using ApiScheduling = Acme.Scheduling.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Scheduling;

/// <summary>
/// Routes availability windows through approval, with escalation and delegation.
/// </summary>
public sealed partial class AvailabilityWindowApprovalEndpoint : ApiScheduling.AvailabilityWindowApprovalService.AvailabilityWindowApprovalServiceBase
{
    private readonly IAvailabilityWindowApprovalEndpointSendRecallAvailabilityWindow1Port _availabilityWindowApprovalEndpointSendRecallAvailabilityWindow1Port;
    private readonly NotificationBus _bus;
    private readonly ApiCorporateevents.VenueApprovalService.VenueApprovalServiceClient _corporateeventsVenueApprovalService;
    private readonly ApiDocuments.DocumentTemplateWorkflowService.DocumentTemplateWorkflowServiceClient _documentsDocumentTemplateWorkflowService;
    private readonly ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient _employeesEmployeeQueryService;
    private readonly ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient _employeesEmploymentContractCalculationService;
    private readonly ApiEmployees.PositionValidationService.PositionValidationServiceClient _employeesPositionValidationService;
    private readonly ILogger<AvailabilityWindowApprovalEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiScheduling.AvailabilityWindowCalculationService.AvailabilityWindowCalculationServiceClient _schedulingAvailabilityWindowCalculationService;
    private readonly ApiScheduling.AvailabilityWindowLifecycleService.AvailabilityWindowLifecycleServiceClient _schedulingAvailabilityWindowLifecycleService;
    private readonly ApiScheduling.AvailabilityWindowValidationService.AvailabilityWindowValidationServiceClient _schedulingAvailabilityWindowValidationService;
    private readonly ApiScheduling.RosterApprovalService.RosterApprovalServiceClient _schedulingRosterApprovalService;
    private readonly ApiScheduling.RosterCalculationService.RosterCalculationServiceClient _schedulingRosterCalculationService;
    private readonly ApiScheduling.RosterQueryService.RosterQueryServiceClient _schedulingRosterQueryService;
    private readonly ApiScheduling.ShiftApprovalService.ShiftApprovalServiceClient _schedulingShiftApprovalService;
    private readonly ApiScheduling.ShiftService.ShiftServiceClient _schedulingShiftService;
    private readonly ApiScheduling.ShiftValidationService.ShiftValidationServiceClient _schedulingShiftValidationService;
    private readonly ApiWorkplaces.BuildingWorkflowService.BuildingWorkflowServiceClient _workplacesBuildingWorkflowService;

    public AvailabilityWindowApprovalEndpoint(
        IAvailabilityWindowApprovalEndpointSendRecallAvailabilityWindow1Port availabilityWindowApprovalEndpointSendRecallAvailabilityWindow1PortAdapter,
        NotificationBus bus,
        ApiCorporateevents.VenueApprovalService.VenueApprovalServiceClient corporateeventsVenueApprovalService,
        ApiDocuments.DocumentTemplateWorkflowService.DocumentTemplateWorkflowServiceClient documentsDocumentTemplateWorkflowService,
        ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient employeesEmployeeQueryService,
        ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient employeesEmploymentContractCalculationService,
        ApiEmployees.PositionValidationService.PositionValidationServiceClient employeesPositionValidationService,
        ILogger<AvailabilityWindowApprovalEndpoint> logger,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiScheduling.AvailabilityWindowCalculationService.AvailabilityWindowCalculationServiceClient schedulingAvailabilityWindowCalculationService,
        ApiScheduling.AvailabilityWindowLifecycleService.AvailabilityWindowLifecycleServiceClient schedulingAvailabilityWindowLifecycleService,
        ApiScheduling.AvailabilityWindowValidationService.AvailabilityWindowValidationServiceClient schedulingAvailabilityWindowValidationService,
        ApiScheduling.RosterApprovalService.RosterApprovalServiceClient schedulingRosterApprovalService,
        ApiScheduling.RosterCalculationService.RosterCalculationServiceClient schedulingRosterCalculationService,
        ApiScheduling.RosterQueryService.RosterQueryServiceClient schedulingRosterQueryService,
        ApiScheduling.ShiftApprovalService.ShiftApprovalServiceClient schedulingShiftApprovalService,
        ApiScheduling.ShiftService.ShiftServiceClient schedulingShiftService,
        ApiScheduling.ShiftValidationService.ShiftValidationServiceClient schedulingShiftValidationService,
        ApiWorkplaces.BuildingWorkflowService.BuildingWorkflowServiceClient workplacesBuildingWorkflowService)
    {
        _availabilityWindowApprovalEndpointSendRecallAvailabilityWindow1Port = availabilityWindowApprovalEndpointSendRecallAvailabilityWindow1PortAdapter;
        _bus = bus;
        _corporateeventsVenueApprovalService = corporateeventsVenueApprovalService;
        _documentsDocumentTemplateWorkflowService = documentsDocumentTemplateWorkflowService;
        _employeesEmployeeQueryService = employeesEmployeeQueryService;
        _employeesEmploymentContractCalculationService = employeesEmploymentContractCalculationService;
        _employeesPositionValidationService = employeesPositionValidationService;
        _logger = logger;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _schedulingAvailabilityWindowCalculationService = schedulingAvailabilityWindowCalculationService;
        _schedulingAvailabilityWindowLifecycleService = schedulingAvailabilityWindowLifecycleService;
        _schedulingAvailabilityWindowValidationService = schedulingAvailabilityWindowValidationService;
        _schedulingRosterApprovalService = schedulingRosterApprovalService;
        _schedulingRosterCalculationService = schedulingRosterCalculationService;
        _schedulingRosterQueryService = schedulingRosterQueryService;
        _schedulingShiftApprovalService = schedulingShiftApprovalService;
        _schedulingShiftService = schedulingShiftService;
        _schedulingShiftValidationService = schedulingShiftValidationService;
        _workplacesBuildingWorkflowService = workplacesBuildingWorkflowService;
    }
}
