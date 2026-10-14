namespace Acme.Workforce.Compensation.EquityGrant.GrantBatch;

using Acme.Workforce.Compensation.MeritCycle.CycleReview;

/// <summary>Functions of lenient equity grant batch from profile.</summary>
public static class LenientEquityGrantBatchFromProfile
{
    public static Dictionary<string, double> SummarizeSeasonalMeritCycleProfilesForBatch(List<SeasonalMeritCycleProfile> items)
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

    public static List<LenientEquityGrantBatch> DeriveLenientEquityGrantBatchesFromProfile(List<SeasonalMeritCycleProfile> items)
    {
        var outValue = new List<LenientEquityGrantBatch>();
        foreach (var item in items)
        {
            var derived = LenientEquityGrantBatch.NewLenientEquityGrantBatch(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitLenientEquityGrantBatch(LenientEquityGrantBatch item)
    {
        return WeightedMeritCycleTierPolicy.WeightedMeritCycleTierBucket(item.MeasuredValue());
    }
}
