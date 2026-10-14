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
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLabeling = Acme.Labeling.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiWebhooks = Acme.Webhooks.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Recruiting;

/// <summary>
/// Manages job openings: creates, reads, changes, and lists them.
/// </summary>
public sealed partial class JobOpeningEndpoint : ApiRecruiting.JobOpeningService.JobOpeningServiceBase
{
    private readonly ApiAudit.AuditEventValidationService.AuditEventValidationServiceClient _auditAuditEventValidationService;
    private readonly ApiBenefits.EnrollmentQueryService.EnrollmentQueryServiceClient _benefitsEnrollmentQueryService;
    private readonly NotificationBus _bus;
    private readonly ApiDevices.DevicePolicyQueryService.DevicePolicyQueryServiceClient _devicesDevicePolicyQueryService;
    private readonly ApiDocuments.DocumentQueryService.DocumentQueryServiceClient _documentsDocumentQueryService;
    private readonly ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient _documentsDocumentTemplateSyncService;
    private readonly CallInvoker _invoker;
    private readonly ApiLabeling.LabelingTaskCalculationService.LabelingTaskCalculationServiceClient _labelingLabelingTaskCalculationService;
    private readonly ILogger<JobOpeningEndpoint> _logger;
    private readonly ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient _metricsMetricSeriesQueryService;
    private readonly RetryPolicy _policy;
    private readonly ApiRecruiting.CandidateQueryService.CandidateQueryServiceClient _recruitingCandidateQueryService;
    private readonly ApiRecruiting.CandidateValidationService.CandidateValidationServiceClient _recruitingCandidateValidationService;
    private readonly ApiRecruiting.InterviewCalculationService.InterviewCalculationServiceClient _recruitingInterviewCalculationService;
    private readonly ApiRecruiting.InterviewValidationService.InterviewValidationServiceClient _recruitingInterviewValidationService;
    private readonly ApiRecruiting.JobOpeningValidationService.JobOpeningValidationServiceClient _recruitingJobOpeningValidationService;
    private readonly ApiRecruiting.OfferValidationService.OfferValidationServiceClient _recruitingOfferValidationService;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiRecruiting.NormalizeOfferRequest, CallOptions, AsyncUnaryCall<ApiRecruiting.NormalizeOfferResponse>> _sendDeleteJobOpening0;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiWebhooks.WebhookEndpointQueryService.WebhookEndpointQueryServiceClient _webhooksWebhookEndpointQueryService;
    private readonly ApiWorkplaces.BuildingCalculationService.BuildingCalculationServiceClient _workplacesBuildingCalculationService;

    public JobOpeningEndpoint(
        ApiAudit.AuditEventValidationService.AuditEventValidationServiceClient auditAuditEventValidationService,
        ApiBenefits.EnrollmentQueryService.EnrollmentQueryServiceClient benefitsEnrollmentQueryService,
        NotificationBus bus,
        ApiDevices.DevicePolicyQueryService.DevicePolicyQueryServiceClient devicesDevicePolicyQueryService,
        ApiDocuments.DocumentQueryService.DocumentQueryServiceClient documentsDocumentQueryService,
        ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient documentsDocumentTemplateSyncService,
        CallInvoker invoker,
        ApiLabeling.LabelingTaskCalculationService.LabelingTaskCalculationServiceClient labelingLabelingTaskCalculationService,
        ILogger<JobOpeningEndpoint> logger,
        ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient metricsMetricSeriesQueryService,
        RetryPolicy policy,
        ApiRecruiting.CandidateQueryService.CandidateQueryServiceClient recruitingCandidateQueryService,
        ApiRecruiting.CandidateValidationService.CandidateValidationServiceClient recruitingCandidateValidationService,
        ApiRecruiting.InterviewCalculationService.InterviewCalculationServiceClient recruitingInterviewCalculationService,
        ApiRecruiting.InterviewValidationService.InterviewValidationServiceClient recruitingInterviewValidationService,
        ApiRecruiting.JobOpeningValidationService.JobOpeningValidationServiceClient recruitingJobOpeningValidationService,
        ApiRecruiting.OfferValidationService.OfferValidationServiceClient recruitingOfferValidationService,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiWebhooks.WebhookEndpointQueryService.WebhookEndpointQueryServiceClient webhooksWebhookEndpointQueryService,
        ApiWorkplaces.BuildingCalculationService.BuildingCalculationServiceClient workplacesBuildingCalculationService)
    {
        _auditAuditEventValidationService = auditAuditEventValidationService;
        _benefitsEnrollmentQueryService = benefitsEnrollmentQueryService;
        _bus = bus;
        _devicesDevicePolicyQueryService = devicesDevicePolicyQueryService;
        _documentsDocumentQueryService = documentsDocumentQueryService;
        _documentsDocumentTemplateSyncService = documentsDocumentTemplateSyncService;
        _invoker = invoker;
        _labelingLabelingTaskCalculationService = labelingLabelingTaskCalculationService;
        _logger = logger;
        _metricsMetricSeriesQueryService = metricsMetricSeriesQueryService;
        _policy = policy;
        _recruitingCandidateQueryService = recruitingCandidateQueryService;
        _recruitingCandidateValidationService = recruitingCandidateValidationService;
        _recruitingInterviewCalculationService = recruitingInterviewCalculationService;
        _recruitingInterviewValidationService = recruitingInterviewValidationService;
        _recruitingJobOpeningValidationService = recruitingJobOpeningValidationService;
        _recruitingOfferValidationService = recruitingOfferValidationService;
        _repository = repository;
        _settings = settings;
        _webhooksWebhookEndpointQueryService = webhooksWebhookEndpointQueryService;
        _workplacesBuildingCalculationService = workplacesBuildingCalculationService;
        _sendDeleteJobOpening0 = _recruitingOfferValidationService.NormalizeOfferAsync;
    }
}
