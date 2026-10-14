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
using ApiBenefits = Acme.Benefits.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLabeling = Acme.Labeling.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Benefits;

/// <summary>
/// Answers read-only questions about enrollments, for screens and reports.
/// </summary>
public sealed partial class EnrollmentQueryEndpoint : ApiBenefits.EnrollmentQueryService.EnrollmentQueryServiceBase
{
    private readonly ApiAudit.AuditEventService.AuditEventServiceClient _auditAuditEventService;
    private readonly ApiBenefits.BenefitPlanQueryService.BenefitPlanQueryServiceClient _benefitsBenefitPlanQueryService;
    private readonly ApiBenefits.LifeEventCalculationService.LifeEventCalculationServiceClient _benefitsLifeEventCalculationService;
    private readonly ApiBenefits.LifeEventValidationService.LifeEventValidationServiceClient _benefitsLifeEventValidationService;
    private readonly ApiDocuments.DocumentTemplateHistoryService.DocumentTemplateHistoryServiceClient _documentsDocumentTemplateHistoryService;
    private readonly ApiHelpdesk.EscalationCalculationService.EscalationCalculationServiceClient _helpdeskEscalationCalculationService;
    private readonly ApiHelpdesk.TicketQueryService.TicketQueryServiceClient _helpdeskTicketQueryService;
    private readonly CallInvoker _invoker;
    private readonly ApiLabeling.LabelingTaskQueryService.LabelingTaskQueryServiceClient _labelingLabelingTaskQueryService;
    private readonly ILogger<EnrollmentQueryEndpoint> _logger;
    private readonly ApiMetrics.MetricSeriesValidationService.MetricSeriesValidationServiceClient _metricsMetricSeriesValidationService;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiLabeling.SuggestLabelingTasksRequest, CallOptions, AsyncUnaryCall<ApiLabeling.SuggestLabelingTasksResponse>> _sendBrowseEnrollments1;

    public EnrollmentQueryEndpoint(
        ApiAudit.AuditEventService.AuditEventServiceClient auditAuditEventService,
        ApiBenefits.BenefitPlanQueryService.BenefitPlanQueryServiceClient benefitsBenefitPlanQueryService,
        ApiBenefits.LifeEventCalculationService.LifeEventCalculationServiceClient benefitsLifeEventCalculationService,
        ApiBenefits.LifeEventValidationService.LifeEventValidationServiceClient benefitsLifeEventValidationService,
        ApiDocuments.DocumentTemplateHistoryService.DocumentTemplateHistoryServiceClient documentsDocumentTemplateHistoryService,
        ApiHelpdesk.EscalationCalculationService.EscalationCalculationServiceClient helpdeskEscalationCalculationService,
        ApiHelpdesk.TicketQueryService.TicketQueryServiceClient helpdeskTicketQueryService,
        CallInvoker invoker,
        ApiLabeling.LabelingTaskQueryService.LabelingTaskQueryServiceClient labelingLabelingTaskQueryService,
        ILogger<EnrollmentQueryEndpoint> logger,
        ApiMetrics.MetricSeriesValidationService.MetricSeriesValidationServiceClient metricsMetricSeriesValidationService,
        IRecordRepository repository)
    {
        _auditAuditEventService = auditAuditEventService;
        _benefitsBenefitPlanQueryService = benefitsBenefitPlanQueryService;
        _benefitsLifeEventCalculationService = benefitsLifeEventCalculationService;
        _benefitsLifeEventValidationService = benefitsLifeEventValidationService;
        _documentsDocumentTemplateHistoryService = documentsDocumentTemplateHistoryService;
        _helpdeskEscalationCalculationService = helpdeskEscalationCalculationService;
        _helpdeskTicketQueryService = helpdeskTicketQueryService;
        _invoker = invoker;
        _labelingLabelingTaskQueryService = labelingLabelingTaskQueryService;
        _logger = logger;
        _metricsMetricSeriesValidationService = metricsMetricSeriesValidationService;
        _repository = repository;
        _sendBrowseEnrollments1 = _labelingLabelingTaskQueryService.SuggestLabelingTasksAsync;
    }
}
