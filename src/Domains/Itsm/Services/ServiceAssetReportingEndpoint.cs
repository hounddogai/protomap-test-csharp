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
using ApiCheckout = Acme.Checkout.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiDrivers = Acme.Drivers.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiShows = Acme.Shows.V1;

namespace Acme.Workforce.Itsm;

/// <summary>
/// Builds and schedules reports about service assets.
/// </summary>
public sealed partial class ServiceAssetReportingEndpoint : ApiItsm.ServiceAssetReportingService.ServiceAssetReportingServiceBase
{
    private readonly ApiCheckout.CheckoutSessionService.CheckoutSessionServiceClient _checkoutCheckoutSessionService;
    private readonly ApiDocuments.DocumentFolderHistoryService.DocumentFolderHistoryServiceClient _documentsDocumentFolderHistoryService;
    private readonly ApiDrivers.DriverCalculationService.DriverCalculationServiceClient _driversDriverCalculationService;
    private readonly ApiEmployees.EmployeeService.EmployeeServiceClient _employeesEmployeeService;
    private readonly ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient _employeesOrgUnitQueryService;
    private readonly ApiIdentity.UserQueryService.UserQueryServiceClient _identityUserQueryService;
    private readonly ApiIdentity.UserValidationService.UserValidationServiceClient _identityUserValidationService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.IncidentService.IncidentServiceClient _itsmIncidentService;
    private readonly ApiItsm.ServiceAssetCalculationService.ServiceAssetCalculationServiceClient _itsmServiceAssetCalculationService;
    private readonly ApiLedger.LedgerAccountValidationService.LedgerAccountValidationServiceClient _ledgerLedgerAccountValidationService;
    private readonly ApiLicensing.EntitlementApprovalService.EntitlementApprovalServiceClient _licensingEntitlementApprovalService;
    private readonly ApiLicensing.LicenseAssignmentLifecycleService.LicenseAssignmentLifecycleServiceClient _licensingLicenseAssignmentLifecycleService;
    private readonly ILogger<ServiceAssetReportingEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly IServiceAssetReportingEndpointSendShareServiceAssetReport0Port _serviceAssetReportingEndpointSendShareServiceAssetReport0Port;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiShows.ShowQueryService.ShowQueryServiceClient _showsShowQueryService;

    public ServiceAssetReportingEndpoint(
        ApiCheckout.CheckoutSessionService.CheckoutSessionServiceClient checkoutCheckoutSessionService,
        ApiDocuments.DocumentFolderHistoryService.DocumentFolderHistoryServiceClient documentsDocumentFolderHistoryService,
        ApiDrivers.DriverCalculationService.DriverCalculationServiceClient driversDriverCalculationService,
        ApiEmployees.EmployeeService.EmployeeServiceClient employeesEmployeeService,
        ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient employeesOrgUnitQueryService,
        ApiIdentity.UserQueryService.UserQueryServiceClient identityUserQueryService,
        ApiIdentity.UserValidationService.UserValidationServiceClient identityUserValidationService,
        CallInvoker invoker,
        ApiItsm.IncidentService.IncidentServiceClient itsmIncidentService,
        ApiItsm.ServiceAssetCalculationService.ServiceAssetCalculationServiceClient itsmServiceAssetCalculationService,
        ApiLedger.LedgerAccountValidationService.LedgerAccountValidationServiceClient ledgerLedgerAccountValidationService,
        ApiLicensing.EntitlementApprovalService.EntitlementApprovalServiceClient licensingEntitlementApprovalService,
        ApiLicensing.LicenseAssignmentLifecycleService.LicenseAssignmentLifecycleServiceClient licensingLicenseAssignmentLifecycleService,
        ILogger<ServiceAssetReportingEndpoint> logger,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository,
        IServiceAssetReportingEndpointSendShareServiceAssetReport0Port serviceAssetReportingEndpointSendShareServiceAssetReport0PortAdapter,
        IOptions<MeshOptions> settings,
        ApiShows.ShowQueryService.ShowQueryServiceClient showsShowQueryService)
    {
        _checkoutCheckoutSessionService = checkoutCheckoutSessionService;
        _documentsDocumentFolderHistoryService = documentsDocumentFolderHistoryService;
        _driversDriverCalculationService = driversDriverCalculationService;
        _employeesEmployeeService = employeesEmployeeService;
        _employeesOrgUnitQueryService = employeesOrgUnitQueryService;
        _identityUserQueryService = identityUserQueryService;
        _identityUserValidationService = identityUserValidationService;
        _invoker = invoker;
        _itsmIncidentService = itsmIncidentService;
        _itsmServiceAssetCalculationService = itsmServiceAssetCalculationService;
        _ledgerLedgerAccountValidationService = ledgerLedgerAccountValidationService;
        _licensingEntitlementApprovalService = licensingEntitlementApprovalService;
        _licensingLicenseAssignmentLifecycleService = licensingLicenseAssignmentLifecycleService;
        _logger = logger;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _serviceAssetReportingEndpointSendShareServiceAssetReport0Port = serviceAssetReportingEndpointSendShareServiceAssetReport0PortAdapter;
        _settings = settings;
        _showsShowQueryService = showsShowQueryService;
    }
}
