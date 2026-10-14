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
using ApiAudit = Acme.Audit.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPayroll = Acme.Payroll.V1;

namespace Acme.Workforce.Itsm;

public sealed class ChangeRequestWorkflowEndpointPauseChangeRequestWorkflow1NotificationHandler(ApiDocuments.DocumentExportService.DocumentExportServiceClient client) : INotificationHandler
{
    public string Topic => "acme.itsm.v1.ChangeRequestWorkflowService/PauseChangeRequestWorkflow/1";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiDocuments.RetryDocumentExportRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.RetryDocumentExportAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
