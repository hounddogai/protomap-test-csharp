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
using ApiAuthorization = Acme.Authorization.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

/// <summary>
/// The application port for PreviewMetricSeries.
/// </summary>
public interface IBuildingValidationEndpointSendValidateBuilding0Port
{
    Task<ApiMetrics.PreviewMetricSeriesResponse> SendAsync(ApiMetrics.PreviewMetricSeriesRequest request, CancellationToken token);
}

public sealed class BuildingValidationEndpointSendValidateBuilding0PortAdapter(ApiMetrics.MetricSeriesValidationService.MetricSeriesValidationServiceClient client) : IBuildingValidationEndpointSendValidateBuilding0Port
{
    public async Task<ApiMetrics.PreviewMetricSeriesResponse> SendAsync(ApiMetrics.PreviewMetricSeriesRequest request, CancellationToken token)
    {
        using var rpc = client.PreviewMetricSeriesAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class BuildingValidationEndpointRequests
{
    /// <summary>
    /// Builds the CalculateDesk request from the current workflow state.
    /// </summary>
    public static ApiWorkplaces.CalculateDeskRequest ScoreBuildingToCalculateDesk1(this ApiWorkplaces.ScoreBuildingRequest request)
    {
        return new ApiWorkplaces.CalculateDeskRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Desk = new ApiWorkplaces.Desk
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Building?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
            Details = new ApiWorkplaces.CalculateDeskRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.Building?.Location?.Region ?? ""),
                },
            },
        };
    }
}
