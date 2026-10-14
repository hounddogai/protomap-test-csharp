namespace Acme.Workforce.Compensation.BonusPool.OverrideItem;

using Acme.Workforce.Compensation.Benchmark.BenchmarkEntry;

/// <summary>Functions of bonus pool ledger from entry.</summary>
public static class BonusPoolLedgerFromEntry
{
    public static Dictionary<string, double> SummarizeScheduledBenchmarkEntriesForLedger(List<ScheduledBenchmarkEntry> items)
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

    public static List<BonusPoolLedger> DeriveBonusPoolLedgersFromEntry(List<ScheduledBenchmarkEntry> items)
    {
        var outValue = new List<BonusPoolLedger>();
        foreach (var item in items)
        {
            var derived = BonusPoolLedger.NewBonusPoolLedger(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyBonusPoolLedgerExtra(BonusPoolLedger item)
    {
        return LenientBenchmarkAuditPolicy.LenientBenchmarkAuditBucket(item.AdjustedLoad());
    }
}
