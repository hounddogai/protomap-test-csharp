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

namespace Acme.Workforce.Itsm;

/// <summary>
/// Registers the itsm domain and its gRPC routes.
/// </summary>
public static class DomainModule
{
    public static IServiceCollection AddItsm(this IServiceCollection services)
    {
        services.AddTransient<ChangeRequestApprovalEndpoint>();
        services.AddSingleton<INotificationHandler, ChangeRequestApprovalEndpointDelegateChangeRequestApproval1NotificationHandler>();
        services.AddTransient<ChangeRequestCalculationEndpoint>();
        services.AddTransient<ChangeRequestLifecycleEndpoint>();
        services.AddTransient<IChangeRequestLifecycleEndpointSendGetChangeRequestLifecycle0Port, ChangeRequestLifecycleEndpointSendGetChangeRequestLifecycle0PortAdapter>();
        services.AddTransient<ChangeRequestQueryEndpoint>();
        services.AddSingleton<INotificationHandler, ChangeRequestQueryEndpointExportChangeRequests3NotificationHandler>();
        services.AddTransient<ChangeRequestReportingEndpoint>();
        services.AddTransient<IChangeRequestReportingEndpointSendScheduleChangeRequestReport1Port, ChangeRequestReportingEndpointSendScheduleChangeRequestReport1PortAdapter>();
        services.AddTransient<ChangeRequestValidationEndpoint>();
        services.AddTransient<ChangeRequestWorkflowEndpoint>();
        services.AddSingleton<INotificationHandler, ChangeRequestWorkflowEndpointPauseChangeRequestWorkflow1NotificationHandler>();
        services.AddTransient<IncidentApprovalEndpoint>();
        services.AddTransient<IIncidentApprovalEndpointSendRecallIncident0Port, IncidentApprovalEndpointSendRecallIncident0PortAdapter>();
        services.AddTransient<IncidentCalculationEndpoint>();
        services.AddSingleton<INotificationHandler, IncidentCalculationEndpointSimulateIncident2NotificationHandler>();
        services.AddTransient<IncidentLifecycleEndpoint>();
        services.AddTransient<IIncidentLifecycleEndpointSendReinstateIncident1Port, IncidentLifecycleEndpointSendReinstateIncident1PortAdapter>();
        services.AddTransient<IncidentQueryEndpoint>();
        services.AddTransient<IncidentEndpoint>();
        services.AddTransient<IIncidentEndpointSendListIncidents0Port, IncidentEndpointSendListIncidents0PortAdapter>();
        services.AddTransient<IncidentValidationEndpoint>();
        services.AddTransient<IIncidentValidationEndpointSendPreviewIncident0Port, IncidentValidationEndpointSendPreviewIncident0PortAdapter>();
        services.AddTransient<IncidentWorkflowEndpoint>();
        services.AddTransient<IIncidentWorkflowEndpointSendPauseIncidentWorkflow0Port, IncidentWorkflowEndpointSendPauseIncidentWorkflow0PortAdapter>();
        services.AddTransient<ProblemApprovalEndpoint>();
        services.AddSingleton<INotificationHandler, ProblemApprovalEndpointApproveProblem3NotificationHandler>();
        services.AddTransient<ProblemCalculationEndpoint>();
        services.AddTransient<IProblemCalculationEndpointSendEstimateProblem1Port, ProblemCalculationEndpointSendEstimateProblem1PortAdapter>();
        services.AddTransient<ProblemLifecycleEndpoint>();
        services.AddTransient<IProblemLifecycleEndpointSendExpireProblem2Port, ProblemLifecycleEndpointSendExpireProblem2PortAdapter>();
        services.AddTransient<IProblemLifecycleEndpointSendTerminateProblem1Port, ProblemLifecycleEndpointSendTerminateProblem1PortAdapter>();
        services.AddTransient<ProblemReportingEndpoint>();
        services.AddTransient<IProblemReportingEndpointSendStreamProblemMetrics0Port, ProblemReportingEndpointSendStreamProblemMetrics0PortAdapter>();
        services.AddTransient<ProblemSyncEndpoint>();
        services.AddTransient<ProblemValidationEndpoint>();
        services.AddTransient<ServiceAssetApprovalEndpoint>();
        services.AddTransient<ServiceAssetCalculationEndpoint>();
        services.AddSingleton<INotificationHandler, ServiceAssetCalculationEndpointRecalculateServiceAssets3NotificationHandler>();
        services.AddTransient<ServiceAssetLifecycleEndpoint>();
        services.AddTransient<IServiceAssetLifecycleEndpointSendTerminateServiceAsset0Port, ServiceAssetLifecycleEndpointSendTerminateServiceAsset0PortAdapter>();
        services.AddTransient<ServiceAssetReportingEndpoint>();
        services.AddTransient<IServiceAssetReportingEndpointSendShareServiceAssetReport0Port, ServiceAssetReportingEndpointSendShareServiceAssetReport0PortAdapter>();
        services.AddTransient<ServiceAssetValidationEndpoint>();
        services.AddTransient<IServiceAssetValidationEndpointSendScoreServiceAsset0Port, ServiceAssetValidationEndpointSendScoreServiceAsset0PortAdapter>();
        services.AddSingleton<INotificationHandler, ServiceAssetValidationEndpointLintServiceAssets2NotificationHandler>();
        services.AddTransient<ServiceAssetWorkflowEndpoint>();
        return services;
    }
    public static void MapItsm(this IEndpointRouteBuilder routes)
    {
        routes.MapGrpcService<ChangeRequestApprovalEndpoint>();
        routes.MapGrpcService<ChangeRequestCalculationEndpoint>();
        routes.MapGrpcService<ChangeRequestLifecycleEndpoint>();
        routes.MapGrpcService<ChangeRequestQueryEndpoint>();
        routes.MapGrpcService<ChangeRequestReportingEndpoint>();
        routes.MapGrpcService<ChangeRequestValidationEndpoint>();
        routes.MapGrpcService<ChangeRequestWorkflowEndpoint>();
        routes.MapGrpcService<IncidentApprovalEndpoint>();
        routes.MapGrpcService<IncidentCalculationEndpoint>();
        routes.MapGrpcService<IncidentLifecycleEndpoint>();
        routes.MapGrpcService<IncidentQueryEndpoint>();
        routes.MapGrpcService<IncidentEndpoint>();
        routes.MapGrpcService<IncidentValidationEndpoint>();
        routes.MapGrpcService<IncidentWorkflowEndpoint>();
        routes.MapGrpcService<ProblemApprovalEndpoint>();
        routes.MapGrpcService<ProblemCalculationEndpoint>();
        routes.MapGrpcService<ProblemLifecycleEndpoint>();
        routes.MapGrpcService<ProblemReportingEndpoint>();
        routes.MapGrpcService<ProblemSyncEndpoint>();
        routes.MapGrpcService<ProblemValidationEndpoint>();
        routes.MapGrpcService<ServiceAssetApprovalEndpoint>();
        routes.MapGrpcService<ServiceAssetCalculationEndpoint>();
        routes.MapGrpcService<ServiceAssetLifecycleEndpoint>();
        routes.MapGrpcService<ServiceAssetReportingEndpoint>();
        routes.MapGrpcService<ServiceAssetValidationEndpoint>();
        routes.MapGrpcService<ServiceAssetWorkflowEndpoint>();
    }
}
