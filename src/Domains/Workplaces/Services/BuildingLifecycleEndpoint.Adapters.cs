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
using ApiAuthorization = Acme.Authorization.V1;
using ApiCommon = Acme.Common.V1;
using ApiCustomers = Acme.Customers.V1;
using ApiDevices = Acme.Devices.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFraud = Acme.Fraud.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiKeymanagement = Acme.Keymanagement.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

/// <summary>
/// The application port for LintBuildings.
/// </summary>
public interface IBuildingLifecycleEndpointSendSuspendBuilding1Port
{
    Task<ApiWorkplaces.LintBuildingsResponse> SendAsync(ApiWorkplaces.LintBuildingsRequest request, CancellationToken token);
}

public sealed class BuildingLifecycleEndpointSendSuspendBuilding1PortAdapter(ApiWorkplaces.BuildingValidationService.BuildingValidationServiceClient client) : IBuildingLifecycleEndpointSendSuspendBuilding1Port
{
    public async Task<ApiWorkplaces.LintBuildingsResponse> SendAsync(ApiWorkplaces.LintBuildingsRequest request, CancellationToken token)
    {
        using var rpc = client.LintBuildingsAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class BuildingLifecycleEndpointRequests
{
    /// <summary>
    /// Builds the RejectTravelBooking request from the current workflow state.
    /// </summary>
    public static ApiTravel.RejectTravelBookingRequest SuspendBuildingToRejectTravelBooking0(this ApiWorkplaces.SuspendBuildingRequest request)
    {
        return new ApiTravel.RejectTravelBookingRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.RejectTravelBookingRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                },
            },
            TravelBooking = new ApiTravel.TravelBooking
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                },
            },
        };
    }
}
