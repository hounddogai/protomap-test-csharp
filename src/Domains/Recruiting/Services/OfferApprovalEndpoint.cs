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
using ApiRecruiting = Acme.Recruiting.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Recruiting;

/// <summary>
/// Routes offers through approval, with escalation and delegation.
/// </summary>
public sealed partial class OfferApprovalEndpoint : ApiRecruiting.OfferApprovalService.OfferApprovalServiceBase
{
    private readonly NotificationBus _bus;
    private readonly ApiDocuments.SignatureNotificationService.SignatureNotificationServiceClient _documentsSignatureNotificationService;
    private readonly ApiEmployees.EmployeeService.EmployeeServiceClient _employeesEmployeeService;
    private readonly ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient _employeesEmploymentContractCalculationService;
    private readonly ILogger<OfferApprovalEndpoint> _logger;
    private readonly ApiRecruiting.OfferLifecycleService.OfferLifecycleServiceClient _recruitingOfferLifecycleService;
    private readonly ApiRecruiting.OfferSyncService.OfferSyncServiceClient _recruitingOfferSyncService;
    private readonly IRecordRepository _repository;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiWorkplaces.DeskApprovalService.DeskApprovalServiceClient _workplacesDeskApprovalService;

    public OfferApprovalEndpoint(
        NotificationBus bus,
        ApiDocuments.SignatureNotificationService.SignatureNotificationServiceClient documentsSignatureNotificationService,
        ApiEmployees.EmployeeService.EmployeeServiceClient employeesEmployeeService,
        ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient employeesEmploymentContractCalculationService,
        ILogger<OfferApprovalEndpoint> logger,
        ApiRecruiting.OfferLifecycleService.OfferLifecycleServiceClient recruitingOfferLifecycleService,
        ApiRecruiting.OfferSyncService.OfferSyncServiceClient recruitingOfferSyncService,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiWorkplaces.DeskApprovalService.DeskApprovalServiceClient workplacesDeskApprovalService)
    {
        _bus = bus;
        _documentsSignatureNotificationService = documentsSignatureNotificationService;
        _employeesEmployeeService = employeesEmployeeService;
        _employeesEmploymentContractCalculationService = employeesEmploymentContractCalculationService;
        _logger = logger;
        _recruitingOfferLifecycleService = recruitingOfferLifecycleService;
        _recruitingOfferSyncService = recruitingOfferSyncService;
        _repository = repository;
        _settings = settings;
        _workplacesDeskApprovalService = workplacesDeskApprovalService;
    }
}
