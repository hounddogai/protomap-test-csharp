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
using ApiAnomalies = Acme.Anomalies.V1;
using ApiAuthorization = Acme.Authorization.V1;
using ApiCareplans = Acme.Careplans.V1;
using ApiCommon = Acme.Common.V1;
using ApiConcessions = Acme.Concessions.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLearning = Acme.Learning.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiMedia = Acme.Media.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Learning;

/// <summary>
/// The application port for ValidateDesk.
/// </summary>
public interface ILearningPathCalculationEndpointSendGetLearningPathRates0Port
{
    Task<ApiWorkplaces.ValidateDeskResponse> SendAsync(ApiWorkplaces.ValidateDeskRequest request, CancellationToken token);
}

public sealed class LearningPathCalculationEndpointSendGetLearningPathRates0PortAdapter(ApiWorkplaces.DeskValidationService.DeskValidationServiceClient client) : ILearningPathCalculationEndpointSendGetLearningPathRates0Port
{
    public async Task<ApiWorkplaces.ValidateDeskResponse> SendAsync(ApiWorkplaces.ValidateDeskRequest request, CancellationToken token)
    {
        using var rpc = client.ValidateDeskAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
