namespace Acme.Workforce.Compensation.PayBand.Tier;

using Acme.Workforce.Compensation.Benchmark.Profile;

/// <summary>Functions of legacy pay band rollup from snapshot.</summary>
public static class LegacyPayBandRollupFromSnapshot
{
    public static Dictionary<string, double> SummarizeBenchmarkSnapshotsForRollup(List<BenchmarkSnapshot> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedVolume();
        }
        return totals;
    }

    public static List<LegacyPayBandRollup> DeriveLegacyPayBandRollupsFromSnapshot(List<BenchmarkSnapshot> items)
    {
        var outValue = new List<LegacyPayBandRollup>();
        foreach (var item in items)
        {
            var derived = LegacyPayBandRollup.NewLegacyPayBandRollup(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchLegacyPayBandRollup(LegacyPayBandRollup item)
    {
        return BenchmarkScorePolicy.BenchmarkScoreBucket(item.AdjustedExposure());
    }
}
