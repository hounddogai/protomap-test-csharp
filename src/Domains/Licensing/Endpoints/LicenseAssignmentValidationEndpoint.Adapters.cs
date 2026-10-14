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
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiRecommendations = Acme.Recommendations.V1;
using ApiSearch = Acme.Search.V1;

namespace Acme.Workforce.Licensing;

/// <summary>
/// The application port for NormalizeLedgerAccount.
/// </summary>
public interface ILicenseAssignmentValidationEndpointSendLintLicenseAssignments1Port
{
    Task<ApiLedger.NormalizeLedgerAccountResponse> SendAsync(ApiLedger.NormalizeLedgerAccountRequest request, CancellationToken token);
}

public sealed class LicenseAssignmentValidationEndpointSendLintLicenseAssignments1PortAdapter(ApiLedger.LedgerAccountValidationService.LedgerAccountValidationServiceClient client) : ILicenseAssignmentValidationEndpointSendLintLicenseAssignments1Port
{
    public async Task<ApiLedger.NormalizeLedgerAccountResponse> SendAsync(ApiLedger.NormalizeLedgerAccountRequest request, CancellationToken token)
    {
        using var rpc = client.NormalizeLedgerAccountAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
