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
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiDriverapp = Acme.Driverapp.V1;
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiScheduling = Acme.Scheduling.V1;

namespace Acme.Workforce.Employeeportal;

/// <summary>
/// The edge API that apps call for portal homes.
/// </summary>
public sealed partial class PortalHomeGatewayEndpoint : ApiEmployeeportal.PortalHomeGatewayService.PortalHomeGatewayServiceBase
{
    private readonly ApiCorporateevents.VenueApprovalService.VenueApprovalServiceClient _corporateeventsVenueApprovalService;
    private readonly ApiDocuments.DocumentFolderQueryService.DocumentFolderQueryServiceClient _documentsDocumentFolderQueryService;
    private readonly ApiDocuments.DocumentFolderValidationService.DocumentFolderValidationServiceClient _documentsDocumentFolderValidationService;
    private readonly ApiDocuments.DocumentService.DocumentServiceClient _documentsDocumentService;
    private readonly ApiDocuments.DocumentTemplateHistoryService.DocumentTemplateHistoryServiceClient _documentsDocumentTemplateHistoryService;
    private readonly ApiDocuments.SignatureHistoryService.SignatureHistoryServiceClient _documentsSignatureHistoryService;
    private readonly ApiDriverapp.DriverHomeService.DriverHomeServiceClient _driverappDriverHomeService;
    private readonly ApiEmployeeportal.AnnouncementIntegrationService.AnnouncementIntegrationServiceClient _employeeportalAnnouncementIntegrationService;
    private readonly ApiEmployeeportal.AnnouncementNotificationService.AnnouncementNotificationServiceClient _employeeportalAnnouncementNotificationService;
    private readonly ApiEmployeeportal.AnnouncementWorkflowService.AnnouncementWorkflowServiceClient _employeeportalAnnouncementWorkflowService;
    private readonly ApiEmployeeportal.PortalHomeNotificationService.PortalHomeNotificationServiceClient _employeeportalPortalHomeNotificationService;
    private readonly ApiEmployeeportal.SelfServiceRequestIntegrationService.SelfServiceRequestIntegrationServiceClient _employeeportalSelfServiceRequestIntegrationService;
    private readonly ApiEmployeeportal.SelfServiceRequestQueryService.SelfServiceRequestQueryServiceClient _employeeportalSelfServiceRequestQueryService;
    private readonly ApiItsm.ChangeRequestWorkflowService.ChangeRequestWorkflowServiceClient _itsmChangeRequestWorkflowService;
    private readonly ILogger<PortalHomeGatewayEndpoint> _logger;
    private readonly ApiPerformance.FeedbackWorkflowService.FeedbackWorkflowServiceClient _performanceFeedbackWorkflowService;
    private readonly IPortalHomeGatewayEndpointSendComposePortalHomePage1Port _portalHomeGatewayEndpointSendComposePortalHomePage1Port;
    private readonly IRecordRepository _repository;
    private readonly ApiScheduling.ShiftWorkflowService.ShiftWorkflowServiceClient _schedulingShiftWorkflowService;

    public PortalHomeGatewayEndpoint(
        ApiCorporateevents.VenueApprovalService.VenueApprovalServiceClient corporateeventsVenueApprovalService,
        ApiDocuments.DocumentFolderQueryService.DocumentFolderQueryServiceClient documentsDocumentFolderQueryService,
        ApiDocuments.DocumentFolderValidationService.DocumentFolderValidationServiceClient documentsDocumentFolderValidationService,
        ApiDocuments.DocumentService.DocumentServiceClient documentsDocumentService,
        ApiDocuments.DocumentTemplateHistoryService.DocumentTemplateHistoryServiceClient documentsDocumentTemplateHistoryService,
        ApiDocuments.SignatureHistoryService.SignatureHistoryServiceClient documentsSignatureHistoryService,
        ApiDriverapp.DriverHomeService.DriverHomeServiceClient driverappDriverHomeService,
        ApiEmployeeportal.AnnouncementIntegrationService.AnnouncementIntegrationServiceClient employeeportalAnnouncementIntegrationService,
        ApiEmployeeportal.AnnouncementNotificationService.AnnouncementNotificationServiceClient employeeportalAnnouncementNotificationService,
        ApiEmployeeportal.AnnouncementWorkflowService.AnnouncementWorkflowServiceClient employeeportalAnnouncementWorkflowService,
        ApiEmployeeportal.PortalHomeNotificationService.PortalHomeNotificationServiceClient employeeportalPortalHomeNotificationService,
        ApiEmployeeportal.SelfServiceRequestIntegrationService.SelfServiceRequestIntegrationServiceClient employeeportalSelfServiceRequestIntegrationService,
        ApiEmployeeportal.SelfServiceRequestQueryService.SelfServiceRequestQueryServiceClient employeeportalSelfServiceRequestQueryService,
        ApiItsm.ChangeRequestWorkflowService.ChangeRequestWorkflowServiceClient itsmChangeRequestWorkflowService,
        ILogger<PortalHomeGatewayEndpoint> logger,
        ApiPerformance.FeedbackWorkflowService.FeedbackWorkflowServiceClient performanceFeedbackWorkflowService,
        IPortalHomeGatewayEndpointSendComposePortalHomePage1Port portalHomeGatewayEndpointSendComposePortalHomePage1PortAdapter,
        IRecordRepository repository,
        ApiScheduling.ShiftWorkflowService.ShiftWorkflowServiceClient schedulingShiftWorkflowService)
    {
        _corporateeventsVenueApprovalService = corporateeventsVenueApprovalService;
        _documentsDocumentFolderQueryService = documentsDocumentFolderQueryService;
        _documentsDocumentFolderValidationService = documentsDocumentFolderValidationService;
        _documentsDocumentService = documentsDocumentService;
        _documentsDocumentTemplateHistoryService = documentsDocumentTemplateHistoryService;
        _documentsSignatureHistoryService = documentsSignatureHistoryService;
        _driverappDriverHomeService = driverappDriverHomeService;
        _employeeportalAnnouncementIntegrationService = employeeportalAnnouncementIntegrationService;
        _employeeportalAnnouncementNotificationService = employeeportalAnnouncementNotificationService;
        _employeeportalAnnouncementWorkflowService = employeeportalAnnouncementWorkflowService;
        _employeeportalPortalHomeNotificationService = employeeportalPortalHomeNotificationService;
        _employeeportalSelfServiceRequestIntegrationService = employeeportalSelfServiceRequestIntegrationService;
        _employeeportalSelfServiceRequestQueryService = employeeportalSelfServiceRequestQueryService;
        _itsmChangeRequestWorkflowService = itsmChangeRequestWorkflowService;
        _logger = logger;
        _performanceFeedbackWorkflowService = performanceFeedbackWorkflowService;
        _portalHomeGatewayEndpointSendComposePortalHomePage1Port = portalHomeGatewayEndpointSendComposePortalHomePage1PortAdapter;
        _repository = repository;
        _schedulingShiftWorkflowService = schedulingShiftWorkflowService;
    }
}
