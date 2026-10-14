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
using ApiIdentity = Acme.Identity.V1;
using ApiInvoicing = Acme.Invoicing.V1;
using ApiLearning = Acme.Learning.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Learning;

public sealed class CertificationWorkflowEndpointSignalCertificationWorkflow0NotificationHandler(ApiDocuments.DocumentFolderQueryService.DocumentFolderQueryServiceClient client) : INotificationHandler
{
    public string Topic => "acme.learning.v1.CertificationWorkflowService/SignalCertificationWorkflow/0";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiDocuments.BrowseDocumentFoldersRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.BrowseDocumentFoldersAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
