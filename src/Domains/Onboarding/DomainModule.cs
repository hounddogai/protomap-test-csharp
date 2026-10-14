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

namespace Acme.Workforce.Onboarding;

/// <summary>
/// Registers the onboarding domain and its gRPC routes.
/// </summary>
public static class DomainModule
{
    public static IServiceCollection AddOnboarding(this IServiceCollection services)
    {
        services.AddTransient<EquipmentApprovalEndpoint>();
        services.AddTransient<IEquipmentApprovalEndpointSendApproveEquipment0Port, EquipmentApprovalEndpointSendApproveEquipment0PortAdapter>();
        services.AddSingleton<INotificationHandler, EquipmentApprovalEndpointGetEquipmentApprovalStatus0NotificationHandler>();
        services.AddTransient<EquipmentCalculationEndpoint>();
        services.AddTransient<EquipmentLifecycleEndpoint>();
        services.AddTransient<EquipmentReportingEndpoint>();
        services.AddTransient<IEquipmentReportingEndpointSendGenerateEquipmentReport2Port, EquipmentReportingEndpointSendGenerateEquipmentReport2PortAdapter>();
        services.AddSingleton<INotificationHandler, EquipmentReportingEndpointShareEquipmentReport1NotificationHandler>();
        services.AddTransient<EquipmentValidationEndpoint>();
        services.AddTransient<EquipmentWorkflowEndpoint>();
        services.AddTransient<IEquipmentWorkflowEndpointSendCancelEquipmentWorkflow0Port, EquipmentWorkflowEndpointSendCancelEquipmentWorkflow0PortAdapter>();
        services.AddSingleton<INotificationHandler, EquipmentWorkflowEndpointRetryEquipmentWorkflow2NotificationHandler>();
        services.AddTransient<OnboardingPlanApprovalEndpoint>();
        services.AddTransient<IOnboardingPlanApprovalEndpointSendGetOnboardingPlanApprovalStatus0Port, OnboardingPlanApprovalEndpointSendGetOnboardingPlanApprovalStatus0PortAdapter>();
        services.AddTransient<OnboardingPlanCalculationEndpoint>();
        services.AddTransient<OnboardingPlanLifecycleEndpoint>();
        services.AddTransient<IOnboardingPlanLifecycleEndpointSendRenewOnboardingPlan0Port, OnboardingPlanLifecycleEndpointSendRenewOnboardingPlan0PortAdapter>();
        services.AddSingleton<INotificationHandler, OnboardingPlanLifecycleEndpointActivateOnboardingPlan0NotificationHandler>();
        services.AddTransient<OnboardingPlanQueryEndpoint>();
        services.AddTransient<OnboardingPlanEndpoint>();
        services.AddTransient<IOnboardingPlanEndpointSendWatchOnboardingPlans1Port, OnboardingPlanEndpointSendWatchOnboardingPlans1PortAdapter>();
        services.AddTransient<OnboardingPlanValidationEndpoint>();
        services.AddTransient<IOnboardingPlanValidationEndpointSendValidateOnboardingPlan0Port, OnboardingPlanValidationEndpointSendValidateOnboardingPlan0PortAdapter>();
        services.AddSingleton<INotificationHandler, OnboardingPlanValidationEndpointExplainOnboardingPlanDecision1NotificationHandler>();
        services.AddTransient<OnboardingPlanWorkflowEndpoint>();
        services.AddTransient<IOnboardingPlanWorkflowEndpointSendRetryOnboardingPlanWorkflow1Port, OnboardingPlanWorkflowEndpointSendRetryOnboardingPlanWorkflow1PortAdapter>();
        services.AddTransient<OnboardingTaskApprovalEndpoint>();
        services.AddSingleton<INotificationHandler, OnboardingTaskApprovalEndpointRecallOnboardingTask0NotificationHandler>();
        services.AddTransient<OnboardingTaskCalculationEndpoint>();
        services.AddTransient<OnboardingTaskLifecycleEndpoint>();
        services.AddTransient<IOnboardingTaskLifecycleEndpointSendSuspendOnboardingTask1Port, OnboardingTaskLifecycleEndpointSendSuspendOnboardingTask1PortAdapter>();
        services.AddTransient<OnboardingTaskQueryEndpoint>();
        services.AddSingleton<INotificationHandler, OnboardingTaskQueryEndpointSummarizeOnboardingTasks1NotificationHandler>();
        services.AddTransient<OnboardingTaskReportingEndpoint>();
        services.AddTransient<IOnboardingTaskReportingEndpointSendGenerateOnboardingTaskReport2Port, OnboardingTaskReportingEndpointSendGenerateOnboardingTaskReport2PortAdapter>();
        services.AddTransient<OnboardingTaskValidationEndpoint>();
        services.AddTransient<IOnboardingTaskValidationEndpointSendGetOnboardingTaskRules0Port, OnboardingTaskValidationEndpointSendGetOnboardingTaskRules0PortAdapter>();
        services.AddTransient<OnboardingTaskWorkflowEndpoint>();
        services.AddTransient<IOnboardingTaskWorkflowEndpointSendInspectOnboardingTaskWorkflow0Port, OnboardingTaskWorkflowEndpointSendInspectOnboardingTaskWorkflow0PortAdapter>();
        return services;
    }
    public static void MapOnboarding(this IEndpointRouteBuilder routes)
    {
        routes.MapGrpcService<EquipmentApprovalEndpoint>();
        routes.MapGrpcService<EquipmentCalculationEndpoint>();
        routes.MapGrpcService<EquipmentLifecycleEndpoint>();
        routes.MapGrpcService<EquipmentReportingEndpoint>();
        routes.MapGrpcService<EquipmentValidationEndpoint>();
        routes.MapGrpcService<EquipmentWorkflowEndpoint>();
        routes.MapGrpcService<OnboardingPlanApprovalEndpoint>();
        routes.MapGrpcService<OnboardingPlanCalculationEndpoint>();
        routes.MapGrpcService<OnboardingPlanLifecycleEndpoint>();
        routes.MapGrpcService<OnboardingPlanQueryEndpoint>();
        routes.MapGrpcService<OnboardingPlanEndpoint>();
        routes.MapGrpcService<OnboardingPlanValidationEndpoint>();
        routes.MapGrpcService<OnboardingPlanWorkflowEndpoint>();
        routes.MapGrpcService<OnboardingTaskApprovalEndpoint>();
        routes.MapGrpcService<OnboardingTaskCalculationEndpoint>();
        routes.MapGrpcService<OnboardingTaskLifecycleEndpoint>();
        routes.MapGrpcService<OnboardingTaskQueryEndpoint>();
        routes.MapGrpcService<OnboardingTaskReportingEndpoint>();
        routes.MapGrpcService<OnboardingTaskValidationEndpoint>();
        routes.MapGrpcService<OnboardingTaskWorkflowEndpoint>();
    }
}
