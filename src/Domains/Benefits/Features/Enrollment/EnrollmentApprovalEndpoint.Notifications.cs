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
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSearch = Acme.Search.V1;

namespace Acme.Workforce.Benefits;

public sealed class EnrollmentApprovalEndpointSubmitEnrollment0NotificationHandler(ApiBenefits.LifeEventCalculationService.LifeEventCalculationServiceClient client) : INotificationHandler
{
    public string Topic => "acme.benefits.v1.EnrollmentApprovalService/SubmitEnrollment/0";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiBenefits.EstimateLifeEventRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.EstimateLifeEventAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
