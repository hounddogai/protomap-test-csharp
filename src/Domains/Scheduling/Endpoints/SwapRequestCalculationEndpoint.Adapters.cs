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
using ApiLicensing = Acme.Licensing.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Scheduling;

/// <summary>
/// The application port for ValidateEntitlement.
/// </summary>
public interface ISwapRequestCalculationEndpointSendCalculateSwapRequest0Port
{
    Task<ApiLicensing.ValidateEntitlementResponse> SendAsync(ApiLicensing.ValidateEntitlementRequest request, CancellationToken token);
}

public sealed class SwapRequestCalculationEndpointSendCalculateSwapRequest0PortAdapter(ApiLicensing.EntitlementValidationService.EntitlementValidationServiceClient client) : ISwapRequestCalculationEndpointSendCalculateSwapRequest0Port
{
    public async Task<ApiLicensing.ValidateEntitlementResponse> SendAsync(ApiLicensing.ValidateEntitlementRequest request, CancellationToken token)
    {
        using var rpc = client.ValidateEntitlementAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class SwapRequestCalculationEndpointRequests
{
    /// <summary>
    /// Builds the RecalculateRosters request from the current workflow state.
    /// </summary>
    public static ApiScheduling.RecalculateRostersRequest RecalculateSwapRequestsToRecalculateRosters3(this ApiScheduling.RecalculateSwapRequestsRequest request, ApiScheduling.EstimateRosterResponse reply2)
    {
        return new ApiScheduling.RecalculateRostersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Roster = new ApiScheduling.Roster
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (reply2.Roster?.BillingAddress?.Locality ?? ""),
                },
            },
        };
    }
}
