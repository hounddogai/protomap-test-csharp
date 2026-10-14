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
using ApiEmployees = Acme.Employees.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLearning = Acme.Learning.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiPrescriptions = Acme.Prescriptions.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Benefits;

/// <summary>
/// The application port for SimulateTravelBooking.
/// </summary>
public interface ILifeEventCalculationEndpointSendRecalculateLifeEvents0Port
{
    Task<ApiTravel.SimulateTravelBookingResponse> SendAsync(ApiTravel.SimulateTravelBookingRequest request, CancellationToken token);
}

public sealed class LifeEventCalculationEndpointSendRecalculateLifeEvents0PortAdapter(ApiTravel.TravelBookingCalculationService.TravelBookingCalculationServiceClient client) : ILifeEventCalculationEndpointSendRecalculateLifeEvents0Port
{
    public async Task<ApiTravel.SimulateTravelBookingResponse> SendAsync(ApiTravel.SimulateTravelBookingRequest request, CancellationToken token)
    {
        using var rpc = client.SimulateTravelBookingAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class LifeEventCalculationEndpointRequests
{
    /// <summary>
    /// Builds the BatchGetPrescriptions request from the current workflow state.
    /// </summary>
    public static ApiPrescriptions.BatchGetPrescriptionsRequest QuoteLifeEventToBatchGetPrescriptions0(this ApiBenefits.QuoteLifeEventRequest request)
    {
        return new ApiPrescriptions.BatchGetPrescriptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
    }
}
