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
using ApiAuthorization = Acme.Authorization.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiExperiments = Acme.Experiments.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiSearch = Acme.Search.V1;

namespace Acme.Workforce.Corporateevents;

/// <summary>
/// Computes the figures of a corporate event, such as totals, taxes, and estimates.
/// </summary>
public sealed partial class CorporateEventCalculationEndpoint : ApiCorporateevents.CorporateEventCalculationService.CorporateEventCalculationServiceBase
{
    private readonly ApiAuthorization.AccessPolicyQueryService.AccessPolicyQueryServiceClient _authorizationAccessPolicyQueryService;
    private readonly ApiDevices.DevicePolicyCalculationService.DevicePolicyCalculationServiceClient _devicesDevicePolicyCalculationService;
    private readonly ApiDocuments.DocumentFolderNotificationService.DocumentFolderNotificationServiceClient _documentsDocumentFolderNotificationService;
    private readonly ApiExperiments.ExperimentCalculationService.ExperimentCalculationServiceClient _experimentsExperimentCalculationService;
    private readonly ApiHelpdesk.EscalationValidationService.EscalationValidationServiceClient _helpdeskEscalationValidationService;
    private readonly ApiHelpdesk.TicketService.TicketServiceClient _helpdeskTicketService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.ChangeRequestValidationService.ChangeRequestValidationServiceClient _itsmChangeRequestValidationService;
    private readonly ApiItsm.IncidentCalculationService.IncidentCalculationServiceClient _itsmIncidentCalculationService;
    private readonly ApiItsm.ProblemCalculationService.ProblemCalculationServiceClient _itsmProblemCalculationService;
    private readonly ApiItsm.ServiceAssetValidationService.ServiceAssetValidationServiceClient _itsmServiceAssetValidationService;
    private readonly ApiLicensing.LicenseAssignmentValidationService.LicenseAssignmentValidationServiceClient _licensingLicenseAssignmentValidationService;
    private readonly ApiLicensing.SoftwareLicenseService.SoftwareLicenseServiceClient _licensingSoftwareLicenseService;
    private readonly ILogger<CorporateEventCalculationEndpoint> _logger;
    private readonly ApiMetrics.MetricSeriesValidationService.MetricSeriesValidationServiceClient _metricsMetricSeriesValidationService;
    private readonly ApiNotifications.NotificationService.NotificationServiceClient _notificationsNotificationService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiSearch.SearchIndexValidationService.SearchIndexValidationServiceClient _searchSearchIndexValidationService;
    private readonly Func<ApiItsm.SimulateProblemRequest, CallOptions, AsyncUnaryCall<ApiItsm.SimulateProblemResponse>> _sendRecalculateCorporateEvents3;

    public CorporateEventCalculationEndpoint(
        ApiAuthorization.AccessPolicyQueryService.AccessPolicyQueryServiceClient authorizationAccessPolicyQueryService,
        ApiDevices.DevicePolicyCalculationService.DevicePolicyCalculationServiceClient devicesDevicePolicyCalculationService,
        ApiDocuments.DocumentFolderNotificationService.DocumentFolderNotificationServiceClient documentsDocumentFolderNotificationService,
        ApiExperiments.ExperimentCalculationService.ExperimentCalculationServiceClient experimentsExperimentCalculationService,
        ApiHelpdesk.EscalationValidationService.EscalationValidationServiceClient helpdeskEscalationValidationService,
        ApiHelpdesk.TicketService.TicketServiceClient helpdeskTicketService,
        CallInvoker invoker,
        ApiItsm.ChangeRequestValidationService.ChangeRequestValidationServiceClient itsmChangeRequestValidationService,
        ApiItsm.IncidentCalculationService.IncidentCalculationServiceClient itsmIncidentCalculationService,
        ApiItsm.ProblemCalculationService.ProblemCalculationServiceClient itsmProblemCalculationService,
        ApiItsm.ServiceAssetValidationService.ServiceAssetValidationServiceClient itsmServiceAssetValidationService,
        ApiLicensing.LicenseAssignmentValidationService.LicenseAssignmentValidationServiceClient licensingLicenseAssignmentValidationService,
        ApiLicensing.SoftwareLicenseService.SoftwareLicenseServiceClient licensingSoftwareLicenseService,
        ILogger<CorporateEventCalculationEndpoint> logger,
        ApiMetrics.MetricSeriesValidationService.MetricSeriesValidationServiceClient metricsMetricSeriesValidationService,
        ApiNotifications.NotificationService.NotificationServiceClient notificationsNotificationService,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiSearch.SearchIndexValidationService.SearchIndexValidationServiceClient searchSearchIndexValidationService)
    {
        _authorizationAccessPolicyQueryService = authorizationAccessPolicyQueryService;
        _devicesDevicePolicyCalculationService = devicesDevicePolicyCalculationService;
        _documentsDocumentFolderNotificationService = documentsDocumentFolderNotificationService;
        _experimentsExperimentCalculationService = experimentsExperimentCalculationService;
        _helpdeskEscalationValidationService = helpdeskEscalationValidationService;
        _helpdeskTicketService = helpdeskTicketService;
        _invoker = invoker;
        _itsmChangeRequestValidationService = itsmChangeRequestValidationService;
        _itsmIncidentCalculationService = itsmIncidentCalculationService;
        _itsmProblemCalculationService = itsmProblemCalculationService;
        _itsmServiceAssetValidationService = itsmServiceAssetValidationService;
        _licensingLicenseAssignmentValidationService = licensingLicenseAssignmentValidationService;
        _licensingSoftwareLicenseService = licensingSoftwareLicenseService;
        _logger = logger;
        _metricsMetricSeriesValidationService = metricsMetricSeriesValidationService;
        _notificationsNotificationService = notificationsNotificationService;
        _registry = registry;
        _repository = repository;
        _searchSearchIndexValidationService = searchSearchIndexValidationService;
        _sendRecalculateCorporateEvents3 = _itsmProblemCalculationService.SimulateProblemAsync;
    }
}
