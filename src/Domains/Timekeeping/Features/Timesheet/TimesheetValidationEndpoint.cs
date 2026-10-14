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
using ApiEmployees = Acme.Employees.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiMaintenance = Acme.Maintenance.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiTelehealth = Acme.Telehealth.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Timekeeping;

/// <summary>
/// Checks timesheets against the rules before anything acts on them.
/// </summary>
public sealed partial class TimesheetValidationEndpoint : ApiTimekeeping.TimesheetValidationService.TimesheetValidationServiceBase
{
    private readonly ApiFeatureflags.FeatureFlagQueryService.FeatureFlagQueryServiceClient _featureflagsFeatureFlagQueryService;
    private readonly ApiFeatureflags.FeatureFlagValidationService.FeatureFlagValidationServiceClient _featureflagsFeatureFlagValidationService;
    private readonly ApiHelpdesk.KnowledgeArticleCalculationService.KnowledgeArticleCalculationServiceClient _helpdeskKnowledgeArticleCalculationService;
    private readonly ApiHelpdesk.SlaPolicyValidationService.SlaPolicyValidationServiceClient _helpdeskSlaPolicyValidationService;
    private readonly CallInvoker _invoker;
    private readonly ApiLedger.LedgerAccountValidationService.LedgerAccountValidationServiceClient _ledgerLedgerAccountValidationService;
    private readonly ApiLicensing.SoftwareLicenseValidationService.SoftwareLicenseValidationServiceClient _licensingSoftwareLicenseValidationService;
    private readonly ILogger<TimesheetValidationEndpoint> _logger;
    private readonly ApiMaintenance.WorkOrderCalculationService.WorkOrderCalculationServiceClient _maintenanceWorkOrderCalculationService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiScheduling.RosterCalculationService.RosterCalculationServiceClient _schedulingRosterCalculationService;
    private readonly Func<ApiFeatureflags.PreviewFeatureFlagRequest, CallOptions, AsyncUnaryCall<ApiFeatureflags.PreviewFeatureFlagResponse>> _sendPreviewTimesheet1;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTelehealth.VideoVisitQueryService.VideoVisitQueryServiceClient _telehealthVideoVisitQueryService;
    private readonly ApiTimekeeping.ClockEventCalculationService.ClockEventCalculationServiceClient _timekeepingClockEventCalculationService;
    private readonly ApiTravel.TravelPolicyValidationService.TravelPolicyValidationServiceClient _travelTravelPolicyValidationService;

    public TimesheetValidationEndpoint(
        ApiFeatureflags.FeatureFlagQueryService.FeatureFlagQueryServiceClient featureflagsFeatureFlagQueryService,
        ApiFeatureflags.FeatureFlagValidationService.FeatureFlagValidationServiceClient featureflagsFeatureFlagValidationService,
        ApiHelpdesk.KnowledgeArticleCalculationService.KnowledgeArticleCalculationServiceClient helpdeskKnowledgeArticleCalculationService,
        ApiHelpdesk.SlaPolicyValidationService.SlaPolicyValidationServiceClient helpdeskSlaPolicyValidationService,
        CallInvoker invoker,
        ApiLedger.LedgerAccountValidationService.LedgerAccountValidationServiceClient ledgerLedgerAccountValidationService,
        ApiLicensing.SoftwareLicenseValidationService.SoftwareLicenseValidationServiceClient licensingSoftwareLicenseValidationService,
        ILogger<TimesheetValidationEndpoint> logger,
        ApiMaintenance.WorkOrderCalculationService.WorkOrderCalculationServiceClient maintenanceWorkOrderCalculationService,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiScheduling.RosterCalculationService.RosterCalculationServiceClient schedulingRosterCalculationService,
        IOptions<MeshOptions> settings,
        ApiTelehealth.VideoVisitQueryService.VideoVisitQueryServiceClient telehealthVideoVisitQueryService,
        ApiTimekeeping.ClockEventCalculationService.ClockEventCalculationServiceClient timekeepingClockEventCalculationService,
        ApiTravel.TravelPolicyValidationService.TravelPolicyValidationServiceClient travelTravelPolicyValidationService)
    {
        _featureflagsFeatureFlagQueryService = featureflagsFeatureFlagQueryService;
        _featureflagsFeatureFlagValidationService = featureflagsFeatureFlagValidationService;
        _helpdeskKnowledgeArticleCalculationService = helpdeskKnowledgeArticleCalculationService;
        _helpdeskSlaPolicyValidationService = helpdeskSlaPolicyValidationService;
        _invoker = invoker;
        _ledgerLedgerAccountValidationService = ledgerLedgerAccountValidationService;
        _licensingSoftwareLicenseValidationService = licensingSoftwareLicenseValidationService;
        _logger = logger;
        _maintenanceWorkOrderCalculationService = maintenanceWorkOrderCalculationService;
        _registry = registry;
        _repository = repository;
        _schedulingRosterCalculationService = schedulingRosterCalculationService;
        _settings = settings;
        _telehealthVideoVisitQueryService = telehealthVideoVisitQueryService;
        _timekeepingClockEventCalculationService = timekeepingClockEventCalculationService;
        _travelTravelPolicyValidationService = travelTravelPolicyValidationService;
        _sendPreviewTimesheet1 = _featureflagsFeatureFlagValidationService.PreviewFeatureFlagAsync;
    }
}
