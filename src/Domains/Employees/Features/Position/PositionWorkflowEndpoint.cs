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
using ApiAuthorization = Acme.Authorization.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Employees;

/// <summary>
/// Runs the long-lived workflow that takes a position from start to finish.
/// </summary>
public sealed partial class PositionWorkflowEndpoint : ApiEmployees.PositionWorkflowService.PositionWorkflowServiceBase
{
    private readonly ApiAuthorization.AccessPolicyService.AccessPolicyServiceClient _authorizationAccessPolicyService;
    private readonly ApiAuthorization.AccessPolicyValidationService.AccessPolicyValidationServiceClient _authorizationAccessPolicyValidationService;
    private readonly ApiCorporateevents.RegistrationReportingService.RegistrationReportingServiceClient _corporateeventsRegistrationReportingService;
    private readonly ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient _employeesEmployeeQueryService;
    private readonly ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient _employeesEmploymentContractValidationService;
    private readonly ApiEmployees.OrgUnitApprovalService.OrgUnitApprovalServiceClient _employeesOrgUnitApprovalService;
    private readonly ApiEmployees.PositionApprovalService.PositionApprovalServiceClient _employeesPositionApprovalService;
    private readonly ApiEmployees.PositionValidationService.PositionValidationServiceClient _employeesPositionValidationService;
    private readonly ILogger<PositionWorkflowEndpoint> _logger;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiEmployees.ApproveOrgUnitRequest, CallOptions, AsyncUnaryCall<ApiEmployees.ApproveOrgUnitResponse>> _sendRetryPositionWorkflow0;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTimekeeping.OvertimeApprovalService.OvertimeApprovalServiceClient _timekeepingOvertimeApprovalService;

    public PositionWorkflowEndpoint(
        ApiAuthorization.AccessPolicyService.AccessPolicyServiceClient authorizationAccessPolicyService,
        ApiAuthorization.AccessPolicyValidationService.AccessPolicyValidationServiceClient authorizationAccessPolicyValidationService,
        ApiCorporateevents.RegistrationReportingService.RegistrationReportingServiceClient corporateeventsRegistrationReportingService,
        ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient employeesEmployeeQueryService,
        ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient employeesEmploymentContractValidationService,
        ApiEmployees.OrgUnitApprovalService.OrgUnitApprovalServiceClient employeesOrgUnitApprovalService,
        ApiEmployees.PositionApprovalService.PositionApprovalServiceClient employeesPositionApprovalService,
        ApiEmployees.PositionValidationService.PositionValidationServiceClient employeesPositionValidationService,
        ILogger<PositionWorkflowEndpoint> logger,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiTimekeeping.OvertimeApprovalService.OvertimeApprovalServiceClient timekeepingOvertimeApprovalService)
    {
        _authorizationAccessPolicyService = authorizationAccessPolicyService;
        _authorizationAccessPolicyValidationService = authorizationAccessPolicyValidationService;
        _corporateeventsRegistrationReportingService = corporateeventsRegistrationReportingService;
        _employeesEmployeeQueryService = employeesEmployeeQueryService;
        _employeesEmploymentContractValidationService = employeesEmploymentContractValidationService;
        _employeesOrgUnitApprovalService = employeesOrgUnitApprovalService;
        _employeesPositionApprovalService = employeesPositionApprovalService;
        _employeesPositionValidationService = employeesPositionValidationService;
        _logger = logger;
        _repository = repository;
        _settings = settings;
        _timekeepingOvertimeApprovalService = timekeepingOvertimeApprovalService;
        _sendRetryPositionWorkflow0 = _employeesOrgUnitApprovalService.ApproveOrgUnitAsync;
    }
}
