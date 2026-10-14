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
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiSearch = Acme.Search.V1;

namespace Acme.Workforce.Employees;

/// <summary>
/// Answers read-only questions about employees, for screens and reports.
/// </summary>
public sealed partial class EmployeeQueryEndpoint : ApiEmployees.EmployeeQueryService.EmployeeQueryServiceBase
{
    private readonly ApiDocuments.SignatureValidationService.SignatureValidationServiceClient _documentsSignatureValidationService;
    private readonly ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient _employeesEmploymentContractValidationService;
    private readonly ApiEmployees.PositionValidationService.PositionValidationServiceClient _employeesPositionValidationService;
    private readonly CallInvoker _invoker;
    private readonly ApiLicensing.EntitlementQueryService.EntitlementQueryServiceClient _licensingEntitlementQueryService;
    private readonly ILogger<EmployeeQueryEndpoint> _logger;
    private readonly ApiNotifications.NotificationService.NotificationServiceClient _notificationsNotificationService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiSearch.SearchIndexValidationService.SearchIndexValidationServiceClient _searchSearchIndexValidationService;
    private readonly IOptions<MeshOptions> _settings;

    public EmployeeQueryEndpoint(
        ApiDocuments.SignatureValidationService.SignatureValidationServiceClient documentsSignatureValidationService,
        ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient employeesEmploymentContractValidationService,
        ApiEmployees.PositionValidationService.PositionValidationServiceClient employeesPositionValidationService,
        CallInvoker invoker,
        ApiLicensing.EntitlementQueryService.EntitlementQueryServiceClient licensingEntitlementQueryService,
        ILogger<EmployeeQueryEndpoint> logger,
        ApiNotifications.NotificationService.NotificationServiceClient notificationsNotificationService,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiSearch.SearchIndexValidationService.SearchIndexValidationServiceClient searchSearchIndexValidationService,
        IOptions<MeshOptions> settings)
    {
        _documentsSignatureValidationService = documentsSignatureValidationService;
        _employeesEmploymentContractValidationService = employeesEmploymentContractValidationService;
        _employeesPositionValidationService = employeesPositionValidationService;
        _invoker = invoker;
        _licensingEntitlementQueryService = licensingEntitlementQueryService;
        _logger = logger;
        _notificationsNotificationService = notificationsNotificationService;
        _registry = registry;
        _repository = repository;
        _searchSearchIndexValidationService = searchSearchIndexValidationService;
        _settings = settings;
    }
}
