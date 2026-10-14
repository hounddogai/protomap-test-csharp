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
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

/// <summary>
/// Routes room bookings through approval, with escalation and delegation.
/// </summary>
public sealed partial class RoomBookingApprovalEndpoint : ApiWorkplaces.RoomBookingApprovalService.RoomBookingApprovalServiceBase
{
    private readonly NotificationBus _bus;
    private readonly ApiCorporateevents.VenueReportingService.VenueReportingServiceClient _corporateeventsVenueReportingService;
    private readonly ApiDocuments.DocumentFolderHistoryService.DocumentFolderHistoryServiceClient _documentsDocumentFolderHistoryService;
    private readonly ApiDocuments.DocumentFolderSyncService.DocumentFolderSyncServiceClient _documentsDocumentFolderSyncService;
    private readonly ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient _documentsDocumentTemplateSyncService;
    private readonly ApiDocuments.SignatureHistoryService.SignatureHistoryServiceClient _documentsSignatureHistoryService;
    private readonly ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient _employeesEmployeeQueryService;
    private readonly ApiEmployees.EmployeeService.EmployeeServiceClient _employeesEmployeeService;
    private readonly ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient _employeesEmploymentContractCalculationService;
    private readonly ApiLedger.LedgerAccountService.LedgerAccountServiceClient _ledgerLedgerAccountService;
    private readonly ILogger<RoomBookingApprovalEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly IRoomBookingApprovalEndpointSendRecallRoomBooking1Port _roomBookingApprovalEndpointSendRecallRoomBooking1Port;
    private readonly Func<ApiEmployees.ResolveEmployeesRequest, CallOptions, AsyncUnaryCall<ApiEmployees.ResolveEmployeesResponse>> _sendDelegateRoomBookingApproval0;
    private readonly ApiWorkplaces.BuildingWorkflowService.BuildingWorkflowServiceClient _workplacesBuildingWorkflowService;
    private readonly ApiWorkplaces.DeskApprovalService.DeskApprovalServiceClient _workplacesDeskApprovalService;
    private readonly ApiWorkplaces.VisitorPassSyncService.VisitorPassSyncServiceClient _workplacesVisitorPassSyncService;

    public RoomBookingApprovalEndpoint(
        NotificationBus bus,
        ApiCorporateevents.VenueReportingService.VenueReportingServiceClient corporateeventsVenueReportingService,
        ApiDocuments.DocumentFolderHistoryService.DocumentFolderHistoryServiceClient documentsDocumentFolderHistoryService,
        ApiDocuments.DocumentFolderSyncService.DocumentFolderSyncServiceClient documentsDocumentFolderSyncService,
        ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient documentsDocumentTemplateSyncService,
        ApiDocuments.SignatureHistoryService.SignatureHistoryServiceClient documentsSignatureHistoryService,
        ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient employeesEmployeeQueryService,
        ApiEmployees.EmployeeService.EmployeeServiceClient employeesEmployeeService,
        ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient employeesEmploymentContractCalculationService,
        ApiLedger.LedgerAccountService.LedgerAccountServiceClient ledgerLedgerAccountService,
        ILogger<RoomBookingApprovalEndpoint> logger,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository,
        IRoomBookingApprovalEndpointSendRecallRoomBooking1Port roomBookingApprovalEndpointSendRecallRoomBooking1PortAdapter,
        ApiWorkplaces.BuildingWorkflowService.BuildingWorkflowServiceClient workplacesBuildingWorkflowService,
        ApiWorkplaces.DeskApprovalService.DeskApprovalServiceClient workplacesDeskApprovalService,
        ApiWorkplaces.VisitorPassSyncService.VisitorPassSyncServiceClient workplacesVisitorPassSyncService)
    {
        _bus = bus;
        _corporateeventsVenueReportingService = corporateeventsVenueReportingService;
        _documentsDocumentFolderHistoryService = documentsDocumentFolderHistoryService;
        _documentsDocumentFolderSyncService = documentsDocumentFolderSyncService;
        _documentsDocumentTemplateSyncService = documentsDocumentTemplateSyncService;
        _documentsSignatureHistoryService = documentsSignatureHistoryService;
        _employeesEmployeeQueryService = employeesEmployeeQueryService;
        _employeesEmployeeService = employeesEmployeeService;
        _employeesEmploymentContractCalculationService = employeesEmploymentContractCalculationService;
        _ledgerLedgerAccountService = ledgerLedgerAccountService;
        _logger = logger;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _roomBookingApprovalEndpointSendRecallRoomBooking1Port = roomBookingApprovalEndpointSendRecallRoomBooking1PortAdapter;
        _workplacesBuildingWorkflowService = workplacesBuildingWorkflowService;
        _workplacesDeskApprovalService = workplacesDeskApprovalService;
        _workplacesVisitorPassSyncService = workplacesVisitorPassSyncService;
        _sendDelegateRoomBookingApproval0 = _employeesEmployeeQueryService.ResolveEmployeesAsync;
    }
}
