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
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Scheduling;

/// <summary>
/// Checks rosters against the rules before anything acts on them.
/// </summary>
public sealed partial class RosterValidationEndpoint : ApiScheduling.RosterValidationService.RosterValidationServiceBase
{
    private readonly ApiLearning.CourseQueryService.CourseQueryServiceClient _learningCourseQueryService;
    private readonly ILogger<RosterValidationEndpoint> _logger;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly IRosterValidationEndpointSendPreviewRoster1Port _rosterValidationEndpointSendPreviewRoster1Port;
    private readonly ApiScheduling.AvailabilityWindowCalculationService.AvailabilityWindowCalculationServiceClient _schedulingAvailabilityWindowCalculationService;
    private readonly ApiScheduling.RosterCalculationService.RosterCalculationServiceClient _schedulingRosterCalculationService;
    private readonly ApiScheduling.ShiftValidationService.ShiftValidationServiceClient _schedulingShiftValidationService;
    private readonly Func<ApiScheduling.CalculateRosterRequest, CallOptions, AsyncUnaryCall<ApiScheduling.CalculateRosterResponse>> _sendGetRosterRules0;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTravel.TripValidationService.TripValidationServiceClient _travelTripValidationService;
    private readonly ApiWorkplaces.DeskCalculationService.DeskCalculationServiceClient _workplacesDeskCalculationService;

    public RosterValidationEndpoint(
        ApiLearning.CourseQueryService.CourseQueryServiceClient learningCourseQueryService,
        ILogger<RosterValidationEndpoint> logger,
        ClientRegistry registry,
        IRecordRepository repository,
        IRosterValidationEndpointSendPreviewRoster1Port rosterValidationEndpointSendPreviewRoster1PortAdapter,
        ApiScheduling.AvailabilityWindowCalculationService.AvailabilityWindowCalculationServiceClient schedulingAvailabilityWindowCalculationService,
        ApiScheduling.RosterCalculationService.RosterCalculationServiceClient schedulingRosterCalculationService,
        ApiScheduling.ShiftValidationService.ShiftValidationServiceClient schedulingShiftValidationService,
        IOptions<MeshOptions> settings,
        ApiTravel.TripValidationService.TripValidationServiceClient travelTripValidationService,
        ApiWorkplaces.DeskCalculationService.DeskCalculationServiceClient workplacesDeskCalculationService)
    {
        _learningCourseQueryService = learningCourseQueryService;
        _logger = logger;
        _registry = registry;
        _repository = repository;
        _rosterValidationEndpointSendPreviewRoster1Port = rosterValidationEndpointSendPreviewRoster1PortAdapter;
        _schedulingAvailabilityWindowCalculationService = schedulingAvailabilityWindowCalculationService;
        _schedulingRosterCalculationService = schedulingRosterCalculationService;
        _schedulingShiftValidationService = schedulingShiftValidationService;
        _settings = settings;
        _travelTripValidationService = travelTripValidationService;
        _workplacesDeskCalculationService = workplacesDeskCalculationService;
        _sendGetRosterRules0 = _schedulingRosterCalculationService.CalculateRosterAsync;
    }
}
