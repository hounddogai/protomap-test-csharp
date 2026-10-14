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
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiSearch = Acme.Search.V1;
using ApiTours = Acme.Tours.V1;

namespace Acme.Workforce.Itsm;

/// <summary>
/// The application port for ExplainIncidentCalculation.
/// </summary>
public interface IIncidentValidationEndpointSendPreviewIncident0Port
{
    Task<ApiItsm.ExplainIncidentCalculationResponse> SendAsync(ApiItsm.ExplainIncidentCalculationRequest request, CancellationToken token);
}

public sealed class IncidentValidationEndpointSendPreviewIncident0PortAdapter(ApiItsm.IncidentCalculationService.IncidentCalculationServiceClient client) : IIncidentValidationEndpointSendPreviewIncident0Port
{
    public async Task<ApiItsm.ExplainIncidentCalculationResponse> SendAsync(ApiItsm.ExplainIncidentCalculationRequest request, CancellationToken token)
    {
        using var rpc = client.ExplainIncidentCalculationAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
