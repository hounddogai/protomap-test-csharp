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
using ApiCart = Acme.Cart.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiOrders = Acme.Orders.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Performance;

public sealed class ReviewWorkflowEndpointListReviewWorkflows0NotificationHandler(ApiRecruiting.JobOpeningQueryService.JobOpeningQueryServiceClient client) : INotificationHandler
{
    public string Topic => "acme.performance.v1.ReviewWorkflowService/ListReviewWorkflows/0";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiRecruiting.SuggestJobOpeningsRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.SuggestJobOpeningsAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
