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
using ApiBenefits = Acme.Benefits.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSearch = Acme.Search.V1;

namespace Acme.Workforce.Benefits;

/// <summary>
/// Routes enrollments through approval, with escalation and delegation.
/// </summary>
public sealed partial class EnrollmentApprovalEndpoint : ApiBenefits.EnrollmentApprovalService.EnrollmentApprovalServiceBase
{
    private readonly ApiBenefits.BenefitPlanApprovalService.BenefitPlanApprovalServiceClient _benefitsBenefitPlanApprovalService;
    private readonly ApiBenefits.BenefitPlanQueryService.BenefitPlanQueryServiceClient _benefitsBenefitPlanQueryService;
    private readonly ApiBenefits.BenefitPlanService.BenefitPlanServiceClient _benefitsBenefitPlanService;
    private readonly ApiBenefits.LifeEventCalculationService.LifeEventCalculationServiceClient _benefitsLifeEventCalculationService;
    private readonly ApiBenefits.LifeEventValidationService.LifeEventValidationServiceClient _benefitsLifeEventValidationService;
    private readonly NotificationBus _bus;
    private readonly ApiDocuments.SignatureNotificationService.SignatureNotificationServiceClient _documentsSignatureNotificationService;
    private readonly IEnrollmentApprovalEndpointSendListPendingEnrollments0Port _enrollmentApprovalEndpointSendListPendingEnrollments0Port;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.IncidentWorkflowService.IncidentWorkflowServiceClient _itsmIncidentWorkflowService;
    private readonly ILogger<EnrollmentApprovalEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly ApiRecruiting.JobOpeningQueryService.JobOpeningQueryServiceClient _recruitingJobOpeningQueryService;
    private readonly ApiRecruiting.JobOpeningService.JobOpeningServiceClient _recruitingJobOpeningService;
    private readonly ApiRecruiting.OfferValidationService.OfferValidationServiceClient _recruitingOfferValidationService;
    private readonly IRecordRepository _repository;
    private readonly ApiSearch.SearchIndexService.SearchIndexServiceClient _searchSearchIndexService;
    private readonly IOptions<MeshOptions> _settings;

    public EnrollmentApprovalEndpoint(
        ApiBenefits.BenefitPlanApprovalService.BenefitPlanApprovalServiceClient benefitsBenefitPlanApprovalService,
        ApiBenefits.BenefitPlanQueryService.BenefitPlanQueryServiceClient benefitsBenefitPlanQueryService,
        ApiBenefits.BenefitPlanService.BenefitPlanServiceClient benefitsBenefitPlanService,
        ApiBenefits.LifeEventCalculationService.LifeEventCalculationServiceClient benefitsLifeEventCalculationService,
        ApiBenefits.LifeEventValidationService.LifeEventValidationServiceClient benefitsLifeEventValidationService,
        NotificationBus bus,
        ApiDocuments.SignatureNotificationService.SignatureNotificationServiceClient documentsSignatureNotificationService,
        IEnrollmentApprovalEndpointSendListPendingEnrollments0Port enrollmentApprovalEndpointSendListPendingEnrollments0PortAdapter,
        CallInvoker invoker,
        ApiItsm.IncidentWorkflowService.IncidentWorkflowServiceClient itsmIncidentWorkflowService,
        ILogger<EnrollmentApprovalEndpoint> logger,
        RetryPolicy policy,
        ApiRecruiting.JobOpeningQueryService.JobOpeningQueryServiceClient recruitingJobOpeningQueryService,
        ApiRecruiting.JobOpeningService.JobOpeningServiceClient recruitingJobOpeningService,
        ApiRecruiting.OfferValidationService.OfferValidationServiceClient recruitingOfferValidationService,
        IRecordRepository repository,
        ApiSearch.SearchIndexService.SearchIndexServiceClient searchSearchIndexService,
        IOptions<MeshOptions> settings)
    {
        _benefitsBenefitPlanApprovalService = benefitsBenefitPlanApprovalService;
        _benefitsBenefitPlanQueryService = benefitsBenefitPlanQueryService;
        _benefitsBenefitPlanService = benefitsBenefitPlanService;
        _benefitsLifeEventCalculationService = benefitsLifeEventCalculationService;
        _benefitsLifeEventValidationService = benefitsLifeEventValidationService;
        _bus = bus;
        _documentsSignatureNotificationService = documentsSignatureNotificationService;
        _enrollmentApprovalEndpointSendListPendingEnrollments0Port = enrollmentApprovalEndpointSendListPendingEnrollments0PortAdapter;
        _invoker = invoker;
        _itsmIncidentWorkflowService = itsmIncidentWorkflowService;
        _logger = logger;
        _policy = policy;
        _recruitingJobOpeningQueryService = recruitingJobOpeningQueryService;
        _recruitingJobOpeningService = recruitingJobOpeningService;
        _recruitingOfferValidationService = recruitingOfferValidationService;
        _repository = repository;
        _searchSearchIndexService = searchSearchIndexService;
        _settings = settings;
    }
}
