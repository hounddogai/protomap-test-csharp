using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiBoxoffice = Acme.Boxoffice.V1;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiShipping = Acme.Shipping.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.TalentBackfill;

/// <summary>Where ExportItineraryApprovalNotes hands each record.</summary>
internal interface IExportItineraryApprovalNotesTarget
{
    Task DeliverAsync(ExportItineraryApprovalNotesRecord record, CancellationToken token);
}

/// <summary>What ExportItineraryApprovalNotes keeps of each DelegateItineraryApproval response.</summary>
internal sealed class ExportItineraryApprovalNotesRecord
{
    public string Notes { get; init; } = "";
    public string Number { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Passes records to acme.devices.v1.DevicePolicyQueryService.</summary>
internal sealed class ExportItineraryApprovalNotesDevicePolicyQueryTarget : IExportItineraryApprovalNotesTarget
{
    private readonly ApiDevices.DevicePolicyQueryService.DevicePolicyQueryServiceClient _client;

    public ExportItineraryApprovalNotesDevicePolicyQueryTarget(ApiDevices.DevicePolicyQueryService.DevicePolicyQueryServiceClient client) => _client = client;

    public async Task DeliverAsync(ExportItineraryApprovalNotesRecord record, CancellationToken token)
    {
        await _client.SuggestDevicePoliciesAsync(new ApiDevices.SuggestDevicePoliciesRequest { Filter = new ApiDevices.SuggestDevicePoliciesRequest.Types.Filter { OwnerId = record.Notes, Region = record.Status }, OrderBy = record.Number }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.shipping.v1.ShipmentService.</summary>
internal sealed class ExportItineraryApprovalNotesShipmentWriter(ApiShipping.ShipmentService.ShipmentServiceClient client) : IExportItineraryApprovalNotesTarget
{
    public async Task DeliverAsync(ExportItineraryApprovalNotesRecord record, CancellationToken token)
    {
        await client.ListShipmentsAsync(new ApiShipping.ListShipmentsRequest { Page = new ApiCommon.PageRequest { PageToken = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.boxoffice.v1.BoxOfficeSaleQueryService.</summary>
internal sealed class ExportItineraryApprovalNotesBoxOfficeSaleQueryTarget(ApiBoxoffice.BoxOfficeSaleQueryService.BoxOfficeSaleQueryServiceClient client) : IExportItineraryApprovalNotesTarget
{
    public async Task DeliverAsync(ExportItineraryApprovalNotesRecord record, CancellationToken token)
    {
        await client.CountBoxOfficeSalesAsync(new ApiBoxoffice.CountBoxOfficeSalesRequest { Filter = new ApiBoxoffice.CountBoxOfficeSalesRequest.Types.Filter { Query = record.Status }, Page = new ApiCommon.PageRequest { PageToken = record.Number } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.travel.v1.ItineraryApprovalService/DelegateItineraryApproval and hands what it keeps to the sink of its kind.</summary>
internal static class ExportItineraryApprovalNotes
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiTravel.ItineraryApprovalService.ItineraryApprovalServiceClient(channel);
        var found = await source.DelegateItineraryApprovalAsync(new ApiTravel.DelegateItineraryApprovalRequest() { Reason = "default-export-itinerary-approval-notes" }, cancellationToken: token);
        var record = new ExportItineraryApprovalNotesRecord
        {
            Notes = found.Itinerary.Notes,
            Number = Text.Normalize(found.Itinerary.Number),
            Status = found.Result.Status,
        };
        var sinks = new Dictionary<string, IExportItineraryApprovalNotesTarget>
        {
            ["device-policy-query"] = new ExportItineraryApprovalNotesDevicePolicyQueryTarget(new ApiDevices.DevicePolicyQueryService.DevicePolicyQueryServiceClient(channel)),
            ["shipment"] = new ExportItineraryApprovalNotesShipmentWriter(new ApiShipping.ShipmentService.ShipmentServiceClient(channel)),
            ["box-office-sale-query"] = new ExportItineraryApprovalNotesBoxOfficeSaleQueryTarget(new ApiBoxoffice.BoxOfficeSaleQueryService.BoxOfficeSaleQueryServiceClient(channel)),
        };
        await RouteExportItineraryApprovalNotesAsync("shipment", record, sinks, token);
    }

    private static async Task RouteExportItineraryApprovalNotesAsync(string kind, ExportItineraryApprovalNotesRecord record, IReadOnlyDictionary<string, IExportItineraryApprovalNotesTarget> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.DeliverAsync(record, token);
    }
}
