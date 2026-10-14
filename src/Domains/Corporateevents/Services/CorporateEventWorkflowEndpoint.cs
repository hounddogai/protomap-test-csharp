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
using ApiCareplans = Acme.Careplans.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLocalization = Acme.Localization.V1;
using ApiSellers = Acme.Sellers.V1;

namespace Acme.Workforce.Corporateevents;

/// <summary>
/// Runs the long-lived workflow that takes a corporate event from start to finish.
/// </summary>
public sealed partial class CorporateEventWorkflowEndpoint : ApiCorporateevents.CorporateEventWorkflowService.CorporateEventWorkflowServiceBase
{
    private readonly ApiCareplans.CarePlanService.CarePlanServiceClient _careplansCarePlanService;
    private readonly ICorporateEventWorkflowEndpointSendAdvanceCorporateEventWorkflow1Port _corporateEventWorkflowEndpointSendAdvanceCorporateEventWorkflow1Port;
    private readonly ApiCorporateevents.RegistrationApprovalService.RegistrationApprovalServiceClient _corporateeventsRegistrationApprovalService;
    private readonly ApiCorporateevents.VenueReportingService.VenueReportingServiceClient _corporateeventsVenueReportingService;
    private readonly ApiCorporateevents.VenueSyncService.VenueSyncServiceClient _corporateeventsVenueSyncService;
    private readonly ApiDevices.ComplianceScanWorkflowService.ComplianceScanWorkflowServiceClient _devicesComplianceScanWorkflowService;
    private readonly ApiDevices.DevicePolicyWorkflowService.DevicePolicyWorkflowServiceClient _devicesDevicePolicyWorkflowService;
    private readonly ApiDocuments.DocumentNotificationService.DocumentNotificationServiceClient _documentsDocumentNotificationService;
    private readonly ApiDocuments.DocumentTemplateNotificationService.DocumentTemplateNotificationServiceClient _documentsDocumentTemplateNotificationService;
    private readonly ApiDocuments.SignatureCalculationService.SignatureCalculationServiceClient _documentsSignatureCalculationService;
    private readonly ApiDocuments.SignatureHistoryService.SignatureHistoryServiceClient _documentsSignatureHistoryService;
    private readonly ApiHelpdesk.TicketWorkflowService.TicketWorkflowServiceClient _helpdeskTicketWorkflowService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.ChangeRequestApprovalService.ChangeRequestApprovalServiceClient _itsmChangeRequestApprovalService;
    private readonly ApiItsm.ServiceAssetApprovalService.ServiceAssetApprovalServiceClient _itsmServiceAssetApprovalService;
    private readonly ApiLocalization.TranslationKeyService.TranslationKeyServiceClient _localizationTranslationKeyService;
    private readonly ILogger<CorporateEventWorkflowEndpoint> _logger;
    private readonly IRecordRepository _repository;
    private readonly ApiSellers.SellerCalculationService.SellerCalculationServiceClient _sellersSellerCalculationService;
    private readonly IOptions<MeshOptions> _settings;

    public CorporateEventWorkflowEndpoint(
        ApiCareplans.CarePlanService.CarePlanServiceClient careplansCarePlanService,
        ICorporateEventWorkflowEndpointSendAdvanceCorporateEventWorkflow1Port corporateEventWorkflowEndpointSendAdvanceCorporateEventWorkflow1PortAdapter,
        ApiCorporateevents.RegistrationApprovalService.RegistrationApprovalServiceClient corporateeventsRegistrationApprovalService,
        ApiCorporateevents.VenueReportingService.VenueReportingServiceClient corporateeventsVenueReportingService,
        ApiCorporateevents.VenueSyncService.VenueSyncServiceClient corporateeventsVenueSyncService,
        ApiDevices.ComplianceScanWorkflowService.ComplianceScanWorkflowServiceClient devicesComplianceScanWorkflowService,
        ApiDevices.DevicePolicyWorkflowService.DevicePolicyWorkflowServiceClient devicesDevicePolicyWorkflowService,
        ApiDocuments.DocumentNotificationService.DocumentNotificationServiceClient documentsDocumentNotificationService,
        ApiDocuments.DocumentTemplateNotificationService.DocumentTemplateNotificationServiceClient documentsDocumentTemplateNotificationService,
        ApiDocuments.SignatureCalculationService.SignatureCalculationServiceClient documentsSignatureCalculationService,
        ApiDocuments.SignatureHistoryService.SignatureHistoryServiceClient documentsSignatureHistoryService,
        ApiHelpdesk.TicketWorkflowService.TicketWorkflowServiceClient helpdeskTicketWorkflowService,
        CallInvoker invoker,
        ApiItsm.ChangeRequestApprovalService.ChangeRequestApprovalServiceClient itsmChangeRequestApprovalService,
        ApiItsm.ServiceAssetApprovalService.ServiceAssetApprovalServiceClient itsmServiceAssetApprovalService,
        ApiLocalization.TranslationKeyService.TranslationKeyServiceClient localizationTranslationKeyService,
        ILogger<CorporateEventWorkflowEndpoint> logger,
        IRecordRepository repository,
        ApiSellers.SellerCalculationService.SellerCalculationServiceClient sellersSellerCalculationService,
        IOptions<MeshOptions> settings)
    {
        _careplansCarePlanService = careplansCarePlanService;
        _corporateEventWorkflowEndpointSendAdvanceCorporateEventWorkflow1Port = corporateEventWorkflowEndpointSendAdvanceCorporateEventWorkflow1PortAdapter;
        _corporateeventsRegistrationApprovalService = corporateeventsRegistrationApprovalService;
        _corporateeventsVenueReportingService = corporateeventsVenueReportingService;
        _corporateeventsVenueSyncService = corporateeventsVenueSyncService;
        _devicesComplianceScanWorkflowService = devicesComplianceScanWorkflowService;
        _devicesDevicePolicyWorkflowService = devicesDevicePolicyWorkflowService;
        _documentsDocumentNotificationService = documentsDocumentNotificationService;
        _documentsDocumentTemplateNotificationService = documentsDocumentTemplateNotificationService;
        _documentsSignatureCalculationService = documentsSignatureCalculationService;
        _documentsSignatureHistoryService = documentsSignatureHistoryService;
        _helpdeskTicketWorkflowService = helpdeskTicketWorkflowService;
        _invoker = invoker;
        _itsmChangeRequestApprovalService = itsmChangeRequestApprovalService;
        _itsmServiceAssetApprovalService = itsmServiceAssetApprovalService;
        _localizationTranslationKeyService = localizationTranslationKeyService;
        _logger = logger;
        _repository = repository;
        _sellersSellerCalculationService = sellersSellerCalculationService;
        _settings = settings;
    }
}
