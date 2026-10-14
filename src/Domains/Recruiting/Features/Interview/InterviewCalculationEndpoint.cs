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
using ApiChat = Acme.Chat.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPrescriptions = Acme.Prescriptions.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSearch = Acme.Search.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Recruiting;

/// <summary>
/// Computes the figures of an interview, such as totals, taxes, and estimates.
/// </summary>
public sealed partial class InterviewCalculationEndpoint : ApiRecruiting.InterviewCalculationService.InterviewCalculationServiceBase
{
    private readonly ApiChat.ChatRoomCalculationService.ChatRoomCalculationServiceClient _chatChatRoomCalculationService;
    private readonly ApiDocuments.DocumentFolderSyncService.DocumentFolderSyncServiceClient _documentsDocumentFolderSyncService;
    private readonly ApiHelpdesk.KnowledgeArticleQueryService.KnowledgeArticleQueryServiceClient _helpdeskKnowledgeArticleQueryService;
    private readonly IInterviewCalculationEndpointSendCompareInterviewOptions1Port _interviewCalculationEndpointSendCompareInterviewOptions1Port;
    private readonly CallInvoker _invoker;
    private readonly ApiLicensing.SoftwareLicenseQueryService.SoftwareLicenseQueryServiceClient _licensingSoftwareLicenseQueryService;
    private readonly ILogger<InterviewCalculationEndpoint> _logger;
    private readonly ApiPrescriptions.PrescriptionQueryService.PrescriptionQueryServiceClient _prescriptionsPrescriptionQueryService;
    private readonly ApiRecruiting.CandidateCalculationService.CandidateCalculationServiceClient _recruitingCandidateCalculationService;
    private readonly ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient _recruitingJobOpeningCalculationService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiSearch.SearchIndexQueryService.SearchIndexQueryServiceClient _searchSearchIndexQueryService;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTravel.ItineraryValidationService.ItineraryValidationServiceClient _travelItineraryValidationService;

    public InterviewCalculationEndpoint(
        ApiChat.ChatRoomCalculationService.ChatRoomCalculationServiceClient chatChatRoomCalculationService,
        ApiDocuments.DocumentFolderSyncService.DocumentFolderSyncServiceClient documentsDocumentFolderSyncService,
        ApiHelpdesk.KnowledgeArticleQueryService.KnowledgeArticleQueryServiceClient helpdeskKnowledgeArticleQueryService,
        IInterviewCalculationEndpointSendCompareInterviewOptions1Port interviewCalculationEndpointSendCompareInterviewOptions1PortAdapter,
        CallInvoker invoker,
        ApiLicensing.SoftwareLicenseQueryService.SoftwareLicenseQueryServiceClient licensingSoftwareLicenseQueryService,
        ILogger<InterviewCalculationEndpoint> logger,
        ApiPrescriptions.PrescriptionQueryService.PrescriptionQueryServiceClient prescriptionsPrescriptionQueryService,
        ApiRecruiting.CandidateCalculationService.CandidateCalculationServiceClient recruitingCandidateCalculationService,
        ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient recruitingJobOpeningCalculationService,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiSearch.SearchIndexQueryService.SearchIndexQueryServiceClient searchSearchIndexQueryService,
        IOptions<MeshOptions> settings,
        ApiTravel.ItineraryValidationService.ItineraryValidationServiceClient travelItineraryValidationService)
    {
        _chatChatRoomCalculationService = chatChatRoomCalculationService;
        _documentsDocumentFolderSyncService = documentsDocumentFolderSyncService;
        _helpdeskKnowledgeArticleQueryService = helpdeskKnowledgeArticleQueryService;
        _interviewCalculationEndpointSendCompareInterviewOptions1Port = interviewCalculationEndpointSendCompareInterviewOptions1PortAdapter;
        _invoker = invoker;
        _licensingSoftwareLicenseQueryService = licensingSoftwareLicenseQueryService;
        _logger = logger;
        _prescriptionsPrescriptionQueryService = prescriptionsPrescriptionQueryService;
        _recruitingCandidateCalculationService = recruitingCandidateCalculationService;
        _recruitingJobOpeningCalculationService = recruitingJobOpeningCalculationService;
        _registry = registry;
        _repository = repository;
        _searchSearchIndexQueryService = searchSearchIndexQueryService;
        _settings = settings;
        _travelItineraryValidationService = travelItineraryValidationService;
    }
}
