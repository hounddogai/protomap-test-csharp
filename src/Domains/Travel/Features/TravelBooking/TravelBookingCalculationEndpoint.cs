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
using ApiDevices = Acme.Devices.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

/// <summary>
/// Computes the figures of a travel booking, such as totals, taxes, and estimates.
/// </summary>
public sealed partial class TravelBookingCalculationEndpoint : ApiTravel.TravelBookingCalculationService.TravelBookingCalculationServiceBase
{
    private readonly ApiAudit.AuditEventQueryService.AuditEventQueryServiceClient _auditAuditEventQueryService;
    private readonly NotificationBus _bus;
    private readonly ApiDevices.ComplianceScanCalculationService.ComplianceScanCalculationServiceClient _devicesComplianceScanCalculationService;
    private readonly ApiFeatureflags.FeatureFlagValidationService.FeatureFlagValidationServiceClient _featureflagsFeatureFlagValidationService;
    private readonly ApiHelpdesk.SlaPolicyCalculationService.SlaPolicyCalculationServiceClient _helpdeskSlaPolicyCalculationService;
    private readonly CallInvoker _invoker;
    private readonly ILogger<TravelBookingCalculationEndpoint> _logger;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiTravel.CalculateTripRequest, CallOptions, AsyncUnaryCall<ApiTravel.CalculateTripResponse>> _sendRecalculateTravelBookings0;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ITravelBookingCalculationEndpointSendEstimateTravelBooking0Port _travelBookingCalculationEndpointSendEstimateTravelBooking0Port;
    private readonly ApiTravel.TripCalculationService.TripCalculationServiceClient _travelTripCalculationService;

    public TravelBookingCalculationEndpoint(
        ApiAudit.AuditEventQueryService.AuditEventQueryServiceClient auditAuditEventQueryService,
        NotificationBus bus,
        ApiDevices.ComplianceScanCalculationService.ComplianceScanCalculationServiceClient devicesComplianceScanCalculationService,
        ApiFeatureflags.FeatureFlagValidationService.FeatureFlagValidationServiceClient featureflagsFeatureFlagValidationService,
        ApiHelpdesk.SlaPolicyCalculationService.SlaPolicyCalculationServiceClient helpdeskSlaPolicyCalculationService,
        CallInvoker invoker,
        ILogger<TravelBookingCalculationEndpoint> logger,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ITravelBookingCalculationEndpointSendEstimateTravelBooking0Port travelBookingCalculationEndpointSendEstimateTravelBooking0PortAdapter,
        ApiTravel.TripCalculationService.TripCalculationServiceClient travelTripCalculationService)
    {
        _auditAuditEventQueryService = auditAuditEventQueryService;
        _bus = bus;
        _devicesComplianceScanCalculationService = devicesComplianceScanCalculationService;
        _featureflagsFeatureFlagValidationService = featureflagsFeatureFlagValidationService;
        _helpdeskSlaPolicyCalculationService = helpdeskSlaPolicyCalculationService;
        _invoker = invoker;
        _logger = logger;
        _registry = registry;
        _repository = repository;
        _settings = settings;
        _travelBookingCalculationEndpointSendEstimateTravelBooking0Port = travelBookingCalculationEndpointSendEstimateTravelBooking0PortAdapter;
        _travelTripCalculationService = travelTripCalculationService;
        _sendRecalculateTravelBookings0 = _travelTripCalculationService.CalculateTripAsync;
    }
}
