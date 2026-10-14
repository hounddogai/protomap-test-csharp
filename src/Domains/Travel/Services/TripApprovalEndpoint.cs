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
using ApiDevices = Acme.Devices.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPerformers = Acme.Performers.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

/// <summary>
/// Routes trips through approval, with escalation and delegation.
/// </summary>
public sealed partial class TripApprovalEndpoint : ApiTravel.TripApprovalService.TripApprovalServiceBase
{
    private readonly ApiDevices.DevicePolicyWorkflowService.DevicePolicyWorkflowServiceClient _devicesDevicePolicyWorkflowService;
    private readonly ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient _employeesEmployeeQueryService;
    private readonly ApiEmployees.EmployeeService.EmployeeServiceClient _employeesEmployeeService;
    private readonly ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient _employeesEmploymentContractCalculationService;
    private readonly ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient _employeesEmploymentContractValidationService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.ProblemApprovalService.ProblemApprovalServiceClient _itsmProblemApprovalService;
    private readonly ApiLicensing.LicenseAssignmentWorkflowService.LicenseAssignmentWorkflowServiceClient _licensingLicenseAssignmentWorkflowService;
    private readonly ILogger<TripApprovalEndpoint> _logger;
    private readonly ApiPerformers.PerformerQueryService.PerformerQueryServiceClient _performersPerformerQueryService;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiTravel.QuoteTravelPolicyRequest, CallOptions, AsyncUnaryCall<ApiTravel.QuoteTravelPolicyResponse>> _sendDelegateTripApproval1;
    private readonly ApiTravel.ItinerarySyncService.ItinerarySyncServiceClient _travelItinerarySyncService;
    private readonly ApiTravel.TravelBookingReportingService.TravelBookingReportingServiceClient _travelTravelBookingReportingService;
    private readonly ApiTravel.TravelPolicyCalculationService.TravelPolicyCalculationServiceClient _travelTravelPolicyCalculationService;
    private readonly ApiTravel.TravelPolicyReportingService.TravelPolicyReportingServiceClient _travelTravelPolicyReportingService;
    private readonly ApiTravel.TripService.TripServiceClient _travelTripService;
    private readonly ITripApprovalEndpointSendRecallTrip0Port _tripApprovalEndpointSendRecallTrip0Port;

    public TripApprovalEndpoint(
        ApiDevices.DevicePolicyWorkflowService.DevicePolicyWorkflowServiceClient devicesDevicePolicyWorkflowService,
        ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient employeesEmployeeQueryService,
        ApiEmployees.EmployeeService.EmployeeServiceClient employeesEmployeeService,
        ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient employeesEmploymentContractCalculationService,
        ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient employeesEmploymentContractValidationService,
        CallInvoker invoker,
        ApiItsm.ProblemApprovalService.ProblemApprovalServiceClient itsmProblemApprovalService,
        ApiLicensing.LicenseAssignmentWorkflowService.LicenseAssignmentWorkflowServiceClient licensingLicenseAssignmentWorkflowService,
        ILogger<TripApprovalEndpoint> logger,
        ApiPerformers.PerformerQueryService.PerformerQueryServiceClient performersPerformerQueryService,
        IRecordRepository repository,
        ApiTravel.ItinerarySyncService.ItinerarySyncServiceClient travelItinerarySyncService,
        ApiTravel.TravelBookingReportingService.TravelBookingReportingServiceClient travelTravelBookingReportingService,
        ApiTravel.TravelPolicyCalculationService.TravelPolicyCalculationServiceClient travelTravelPolicyCalculationService,
        ApiTravel.TravelPolicyReportingService.TravelPolicyReportingServiceClient travelTravelPolicyReportingService,
        ApiTravel.TripService.TripServiceClient travelTripService,
        ITripApprovalEndpointSendRecallTrip0Port tripApprovalEndpointSendRecallTrip0PortAdapter)
    {
        _devicesDevicePolicyWorkflowService = devicesDevicePolicyWorkflowService;
        _employeesEmployeeQueryService = employeesEmployeeQueryService;
        _employeesEmployeeService = employeesEmployeeService;
        _employeesEmploymentContractCalculationService = employeesEmploymentContractCalculationService;
        _employeesEmploymentContractValidationService = employeesEmploymentContractValidationService;
        _invoker = invoker;
        _itsmProblemApprovalService = itsmProblemApprovalService;
        _licensingLicenseAssignmentWorkflowService = licensingLicenseAssignmentWorkflowService;
        _logger = logger;
        _performersPerformerQueryService = performersPerformerQueryService;
        _repository = repository;
        _travelItinerarySyncService = travelItinerarySyncService;
        _travelTravelBookingReportingService = travelTravelBookingReportingService;
        _travelTravelPolicyCalculationService = travelTravelPolicyCalculationService;
        _travelTravelPolicyReportingService = travelTravelPolicyReportingService;
        _travelTripService = travelTripService;
        _tripApprovalEndpointSendRecallTrip0Port = tripApprovalEndpointSendRecallTrip0PortAdapter;
        _sendDelegateTripApproval1 = _travelTravelPolicyCalculationService.QuoteTravelPolicyAsync;
    }
}
