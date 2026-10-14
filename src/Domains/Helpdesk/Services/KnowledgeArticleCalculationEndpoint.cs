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
using ApiDeliveries = Acme.Deliveries.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiMessaging = Acme.Messaging.V1;
using ApiNotifications = Acme.Notifications.V1;

namespace Acme.Workforce.Helpdesk;

/// <summary>
/// Computes the figures of a knowledge article, such as totals, taxes, and estimates.
/// </summary>
public sealed partial class KnowledgeArticleCalculationEndpoint : ApiHelpdesk.KnowledgeArticleCalculationService.KnowledgeArticleCalculationServiceBase
{
    private readonly ApiChat.ChatRoomService.ChatRoomServiceClient _chatChatRoomService;
    private readonly ApiDeliveries.DeliveryQueryService.DeliveryQueryServiceClient _deliveriesDeliveryQueryService;
    private readonly ApiDocuments.DocumentTemplateCalculationService.DocumentTemplateCalculationServiceClient _documentsDocumentTemplateCalculationService;
    private readonly ApiHelpdesk.TicketCalculationService.TicketCalculationServiceClient _helpdeskTicketCalculationService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.IncidentCalculationService.IncidentCalculationServiceClient _itsmIncidentCalculationService;
    private readonly ApiItsm.IncidentQueryService.IncidentQueryServiceClient _itsmIncidentQueryService;
    private readonly ApiItsm.ProblemCalculationService.ProblemCalculationServiceClient _itsmProblemCalculationService;
    private readonly ILogger<KnowledgeArticleCalculationEndpoint> _logger;
    private readonly ApiMessaging.ConversationCalculationService.ConversationCalculationServiceClient _messagingConversationCalculationService;
    private readonly ApiNotifications.NotificationValidationService.NotificationValidationServiceClient _notificationsNotificationValidationService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiHelpdesk.QuoteTicketRequest, CallOptions, AsyncUnaryCall<ApiHelpdesk.QuoteTicketResponse>> _sendQuoteKnowledgeArticle0;
    private readonly IOptions<MeshOptions> _settings;

    public KnowledgeArticleCalculationEndpoint(
        ApiChat.ChatRoomService.ChatRoomServiceClient chatChatRoomService,
        ApiDeliveries.DeliveryQueryService.DeliveryQueryServiceClient deliveriesDeliveryQueryService,
        ApiDocuments.DocumentTemplateCalculationService.DocumentTemplateCalculationServiceClient documentsDocumentTemplateCalculationService,
        ApiHelpdesk.TicketCalculationService.TicketCalculationServiceClient helpdeskTicketCalculationService,
        CallInvoker invoker,
        ApiItsm.IncidentCalculationService.IncidentCalculationServiceClient itsmIncidentCalculationService,
        ApiItsm.IncidentQueryService.IncidentQueryServiceClient itsmIncidentQueryService,
        ApiItsm.ProblemCalculationService.ProblemCalculationServiceClient itsmProblemCalculationService,
        ILogger<KnowledgeArticleCalculationEndpoint> logger,
        ApiMessaging.ConversationCalculationService.ConversationCalculationServiceClient messagingConversationCalculationService,
        ApiNotifications.NotificationValidationService.NotificationValidationServiceClient notificationsNotificationValidationService,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings)
    {
        _chatChatRoomService = chatChatRoomService;
        _deliveriesDeliveryQueryService = deliveriesDeliveryQueryService;
        _documentsDocumentTemplateCalculationService = documentsDocumentTemplateCalculationService;
        _helpdeskTicketCalculationService = helpdeskTicketCalculationService;
        _invoker = invoker;
        _itsmIncidentCalculationService = itsmIncidentCalculationService;
        _itsmIncidentQueryService = itsmIncidentQueryService;
        _itsmProblemCalculationService = itsmProblemCalculationService;
        _logger = logger;
        _messagingConversationCalculationService = messagingConversationCalculationService;
        _notificationsNotificationValidationService = notificationsNotificationValidationService;
        _registry = registry;
        _repository = repository;
        _settings = settings;
        _sendQuoteKnowledgeArticle0 = _helpdeskTicketCalculationService.QuoteTicketAsync;
    }
}
