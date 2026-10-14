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
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFleet = Acme.Fleet.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;

namespace Acme.Workforce.Helpdesk;

/// <summary>
/// Checks sla policies against the rules before anything acts on them.
/// </summary>
public sealed partial class SlaPolicyValidationEndpoint : ApiHelpdesk.SlaPolicyValidationService.SlaPolicyValidationServiceBase
{
    private readonly ApiAuthorization.AccessPolicyQueryService.AccessPolicyQueryServiceClient _authorizationAccessPolicyQueryService;
    private readonly ApiAuthorization.AccessPolicyService.AccessPolicyServiceClient _authorizationAccessPolicyService;
    private readonly ApiDevices.ManagedDeviceValidationService.ManagedDeviceValidationServiceClient _devicesManagedDeviceValidationService;
    private readonly ApiDocuments.DocumentTemplateExportService.DocumentTemplateExportServiceClient _documentsDocumentTemplateExportService;
    private readonly ApiFleet.VehicleQueryService.VehicleQueryServiceClient _fleetVehicleQueryService;
    private readonly ApiHelpdesk.KnowledgeArticleCalculationService.KnowledgeArticleCalculationServiceClient _helpdeskKnowledgeArticleCalculationService;
    private readonly ApiHelpdesk.KnowledgeArticleValidationService.KnowledgeArticleValidationServiceClient _helpdeskKnowledgeArticleValidationService;
    private readonly ApiHelpdesk.SlaPolicyCalculationService.SlaPolicyCalculationServiceClient _helpdeskSlaPolicyCalculationService;
    private readonly ApiHelpdesk.TicketValidationService.TicketValidationServiceClient _helpdeskTicketValidationService;
    private readonly CallInvoker _invoker;
    private readonly ApiLicensing.SoftwareLicenseCalculationService.SoftwareLicenseCalculationServiceClient _licensingSoftwareLicenseCalculationService;
    private readonly ApiLicensing.SoftwareLicenseQueryService.SoftwareLicenseQueryServiceClient _licensingSoftwareLicenseQueryService;
    private readonly ILogger<SlaPolicyValidationEndpoint> _logger;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiDevices.ScoreManagedDeviceRequest, CallOptions, AsyncUnaryCall<ApiDevices.ScoreManagedDeviceResponse>> _sendExplainSlaPolicyDecision1;
    private readonly ISlaPolicyValidationEndpointSendPreviewSlaPolicy3Port _slaPolicyValidationEndpointSendPreviewSlaPolicy3Port;

    public SlaPolicyValidationEndpoint(
        ApiAuthorization.AccessPolicyQueryService.AccessPolicyQueryServiceClient authorizationAccessPolicyQueryService,
        ApiAuthorization.AccessPolicyService.AccessPolicyServiceClient authorizationAccessPolicyService,
        ApiDevices.ManagedDeviceValidationService.ManagedDeviceValidationServiceClient devicesManagedDeviceValidationService,
        ApiDocuments.DocumentTemplateExportService.DocumentTemplateExportServiceClient documentsDocumentTemplateExportService,
        ApiFleet.VehicleQueryService.VehicleQueryServiceClient fleetVehicleQueryService,
        ApiHelpdesk.KnowledgeArticleCalculationService.KnowledgeArticleCalculationServiceClient helpdeskKnowledgeArticleCalculationService,
        ApiHelpdesk.KnowledgeArticleValidationService.KnowledgeArticleValidationServiceClient helpdeskKnowledgeArticleValidationService,
        ApiHelpdesk.SlaPolicyCalculationService.SlaPolicyCalculationServiceClient helpdeskSlaPolicyCalculationService,
        ApiHelpdesk.TicketValidationService.TicketValidationServiceClient helpdeskTicketValidationService,
        CallInvoker invoker,
        ApiLicensing.SoftwareLicenseCalculationService.SoftwareLicenseCalculationServiceClient licensingSoftwareLicenseCalculationService,
        ApiLicensing.SoftwareLicenseQueryService.SoftwareLicenseQueryServiceClient licensingSoftwareLicenseQueryService,
        ILogger<SlaPolicyValidationEndpoint> logger,
        ClientRegistry registry,
        IRecordRepository repository,
        ISlaPolicyValidationEndpointSendPreviewSlaPolicy3Port slaPolicyValidationEndpointSendPreviewSlaPolicy3PortAdapter)
    {
        _authorizationAccessPolicyQueryService = authorizationAccessPolicyQueryService;
        _authorizationAccessPolicyService = authorizationAccessPolicyService;
        _devicesManagedDeviceValidationService = devicesManagedDeviceValidationService;
        _documentsDocumentTemplateExportService = documentsDocumentTemplateExportService;
        _fleetVehicleQueryService = fleetVehicleQueryService;
        _helpdeskKnowledgeArticleCalculationService = helpdeskKnowledgeArticleCalculationService;
        _helpdeskKnowledgeArticleValidationService = helpdeskKnowledgeArticleValidationService;
        _helpdeskSlaPolicyCalculationService = helpdeskSlaPolicyCalculationService;
        _helpdeskTicketValidationService = helpdeskTicketValidationService;
        _invoker = invoker;
        _licensingSoftwareLicenseCalculationService = licensingSoftwareLicenseCalculationService;
        _licensingSoftwareLicenseQueryService = licensingSoftwareLicenseQueryService;
        _logger = logger;
        _registry = registry;
        _repository = repository;
        _slaPolicyValidationEndpointSendPreviewSlaPolicy3Port = slaPolicyValidationEndpointSendPreviewSlaPolicy3PortAdapter;
        _sendExplainSlaPolicyDecision1 = _devicesManagedDeviceValidationService.ScoreManagedDeviceAsync;
    }
}
