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
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSearch = Acme.Search.V1;
using ApiSellers = Acme.Sellers.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Licensing;

/// <summary>
/// The application port for ScheduleEntitlementExpiry.
/// </summary>
public interface ILicenseAssignmentApprovalEndpointSendEscalateLicenseAssignment3Port
{
    Task<ApiLicensing.ScheduleEntitlementExpiryResponse> SendAsync(ApiLicensing.ScheduleEntitlementExpiryRequest request, CancellationToken token);
}

public sealed class LicenseAssignmentApprovalEndpointSendEscalateLicenseAssignment3PortAdapter(ApiLicensing.EntitlementLifecycleService.EntitlementLifecycleServiceClient client) : ILicenseAssignmentApprovalEndpointSendEscalateLicenseAssignment3Port
{
    public async Task<ApiLicensing.ScheduleEntitlementExpiryResponse> SendAsync(ApiLicensing.ScheduleEntitlementExpiryRequest request, CancellationToken token)
    {
        using var rpc = client.ScheduleEntitlementExpiryAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class LicenseAssignmentApprovalEndpointRequests
{
    /// <summary>
    /// Builds the ListSoftwareLicenses request from the current workflow state.
    /// </summary>
    public static ApiLicensing.ListSoftwareLicensesRequest EscalateLicenseAssignmentToListSoftwareLicenses2(this ApiLicensing.EscalateLicenseAssignmentRequest request)
    {
        return new ApiLicensing.ListSoftwareLicensesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLicensing.ListSoftwareLicensesRequest.Types.Filter
            {
                CreatedAfter = (request.LicenseAssignment?.Audit?.CreatedAt ?? new Timestamp()),
                OwnerId = (request.LicenseAssignment?.Counterparty?.EntitlementId ?? ""),
                Region = (request.LicenseAssignment?.BillingAddress?.Region ?? ""),
            },
        };
    }
}
