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

namespace Acme.Workforce.Workplaces;

/// <summary>
/// Registers the workplaces domain and its gRPC routes.
/// </summary>
public static class DomainModule
{
    public static IServiceCollection AddWorkplaces(this IServiceCollection services)
    {
        services.AddTransient<BuildingApprovalEndpoint>();
        services.AddSingleton<INotificationHandler, BuildingApprovalEndpointApproveBuilding1NotificationHandler>();
        services.AddTransient<BuildingCalculationEndpoint>();
        services.AddTransient<IBuildingCalculationEndpointSendSimulateBuilding1Port, BuildingCalculationEndpointSendSimulateBuilding1PortAdapter>();
        services.AddTransient<BuildingLifecycleEndpoint>();
        services.AddTransient<IBuildingLifecycleEndpointSendSuspendBuilding1Port, BuildingLifecycleEndpointSendSuspendBuilding1PortAdapter>();
        services.AddTransient<BuildingQueryEndpoint>();
        services.AddTransient<BuildingEndpoint>();
        services.AddTransient<IBuildingEndpointSendWatchBuildings0Port, BuildingEndpointSendWatchBuildings0PortAdapter>();
        services.AddTransient<BuildingValidationEndpoint>();
        services.AddTransient<IBuildingValidationEndpointSendValidateBuilding0Port, BuildingValidationEndpointSendValidateBuilding0PortAdapter>();
        services.AddSingleton<INotificationHandler, BuildingValidationEndpointScoreBuilding0NotificationHandler>();
        services.AddTransient<BuildingWorkflowEndpoint>();
        services.AddTransient<DeskApprovalEndpoint>();
        services.AddTransient<DeskCalculationEndpoint>();
        services.AddSingleton<INotificationHandler, DeskCalculationEndpointExplainDeskCalculation2NotificationHandler>();
        services.AddTransient<DeskLifecycleEndpoint>();
        services.AddTransient<DeskQueryEndpoint>();
        services.AddTransient<DeskValidationEndpoint>();
        services.AddTransient<IDeskValidationEndpointSendCheckDeskEligibility3Port, DeskValidationEndpointSendCheckDeskEligibility3PortAdapter>();
        services.AddSingleton<INotificationHandler, DeskValidationEndpointLintDesks0NotificationHandler>();
        services.AddTransient<DeskWorkflowEndpoint>();
        services.AddTransient<IDeskWorkflowEndpointSendAdvanceDeskWorkflow0Port, DeskWorkflowEndpointSendAdvanceDeskWorkflow0PortAdapter>();
        services.AddTransient<IDeskWorkflowEndpointSendSignalDeskWorkflow2Port, DeskWorkflowEndpointSendSignalDeskWorkflow2PortAdapter>();
        services.AddTransient<RoomBookingApprovalEndpoint>();
        services.AddTransient<IRoomBookingApprovalEndpointSendRecallRoomBooking1Port, RoomBookingApprovalEndpointSendRecallRoomBooking1PortAdapter>();
        services.AddSingleton<INotificationHandler, RoomBookingApprovalEndpointRecallRoomBooking0NotificationHandler>();
        services.AddTransient<RoomBookingCalculationEndpoint>();
        services.AddTransient<RoomBookingLifecycleEndpoint>();
        services.AddTransient<IRoomBookingLifecycleEndpointSendTerminateRoomBooking2Port, RoomBookingLifecycleEndpointSendTerminateRoomBooking2PortAdapter>();
        services.AddTransient<RoomBookingReportingEndpoint>();
        services.AddSingleton<INotificationHandler, RoomBookingReportingEndpointStreamRoomBookingMetrics0NotificationHandler>();
        services.AddTransient<RoomBookingValidationEndpoint>();
        services.AddTransient<IRoomBookingValidationEndpointSendCheckRoomBookingEligibility2Port, RoomBookingValidationEndpointSendCheckRoomBookingEligibility2PortAdapter>();
        services.AddTransient<IRoomBookingValidationEndpointSendValidateRoomBooking2Port, RoomBookingValidationEndpointSendValidateRoomBooking2PortAdapter>();
        services.AddTransient<RoomBookingWorkflowEndpoint>();
        services.AddTransient<VisitorPassApprovalEndpoint>();
        services.AddTransient<IVisitorPassApprovalEndpointSendApproveVisitorPass0Port, VisitorPassApprovalEndpointSendApproveVisitorPass0PortAdapter>();
        services.AddSingleton<INotificationHandler, VisitorPassApprovalEndpointDelegateVisitorPassApproval1NotificationHandler>();
        services.AddTransient<VisitorPassCalculationEndpoint>();
        services.AddTransient<IVisitorPassCalculationEndpointSendCalculateVisitorPass2Port, VisitorPassCalculationEndpointSendCalculateVisitorPass2PortAdapter>();
        services.AddTransient<IVisitorPassCalculationEndpointSendRecalculateVisitorPasses2Port, VisitorPassCalculationEndpointSendRecalculateVisitorPasses2PortAdapter>();
        services.AddTransient<VisitorPassLifecycleEndpoint>();
        services.AddSingleton<INotificationHandler, VisitorPassLifecycleEndpointSuspendVisitorPass1NotificationHandler>();
        services.AddTransient<VisitorPassReportingEndpoint>();
        services.AddTransient<VisitorPassSyncEndpoint>();
        services.AddTransient<VisitorPassValidationEndpoint>();
        return services;
    }
    public static void MapWorkplaces(this IEndpointRouteBuilder routes)
    {
        routes.MapGrpcService<BuildingApprovalEndpoint>();
        routes.MapGrpcService<BuildingCalculationEndpoint>();
        routes.MapGrpcService<BuildingLifecycleEndpoint>();
        routes.MapGrpcService<BuildingQueryEndpoint>();
        routes.MapGrpcService<BuildingEndpoint>();
        routes.MapGrpcService<BuildingValidationEndpoint>();
        routes.MapGrpcService<BuildingWorkflowEndpoint>();
        routes.MapGrpcService<DeskApprovalEndpoint>();
        routes.MapGrpcService<DeskCalculationEndpoint>();
        routes.MapGrpcService<DeskLifecycleEndpoint>();
        routes.MapGrpcService<DeskQueryEndpoint>();
        routes.MapGrpcService<DeskValidationEndpoint>();
        routes.MapGrpcService<DeskWorkflowEndpoint>();
        routes.MapGrpcService<RoomBookingApprovalEndpoint>();
        routes.MapGrpcService<RoomBookingCalculationEndpoint>();
        routes.MapGrpcService<RoomBookingLifecycleEndpoint>();
        routes.MapGrpcService<RoomBookingReportingEndpoint>();
        routes.MapGrpcService<RoomBookingValidationEndpoint>();
        routes.MapGrpcService<RoomBookingWorkflowEndpoint>();
        routes.MapGrpcService<VisitorPassApprovalEndpoint>();
        routes.MapGrpcService<VisitorPassCalculationEndpoint>();
        routes.MapGrpcService<VisitorPassLifecycleEndpoint>();
        routes.MapGrpcService<VisitorPassReportingEndpoint>();
        routes.MapGrpcService<VisitorPassSyncEndpoint>();
        routes.MapGrpcService<VisitorPassValidationEndpoint>();
    }
}
