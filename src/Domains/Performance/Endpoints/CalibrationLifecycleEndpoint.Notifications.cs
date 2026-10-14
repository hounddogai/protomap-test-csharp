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
using ApiCatalog = Acme.Catalog.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiCustomers = Acme.Customers.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecommendations = Acme.Recommendations.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Performance;

public sealed class CalibrationLifecycleEndpointExpireCalibration0NotificationHandler(ApiRecruiting.CandidateCalculationService.CandidateCalculationServiceClient client) : INotificationHandler
{
    public string Topic => "acme.performance.v1.CalibrationLifecycleService/ExpireCalibration/0";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiRecruiting.CalculateCandidateRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.CalculateCandidateAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
