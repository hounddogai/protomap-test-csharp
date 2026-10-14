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
using ApiFuel = Acme.Fuel.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiSearch = Acme.Search.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Helpdesk;

/// <summary>
/// The application port for RecalculateKnowledgeArticles.
/// </summary>
public interface IEscalationCalculationEndpointSendQuoteEscalation3Port
{
    Task<ApiHelpdesk.RecalculateKnowledgeArticlesResponse> SendAsync(ApiHelpdesk.RecalculateKnowledgeArticlesRequest request, CancellationToken token);
}

public sealed class EscalationCalculationEndpointSendQuoteEscalation3PortAdapter(ApiHelpdesk.KnowledgeArticleCalculationService.KnowledgeArticleCalculationServiceClient client) : IEscalationCalculationEndpointSendQuoteEscalation3Port
{
    public async Task<ApiHelpdesk.RecalculateKnowledgeArticlesResponse> SendAsync(ApiHelpdesk.RecalculateKnowledgeArticlesRequest request, CancellationToken token)
    {
        using var rpc = client.RecalculateKnowledgeArticlesAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class EscalationCalculationEndpointRequests
{
    /// <summary>
    /// Builds the PreviewServiceAsset request from the current workflow state.
    /// </summary>
    public static ApiItsm.PreviewServiceAssetRequest QuoteEscalationToPreviewServiceAsset2(this ApiHelpdesk.QuoteEscalationRequest request, ApiHelpdesk.ValidateTicketResponse reply0)
    {
        return new ApiItsm.PreviewServiceAssetRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Address = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Details?.Destination?.PostalCode ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Escalation?.Audit?.Etag ?? ""),
                    UpdatedBy = (request.Escalation?.Audit?.UpdatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Ticket?.Assignee?.DisplayName ?? ""),
                },
            },
        };
    }
}
