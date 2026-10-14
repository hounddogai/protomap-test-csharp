namespace Acme.Workforce.Assets.LicenseSeat.RecordItem;

using Acme.Workforce.Assets.AssetReturn.Adjustment;

/// <summary>Functions of license seat summary from adjustment.</summary>
public static class LicenseSeatSummaryFromAdjustment
{
    public static Dictionary<string, double> SummarizeAssetReturnAdjustmentsForSummary(List<AssetReturnAdjustment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetTotal();
        }
        return totals;
    }

    public static List<LicenseSeatSummary> DeriveLicenseSeatSummariesFromAdjustment(List<AssetReturnAdjustment> items)
    {
        var outValue = new List<LicenseSeatSummary>();
        foreach (var item in items)
        {
            var derived = LicenseSeatSummary.NewLicenseSeatSummary(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveLicenseSeatSummary(LicenseSeatSummary item)
    {
        return AssetReturnBatchPolicy.NormalizeAssetReturnBatchCode(item.Code);
    }
}
