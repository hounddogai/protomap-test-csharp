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
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

/// <summary>
/// Builds and schedules reports about travel bookings.
/// </summary>
public sealed partial class TravelBookingReportingEndpoint : ApiTravel.TravelBookingReportingService.TravelBookingReportingServiceBase
{
    private readonly ApiAudit.AuditEventQueryService.AuditEventQueryServiceClient _auditAuditEventQueryService;
    private readonly ApiCorporateevents.RegistrationApprovalService.RegistrationApprovalServiceClient _corporateeventsRegistrationApprovalService;
    private readonly ApiDevices.ManagedDeviceApprovalService.ManagedDeviceApprovalServiceClient _devicesManagedDeviceApprovalService;
    private readonly ApiDocuments.DocumentValidationService.DocumentValidationServiceClient _documentsDocumentValidationService;
    private readonly ApiDocuments.SignatureExportService.SignatureExportServiceClient _documentsSignatureExportService;
    private readonly ApiDocuments.SignatureSyncService.SignatureSyncServiceClient _documentsSignatureSyncService;
    private readonly ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient _employeesEmployeeQueryService;
    private readonly ApiEmployees.EmployeeService.EmployeeServiceClient _employeesEmployeeService;
    private readonly ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient _employeesEmploymentContractValidationService;
    private readonly ApiHelpdesk.KnowledgeArticleWorkflowService.KnowledgeArticleWorkflowServiceClient _helpdeskKnowledgeArticleWorkflowService;
    private readonly CallInvoker _invoker;
    private readonly ILogger<TravelBookingReportingEndpoint> _logger;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiEmployees.CountEmployeesRequest, CallOptions, AsyncUnaryCall<ApiEmployees.CountEmployeesResponse>> _sendCountTravelBookingTotals0;
    private readonly ITravelBookingReportingEndpointSendScheduleTravelBookingReport0Port _travelBookingReportingEndpointSendScheduleTravelBookingReport0Port;
    private readonly ApiTravel.TravelBookingCalculationService.TravelBookingCalculationServiceClient _travelTravelBookingCalculationService;
    private readonly ApiTravel.TravelPolicySyncService.TravelPolicySyncServiceClient _travelTravelPolicySyncService;
    private readonly ApiTravel.TripValidationService.TripValidationServiceClient _travelTripValidationService;

    public TravelBookingReportingEndpoint(
        ApiAudit.AuditEventQueryService.AuditEventQueryServiceClient auditAuditEventQueryService,
        ApiCorporateevents.RegistrationApprovalService.RegistrationApprovalServiceClient corporateeventsRegistrationApprovalService,
        ApiDevices.ManagedDeviceApprovalService.ManagedDeviceApprovalServiceClient devicesManagedDeviceApprovalService,
        ApiDocuments.DocumentValidationService.DocumentValidationServiceClient documentsDocumentValidationService,
        ApiDocuments.SignatureExportService.SignatureExportServiceClient documentsSignatureExportService,
        ApiDocuments.SignatureSyncService.SignatureSyncServiceClient documentsSignatureSyncService,
        ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient employeesEmployeeQueryService,
        ApiEmployees.EmployeeService.EmployeeServiceClient employeesEmployeeService,
        ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient employeesEmploymentContractValidationService,
        ApiHelpdesk.KnowledgeArticleWorkflowService.KnowledgeArticleWorkflowServiceClient helpdeskKnowledgeArticleWorkflowService,
        CallInvoker invoker,
        ILogger<TravelBookingReportingEndpoint> logger,
        ClientRegistry registry,
        IRecordRepository repository,
        ITravelBookingReportingEndpointSendScheduleTravelBookingReport0Port travelBookingReportingEndpointSendScheduleTravelBookingReport0PortAdapter,
        ApiTravel.TravelBookingCalculationService.TravelBookingCalculationServiceClient travelTravelBookingCalculationService,
        ApiTravel.TravelPolicySyncService.TravelPolicySyncServiceClient travelTravelPolicySyncService,
        ApiTravel.TripValidationService.TripValidationServiceClient travelTripValidationService)
    {
        _auditAuditEventQueryService = auditAuditEventQueryService;
        _corporateeventsRegistrationApprovalService = corporateeventsRegistrationApprovalService;
        _devicesManagedDeviceApprovalService = devicesManagedDeviceApprovalService;
        _documentsDocumentValidationService = documentsDocumentValidationService;
        _documentsSignatureExportService = documentsSignatureExportService;
        _documentsSignatureSyncService = documentsSignatureSyncService;
        _employeesEmployeeQueryService = employeesEmployeeQueryService;
        _employeesEmployeeService = employeesEmployeeService;
        _employeesEmploymentContractValidationService = employeesEmploymentContractValidationService;
        _helpdeskKnowledgeArticleWorkflowService = helpdeskKnowledgeArticleWorkflowService;
        _invoker = invoker;
        _logger = logger;
        _registry = registry;
        _repository = repository;
        _travelBookingReportingEndpointSendScheduleTravelBookingReport0Port = travelBookingReportingEndpointSendScheduleTravelBookingReport0PortAdapter;
        _travelTravelBookingCalculationService = travelTravelBookingCalculationService;
        _travelTravelPolicySyncService = travelTravelPolicySyncService;
        _travelTripValidationService = travelTripValidationService;
        _sendCountTravelBookingTotals0 = _employeesEmployeeQueryService.CountEmployeesAsync;
    }
}
