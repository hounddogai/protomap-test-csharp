namespace Acme.Workforce.Compensation.MeritCycle.Plan;

using Acme.Workforce.Compensation.Benchmark.Summary;

/// <summary>Functions of merit cycle claim from estimate.</summary>
public static class MeritCycleClaimFromEstimate
{
    public static Dictionary<string, double> SummarizeBenchmarkEstimatesForClaim(List<BenchmarkEstimate> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedTotal();
        }
        return totals;
    }

    public static List<MeritCycleClaim> DeriveMeritCycleClaimsFromEstimate(List<BenchmarkEstimate> items)
    {
        var outValue = new List<MeritCycleClaim>();
        foreach (var item in items)
        {
            var derived = MeritCycleClaim.NewMeritCycleClaim(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchMeritCycleClaim(MeritCycleClaim item)
    {
        return BenchmarkSummaryPolicy.NormalizeBenchmarkSummaryCode(item.Code);
    }
}
