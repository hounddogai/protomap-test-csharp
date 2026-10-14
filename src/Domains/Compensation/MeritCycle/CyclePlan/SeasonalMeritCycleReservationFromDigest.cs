namespace Acme.Workforce.Compensation.MeritCycle.CyclePlan;

using Acme.Workforce.Compensation.Benchmark.BenchmarkEntry;

/// <summary>Functions of seasonal merit cycle reservation from digest.</summary>
public static class SeasonalMeritCycleReservationFromDigest
{
    public static Dictionary<string, double> SummarizeSeasonalBenchmarkDigestsForReservation(List<SeasonalBenchmarkDigest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedExposure();
        }
        return totals;
    }

    public static List<SeasonalMeritCycleReservation> DeriveSeasonalMeritCycleReservationsFromDigest(List<SeasonalBenchmarkDigest> items)
    {
        var outValue = new List<SeasonalMeritCycleReservation>();
        foreach (var item in items)
        {
            var derived = SeasonalMeritCycleReservation.NewSeasonalMeritCycleReservation(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeSeasonalMeritCycleReservation(SeasonalMeritCycleReservation item)
    {
        return SeasonalBenchmarkDigestPolicy.NormalizeSeasonalBenchmarkDigestCode(item.Code);
    }
}
