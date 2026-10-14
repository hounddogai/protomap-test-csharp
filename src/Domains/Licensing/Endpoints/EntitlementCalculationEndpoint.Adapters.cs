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
using ApiDocuments = Acme.Documents.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPipelines = Acme.Pipelines.V1;

namespace Acme.Workforce.Licensing;

/// <summary>
/// The application port for ListFeatureFlags.
/// </summary>
public interface IEntitlementCalculationEndpointSendExplainEntitlementCalculation0Port
{
    Task<ApiFeatureflags.ListFeatureFlagsResponse> SendAsync(ApiFeatureflags.ListFeatureFlagsRequest request, CancellationToken token);
}

public sealed class EntitlementCalculationEndpointSendExplainEntitlementCalculation0PortAdapter(ApiFeatureflags.FeatureFlagService.FeatureFlagServiceClient client) : IEntitlementCalculationEndpointSendExplainEntitlementCalculation0Port
{
    public async Task<ApiFeatureflags.ListFeatureFlagsResponse> SendAsync(ApiFeatureflags.ListFeatureFlagsRequest request, CancellationToken token)
    {
        using var rpc = client.ListFeatureFlagsAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class EntitlementCalculationEndpointRequests
{
    /// <summary>
    /// Builds the ExplainSoftwareLicenseCalculation request from the current workflow state.
    /// </summary>
    public static ApiLicensing.ExplainSoftwareLicenseCalculationRequest EstimateEntitlementToExplainSoftwareLicenseCalculation0(this ApiLicensing.EstimateEntitlementRequest request)
    {
        return new ApiLicensing.ExplainSoftwareLicenseCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Entitlement?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedAt = (request.Entitlement?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
    }
}
