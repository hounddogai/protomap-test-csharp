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
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Recruiting;

public sealed class OfferReportingEndpointStreamOfferMetrics2NotificationHandler(ApiCorporateevents.VenueApprovalService.VenueApprovalServiceClient client) : INotificationHandler
{
    public string Topic => "acme.recruiting.v1.OfferReportingService/StreamOfferMetrics/2";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiCorporateevents.CommentVenueApprovalRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.CommentVenueApprovalAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
