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
using ApiCareplans = Acme.Careplans.V1;
using ApiCommon = Acme.Common.V1;
using ApiDispatch = Acme.Dispatch.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiScheduling = Acme.Scheduling.V1;

namespace Acme.Workforce.Scheduling;

/// <summary>
/// Answers read-only questions about shifts, for screens and reports.
/// </summary>
public sealed partial class ShiftQueryEndpoint : ApiScheduling.ShiftQueryService.ShiftQueryServiceBase
{
    private readonly ApiAudit.AuditEventService.AuditEventServiceClient _auditAuditEventService;
    private readonly ApiAudit.AuditEventValidationService.AuditEventValidationServiceClient _auditAuditEventValidationService;
    private readonly ApiCareplans.CarePlanCalculationService.CarePlanCalculationServiceClient _careplansCarePlanCalculationService;
    private readonly ApiDispatch.DispatchOrderQueryService.DispatchOrderQueryServiceClient _dispatchDispatchOrderQueryService;
    private readonly ApiDocuments.DocumentHistoryService.DocumentHistoryServiceClient _documentsDocumentHistoryService;
    private readonly ApiDocuments.SignatureNotificationService.SignatureNotificationServiceClient _documentsSignatureNotificationService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.IncidentCalculationService.IncidentCalculationServiceClient _itsmIncidentCalculationService;
    private readonly ILogger<ShiftQueryEndpoint> _logger;
    private readonly ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient _metricsMetricSeriesQueryService;
    private readonly IRecordRepository _repository;
    private readonly ApiScheduling.AvailabilityWindowValidationService.AvailabilityWindowValidationServiceClient _schedulingAvailabilityWindowValidationService;
    private readonly ApiScheduling.SwapRequestCalculationService.SwapRequestCalculationServiceClient _schedulingSwapRequestCalculationService;

    public ShiftQueryEndpoint(
        ApiAudit.AuditEventService.AuditEventServiceClient auditAuditEventService,
        ApiAudit.AuditEventValidationService.AuditEventValidationServiceClient auditAuditEventValidationService,
        ApiCareplans.CarePlanCalculationService.CarePlanCalculationServiceClient careplansCarePlanCalculationService,
        ApiDispatch.DispatchOrderQueryService.DispatchOrderQueryServiceClient dispatchDispatchOrderQueryService,
        ApiDocuments.DocumentHistoryService.DocumentHistoryServiceClient documentsDocumentHistoryService,
        ApiDocuments.SignatureNotificationService.SignatureNotificationServiceClient documentsSignatureNotificationService,
        CallInvoker invoker,
        ApiItsm.IncidentCalculationService.IncidentCalculationServiceClient itsmIncidentCalculationService,
        ILogger<ShiftQueryEndpoint> logger,
        ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient metricsMetricSeriesQueryService,
        IRecordRepository repository,
        ApiScheduling.AvailabilityWindowValidationService.AvailabilityWindowValidationServiceClient schedulingAvailabilityWindowValidationService,
        ApiScheduling.SwapRequestCalculationService.SwapRequestCalculationServiceClient schedulingSwapRequestCalculationService)
    {
        _auditAuditEventService = auditAuditEventService;
        _auditAuditEventValidationService = auditAuditEventValidationService;
        _careplansCarePlanCalculationService = careplansCarePlanCalculationService;
        _dispatchDispatchOrderQueryService = dispatchDispatchOrderQueryService;
        _documentsDocumentHistoryService = documentsDocumentHistoryService;
        _documentsSignatureNotificationService = documentsSignatureNotificationService;
        _invoker = invoker;
        _itsmIncidentCalculationService = itsmIncidentCalculationService;
        _logger = logger;
        _metricsMetricSeriesQueryService = metricsMetricSeriesQueryService;
        _repository = repository;
        _schedulingAvailabilityWindowValidationService = schedulingAvailabilityWindowValidationService;
        _schedulingSwapRequestCalculationService = schedulingSwapRequestCalculationService;
    }
}
