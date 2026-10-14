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
using ApiAnomalies = Acme.Anomalies.V1;
using ApiAuthorization = Acme.Authorization.V1;
using ApiCareplans = Acme.Careplans.V1;
using ApiCommon = Acme.Common.V1;
using ApiConcessions = Acme.Concessions.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLearning = Acme.Learning.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiMedia = Acme.Media.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Learning;

/// <summary>
/// Computes the figures of a learning path, such as totals, taxes, and estimates.
/// </summary>
public sealed partial class LearningPathCalculationEndpoint : ApiLearning.LearningPathCalculationService.LearningPathCalculationServiceBase
{
    private readonly ApiAnomalies.AnomalyQueryService.AnomalyQueryServiceClient _anomaliesAnomalyQueryService;
    private readonly ApiAuthorization.AccessPolicyService.AccessPolicyServiceClient _authorizationAccessPolicyService;
    private readonly ApiAuthorization.AccessPolicyValidationService.AccessPolicyValidationServiceClient _authorizationAccessPolicyValidationService;
    private readonly ApiCareplans.CarePlanService.CarePlanServiceClient _careplansCarePlanService;
    private readonly ApiConcessions.ConcessionStandCalculationService.ConcessionStandCalculationServiceClient _concessionsConcessionStandCalculationService;
    private readonly ApiCorporateevents.CorporateEventService.CorporateEventServiceClient _corporateeventsCorporateEventService;
    private readonly ApiDocuments.DocumentNotificationService.DocumentNotificationServiceClient _documentsDocumentNotificationService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.ChangeRequestCalculationService.ChangeRequestCalculationServiceClient _itsmChangeRequestCalculationService;
    private readonly ApiItsm.IncidentService.IncidentServiceClient _itsmIncidentService;
    private readonly ILearningPathCalculationEndpointSendGetLearningPathRates0Port _learningPathCalculationEndpointSendGetLearningPathRates0Port;
    private readonly ApiLicensing.LicenseAssignmentCalculationService.LicenseAssignmentCalculationServiceClient _licensingLicenseAssignmentCalculationService;
    private readonly ILogger<LearningPathCalculationEndpoint> _logger;
    private readonly ApiMedia.MediaAssetService.MediaAssetServiceClient _mediaMediaAssetService;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiConcessions.ExplainConcessionStandCalculationRequest, CallOptions, AsyncUnaryCall<ApiConcessions.ExplainConcessionStandCalculationResponse>> _sendCompareLearningPathOptions1;
    private readonly ApiWorkplaces.BuildingQueryService.BuildingQueryServiceClient _workplacesBuildingQueryService;
    private readonly ApiWorkplaces.DeskValidationService.DeskValidationServiceClient _workplacesDeskValidationService;

    public LearningPathCalculationEndpoint(
        ApiAnomalies.AnomalyQueryService.AnomalyQueryServiceClient anomaliesAnomalyQueryService,
        ApiAuthorization.AccessPolicyService.AccessPolicyServiceClient authorizationAccessPolicyService,
        ApiAuthorization.AccessPolicyValidationService.AccessPolicyValidationServiceClient authorizationAccessPolicyValidationService,
        ApiCareplans.CarePlanService.CarePlanServiceClient careplansCarePlanService,
        ApiConcessions.ConcessionStandCalculationService.ConcessionStandCalculationServiceClient concessionsConcessionStandCalculationService,
        ApiCorporateevents.CorporateEventService.CorporateEventServiceClient corporateeventsCorporateEventService,
        ApiDocuments.DocumentNotificationService.DocumentNotificationServiceClient documentsDocumentNotificationService,
        CallInvoker invoker,
        ApiItsm.ChangeRequestCalculationService.ChangeRequestCalculationServiceClient itsmChangeRequestCalculationService,
        ApiItsm.IncidentService.IncidentServiceClient itsmIncidentService,
        ILearningPathCalculationEndpointSendGetLearningPathRates0Port learningPathCalculationEndpointSendGetLearningPathRates0PortAdapter,
        ApiLicensing.LicenseAssignmentCalculationService.LicenseAssignmentCalculationServiceClient licensingLicenseAssignmentCalculationService,
        ILogger<LearningPathCalculationEndpoint> logger,
        ApiMedia.MediaAssetService.MediaAssetServiceClient mediaMediaAssetService,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiWorkplaces.BuildingQueryService.BuildingQueryServiceClient workplacesBuildingQueryService,
        ApiWorkplaces.DeskValidationService.DeskValidationServiceClient workplacesDeskValidationService)
    {
        _anomaliesAnomalyQueryService = anomaliesAnomalyQueryService;
        _authorizationAccessPolicyService = authorizationAccessPolicyService;
        _authorizationAccessPolicyValidationService = authorizationAccessPolicyValidationService;
        _careplansCarePlanService = careplansCarePlanService;
        _concessionsConcessionStandCalculationService = concessionsConcessionStandCalculationService;
        _corporateeventsCorporateEventService = corporateeventsCorporateEventService;
        _documentsDocumentNotificationService = documentsDocumentNotificationService;
        _invoker = invoker;
        _itsmChangeRequestCalculationService = itsmChangeRequestCalculationService;
        _itsmIncidentService = itsmIncidentService;
        _learningPathCalculationEndpointSendGetLearningPathRates0Port = learningPathCalculationEndpointSendGetLearningPathRates0PortAdapter;
        _licensingLicenseAssignmentCalculationService = licensingLicenseAssignmentCalculationService;
        _logger = logger;
        _mediaMediaAssetService = mediaMediaAssetService;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _workplacesBuildingQueryService = workplacesBuildingQueryService;
        _workplacesDeskValidationService = workplacesDeskValidationService;
        _sendCompareLearningPathOptions1 = _concessionsConcessionStandCalculationService.ExplainConcessionStandCalculationAsync;
    }
}
