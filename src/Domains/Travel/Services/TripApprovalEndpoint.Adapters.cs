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
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPerformers = Acme.Performers.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

/// <summary>
/// The application port for SuggestEmployees.
/// </summary>
public interface ITripApprovalEndpointSendRecallTrip0Port
{
    Task<ApiEmployees.SuggestEmployeesResponse> SendAsync(ApiEmployees.SuggestEmployeesRequest request, CancellationToken token);
}

public sealed class TripApprovalEndpointSendRecallTrip0PortAdapter(ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient client) : ITripApprovalEndpointSendRecallTrip0Port
{
    public async Task<ApiEmployees.SuggestEmployeesResponse> SendAsync(ApiEmployees.SuggestEmployeesRequest request, CancellationToken token)
    {
        using var rpc = client.SuggestEmployeesAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class TripApprovalEndpointRequests
{
    /// <summary>
    /// Builds the SummarizeEmployees request from the current workflow state.
    /// </summary>
    public static ApiEmployees.SummarizeEmployeesRequest ListPendingTripsToSummarizeEmployees0(this ApiTravel.ListPendingTripsRequest request)
    {
        return new ApiEmployees.SummarizeEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.SummarizeEmployeesRequest.Types.Details
            {
                Filters = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
    }
}
