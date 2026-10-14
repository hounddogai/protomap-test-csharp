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
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

/// <summary>
/// The application port for QuoteDesk.
/// </summary>
public interface IVisitorPassCalculationEndpointSendCalculateVisitorPass2Port
{
    Task<ApiWorkplaces.QuoteDeskResponse> SendAsync(ApiWorkplaces.QuoteDeskRequest request, CancellationToken token);
}

public sealed class VisitorPassCalculationEndpointSendCalculateVisitorPass2PortAdapter(ApiWorkplaces.DeskCalculationService.DeskCalculationServiceClient client) : IVisitorPassCalculationEndpointSendCalculateVisitorPass2Port
{
    public async Task<ApiWorkplaces.QuoteDeskResponse> SendAsync(ApiWorkplaces.QuoteDeskRequest request, CancellationToken token)
    {
        using var rpc = client.QuoteDeskAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

/// <summary>
/// The application port for CheckBuildingEligibility.
/// </summary>
public interface IVisitorPassCalculationEndpointSendRecalculateVisitorPasses2Port
{
    Task<ApiWorkplaces.CheckBuildingEligibilityResponse> SendAsync(ApiWorkplaces.CheckBuildingEligibilityRequest request, CancellationToken token);
}

public sealed class VisitorPassCalculationEndpointSendRecalculateVisitorPasses2PortAdapter(ApiWorkplaces.BuildingValidationService.BuildingValidationServiceClient client) : IVisitorPassCalculationEndpointSendRecalculateVisitorPasses2Port
{
    public async Task<ApiWorkplaces.CheckBuildingEligibilityResponse> SendAsync(ApiWorkplaces.CheckBuildingEligibilityRequest request, CancellationToken token)
    {
        using var rpc = client.CheckBuildingEligibilityAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class VisitorPassCalculationEndpointRequests
{
    /// <summary>
    /// Builds the ListFeatureFlags request from the current workflow state.
    /// </summary>
    public static ApiFeatureflags.ListFeatureFlagsRequest CalculateVisitorPassToListFeatureFlags1(this ApiWorkplaces.CalculateVisitorPassRequest request)
    {
        return new ApiFeatureflags.ListFeatureFlagsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiFeatureflags.ListFeatureFlagsRequest.Types.Filter
            {
                CreatedAfter = (request.VisitorPass?.Audit?.CreatedAt ?? new Timestamp()),
                OwnerId = (request.VisitorPass?.VisitorPassId ?? ""),
                Region = (request.Details?.Destination?.Region ?? ""),
            },
        };
    }
    /// <summary>
    /// Builds the PreviewBuilding request from the current workflow state.
    /// </summary>
    public static ApiWorkplaces.PreviewBuildingRequest RecalculateVisitorPassesToPreviewBuilding1(this ApiWorkplaces.RecalculateVisitorPassesRequest request)
    {
        return new ApiWorkplaces.PreviewBuildingRequest
        {
            Building = new ApiWorkplaces.Building
            {
                Children = { new ApiWorkplaces.Building
                {
                    Children = { new ApiWorkplaces.Building
                    {
                        Location = new ApiCommon.PostalAddress
                        {
                            CountryCode = (request.Details?.Destination?.CountryCode ?? ""),
                            Recipient = (request.VisitorPass?.BillingAddress?.Recipient ?? ""),
                        },
                    } },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
    }
}
