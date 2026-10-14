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
using ApiAuthorization = Acme.Authorization.V1;
using ApiBenefits = Acme.Benefits.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiWebhooks = Acme.Webhooks.V1;

namespace Acme.Workforce.Benefits;

/// <summary>
/// The application port for SummarizeWebhookEndpoints.
/// </summary>
public interface IDependentValidationEndpointSendScoreDependent0Port
{
    Task<ApiWebhooks.SummarizeWebhookEndpointsResponse> SendAsync(ApiWebhooks.SummarizeWebhookEndpointsRequest request, CancellationToken token);
}

public sealed class DependentValidationEndpointSendScoreDependent0PortAdapter(ApiWebhooks.WebhookEndpointQueryService.WebhookEndpointQueryServiceClient client) : IDependentValidationEndpointSendScoreDependent0Port
{
    public async Task<ApiWebhooks.SummarizeWebhookEndpointsResponse> SendAsync(ApiWebhooks.SummarizeWebhookEndpointsRequest request, CancellationToken token)
    {
        using var rpc = client.SummarizeWebhookEndpointsAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
