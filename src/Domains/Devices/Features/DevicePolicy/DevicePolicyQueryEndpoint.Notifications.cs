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
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiRatelimiting = Acme.Ratelimiting.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSearch = Acme.Search.V1;
using ApiSeating = Acme.Seating.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Devices;

public sealed class DevicePolicyQueryEndpointSummarizeDevicePolicies1NotificationHandler(ApiDevices.ComplianceScanValidationService.ComplianceScanValidationServiceClient client) : INotificationHandler
{
    public string Topic => "acme.devices.v1.DevicePolicyQueryService/SummarizeDevicePolicies/1";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiDevices.LintComplianceScansRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.LintComplianceScansAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
