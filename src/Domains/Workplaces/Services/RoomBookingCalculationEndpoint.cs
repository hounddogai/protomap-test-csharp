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
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

/// <summary>
/// Computes the figures of a room booking, such as totals, taxes, and estimates.
/// </summary>
public sealed partial class RoomBookingCalculationEndpoint : ApiWorkplaces.RoomBookingCalculationService.RoomBookingCalculationServiceBase
{
    private readonly ApiDevices.ComplianceScanValidationService.ComplianceScanValidationServiceClient _devicesComplianceScanValidationService;
    private readonly ApiDevices.DevicePolicyValidationService.DevicePolicyValidationServiceClient _devicesDevicePolicyValidationService;
    private readonly ApiDocuments.DocumentFolderQueryService.DocumentFolderQueryServiceClient _documentsDocumentFolderQueryService;
    private readonly ApiDocuments.DocumentService.DocumentServiceClient _documentsDocumentService;
    private readonly ApiHelpdesk.EscalationCalculationService.EscalationCalculationServiceClient _helpdeskEscalationCalculationService;
    private readonly CallInvoker _invoker;
    private readonly ILogger<RoomBookingCalculationEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiWorkplaces.ScoreBuildingRequest, CallOptions, AsyncUnaryCall<ApiWorkplaces.ScoreBuildingResponse>> _sendExplainRoomBookingCalculation1;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiWorkplaces.BuildingValidationService.BuildingValidationServiceClient _workplacesBuildingValidationService;
    private readonly ApiWorkplaces.DeskCalculationService.DeskCalculationServiceClient _workplacesDeskCalculationService;

    public RoomBookingCalculationEndpoint(
        ApiDevices.ComplianceScanValidationService.ComplianceScanValidationServiceClient devicesComplianceScanValidationService,
        ApiDevices.DevicePolicyValidationService.DevicePolicyValidationServiceClient devicesDevicePolicyValidationService,
        ApiDocuments.DocumentFolderQueryService.DocumentFolderQueryServiceClient documentsDocumentFolderQueryService,
        ApiDocuments.DocumentService.DocumentServiceClient documentsDocumentService,
        ApiHelpdesk.EscalationCalculationService.EscalationCalculationServiceClient helpdeskEscalationCalculationService,
        CallInvoker invoker,
        ILogger<RoomBookingCalculationEndpoint> logger,
        RetryPolicy policy,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiWorkplaces.BuildingValidationService.BuildingValidationServiceClient workplacesBuildingValidationService,
        ApiWorkplaces.DeskCalculationService.DeskCalculationServiceClient workplacesDeskCalculationService)
    {
        _devicesComplianceScanValidationService = devicesComplianceScanValidationService;
        _devicesDevicePolicyValidationService = devicesDevicePolicyValidationService;
        _documentsDocumentFolderQueryService = documentsDocumentFolderQueryService;
        _documentsDocumentService = documentsDocumentService;
        _helpdeskEscalationCalculationService = helpdeskEscalationCalculationService;
        _invoker = invoker;
        _logger = logger;
        _policy = policy;
        _repository = repository;
        _settings = settings;
        _workplacesBuildingValidationService = workplacesBuildingValidationService;
        _workplacesDeskCalculationService = workplacesDeskCalculationService;
        _sendExplainRoomBookingCalculation1 = _workplacesBuildingValidationService.ScoreBuildingAsync;
    }
}
