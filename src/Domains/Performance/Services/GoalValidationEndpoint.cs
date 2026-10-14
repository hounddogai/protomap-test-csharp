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
using ApiCommon = Acme.Common.V1;
using ApiCommunity = Acme.Community.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiMedia = Acme.Media.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTravel = Acme.Travel.V1;
using ApiVenues = Acme.Venues.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Performance;

/// <summary>
/// Checks goals against the rules before anything acts on them.
/// </summary>
public sealed partial class GoalValidationEndpoint : ApiPerformance.GoalValidationService.GoalValidationServiceBase
{
    private readonly ApiAudit.AuditEventQueryService.AuditEventQueryServiceClient _auditAuditEventQueryService;
    private readonly ApiCommunity.ForumService.ForumServiceClient _communityForumService;
    private readonly ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient _documentsDocumentTemplateSyncService;
    private readonly ApiDocuments.SignatureCalculationService.SignatureCalculationServiceClient _documentsSignatureCalculationService;
    private readonly ApiHelpdesk.SlaPolicyValidationService.SlaPolicyValidationServiceClient _helpdeskSlaPolicyValidationService;
    private readonly ApiIdentity.UserValidationService.UserValidationServiceClient _identityUserValidationService;
    private readonly CallInvoker _invoker;
    private readonly ILogger<GoalValidationEndpoint> _logger;
    private readonly ApiMedia.MediaAssetService.MediaAssetServiceClient _mediaMediaAssetService;
    private readonly ApiPerformance.FeedbackCalculationService.FeedbackCalculationServiceClient _performanceFeedbackCalculationService;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiIdentity.ExplainUserDecisionRequest, CallOptions, AsyncUnaryCall<ApiIdentity.ExplainUserDecisionResponse>> _sendLintGoals1;
    private readonly ApiTravel.TripCalculationService.TripCalculationServiceClient _travelTripCalculationService;
    private readonly ApiVenues.VenueService.VenueServiceClient _venuesVenueService;
    private readonly ApiWorkplaces.BuildingCalculationService.BuildingCalculationServiceClient _workplacesBuildingCalculationService;

    public GoalValidationEndpoint(
        ApiAudit.AuditEventQueryService.AuditEventQueryServiceClient auditAuditEventQueryService,
        ApiCommunity.ForumService.ForumServiceClient communityForumService,
        ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient documentsDocumentTemplateSyncService,
        ApiDocuments.SignatureCalculationService.SignatureCalculationServiceClient documentsSignatureCalculationService,
        ApiHelpdesk.SlaPolicyValidationService.SlaPolicyValidationServiceClient helpdeskSlaPolicyValidationService,
        ApiIdentity.UserValidationService.UserValidationServiceClient identityUserValidationService,
        CallInvoker invoker,
        ILogger<GoalValidationEndpoint> logger,
        ApiMedia.MediaAssetService.MediaAssetServiceClient mediaMediaAssetService,
        ApiPerformance.FeedbackCalculationService.FeedbackCalculationServiceClient performanceFeedbackCalculationService,
        IRecordRepository repository,
        ApiTravel.TripCalculationService.TripCalculationServiceClient travelTripCalculationService,
        ApiVenues.VenueService.VenueServiceClient venuesVenueService,
        ApiWorkplaces.BuildingCalculationService.BuildingCalculationServiceClient workplacesBuildingCalculationService)
    {
        _auditAuditEventQueryService = auditAuditEventQueryService;
        _communityForumService = communityForumService;
        _documentsDocumentTemplateSyncService = documentsDocumentTemplateSyncService;
        _documentsSignatureCalculationService = documentsSignatureCalculationService;
        _helpdeskSlaPolicyValidationService = helpdeskSlaPolicyValidationService;
        _identityUserValidationService = identityUserValidationService;
        _invoker = invoker;
        _logger = logger;
        _mediaMediaAssetService = mediaMediaAssetService;
        _performanceFeedbackCalculationService = performanceFeedbackCalculationService;
        _repository = repository;
        _travelTripCalculationService = travelTripCalculationService;
        _venuesVenueService = venuesVenueService;
        _workplacesBuildingCalculationService = workplacesBuildingCalculationService;
        _sendLintGoals1 = _identityUserValidationService.ExplainUserDecisionAsync;
    }
}
