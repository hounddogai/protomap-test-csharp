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
using ApiIdentity = Acme.Identity.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiRouting = Acme.Routing.V1;
using ApiSecrets = Acme.Secrets.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Recruiting;

/// <summary>
/// The application port for CompareJobOpeningOptions.
/// </summary>
public interface IJobOpeningValidationEndpointSendLintJobOpenings1Port
{
    Task<ApiRecruiting.CompareJobOpeningOptionsResponse> SendAsync(ApiRecruiting.CompareJobOpeningOptionsRequest request, CancellationToken token);
}

public sealed class JobOpeningValidationEndpointSendLintJobOpenings1PortAdapter(ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient client) : IJobOpeningValidationEndpointSendLintJobOpenings1Port
{
    public async Task<ApiRecruiting.CompareJobOpeningOptionsResponse> SendAsync(ApiRecruiting.CompareJobOpeningOptionsRequest request, CancellationToken token)
    {
        using var rpc = client.CompareJobOpeningOptionsAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
