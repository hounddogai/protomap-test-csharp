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
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiTravel = Acme.Travel.V1;
using ApiTreasury = Acme.Treasury.V1;

namespace Acme.Workforce.Timekeeping;

/// <summary>
/// The application port for CountClockEvents.
/// </summary>
public interface ITimesheetWorkflowEndpointSendStartTimesheetWorkflow1Port
{
    Task<ApiTimekeeping.CountClockEventsResponse> SendAsync(ApiTimekeeping.CountClockEventsRequest request, CancellationToken token);
}

public sealed class TimesheetWorkflowEndpointSendStartTimesheetWorkflow1PortAdapter(ApiTimekeeping.ClockEventQueryService.ClockEventQueryServiceClient client) : ITimesheetWorkflowEndpointSendStartTimesheetWorkflow1Port
{
    public async Task<ApiTimekeeping.CountClockEventsResponse> SendAsync(ApiTimekeeping.CountClockEventsRequest request, CancellationToken token)
    {
        using var rpc = client.CountClockEventsAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class TimesheetWorkflowEndpointRequests
{
    /// <summary>
    /// Builds the PreviewTimesheet request from the current workflow state.
    /// </summary>
    public static ApiTimekeeping.PreviewTimesheetRequest StartTimesheetWorkflowToPreviewTimesheet0(this ApiTimekeeping.StartTimesheetWorkflowRequest request)
    {
        return new ApiTimekeeping.PreviewTimesheetRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Timesheet = new ApiTimekeeping.Timesheet
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (request.Timesheet?.BillingAddress?.Locality ?? ""),
                },
            },
        };
    }
}
