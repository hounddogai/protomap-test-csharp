using Grpc.Net.Client;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiFanportal = Acme.Fanportal.V1;

namespace Acme.Workforce.Ops.AssetsSweeper;

/// <summary>What PropagateCorporateEventReportTimezone keeps of each GenerateCorporateEventReport response.</summary>
internal sealed class PropagateCorporateEventReportTimezoneRecord
{
    public string Timezone { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Sends records to acme.fanportal.v1.FanSessionQueryService.</summary>
internal sealed class PropagateCorporateEventReportTimezoneFanSessionQueryTarget(ApiFanportal.FanSessionQueryService.FanSessionQueryServiceClient client)
{
    public async Task PushAsync(PropagateCorporateEventReportTimezoneRecord record, CancellationToken token)
    {
        await client.LookupFanSessionAsync(new ApiFanportal.LookupFanSessionRequest { FanSessionId = record.Message }, cancellationToken: token);
    }
}

/// <summary>Reads acme.corporateevents.v1.CorporateEventReportingService/GenerateCorporateEventReport and hands what it keeps to acme.fanportal.v1.FanSessionQueryService.</summary>
internal static class PropagateCorporateEventReportTimezone
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiCorporateevents.CorporateEventReportingService.CorporateEventReportingServiceClient(channel);
        var found = await source.GenerateCorporateEventReportAsync(new ApiCorporateevents.GenerateCorporateEventReportRequest() { CorporateEventId = "nightly-propagate-corporate-event-report-timezone" }, cancellationToken: token);
        var record = new PropagateCorporateEventReportTimezoneRecord
        {
            Timezone = found.Period.Timezone,
            Message = found.Result.Message,
        };
        await new PropagateCorporateEventReportTimezoneFanSessionQueryTarget(new ApiFanportal.FanSessionQueryService.FanSessionQueryServiceClient(channel)).PushAsync(record, token);
    }
}
