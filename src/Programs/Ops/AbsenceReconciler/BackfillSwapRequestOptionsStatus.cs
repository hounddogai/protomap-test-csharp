using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiScheduling = Acme.Scheduling.V1;

namespace Acme.Workforce.Ops.AbsenceReconciler;

/// <summary>Where BackfillSwapRequestOptionsStatus hands each record.</summary>
internal interface IBackfillSwapRequestOptionsStatusWriter
{
    Task ApplyAsync(BackfillSwapRequestOptionsStatusRecord record, CancellationToken token);
}

/// <summary>What BackfillSwapRequestOptionsStatus keeps of each CompareSwapRequestOptions response.</summary>
internal sealed class BackfillSwapRequestOptionsStatusRecord
{
    public string Status { get; init; } = "";
    public string Subject { get; init; } = "";
    public string CurrencyCode { get; init; } = "";
}

/// <summary>Hands records to acme.corporateevents.v1.VenueReportingService.</summary>
internal sealed class BackfillSwapRequestOptionsStatusVenueReportingWriter(ApiCorporateevents.VenueReportingService.VenueReportingServiceClient client) : IBackfillSwapRequestOptionsStatusWriter
{
    public async Task ApplyAsync(BackfillSwapRequestOptionsStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiCorporateevents.ListVenueReportsRequest { OrderBy = record.Status }, token, client.ListVenueReportsAsync);
    }
}

/// <summary>Writes records to acme.scheduling.v1.AvailabilityWindowValidationService.</summary>
internal sealed class BackfillSwapRequestOptionsStatusAvailabilityWindowValidationWriter(ApiScheduling.AvailabilityWindowValidationService.AvailabilityWindowValidationServiceClient client) : IBackfillSwapRequestOptionsStatusWriter
{
    public async Task ApplyAsync(BackfillSwapRequestOptionsStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiScheduling.PreviewAvailabilityWindowRequest { AvailabilityWindow = new ApiScheduling.AvailabilityWindow { AvailabilityWindowId = record.Status, Title = record.CurrencyCode }, Details = new ApiScheduling.PreviewAvailabilityWindowRequest.Types.Details { Locale = record.Subject } }, token, client.PreviewAvailabilityWindowAsync);
    }
}

/// <summary>Reads acme.scheduling.v1.SwapRequestCalculationService/CompareSwapRequestOptions and hands what it keeps to the sink of its kind.</summary>
internal static class BackfillSwapRequestOptionsStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiScheduling.SwapRequestCalculationService.SwapRequestCalculationServiceClient(channel);
        var found = await source.CompareSwapRequestOptionsAsync(new ApiScheduling.CompareSwapRequestOptionsRequest() { SwapRequestId = "live-backfill-swap-request-options-status" }, cancellationToken: token);
        var record = new BackfillSwapRequestOptionsStatusRecord
        {
            Status = found.Result.Status,
            Subject = found.SwapRequest.Subject,
            CurrencyCode = found.Breakdown.CurrencyCode,
        };
        var sinks = new List<IBackfillSwapRequestOptionsStatusWriter>
        {
            new BackfillSwapRequestOptionsStatusVenueReportingWriter(new ApiCorporateevents.VenueReportingService.VenueReportingServiceClient(channel)),
            new BackfillSwapRequestOptionsStatusAvailabilityWindowValidationWriter(new ApiScheduling.AvailabilityWindowValidationService.AvailabilityWindowValidationServiceClient(channel)),
        };
        await sinks[0].ApplyAsync(record, token);
    }
}
