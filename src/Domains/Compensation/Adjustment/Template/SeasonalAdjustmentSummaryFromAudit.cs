namespace Acme.Workforce.Compensation.Adjustment.Template;

using Acme.Workforce.Compensation.Benchmark.BenchmarkAudit;

/// <summary>Functions of seasonal adjustment summary from audit.</summary>
public static class SeasonalAdjustmentSummaryFromAudit
{
    public static Dictionary<string, double> SummarizeStrictBenchmarkAuditsForSummary(List<StrictBenchmarkAudit> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveVolume();
        }
        return totals;
    }

    public static List<SeasonalAdjustmentSummary> DeriveSeasonalAdjustmentSummariesFromAudit(List<StrictBenchmarkAudit> items)
    {
        var outValue = new List<SeasonalAdjustmentSummary>();
        foreach (var item in items)
        {
            var derived = SeasonalAdjustmentSummary.NewSeasonalAdjustmentSummary(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ScoreSeasonalAdjustmentSummary(SeasonalAdjustmentSummary item)
    {
        return BenchmarkSchedulePolicy.BenchmarkScheduleWeight(1, true);
    }
}
