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
using ApiBoxoffice = Acme.Boxoffice.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiPipelines = Acme.Pipelines.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

/// <summary>
/// Routes visitor passes through approval, with escalation and delegation.
/// </summary>
public sealed partial class VisitorPassApprovalEndpoint : ApiWorkplaces.VisitorPassApprovalService.VisitorPassApprovalServiceBase
{
    private readonly ApiBoxoffice.BoxOfficeSaleService.BoxOfficeSaleServiceClient _boxofficeBoxOfficeSaleService;
    private readonly NotificationBus _bus;
    private readonly ApiDocuments.DocumentService.DocumentServiceClient _documentsDocumentService;
    private readonly ApiDocuments.DocumentTemplateHistoryService.DocumentTemplateHistoryServiceClient _documentsDocumentTemplateHistoryService;
    private readonly ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient _employeesEmployeeQueryService;
    private readonly ApiEmployees.EmployeeService.EmployeeServiceClient _employeesEmployeeService;
    private readonly ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient _employeesEmploymentContractCalculationService;
    private readonly ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient _employeesEmploymentContractValidationService;
    private readonly ApiEmployees.PositionValidationService.PositionValidationServiceClient _employeesPositionValidationService;
    private readonly ApiIdentity.UserService.UserServiceClient _identityUserService;
    private readonly ILogger<VisitorPassApprovalEndpoint> _logger;
    private readonly ApiPipelines.PipelineCalculationService.PipelineCalculationServiceClient _pipelinesPipelineCalculationService;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiEmployees.ResolveEmployeesRequest, CallOptions, AsyncUnaryCall<ApiEmployees.ResolveEmployeesResponse>> _sendRecallVisitorPass0;
    private readonly IVisitorPassApprovalEndpointSendApproveVisitorPass0Port _visitorPassApprovalEndpointSendApproveVisitorPass0Port;
    private readonly ApiWorkplaces.BuildingWorkflowService.BuildingWorkflowServiceClient _workplacesBuildingWorkflowService;
    private readonly ApiWorkplaces.RoomBookingCalculationService.RoomBookingCalculationServiceClient _workplacesRoomBookingCalculationService;
    private readonly ApiWorkplaces.VisitorPassReportingService.VisitorPassReportingServiceClient _workplacesVisitorPassReportingService;

    public VisitorPassApprovalEndpoint(
        ApiBoxoffice.BoxOfficeSaleService.BoxOfficeSaleServiceClient boxofficeBoxOfficeSaleService,
        NotificationBus bus,
        ApiDocuments.DocumentService.DocumentServiceClient documentsDocumentService,
        ApiDocuments.DocumentTemplateHistoryService.DocumentTemplateHistoryServiceClient documentsDocumentTemplateHistoryService,
        ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient employeesEmployeeQueryService,
        ApiEmployees.EmployeeService.EmployeeServiceClient employeesEmployeeService,
        ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient employeesEmploymentContractCalculationService,
        ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient employeesEmploymentContractValidationService,
        ApiEmployees.PositionValidationService.PositionValidationServiceClient employeesPositionValidationService,
        ApiIdentity.UserService.UserServiceClient identityUserService,
        ILogger<VisitorPassApprovalEndpoint> logger,
        ApiPipelines.PipelineCalculationService.PipelineCalculationServiceClient pipelinesPipelineCalculationService,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository,
        IVisitorPassApprovalEndpointSendApproveVisitorPass0Port visitorPassApprovalEndpointSendApproveVisitorPass0PortAdapter,
        ApiWorkplaces.BuildingWorkflowService.BuildingWorkflowServiceClient workplacesBuildingWorkflowService,
        ApiWorkplaces.RoomBookingCalculationService.RoomBookingCalculationServiceClient workplacesRoomBookingCalculationService,
        ApiWorkplaces.VisitorPassReportingService.VisitorPassReportingServiceClient workplacesVisitorPassReportingService)
    {
        _boxofficeBoxOfficeSaleService = boxofficeBoxOfficeSaleService;
        _bus = bus;
        _documentsDocumentService = documentsDocumentService;
        _documentsDocumentTemplateHistoryService = documentsDocumentTemplateHistoryService;
        _employeesEmployeeQueryService = employeesEmployeeQueryService;
        _employeesEmployeeService = employeesEmployeeService;
        _employeesEmploymentContractCalculationService = employeesEmploymentContractCalculationService;
        _employeesEmploymentContractValidationService = employeesEmploymentContractValidationService;
        _employeesPositionValidationService = employeesPositionValidationService;
        _identityUserService = identityUserService;
        _logger = logger;
        _pipelinesPipelineCalculationService = pipelinesPipelineCalculationService;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _visitorPassApprovalEndpointSendApproveVisitorPass0Port = visitorPassApprovalEndpointSendApproveVisitorPass0PortAdapter;
        _workplacesBuildingWorkflowService = workplacesBuildingWorkflowService;
        _workplacesRoomBookingCalculationService = workplacesRoomBookingCalculationService;
        _workplacesVisitorPassReportingService = workplacesVisitorPassReportingService;
        _sendRecallVisitorPass0 = _employeesEmployeeQueryService.ResolveEmployeesAsync;
    }
}
