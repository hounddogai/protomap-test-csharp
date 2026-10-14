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
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;

namespace Acme.Workforce.Employeeportal;

public sealed class PortalHomeEndpointClonePortalHome0NotificationHandler(ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient client) : INotificationHandler
{
    public string Topic => "acme.employeeportal.v1.PortalHomeService/ClonePortalHome/0";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiDocuments.ResolveDocumentTemplateConflictRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.ResolveDocumentTemplateConflictAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
