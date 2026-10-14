namespace Acme.Workforce.Compensation.Benchmark.BenchmarkAudit;

using Acme.Workforce.Compensation.Benchmark.Allocation;

/// <summary>Functions of benchmark schedule from batch.</summary>
public static class BenchmarkScheduleFromBatch
{
    public static Dictionary<string, double> SummarizeBenchmarkBatchesForSchedule(List<BenchmarkBatch> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedTotal();
        }
        return totals;
    }

    public static List<BenchmarkSchedule> DeriveBenchmarkSchedulesFromBatch(List<BenchmarkBatch> items)
    {
        var outValue = new List<BenchmarkSchedule>();
        foreach (var item in items)
        {
            var derived = BenchmarkSchedule.NewBenchmarkSchedule(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveBenchmarkSchedule(BenchmarkSchedule item)
    {
        return BenchmarkBatchPolicy.NormalizeBenchmarkBatchCode(item.Code);
    }
}
