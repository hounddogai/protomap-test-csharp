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
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiPipelines = Acme.Pipelines.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTravel = Acme.Travel.V1;
using ApiYards = Acme.Yards.V1;

namespace Acme.Workforce.Travel;

/// <summary>
/// The application port for LintCorporateEvents.
/// </summary>
public interface ITripCalculationEndpointSendExplainTripCalculation3Port
{
    Task<ApiCorporateevents.LintCorporateEventsResponse> SendAsync(ApiCorporateevents.LintCorporateEventsRequest request, CancellationToken token);
}

public sealed class TripCalculationEndpointSendExplainTripCalculation3PortAdapter(ApiCorporateevents.CorporateEventValidationService.CorporateEventValidationServiceClient client) : ITripCalculationEndpointSendExplainTripCalculation3Port
{
    public async Task<ApiCorporateevents.LintCorporateEventsResponse> SendAsync(ApiCorporateevents.LintCorporateEventsRequest request, CancellationToken token)
    {
        using var rpc = client.LintCorporateEventsAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
