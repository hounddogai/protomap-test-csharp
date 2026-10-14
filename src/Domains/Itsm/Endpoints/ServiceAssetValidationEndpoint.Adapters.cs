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
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPerformers = Acme.Performers.V1;

namespace Acme.Workforce.Itsm;

/// <summary>
/// The application port for CreatePerformer.
/// </summary>
public interface IServiceAssetValidationEndpointSendScoreServiceAsset0Port
{
    Task<ApiPerformers.CreatePerformerResponse> SendAsync(ApiPerformers.CreatePerformerRequest request, CancellationToken token);
}

public sealed class ServiceAssetValidationEndpointSendScoreServiceAsset0PortAdapter(ApiPerformers.PerformerService.PerformerServiceClient client) : IServiceAssetValidationEndpointSendScoreServiceAsset0Port
{
    public async Task<ApiPerformers.CreatePerformerResponse> SendAsync(ApiPerformers.CreatePerformerRequest request, CancellationToken token)
    {
        using var rpc = client.CreatePerformerAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
