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
using ApiAudiences = Acme.Audiences.V1;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiSeating = Acme.Seating.V1;

namespace Acme.Workforce.Helpdesk;

/// <summary>
/// Computes the figures of a sla policy, such as totals, taxes, and estimates.
/// </summary>
public sealed partial class SlaPolicyCalculationEndpoint : ApiHelpdesk.SlaPolicyCalculationService.SlaPolicyCalculationServiceBase
{
    private readonly ApiAudiences.AudienceQueryService.AudienceQueryServiceClient _audiencesAudienceQueryService;
    private readonly ApiDevices.ComplianceScanCalculationService.ComplianceScanCalculationServiceClient _devicesComplianceScanCalculationService;
    private readonly ApiDevices.ManagedDeviceQueryService.ManagedDeviceQueryServiceClient _devicesManagedDeviceQueryService;
    private readonly ApiHelpdesk.KnowledgeArticleCalculationService.KnowledgeArticleCalculationServiceClient _helpdeskKnowledgeArticleCalculationService;
    private readonly ApiHelpdesk.TicketValidationService.TicketValidationServiceClient _helpdeskTicketValidationService;
    private readonly ApiIdentity.UserValidationService.UserValidationServiceClient _identityUserValidationService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.IncidentQueryService.IncidentQueryServiceClient _itsmIncidentQueryService;
    private readonly ApiItsm.ServiceAssetCalculationService.ServiceAssetCalculationServiceClient _itsmServiceAssetCalculationService;
    private readonly ApiLicensing.EntitlementValidationService.EntitlementValidationServiceClient _licensingEntitlementValidationService;
    private readonly ApiLicensing.LicenseAssignmentValidationService.LicenseAssignmentValidationServiceClient _licensingLicenseAssignmentValidationService;
    private readonly ILogger<SlaPolicyCalculationEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly IRecordRepository _repository;
    private readonly ApiSeating.SeatMapQueryService.SeatMapQueryServiceClient _seatingSeatMapQueryService;
    private readonly IOptions<MeshOptions> _settings;

    public SlaPolicyCalculationEndpoint(
        ApiAudiences.AudienceQueryService.AudienceQueryServiceClient audiencesAudienceQueryService,
        ApiDevices.ComplianceScanCalculationService.ComplianceScanCalculationServiceClient devicesComplianceScanCalculationService,
        ApiDevices.ManagedDeviceQueryService.ManagedDeviceQueryServiceClient devicesManagedDeviceQueryService,
        ApiHelpdesk.KnowledgeArticleCalculationService.KnowledgeArticleCalculationServiceClient helpdeskKnowledgeArticleCalculationService,
        ApiHelpdesk.TicketValidationService.TicketValidationServiceClient helpdeskTicketValidationService,
        ApiIdentity.UserValidationService.UserValidationServiceClient identityUserValidationService,
        CallInvoker invoker,
        ApiItsm.IncidentQueryService.IncidentQueryServiceClient itsmIncidentQueryService,
        ApiItsm.ServiceAssetCalculationService.ServiceAssetCalculationServiceClient itsmServiceAssetCalculationService,
        ApiLicensing.EntitlementValidationService.EntitlementValidationServiceClient licensingEntitlementValidationService,
        ApiLicensing.LicenseAssignmentValidationService.LicenseAssignmentValidationServiceClient licensingLicenseAssignmentValidationService,
        ILogger<SlaPolicyCalculationEndpoint> logger,
        RetryPolicy policy,
        IRecordRepository repository,
        ApiSeating.SeatMapQueryService.SeatMapQueryServiceClient seatingSeatMapQueryService,
        IOptions<MeshOptions> settings)
    {
        _audiencesAudienceQueryService = audiencesAudienceQueryService;
        _devicesComplianceScanCalculationService = devicesComplianceScanCalculationService;
        _devicesManagedDeviceQueryService = devicesManagedDeviceQueryService;
        _helpdeskKnowledgeArticleCalculationService = helpdeskKnowledgeArticleCalculationService;
        _helpdeskTicketValidationService = helpdeskTicketValidationService;
        _identityUserValidationService = identityUserValidationService;
        _invoker = invoker;
        _itsmIncidentQueryService = itsmIncidentQueryService;
        _itsmServiceAssetCalculationService = itsmServiceAssetCalculationService;
        _licensingEntitlementValidationService = licensingEntitlementValidationService;
        _licensingLicenseAssignmentValidationService = licensingLicenseAssignmentValidationService;
        _logger = logger;
        _policy = policy;
        _repository = repository;
        _seatingSeatMapQueryService = seatingSeatMapQueryService;
        _settings = settings;
    }
}
