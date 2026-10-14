namespace Acme.Workforce.Compensation.EquityGrant.GrantRecord;

using Acme.Workforce.Compensation.PayBand.Estimate;

/// <summary>Functions of tiered equity grant record from quota.</summary>
public static class TieredEquityGrantRecordFromQuota
{
    public static Dictionary<string, double> SummarizeSeasonalPayBandQuotasForRecord(List<SeasonalPayBandQuota> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedValue();
        }
        return totals;
    }

    public static List<TieredEquityGrantRecord> DeriveTieredEquityGrantRecordsFromQuota(List<SeasonalPayBandQuota> items)
    {
        var outValue = new List<TieredEquityGrantRecord>();
        foreach (var item in items)
        {
            var derived = TieredEquityGrantRecord.NewTieredEquityGrantRecord(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleTieredEquityGrantRecord(TieredEquityGrantRecord item)
    {
        return CappedPayBandThresholdPolicy.CappedPayBandThresholdBucket(item.NetTotal());
    }
}
