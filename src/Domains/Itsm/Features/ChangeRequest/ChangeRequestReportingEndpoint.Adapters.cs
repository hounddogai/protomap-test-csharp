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
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiMaintenance = Acme.Maintenance.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Itsm;

/// <summary>
/// The application port for GenerateLicenseAssignmentReport.
/// </summary>
public interface IChangeRequestReportingEndpointSendScheduleChangeRequestReport1Port
{
    Task<ApiLicensing.GenerateLicenseAssignmentReportResponse> SendAsync(ApiLicensing.GenerateLicenseAssignmentReportRequest request, CancellationToken token);
}

public sealed class ChangeRequestReportingEndpointSendScheduleChangeRequestReport1PortAdapter(ApiLicensing.LicenseAssignmentReportingService.LicenseAssignmentReportingServiceClient client) : IChangeRequestReportingEndpointSendScheduleChangeRequestReport1Port
{
    public async Task<ApiLicensing.GenerateLicenseAssignmentReportResponse> SendAsync(ApiLicensing.GenerateLicenseAssignmentReportRequest request, CancellationToken token)
    {
        using var rpc = client.GenerateLicenseAssignmentReportAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class ChangeRequestReportingEndpointRequests
{
    /// <summary>
    /// Builds the ListSignatureExports request from the current workflow state.
    /// </summary>
    public static ApiDocuments.ListSignatureExportsRequest ScheduleChangeRequestReportToListSignatureExports0(this ApiItsm.ScheduleChangeRequestReportRequest request)
    {
        return new ApiDocuments.ListSignatureExportsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListSignatureExportsRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
                Labels = { (request.Details?.Filters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.ChangeRequestId ?? ""),
            },
        };
    }
}
