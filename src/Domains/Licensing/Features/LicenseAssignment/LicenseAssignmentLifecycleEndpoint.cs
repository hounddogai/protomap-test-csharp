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
using ApiFleet = Acme.Fleet.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiInsurance = Acme.Insurance.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Licensing;

/// <summary>
/// Moves a license assignment between the states of its lifecycle.
/// </summary>
public sealed partial class LicenseAssignmentLifecycleEndpoint : ApiLicensing.LicenseAssignmentLifecycleService.LicenseAssignmentLifecycleServiceBase
{
    private readonly ApiDocuments.DocumentFolderSyncService.DocumentFolderSyncServiceClient _documentsDocumentFolderSyncService;
    private readonly ApiDocuments.DocumentHistoryService.DocumentHistoryServiceClient _documentsDocumentHistoryService;
    private readonly ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient _documentsDocumentTemplateSyncService;
    private readonly ApiFleet.VehicleService.VehicleServiceClient _fleetVehicleService;
    private readonly ApiInsurance.PolicyQueryService.PolicyQueryServiceClient _insurancePolicyQueryService;
    private readonly CallInvoker _invoker;
    private readonly ILicenseAssignmentLifecycleEndpointSendReinstateLicenseAssignment0Port _licenseAssignmentLifecycleEndpointSendReinstateLicenseAssignment0Port;
    private readonly ApiLicensing.EntitlementCalculationService.EntitlementCalculationServiceClient _licensingEntitlementCalculationService;
    private readonly ApiLicensing.EntitlementQueryService.EntitlementQueryServiceClient _licensingEntitlementQueryService;
    private readonly ApiLicensing.EntitlementValidationService.EntitlementValidationServiceClient _licensingEntitlementValidationService;
    private readonly ApiLicensing.LicenseAssignmentReportingService.LicenseAssignmentReportingServiceClient _licensingLicenseAssignmentReportingService;
    private readonly ApiLicensing.SoftwareLicenseQueryService.SoftwareLicenseQueryServiceClient _licensingSoftwareLicenseQueryService;
    private readonly ApiLicensing.SoftwareLicenseService.SoftwareLicenseServiceClient _licensingSoftwareLicenseService;
    private readonly ILogger<LicenseAssignmentLifecycleEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiLicensing.ValidateEntitlementRequest, CallOptions, AsyncUnaryCall<ApiLicensing.ValidateEntitlementResponse>> _sendTerminateLicenseAssignment0;
    private readonly ApiTravel.ItineraryValidationService.ItineraryValidationServiceClient _travelItineraryValidationService;

    public LicenseAssignmentLifecycleEndpoint(
        ApiDocuments.DocumentFolderSyncService.DocumentFolderSyncServiceClient documentsDocumentFolderSyncService,
        ApiDocuments.DocumentHistoryService.DocumentHistoryServiceClient documentsDocumentHistoryService,
        ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient documentsDocumentTemplateSyncService,
        ApiFleet.VehicleService.VehicleServiceClient fleetVehicleService,
        ApiInsurance.PolicyQueryService.PolicyQueryServiceClient insurancePolicyQueryService,
        CallInvoker invoker,
        ILicenseAssignmentLifecycleEndpointSendReinstateLicenseAssignment0Port licenseAssignmentLifecycleEndpointSendReinstateLicenseAssignment0PortAdapter,
        ApiLicensing.EntitlementCalculationService.EntitlementCalculationServiceClient licensingEntitlementCalculationService,
        ApiLicensing.EntitlementQueryService.EntitlementQueryServiceClient licensingEntitlementQueryService,
        ApiLicensing.EntitlementValidationService.EntitlementValidationServiceClient licensingEntitlementValidationService,
        ApiLicensing.LicenseAssignmentReportingService.LicenseAssignmentReportingServiceClient licensingLicenseAssignmentReportingService,
        ApiLicensing.SoftwareLicenseQueryService.SoftwareLicenseQueryServiceClient licensingSoftwareLicenseQueryService,
        ApiLicensing.SoftwareLicenseService.SoftwareLicenseServiceClient licensingSoftwareLicenseService,
        ILogger<LicenseAssignmentLifecycleEndpoint> logger,
        RetryPolicy policy,
        IRecordRepository repository,
        ApiTravel.ItineraryValidationService.ItineraryValidationServiceClient travelItineraryValidationService)
    {
        _documentsDocumentFolderSyncService = documentsDocumentFolderSyncService;
        _documentsDocumentHistoryService = documentsDocumentHistoryService;
        _documentsDocumentTemplateSyncService = documentsDocumentTemplateSyncService;
        _fleetVehicleService = fleetVehicleService;
        _insurancePolicyQueryService = insurancePolicyQueryService;
        _invoker = invoker;
        _licenseAssignmentLifecycleEndpointSendReinstateLicenseAssignment0Port = licenseAssignmentLifecycleEndpointSendReinstateLicenseAssignment0PortAdapter;
        _licensingEntitlementCalculationService = licensingEntitlementCalculationService;
        _licensingEntitlementQueryService = licensingEntitlementQueryService;
        _licensingEntitlementValidationService = licensingEntitlementValidationService;
        _licensingLicenseAssignmentReportingService = licensingLicenseAssignmentReportingService;
        _licensingSoftwareLicenseQueryService = licensingSoftwareLicenseQueryService;
        _licensingSoftwareLicenseService = licensingSoftwareLicenseService;
        _logger = logger;
        _policy = policy;
        _repository = repository;
        _travelItineraryValidationService = travelItineraryValidationService;
        _sendTerminateLicenseAssignment0 = _licensingEntitlementValidationService.ValidateEntitlementAsync;
    }
}
