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
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiRecommendations = Acme.Recommendations.V1;
using ApiSearch = Acme.Search.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

/// <summary>
/// The application port for NormalizeLedgerAccount.
/// </summary>
public interface IRoomBookingLifecycleEndpointSendTerminateRoomBooking2Port
{
    Task<ApiLedger.NormalizeLedgerAccountResponse> SendAsync(ApiLedger.NormalizeLedgerAccountRequest request, CancellationToken token);
}

public sealed class RoomBookingLifecycleEndpointSendTerminateRoomBooking2PortAdapter(ApiLedger.LedgerAccountValidationService.LedgerAccountValidationServiceClient client) : IRoomBookingLifecycleEndpointSendTerminateRoomBooking2Port
{
    public async Task<ApiLedger.NormalizeLedgerAccountResponse> SendAsync(ApiLedger.NormalizeLedgerAccountRequest request, CancellationToken token)
    {
        using var rpc = client.NormalizeLedgerAccountAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class RoomBookingLifecycleEndpointRequests
{
    /// <summary>
    /// Builds the EstimateDesk request from the current workflow state.
    /// </summary>
    public static ApiWorkplaces.EstimateDeskRequest TerminateRoomBookingToEstimateDesk1(this ApiWorkplaces.TerminateRoomBookingRequest request, ApiCorporateevents.ApproveRegistrationResponse reply0)
    {
        return new ApiWorkplaces.EstimateDeskRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Desk = new ApiWorkplaces.Desk
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Registration?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
        };
    }
}
