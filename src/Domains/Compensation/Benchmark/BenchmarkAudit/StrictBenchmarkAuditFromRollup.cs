namespace Acme.Workforce.Compensation.Benchmark.BenchmarkAudit;

using Acme.Workforce.Compensation.Adjustment.Mapping;

/// <summary>Functions of strict benchmark audit from rollup.</summary>
public static class StrictBenchmarkAuditFromRollup
{
    public static Dictionary<string, double> SummarizeGlobalAdjustmentRollupsForAudit(List<GlobalAdjustmentRollup> items)
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

    public static List<StrictBenchmarkAudit> DeriveStrictBenchmarkAuditsFromRollup(List<GlobalAdjustmentRollup> items)
    {
        var outValue = new List<StrictBenchmarkAudit>();
        foreach (var item in items)
        {
            var derived = StrictBenchmarkAudit.NewStrictBenchmarkAudit(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceStrictBenchmarkAudit(StrictBenchmarkAudit item)
    {
        return PrimaryAdjustmentPreferencePolicy.PrimaryAdjustmentPreferenceBucket(item.EffectiveVolume());
    }
}
