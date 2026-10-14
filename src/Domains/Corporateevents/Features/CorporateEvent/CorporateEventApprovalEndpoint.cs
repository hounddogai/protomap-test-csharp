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
using ApiCampaigns = Acme.Campaigns.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDevices = Acme.Devices.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiSellers = Acme.Sellers.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Corporateevents;

/// <summary>
/// Routes corporate events through approval, with escalation and delegation.
/// </summary>
public sealed partial class CorporateEventApprovalEndpoint : ApiCorporateevents.CorporateEventApprovalService.CorporateEventApprovalServiceBase
{
    private readonly NotificationBus _bus;
    private readonly ApiCampaigns.MarketingCampaignService.MarketingCampaignServiceClient _campaignsMarketingCampaignService;
    private readonly ApiCorporateevents.CorporateEventLifecycleService.CorporateEventLifecycleServiceClient _corporateeventsCorporateEventLifecycleService;
    private readonly ApiCorporateevents.CorporateEventQueryService.CorporateEventQueryServiceClient _corporateeventsCorporateEventQueryService;
    private readonly ApiCorporateevents.CorporateEventReportingService.CorporateEventReportingServiceClient _corporateeventsCorporateEventReportingService;
    private readonly ApiCorporateevents.CorporateEventService.CorporateEventServiceClient _corporateeventsCorporateEventService;
    private readonly ApiCorporateevents.RegistrationLifecycleService.RegistrationLifecycleServiceClient _corporateeventsRegistrationLifecycleService;
    private readonly ApiCorporateevents.RegistrationReportingService.RegistrationReportingServiceClient _corporateeventsRegistrationReportingService;
    private readonly ApiCorporateevents.VenueSyncService.VenueSyncServiceClient _corporateeventsVenueSyncService;
    private readonly ApiDevices.DevicePolicyWorkflowService.DevicePolicyWorkflowServiceClient _devicesDevicePolicyWorkflowService;
    private readonly ApiFeatureflags.FeatureFlagValidationService.FeatureFlagValidationServiceClient _featureflagsFeatureFlagValidationService;
    private readonly ApiHelpdesk.SlaPolicyWorkflowService.SlaPolicyWorkflowServiceClient _helpdeskSlaPolicyWorkflowService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.ServiceAssetApprovalService.ServiceAssetApprovalServiceClient _itsmServiceAssetApprovalService;
    private readonly ApiLicensing.EntitlementApprovalService.EntitlementApprovalServiceClient _licensingEntitlementApprovalService;
    private readonly ILogger<CorporateEventApprovalEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiSellers.SellerQueryService.SellerQueryServiceClient _sellersSellerQueryService;
    private readonly Func<ApiSellers.BrowseSellersRequest, CallOptions, AsyncUnaryCall<ApiSellers.BrowseSellersResponse>> _sendDelegateCorporateEventApproval2;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiVenues.VenueQueryService.VenueQueryServiceClient _venuesVenueQueryService;

    public CorporateEventApprovalEndpoint(
        NotificationBus bus,
        ApiCampaigns.MarketingCampaignService.MarketingCampaignServiceClient campaignsMarketingCampaignService,
        ApiCorporateevents.CorporateEventLifecycleService.CorporateEventLifecycleServiceClient corporateeventsCorporateEventLifecycleService,
        ApiCorporateevents.CorporateEventQueryService.CorporateEventQueryServiceClient corporateeventsCorporateEventQueryService,
        ApiCorporateevents.CorporateEventReportingService.CorporateEventReportingServiceClient corporateeventsCorporateEventReportingService,
        ApiCorporateevents.CorporateEventService.CorporateEventServiceClient corporateeventsCorporateEventService,
        ApiCorporateevents.RegistrationLifecycleService.RegistrationLifecycleServiceClient corporateeventsRegistrationLifecycleService,
        ApiCorporateevents.RegistrationReportingService.RegistrationReportingServiceClient corporateeventsRegistrationReportingService,
        ApiCorporateevents.VenueSyncService.VenueSyncServiceClient corporateeventsVenueSyncService,
        ApiDevices.DevicePolicyWorkflowService.DevicePolicyWorkflowServiceClient devicesDevicePolicyWorkflowService,
        ApiFeatureflags.FeatureFlagValidationService.FeatureFlagValidationServiceClient featureflagsFeatureFlagValidationService,
        ApiHelpdesk.SlaPolicyWorkflowService.SlaPolicyWorkflowServiceClient helpdeskSlaPolicyWorkflowService,
        CallInvoker invoker,
        ApiItsm.ServiceAssetApprovalService.ServiceAssetApprovalServiceClient itsmServiceAssetApprovalService,
        ApiLicensing.EntitlementApprovalService.EntitlementApprovalServiceClient licensingEntitlementApprovalService,
        ILogger<CorporateEventApprovalEndpoint> logger,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiSellers.SellerQueryService.SellerQueryServiceClient sellersSellerQueryService,
        IOptions<MeshOptions> settings,
        ApiVenues.VenueQueryService.VenueQueryServiceClient venuesVenueQueryService)
    {
        _bus = bus;
        _campaignsMarketingCampaignService = campaignsMarketingCampaignService;
        _corporateeventsCorporateEventLifecycleService = corporateeventsCorporateEventLifecycleService;
        _corporateeventsCorporateEventQueryService = corporateeventsCorporateEventQueryService;
        _corporateeventsCorporateEventReportingService = corporateeventsCorporateEventReportingService;
        _corporateeventsCorporateEventService = corporateeventsCorporateEventService;
        _corporateeventsRegistrationLifecycleService = corporateeventsRegistrationLifecycleService;
        _corporateeventsRegistrationReportingService = corporateeventsRegistrationReportingService;
        _corporateeventsVenueSyncService = corporateeventsVenueSyncService;
        _devicesDevicePolicyWorkflowService = devicesDevicePolicyWorkflowService;
        _featureflagsFeatureFlagValidationService = featureflagsFeatureFlagValidationService;
        _helpdeskSlaPolicyWorkflowService = helpdeskSlaPolicyWorkflowService;
        _invoker = invoker;
        _itsmServiceAssetApprovalService = itsmServiceAssetApprovalService;
        _licensingEntitlementApprovalService = licensingEntitlementApprovalService;
        _logger = logger;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _sellersSellerQueryService = sellersSellerQueryService;
        _settings = settings;
        _venuesVenueQueryService = venuesVenueQueryService;
        _sendDelegateCorporateEventApproval2 = _sellersSellerQueryService.BrowseSellersAsync;
    }
}
