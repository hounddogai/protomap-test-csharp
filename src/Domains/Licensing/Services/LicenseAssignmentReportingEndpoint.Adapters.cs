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
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPayments = Acme.Payments.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSurveys = Acme.Surveys.V1;

namespace Acme.Workforce.Licensing;

/// <summary>
/// The application port for FindEntitlements.
/// </summary>
public interface ILicenseAssignmentReportingEndpointSendFetchLicenseAssignmentReport0Port
{
    Task<ApiLicensing.FindEntitlementsResponse> SendAsync(ApiLicensing.FindEntitlementsRequest request, CancellationToken token);
}

public sealed class LicenseAssignmentReportingEndpointSendFetchLicenseAssignmentReport0PortAdapter(ApiLicensing.EntitlementQueryService.EntitlementQueryServiceClient client) : ILicenseAssignmentReportingEndpointSendFetchLicenseAssignmentReport0Port
{
    public async Task<ApiLicensing.FindEntitlementsResponse> SendAsync(ApiLicensing.FindEntitlementsRequest request, CancellationToken token)
    {
        using var rpc = client.FindEntitlementsAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class LicenseAssignmentReportingEndpointRequests
{
    /// <summary>
    /// Builds the ResolveSoftwareLicenses request from the current workflow state.
    /// </summary>
    public static ApiLicensing.ResolveSoftwareLicensesRequest DeleteLicenseAssignmentReportToResolveSoftwareLicenses3(this ApiLicensing.DeleteLicenseAssignmentReportRequest request)
    {
        return new ApiLicensing.ResolveSoftwareLicensesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
    }
}
