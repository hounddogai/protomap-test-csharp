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

/// <summary>
/// The application port for SummarizeAccessPolicies.
/// </summary>
public interface IKnowledgeArticleApprovalEndpointSendSubmitKnowledgeArticle1Port
{
    Task<ApiAuthorization.SummarizeAccessPoliciesResponse> SendAsync(ApiAuthorization.SummarizeAccessPoliciesRequest request, CancellationToken token);
}

public sealed class KnowledgeArticleApprovalEndpointSendSubmitKnowledgeArticle1PortAdapter(ApiAuthorization.AccessPolicyQueryService.AccessPolicyQueryServiceClient client) : IKnowledgeArticleApprovalEndpointSendSubmitKnowledgeArticle1Port
{
    public async Task<ApiAuthorization.SummarizeAccessPoliciesResponse> SendAsync(ApiAuthorization.SummarizeAccessPoliciesRequest request, CancellationToken token)
    {
        using var rpc = client.SummarizeAccessPoliciesAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class KnowledgeArticleApprovalEndpointRequests
{
    /// <summary>
    /// Builds the DelegateIncidentApproval request from the current workflow state.
    /// </summary>
    public static ApiItsm.DelegateIncidentApprovalRequest SubmitKnowledgeArticleToDelegateIncidentApproval0(this ApiHelpdesk.SubmitKnowledgeArticleRequest request)
    {
        return new ApiItsm.DelegateIncidentApprovalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Incident = new ApiItsm.Incident
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                },
            },
        };
    }
}
