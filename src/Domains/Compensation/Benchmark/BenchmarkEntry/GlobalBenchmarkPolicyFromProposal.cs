namespace Acme.Workforce.Compensation.Benchmark.BenchmarkEntry;

using Acme.Workforce.Compensation.Benchmark.Proposal;

/// <summary>Functions of global benchmark policy from proposal.</summary>
public static class GlobalBenchmarkPolicyFromProposal
{
    public static Dictionary<string, double> SummarizeLenientBenchmarkProposalsForPolicy(List<LenientBenchmarkProposal> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<GlobalBenchmarkPolicy> DeriveGlobalBenchmarkPoliciesFromProposal(List<LenientBenchmarkProposal> items)
    {
        var outValue = new List<GlobalBenchmarkPolicy>();
        foreach (var item in items)
        {
            var derived = GlobalBenchmarkPolicy.NewGlobalBenchmarkPolicy(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeGlobalBenchmarkPolicy(GlobalBenchmarkPolicy item)
    {
        return TieredBenchmarkSummaryPolicy.ClassifyTieredBenchmarkSummary(item.EffectiveLoad());
    }
}
