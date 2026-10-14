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
using ApiAppointments = Acme.Appointments.V1;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiScheduling = Acme.Scheduling.V1;

namespace Acme.Workforce.Onboarding;

/// <summary>
/// The application port for SummarizeAppointments.
/// </summary>
public interface IOnboardingPlanEndpointSendWatchOnboardingPlans1Port
{
    Task<ApiAppointments.SummarizeAppointmentsResponse> SendAsync(ApiAppointments.SummarizeAppointmentsRequest request, CancellationToken token);
}

public sealed class OnboardingPlanEndpointSendWatchOnboardingPlans1PortAdapter(ApiAppointments.AppointmentQueryService.AppointmentQueryServiceClient client) : IOnboardingPlanEndpointSendWatchOnboardingPlans1Port
{
    public async Task<ApiAppointments.SummarizeAppointmentsResponse> SendAsync(ApiAppointments.SummarizeAppointmentsRequest request, CancellationToken token)
    {
        using var rpc = client.SummarizeAppointmentsAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class OnboardingPlanEndpointRequests
{
    /// <summary>
    /// Builds the CompareServiceAssetOptions request from the current workflow state.
    /// </summary>
    public static ApiItsm.CompareServiceAssetOptionsRequest WatchOnboardingPlansToCompareServiceAssetOptions0(this ApiOnboarding.WatchOnboardingPlansRequest request)
    {
        return new ApiItsm.CompareServiceAssetOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.CompareServiceAssetOptionsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Subscriber?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
    }
}
