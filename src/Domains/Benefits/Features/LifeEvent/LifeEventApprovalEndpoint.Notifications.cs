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
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiMessaging = Acme.Messaging.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSearch = Acme.Search.V1;
using ApiTelematics = Acme.Telematics.V1;

namespace Acme.Workforce.Benefits;

public sealed class LifeEventApprovalEndpointSubmitLifeEvent0NotificationHandler(ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient client) : INotificationHandler
{
    public string Topic => "acme.benefits.v1.LifeEventApprovalService/SubmitLifeEvent/0";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiRecruiting.CalculateJobOpeningRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.CalculateJobOpeningAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
