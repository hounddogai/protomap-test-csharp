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
using ApiBanking = Acme.Banking.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;

namespace Acme.Workforce.Helpdesk;

public sealed class KnowledgeArticleApprovalEndpointCommentKnowledgeArticleApproval3NotificationHandler(ApiDocuments.SignatureValidationService.SignatureValidationServiceClient client) : INotificationHandler
{
    public string Topic => "acme.helpdesk.v1.KnowledgeArticleApprovalService/CommentKnowledgeArticleApproval/3";
    public async Task HandleAsync(INotification notification, CancellationToken token)
    {
        if (notification is not TransportNotification<ApiDocuments.ValidateSignatureRequest> message)
            throw new InvalidOperationException("Unexpected notification contract");
        using var rpc = client.ValidateSignatureAsync(message.Request, cancellationToken: token);
        await rpc.ResponseAsync;
    }
}
