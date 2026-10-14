using Acme.Workforce.Platform;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiDrivers = Acme.Drivers.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiPatients = Acme.Patients.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiPrescriptions = Acme.Prescriptions.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSearch = Acme.Search.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Onboarding;

/// <summary>
/// The application port for SuggestOnboardingTasks.
/// </summary>
public interface IOnboardingTaskWorkflowEndpointSendInspectOnboardingTaskWorkflow0Port
{
    Task<ApiOnboarding.SuggestOnboardingTasksResponse> SendAsync(ApiOnboarding.SuggestOnboardingTasksRequest request, CancellationToken token);
}

public sealed class OnboardingTaskWorkflowEndpointSendInspectOnboardingTaskWorkflow0PortAdapter(ApiOnboarding.OnboardingTaskQueryService.OnboardingTaskQueryServiceClient client) : IOnboardingTaskWorkflowEndpointSendInspectOnboardingTaskWorkflow0Port
{
    public async Task<ApiOnboarding.SuggestOnboardingTasksResponse> SendAsync(ApiOnboarding.SuggestOnboardingTasksRequest request, CancellationToken token)
    {
        using var rpc = client.SuggestOnboardingTasksAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class OnboardingTaskWorkflowEndpointRequests
{
    /// <summary>
    /// Builds the RejectOnboardingTask request from the current workflow state.
    /// </summary>
    public static ApiOnboarding.RejectOnboardingTaskRequest StartOnboardingTaskWorkflowToRejectOnboardingTask0(this ApiOnboarding.StartOnboardingTaskWorkflowRequest request)
    {
        return new ApiOnboarding.RejectOnboardingTaskRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (request.OnboardingTask?.Assignee?.Email ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.OnboardingTask?.Audit?.Etag ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Details?.Notify?.PreferredChannel ?? ""),
                },
                Sla = new ApiOnboarding.OnboardingTaskSla
                {
                    DueAt = (request.OnboardingTask?.Sla?.DueAt ?? new Timestamp()),
                },
            },
        };
    }
}
