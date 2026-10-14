namespace Acme.Workforce.Compensation.Benchmark.Audit;

using Acme.Workforce.Compensation.Benchmark.Entry;

/// <summary>Functions of benchmark audit from entry.</summary>
public static class BenchmarkAuditFromEntry
{
    public static Dictionary<string, double> SummarizeBatchedBenchmarkEntriesForAudit(List<BatchedBenchmarkEntry> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveValue();
        }
        return totals;
    }

    public static List<BenchmarkAudit> DeriveBenchmarkAuditsFromEntry(List<BatchedBenchmarkEntry> items)
    {
        var outValue = new List<BenchmarkAudit>();
        foreach (var item in items)
        {
            var derived = BenchmarkAudit.NewBenchmarkAudit(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeBenchmarkAudit(BenchmarkAudit item)
    {
        return BenchmarkEntryPolicy.ClassifyBenchmarkEntry(item.ProjectedVolume());
    }
}
