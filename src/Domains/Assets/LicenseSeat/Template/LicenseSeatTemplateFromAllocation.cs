namespace Acme.Workforce.Assets.LicenseSeat.Template;

using Acme.Workforce.Assets.Refresh.Ledger;

/// <summary>Functions of license seat template from allocation.</summary>
public static class LicenseSeatTemplateFromAllocation
{
    public static Dictionary<string, double> SummarizeSeasonalRefreshAllocationsForTemplate(List<SeasonalRefreshAllocation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedExposure();
        }
        return totals;
    }

    public static List<LicenseSeatTemplate> DeriveLicenseSeatTemplatesFromAllocation(List<SeasonalRefreshAllocation> items)
    {
        var outValue = new List<LicenseSeatTemplate>();
        foreach (var item in items)
        {
            var derived = LicenseSeatTemplate.NewLicenseSeatTemplate(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimateLicenseSeatTemplate(LicenseSeatTemplate item)
    {
        return RefreshLedgerPolicy.RefreshLedgerBucket(item.WeightedTotal());
    }
}
