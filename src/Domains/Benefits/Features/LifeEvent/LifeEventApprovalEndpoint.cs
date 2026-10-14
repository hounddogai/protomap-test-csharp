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
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiMessaging = Acme.Messaging.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSearch = Acme.Search.V1;
using ApiTelematics = Acme.Telematics.V1;

namespace Acme.Workforce.Benefits;

/// <summary>
/// Routes life events through approval, with escalation and delegation.
/// </summary>
public sealed partial class LifeEventApprovalEndpoint : ApiBenefits.LifeEventApprovalService.LifeEventApprovalServiceBase
{
    private readonly ApiBenefits.DependentValidationService.DependentValidationServiceClient _benefitsDependentValidationService;
    private readonly ApiBenefits.EnrollmentApprovalService.EnrollmentApprovalServiceClient _benefitsEnrollmentApprovalService;
    private readonly ApiBenefits.LifeEventValidationService.LifeEventValidationServiceClient _benefitsLifeEventValidationService;
    private readonly NotificationBus _bus;
    private readonly ApiHelpdesk.TicketApprovalService.TicketApprovalServiceClient _helpdeskTicketApprovalService;
    private readonly CallInvoker _invoker;
    private readonly ApiLearning.LearningPathApprovalService.LearningPathApprovalServiceClient _learningLearningPathApprovalService;
    private readonly ILogger<LifeEventApprovalEndpoint> _logger;
    private readonly ApiMessaging.ConversationService.ConversationServiceClient _messagingConversationService;
    private readonly ApiRecruiting.CandidateCalculationService.CandidateCalculationServiceClient _recruitingCandidateCalculationService;
    private readonly ApiRecruiting.CandidateValidationService.CandidateValidationServiceClient _recruitingCandidateValidationService;
    private readonly ApiRecruiting.InterviewValidationService.InterviewValidationServiceClient _recruitingInterviewValidationService;
    private readonly ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient _recruitingJobOpeningCalculationService;
    private readonly ApiRecruiting.OfferValidationService.OfferValidationServiceClient _recruitingOfferValidationService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiSearch.SearchIndexQueryService.SearchIndexQueryServiceClient _searchSearchIndexQueryService;
    private readonly Func<ApiRecruiting.ExplainCandidateDecisionRequest, CallOptions, AsyncUnaryCall<ApiRecruiting.ExplainCandidateDecisionResponse>> _sendGetLifeEventApprovalStatus0;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTelematics.TelemetryReadingCalculationService.TelemetryReadingCalculationServiceClient _telematicsTelemetryReadingCalculationService;

    public LifeEventApprovalEndpoint(
        ApiBenefits.DependentValidationService.DependentValidationServiceClient benefitsDependentValidationService,
        ApiBenefits.EnrollmentApprovalService.EnrollmentApprovalServiceClient benefitsEnrollmentApprovalService,
        ApiBenefits.LifeEventValidationService.LifeEventValidationServiceClient benefitsLifeEventValidationService,
        NotificationBus bus,
        ApiHelpdesk.TicketApprovalService.TicketApprovalServiceClient helpdeskTicketApprovalService,
        CallInvoker invoker,
        ApiLearning.LearningPathApprovalService.LearningPathApprovalServiceClient learningLearningPathApprovalService,
        ILogger<LifeEventApprovalEndpoint> logger,
        ApiMessaging.ConversationService.ConversationServiceClient messagingConversationService,
        ApiRecruiting.CandidateCalculationService.CandidateCalculationServiceClient recruitingCandidateCalculationService,
        ApiRecruiting.CandidateValidationService.CandidateValidationServiceClient recruitingCandidateValidationService,
        ApiRecruiting.InterviewValidationService.InterviewValidationServiceClient recruitingInterviewValidationService,
        ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient recruitingJobOpeningCalculationService,
        ApiRecruiting.OfferValidationService.OfferValidationServiceClient recruitingOfferValidationService,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiSearch.SearchIndexQueryService.SearchIndexQueryServiceClient searchSearchIndexQueryService,
        IOptions<MeshOptions> settings,
        ApiTelematics.TelemetryReadingCalculationService.TelemetryReadingCalculationServiceClient telematicsTelemetryReadingCalculationService)
    {
        _benefitsDependentValidationService = benefitsDependentValidationService;
        _benefitsEnrollmentApprovalService = benefitsEnrollmentApprovalService;
        _benefitsLifeEventValidationService = benefitsLifeEventValidationService;
        _bus = bus;
        _helpdeskTicketApprovalService = helpdeskTicketApprovalService;
        _invoker = invoker;
        _learningLearningPathApprovalService = learningLearningPathApprovalService;
        _logger = logger;
        _messagingConversationService = messagingConversationService;
        _recruitingCandidateCalculationService = recruitingCandidateCalculationService;
        _recruitingCandidateValidationService = recruitingCandidateValidationService;
        _recruitingInterviewValidationService = recruitingInterviewValidationService;
        _recruitingJobOpeningCalculationService = recruitingJobOpeningCalculationService;
        _recruitingOfferValidationService = recruitingOfferValidationService;
        _registry = registry;
        _repository = repository;
        _searchSearchIndexQueryService = searchSearchIndexQueryService;
        _settings = settings;
        _telematicsTelemetryReadingCalculationService = telematicsTelemetryReadingCalculationService;
        _sendGetLifeEventApprovalStatus0 = _recruitingCandidateValidationService.ExplainCandidateDecisionAsync;
    }
}
