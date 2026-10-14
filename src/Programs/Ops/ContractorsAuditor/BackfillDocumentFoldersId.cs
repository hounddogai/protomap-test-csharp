using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiDocuments = Acme.Documents.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.ContractorsAuditor;

/// <summary>Where BackfillDocumentFoldersId hands each record.</summary>
internal interface IBackfillDocumentFoldersIdDestination
{
    Task PushAsync(BackfillDocumentFoldersIdRecord record, CancellationToken token);
}

/// <summary>What BackfillDocumentFoldersId keeps of each LintDocumentFolders response.</summary>
internal sealed class BackfillDocumentFoldersIdRecord
{
    public string ReferenceId { get; init; } = "";
    public string DocumentFolderId { get; init; } = "";
    public string Status { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Sends records to acme.travel.v1.TravelBookingQueryService.</summary>
internal sealed class BackfillDocumentFoldersIdTravelBookingQueryRelay(ApiTravel.TravelBookingQueryService.TravelBookingQueryServiceClient client) : IBackfillDocumentFoldersIdDestination
{
    public async Task PushAsync(BackfillDocumentFoldersIdRecord record, CancellationToken token)
    {
        await client.LookupTravelBookingAsync(new ApiTravel.LookupTravelBookingRequest { TravelBookingId = record.Message }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.travel.v1.TripQueryService.</summary>
internal sealed class BackfillDocumentFoldersIdTripQueryRelay(ApiTravel.TripQueryService.TripQueryServiceClient client) : IBackfillDocumentFoldersIdDestination
{
    public async Task PushAsync(BackfillDocumentFoldersIdRecord record, CancellationToken token)
    {
        await client.SummarizeTripsAsync(new ApiTravel.SummarizeTripsRequest { Details = new ApiTravel.SummarizeTripsRequest.Types.Details { Cron = record.Message, Format = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.licensing.v1.LicenseAssignmentApprovalService.</summary>
internal sealed class BackfillDocumentFoldersIdLicenseAssignmentApprovalTarget : IBackfillDocumentFoldersIdDestination
{
    private readonly ApiLicensing.LicenseAssignmentApprovalService.LicenseAssignmentApprovalServiceClient _client;

    public BackfillDocumentFoldersIdLicenseAssignmentApprovalTarget(ApiLicensing.LicenseAssignmentApprovalService.LicenseAssignmentApprovalServiceClient client) => _client = client;

    public async Task PushAsync(BackfillDocumentFoldersIdRecord record, CancellationToken token)
    {
        await _client.ApproveLicenseAssignmentAsync(new ApiLicensing.ApproveLicenseAssignmentRequest { LicenseAssignment = new ApiLicensing.LicenseAssignment { LicenseAssignmentId = record.Message, Notes = record.DocumentFolderId }, Details = new ApiLicensing.ApproveLicenseAssignmentRequest.Types.Details { Comment = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.documents.v1.DocumentFolderValidationService/LintDocumentFolders and hands what it keeps to the sink of its kind.</summary>
internal static class BackfillDocumentFoldersId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiDocuments.DocumentFolderValidationService.DocumentFolderValidationServiceClient(channel);
        var response = await source.LintDocumentFoldersAsync(new ApiDocuments.LintDocumentFoldersRequest() { DocumentFolderId = "nightly-backfill-document-folders-id" }, cancellationToken: token);
        var record = new BackfillDocumentFoldersIdRecord
        {
            ReferenceId = Text.Normalize(response.Outcome.ReferenceId),
            DocumentFolderId = response.DocumentFolder.DocumentFolderId,
            Status = response.Result.Status,
            Message = response.Result.Message,
        };
        var sinks = new Dictionary<string, IBackfillDocumentFoldersIdDestination>
        {
            ["travel-booking-query"] = new BackfillDocumentFoldersIdTravelBookingQueryRelay(new ApiTravel.TravelBookingQueryService.TravelBookingQueryServiceClient(channel)),
            ["trip-query"] = new BackfillDocumentFoldersIdTripQueryRelay(new ApiTravel.TripQueryService.TripQueryServiceClient(channel)),
            ["license-assignment-approval"] = new BackfillDocumentFoldersIdLicenseAssignmentApprovalTarget(new ApiLicensing.LicenseAssignmentApprovalService.LicenseAssignmentApprovalServiceClient(channel)),
        };
        foreach (var (kind, sink) in sinks)
        {
            if (kind != "travel-booking-query")
                continue;
            await sink.PushAsync(record, token);
        }
    }
}
