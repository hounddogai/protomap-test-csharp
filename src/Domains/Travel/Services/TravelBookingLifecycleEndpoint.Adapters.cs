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
using ApiEmployees = Acme.Employees.V1;
using ApiFeaturestore = Acme.Featurestore.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

/// <summary>
/// The application port for ApproveLicenseAssignment.
/// </summary>
public interface ITravelBookingLifecycleEndpointSendReinstateTravelBooking0Port
{
    Task<ApiLicensing.ApproveLicenseAssignmentResponse> SendAsync(ApiLicensing.ApproveLicenseAssignmentRequest request, CancellationToken token);
}

public sealed class TravelBookingLifecycleEndpointSendReinstateTravelBooking0PortAdapter(ApiLicensing.LicenseAssignmentApprovalService.LicenseAssignmentApprovalServiceClient client) : ITravelBookingLifecycleEndpointSendReinstateTravelBooking0Port
{
    public async Task<ApiLicensing.ApproveLicenseAssignmentResponse> SendAsync(ApiLicensing.ApproveLicenseAssignmentRequest request, CancellationToken token)
    {
        using var rpc = client.ApproveLicenseAssignmentAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class TravelBookingLifecycleEndpointRequests
{
    /// <summary>
    /// Builds the ScoreTrip request from the current workflow state.
    /// </summary>
    public static ApiTravel.ScoreTripRequest GetTravelBookingLifecycleToScoreTrip0(this ApiTravel.GetTravelBookingLifecycleRequest request)
    {
        return new ApiTravel.ScoreTripRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Trip = new ApiTravel.Trip
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Lines = { new ApiTravel.TripLine
                {
                    Attributes = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                } },
            },
        };
    }
}
