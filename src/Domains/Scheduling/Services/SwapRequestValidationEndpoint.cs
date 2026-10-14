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
using ApiConfiguration = Acme.Configuration.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiScheduling = Acme.Scheduling.V1;

namespace Acme.Workforce.Scheduling;

/// <summary>
/// Checks swap requests against the rules before anything acts on them.
/// </summary>
public sealed partial class SwapRequestValidationEndpoint : ApiScheduling.SwapRequestValidationService.SwapRequestValidationServiceBase
{
    private readonly ApiConfiguration.ConfigSetQueryService.ConfigSetQueryServiceClient _configurationConfigSetQueryService;
    private readonly ApiLearning.LearningPathQueryService.LearningPathQueryServiceClient _learningLearningPathQueryService;
    private readonly ApiLedger.LedgerAccountValidationService.LedgerAccountValidationServiceClient _ledgerLedgerAccountValidationService;
    private readonly ILogger<SwapRequestValidationEndpoint> _logger;
    private readonly ApiMetrics.MetricSeriesValidationService.MetricSeriesValidationServiceClient _metricsMetricSeriesValidationService;
    private readonly ApiPerformance.ReviewService.ReviewServiceClient _performanceReviewService;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiScheduling.AvailabilityWindowValidationService.AvailabilityWindowValidationServiceClient _schedulingAvailabilityWindowValidationService;
    private readonly ApiScheduling.RosterCalculationService.RosterCalculationServiceClient _schedulingRosterCalculationService;
    private readonly ApiScheduling.RosterValidationService.RosterValidationServiceClient _schedulingRosterValidationService;
    private readonly ApiScheduling.ShiftValidationService.ShiftValidationServiceClient _schedulingShiftValidationService;
    private readonly ApiScheduling.SwapRequestCalculationService.SwapRequestCalculationServiceClient _schedulingSwapRequestCalculationService;
    private readonly Func<ApiLedger.ValidateLedgerAccountRequest, CallOptions, AsyncUnaryCall<ApiLedger.ValidateLedgerAccountResponse>> _sendPreviewSwapRequest3;
    private readonly IOptions<MeshOptions> _settings;

    public SwapRequestValidationEndpoint(
        ApiConfiguration.ConfigSetQueryService.ConfigSetQueryServiceClient configurationConfigSetQueryService,
        ApiLearning.LearningPathQueryService.LearningPathQueryServiceClient learningLearningPathQueryService,
        ApiLedger.LedgerAccountValidationService.LedgerAccountValidationServiceClient ledgerLedgerAccountValidationService,
        ILogger<SwapRequestValidationEndpoint> logger,
        ApiMetrics.MetricSeriesValidationService.MetricSeriesValidationServiceClient metricsMetricSeriesValidationService,
        ApiPerformance.ReviewService.ReviewServiceClient performanceReviewService,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiScheduling.AvailabilityWindowValidationService.AvailabilityWindowValidationServiceClient schedulingAvailabilityWindowValidationService,
        ApiScheduling.RosterCalculationService.RosterCalculationServiceClient schedulingRosterCalculationService,
        ApiScheduling.RosterValidationService.RosterValidationServiceClient schedulingRosterValidationService,
        ApiScheduling.ShiftValidationService.ShiftValidationServiceClient schedulingShiftValidationService,
        ApiScheduling.SwapRequestCalculationService.SwapRequestCalculationServiceClient schedulingSwapRequestCalculationService,
        IOptions<MeshOptions> settings)
    {
        _configurationConfigSetQueryService = configurationConfigSetQueryService;
        _learningLearningPathQueryService = learningLearningPathQueryService;
        _ledgerLedgerAccountValidationService = ledgerLedgerAccountValidationService;
        _logger = logger;
        _metricsMetricSeriesValidationService = metricsMetricSeriesValidationService;
        _performanceReviewService = performanceReviewService;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _schedulingAvailabilityWindowValidationService = schedulingAvailabilityWindowValidationService;
        _schedulingRosterCalculationService = schedulingRosterCalculationService;
        _schedulingRosterValidationService = schedulingRosterValidationService;
        _schedulingShiftValidationService = schedulingShiftValidationService;
        _schedulingSwapRequestCalculationService = schedulingSwapRequestCalculationService;
        _settings = settings;
        _sendPreviewSwapRequest3 = _ledgerLedgerAccountValidationService.ValidateLedgerAccountAsync;
    }
}
