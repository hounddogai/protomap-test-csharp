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

namespace Acme.Workforce.Performance;

/// <summary>
/// Registers the performance domain and its gRPC routes.
/// </summary>
public static class DomainModule
{
    public static IServiceCollection AddPerformance(this IServiceCollection services)
    {
        services.AddTransient<CalibrationApprovalEndpoint>();
        services.AddTransient<ICalibrationApprovalEndpointSendSubmitCalibration1Port, CalibrationApprovalEndpointSendSubmitCalibration1PortAdapter>();
        services.AddTransient<CalibrationCalculationEndpoint>();
        services.AddTransient<CalibrationLifecycleEndpoint>();
        services.AddTransient<ICalibrationLifecycleEndpointSendActivateCalibration2Port, CalibrationLifecycleEndpointSendActivateCalibration2PortAdapter>();
        services.AddSingleton<INotificationHandler, CalibrationLifecycleEndpointExpireCalibration0NotificationHandler>();
        services.AddTransient<CalibrationValidationEndpoint>();
        services.AddTransient<ICalibrationValidationEndpointSendCheckCalibrationEligibility1Port, CalibrationValidationEndpointSendCheckCalibrationEligibility1PortAdapter>();
        services.AddTransient<FeedbackApprovalEndpoint>();
        services.AddTransient<FeedbackCalculationEndpoint>();
        services.AddTransient<IFeedbackCalculationEndpointSendCompareFeedbackOptions0Port, FeedbackCalculationEndpointSendCompareFeedbackOptions0PortAdapter>();
        services.AddTransient<IFeedbackCalculationEndpointSendSimulateFeedback0Port, FeedbackCalculationEndpointSendSimulateFeedback0PortAdapter>();
        services.AddTransient<FeedbackValidationEndpoint>();
        services.AddTransient<FeedbackWorkflowEndpoint>();
        services.AddSingleton<INotificationHandler, FeedbackWorkflowEndpointCancelFeedbackWorkflow3NotificationHandler>();
        services.AddTransient<GoalApprovalEndpoint>();
        services.AddTransient<IGoalApprovalEndpointSendGetGoalApprovalStatus0Port, GoalApprovalEndpointSendGetGoalApprovalStatus0PortAdapter>();
        services.AddTransient<GoalQueryEndpoint>();
        services.AddTransient<IGoalQueryEndpointSendLookupGoal0Port, GoalQueryEndpointSendLookupGoal0PortAdapter>();
        services.AddSingleton<INotificationHandler, GoalQueryEndpointSummarizeGoals0NotificationHandler>();
        services.AddTransient<GoalValidationEndpoint>();
        services.AddTransient<GoalWorkflowEndpoint>();
        services.AddTransient<IGoalWorkflowEndpointSendListGoalWorkflows0Port, GoalWorkflowEndpointSendListGoalWorkflows0PortAdapter>();
        services.AddTransient<ReviewApprovalEndpoint>();
        services.AddTransient<IReviewApprovalEndpointSendGetReviewApprovalStatus0Port, ReviewApprovalEndpointSendGetReviewApprovalStatus0PortAdapter>();
        services.AddTransient<ReviewQueryEndpoint>();
        services.AddTransient<ReviewEndpoint>();
        services.AddTransient<ReviewWorkflowEndpoint>();
        services.AddSingleton<INotificationHandler, ReviewWorkflowEndpointListReviewWorkflows0NotificationHandler>();
        return services;
    }
    public static void MapPerformance(this IEndpointRouteBuilder routes)
    {
        routes.MapGrpcService<CalibrationApprovalEndpoint>();
        routes.MapGrpcService<CalibrationCalculationEndpoint>();
        routes.MapGrpcService<CalibrationLifecycleEndpoint>();
        routes.MapGrpcService<CalibrationValidationEndpoint>();
        routes.MapGrpcService<FeedbackApprovalEndpoint>();
        routes.MapGrpcService<FeedbackCalculationEndpoint>();
        routes.MapGrpcService<FeedbackValidationEndpoint>();
        routes.MapGrpcService<FeedbackWorkflowEndpoint>();
        routes.MapGrpcService<GoalApprovalEndpoint>();
        routes.MapGrpcService<GoalQueryEndpoint>();
        routes.MapGrpcService<GoalValidationEndpoint>();
        routes.MapGrpcService<GoalWorkflowEndpoint>();
        routes.MapGrpcService<ReviewApprovalEndpoint>();
        routes.MapGrpcService<ReviewQueryEndpoint>();
        routes.MapGrpcService<ReviewEndpoint>();
        routes.MapGrpcService<ReviewWorkflowEndpoint>();
    }
}
