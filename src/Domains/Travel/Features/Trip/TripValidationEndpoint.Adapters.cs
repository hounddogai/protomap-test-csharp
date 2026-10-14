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
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

/// <summary>
/// The application port for CalculateManagedDevice.
/// </summary>
public interface ITripValidationEndpointSendNormalizeTrip0Port
{
    Task<ApiDevices.CalculateManagedDeviceResponse> SendAsync(ApiDevices.CalculateManagedDeviceRequest request, CancellationToken token);
}

public sealed class TripValidationEndpointSendNormalizeTrip0PortAdapter(ApiDevices.ManagedDeviceCalculationService.ManagedDeviceCalculationServiceClient client) : ITripValidationEndpointSendNormalizeTrip0Port
{
    public async Task<ApiDevices.CalculateManagedDeviceResponse> SendAsync(ApiDevices.CalculateManagedDeviceRequest request, CancellationToken token)
    {
        using var rpc = client.CalculateManagedDeviceAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class TripValidationEndpointRequests
{
    /// <summary>
    /// Builds the GetTripRates request from the current workflow state.
    /// </summary>
    public static ApiTravel.GetTripRatesRequest LintTripsToGetTripRates1(this ApiTravel.LintTripsRequest request)
    {
        return new ApiTravel.GetTripRatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            TripId = (request.Trip?.TripId ?? ""),
        };
    }
}
