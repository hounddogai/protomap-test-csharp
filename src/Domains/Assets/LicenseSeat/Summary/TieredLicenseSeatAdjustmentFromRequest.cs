namespace Acme.Workforce.Assets.LicenseSeat.Summary;

using Acme.Workforce.Assets.Refresh.Request;

/// <summary>Functions of tiered license seat adjustment from request.</summary>
public static class TieredLicenseSeatAdjustmentFromRequest
{
    public static Dictionary<string, double> SummarizeRegionalRefreshRequestsForAdjustment(List<RegionalRefreshRequest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedLoad();
        }
        return totals;
    }

    public static List<TieredLicenseSeatAdjustment> DeriveTieredLicenseSeatAdjustmentsFromRequest(List<RegionalRefreshRequest> items)
    {
        var outValue = new List<TieredLicenseSeatAdjustment>();
        foreach (var item in items)
        {
            var derived = TieredLicenseSeatAdjustment.NewTieredLicenseSeatAdjustment(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveTieredLicenseSeatAdjustment(TieredLicenseSeatAdjustment item)
    {
        return PendingRefreshMappingPolicy.ClassifyPendingRefreshMapping(item.NetTotal());
    }
}
