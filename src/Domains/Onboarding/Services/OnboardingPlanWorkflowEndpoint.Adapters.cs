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
using ApiBenefits = Acme.Benefits.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiLineage = Acme.Lineage.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiRouting = Acme.Routing.V1;
using ApiSearch = Acme.Search.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Onboarding;

/// <summary>
/// The application port for SummarizeVenues.
/// </summary>
public interface IOnboardingPlanWorkflowEndpointSendRetryOnboardingPlanWorkflow1Port
{
    Task<ApiVenues.SummarizeVenuesResponse> SendAsync(ApiVenues.SummarizeVenuesRequest request, CancellationToken token);
}

public sealed class OnboardingPlanWorkflowEndpointSendRetryOnboardingPlanWorkflow1PortAdapter(ApiVenues.VenueQueryService.VenueQueryServiceClient client) : IOnboardingPlanWorkflowEndpointSendRetryOnboardingPlanWorkflow1Port
{
    public async Task<ApiVenues.SummarizeVenuesResponse> SendAsync(ApiVenues.SummarizeVenuesRequest request, CancellationToken token)
    {
        using var rpc = client.SummarizeVenuesAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
