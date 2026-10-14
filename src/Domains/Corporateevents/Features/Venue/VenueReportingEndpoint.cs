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
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiReturns = Acme.Returns.V1;

namespace Acme.Workforce.Corporateevents;

/// <summary>
/// Builds and schedules reports about venues.
/// </summary>
public sealed partial class VenueReportingEndpoint : ApiCorporateevents.VenueReportingService.VenueReportingServiceBase
{
    private readonly ApiCorporateevents.CorporateEventQueryService.CorporateEventQueryServiceClient _corporateeventsCorporateEventQueryService;
    private readonly ApiCorporateevents.VenueValidationService.VenueValidationServiceClient _corporateeventsVenueValidationService;
    private readonly ApiDocuments.DocumentFolderNotificationService.DocumentFolderNotificationServiceClient _documentsDocumentFolderNotificationService;
    private readonly ApiDocuments.DocumentFolderValidationService.DocumentFolderValidationServiceClient _documentsDocumentFolderValidationService;
    private readonly ApiDocuments.DocumentSyncService.DocumentSyncServiceClient _documentsDocumentSyncService;
    private readonly ApiFeatureflags.FeatureFlagService.FeatureFlagServiceClient _featureflagsFeatureFlagService;
    private readonly CallInvoker _invoker;
    private readonly ApiLicensing.LicenseAssignmentReportingService.LicenseAssignmentReportingServiceClient _licensingLicenseAssignmentReportingService;
    private readonly ILogger<VenueReportingEndpoint> _logger;
    private readonly IRecordRepository _repository;
    private readonly ApiReturns.ReturnRequestQueryService.ReturnRequestQueryServiceClient _returnsReturnRequestQueryService;
    private readonly ApiReturns.ReturnRequestService.ReturnRequestServiceClient _returnsReturnRequestService;
    private readonly Func<ApiReturns.SuggestReturnRequestsRequest, CallOptions, AsyncUnaryCall<ApiReturns.SuggestReturnRequestsResponse>> _sendDeleteVenueReport0;
    private readonly IOptions<MeshOptions> _settings;
    private readonly IVenueReportingEndpointSendShareVenueReport1Port _venueReportingEndpointSendShareVenueReport1Port;

    public VenueReportingEndpoint(
        ApiCorporateevents.CorporateEventQueryService.CorporateEventQueryServiceClient corporateeventsCorporateEventQueryService,
        ApiCorporateevents.VenueValidationService.VenueValidationServiceClient corporateeventsVenueValidationService,
        ApiDocuments.DocumentFolderNotificationService.DocumentFolderNotificationServiceClient documentsDocumentFolderNotificationService,
        ApiDocuments.DocumentFolderValidationService.DocumentFolderValidationServiceClient documentsDocumentFolderValidationService,
        ApiDocuments.DocumentSyncService.DocumentSyncServiceClient documentsDocumentSyncService,
        ApiFeatureflags.FeatureFlagService.FeatureFlagServiceClient featureflagsFeatureFlagService,
        CallInvoker invoker,
        ApiLicensing.LicenseAssignmentReportingService.LicenseAssignmentReportingServiceClient licensingLicenseAssignmentReportingService,
        ILogger<VenueReportingEndpoint> logger,
        IRecordRepository repository,
        ApiReturns.ReturnRequestQueryService.ReturnRequestQueryServiceClient returnsReturnRequestQueryService,
        ApiReturns.ReturnRequestService.ReturnRequestServiceClient returnsReturnRequestService,
        IOptions<MeshOptions> settings,
        IVenueReportingEndpointSendShareVenueReport1Port venueReportingEndpointSendShareVenueReport1PortAdapter)
    {
        _corporateeventsCorporateEventQueryService = corporateeventsCorporateEventQueryService;
        _corporateeventsVenueValidationService = corporateeventsVenueValidationService;
        _documentsDocumentFolderNotificationService = documentsDocumentFolderNotificationService;
        _documentsDocumentFolderValidationService = documentsDocumentFolderValidationService;
        _documentsDocumentSyncService = documentsDocumentSyncService;
        _featureflagsFeatureFlagService = featureflagsFeatureFlagService;
        _invoker = invoker;
        _licensingLicenseAssignmentReportingService = licensingLicenseAssignmentReportingService;
        _logger = logger;
        _repository = repository;
        _returnsReturnRequestQueryService = returnsReturnRequestQueryService;
        _returnsReturnRequestService = returnsReturnRequestService;
        _settings = settings;
        _venueReportingEndpointSendShareVenueReport1Port = venueReportingEndpointSendShareVenueReport1PortAdapter;
        _sendDeleteVenueReport0 = _returnsReturnRequestQueryService.SuggestReturnRequestsAsync;
    }
}
