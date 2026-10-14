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
using ApiEmployees = Acme.Employees.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiKeymanagement = Acme.Keymanagement.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPipelines = Acme.Pipelines.V1;

namespace Acme.Workforce.Helpdesk;

/// <summary>
/// The application port for ComparePipelineOptions.
/// </summary>
public interface ITicketValidationEndpointSendCheckTicketEligibility1Port
{
    Task<ApiPipelines.ComparePipelineOptionsResponse> SendAsync(ApiPipelines.ComparePipelineOptionsRequest request, CancellationToken token);
}

public sealed class TicketValidationEndpointSendCheckTicketEligibility1PortAdapter(ApiPipelines.PipelineCalculationService.PipelineCalculationServiceClient client) : ITicketValidationEndpointSendCheckTicketEligibility1Port
{
    public async Task<ApiPipelines.ComparePipelineOptionsResponse> SendAsync(ApiPipelines.ComparePipelineOptionsRequest request, CancellationToken token)
    {
        using var rpc = client.ComparePipelineOptionsAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
