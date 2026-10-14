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

namespace Acme.Workforce.Learning;

/// <summary>
/// Registers the learning domain and its gRPC routes.
/// </summary>
public static class DomainModule
{
    public static IServiceCollection AddLearning(this IServiceCollection services)
    {
        services.AddTransient<CertificationApprovalEndpoint>();
        services.AddSingleton<INotificationHandler, CertificationApprovalEndpointSubmitCertification3NotificationHandler>();
        services.AddTransient<CertificationCalculationEndpoint>();
        services.AddTransient<CertificationValidationEndpoint>();
        services.AddTransient<CertificationWorkflowEndpoint>();
        services.AddTransient<ICertificationWorkflowEndpointSendSignalCertificationWorkflow0Port, CertificationWorkflowEndpointSendSignalCertificationWorkflow0PortAdapter>();
        services.AddSingleton<INotificationHandler, CertificationWorkflowEndpointSignalCertificationWorkflow0NotificationHandler>();
        services.AddTransient<CourseApprovalEndpoint>();
        services.AddTransient<CourseEnrollmentApprovalEndpoint>();
        services.AddTransient<ICourseEnrollmentApprovalEndpointSendRejectCourseEnrollment3Port, CourseEnrollmentApprovalEndpointSendRejectCourseEnrollment3PortAdapter>();
        services.AddTransient<CourseEnrollmentCalculationEndpoint>();
        services.AddTransient<CourseEnrollmentLifecycleEndpoint>();
        services.AddTransient<ICourseEnrollmentLifecycleEndpointSendActivateCourseEnrollment1Port, CourseEnrollmentLifecycleEndpointSendActivateCourseEnrollment1PortAdapter>();
        services.AddTransient<CourseEnrollmentValidationEndpoint>();
        services.AddTransient<ICourseEnrollmentValidationEndpointSendGetCourseEnrollmentRules0Port, CourseEnrollmentValidationEndpointSendGetCourseEnrollmentRules0PortAdapter>();
        services.AddTransient<CourseQueryEndpoint>();
        services.AddTransient<ICourseQueryEndpointSendCountCourses0Port, CourseQueryEndpointSendCountCourses0PortAdapter>();
        services.AddTransient<CourseEndpoint>();
        services.AddSingleton<INotificationHandler, CourseEndpointBatchCreateCourses0NotificationHandler>();
        services.AddTransient<CourseValidationEndpoint>();
        services.AddTransient<ICourseValidationEndpointSendPreviewCourse2Port, CourseValidationEndpointSendPreviewCourse2PortAdapter>();
        services.AddTransient<CourseWorkflowEndpoint>();
        services.AddTransient<ICourseWorkflowEndpointSendStartCourseWorkflow1Port, CourseWorkflowEndpointSendStartCourseWorkflow1PortAdapter>();
        services.AddSingleton<INotificationHandler, CourseWorkflowEndpointStartCourseWorkflow2NotificationHandler>();
        services.AddTransient<LearningPathApprovalEndpoint>();
        services.AddTransient<ILearningPathApprovalEndpointSendRecallLearningPath1Port, LearningPathApprovalEndpointSendRecallLearningPath1PortAdapter>();
        services.AddTransient<LearningPathCalculationEndpoint>();
        services.AddTransient<ILearningPathCalculationEndpointSendGetLearningPathRates0Port, LearningPathCalculationEndpointSendGetLearningPathRates0PortAdapter>();
        services.AddTransient<LearningPathQueryEndpoint>();
        services.AddSingleton<INotificationHandler, LearningPathQueryEndpointResolveLearningPaths1NotificationHandler>();
        services.AddTransient<LearningPathValidationEndpoint>();
        services.AddTransient<LearningPathWorkflowEndpoint>();
        services.AddTransient<ILearningPathWorkflowEndpointSendCancelLearningPathWorkflow0Port, LearningPathWorkflowEndpointSendCancelLearningPathWorkflow0PortAdapter>();
        return services;
    }
    public static void MapLearning(this IEndpointRouteBuilder routes)
    {
        routes.MapGrpcService<CertificationApprovalEndpoint>();
        routes.MapGrpcService<CertificationCalculationEndpoint>();
        routes.MapGrpcService<CertificationValidationEndpoint>();
        routes.MapGrpcService<CertificationWorkflowEndpoint>();
        routes.MapGrpcService<CourseApprovalEndpoint>();
        routes.MapGrpcService<CourseEnrollmentApprovalEndpoint>();
        routes.MapGrpcService<CourseEnrollmentCalculationEndpoint>();
        routes.MapGrpcService<CourseEnrollmentLifecycleEndpoint>();
        routes.MapGrpcService<CourseEnrollmentValidationEndpoint>();
        routes.MapGrpcService<CourseQueryEndpoint>();
        routes.MapGrpcService<CourseEndpoint>();
        routes.MapGrpcService<CourseValidationEndpoint>();
        routes.MapGrpcService<CourseWorkflowEndpoint>();
        routes.MapGrpcService<LearningPathApprovalEndpoint>();
        routes.MapGrpcService<LearningPathCalculationEndpoint>();
        routes.MapGrpcService<LearningPathQueryEndpoint>();
        routes.MapGrpcService<LearningPathValidationEndpoint>();
        routes.MapGrpcService<LearningPathWorkflowEndpoint>();
    }
}
