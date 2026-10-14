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
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiSubscriptions = Acme.Subscriptions.V1;
using ApiVenues = Acme.Venues.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Employees;

/// <summary>
/// The application port for DelegateKnowledgeArticleApproval.
/// </summary>
public interface IPositionApprovalEndpointSendRejectPosition0Port
{
    Task<ApiHelpdesk.DelegateKnowledgeArticleApprovalResponse> SendAsync(ApiHelpdesk.DelegateKnowledgeArticleApprovalRequest request, CancellationToken token);
}

public sealed class PositionApprovalEndpointSendRejectPosition0PortAdapter(ApiHelpdesk.KnowledgeArticleApprovalService.KnowledgeArticleApprovalServiceClient client) : IPositionApprovalEndpointSendRejectPosition0Port
{
    public async Task<ApiHelpdesk.DelegateKnowledgeArticleApprovalResponse> SendAsync(ApiHelpdesk.DelegateKnowledgeArticleApprovalRequest request, CancellationToken token)
    {
        using var rpc = client.DelegateKnowledgeArticleApprovalAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
