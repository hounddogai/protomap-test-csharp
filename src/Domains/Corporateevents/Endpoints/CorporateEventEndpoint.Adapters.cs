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
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiMetrics = Acme.Metrics.V1;

namespace Acme.Workforce.Corporateevents;

/// <summary>
/// The application port for CountRegistrations.
/// </summary>
public interface ICorporateEventEndpointSendCreateCorporateEvent0Port
{
    Task<ApiCorporateevents.CountRegistrationsResponse> SendAsync(ApiCorporateevents.CountRegistrationsRequest request, CancellationToken token);
}

public sealed class CorporateEventEndpointSendCreateCorporateEvent0PortAdapter(ApiCorporateevents.RegistrationQueryService.RegistrationQueryServiceClient client) : ICorporateEventEndpointSendCreateCorporateEvent0Port
{
    public async Task<ApiCorporateevents.CountRegistrationsResponse> SendAsync(ApiCorporateevents.CountRegistrationsRequest request, CancellationToken token)
    {
        using var rpc = client.CountRegistrationsAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
