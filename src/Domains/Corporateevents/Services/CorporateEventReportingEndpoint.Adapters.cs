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
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiModelregistry = Acme.Modelregistry.V1;

namespace Acme.Workforce.Corporateevents;

/// <summary>
/// The application port for RecalculateRegistrations.
/// </summary>
public interface ICorporateEventReportingEndpointSendListCorporateEventReports0Port
{
    Task<ApiCorporateevents.RecalculateRegistrationsResponse> SendAsync(ApiCorporateevents.RecalculateRegistrationsRequest request, CancellationToken token);
}

public sealed class CorporateEventReportingEndpointSendListCorporateEventReports0PortAdapter(ApiCorporateevents.RegistrationCalculationService.RegistrationCalculationServiceClient client) : ICorporateEventReportingEndpointSendListCorporateEventReports0Port
{
    public async Task<ApiCorporateevents.RecalculateRegistrationsResponse> SendAsync(ApiCorporateevents.RecalculateRegistrationsRequest request, CancellationToken token)
    {
        using var rpc = client.RecalculateRegistrationsAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
