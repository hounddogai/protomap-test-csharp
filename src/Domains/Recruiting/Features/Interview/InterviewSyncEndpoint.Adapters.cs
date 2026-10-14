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
using ApiAuthorization = Acme.Authorization.V1;
using ApiCommon = Acme.Common.V1;
using ApiDeliveries = Acme.Deliveries.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiGiftcards = Acme.Giftcards.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Recruiting;

/// <summary>
/// The application port for CreateEmployee.
/// </summary>
public interface IInterviewSyncEndpointSendPushInterview0Port
{
    Task<ApiEmployees.CreateEmployeeResponse> SendAsync(ApiEmployees.CreateEmployeeRequest request, CancellationToken token);
}

public sealed class InterviewSyncEndpointSendPushInterview0PortAdapter(ApiEmployees.EmployeeService.EmployeeServiceClient client) : IInterviewSyncEndpointSendPushInterview0Port
{
    public async Task<ApiEmployees.CreateEmployeeResponse> SendAsync(ApiEmployees.CreateEmployeeRequest request, CancellationToken token)
    {
        using var rpc = client.CreateEmployeeAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class InterviewSyncEndpointRequests
{
    /// <summary>
    /// Builds the SearchDeliveries request from the current workflow state.
    /// </summary>
    public static ApiDeliveries.SearchDeliveriesRequest PullInterviewsToSearchDeliveries2(this ApiRecruiting.PullInterviewsRequest request, ApiWorkplaces.StartDeskWorkflowResponse reply0, ApiAuthorization.CloneAccessPolicyResponse reply1)
    {
        return new ApiDeliveries.SearchDeliveriesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDeliveries.SearchDeliveriesRequest.Types.Filter
            {
                CreatedAfter = (reply1.AccessPolicy?.Audit?.UpdatedAt ?? new Timestamp()),
                OwnerId = (reply0.Desk?.DeskId ?? ""),
                Region = (request.Interview?.Location?.Region ?? ""),
            },
        };
    }
}
