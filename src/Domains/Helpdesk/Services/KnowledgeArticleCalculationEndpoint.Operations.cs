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

public sealed partial class KnowledgeArticleCalculationEndpoint
{
    /// <summary>
    /// Calculates a knowledge article.
    /// </summary>
    public override async Task<ApiHelpdesk.CalculateKnowledgeArticleResponse> CalculateKnowledgeArticle(ApiHelpdesk.CalculateKnowledgeArticleRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CalculateKnowledgeArticle for {Tenant}", requestContext.TenantId);
        if (request.KnowledgeArticle is { } record)
        {
            var snapshot = KnowledgeArticleMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiNotifications.LintNotificationsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiNotifications.LintNotificationsRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.KnowledgeArticle?.Counterparty?.Email ?? ""),
                },
            },
            Notification = new ApiNotifications.Notification
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.KnowledgeArticle?.Audit?.CreatedBy ?? ""),
                },
                Customer = new ApiNotifications.NotificationTemplateRef
                {
                    DisplayName = (request.KnowledgeArticle?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _notificationsNotificationValidationService.LintNotificationsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiChat.CloneChatRoomRequest
        {
            ChatRoom = new ApiChat.ChatRoom
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.KnowledgeArticle?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedAt = (reply0.Notification?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.KnowledgeArticle?.Counterparty?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var packed1 = Any.Pack(input1);
        input1 = packed1.Unpack<ApiChat.CloneChatRoomRequest>();
        using var rpc1 = _chatChatRoomService.CloneChatRoomAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiHelpdesk.CalculateKnowledgeArticleResponse
        {
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.ChatRoom?.Audit?.Etag ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (reply0.Notification?.Assignee?.DisplayName ?? ""),
                },
                KnowledgeArticleId = (request.KnowledgeArticleId ?? ""),
                Notes = (request.KnowledgeArticle?.Notes ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "knowledge_article",
            "audit", "CalculateKnowledgeArticle", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Compares a knowledge article.
    /// </summary>
    public override async Task<ApiHelpdesk.CompareKnowledgeArticleOptionsResponse> CompareKnowledgeArticleOptions(ApiHelpdesk.CompareKnowledgeArticleOptionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CompareKnowledgeArticleOptions for {Tenant}", requestContext.TenantId);
        if (request.KnowledgeArticle is { } record)
        {
            var snapshot = KnowledgeArticleMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.ResolveIncidentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiItsm.ResolveIncidentsRequest>(json0);
        using var rpc0 = _itsmIncidentQueryService.ResolveIncidentsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.CompareIncidentOptionsRequest();
        input1.Context = (request.Context ?? new ApiCommon.RequestContext());
        input1.Incident = new ApiItsm.Incident
        {
            Audit = new ApiCommon.AuditInfo
            {
                UpdatedBy = (request.KnowledgeArticle?.Audit?.UpdatedBy ?? ""),
            },
        };
        using var rpc1 = _itsmIncidentCalculationService.CompareIncidentOptionsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiHelpdesk.CalculateTicketRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.CalculateTicketRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (request.KnowledgeArticle?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Locality = (request.KnowledgeArticle?.BillingAddress?.Locality ?? ""),
                },
            },
            Ticket = new ApiHelpdesk.Ticket
            {
                Requester = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (reply1.Incident?.Requester?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc2 = _helpdeskTicketCalculationService.CalculateTicketAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiHelpdesk.CompareKnowledgeArticleOptionsResponse
        {
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.Incident?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                BillingAddress = (request.KnowledgeArticle?.BillingAddress ?? new ApiCommon.PostalAddress()),
                KnowledgeArticleId = (request.KnowledgeArticleId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply2.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "knowledge_article",
            "audit", "CompareKnowledgeArticleOptions", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Estimates a knowledge article.
    /// </summary>
    public override async Task<ApiHelpdesk.EstimateKnowledgeArticleResponse> EstimateKnowledgeArticle(ApiHelpdesk.EstimateKnowledgeArticleRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EstimateKnowledgeArticle for {Tenant}", requestContext.TenantId);
        if (request.KnowledgeArticle is { } record)
        {
            var snapshot = KnowledgeArticleMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.CalculateProblemRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Problem = new ApiItsm.Problem
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.KnowledgeArticle?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _itsmProblemCalculationService.CalculateProblemAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiHelpdesk.EstimateKnowledgeArticleResponse
        {
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.Problem?.Audit?.UpdatedBy ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (reply0.Problem?.Assignee?.Email ?? ""),
                },
                KnowledgeArticleId = (request.KnowledgeArticleId ?? ""),
                Metadata = { (request.KnowledgeArticle?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "knowledge_article",
            "audit", "EstimateKnowledgeArticle", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a knowledge article.
    /// </summary>
    public override async Task<ApiHelpdesk.ExplainKnowledgeArticleCalculationResponse> ExplainKnowledgeArticleCalculation(ApiHelpdesk.ExplainKnowledgeArticleCalculationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExplainKnowledgeArticleCalculation for {Tenant}", requestContext.TenantId);
        if (request.KnowledgeArticle is { } record)
        {
            var snapshot = KnowledgeArticleMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDeliveries.ExportDeliveriesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDeliveries.ExportDeliveriesRequest.Types.Filter
            {
                CreatedAfter = (request.KnowledgeArticle?.Audit?.UpdatedAt ?? new Timestamp()),
                Region = (request.KnowledgeArticle?.BillingAddress?.Region ?? ""),
            },
        };
        using var stream0 = _deliveriesDeliveryQueryService.ExportDeliveries(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportDeliveries frame {Type}", change0.GetType().Name);
        }
        var batch1 = (request.KnowledgeArticle?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiHelpdesk.KnowledgeArticleLine>()).Select(async item =>
        {
            var input1 = new ApiDocuments.CompareDocumentTemplateOptionsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                DocumentTemplate = new ApiDocuments.DocumentTemplate
                {
                    Lines = { new ApiDocuments.DocumentTemplateLine
                    {
                        Amount = (item.Tax?.Amount ?? new ApiCommon.Money()),
                        Tax = new ApiDocuments.DocumentTemplateLineTax
                        {
                            Amount = (item.Amount ?? new ApiCommon.Money()),
                        },
                    } },
                },
            };
            using var rpc1 = _documentsDocumentTemplateCalculationService.CompareDocumentTemplateOptionsAsync(input1, cancellationToken: token);
            var reply1 = await rpc1.ResponseAsync;
            return reply1;
        }).ToArray();
        var replies1 = await Task.WhenAll(batch1);
        var reply1 = replies1.LastOrDefault() ?? new ApiDocuments.CompareDocumentTemplateOptionsResponse();
        var response = new ApiHelpdesk.ExplainKnowledgeArticleCalculationResponse
        {
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                KnowledgeArticleId = (request.KnowledgeArticleId ?? ""),
                Status = (request.KnowledgeArticle?.Status ?? (ApiHelpdesk.KnowledgeArticleStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "knowledge_article",
            "audit", "ExplainKnowledgeArticleCalculation", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Quotes a knowledge article.
    /// </summary>
    public override async Task<ApiHelpdesk.QuoteKnowledgeArticleResponse> QuoteKnowledgeArticle(ApiHelpdesk.QuoteKnowledgeArticleRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling QuoteKnowledgeArticle for {Tenant}", requestContext.TenantId);
        if (request.KnowledgeArticle is { } record)
        {
            var snapshot = KnowledgeArticleMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.QuoteTicketRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.QuoteTicketRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Recipient = (request.KnowledgeArticle?.BillingAddress?.Recipient ?? ""),
                },
            },
        };
        using var rpc0 = _sendQuoteKnowledgeArticle0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.SimulateIncidentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.SimulateIncidentRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Recipient = (request.Details?.Destination?.Recipient ?? ""),
                },
            },
            Incident = new ApiItsm.Incident
            {
                Requester = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (reply0.Ticket?.Requester?.PreferredChannel ?? ""),
                },
            },
        };
        var named1 = _registry.Get<ApiItsm.IncidentCalculationService.IncidentCalculationServiceClient>("acme.itsm.v1.IncidentCalculationService");
        using var rpc1 = named1.SimulateIncidentAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiHelpdesk.QuoteKnowledgeArticleResponse
        {
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    EmployeeId = (reply1.Incident?.Customer?.EmployeeId ?? ""),
                },
                IssuedAt = (request.KnowledgeArticle?.IssuedAt ?? new Timestamp()),
                KnowledgeArticleId = (request.KnowledgeArticleId ?? ""),
                Total = (reply1.Breakdown?.Total ?? new ApiCommon.Money()),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "knowledge_article",
            "audit", "QuoteKnowledgeArticle", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalculates a knowledge article.
    /// </summary>
    public override async Task<ApiHelpdesk.RecalculateKnowledgeArticlesResponse> RecalculateKnowledgeArticles(ApiHelpdesk.RecalculateKnowledgeArticlesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecalculateKnowledgeArticles for {Tenant}", requestContext.TenantId);
        if (request.KnowledgeArticle is { } record)
        {
            var snapshot = KnowledgeArticleMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.QuoteTicketRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.QuoteTicketRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Locality = (request.KnowledgeArticle?.BillingAddress?.Locality ?? ""),
                    PostalCode = (request.Details?.Destination?.PostalCode ?? ""),
                },
            },
            Ticket = new ApiHelpdesk.Ticket
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (request.KnowledgeArticle?.Counterparty?.DisplayName ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.KnowledgeArticle?.Audit?.CreatedBy ?? ""),
                },
            },
        };
        var descriptor0 = new Method<ApiHelpdesk.QuoteTicketRequest, ApiHelpdesk.QuoteTicketResponse>(
            MethodType.Unary, "acme.helpdesk.v1" + ".TicketCalculationService", "QuoteTicket",
            Wire.Marshaller<ApiHelpdesk.QuoteTicketRequest>(), Wire.Marshaller<ApiHelpdesk.QuoteTicketResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiHelpdesk.RecalculateKnowledgeArticlesResponse
        {
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                IssuedAt = (request.KnowledgeArticle?.IssuedAt ?? new Timestamp()),
                KnowledgeArticleId = (request.KnowledgeArticleId ?? ""),
                Total = (reply0.Breakdown?.Total ?? new ApiCommon.Money()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "knowledge_article",
            "audit", "RecalculateKnowledgeArticles", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Simulates a knowledge article.
    /// </summary>
    public override async Task<ApiHelpdesk.SimulateKnowledgeArticleResponse> SimulateKnowledgeArticle(ApiHelpdesk.SimulateKnowledgeArticleRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SimulateKnowledgeArticle for {Tenant}", requestContext.TenantId);
        if (request.KnowledgeArticle is { } record)
        {
            var snapshot = KnowledgeArticleMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.CompareTicketOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.CompareTicketOptionsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Details?.Destination?.PostalCode ?? ""),
                },
            },
        };
        var command0 = new DispatchCommand<ApiHelpdesk.CompareTicketOptionsRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _helpdeskTicketCalculationService.CompareTicketOptionsAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiMessaging.CompareConversationOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Conversation = new ApiMessaging.Conversation
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Ticket?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedAt = (request.KnowledgeArticle?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Channel = new ApiMessaging.ConversationChannelOrigin
                {
                    Address = (reply0.Ticket ?? new ApiHelpdesk.Ticket()).OriginCase switch
                    {
                        ApiHelpdesk.Ticket.OriginOneofCase.Channel => (reply0.Ticket?.Channel?.Address ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _messagingConversationCalculationService.CompareConversationOptionsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiHelpdesk.CalculateTicketRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.CalculateTicketRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Recipient = (request.KnowledgeArticle?.BillingAddress?.Recipient ?? ""),
                },
            },
            Ticket = new ApiHelpdesk.Ticket
            {
                Channel = new ApiHelpdesk.TicketChannelOrigin
                {
                    Address = (reply0.Ticket ?? new ApiHelpdesk.Ticket()).OriginCase switch
                    {
                        ApiHelpdesk.Ticket.OriginOneofCase.Channel => (reply0.Ticket?.Channel?.Address ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc2 = _helpdeskTicketCalculationService.CalculateTicketAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiHelpdesk.SimulateKnowledgeArticleResponse
        {
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.Ticket?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                KnowledgeArticleId = (reply2.Ticket?.Customer?.KnowledgeArticleId ?? ""),
                Metadata = { (request.KnowledgeArticle?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Number = (reply0.Ticket?.Customer?.Number ?? ""),
                Total = (request.KnowledgeArticle?.Total ?? new ApiCommon.Money()),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "knowledge_article",
            "audit", "SimulateKnowledgeArticle", (request.Reason ?? ""), token);
        return response;
    }
}
