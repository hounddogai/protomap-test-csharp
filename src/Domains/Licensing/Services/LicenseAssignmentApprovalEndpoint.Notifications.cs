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
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSearch = Acme.Search.V1;
using ApiSellers = Acme.Sellers.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Licensing;

public sealed class LicenseAssignmentApprovalEndpointSubmitLicenseAssignment3NotificationHandler(ApiLicensing.LicenseAssignmentReportingService.LicenseAssignmentReportingServiceClient client) : INotificationHandler
{
    public string Topic => "acme.licensing.v1.LicenseAssignmentApprovalService/SubmitLicenseAssignment/3";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiLicensing.CountLicenseAssignmentTotalsRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.CountLicenseAssignmentTotalsAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
