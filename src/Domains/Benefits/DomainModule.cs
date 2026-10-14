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

namespace Acme.Workforce.Benefits;

/// <summary>
/// Registers the benefits domain and its gRPC routes.
/// </summary>
public static class DomainModule
{
    public static IServiceCollection AddBenefits(this IServiceCollection services)
    {
        services.AddTransient<BenefitPlanApprovalEndpoint>();
        services.AddTransient<IBenefitPlanApprovalEndpointSendEscalateBenefitPlan3Port, BenefitPlanApprovalEndpointSendEscalateBenefitPlan3PortAdapter>();
        services.AddTransient<BenefitPlanQueryEndpoint>();
        services.AddTransient<BenefitPlanEndpoint>();
        services.AddTransient<BenefitPlanWorkflowEndpoint>();
        services.AddTransient<IBenefitPlanWorkflowEndpointSendStartBenefitPlanWorkflow0Port, BenefitPlanWorkflowEndpointSendStartBenefitPlanWorkflow0PortAdapter>();
        services.AddSingleton<INotificationHandler, BenefitPlanWorkflowEndpointAdvanceBenefitPlanWorkflow1NotificationHandler>();
        services.AddTransient<DependentApprovalEndpoint>();
        services.AddTransient<DependentValidationEndpoint>();
        services.AddTransient<IDependentValidationEndpointSendScoreDependent0Port, DependentValidationEndpointSendScoreDependent0PortAdapter>();
        services.AddTransient<DependentWorkflowEndpoint>();
        services.AddTransient<EnrollmentApprovalEndpoint>();
        services.AddTransient<IEnrollmentApprovalEndpointSendListPendingEnrollments0Port, EnrollmentApprovalEndpointSendListPendingEnrollments0PortAdapter>();
        services.AddSingleton<INotificationHandler, EnrollmentApprovalEndpointSubmitEnrollment0NotificationHandler>();
        services.AddTransient<EnrollmentQueryEndpoint>();
        services.AddTransient<EnrollmentWorkflowEndpoint>();
        services.AddTransient<IEnrollmentWorkflowEndpointSendSignalEnrollmentWorkflow0Port, EnrollmentWorkflowEndpointSendSignalEnrollmentWorkflow0PortAdapter>();
        services.AddTransient<LifeEventApprovalEndpoint>();
        services.AddSingleton<INotificationHandler, LifeEventApprovalEndpointSubmitLifeEvent0NotificationHandler>();
        services.AddTransient<LifeEventCalculationEndpoint>();
        services.AddTransient<ILifeEventCalculationEndpointSendRecalculateLifeEvents0Port, LifeEventCalculationEndpointSendRecalculateLifeEvents0PortAdapter>();
        services.AddTransient<LifeEventValidationEndpoint>();
        return services;
    }
    public static void MapBenefits(this IEndpointRouteBuilder routes)
    {
        routes.MapGrpcService<BenefitPlanApprovalEndpoint>();
        routes.MapGrpcService<BenefitPlanQueryEndpoint>();
        routes.MapGrpcService<BenefitPlanEndpoint>();
        routes.MapGrpcService<BenefitPlanWorkflowEndpoint>();
        routes.MapGrpcService<DependentApprovalEndpoint>();
        routes.MapGrpcService<DependentValidationEndpoint>();
        routes.MapGrpcService<DependentWorkflowEndpoint>();
        routes.MapGrpcService<EnrollmentApprovalEndpoint>();
        routes.MapGrpcService<EnrollmentQueryEndpoint>();
        routes.MapGrpcService<EnrollmentWorkflowEndpoint>();
        routes.MapGrpcService<LifeEventApprovalEndpoint>();
        routes.MapGrpcService<LifeEventCalculationEndpoint>();
        routes.MapGrpcService<LifeEventValidationEndpoint>();
    }
}
