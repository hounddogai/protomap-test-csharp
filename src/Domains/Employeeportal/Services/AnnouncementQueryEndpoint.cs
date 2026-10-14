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
using ApiDocuments = Acme.Documents.V1;
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiSurveys = Acme.Surveys.V1;

namespace Acme.Workforce.Employeeportal;

/// <summary>
/// Answers read-only questions about announcements, for screens and reports.
/// </summary>
public sealed partial class AnnouncementQueryEndpoint : ApiEmployeeportal.AnnouncementQueryService.AnnouncementQueryServiceBase
{
    private readonly ApiDocuments.SignatureValidationService.SignatureValidationServiceClient _documentsSignatureValidationService;
    private readonly ApiEmployeeportal.AnnouncementIntegrationService.AnnouncementIntegrationServiceClient _employeeportalAnnouncementIntegrationService;
    private readonly ApiEmployeeportal.PortalHomeIntegrationService.PortalHomeIntegrationServiceClient _employeeportalPortalHomeIntegrationService;
    private readonly ApiEmployeeportal.PortalHomeQueryService.PortalHomeQueryServiceClient _employeeportalPortalHomeQueryService;
    private readonly ApiEmployeeportal.SelfServiceRequestQueryService.SelfServiceRequestQueryServiceClient _employeeportalSelfServiceRequestQueryService;
    private readonly ApiLedger.LedgerAccountQueryService.LedgerAccountQueryServiceClient _ledgerLedgerAccountQueryService;
    private readonly ILogger<AnnouncementQueryEndpoint> _logger;
    private readonly ApiMetrics.MetricSeriesService.MetricSeriesServiceClient _metricsMetricSeriesService;
    private readonly IRecordRepository _repository;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiSurveys.SurveyService.SurveyServiceClient _surveysSurveyService;

    public AnnouncementQueryEndpoint(
        ApiDocuments.SignatureValidationService.SignatureValidationServiceClient documentsSignatureValidationService,
        ApiEmployeeportal.AnnouncementIntegrationService.AnnouncementIntegrationServiceClient employeeportalAnnouncementIntegrationService,
        ApiEmployeeportal.PortalHomeIntegrationService.PortalHomeIntegrationServiceClient employeeportalPortalHomeIntegrationService,
        ApiEmployeeportal.PortalHomeQueryService.PortalHomeQueryServiceClient employeeportalPortalHomeQueryService,
        ApiEmployeeportal.SelfServiceRequestQueryService.SelfServiceRequestQueryServiceClient employeeportalSelfServiceRequestQueryService,
        ApiLedger.LedgerAccountQueryService.LedgerAccountQueryServiceClient ledgerLedgerAccountQueryService,
        ILogger<AnnouncementQueryEndpoint> logger,
        ApiMetrics.MetricSeriesService.MetricSeriesServiceClient metricsMetricSeriesService,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiSurveys.SurveyService.SurveyServiceClient surveysSurveyService)
    {
        _documentsSignatureValidationService = documentsSignatureValidationService;
        _employeeportalAnnouncementIntegrationService = employeeportalAnnouncementIntegrationService;
        _employeeportalPortalHomeIntegrationService = employeeportalPortalHomeIntegrationService;
        _employeeportalPortalHomeQueryService = employeeportalPortalHomeQueryService;
        _employeeportalSelfServiceRequestQueryService = employeeportalSelfServiceRequestQueryService;
        _ledgerLedgerAccountQueryService = ledgerLedgerAccountQueryService;
        _logger = logger;
        _metricsMetricSeriesService = metricsMetricSeriesService;
        _repository = repository;
        _settings = settings;
        _surveysSurveyService = surveysSurveyService;
    }
}
