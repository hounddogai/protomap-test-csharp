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
using ApiBenefits = Acme.Benefits.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLearning = Acme.Learning.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiPrescriptions = Acme.Prescriptions.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Benefits;

/// <summary>
/// Computes the figures of a life event, such as totals, taxes, and estimates.
/// </summary>
public sealed partial class LifeEventCalculationEndpoint : ApiBenefits.LifeEventCalculationService.LifeEventCalculationServiceBase
{
    private readonly ApiDocuments.DocumentFolderExportService.DocumentFolderExportServiceClient _documentsDocumentFolderExportService;
    private readonly ApiDocuments.DocumentValidationService.DocumentValidationServiceClient _documentsDocumentValidationService;
    private readonly ApiDocuments.SignatureExportService.SignatureExportServiceClient _documentsSignatureExportService;
    private readonly ApiDocuments.SignatureSyncService.SignatureSyncServiceClient _documentsSignatureSyncService;
    private readonly ApiFeatureflags.FeatureFlagService.FeatureFlagServiceClient _featureflagsFeatureFlagService;
    private readonly ApiHelpdesk.TicketCalculationService.TicketCalculationServiceClient _helpdeskTicketCalculationService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.ProblemValidationService.ProblemValidationServiceClient _itsmProblemValidationService;
    private readonly ApiLearning.CertificationCalculationService.CertificationCalculationServiceClient _learningCertificationCalculationService;
    private readonly ApiLedger.LedgerAccountQueryService.LedgerAccountQueryServiceClient _ledgerLedgerAccountQueryService;
    private readonly ApiLicensing.EntitlementCalculationService.EntitlementCalculationServiceClient _licensingEntitlementCalculationService;
    private readonly ILifeEventCalculationEndpointSendRecalculateLifeEvents0Port _lifeEventCalculationEndpointSendRecalculateLifeEvents0Port;
    private readonly ILogger<LifeEventCalculationEndpoint> _logger;
    private readonly ApiPerformance.FeedbackValidationService.FeedbackValidationServiceClient _performanceFeedbackValidationService;
    private readonly RetryPolicy _policy;
    private readonly ApiPrescriptions.PrescriptionService.PrescriptionServiceClient _prescriptionsPrescriptionService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiLearning.RecalculateCertificationsRequest, CallOptions, AsyncUnaryCall<ApiLearning.RecalculateCertificationsResponse>> _sendEstimateLifeEvent2;
    private readonly ApiTravel.TravelBookingCalculationService.TravelBookingCalculationServiceClient _travelTravelBookingCalculationService;
    private readonly ApiTravel.TravelBookingValidationService.TravelBookingValidationServiceClient _travelTravelBookingValidationService;
    private readonly ApiWorkplaces.BuildingQueryService.BuildingQueryServiceClient _workplacesBuildingQueryService;

    public LifeEventCalculationEndpoint(
        ApiDocuments.DocumentFolderExportService.DocumentFolderExportServiceClient documentsDocumentFolderExportService,
        ApiDocuments.DocumentValidationService.DocumentValidationServiceClient documentsDocumentValidationService,
        ApiDocuments.SignatureExportService.SignatureExportServiceClient documentsSignatureExportService,
        ApiDocuments.SignatureSyncService.SignatureSyncServiceClient documentsSignatureSyncService,
        ApiFeatureflags.FeatureFlagService.FeatureFlagServiceClient featureflagsFeatureFlagService,
        ApiHelpdesk.TicketCalculationService.TicketCalculationServiceClient helpdeskTicketCalculationService,
        CallInvoker invoker,
        ApiItsm.ProblemValidationService.ProblemValidationServiceClient itsmProblemValidationService,
        ApiLearning.CertificationCalculationService.CertificationCalculationServiceClient learningCertificationCalculationService,
        ApiLedger.LedgerAccountQueryService.LedgerAccountQueryServiceClient ledgerLedgerAccountQueryService,
        ApiLicensing.EntitlementCalculationService.EntitlementCalculationServiceClient licensingEntitlementCalculationService,
        ILifeEventCalculationEndpointSendRecalculateLifeEvents0Port lifeEventCalculationEndpointSendRecalculateLifeEvents0PortAdapter,
        ILogger<LifeEventCalculationEndpoint> logger,
        ApiPerformance.FeedbackValidationService.FeedbackValidationServiceClient performanceFeedbackValidationService,
        RetryPolicy policy,
        ApiPrescriptions.PrescriptionService.PrescriptionServiceClient prescriptionsPrescriptionService,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiTravel.TravelBookingCalculationService.TravelBookingCalculationServiceClient travelTravelBookingCalculationService,
        ApiTravel.TravelBookingValidationService.TravelBookingValidationServiceClient travelTravelBookingValidationService,
        ApiWorkplaces.BuildingQueryService.BuildingQueryServiceClient workplacesBuildingQueryService)
    {
        _documentsDocumentFolderExportService = documentsDocumentFolderExportService;
        _documentsDocumentValidationService = documentsDocumentValidationService;
        _documentsSignatureExportService = documentsSignatureExportService;
        _documentsSignatureSyncService = documentsSignatureSyncService;
        _featureflagsFeatureFlagService = featureflagsFeatureFlagService;
        _helpdeskTicketCalculationService = helpdeskTicketCalculationService;
        _invoker = invoker;
        _itsmProblemValidationService = itsmProblemValidationService;
        _learningCertificationCalculationService = learningCertificationCalculationService;
        _ledgerLedgerAccountQueryService = ledgerLedgerAccountQueryService;
        _licensingEntitlementCalculationService = licensingEntitlementCalculationService;
        _lifeEventCalculationEndpointSendRecalculateLifeEvents0Port = lifeEventCalculationEndpointSendRecalculateLifeEvents0PortAdapter;
        _logger = logger;
        _performanceFeedbackValidationService = performanceFeedbackValidationService;
        _policy = policy;
        _prescriptionsPrescriptionService = prescriptionsPrescriptionService;
        _registry = registry;
        _repository = repository;
        _travelTravelBookingCalculationService = travelTravelBookingCalculationService;
        _travelTravelBookingValidationService = travelTravelBookingValidationService;
        _workplacesBuildingQueryService = workplacesBuildingQueryService;
        _sendEstimateLifeEvent2 = _learningCertificationCalculationService.RecalculateCertificationsAsync;
    }
}
