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
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiPipelines = Acme.Pipelines.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

/// <summary>
/// Manages buildings: creates, reads, changes, and lists them.
/// </summary>
public sealed partial class BuildingEndpoint : ApiWorkplaces.BuildingService.BuildingServiceBase
{
    private readonly ApiAudit.AuditEventValidationService.AuditEventValidationServiceClient _auditAuditEventValidationService;
    private readonly IBuildingEndpointSendWatchBuildings0Port _buildingEndpointSendWatchBuildings0Port;
    private readonly ApiDocuments.DocumentFolderQueryService.DocumentFolderQueryServiceClient _documentsDocumentFolderQueryService;
    private readonly ApiDocuments.DocumentTemplateNotificationService.DocumentTemplateNotificationServiceClient _documentsDocumentTemplateNotificationService;
    private readonly ApiHelpdesk.EscalationCalculationService.EscalationCalculationServiceClient _helpdeskEscalationCalculationService;
    private readonly ApiIdentity.UserValidationService.UserValidationServiceClient _identityUserValidationService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.ServiceAssetValidationService.ServiceAssetValidationServiceClient _itsmServiceAssetValidationService;
    private readonly ILogger<BuildingEndpoint> _logger;
    private readonly ApiPipelines.PipelineService.PipelineServiceClient _pipelinesPipelineService;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiWorkplaces.SimulateDeskRequest, CallOptions, AsyncUnaryCall<ApiWorkplaces.SimulateDeskResponse>> _sendDeleteBuilding1;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiWorkplaces.BuildingQueryService.BuildingQueryServiceClient _workplacesBuildingQueryService;
    private readonly ApiWorkplaces.BuildingValidationService.BuildingValidationServiceClient _workplacesBuildingValidationService;
    private readonly ApiWorkplaces.DeskCalculationService.DeskCalculationServiceClient _workplacesDeskCalculationService;
    private readonly ApiWorkplaces.RoomBookingCalculationService.RoomBookingCalculationServiceClient _workplacesRoomBookingCalculationService;
    private readonly ApiWorkplaces.VisitorPassCalculationService.VisitorPassCalculationServiceClient _workplacesVisitorPassCalculationService;
    private readonly ApiWorkplaces.VisitorPassValidationService.VisitorPassValidationServiceClient _workplacesVisitorPassValidationService;

    public BuildingEndpoint(
        ApiAudit.AuditEventValidationService.AuditEventValidationServiceClient auditAuditEventValidationService,
        IBuildingEndpointSendWatchBuildings0Port buildingEndpointSendWatchBuildings0PortAdapter,
        ApiDocuments.DocumentFolderQueryService.DocumentFolderQueryServiceClient documentsDocumentFolderQueryService,
        ApiDocuments.DocumentTemplateNotificationService.DocumentTemplateNotificationServiceClient documentsDocumentTemplateNotificationService,
        ApiHelpdesk.EscalationCalculationService.EscalationCalculationServiceClient helpdeskEscalationCalculationService,
        ApiIdentity.UserValidationService.UserValidationServiceClient identityUserValidationService,
        CallInvoker invoker,
        ApiItsm.ServiceAssetValidationService.ServiceAssetValidationServiceClient itsmServiceAssetValidationService,
        ILogger<BuildingEndpoint> logger,
        ApiPipelines.PipelineService.PipelineServiceClient pipelinesPipelineService,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiWorkplaces.BuildingQueryService.BuildingQueryServiceClient workplacesBuildingQueryService,
        ApiWorkplaces.BuildingValidationService.BuildingValidationServiceClient workplacesBuildingValidationService,
        ApiWorkplaces.DeskCalculationService.DeskCalculationServiceClient workplacesDeskCalculationService,
        ApiWorkplaces.RoomBookingCalculationService.RoomBookingCalculationServiceClient workplacesRoomBookingCalculationService,
        ApiWorkplaces.VisitorPassCalculationService.VisitorPassCalculationServiceClient workplacesVisitorPassCalculationService,
        ApiWorkplaces.VisitorPassValidationService.VisitorPassValidationServiceClient workplacesVisitorPassValidationService)
    {
        _auditAuditEventValidationService = auditAuditEventValidationService;
        _buildingEndpointSendWatchBuildings0Port = buildingEndpointSendWatchBuildings0PortAdapter;
        _documentsDocumentFolderQueryService = documentsDocumentFolderQueryService;
        _documentsDocumentTemplateNotificationService = documentsDocumentTemplateNotificationService;
        _helpdeskEscalationCalculationService = helpdeskEscalationCalculationService;
        _identityUserValidationService = identityUserValidationService;
        _invoker = invoker;
        _itsmServiceAssetValidationService = itsmServiceAssetValidationService;
        _logger = logger;
        _pipelinesPipelineService = pipelinesPipelineService;
        _repository = repository;
        _settings = settings;
        _workplacesBuildingQueryService = workplacesBuildingQueryService;
        _workplacesBuildingValidationService = workplacesBuildingValidationService;
        _workplacesDeskCalculationService = workplacesDeskCalculationService;
        _workplacesRoomBookingCalculationService = workplacesRoomBookingCalculationService;
        _workplacesVisitorPassCalculationService = workplacesVisitorPassCalculationService;
        _workplacesVisitorPassValidationService = workplacesVisitorPassValidationService;
        _sendDeleteBuilding1 = _workplacesDeskCalculationService.SimulateDeskAsync;
    }
}
