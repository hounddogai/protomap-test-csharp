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
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiGiftcards = Acme.Giftcards.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPricing = Acme.Pricing.V1;
using ApiReturns = Acme.Returns.V1;

namespace Acme.Workforce.Itsm;

/// <summary>
/// Builds and schedules reports about problems.
/// </summary>
public sealed partial class ProblemReportingEndpoint : ApiItsm.ProblemReportingService.ProblemReportingServiceBase
{
    private readonly ApiDevices.ManagedDeviceApprovalService.ManagedDeviceApprovalServiceClient _devicesManagedDeviceApprovalService;
    private readonly ApiDevices.ManagedDeviceValidationService.ManagedDeviceValidationServiceClient _devicesManagedDeviceValidationService;
    private readonly ApiDocuments.DocumentFolderValidationService.DocumentFolderValidationServiceClient _documentsDocumentFolderValidationService;
    private readonly ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient _employeesEmployeeQueryService;
    private readonly ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient _employeesEmploymentContractCalculationService;
    private readonly ApiGiftcards.GiftCardService.GiftCardServiceClient _giftcardsGiftCardService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.ChangeRequestCalculationService.ChangeRequestCalculationServiceClient _itsmChangeRequestCalculationService;
    private readonly ApiItsm.ChangeRequestReportingService.ChangeRequestReportingServiceClient _itsmChangeRequestReportingService;
    private readonly ApiItsm.ChangeRequestValidationService.ChangeRequestValidationServiceClient _itsmChangeRequestValidationService;
    private readonly ApiItsm.IncidentValidationService.IncidentValidationServiceClient _itsmIncidentValidationService;
    private readonly ApiItsm.ServiceAssetCalculationService.ServiceAssetCalculationServiceClient _itsmServiceAssetCalculationService;
    private readonly ApiItsm.ServiceAssetReportingService.ServiceAssetReportingServiceClient _itsmServiceAssetReportingService;
    private readonly ApiLicensing.SoftwareLicenseWorkflowService.SoftwareLicenseWorkflowServiceClient _licensingSoftwareLicenseWorkflowService;
    private readonly ILogger<ProblemReportingEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly ApiPricing.PriceListQueryService.PriceListQueryServiceClient _pricingPriceListQueryService;
    private readonly IProblemReportingEndpointSendStreamProblemMetrics0Port _problemReportingEndpointSendStreamProblemMetrics0Port;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiReturns.ReturnRequestQueryService.ReturnRequestQueryServiceClient _returnsReturnRequestQueryService;
    private readonly IOptions<MeshOptions> _settings;

    public ProblemReportingEndpoint(
        ApiDevices.ManagedDeviceApprovalService.ManagedDeviceApprovalServiceClient devicesManagedDeviceApprovalService,
        ApiDevices.ManagedDeviceValidationService.ManagedDeviceValidationServiceClient devicesManagedDeviceValidationService,
        ApiDocuments.DocumentFolderValidationService.DocumentFolderValidationServiceClient documentsDocumentFolderValidationService,
        ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient employeesEmployeeQueryService,
        ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient employeesEmploymentContractCalculationService,
        ApiGiftcards.GiftCardService.GiftCardServiceClient giftcardsGiftCardService,
        CallInvoker invoker,
        ApiItsm.ChangeRequestCalculationService.ChangeRequestCalculationServiceClient itsmChangeRequestCalculationService,
        ApiItsm.ChangeRequestReportingService.ChangeRequestReportingServiceClient itsmChangeRequestReportingService,
        ApiItsm.ChangeRequestValidationService.ChangeRequestValidationServiceClient itsmChangeRequestValidationService,
        ApiItsm.IncidentValidationService.IncidentValidationServiceClient itsmIncidentValidationService,
        ApiItsm.ServiceAssetCalculationService.ServiceAssetCalculationServiceClient itsmServiceAssetCalculationService,
        ApiItsm.ServiceAssetReportingService.ServiceAssetReportingServiceClient itsmServiceAssetReportingService,
        ApiLicensing.SoftwareLicenseWorkflowService.SoftwareLicenseWorkflowServiceClient licensingSoftwareLicenseWorkflowService,
        ILogger<ProblemReportingEndpoint> logger,
        RetryPolicy policy,
        ApiPricing.PriceListQueryService.PriceListQueryServiceClient pricingPriceListQueryService,
        IProblemReportingEndpointSendStreamProblemMetrics0Port problemReportingEndpointSendStreamProblemMetrics0PortAdapter,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiReturns.ReturnRequestQueryService.ReturnRequestQueryServiceClient returnsReturnRequestQueryService,
        IOptions<MeshOptions> settings)
    {
        _devicesManagedDeviceApprovalService = devicesManagedDeviceApprovalService;
        _devicesManagedDeviceValidationService = devicesManagedDeviceValidationService;
        _documentsDocumentFolderValidationService = documentsDocumentFolderValidationService;
        _employeesEmployeeQueryService = employeesEmployeeQueryService;
        _employeesEmploymentContractCalculationService = employeesEmploymentContractCalculationService;
        _giftcardsGiftCardService = giftcardsGiftCardService;
        _invoker = invoker;
        _itsmChangeRequestCalculationService = itsmChangeRequestCalculationService;
        _itsmChangeRequestReportingService = itsmChangeRequestReportingService;
        _itsmChangeRequestValidationService = itsmChangeRequestValidationService;
        _itsmIncidentValidationService = itsmIncidentValidationService;
        _itsmServiceAssetCalculationService = itsmServiceAssetCalculationService;
        _itsmServiceAssetReportingService = itsmServiceAssetReportingService;
        _licensingSoftwareLicenseWorkflowService = licensingSoftwareLicenseWorkflowService;
        _logger = logger;
        _policy = policy;
        _pricingPriceListQueryService = pricingPriceListQueryService;
        _problemReportingEndpointSendStreamProblemMetrics0Port = problemReportingEndpointSendStreamProblemMetrics0PortAdapter;
        _registry = registry;
        _repository = repository;
        _returnsReturnRequestQueryService = returnsReturnRequestQueryService;
        _settings = settings;
    }
}
