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
using ApiScheduling = Acme.Scheduling.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Scheduling;

public sealed class AvailabilityWindowApprovalEndpointSubmitAvailabilityWindow2NotificationHandler(ApiDocuments.DocumentTemplateWorkflowService.DocumentTemplateWorkflowServiceClient client) : INotificationHandler
{
    public string Topic => "acme.scheduling.v1.AvailabilityWindowApprovalService/SubmitAvailabilityWindow/2";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiDocuments.StartDocumentTemplateWorkflowRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.StartDocumentTemplateWorkflowAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
