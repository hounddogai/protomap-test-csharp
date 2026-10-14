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
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiModelregistry = Acme.Modelregistry.V1;

namespace Acme.Workforce.Corporateevents;

/// <summary>
/// Builds and schedules reports about corporate events.
/// </summary>
public sealed partial class CorporateEventReportingEndpoint : ApiCorporateevents.CorporateEventReportingService.CorporateEventReportingServiceBase
{
    private readonly ICorporateEventReportingEndpointSendListCorporateEventReports0Port _corporateEventReportingEndpointSendListCorporateEventReports0Port;
    private readonly ApiCorporateevents.CorporateEventCalculationService.CorporateEventCalculationServiceClient _corporateeventsCorporateEventCalculationService;
    private readonly ApiCorporateevents.CorporateEventValidationService.CorporateEventValidationServiceClient _corporateeventsCorporateEventValidationService;
    private readonly ApiCorporateevents.RegistrationCalculationService.RegistrationCalculationServiceClient _corporateeventsRegistrationCalculationService;
    private readonly ApiCorporateevents.RegistrationQueryService.RegistrationQueryServiceClient _corporateeventsRegistrationQueryService;
    private readonly ApiCorporateevents.RegistrationValidationService.RegistrationValidationServiceClient _corporateeventsRegistrationValidationService;
    private readonly ApiCorporateevents.VenueCalculationService.VenueCalculationServiceClient _corporateeventsVenueCalculationService;
    private readonly ApiCorporateevents.VenueValidationService.VenueValidationServiceClient _corporateeventsVenueValidationService;
    private readonly ApiDocuments.DocumentNotificationService.DocumentNotificationServiceClient _documentsDocumentNotificationService;
    private readonly ApiDocuments.DocumentService.DocumentServiceClient _documentsDocumentService;
    private readonly ApiIdentity.UserQueryService.UserQueryServiceClient _identityUserQueryService;
    private readonly ApiIdentity.UserValidationService.UserValidationServiceClient _identityUserValidationService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.ChangeRequestApprovalService.ChangeRequestApprovalServiceClient _itsmChangeRequestApprovalService;
    private readonly ApiLicensing.EntitlementApprovalService.EntitlementApprovalServiceClient _licensingEntitlementApprovalService;
    private readonly ApiLicensing.LicenseAssignmentWorkflowService.LicenseAssignmentWorkflowServiceClient _licensingLicenseAssignmentWorkflowService;
    private readonly ILogger<CorporateEventReportingEndpoint> _logger;
    private readonly ApiModelregistry.RegisteredModelCalculationService.RegisteredModelCalculationServiceClient _modelregistryRegisteredModelCalculationService;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiCorporateevents.FindRegistrationsRequest, CallOptions, AsyncUnaryCall<ApiCorporateevents.FindRegistrationsResponse>> _sendDeleteCorporateEventReport0;
    private readonly Func<ApiDocuments.ListDocumentsRequest, CallOptions, AsyncUnaryCall<ApiDocuments.ListDocumentsResponse>> _sendStreamCorporateEventMetrics0;
    private readonly IOptions<MeshOptions> _settings;

    public CorporateEventReportingEndpoint(
        ICorporateEventReportingEndpointSendListCorporateEventReports0Port corporateEventReportingEndpointSendListCorporateEventReports0PortAdapter,
        ApiCorporateevents.CorporateEventCalculationService.CorporateEventCalculationServiceClient corporateeventsCorporateEventCalculationService,
        ApiCorporateevents.CorporateEventValidationService.CorporateEventValidationServiceClient corporateeventsCorporateEventValidationService,
        ApiCorporateevents.RegistrationCalculationService.RegistrationCalculationServiceClient corporateeventsRegistrationCalculationService,
        ApiCorporateevents.RegistrationQueryService.RegistrationQueryServiceClient corporateeventsRegistrationQueryService,
        ApiCorporateevents.RegistrationValidationService.RegistrationValidationServiceClient corporateeventsRegistrationValidationService,
        ApiCorporateevents.VenueCalculationService.VenueCalculationServiceClient corporateeventsVenueCalculationService,
        ApiCorporateevents.VenueValidationService.VenueValidationServiceClient corporateeventsVenueValidationService,
        ApiDocuments.DocumentNotificationService.DocumentNotificationServiceClient documentsDocumentNotificationService,
        ApiDocuments.DocumentService.DocumentServiceClient documentsDocumentService,
        ApiIdentity.UserQueryService.UserQueryServiceClient identityUserQueryService,
        ApiIdentity.UserValidationService.UserValidationServiceClient identityUserValidationService,
        CallInvoker invoker,
        ApiItsm.ChangeRequestApprovalService.ChangeRequestApprovalServiceClient itsmChangeRequestApprovalService,
        ApiLicensing.EntitlementApprovalService.EntitlementApprovalServiceClient licensingEntitlementApprovalService,
        ApiLicensing.LicenseAssignmentWorkflowService.LicenseAssignmentWorkflowServiceClient licensingLicenseAssignmentWorkflowService,
        ILogger<CorporateEventReportingEndpoint> logger,
        ApiModelregistry.RegisteredModelCalculationService.RegisteredModelCalculationServiceClient modelregistryRegisteredModelCalculationService,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings)
    {
        _corporateEventReportingEndpointSendListCorporateEventReports0Port = corporateEventReportingEndpointSendListCorporateEventReports0PortAdapter;
        _corporateeventsCorporateEventCalculationService = corporateeventsCorporateEventCalculationService;
        _corporateeventsCorporateEventValidationService = corporateeventsCorporateEventValidationService;
        _corporateeventsRegistrationCalculationService = corporateeventsRegistrationCalculationService;
        _corporateeventsRegistrationQueryService = corporateeventsRegistrationQueryService;
        _corporateeventsRegistrationValidationService = corporateeventsRegistrationValidationService;
        _corporateeventsVenueCalculationService = corporateeventsVenueCalculationService;
        _corporateeventsVenueValidationService = corporateeventsVenueValidationService;
        _documentsDocumentNotificationService = documentsDocumentNotificationService;
        _documentsDocumentService = documentsDocumentService;
        _identityUserQueryService = identityUserQueryService;
        _identityUserValidationService = identityUserValidationService;
        _invoker = invoker;
        _itsmChangeRequestApprovalService = itsmChangeRequestApprovalService;
        _licensingEntitlementApprovalService = licensingEntitlementApprovalService;
        _licensingLicenseAssignmentWorkflowService = licensingLicenseAssignmentWorkflowService;
        _logger = logger;
        _modelregistryRegisteredModelCalculationService = modelregistryRegisteredModelCalculationService;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _settings = settings;
        _sendDeleteCorporateEventReport0 = _corporateeventsRegistrationQueryService.FindRegistrationsAsync;
        _sendStreamCorporateEventMetrics0 = _documentsDocumentService.ListDocumentsAsync;
    }
}
