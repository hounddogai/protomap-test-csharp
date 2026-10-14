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
using ApiBudgeting = Acme.Budgeting.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiNotifications = Acme.Notifications.V1;

namespace Acme.Workforce.Itsm;

/// <summary>
/// The application port for RecalculateCorporateEvents.
/// </summary>
public interface IIncidentApprovalEndpointSendRecallIncident0Port
{
    Task<ApiCorporateevents.RecalculateCorporateEventsResponse> SendAsync(ApiCorporateevents.RecalculateCorporateEventsRequest request, CancellationToken token);
}

public sealed class IncidentApprovalEndpointSendRecallIncident0PortAdapter(ApiCorporateevents.CorporateEventCalculationService.CorporateEventCalculationServiceClient client) : IIncidentApprovalEndpointSendRecallIncident0Port
{
    public async Task<ApiCorporateevents.RecalculateCorporateEventsResponse> SendAsync(ApiCorporateevents.RecalculateCorporateEventsRequest request, CancellationToken token)
    {
        using var rpc = client.RecalculateCorporateEventsAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class IncidentApprovalEndpointRequests
{
    /// <summary>
    /// Builds the GenerateProblemReport request from the current workflow state.
    /// </summary>
    public static ApiItsm.GenerateProblemReportRequest ListPendingIncidentsToGenerateProblemReport0(this ApiItsm.ListPendingIncidentsRequest request)
    {
        return new ApiItsm.GenerateProblemReportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.GenerateProblemReportRequest.Types.Details
            {
                Filters = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
    }
}
