namespace Acme.Workforce.Compensation.Benchmark.Adjustment;

using Acme.Workforce.Compensation.Benchmark.Milestone;

/// <summary>Functions of rolling benchmark summary from claim.</summary>
public static class RollingBenchmarkSummaryFromClaim
{
    public static Dictionary<string, double> SummarizePrimaryBenchmarkClaimsForSummary(List<PrimaryBenchmarkClaim> items)
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

    public static List<RollingBenchmarkSummary> DeriveRollingBenchmarkSummariesFromClaim(List<PrimaryBenchmarkClaim> items)
    {
        var outValue = new List<RollingBenchmarkSummary>();
        foreach (var item in items)
        {
            var derived = RollingBenchmarkSummary.NewRollingBenchmarkSummary(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanRollingBenchmarkSummary(RollingBenchmarkSummary item)
    {
        return BenchmarkWindowPolicy.NormalizeBenchmarkWindowCode(item.Code);
    }
}
