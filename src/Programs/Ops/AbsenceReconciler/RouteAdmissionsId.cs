using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiAdmissions = Acme.Admissions.V1;
using ApiCompliance = Acme.Compliance.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.AbsenceReconciler;

/// <summary>Where RouteAdmissionsId hands each record.</summary>
internal interface IRouteAdmissionsIdTarget
{
    Task WriteAsync(RouteAdmissionsIdRecord record, CancellationToken token);
}

/// <summary>What RouteAdmissionsId keeps of each ListAdmissions response.</summary>
internal sealed class RouteAdmissionsIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string NextPageToken { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Writes records to acme.compliance.v1.ComplianceCaseQueryService.</summary>
internal sealed class RouteAdmissionsIdComplianceCaseQuerySink(ApiCompliance.ComplianceCaseQueryService.ComplianceCaseQueryServiceClient client) : IRouteAdmissionsIdTarget
{
    public async Task WriteAsync(RouteAdmissionsIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiCompliance.FindComplianceCasesRequest { Filter = new ApiCompliance.FindComplianceCasesRequest.Types.Filter { Region = record.CorrelationId }, OrderBy = record.Message }, token, client.FindComplianceCasesAsync);
    }
}

/// <summary>Passes records to acme.workplaces.v1.RoomBookingReportingService.</summary>
internal sealed class RouteAdmissionsIdRoomBookingReportingWriter : IRouteAdmissionsIdTarget
{
    private readonly ApiWorkplaces.RoomBookingReportingService.RoomBookingReportingServiceClient _client;

    public RouteAdmissionsIdRoomBookingReportingWriter(ApiWorkplaces.RoomBookingReportingService.RoomBookingReportingServiceClient client) => _client = client;

    public async Task WriteAsync(RouteAdmissionsIdRecord record, CancellationToken token)
    {
        await _client.ListRoomBookingReportsAsync(new ApiWorkplaces.ListRoomBookingReportsRequest { Filter = new ApiWorkplaces.ListRoomBookingReportsRequest.Types.Filter { Region = record.CorrelationId, OwnerId = record.NextPageToken }, OrderBy = record.Message }, cancellationToken: token);
    }
}

/// <summary>Reads acme.admissions.v1.AdmissionService/ListAdmissions and hands what it keeps to the sink of its kind.</summary>
internal static class RouteAdmissionsId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiAdmissions.AdmissionService.AdmissionServiceClient(channel);
        var found = await source.ListAdmissionsAsync(new ApiAdmissions.ListAdmissionsRequest() { OrderBy = "current-route-admissions-id" }, cancellationToken: token);
        var record = new RouteAdmissionsIdRecord
        {
            CorrelationId = found.Result.CorrelationId,
            NextPageToken = found.PageInfo.NextPageToken,
            Message = found.Result.Message,
        };
        var sinks = new Dictionary<string, IRouteAdmissionsIdTarget>
        {
            ["compliance-case-query"] = new RouteAdmissionsIdComplianceCaseQuerySink(new ApiCompliance.ComplianceCaseQueryService.ComplianceCaseQueryServiceClient(channel)),
            ["room-booking-reporting"] = new RouteAdmissionsIdRoomBookingReportingWriter(new ApiWorkplaces.RoomBookingReportingService.RoomBookingReportingServiceClient(channel)),
        };
        foreach (var (kind, sink) in sinks)
        {
            if (kind != "compliance-case-query")
                continue;
            await sink.WriteAsync(record, token);
        }
    }
}
