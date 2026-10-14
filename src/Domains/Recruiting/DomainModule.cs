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

namespace Acme.Workforce.Recruiting;

/// <summary>
/// Registers the recruiting domain and its gRPC routes.
/// </summary>
public static class DomainModule
{
    public static IServiceCollection AddRecruiting(this IServiceCollection services)
    {
        services.AddTransient<CandidateApprovalEndpoint>();
        services.AddTransient<ICandidateApprovalEndpointSendApproveCandidate0Port, CandidateApprovalEndpointSendApproveCandidate0PortAdapter>();
        services.AddTransient<CandidateCalculationEndpoint>();
        services.AddTransient<ICandidateCalculationEndpointSendCalculateCandidate0Port, CandidateCalculationEndpointSendCalculateCandidate0PortAdapter>();
        services.AddTransient<ICandidateCalculationEndpointSendQuoteCandidate1Port, CandidateCalculationEndpointSendQuoteCandidate1PortAdapter>();
        services.AddSingleton<INotificationHandler, CandidateCalculationEndpointSimulateCandidate0NotificationHandler>();
        services.AddTransient<CandidateLifecycleEndpoint>();
        services.AddTransient<ICandidateLifecycleEndpointSendSuspendCandidate0Port, CandidateLifecycleEndpointSendSuspendCandidate0PortAdapter>();
        services.AddTransient<CandidateQueryEndpoint>();
        services.AddTransient<CandidateReportingEndpoint>();
        services.AddTransient<CandidateValidationEndpoint>();
        services.AddSingleton<INotificationHandler, CandidateValidationEndpointCheckCandidateEligibility0NotificationHandler>();
        services.AddTransient<CandidateWorkflowEndpoint>();
        services.AddTransient<ICandidateWorkflowEndpointSendListCandidateWorkflows0Port, CandidateWorkflowEndpointSendListCandidateWorkflows0PortAdapter>();
        services.AddTransient<InterviewApprovalEndpoint>();
        services.AddTransient<IInterviewApprovalEndpointSendEscalateInterview0Port, InterviewApprovalEndpointSendEscalateInterview0PortAdapter>();
        services.AddTransient<InterviewCalculationEndpoint>();
        services.AddTransient<IInterviewCalculationEndpointSendCompareInterviewOptions1Port, InterviewCalculationEndpointSendCompareInterviewOptions1PortAdapter>();
        services.AddTransient<InterviewLifecycleEndpoint>();
        services.AddTransient<IInterviewLifecycleEndpointSendGetInterviewLifecycle0Port, InterviewLifecycleEndpointSendGetInterviewLifecycle0PortAdapter>();
        services.AddTransient<InterviewReportingEndpoint>();
        services.AddTransient<IInterviewReportingEndpointSendGenerateInterviewReport0Port, InterviewReportingEndpointSendGenerateInterviewReport0PortAdapter>();
        services.AddSingleton<INotificationHandler, InterviewReportingEndpointScheduleInterviewReport2NotificationHandler>();
        services.AddTransient<InterviewSyncEndpoint>();
        services.AddTransient<IInterviewSyncEndpointSendPushInterview0Port, InterviewSyncEndpointSendPushInterview0PortAdapter>();
        services.AddTransient<InterviewValidationEndpoint>();
        services.AddTransient<InterviewWorkflowEndpoint>();
        services.AddSingleton<INotificationHandler, InterviewWorkflowEndpointPauseInterviewWorkflow1NotificationHandler>();
        services.AddTransient<JobOpeningApprovalEndpoint>();
        services.AddTransient<IJobOpeningApprovalEndpointSendListPendingJobOpenings0Port, JobOpeningApprovalEndpointSendListPendingJobOpenings0PortAdapter>();
        services.AddTransient<JobOpeningCalculationEndpoint>();
        services.AddTransient<JobOpeningLifecycleEndpoint>();
        services.AddSingleton<INotificationHandler, JobOpeningLifecycleEndpointReinstateJobOpening0NotificationHandler>();
        services.AddTransient<JobOpeningQueryEndpoint>();
        services.AddTransient<JobOpeningEndpoint>();
        services.AddSingleton<INotificationHandler, JobOpeningEndpointWatchJobOpenings2NotificationHandler>();
        services.AddTransient<JobOpeningValidationEndpoint>();
        services.AddTransient<IJobOpeningValidationEndpointSendLintJobOpenings1Port, JobOpeningValidationEndpointSendLintJobOpenings1PortAdapter>();
        services.AddTransient<JobOpeningWorkflowEndpoint>();
        services.AddTransient<OfferApprovalEndpoint>();
        services.AddSingleton<INotificationHandler, OfferApprovalEndpointListPendingOffers0NotificationHandler>();
        services.AddTransient<OfferCalculationEndpoint>();
        services.AddTransient<IOfferCalculationEndpointSendSimulateOffer0Port, OfferCalculationEndpointSendSimulateOffer0PortAdapter>();
        services.AddTransient<OfferLifecycleEndpoint>();
        services.AddTransient<IOfferLifecycleEndpointSendTerminateOffer2Port, OfferLifecycleEndpointSendTerminateOffer2PortAdapter>();
        services.AddTransient<OfferReportingEndpoint>();
        services.AddSingleton<INotificationHandler, OfferReportingEndpointStreamOfferMetrics2NotificationHandler>();
        services.AddTransient<OfferSyncEndpoint>();
        services.AddTransient<IOfferSyncEndpointSendReconcileOffers0Port, OfferSyncEndpointSendReconcileOffers0PortAdapter>();
        services.AddTransient<OfferValidationEndpoint>();
        services.AddTransient<IOfferValidationEndpointSendNormalizeOffer1Port, OfferValidationEndpointSendNormalizeOffer1PortAdapter>();
        return services;
    }
    public static void MapRecruiting(this IEndpointRouteBuilder routes)
    {
        routes.MapGrpcService<CandidateApprovalEndpoint>();
        routes.MapGrpcService<CandidateCalculationEndpoint>();
        routes.MapGrpcService<CandidateLifecycleEndpoint>();
        routes.MapGrpcService<CandidateQueryEndpoint>();
        routes.MapGrpcService<CandidateReportingEndpoint>();
        routes.MapGrpcService<CandidateValidationEndpoint>();
        routes.MapGrpcService<CandidateWorkflowEndpoint>();
        routes.MapGrpcService<InterviewApprovalEndpoint>();
        routes.MapGrpcService<InterviewCalculationEndpoint>();
        routes.MapGrpcService<InterviewLifecycleEndpoint>();
        routes.MapGrpcService<InterviewReportingEndpoint>();
        routes.MapGrpcService<InterviewSyncEndpoint>();
        routes.MapGrpcService<InterviewValidationEndpoint>();
        routes.MapGrpcService<InterviewWorkflowEndpoint>();
        routes.MapGrpcService<JobOpeningApprovalEndpoint>();
        routes.MapGrpcService<JobOpeningCalculationEndpoint>();
        routes.MapGrpcService<JobOpeningLifecycleEndpoint>();
        routes.MapGrpcService<JobOpeningQueryEndpoint>();
        routes.MapGrpcService<JobOpeningEndpoint>();
        routes.MapGrpcService<JobOpeningValidationEndpoint>();
        routes.MapGrpcService<JobOpeningWorkflowEndpoint>();
        routes.MapGrpcService<OfferApprovalEndpoint>();
        routes.MapGrpcService<OfferCalculationEndpoint>();
        routes.MapGrpcService<OfferLifecycleEndpoint>();
        routes.MapGrpcService<OfferReportingEndpoint>();
        routes.MapGrpcService<OfferSyncEndpoint>();
        routes.MapGrpcService<OfferValidationEndpoint>();
    }
}
