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

namespace Acme.Workforce.Timekeeping;

/// <summary>
/// Registers the timekeeping domain and its gRPC routes.
/// </summary>
public static class DomainModule
{
    public static IServiceCollection AddTimekeeping(this IServiceCollection services)
    {
        services.AddTransient<ClockEventApprovalEndpoint>();
        services.AddTransient<IClockEventApprovalEndpointSendCommentClockEventApproval0Port, ClockEventApprovalEndpointSendCommentClockEventApproval0PortAdapter>();
        services.AddSingleton<INotificationHandler, ClockEventApprovalEndpointDelegateClockEventApproval0NotificationHandler>();
        services.AddTransient<ClockEventCalculationEndpoint>();
        services.AddTransient<ClockEventQueryEndpoint>();
        services.AddTransient<ClockEventValidationEndpoint>();
        services.AddTransient<IClockEventValidationEndpointSendPreviewClockEvent1Port, ClockEventValidationEndpointSendPreviewClockEvent1PortAdapter>();
        services.AddSingleton<INotificationHandler, ClockEventValidationEndpointNormalizeClockEvent3NotificationHandler>();
        services.AddTransient<ClockEventWorkflowEndpoint>();
        services.AddTransient<IClockEventWorkflowEndpointSendSignalClockEventWorkflow0Port, ClockEventWorkflowEndpointSendSignalClockEventWorkflow0PortAdapter>();
        services.AddTransient<OvertimeApprovalEndpoint>();
        services.AddSingleton<INotificationHandler, OvertimeApprovalEndpointSubmitOvertime1NotificationHandler>();
        services.AddTransient<OvertimeCalculationEndpoint>();
        services.AddTransient<OvertimeLifecycleEndpoint>();
        services.AddTransient<IOvertimeLifecycleEndpointSendReinstateOvertime1Port, OvertimeLifecycleEndpointSendReinstateOvertime1PortAdapter>();
        services.AddTransient<OvertimeValidationEndpoint>();
        services.AddTransient<OvertimeWorkflowEndpoint>();
        services.AddTransient<TimesheetApprovalEndpoint>();
        services.AddTransient<TimesheetQueryEndpoint>();
        services.AddTransient<ITimesheetQueryEndpointSendSummarizeTimesheets1Port, TimesheetQueryEndpointSendSummarizeTimesheets1PortAdapter>();
        services.AddSingleton<INotificationHandler, TimesheetQueryEndpointSummarizeTimesheets0NotificationHandler>();
        services.AddTransient<TimesheetEndpoint>();
        services.AddTransient<TimesheetValidationEndpoint>();
        services.AddTransient<TimesheetWorkflowEndpoint>();
        services.AddTransient<ITimesheetWorkflowEndpointSendStartTimesheetWorkflow1Port, TimesheetWorkflowEndpointSendStartTimesheetWorkflow1PortAdapter>();
        services.AddSingleton<INotificationHandler, TimesheetWorkflowEndpointResumeTimesheetWorkflow0NotificationHandler>();
        return services;
    }
    public static void MapTimekeeping(this IEndpointRouteBuilder routes)
    {
        routes.MapGrpcService<ClockEventApprovalEndpoint>();
        routes.MapGrpcService<ClockEventCalculationEndpoint>();
        routes.MapGrpcService<ClockEventQueryEndpoint>();
        routes.MapGrpcService<ClockEventValidationEndpoint>();
        routes.MapGrpcService<ClockEventWorkflowEndpoint>();
        routes.MapGrpcService<OvertimeApprovalEndpoint>();
        routes.MapGrpcService<OvertimeCalculationEndpoint>();
        routes.MapGrpcService<OvertimeLifecycleEndpoint>();
        routes.MapGrpcService<OvertimeValidationEndpoint>();
        routes.MapGrpcService<OvertimeWorkflowEndpoint>();
        routes.MapGrpcService<TimesheetApprovalEndpoint>();
        routes.MapGrpcService<TimesheetQueryEndpoint>();
        routes.MapGrpcService<TimesheetEndpoint>();
        routes.MapGrpcService<TimesheetValidationEndpoint>();
        routes.MapGrpcService<TimesheetWorkflowEndpoint>();
    }
}
