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
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiSellers = Acme.Sellers.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

public sealed class TravelPolicyReportingEndpointFetchTravelPolicyReport0NotificationHandler(ApiEmployees.PositionValidationService.PositionValidationServiceClient client) : INotificationHandler
{
    public string Topic => "acme.travel.v1.TravelPolicyReportingService/FetchTravelPolicyReport/0";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiEmployees.ValidatePositionRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.ValidatePositionAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
