namespace Acme.Workforce.Talent.Pool.Quota;

using Acme.Workforce.Talent.Nomination.Segment;

/// <summary>Functions of pool quota from segment.</summary>
public static class PoolQuotaFromSegment
{
    public static Dictionary<string, double> SummarizeNominationSegmentsForQuota(List<NominationSegment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<PoolQuota> DerivePoolQuotasFromSegment(List<NominationSegment> items)
    {
        var outValue = new List<PoolQuota>();
        foreach (var item in items)
        {
            var derived = PoolQuota.NewPoolQuota(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AllocatePoolQuota(PoolQuota item)
    {
        return BatchedNominationForecastPolicy.BatchedNominationForecastWeight(3, true);
    }
}
