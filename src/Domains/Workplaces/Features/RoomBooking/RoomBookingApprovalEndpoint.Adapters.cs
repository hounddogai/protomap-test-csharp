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
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

/// <summary>
/// The application port for ListVisitorPassConflicts.
/// </summary>
public interface IRoomBookingApprovalEndpointSendRecallRoomBooking1Port
{
    Task<ApiWorkplaces.ListVisitorPassConflictsResponse> SendAsync(ApiWorkplaces.ListVisitorPassConflictsRequest request, CancellationToken token);
}

public sealed class RoomBookingApprovalEndpointSendRecallRoomBooking1PortAdapter(ApiWorkplaces.VisitorPassSyncService.VisitorPassSyncServiceClient client) : IRoomBookingApprovalEndpointSendRecallRoomBooking1Port
{
    public async Task<ApiWorkplaces.ListVisitorPassConflictsResponse> SendAsync(ApiWorkplaces.ListVisitorPassConflictsRequest request, CancellationToken token)
    {
        using var rpc = client.ListVisitorPassConflictsAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}

public static class RoomBookingApprovalEndpointRequests
{
    /// <summary>
    /// Builds the PauseBuildingWorkflow request from the current workflow state.
    /// </summary>
    public static ApiWorkplaces.PauseBuildingWorkflowRequest RecallRoomBookingToPauseBuildingWorkflow0(this ApiWorkplaces.RecallRoomBookingRequest request)
    {
        return new ApiWorkplaces.PauseBuildingWorkflowRequest
        {
            Building = new ApiWorkplaces.Building
            {
                Children = { new ApiWorkplaces.Building
                {
                    Children = { new ApiWorkplaces.Building
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            CreatedAt = (request.RoomBooking?.Audit?.CreatedAt ?? new Timestamp()),
                            UpdatedBy = (request.RoomBooking?.Audit?.UpdatedBy ?? ""),
                        },
                        Manager = new ApiIdentity.UserRef
                        {
                            DisplayName = (request.RoomBooking?.Organizer?.DisplayName ?? ""),
                        },
                    } },
                    Manager = new ApiIdentity.UserRef
                    {
                        UserId = (request.Details?.Approver?.UserId ?? ""),
                    },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
    }
}
