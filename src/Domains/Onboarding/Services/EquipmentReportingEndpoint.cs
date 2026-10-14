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
using ApiAuthorization = Acme.Authorization.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDevices = Acme.Devices.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiOnboarding = Acme.Onboarding.V1;

namespace Acme.Workforce.Onboarding;

/// <summary>
/// Builds and schedules reports about equipments.
/// </summary>
public sealed partial class EquipmentReportingEndpoint : ApiOnboarding.EquipmentReportingService.EquipmentReportingServiceBase
{
    private readonly ApiAuthorization.AccessPolicyQueryService.AccessPolicyQueryServiceClient _authorizationAccessPolicyQueryService;
    private readonly NotificationBus _bus;
    private readonly ApiCorporateevents.RegistrationApprovalService.RegistrationApprovalServiceClient _corporateeventsRegistrationApprovalService;
    private readonly ApiDevices.DevicePolicyApprovalService.DevicePolicyApprovalServiceClient _devicesDevicePolicyApprovalService;
    private readonly IEquipmentReportingEndpointSendGenerateEquipmentReport2Port _equipmentReportingEndpointSendGenerateEquipmentReport2Port;
    private readonly ILogger<EquipmentReportingEndpoint> _logger;
    private readonly ApiOnboarding.EquipmentValidationService.EquipmentValidationServiceClient _onboardingEquipmentValidationService;
    private readonly ApiOnboarding.OnboardingPlanService.OnboardingPlanServiceClient _onboardingOnboardingPlanService;
    private readonly ApiOnboarding.OnboardingTaskQueryService.OnboardingTaskQueryServiceClient _onboardingOnboardingTaskQueryService;
    private readonly ApiOnboarding.OnboardingTaskValidationService.OnboardingTaskValidationServiceClient _onboardingOnboardingTaskValidationService;
    private readonly IRecordRepository _repository;
    private readonly IOptions<MeshOptions> _settings;

    public EquipmentReportingEndpoint(
        ApiAuthorization.AccessPolicyQueryService.AccessPolicyQueryServiceClient authorizationAccessPolicyQueryService,
        NotificationBus bus,
        ApiCorporateevents.RegistrationApprovalService.RegistrationApprovalServiceClient corporateeventsRegistrationApprovalService,
        ApiDevices.DevicePolicyApprovalService.DevicePolicyApprovalServiceClient devicesDevicePolicyApprovalService,
        IEquipmentReportingEndpointSendGenerateEquipmentReport2Port equipmentReportingEndpointSendGenerateEquipmentReport2PortAdapter,
        ILogger<EquipmentReportingEndpoint> logger,
        ApiOnboarding.EquipmentValidationService.EquipmentValidationServiceClient onboardingEquipmentValidationService,
        ApiOnboarding.OnboardingPlanService.OnboardingPlanServiceClient onboardingOnboardingPlanService,
        ApiOnboarding.OnboardingTaskQueryService.OnboardingTaskQueryServiceClient onboardingOnboardingTaskQueryService,
        ApiOnboarding.OnboardingTaskValidationService.OnboardingTaskValidationServiceClient onboardingOnboardingTaskValidationService,
        IRecordRepository repository,
        IOptions<MeshOptions> settings)
    {
        _authorizationAccessPolicyQueryService = authorizationAccessPolicyQueryService;
        _bus = bus;
        _corporateeventsRegistrationApprovalService = corporateeventsRegistrationApprovalService;
        _devicesDevicePolicyApprovalService = devicesDevicePolicyApprovalService;
        _equipmentReportingEndpointSendGenerateEquipmentReport2Port = equipmentReportingEndpointSendGenerateEquipmentReport2PortAdapter;
        _logger = logger;
        _onboardingEquipmentValidationService = onboardingEquipmentValidationService;
        _onboardingOnboardingPlanService = onboardingOnboardingPlanService;
        _onboardingOnboardingTaskQueryService = onboardingOnboardingTaskQueryService;
        _onboardingOnboardingTaskValidationService = onboardingOnboardingTaskValidationService;
        _repository = repository;
        _settings = settings;
    }
}
