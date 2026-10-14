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
using ApiBoxoffice = Acme.Boxoffice.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiPipelines = Acme.Pipelines.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

/// <summary>
/// The application port for CompareRoomBookingOptions.
/// </summary>
public interface IVisitorPassApprovalEndpointSendApproveVisitorPass0Port
{
    Task<ApiWorkplaces.CompareRoomBookingOptionsResponse> SendAsync(ApiWorkplaces.CompareRoomBookingOptionsRequest request, CancellationToken token);
}

public sealed class VisitorPassApprovalEndpointSendApproveVisitorPass0PortAdapter(ApiWorkplaces.RoomBookingCalculationService.RoomBookingCalculationServiceClient client) : IVisitorPassApprovalEndpointSendApproveVisitorPass0Port
{
    public async Task<ApiWorkplaces.CompareRoomBookingOptionsResponse> SendAsync(ApiWorkplaces.CompareRoomBookingOptionsRequest request, CancellationToken token)
    {
        using var rpc = client.CompareRoomBookingOptionsAsync(request, cancellationToken: token);
        return await rpc.ResponseAsync;
    }
}
