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
using ApiCheckout = Acme.Checkout.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiDrivers = Acme.Drivers.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiShows = Acme.Shows.V1;

namespace Acme.Workforce.Itsm;

/// <summary>
/// The application port for EscalateEntitlement.
/// </summary>
public interface IServiceAssetReportingEndpointSendShareServiceAssetReport0Port
{
    Task<ApiLicensing.EscalateEntitlementResponse> SendAsync(ApiLicensing.EscalateEntitlementRequest request, CancellationToken token);
}

public sealed class ServiceAssetReportingEndpointSendShareServiceAssetReport0PortAdapter(ApiLicensing.EntitlementApprovalService.EntitlementApprovalServiceClient client) : IServiceAssetReportingEndpointSendShareServiceAssetReport0Port
{
    public async Task<ApiLicensing.EscalateEntitlementResponse> SendAsync(ApiLicensing.EscalateEntitlementRequest request, CancellationToken token)
    {
        using var rpc = client.EscalateEntitlementAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
