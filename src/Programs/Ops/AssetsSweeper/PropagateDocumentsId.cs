using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiDocuments = Acme.Documents.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Ops.AssetsSweeper;

/// <summary>What PropagateDocumentsId keeps of each ResolveDocuments response.</summary>
internal sealed class PropagateDocumentsIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string NextPageToken { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Passes records to acme.venues.v1.VenueValidationService.</summary>
internal sealed class PropagateDocumentsIdVenueValidationRelay(ApiVenues.VenueValidationService.VenueValidationServiceClient client)
{
    public async Task PushAsync(PropagateDocumentsIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiVenues.NormalizeVenueRequest { Venue = new ApiVenues.Venue { Name = record.CorrelationId, VenueId = record.NextPageToken }, Reason = record.Status }, token, client.NormalizeVenueAsync);
    }
}

/// <summary>Reads acme.documents.v1.DocumentQueryService/ResolveDocuments and hands what it keeps to acme.venues.v1.VenueValidationService.</summary>
internal static class PropagateDocumentsId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiDocuments.DocumentQueryService.DocumentQueryServiceClient(channel);
        var reply = await source.ResolveDocumentsAsync(new ApiDocuments.ResolveDocumentsRequest(), cancellationToken: token);
        var record = new PropagateDocumentsIdRecord
        {
            CorrelationId = reply.Result.CorrelationId,
            NextPageToken = reply.PageInfo.NextPageToken,
            Status = reply.Result.Status,
        };
        await new PropagateDocumentsIdVenueValidationRelay(new ApiVenues.VenueValidationService.VenueValidationServiceClient(channel)).PushAsync(record, token);
    }
}
