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
using ApiBenefits = Acme.Benefits.V1;
using ApiCommon = Acme.Common.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiSearch = Acme.Search.V1;
using ApiShipping = Acme.Shipping.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Onboarding;

/// <summary>
/// The application port for CalculateOnboardingTask.
/// </summary>
public interface IOnboardingTaskLifecycleEndpointSendSuspendOnboardingTask1Port
{
    Task<ApiOnboarding.CalculateOnboardingTaskResponse> SendAsync(ApiOnboarding.CalculateOnboardingTaskRequest request, CancellationToken token);
}

public sealed class OnboardingTaskLifecycleEndpointSendSuspendOnboardingTask1PortAdapter(ApiOnboarding.OnboardingTaskCalculationService.OnboardingTaskCalculationServiceClient client) : IOnboardingTaskLifecycleEndpointSendSuspendOnboardingTask1Port
{
    public async Task<ApiOnboarding.CalculateOnboardingTaskResponse> SendAsync(ApiOnboarding.CalculateOnboardingTaskRequest request, CancellationToken token)
    {
        using var rpc = client.CalculateOnboardingTaskAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class OnboardingTaskLifecycleEndpointRequests
{
    /// <summary>
    /// Builds the RetryTravelBookingWorkflow request from the current workflow state.
    /// </summary>
    public static ApiTravel.RetryTravelBookingWorkflowRequest SuspendOnboardingTaskToRetryTravelBookingWorkflow0(this ApiOnboarding.SuspendOnboardingTaskRequest request)
    {
        return new ApiTravel.RetryTravelBookingWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.RetryTravelBookingWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                    PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                },
            },
            TravelBooking = new ApiTravel.TravelBooking
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                },
            },
        };
    }
}
