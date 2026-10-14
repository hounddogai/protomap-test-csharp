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
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

/// <summary>
/// Computes the figures of a desk, such as totals, taxes, and estimates.
/// </summary>
public sealed partial class DeskCalculationEndpoint : ApiWorkplaces.DeskCalculationService.DeskCalculationServiceBase
{
    private readonly NotificationBus _bus;
    private readonly ApiCorporateevents.RegistrationQueryService.RegistrationQueryServiceClient _corporateeventsRegistrationQueryService;
    private readonly ApiDocuments.DocumentExportService.DocumentExportServiceClient _documentsDocumentExportService;
    private readonly ApiFeatureflags.FeatureFlagValidationService.FeatureFlagValidationServiceClient _featureflagsFeatureFlagValidationService;
    private readonly ApiHelpdesk.EscalationCalculationService.EscalationCalculationServiceClient _helpdeskEscalationCalculationService;
    private readonly ApiHelpdesk.SlaPolicyCalculationService.SlaPolicyCalculationServiceClient _helpdeskSlaPolicyCalculationService;
    private readonly ApiHelpdesk.TicketQueryService.TicketQueryServiceClient _helpdeskTicketQueryService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.ChangeRequestQueryService.ChangeRequestQueryServiceClient _itsmChangeRequestQueryService;
    private readonly ApiItsm.ServiceAssetValidationService.ServiceAssetValidationServiceClient _itsmServiceAssetValidationService;
    private readonly ILogger<DeskCalculationEndpoint> _logger;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiWorkplaces.SimulateBuildingRequest, CallOptions, AsyncUnaryCall<ApiWorkplaces.SimulateBuildingResponse>> _sendExplainDeskCalculation2;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiWorkplaces.BuildingCalculationService.BuildingCalculationServiceClient _workplacesBuildingCalculationService;

    public DeskCalculationEndpoint(
        NotificationBus bus,
        ApiCorporateevents.RegistrationQueryService.RegistrationQueryServiceClient corporateeventsRegistrationQueryService,
        ApiDocuments.DocumentExportService.DocumentExportServiceClient documentsDocumentExportService,
        ApiFeatureflags.FeatureFlagValidationService.FeatureFlagValidationServiceClient featureflagsFeatureFlagValidationService,
        ApiHelpdesk.EscalationCalculationService.EscalationCalculationServiceClient helpdeskEscalationCalculationService,
        ApiHelpdesk.SlaPolicyCalculationService.SlaPolicyCalculationServiceClient helpdeskSlaPolicyCalculationService,
        ApiHelpdesk.TicketQueryService.TicketQueryServiceClient helpdeskTicketQueryService,
        CallInvoker invoker,
        ApiItsm.ChangeRequestQueryService.ChangeRequestQueryServiceClient itsmChangeRequestQueryService,
        ApiItsm.ServiceAssetValidationService.ServiceAssetValidationServiceClient itsmServiceAssetValidationService,
        ILogger<DeskCalculationEndpoint> logger,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiWorkplaces.BuildingCalculationService.BuildingCalculationServiceClient workplacesBuildingCalculationService)
    {
        _bus = bus;
        _corporateeventsRegistrationQueryService = corporateeventsRegistrationQueryService;
        _documentsDocumentExportService = documentsDocumentExportService;
        _featureflagsFeatureFlagValidationService = featureflagsFeatureFlagValidationService;
        _helpdeskEscalationCalculationService = helpdeskEscalationCalculationService;
        _helpdeskSlaPolicyCalculationService = helpdeskSlaPolicyCalculationService;
        _helpdeskTicketQueryService = helpdeskTicketQueryService;
        _invoker = invoker;
        _itsmChangeRequestQueryService = itsmChangeRequestQueryService;
        _itsmServiceAssetValidationService = itsmServiceAssetValidationService;
        _logger = logger;
        _registry = registry;
        _repository = repository;
        _settings = settings;
        _workplacesBuildingCalculationService = workplacesBuildingCalculationService;
        _sendExplainDeskCalculation2 = _workplacesBuildingCalculationService.SimulateBuildingAsync;
    }
}
