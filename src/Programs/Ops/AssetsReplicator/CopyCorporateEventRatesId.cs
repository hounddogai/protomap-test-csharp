using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiCampaigns = Acme.Campaigns.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;

namespace Acme.Workforce.Ops.AssetsReplicator;

/// <summary>What CopyCorporateEventRatesId keeps of each GetCorporateEventRates response.</summary>
internal sealed class CopyCorporateEventRatesIdRecord
{
    public string CorporateEventId { get; init; } = "";
    public string Title { get; init; } = "";
}

/// <summary>Writes records to acme.campaigns.v1.MarketingCampaignService.</summary>
internal sealed class CopyCorporateEventRatesIdMarketingCampaignSink(ApiCampaigns.MarketingCampaignService.MarketingCampaignServiceClient client)
{
    public async Task DeliverAsync(CopyCorporateEventRatesIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiCampaigns.SearchMarketingCampaignsRequest { OrderBy = record.CorporateEventId, Filter = new ApiCampaigns.SearchMarketingCampaignsRequest.Types.Filter { Query = record.Title } }, token, client.SearchMarketingCampaignsAsync);
    }
}

/// <summary>Reads acme.corporateevents.v1.CorporateEventCalculationService/GetCorporateEventRates and hands what it keeps to acme.campaigns.v1.MarketingCampaignService.</summary>
internal static class CopyCorporateEventRatesId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiCorporateevents.CorporateEventCalculationService.CorporateEventCalculationServiceClient(channel);
        var got = await source.GetCorporateEventRatesAsync(new ApiCorporateevents.GetCorporateEventRatesRequest() { CorporateEventId = "current-copy-corporate-event-rates-id" }, cancellationToken: token);
        var record = new CopyCorporateEventRatesIdRecord
        {
            CorporateEventId = got.CorporateEvent.CorporateEventId,
            Title = got.CorporateEvent.Title,
        };
        await new CopyCorporateEventRatesIdMarketingCampaignSink(new ApiCampaigns.MarketingCampaignService.MarketingCampaignServiceClient(channel)).DeliverAsync(record, token);
    }
}
