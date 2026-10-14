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
using ApiDeliveries = Acme.Deliveries.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiRisk = Acme.Risk.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Corporateevents;

/// <summary>
/// Builds and schedules reports about registrations.
/// </summary>
public sealed partial class RegistrationReportingEndpoint : ApiCorporateevents.RegistrationReportingService.RegistrationReportingServiceBase
{
    private readonly ApiAuthorization.AccessPolicyQueryService.AccessPolicyQueryServiceClient _authorizationAccessPolicyQueryService;
    private readonly ApiCorporateevents.CorporateEventCalculationService.CorporateEventCalculationServiceClient _corporateeventsCorporateEventCalculationService;
    private readonly ApiCorporateevents.VenueSyncService.VenueSyncServiceClient _corporateeventsVenueSyncService;
    private readonly ApiCorporateevents.VenueValidationService.VenueValidationServiceClient _corporateeventsVenueValidationService;
    private readonly ApiDeliveries.DeliveryQueryService.DeliveryQueryServiceClient _deliveriesDeliveryQueryService;
    private readonly ApiDevices.DevicePolicyWorkflowService.DevicePolicyWorkflowServiceClient _devicesDevicePolicyWorkflowService;
    private readonly ApiDocuments.DocumentFolderValidationService.DocumentFolderValidationServiceClient _documentsDocumentFolderValidationService;
    private readonly ApiHelpdesk.TicketWorkflowService.TicketWorkflowServiceClient _helpdeskTicketWorkflowService;
    private readonly ApiIdentity.UserQueryService.UserQueryServiceClient _identityUserQueryService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.ChangeRequestReportingService.ChangeRequestReportingServiceClient _itsmChangeRequestReportingService;
    private readonly ApiItsm.ProblemReportingService.ProblemReportingServiceClient _itsmProblemReportingService;
    private readonly ApiLicensing.EntitlementLifecycleService.EntitlementLifecycleServiceClient _licensingEntitlementLifecycleService;
    private readonly ILogger<RegistrationReportingEndpoint> _logger;
    private readonly ApiMetrics.MetricSeriesValidationService.MetricSeriesValidationServiceClient _metricsMetricSeriesValidationService;
    private readonly RetryPolicy _policy;
    private readonly IRegistrationReportingEndpointSendScheduleRegistrationReport3Port _registrationReportingEndpointSendScheduleRegistrationReport3Port;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiRisk.RiskRegisterService.RiskRegisterServiceClient _riskRiskRegisterService;
    private readonly Func<ApiDocuments.NormalizeDocumentFolderRequest, CallOptions, AsyncUnaryCall<ApiDocuments.NormalizeDocumentFolderResponse>> _sendGenerateRegistrationReport0;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiVenues.VenueService.VenueServiceClient _venuesVenueService;

    public RegistrationReportingEndpoint(
        ApiAuthorization.AccessPolicyQueryService.AccessPolicyQueryServiceClient authorizationAccessPolicyQueryService,
        ApiCorporateevents.CorporateEventCalculationService.CorporateEventCalculationServiceClient corporateeventsCorporateEventCalculationService,
        ApiCorporateevents.VenueSyncService.VenueSyncServiceClient corporateeventsVenueSyncService,
        ApiCorporateevents.VenueValidationService.VenueValidationServiceClient corporateeventsVenueValidationService,
        ApiDeliveries.DeliveryQueryService.DeliveryQueryServiceClient deliveriesDeliveryQueryService,
        ApiDevices.DevicePolicyWorkflowService.DevicePolicyWorkflowServiceClient devicesDevicePolicyWorkflowService,
        ApiDocuments.DocumentFolderValidationService.DocumentFolderValidationServiceClient documentsDocumentFolderValidationService,
        ApiHelpdesk.TicketWorkflowService.TicketWorkflowServiceClient helpdeskTicketWorkflowService,
        ApiIdentity.UserQueryService.UserQueryServiceClient identityUserQueryService,
        CallInvoker invoker,
        ApiItsm.ChangeRequestReportingService.ChangeRequestReportingServiceClient itsmChangeRequestReportingService,
        ApiItsm.ProblemReportingService.ProblemReportingServiceClient itsmProblemReportingService,
        ApiLicensing.EntitlementLifecycleService.EntitlementLifecycleServiceClient licensingEntitlementLifecycleService,
        ILogger<RegistrationReportingEndpoint> logger,
        ApiMetrics.MetricSeriesValidationService.MetricSeriesValidationServiceClient metricsMetricSeriesValidationService,
        RetryPolicy policy,
        IRegistrationReportingEndpointSendScheduleRegistrationReport3Port registrationReportingEndpointSendScheduleRegistrationReport3PortAdapter,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiRisk.RiskRegisterService.RiskRegisterServiceClient riskRiskRegisterService,
        IOptions<MeshOptions> settings,
        ApiVenues.VenueService.VenueServiceClient venuesVenueService)
    {
        _authorizationAccessPolicyQueryService = authorizationAccessPolicyQueryService;
        _corporateeventsCorporateEventCalculationService = corporateeventsCorporateEventCalculationService;
        _corporateeventsVenueSyncService = corporateeventsVenueSyncService;
        _corporateeventsVenueValidationService = corporateeventsVenueValidationService;
        _deliveriesDeliveryQueryService = deliveriesDeliveryQueryService;
        _devicesDevicePolicyWorkflowService = devicesDevicePolicyWorkflowService;
        _documentsDocumentFolderValidationService = documentsDocumentFolderValidationService;
        _helpdeskTicketWorkflowService = helpdeskTicketWorkflowService;
        _identityUserQueryService = identityUserQueryService;
        _invoker = invoker;
        _itsmChangeRequestReportingService = itsmChangeRequestReportingService;
        _itsmProblemReportingService = itsmProblemReportingService;
        _licensingEntitlementLifecycleService = licensingEntitlementLifecycleService;
        _logger = logger;
        _metricsMetricSeriesValidationService = metricsMetricSeriesValidationService;
        _policy = policy;
        _registrationReportingEndpointSendScheduleRegistrationReport3Port = registrationReportingEndpointSendScheduleRegistrationReport3PortAdapter;
        _registry = registry;
        _repository = repository;
        _riskRiskRegisterService = riskRiskRegisterService;
        _settings = settings;
        _venuesVenueService = venuesVenueService;
        _sendGenerateRegistrationReport0 = _documentsDocumentFolderValidationService.NormalizeDocumentFolderAsync;
    }
}
