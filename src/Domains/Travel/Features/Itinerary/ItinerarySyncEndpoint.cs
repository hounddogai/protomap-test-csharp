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
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFraud = Acme.Fraud.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

/// <summary>
/// Keeps itineraries in step with the systems of record in other regions.
/// </summary>
public sealed partial class ItinerarySyncEndpoint : ApiTravel.ItinerarySyncService.ItinerarySyncServiceBase
{
    private readonly ApiAudit.AuditEventValidationService.AuditEventValidationServiceClient _auditAuditEventValidationService;
    private readonly NotificationBus _bus;
    private readonly ApiDevices.DevicePolicyApprovalService.DevicePolicyApprovalServiceClient _devicesDevicePolicyApprovalService;
    private readonly ApiDevices.ManagedDeviceWorkflowService.ManagedDeviceWorkflowServiceClient _devicesManagedDeviceWorkflowService;
    private readonly ApiDocuments.DocumentFolderQueryService.DocumentFolderQueryServiceClient _documentsDocumentFolderQueryService;
    private readonly ApiDocuments.SignatureCalculationService.SignatureCalculationServiceClient _documentsSignatureCalculationService;
    private readonly ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient _employeesEmploymentContractCalculationService;
    private readonly ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient _employeesEmploymentContractValidationService;
    private readonly ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient _employeesOrgUnitQueryService;
    private readonly ApiFraud.RiskAssessmentService.RiskAssessmentServiceClient _fraudRiskAssessmentService;
    private readonly CallInvoker _invoker;
    private readonly IItinerarySyncEndpointSendSyncItineraries0Port _itinerarySyncEndpointSendSyncItineraries0Port;
    private readonly ILogger<ItinerarySyncEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiTravel.ScoreTravelPolicyRequest, CallOptions, AsyncUnaryCall<ApiTravel.ScoreTravelPolicyResponse>> _sendReconcileItineraries1;
    private readonly ApiTravel.TravelBookingReportingService.TravelBookingReportingServiceClient _travelTravelBookingReportingService;
    private readonly ApiTravel.TravelPolicySyncService.TravelPolicySyncServiceClient _travelTravelPolicySyncService;
    private readonly ApiTravel.TravelPolicyValidationService.TravelPolicyValidationServiceClient _travelTravelPolicyValidationService;
    private readonly ApiTravel.TripQueryService.TripQueryServiceClient _travelTripQueryService;

    public ItinerarySyncEndpoint(
        ApiAudit.AuditEventValidationService.AuditEventValidationServiceClient auditAuditEventValidationService,
        NotificationBus bus,
        ApiDevices.DevicePolicyApprovalService.DevicePolicyApprovalServiceClient devicesDevicePolicyApprovalService,
        ApiDevices.ManagedDeviceWorkflowService.ManagedDeviceWorkflowServiceClient devicesManagedDeviceWorkflowService,
        ApiDocuments.DocumentFolderQueryService.DocumentFolderQueryServiceClient documentsDocumentFolderQueryService,
        ApiDocuments.SignatureCalculationService.SignatureCalculationServiceClient documentsSignatureCalculationService,
        ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient employeesEmploymentContractCalculationService,
        ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient employeesEmploymentContractValidationService,
        ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient employeesOrgUnitQueryService,
        ApiFraud.RiskAssessmentService.RiskAssessmentServiceClient fraudRiskAssessmentService,
        CallInvoker invoker,
        IItinerarySyncEndpointSendSyncItineraries0Port itinerarySyncEndpointSendSyncItineraries0PortAdapter,
        ILogger<ItinerarySyncEndpoint> logger,
        RetryPolicy policy,
        IRecordRepository repository,
        ApiTravel.TravelBookingReportingService.TravelBookingReportingServiceClient travelTravelBookingReportingService,
        ApiTravel.TravelPolicySyncService.TravelPolicySyncServiceClient travelTravelPolicySyncService,
        ApiTravel.TravelPolicyValidationService.TravelPolicyValidationServiceClient travelTravelPolicyValidationService,
        ApiTravel.TripQueryService.TripQueryServiceClient travelTripQueryService)
    {
        _auditAuditEventValidationService = auditAuditEventValidationService;
        _bus = bus;
        _devicesDevicePolicyApprovalService = devicesDevicePolicyApprovalService;
        _devicesManagedDeviceWorkflowService = devicesManagedDeviceWorkflowService;
        _documentsDocumentFolderQueryService = documentsDocumentFolderQueryService;
        _documentsSignatureCalculationService = documentsSignatureCalculationService;
        _employeesEmploymentContractCalculationService = employeesEmploymentContractCalculationService;
        _employeesEmploymentContractValidationService = employeesEmploymentContractValidationService;
        _employeesOrgUnitQueryService = employeesOrgUnitQueryService;
        _fraudRiskAssessmentService = fraudRiskAssessmentService;
        _invoker = invoker;
        _itinerarySyncEndpointSendSyncItineraries0Port = itinerarySyncEndpointSendSyncItineraries0PortAdapter;
        _logger = logger;
        _policy = policy;
        _repository = repository;
        _travelTravelBookingReportingService = travelTravelBookingReportingService;
        _travelTravelPolicySyncService = travelTravelPolicySyncService;
        _travelTravelPolicyValidationService = travelTravelPolicyValidationService;
        _travelTripQueryService = travelTripQueryService;
        _sendReconcileItineraries1 = _travelTravelPolicyValidationService.ScoreTravelPolicyAsync;
    }
}
