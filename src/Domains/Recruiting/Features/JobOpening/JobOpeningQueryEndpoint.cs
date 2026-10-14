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
using ApiEmployees = Acme.Employees.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Recruiting;

/// <summary>
/// Answers read-only questions about job openings, for screens and reports.
/// </summary>
public sealed partial class JobOpeningQueryEndpoint : ApiRecruiting.JobOpeningQueryService.JobOpeningQueryServiceBase
{
    private readonly ApiCorporateevents.CorporateEventCalculationService.CorporateEventCalculationServiceClient _corporateeventsCorporateEventCalculationService;
    private readonly ApiDocuments.DocumentValidationService.DocumentValidationServiceClient _documentsDocumentValidationService;
    private readonly CallInvoker _invoker;
    private readonly ApiLicensing.EntitlementValidationService.EntitlementValidationServiceClient _licensingEntitlementValidationService;
    private readonly ILogger<JobOpeningQueryEndpoint> _logger;
    private readonly ApiRecruiting.InterviewValidationService.InterviewValidationServiceClient _recruitingInterviewValidationService;
    private readonly ApiRecruiting.JobOpeningValidationService.JobOpeningValidationServiceClient _recruitingJobOpeningValidationService;
    private readonly ApiRecruiting.OfferCalculationService.OfferCalculationServiceClient _recruitingOfferCalculationService;
    private readonly ApiRecruiting.OfferValidationService.OfferValidationServiceClient _recruitingOfferValidationService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiRecruiting.CheckOfferEligibilityRequest, CallOptions, AsyncUnaryCall<ApiRecruiting.CheckOfferEligibilityResponse>> _sendExportJobOpenings0;
    private readonly IOptions<MeshOptions> _settings;

    public JobOpeningQueryEndpoint(
        ApiCorporateevents.CorporateEventCalculationService.CorporateEventCalculationServiceClient corporateeventsCorporateEventCalculationService,
        ApiDocuments.DocumentValidationService.DocumentValidationServiceClient documentsDocumentValidationService,
        CallInvoker invoker,
        ApiLicensing.EntitlementValidationService.EntitlementValidationServiceClient licensingEntitlementValidationService,
        ILogger<JobOpeningQueryEndpoint> logger,
        ApiRecruiting.InterviewValidationService.InterviewValidationServiceClient recruitingInterviewValidationService,
        ApiRecruiting.JobOpeningValidationService.JobOpeningValidationServiceClient recruitingJobOpeningValidationService,
        ApiRecruiting.OfferCalculationService.OfferCalculationServiceClient recruitingOfferCalculationService,
        ApiRecruiting.OfferValidationService.OfferValidationServiceClient recruitingOfferValidationService,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings)
    {
        _corporateeventsCorporateEventCalculationService = corporateeventsCorporateEventCalculationService;
        _documentsDocumentValidationService = documentsDocumentValidationService;
        _invoker = invoker;
        _licensingEntitlementValidationService = licensingEntitlementValidationService;
        _logger = logger;
        _recruitingInterviewValidationService = recruitingInterviewValidationService;
        _recruitingJobOpeningValidationService = recruitingJobOpeningValidationService;
        _recruitingOfferCalculationService = recruitingOfferCalculationService;
        _recruitingOfferValidationService = recruitingOfferValidationService;
        _registry = registry;
        _repository = repository;
        _settings = settings;
        _sendExportJobOpenings0 = _recruitingOfferValidationService.CheckOfferEligibilityAsync;
    }
}
