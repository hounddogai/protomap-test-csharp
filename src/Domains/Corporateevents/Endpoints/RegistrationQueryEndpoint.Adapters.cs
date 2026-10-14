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
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDatasets = Acme.Datasets.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;

namespace Acme.Workforce.Corporateevents;

/// <summary>
/// The application port for ExplainRegistrationDecision.
/// </summary>
public interface IRegistrationQueryEndpointSendPeekRegistration0Port
{
    Task<ApiCorporateevents.ExplainRegistrationDecisionResponse> SendAsync(ApiCorporateevents.ExplainRegistrationDecisionRequest request, CancellationToken token);
}

public sealed class RegistrationQueryEndpointSendPeekRegistration0PortAdapter(ApiCorporateevents.RegistrationValidationService.RegistrationValidationServiceClient client) : IRegistrationQueryEndpointSendPeekRegistration0Port
{
    public async Task<ApiCorporateevents.ExplainRegistrationDecisionResponse> SendAsync(ApiCorporateevents.ExplainRegistrationDecisionRequest request, CancellationToken token)
    {
        using var rpc = client.ExplainRegistrationDecisionAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
