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
using ApiCustomers = Acme.Customers.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

/// <summary>
/// Builds and schedules reports about room bookings.
/// </summary>
public sealed partial class RoomBookingReportingEndpoint : ApiWorkplaces.RoomBookingReportingService.RoomBookingReportingServiceBase
{
    private readonly NotificationBus _bus;
    private readonly ApiCustomers.CustomerService.CustomerServiceClient _customersCustomerService;
    private readonly ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient _documentsDocumentTemplateSyncService;
    private readonly ApiEmployees.EmployeeService.EmployeeServiceClient _employeesEmployeeService;
    private readonly ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient _employeesOrgUnitQueryService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.ChangeRequestReportingService.ChangeRequestReportingServiceClient _itsmChangeRequestReportingService;
    private readonly ApiLedger.LedgerAccountService.LedgerAccountServiceClient _ledgerLedgerAccountService;
    private readonly ApiLicensing.SoftwareLicenseApprovalService.SoftwareLicenseApprovalServiceClient _licensingSoftwareLicenseApprovalService;
    private readonly ILogger<RoomBookingReportingEndpoint> _logger;
    private readonly ApiMetrics.MetricSeriesValidationService.MetricSeriesValidationServiceClient _metricsMetricSeriesValidationService;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiLedger.CreateLedgerAccountRequest, CallOptions, AsyncUnaryCall<ApiLedger.CreateLedgerAccountResponse>> _sendShareRoomBookingReport2;
    private readonly ApiWorkplaces.BuildingQueryService.BuildingQueryServiceClient _workplacesBuildingQueryService;
    private readonly ApiWorkplaces.DeskQueryService.DeskQueryServiceClient _workplacesDeskQueryService;
    private readonly ApiWorkplaces.RoomBookingCalculationService.RoomBookingCalculationServiceClient _workplacesRoomBookingCalculationService;
    private readonly ApiWorkplaces.RoomBookingValidationService.RoomBookingValidationServiceClient _workplacesRoomBookingValidationService;

    public RoomBookingReportingEndpoint(
        NotificationBus bus,
        ApiCustomers.CustomerService.CustomerServiceClient customersCustomerService,
        ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient documentsDocumentTemplateSyncService,
        ApiEmployees.EmployeeService.EmployeeServiceClient employeesEmployeeService,
        ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient employeesOrgUnitQueryService,
        CallInvoker invoker,
        ApiItsm.ChangeRequestReportingService.ChangeRequestReportingServiceClient itsmChangeRequestReportingService,
        ApiLedger.LedgerAccountService.LedgerAccountServiceClient ledgerLedgerAccountService,
        ApiLicensing.SoftwareLicenseApprovalService.SoftwareLicenseApprovalServiceClient licensingSoftwareLicenseApprovalService,
        ILogger<RoomBookingReportingEndpoint> logger,
        ApiMetrics.MetricSeriesValidationService.MetricSeriesValidationServiceClient metricsMetricSeriesValidationService,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiWorkplaces.BuildingQueryService.BuildingQueryServiceClient workplacesBuildingQueryService,
        ApiWorkplaces.DeskQueryService.DeskQueryServiceClient workplacesDeskQueryService,
        ApiWorkplaces.RoomBookingCalculationService.RoomBookingCalculationServiceClient workplacesRoomBookingCalculationService,
        ApiWorkplaces.RoomBookingValidationService.RoomBookingValidationServiceClient workplacesRoomBookingValidationService)
    {
        _bus = bus;
        _customersCustomerService = customersCustomerService;
        _documentsDocumentTemplateSyncService = documentsDocumentTemplateSyncService;
        _employeesEmployeeService = employeesEmployeeService;
        _employeesOrgUnitQueryService = employeesOrgUnitQueryService;
        _invoker = invoker;
        _itsmChangeRequestReportingService = itsmChangeRequestReportingService;
        _ledgerLedgerAccountService = ledgerLedgerAccountService;
        _licensingSoftwareLicenseApprovalService = licensingSoftwareLicenseApprovalService;
        _logger = logger;
        _metricsMetricSeriesValidationService = metricsMetricSeriesValidationService;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _workplacesBuildingQueryService = workplacesBuildingQueryService;
        _workplacesDeskQueryService = workplacesDeskQueryService;
        _workplacesRoomBookingCalculationService = workplacesRoomBookingCalculationService;
        _workplacesRoomBookingValidationService = workplacesRoomBookingValidationService;
        _sendShareRoomBookingReport2 = _ledgerLedgerAccountService.CreateLedgerAccountAsync;
    }
}
