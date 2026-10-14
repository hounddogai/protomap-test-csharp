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
using ApiCommon = Acme.Common.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiLineage = Acme.Lineage.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiSearch = Acme.Search.V1;

namespace Acme.Workforce.Licensing;

/// <summary>
/// The application port for CountMetricSerieses.
/// </summary>
public interface ISoftwareLicenseValidationEndpointSendCheckSoftwareLicenseEligibility0Port
{
    Task<ApiMetrics.CountMetricSeriesesResponse> SendAsync(ApiMetrics.CountMetricSeriesesRequest request, CancellationToken token);
}

public sealed class SoftwareLicenseValidationEndpointSendCheckSoftwareLicenseEligibility0PortAdapter(ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient client) : ISoftwareLicenseValidationEndpointSendCheckSoftwareLicenseEligibility0Port
{
    public async Task<ApiMetrics.CountMetricSeriesesResponse> SendAsync(ApiMetrics.CountMetricSeriesesRequest request, CancellationToken token)
    {
        using var rpc = client.CountMetricSeriesesAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

/// <summary>
/// The application port for ExplainEntitlementCalculation.
/// </summary>
public interface ISoftwareLicenseValidationEndpointSendValidateSoftwareLicense2Port
{
    Task<ApiLicensing.ExplainEntitlementCalculationResponse> SendAsync(ApiLicensing.ExplainEntitlementCalculationRequest request, CancellationToken token);
}

public sealed class SoftwareLicenseValidationEndpointSendValidateSoftwareLicense2PortAdapter(ApiLicensing.EntitlementCalculationService.EntitlementCalculationServiceClient client) : ISoftwareLicenseValidationEndpointSendValidateSoftwareLicense2Port
{
    public async Task<ApiLicensing.ExplainEntitlementCalculationResponse> SendAsync(ApiLicensing.ExplainEntitlementCalculationRequest request, CancellationToken token)
    {
        using var rpc = client.ExplainEntitlementCalculationAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
