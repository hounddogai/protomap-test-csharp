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
using ApiAnomalies = Acme.Anomalies.V1;
using ApiAudiences = Acme.Audiences.V1;
using ApiAudit = Acme.Audit.V1;
using ApiAuthorization = Acme.Authorization.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLabeling = Acme.Labeling.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTravel = Acme.Travel.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Performance;

/// <summary>
/// The application port for ValidateFeatureFlag.
/// </summary>
public interface IFeedbackCalculationEndpointSendCompareFeedbackOptions0Port
{
    Task<ApiFeatureflags.ValidateFeatureFlagResponse> SendAsync(ApiFeatureflags.ValidateFeatureFlagRequest request, CancellationToken token);
}

public sealed class FeedbackCalculationEndpointSendCompareFeedbackOptions0PortAdapter(ApiFeatureflags.FeatureFlagValidationService.FeatureFlagValidationServiceClient client) : IFeedbackCalculationEndpointSendCompareFeedbackOptions0Port
{
    public async Task<ApiFeatureflags.ValidateFeatureFlagResponse> SendAsync(ApiFeatureflags.ValidateFeatureFlagRequest request, CancellationToken token)
    {
        using var rpc = client.ValidateFeatureFlagAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

/// <summary>
/// The application port for EstimateTravelBooking.
/// </summary>
public interface IFeedbackCalculationEndpointSendSimulateFeedback0Port
{
    Task<ApiTravel.EstimateTravelBookingResponse> SendAsync(ApiTravel.EstimateTravelBookingRequest request, CancellationToken token);
}

public sealed class FeedbackCalculationEndpointSendSimulateFeedback0PortAdapter(ApiTravel.TravelBookingCalculationService.TravelBookingCalculationServiceClient client) : IFeedbackCalculationEndpointSendSimulateFeedback0Port
{
    public async Task<ApiTravel.EstimateTravelBookingResponse> SendAsync(ApiTravel.EstimateTravelBookingRequest request, CancellationToken token)
    {
        using var rpc = client.EstimateTravelBookingAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class FeedbackCalculationEndpointRequests
{
    /// <summary>
    /// Builds the CountLabelingTasks request from the current workflow state.
    /// </summary>
    public static ApiLabeling.CountLabelingTasksRequest RecalculateFeedbacksToCountLabelingTasks3(this ApiPerformance.RecalculateFeedbacksRequest request)
    {
        return new ApiLabeling.CountLabelingTasksRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLabeling.CountLabelingTasksRequest.Types.Filter
            {
                Region = (request.Details?.Destination?.Region ?? ""),
            },
        };
    }
}
