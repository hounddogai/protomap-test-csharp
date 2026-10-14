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
using ApiDevices = Acme.Devices.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiLoyalty = Acme.Loyalty.V1;

namespace Acme.Workforce.Corporateevents;

/// <summary>
/// Routes venues through approval, with escalation and delegation.
/// </summary>
public sealed partial class VenueApprovalEndpoint : ApiCorporateevents.VenueApprovalService.VenueApprovalServiceBase
{
    private readonly ApiCorporateevents.CorporateEventLifecycleService.CorporateEventLifecycleServiceClient _corporateeventsCorporateEventLifecycleService;
    private readonly ApiCorporateevents.CorporateEventReportingService.CorporateEventReportingServiceClient _corporateeventsCorporateEventReportingService;
    private readonly ApiCorporateevents.CorporateEventService.CorporateEventServiceClient _corporateeventsCorporateEventService;
    private readonly ApiCorporateevents.CorporateEventWorkflowService.CorporateEventWorkflowServiceClient _corporateeventsCorporateEventWorkflowService;
    private readonly ApiCorporateevents.RegistrationValidationService.RegistrationValidationServiceClient _corporateeventsRegistrationValidationService;
    private readonly ApiCorporateevents.VenueReportingService.VenueReportingServiceClient _corporateeventsVenueReportingService;
    private readonly ApiDevices.ManagedDeviceWorkflowService.ManagedDeviceWorkflowServiceClient _devicesManagedDeviceWorkflowService;
    private readonly ApiItsm.IncidentApprovalService.IncidentApprovalServiceClient _itsmIncidentApprovalService;
    private readonly ApiLicensing.EntitlementWorkflowService.EntitlementWorkflowServiceClient _licensingEntitlementWorkflowService;
    private readonly ApiLicensing.LicenseAssignmentWorkflowService.LicenseAssignmentWorkflowServiceClient _licensingLicenseAssignmentWorkflowService;
    private readonly ILogger<VenueApprovalEndpoint> _logger;
    private readonly ApiLoyalty.LoyaltyMemberService.LoyaltyMemberServiceClient _loyaltyLoyaltyMemberService;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly IOptions<MeshOptions> _settings;

    public VenueApprovalEndpoint(
        ApiCorporateevents.CorporateEventLifecycleService.CorporateEventLifecycleServiceClient corporateeventsCorporateEventLifecycleService,
        ApiCorporateevents.CorporateEventReportingService.CorporateEventReportingServiceClient corporateeventsCorporateEventReportingService,
        ApiCorporateevents.CorporateEventService.CorporateEventServiceClient corporateeventsCorporateEventService,
        ApiCorporateevents.CorporateEventWorkflowService.CorporateEventWorkflowServiceClient corporateeventsCorporateEventWorkflowService,
        ApiCorporateevents.RegistrationValidationService.RegistrationValidationServiceClient corporateeventsRegistrationValidationService,
        ApiCorporateevents.VenueReportingService.VenueReportingServiceClient corporateeventsVenueReportingService,
        ApiDevices.ManagedDeviceWorkflowService.ManagedDeviceWorkflowServiceClient devicesManagedDeviceWorkflowService,
        ApiItsm.IncidentApprovalService.IncidentApprovalServiceClient itsmIncidentApprovalService,
        ApiLicensing.EntitlementWorkflowService.EntitlementWorkflowServiceClient licensingEntitlementWorkflowService,
        ApiLicensing.LicenseAssignmentWorkflowService.LicenseAssignmentWorkflowServiceClient licensingLicenseAssignmentWorkflowService,
        ILogger<VenueApprovalEndpoint> logger,
        ApiLoyalty.LoyaltyMemberService.LoyaltyMemberServiceClient loyaltyLoyaltyMemberService,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings)
    {
        _corporateeventsCorporateEventLifecycleService = corporateeventsCorporateEventLifecycleService;
        _corporateeventsCorporateEventReportingService = corporateeventsCorporateEventReportingService;
        _corporateeventsCorporateEventService = corporateeventsCorporateEventService;
        _corporateeventsCorporateEventWorkflowService = corporateeventsCorporateEventWorkflowService;
        _corporateeventsRegistrationValidationService = corporateeventsRegistrationValidationService;
        _corporateeventsVenueReportingService = corporateeventsVenueReportingService;
        _devicesManagedDeviceWorkflowService = devicesManagedDeviceWorkflowService;
        _itsmIncidentApprovalService = itsmIncidentApprovalService;
        _licensingEntitlementWorkflowService = licensingEntitlementWorkflowService;
        _licensingLicenseAssignmentWorkflowService = licensingLicenseAssignmentWorkflowService;
        _logger = logger;
        _loyaltyLoyaltyMemberService = loyaltyLoyaltyMemberService;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _settings = settings;
    }
}
