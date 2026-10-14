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
using ApiBenefits = Acme.Benefits.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiSurveys = Acme.Surveys.V1;

namespace Acme.Workforce.Scheduling;

/// <summary>
/// The application port for ScoreAvailabilityWindow.
/// </summary>
public interface IRosterApprovalEndpointSendSubmitRoster3Port
{
    Task<ApiScheduling.ScoreAvailabilityWindowResponse> SendAsync(ApiScheduling.ScoreAvailabilityWindowRequest request, CancellationToken token);
}

public sealed class RosterApprovalEndpointSendSubmitRoster3PortAdapter(ApiScheduling.AvailabilityWindowValidationService.AvailabilityWindowValidationServiceClient client) : IRosterApprovalEndpointSendSubmitRoster3Port
{
    public async Task<ApiScheduling.ScoreAvailabilityWindowResponse> SendAsync(ApiScheduling.ScoreAvailabilityWindowRequest request, CancellationToken token)
    {
        using var rpc = client.ScoreAvailabilityWindowAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class RosterApprovalEndpointRequests
{
    /// <summary>
    /// Builds the BrowseOrgUnits request from the current workflow state.
    /// </summary>
    public static ApiEmployees.BrowseOrgUnitsRequest CommentRosterApprovalToBrowseOrgUnits0(this ApiScheduling.CommentRosterApprovalRequest request)
    {
        return new ApiEmployees.BrowseOrgUnitsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.BrowseOrgUnitsRequest.Types.Filter
            {
                CreatedAfter = (request.Roster?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (request.Roster?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Region = (request.Roster?.BillingAddress?.Region ?? ""),
            },
        };
    }
}
