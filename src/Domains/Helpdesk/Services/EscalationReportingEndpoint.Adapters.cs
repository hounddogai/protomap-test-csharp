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
using ApiFuel = Acme.Fuel.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiSurveys = Acme.Surveys.V1;

namespace Acme.Workforce.Helpdesk;

/// <summary>
/// The application port for BrowseMetricSerieses.
/// </summary>
public interface IEscalationReportingEndpointSendShareEscalationReport2Port
{
    Task<ApiMetrics.BrowseMetricSeriesesResponse> SendAsync(ApiMetrics.BrowseMetricSeriesesRequest request, CancellationToken token);
}

public sealed class EscalationReportingEndpointSendShareEscalationReport2PortAdapter(ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient client) : IEscalationReportingEndpointSendShareEscalationReport2Port
{
    public async Task<ApiMetrics.BrowseMetricSeriesesResponse> SendAsync(ApiMetrics.BrowseMetricSeriesesRequest request, CancellationToken token)
    {
        using var rpc = client.BrowseMetricSeriesesAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class EscalationReportingEndpointRequests
{
    /// <summary>
    /// Builds the CloneTicket request from the current workflow state.
    /// </summary>
    public static ApiHelpdesk.CloneTicketRequest ShareEscalationReportToCloneTicket1(this ApiHelpdesk.ShareEscalationReportRequest request)
    {
        return new ApiHelpdesk.CloneTicketRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Ticket = new ApiHelpdesk.Ticket
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Recipient?.Email ?? ""),
                },
                Channel = new ApiHelpdesk.TicketChannelOrigin
                {
                    Channel = (request.Details?.Channel ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.Recipient?.Phone ?? ""),
                    PreferredChannel = (request.Details?.Recipient?.PreferredChannel ?? ""),
                },
            },
        };
    }
}
