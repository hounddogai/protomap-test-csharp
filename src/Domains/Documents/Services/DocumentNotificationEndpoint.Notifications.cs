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
using ApiAuthorization = Acme.Authorization.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;

namespace Acme.Workforce.Documents;

public sealed class DocumentNotificationEndpointSendDocumentTestNotice0NotificationHandler(ApiDocuments.DocumentExportService.DocumentExportServiceClient client) : INotificationHandler
{
    public string Topic => "acme.documents.v1.DocumentNotificationService/SendDocumentTestNotice/0";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiDocuments.ListDocumentExportsRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.ListDocumentExportsAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
