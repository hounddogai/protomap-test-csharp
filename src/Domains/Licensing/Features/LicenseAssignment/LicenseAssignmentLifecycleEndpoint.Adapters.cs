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
using ApiFleet = Acme.Fleet.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiInsurance = Acme.Insurance.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Licensing;

/// <summary>
/// The application port for DeleteLicenseAssignmentReport.
/// </summary>
public interface ILicenseAssignmentLifecycleEndpointSendReinstateLicenseAssignment0Port
{
    Task<ApiLicensing.DeleteLicenseAssignmentReportResponse> SendAsync(ApiLicensing.DeleteLicenseAssignmentReportRequest request, CancellationToken token);
}

public sealed class LicenseAssignmentLifecycleEndpointSendReinstateLicenseAssignment0PortAdapter(ApiLicensing.LicenseAssignmentReportingService.LicenseAssignmentReportingServiceClient client) : ILicenseAssignmentLifecycleEndpointSendReinstateLicenseAssignment0Port
{
    public async Task<ApiLicensing.DeleteLicenseAssignmentReportResponse> SendAsync(ApiLicensing.DeleteLicenseAssignmentReportRequest request, CancellationToken token)
    {
        using var rpc = client.DeleteLicenseAssignmentReportAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class LicenseAssignmentLifecycleEndpointRequests
{
    /// <summary>
    /// Builds the CountLicenseAssignmentTotals request from the current workflow state.
    /// </summary>
    public static ApiLicensing.CountLicenseAssignmentTotalsRequest GetLicenseAssignmentLifecycleToCountLicenseAssignmentTotals0(this ApiLicensing.GetLicenseAssignmentLifecycleRequest request)
    {
        return new ApiLicensing.CountLicenseAssignmentTotalsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLicensing.CountLicenseAssignmentTotalsRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
                Labels = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.LicenseAssignmentId ?? ""),
            },
        };
    }
}
