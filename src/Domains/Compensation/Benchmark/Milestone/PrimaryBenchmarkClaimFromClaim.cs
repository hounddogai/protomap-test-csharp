namespace Acme.Workforce.Compensation.Benchmark.Milestone;

using Acme.Workforce.Compensation.Benchmark.Threshold;

/// <summary>Functions of primary benchmark claim from claim.</summary>
public static class PrimaryBenchmarkClaimFromClaim
{
    public static Dictionary<string, double> SummarizeBenchmarkClaimsForClaim(List<BenchmarkClaim> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<PrimaryBenchmarkClaim> DerivePrimaryBenchmarkClaimsFromClaim(List<BenchmarkClaim> items)
    {
        var outValue = new List<PrimaryBenchmarkClaim>();
        foreach (var item in items)
        {
            var derived = PrimaryBenchmarkClaim.NewPrimaryBenchmarkClaim(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimatePrimaryBenchmarkClaim(PrimaryBenchmarkClaim item)
    {
        return BenchmarkBudgetPolicy.ClassifyBenchmarkBudget(item.MeasuredValue());
    }
}
