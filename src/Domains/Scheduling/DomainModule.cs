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

namespace Acme.Workforce.Scheduling;

/// <summary>
/// Registers the scheduling domain and its gRPC routes.
/// </summary>
public static class DomainModule
{
    public static IServiceCollection AddScheduling(this IServiceCollection services)
    {
        services.AddTransient<AvailabilityWindowApprovalEndpoint>();
        services.AddTransient<IAvailabilityWindowApprovalEndpointSendRecallAvailabilityWindow1Port, AvailabilityWindowApprovalEndpointSendRecallAvailabilityWindow1PortAdapter>();
        services.AddSingleton<INotificationHandler, AvailabilityWindowApprovalEndpointSubmitAvailabilityWindow2NotificationHandler>();
        services.AddTransient<AvailabilityWindowCalculationEndpoint>();
        services.AddTransient<IAvailabilityWindowCalculationEndpointSendEstimateAvailabilityWindow1Port, AvailabilityWindowCalculationEndpointSendEstimateAvailabilityWindow1PortAdapter>();
        services.AddTransient<AvailabilityWindowLifecycleEndpoint>();
        services.AddTransient<AvailabilityWindowValidationEndpoint>();
        services.AddTransient<IAvailabilityWindowValidationEndpointSendPreviewAvailabilityWindow1Port, AvailabilityWindowValidationEndpointSendPreviewAvailabilityWindow1PortAdapter>();
        services.AddSingleton<INotificationHandler, AvailabilityWindowValidationEndpointPreviewAvailabilityWindow2NotificationHandler>();
        services.AddTransient<AvailabilityWindowWorkflowEndpoint>();
        services.AddTransient<RosterApprovalEndpoint>();
        services.AddTransient<IRosterApprovalEndpointSendSubmitRoster3Port, RosterApprovalEndpointSendSubmitRoster3PortAdapter>();
        services.AddTransient<RosterCalculationEndpoint>();
        services.AddTransient<IRosterCalculationEndpointSendQuoteRoster2Port, RosterCalculationEndpointSendQuoteRoster2PortAdapter>();
        services.AddTransient<RosterQueryEndpoint>();
        services.AddTransient<IRosterQueryEndpointSendSuggestRosters1Port, RosterQueryEndpointSendSuggestRosters1PortAdapter>();
        services.AddTransient<RosterValidationEndpoint>();
        services.AddTransient<IRosterValidationEndpointSendPreviewRoster1Port, RosterValidationEndpointSendPreviewRoster1PortAdapter>();
        services.AddTransient<RosterWorkflowEndpoint>();
        services.AddTransient<IRosterWorkflowEndpointSendRetryRosterWorkflow2Port, RosterWorkflowEndpointSendRetryRosterWorkflow2PortAdapter>();
        services.AddTransient<ShiftApprovalEndpoint>();
        services.AddSingleton<INotificationHandler, ShiftApprovalEndpointGetShiftApprovalStatus1NotificationHandler>();
        services.AddTransient<ShiftQueryEndpoint>();
        services.AddTransient<ShiftEndpoint>();
        services.AddTransient<ShiftValidationEndpoint>();
        services.AddTransient<IShiftValidationEndpointSendValidateShift2Port, ShiftValidationEndpointSendValidateShift2PortAdapter>();
        services.AddTransient<ShiftWorkflowEndpoint>();
        services.AddTransient<SwapRequestApprovalEndpoint>();
        services.AddTransient<ISwapRequestApprovalEndpointSendApproveSwapRequest0Port, SwapRequestApprovalEndpointSendApproveSwapRequest0PortAdapter>();
        services.AddTransient<SwapRequestCalculationEndpoint>();
        services.AddTransient<ISwapRequestCalculationEndpointSendCalculateSwapRequest0Port, SwapRequestCalculationEndpointSendCalculateSwapRequest0PortAdapter>();
        services.AddSingleton<INotificationHandler, SwapRequestCalculationEndpointQuoteSwapRequest0NotificationHandler>();
        services.AddTransient<SwapRequestLifecycleEndpoint>();
        services.AddTransient<SwapRequestValidationEndpoint>();
        return services;
    }
    public static void MapScheduling(this IEndpointRouteBuilder routes)
    {
        routes.MapGrpcService<AvailabilityWindowApprovalEndpoint>();
        routes.MapGrpcService<AvailabilityWindowCalculationEndpoint>();
        routes.MapGrpcService<AvailabilityWindowLifecycleEndpoint>();
        routes.MapGrpcService<AvailabilityWindowValidationEndpoint>();
        routes.MapGrpcService<AvailabilityWindowWorkflowEndpoint>();
        routes.MapGrpcService<RosterApprovalEndpoint>();
        routes.MapGrpcService<RosterCalculationEndpoint>();
        routes.MapGrpcService<RosterQueryEndpoint>();
        routes.MapGrpcService<RosterValidationEndpoint>();
        routes.MapGrpcService<RosterWorkflowEndpoint>();
        routes.MapGrpcService<ShiftApprovalEndpoint>();
        routes.MapGrpcService<ShiftQueryEndpoint>();
        routes.MapGrpcService<ShiftEndpoint>();
        routes.MapGrpcService<ShiftValidationEndpoint>();
        routes.MapGrpcService<ShiftWorkflowEndpoint>();
        routes.MapGrpcService<SwapRequestApprovalEndpoint>();
        routes.MapGrpcService<SwapRequestCalculationEndpoint>();
        routes.MapGrpcService<SwapRequestLifecycleEndpoint>();
        routes.MapGrpcService<SwapRequestValidationEndpoint>();
    }
}
