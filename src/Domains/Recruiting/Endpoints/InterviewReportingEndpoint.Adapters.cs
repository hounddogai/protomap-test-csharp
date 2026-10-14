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
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Recruiting;

/// <summary>
/// The application port for CreateEmployee.
/// </summary>
public interface IInterviewReportingEndpointSendGenerateInterviewReport0Port
{
    Task<ApiEmployees.CreateEmployeeResponse> SendAsync(ApiEmployees.CreateEmployeeRequest request, CancellationToken token);
}

public sealed class InterviewReportingEndpointSendGenerateInterviewReport0PortAdapter(ApiEmployees.EmployeeService.EmployeeServiceClient client) : IInterviewReportingEndpointSendGenerateInterviewReport0Port
{
    public async Task<ApiEmployees.CreateEmployeeResponse> SendAsync(ApiEmployees.CreateEmployeeRequest request, CancellationToken token)
    {
        using var rpc = client.CreateEmployeeAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class InterviewReportingEndpointRequests
{
    /// <summary>
    /// Builds the BrowseVenues request from the current workflow state.
    /// </summary>
    public static ApiVenues.BrowseVenuesRequest FetchInterviewReportToBrowseVenues0(this ApiRecruiting.FetchInterviewReportRequest request)
    {
        return new ApiVenues.BrowseVenuesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiVenues.BrowseVenuesRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
                Labels = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.InterviewId ?? ""),
            },
        };
    }
}
