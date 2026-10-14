namespace Acme.Workforce.Compensation.Adjustment.AdjustmentAgreement;

using Acme.Workforce.Compensation.Benchmark.Rule;

/// <summary>Functions of pending adjustment agreement from rule.</summary>
public static class PendingAdjustmentAgreementFromRule
{
    public static Dictionary<string, double> SummarizeBenchmarkRulesForAgreement(List<BenchmarkRule> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetTotal();
        }
        return totals;
    }

    public static List<PendingAdjustmentAgreement> DerivePendingAdjustmentAgreementsFromRule(List<BenchmarkRule> items)
    {
        var outValue = new List<PendingAdjustmentAgreement>();
        foreach (var item in items)
        {
            var derived = PendingAdjustmentAgreement.NewPendingAdjustmentAgreement(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizePendingAdjustmentAgreement(PendingAdjustmentAgreement item)
    {
        return WeightedBenchmarkMappingPolicy.NormalizeWeightedBenchmarkMappingCode(item.Code);
    }
}
