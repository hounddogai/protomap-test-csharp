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
using ApiEdgegateway = Acme.Edgegateway.V1;
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiFanportal = Acme.Fanportal.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;

namespace Acme.Workforce.Employeeportal;

/// <summary>
/// Tells the people who follow a self service request what changed.
/// </summary>
public sealed partial class SelfServiceRequestNotificationEndpoint : ApiEmployeeportal.SelfServiceRequestNotificationService.SelfServiceRequestNotificationServiceBase
{
    private readonly NotificationBus _bus;
    private readonly ApiDocuments.DocumentFolderValidationService.DocumentFolderValidationServiceClient _documentsDocumentFolderValidationService;
    private readonly ApiDocuments.DocumentNotificationService.DocumentNotificationServiceClient _documentsDocumentNotificationService;
    private readonly ApiEdgegateway.RouteBindingService.RouteBindingServiceClient _edgegatewayRouteBindingService;
    private readonly ApiEmployeeportal.AnnouncementIntegrationService.AnnouncementIntegrationServiceClient _employeeportalAnnouncementIntegrationService;
    private readonly ApiEmployeeportal.PortalHomeIntegrationService.PortalHomeIntegrationServiceClient _employeeportalPortalHomeIntegrationService;
    private readonly ApiEmployeeportal.PortalHomeNotificationService.PortalHomeNotificationServiceClient _employeeportalPortalHomeNotificationService;
    private readonly ApiEmployeeportal.SelfServiceRequestIntegrationService.SelfServiceRequestIntegrationServiceClient _employeeportalSelfServiceRequestIntegrationService;
    private readonly ApiFanportal.FanSessionQueryService.FanSessionQueryServiceClient _fanportalFanSessionQueryService;
    private readonly ApiFeatureflags.FeatureFlagService.FeatureFlagServiceClient _featureflagsFeatureFlagService;
    private readonly ApiHelpdesk.TicketWorkflowService.TicketWorkflowServiceClient _helpdeskTicketWorkflowService;
    private readonly CallInvoker _invoker;
    private readonly ILogger<SelfServiceRequestNotificationEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly IRecordRepository _repository;
    private readonly IOptions<MeshOptions> _settings;

    public SelfServiceRequestNotificationEndpoint(
        NotificationBus bus,
        ApiDocuments.DocumentFolderValidationService.DocumentFolderValidationServiceClient documentsDocumentFolderValidationService,
        ApiDocuments.DocumentNotificationService.DocumentNotificationServiceClient documentsDocumentNotificationService,
        ApiEdgegateway.RouteBindingService.RouteBindingServiceClient edgegatewayRouteBindingService,
        ApiEmployeeportal.AnnouncementIntegrationService.AnnouncementIntegrationServiceClient employeeportalAnnouncementIntegrationService,
        ApiEmployeeportal.PortalHomeIntegrationService.PortalHomeIntegrationServiceClient employeeportalPortalHomeIntegrationService,
        ApiEmployeeportal.PortalHomeNotificationService.PortalHomeNotificationServiceClient employeeportalPortalHomeNotificationService,
        ApiEmployeeportal.SelfServiceRequestIntegrationService.SelfServiceRequestIntegrationServiceClient employeeportalSelfServiceRequestIntegrationService,
        ApiFanportal.FanSessionQueryService.FanSessionQueryServiceClient fanportalFanSessionQueryService,
        ApiFeatureflags.FeatureFlagService.FeatureFlagServiceClient featureflagsFeatureFlagService,
        ApiHelpdesk.TicketWorkflowService.TicketWorkflowServiceClient helpdeskTicketWorkflowService,
        CallInvoker invoker,
        ILogger<SelfServiceRequestNotificationEndpoint> logger,
        RetryPolicy policy,
        IRecordRepository repository,
        IOptions<MeshOptions> settings)
    {
        _bus = bus;
        _documentsDocumentFolderValidationService = documentsDocumentFolderValidationService;
        _documentsDocumentNotificationService = documentsDocumentNotificationService;
        _edgegatewayRouteBindingService = edgegatewayRouteBindingService;
        _employeeportalAnnouncementIntegrationService = employeeportalAnnouncementIntegrationService;
        _employeeportalPortalHomeIntegrationService = employeeportalPortalHomeIntegrationService;
        _employeeportalPortalHomeNotificationService = employeeportalPortalHomeNotificationService;
        _employeeportalSelfServiceRequestIntegrationService = employeeportalSelfServiceRequestIntegrationService;
        _fanportalFanSessionQueryService = fanportalFanSessionQueryService;
        _featureflagsFeatureFlagService = featureflagsFeatureFlagService;
        _helpdeskTicketWorkflowService = helpdeskTicketWorkflowService;
        _invoker = invoker;
        _logger = logger;
        _policy = policy;
        _repository = repository;
        _settings = settings;
    }
}
