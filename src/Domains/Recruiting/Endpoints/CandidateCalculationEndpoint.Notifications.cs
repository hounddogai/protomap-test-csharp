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
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiShows = Acme.Shows.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Recruiting;

public sealed class CandidateCalculationEndpointSimulateCandidate0NotificationHandler(ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient client) : INotificationHandler
{
    public string Topic => "acme.recruiting.v1.CandidateCalculationService/SimulateCandidate/0";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiRecruiting.SimulateJobOpeningRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.SimulateJobOpeningAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
