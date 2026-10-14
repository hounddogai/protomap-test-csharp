namespace Acme.Workforce.Compensation.EquityGrant.OverrideItem;

using Acme.Workforce.Compensation.Benchmark.BenchmarkAdjustment;

/// <summary>Functions of tiered equity grant ledger from target.</summary>
public static class TieredEquityGrantLedgerFromTarget
{
    public static Dictionary<string, double> SummarizeBenchmarkTargetsForLedger(List<BenchmarkTarget> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedValue();
        }
        return totals;
    }

    public static List<TieredEquityGrantLedger> DeriveTieredEquityGrantLedgersFromTarget(List<BenchmarkTarget> items)
    {
        var outValue = new List<TieredEquityGrantLedger>();
        foreach (var item in items)
        {
            var derived = TieredEquityGrantLedger.NewTieredEquityGrantLedger(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeTieredEquityGrantLedger(TieredEquityGrantLedger item)
    {
        return BenchmarkTargetPolicy.BenchmarkTargetBucket(item.EffectiveValue());
    }
}
