using Acme.Workforce.Platform;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Acme.Workforce.Helpdesk;

/// <summary>
/// Registers the helpdesk domain and its gRPC routes.
/// </summary>
public static class DomainModule
{
    public static IServiceCollection AddHelpdesk(this IServiceCollection services)
    {
        services.AddTransient<EscalationApprovalEndpoint>();
        services.AddSingleton<INotificationHandler, EscalationApprovalEndpointApproveEscalation0NotificationHandler>();
        services.AddTransient<EscalationCalculationEndpoint>();
        services.AddTransient<IEscalationCalculationEndpointSendQuoteEscalation3Port, EscalationCalculationEndpointSendQuoteEscalation3PortAdapter>();
        services.AddTransient<EscalationLifecycleEndpoint>();
        services.AddTransient<IEscalationLifecycleEndpointSendTerminateEscalation0Port, EscalationLifecycleEndpointSendTerminateEscalation0PortAdapter>();
        services.AddSingleton<INotificationHandler, EscalationLifecycleEndpointRenewEscalation2NotificationHandler>();
        services.AddTransient<EscalationReportingEndpoint>();
        services.AddTransient<IEscalationReportingEndpointSendShareEscalationReport2Port, EscalationReportingEndpointSendShareEscalationReport2PortAdapter>();
        services.AddTransient<EscalationValidationEndpoint>();
        services.AddTransient<KnowledgeArticleApprovalEndpoint>();
        services.AddTransient<IKnowledgeArticleApprovalEndpointSendSubmitKnowledgeArticle1Port, KnowledgeArticleApprovalEndpointSendSubmitKnowledgeArticle1PortAdapter>();
        services.AddSingleton<INotificationHandler, KnowledgeArticleApprovalEndpointCommentKnowledgeArticleApproval3NotificationHandler>();
        services.AddTransient<KnowledgeArticleCalculationEndpoint>();
        services.AddTransient<KnowledgeArticleLifecycleEndpoint>();
        services.AddTransient<KnowledgeArticleQueryEndpoint>();
        services.AddSingleton<INotificationHandler, KnowledgeArticleQueryEndpointBrowseKnowledgeArticles1NotificationHandler>();
        services.AddTransient<KnowledgeArticleValidationEndpoint>();
        services.AddTransient<IKnowledgeArticleValidationEndpointSendLintKnowledgeArticles1Port, KnowledgeArticleValidationEndpointSendLintKnowledgeArticles1PortAdapter>();
        services.AddTransient<KnowledgeArticleWorkflowEndpoint>();
        services.AddTransient<SlaPolicyApprovalEndpoint>();
        services.AddSingleton<INotificationHandler, SlaPolicyApprovalEndpointCommentSlaPolicyApproval2NotificationHandler>();
        services.AddTransient<SlaPolicyCalculationEndpoint>();
        services.AddTransient<SlaPolicyLifecycleEndpoint>();
        services.AddTransient<ISlaPolicyLifecycleEndpointSendRenewSlaPolicy0Port, SlaPolicyLifecycleEndpointSendRenewSlaPolicy0PortAdapter>();
        services.AddSingleton<INotificationHandler, SlaPolicyLifecycleEndpointTerminateSlaPolicy0NotificationHandler>();
        services.AddTransient<SlaPolicyValidationEndpoint>();
        services.AddTransient<ISlaPolicyValidationEndpointSendPreviewSlaPolicy3Port, SlaPolicyValidationEndpointSendPreviewSlaPolicy3PortAdapter>();
        services.AddTransient<SlaPolicyWorkflowEndpoint>();
        services.AddTransient<ISlaPolicyWorkflowEndpointSendResumeSlaPolicyWorkflow1Port, SlaPolicyWorkflowEndpointSendResumeSlaPolicyWorkflow1PortAdapter>();
        services.AddTransient<TicketApprovalEndpoint>();
        services.AddTransient<ITicketApprovalEndpointSendGetTicketApprovalStatus0Port, TicketApprovalEndpointSendGetTicketApprovalStatus0PortAdapter>();
        services.AddSingleton<INotificationHandler, TicketApprovalEndpointRecallTicket0NotificationHandler>();
        services.AddTransient<TicketCalculationEndpoint>();
        services.AddTransient<TicketQueryEndpoint>();
        services.AddTransient<ITicketQueryEndpointSendSummarizeTickets0Port, TicketQueryEndpointSendSummarizeTickets0PortAdapter>();
        services.AddTransient<TicketEndpoint>();
        services.AddSingleton<INotificationHandler, TicketEndpointUpdateTicket0NotificationHandler>();
        services.AddTransient<TicketValidationEndpoint>();
        services.AddTransient<ITicketValidationEndpointSendCheckTicketEligibility1Port, TicketValidationEndpointSendCheckTicketEligibility1PortAdapter>();
        services.AddTransient<TicketWorkflowEndpoint>();
        services.AddTransient<ITicketWorkflowEndpointSendAdvanceTicketWorkflow1Port, TicketWorkflowEndpointSendAdvanceTicketWorkflow1PortAdapter>();
        return services;
    }
    public static void MapHelpdesk(this IEndpointRouteBuilder routes)
    {
        routes.MapGrpcService<EscalationApprovalEndpoint>();
        routes.MapGrpcService<EscalationCalculationEndpoint>();
        routes.MapGrpcService<EscalationLifecycleEndpoint>();
        routes.MapGrpcService<EscalationReportingEndpoint>();
        routes.MapGrpcService<EscalationValidationEndpoint>();
        routes.MapGrpcService<KnowledgeArticleApprovalEndpoint>();
        routes.MapGrpcService<KnowledgeArticleCalculationEndpoint>();
        routes.MapGrpcService<KnowledgeArticleLifecycleEndpoint>();
        routes.MapGrpcService<KnowledgeArticleQueryEndpoint>();
        routes.MapGrpcService<KnowledgeArticleValidationEndpoint>();
        routes.MapGrpcService<KnowledgeArticleWorkflowEndpoint>();
        routes.MapGrpcService<SlaPolicyApprovalEndpoint>();
        routes.MapGrpcService<SlaPolicyCalculationEndpoint>();
        routes.MapGrpcService<SlaPolicyLifecycleEndpoint>();
        routes.MapGrpcService<SlaPolicyValidationEndpoint>();
        routes.MapGrpcService<SlaPolicyWorkflowEndpoint>();
        routes.MapGrpcService<TicketApprovalEndpoint>();
        routes.MapGrpcService<TicketCalculationEndpoint>();
        routes.MapGrpcService<TicketQueryEndpoint>();
        routes.MapGrpcService<TicketEndpoint>();
        routes.MapGrpcService<TicketValidationEndpoint>();
        routes.MapGrpcService<TicketWorkflowEndpoint>();
    }
}
