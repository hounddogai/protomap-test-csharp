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
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPayments = Acme.Payments.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSurveys = Acme.Surveys.V1;

namespace Acme.Workforce.Licensing;

/// <summary>
/// Builds and schedules reports about license assignments.
/// </summary>
public sealed partial class LicenseAssignmentReportingEndpoint : ApiLicensing.LicenseAssignmentReportingService.LicenseAssignmentReportingServiceBase
{
    private readonly NotificationBus _bus;
    private readonly ApiDocuments.DocumentValidationService.DocumentValidationServiceClient _documentsDocumentValidationService;
    private readonly CallInvoker _invoker;
    private readonly ApiLearning.CertificationCalculationService.CertificationCalculationServiceClient _learningCertificationCalculationService;
    private readonly ILicenseAssignmentReportingEndpointSendFetchLicenseAssignmentReport0Port _licenseAssignmentReportingEndpointSendFetchLicenseAssignmentReport0Port;
    private readonly ApiLicensing.EntitlementCalculationService.EntitlementCalculationServiceClient _licensingEntitlementCalculationService;
    private readonly ApiLicensing.EntitlementQueryService.EntitlementQueryServiceClient _licensingEntitlementQueryService;
    private readonly ApiLicensing.EntitlementValidationService.EntitlementValidationServiceClient _licensingEntitlementValidationService;
    private readonly ApiLicensing.LicenseAssignmentValidationService.LicenseAssignmentValidationServiceClient _licensingLicenseAssignmentValidationService;
    private readonly ApiLicensing.SoftwareLicenseCalculationService.SoftwareLicenseCalculationServiceClient _licensingSoftwareLicenseCalculationService;
    private readonly ApiLicensing.SoftwareLicenseQueryService.SoftwareLicenseQueryServiceClient _licensingSoftwareLicenseQueryService;
    private readonly ApiLicensing.SoftwareLicenseService.SoftwareLicenseServiceClient _licensingSoftwareLicenseService;
    private readonly ApiLicensing.SoftwareLicenseValidationService.SoftwareLicenseValidationServiceClient _licensingSoftwareLicenseValidationService;
    private readonly ILogger<LicenseAssignmentReportingEndpoint> _logger;
    private readonly ApiPayments.PaymentCalculationService.PaymentCalculationServiceClient _paymentsPaymentCalculationService;
    private readonly RetryPolicy _policy;
    private readonly IRecordRepository _repository;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiSurveys.SurveyService.SurveyServiceClient _surveysSurveyService;

    public LicenseAssignmentReportingEndpoint(
        NotificationBus bus,
        ApiDocuments.DocumentValidationService.DocumentValidationServiceClient documentsDocumentValidationService,
        CallInvoker invoker,
        ApiLearning.CertificationCalculationService.CertificationCalculationServiceClient learningCertificationCalculationService,
        ILicenseAssignmentReportingEndpointSendFetchLicenseAssignmentReport0Port licenseAssignmentReportingEndpointSendFetchLicenseAssignmentReport0PortAdapter,
        ApiLicensing.EntitlementCalculationService.EntitlementCalculationServiceClient licensingEntitlementCalculationService,
        ApiLicensing.EntitlementQueryService.EntitlementQueryServiceClient licensingEntitlementQueryService,
        ApiLicensing.EntitlementValidationService.EntitlementValidationServiceClient licensingEntitlementValidationService,
        ApiLicensing.LicenseAssignmentValidationService.LicenseAssignmentValidationServiceClient licensingLicenseAssignmentValidationService,
        ApiLicensing.SoftwareLicenseCalculationService.SoftwareLicenseCalculationServiceClient licensingSoftwareLicenseCalculationService,
        ApiLicensing.SoftwareLicenseQueryService.SoftwareLicenseQueryServiceClient licensingSoftwareLicenseQueryService,
        ApiLicensing.SoftwareLicenseService.SoftwareLicenseServiceClient licensingSoftwareLicenseService,
        ApiLicensing.SoftwareLicenseValidationService.SoftwareLicenseValidationServiceClient licensingSoftwareLicenseValidationService,
        ILogger<LicenseAssignmentReportingEndpoint> logger,
        ApiPayments.PaymentCalculationService.PaymentCalculationServiceClient paymentsPaymentCalculationService,
        RetryPolicy policy,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiSurveys.SurveyService.SurveyServiceClient surveysSurveyService)
    {
        _bus = bus;
        _documentsDocumentValidationService = documentsDocumentValidationService;
        _invoker = invoker;
        _learningCertificationCalculationService = learningCertificationCalculationService;
        _licenseAssignmentReportingEndpointSendFetchLicenseAssignmentReport0Port = licenseAssignmentReportingEndpointSendFetchLicenseAssignmentReport0PortAdapter;
        _licensingEntitlementCalculationService = licensingEntitlementCalculationService;
        _licensingEntitlementQueryService = licensingEntitlementQueryService;
        _licensingEntitlementValidationService = licensingEntitlementValidationService;
        _licensingLicenseAssignmentValidationService = licensingLicenseAssignmentValidationService;
        _licensingSoftwareLicenseCalculationService = licensingSoftwareLicenseCalculationService;
        _licensingSoftwareLicenseQueryService = licensingSoftwareLicenseQueryService;
        _licensingSoftwareLicenseService = licensingSoftwareLicenseService;
        _licensingSoftwareLicenseValidationService = licensingSoftwareLicenseValidationService;
        _logger = logger;
        _paymentsPaymentCalculationService = paymentsPaymentCalculationService;
        _policy = policy;
        _repository = repository;
        _settings = settings;
        _surveysSurveyService = surveysSurveyService;
    }
}
