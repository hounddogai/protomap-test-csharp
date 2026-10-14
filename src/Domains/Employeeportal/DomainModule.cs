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

namespace Acme.Workforce.Employeeportal;

/// <summary>
/// Registers the employeeportal domain and its gRPC routes.
/// </summary>
public static class DomainModule
{
    public static IServiceCollection AddEmployeeportal(this IServiceCollection services)
    {
        services.AddTransient<AnnouncementIntegrationEndpoint>();
        services.AddTransient<IAnnouncementIntegrationEndpointSendDisconnectAnnouncementSource3Port, AnnouncementIntegrationEndpointSendDisconnectAnnouncementSource3PortAdapter>();
        services.AddSingleton<INotificationHandler, AnnouncementIntegrationEndpointListAnnouncementSources0NotificationHandler>();
        services.AddTransient<AnnouncementNotificationEndpoint>();
        services.AddTransient<IAnnouncementNotificationEndpointSendSendAnnouncementTestNotice0Port, AnnouncementNotificationEndpointSendSendAnnouncementTestNotice0PortAdapter>();
        services.AddTransient<AnnouncementQueryEndpoint>();
        services.AddTransient<AnnouncementWorkflowEndpoint>();
        services.AddTransient<IAnnouncementWorkflowEndpointSendListAnnouncementWorkflows0Port, AnnouncementWorkflowEndpointSendListAnnouncementWorkflows0PortAdapter>();
        services.AddSingleton<INotificationHandler, AnnouncementWorkflowEndpointCancelAnnouncementWorkflow1NotificationHandler>();
        services.AddTransient<PortalHomeGatewayEndpoint>();
        services.AddTransient<IPortalHomeGatewayEndpointSendComposePortalHomePage1Port, PortalHomeGatewayEndpointSendComposePortalHomePage1PortAdapter>();
        services.AddTransient<PortalHomeIntegrationEndpoint>();
        services.AddTransient<IPortalHomeIntegrationEndpointSendReplayPortalHomeEvents0Port, PortalHomeIntegrationEndpointSendReplayPortalHomeEvents0PortAdapter>();
        services.AddTransient<PortalHomeNotificationEndpoint>();
        services.AddTransient<IPortalHomeNotificationEndpointSendSubscribePortalHomeUpdates1Port, PortalHomeNotificationEndpointSendSubscribePortalHomeUpdates1PortAdapter>();
        services.AddTransient<PortalHomeQueryEndpoint>();
        services.AddTransient<PortalHomeEndpoint>();
        services.AddSingleton<INotificationHandler, PortalHomeEndpointClonePortalHome0NotificationHandler>();
        services.AddTransient<SelfServiceRequestIntegrationEndpoint>();
        services.AddTransient<ISelfServiceRequestIntegrationEndpointSendListSelfServiceRequestSources0Port, SelfServiceRequestIntegrationEndpointSendListSelfServiceRequestSources0PortAdapter>();
        services.AddTransient<SelfServiceRequestNotificationEndpoint>();
        services.AddSingleton<INotificationHandler, SelfServiceRequestNotificationEndpointNotifySelfServiceRequestWatchers0NotificationHandler>();
        services.AddTransient<SelfServiceRequestQueryEndpoint>();
        services.AddTransient<SelfServiceRequestEndpoint>();
        services.AddTransient<ISelfServiceRequestEndpointSendListSelfServiceRequests0Port, SelfServiceRequestEndpointSendListSelfServiceRequests0PortAdapter>();
        return services;
    }
    public static void MapEmployeeportal(this IEndpointRouteBuilder routes)
    {
        routes.MapGrpcService<AnnouncementIntegrationEndpoint>();
        routes.MapGrpcService<AnnouncementNotificationEndpoint>();
        routes.MapGrpcService<AnnouncementQueryEndpoint>();
        routes.MapGrpcService<AnnouncementWorkflowEndpoint>();
        routes.MapGrpcService<PortalHomeGatewayEndpoint>();
        routes.MapGrpcService<PortalHomeIntegrationEndpoint>();
        routes.MapGrpcService<PortalHomeNotificationEndpoint>();
        routes.MapGrpcService<PortalHomeQueryEndpoint>();
        routes.MapGrpcService<PortalHomeEndpoint>();
        routes.MapGrpcService<SelfServiceRequestIntegrationEndpoint>();
        routes.MapGrpcService<SelfServiceRequestNotificationEndpoint>();
        routes.MapGrpcService<SelfServiceRequestQueryEndpoint>();
        routes.MapGrpcService<SelfServiceRequestEndpoint>();
    }
}
