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
using ApiAppointments = Acme.Appointments.V1;
using ApiCareplans = Acme.Careplans.V1;
using ApiCommon = Acme.Common.V1;
using ApiCustomers = Acme.Customers.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiReturns = Acme.Returns.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

/// <summary>
/// The application port for ScoreUser.
/// </summary>
public interface ITravelPolicySyncEndpointSendReconcileTravelPolicies0Port
{
    Task<ApiIdentity.ScoreUserResponse> SendAsync(ApiIdentity.ScoreUserRequest request, CancellationToken token);
}

public sealed class TravelPolicySyncEndpointSendReconcileTravelPolicies0PortAdapter(ApiIdentity.UserValidationService.UserValidationServiceClient client) : ITravelPolicySyncEndpointSendReconcileTravelPolicies0Port
{
    public async Task<ApiIdentity.ScoreUserResponse> SendAsync(ApiIdentity.ScoreUserRequest request, CancellationToken token)
    {
        using var rpc = client.ScoreUserAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class TravelPolicySyncEndpointRequests
{
    /// <summary>
    /// Builds the ExplainEmploymentContractCalculation request from the current workflow state.
    /// </summary>
    public static ApiEmployees.ExplainEmploymentContractCalculationRequest PushTravelPolicyToExplainEmploymentContractCalculation2(this ApiTravel.PushTravelPolicyRequest request, ApiTravel.SimulateTripResponse reply0, ApiNotifications.ScoreNotificationResponse reply1)
    {
        return new ApiEmployees.ExplainEmploymentContractCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.Notification?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (reply0.Trip?.Counterparty?.Email ?? ""),
                },
            },
        };
    }
}
