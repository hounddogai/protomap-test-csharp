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
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiScheduling = Acme.Scheduling.V1;

namespace Acme.Workforce.Scheduling;

/// <summary>
/// Checks availability windows against the rules before anything acts on them.
/// </summary>
public sealed partial class AvailabilityWindowValidationEndpoint : ApiScheduling.AvailabilityWindowValidationService.AvailabilityWindowValidationServiceBase
{
    private readonly IAvailabilityWindowValidationEndpointSendPreviewAvailabilityWindow1Port _availabilityWindowValidationEndpointSendPreviewAvailabilityWindow1Port;
    private readonly NotificationBus _bus;
    private readonly ApiDevices.DevicePolicyCalculationService.DevicePolicyCalculationServiceClient _devicesDevicePolicyCalculationService;
    private readonly ApiDocuments.DocumentValidationService.DocumentValidationServiceClient _documentsDocumentValidationService;
    private readonly CallInvoker _invoker;
    private readonly ApiLedger.LedgerAccountValidationService.LedgerAccountValidationServiceClient _ledgerLedgerAccountValidationService;
    private readonly ILogger<AvailabilityWindowValidationEndpoint> _logger;
    private readonly ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient _metricsMetricSeriesQueryService;
    private readonly ApiPerformance.GoalValidationService.GoalValidationServiceClient _performanceGoalValidationService;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiScheduling.AvailabilityWindowCalculationService.AvailabilityWindowCalculationServiceClient _schedulingAvailabilityWindowCalculationService;
    private readonly ApiScheduling.RosterCalculationService.RosterCalculationServiceClient _schedulingRosterCalculationService;
    private readonly ApiScheduling.RosterValidationService.RosterValidationServiceClient _schedulingRosterValidationService;
    private readonly ApiScheduling.ShiftValidationService.ShiftValidationServiceClient _schedulingShiftValidationService;
    private readonly ApiScheduling.SwapRequestCalculationService.SwapRequestCalculationServiceClient _schedulingSwapRequestCalculationService;
    private readonly IOptions<MeshOptions> _settings;

    public AvailabilityWindowValidationEndpoint(
        IAvailabilityWindowValidationEndpointSendPreviewAvailabilityWindow1Port availabilityWindowValidationEndpointSendPreviewAvailabilityWindow1PortAdapter,
        NotificationBus bus,
        ApiDevices.DevicePolicyCalculationService.DevicePolicyCalculationServiceClient devicesDevicePolicyCalculationService,
        ApiDocuments.DocumentValidationService.DocumentValidationServiceClient documentsDocumentValidationService,
        CallInvoker invoker,
        ApiLedger.LedgerAccountValidationService.LedgerAccountValidationServiceClient ledgerLedgerAccountValidationService,
        ILogger<AvailabilityWindowValidationEndpoint> logger,
        ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient metricsMetricSeriesQueryService,
        ApiPerformance.GoalValidationService.GoalValidationServiceClient performanceGoalValidationService,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiScheduling.AvailabilityWindowCalculationService.AvailabilityWindowCalculationServiceClient schedulingAvailabilityWindowCalculationService,
        ApiScheduling.RosterCalculationService.RosterCalculationServiceClient schedulingRosterCalculationService,
        ApiScheduling.RosterValidationService.RosterValidationServiceClient schedulingRosterValidationService,
        ApiScheduling.ShiftValidationService.ShiftValidationServiceClient schedulingShiftValidationService,
        ApiScheduling.SwapRequestCalculationService.SwapRequestCalculationServiceClient schedulingSwapRequestCalculationService,
        IOptions<MeshOptions> settings)
    {
        _availabilityWindowValidationEndpointSendPreviewAvailabilityWindow1Port = availabilityWindowValidationEndpointSendPreviewAvailabilityWindow1PortAdapter;
        _bus = bus;
        _devicesDevicePolicyCalculationService = devicesDevicePolicyCalculationService;
        _documentsDocumentValidationService = documentsDocumentValidationService;
        _invoker = invoker;
        _ledgerLedgerAccountValidationService = ledgerLedgerAccountValidationService;
        _logger = logger;
        _metricsMetricSeriesQueryService = metricsMetricSeriesQueryService;
        _performanceGoalValidationService = performanceGoalValidationService;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _schedulingAvailabilityWindowCalculationService = schedulingAvailabilityWindowCalculationService;
        _schedulingRosterCalculationService = schedulingRosterCalculationService;
        _schedulingRosterValidationService = schedulingRosterValidationService;
        _schedulingShiftValidationService = schedulingShiftValidationService;
        _schedulingSwapRequestCalculationService = schedulingSwapRequestCalculationService;
        _settings = settings;
    }
}
