namespace Acme.Workforce.Compensation.Benchmark.Preference;

using Acme.Workforce.Compensation.Adjustment.Draft;

/// <summary>Functions of benchmark request from draft.</summary>
public static class BenchmarkRequestFromDraft
{
    public static Dictionary<string, double> SummarizeFallbackAdjustmentDraftsForRequest(List<FallbackAdjustmentDraft> items)
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

    public static List<BenchmarkRequest> DeriveBenchmarkRequestsFromDraft(List<FallbackAdjustmentDraft> items)
    {
        var outValue = new List<BenchmarkRequest>();
        foreach (var item in items)
        {
            var derived = BenchmarkRequest.NewBenchmarkRequest(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveBenchmarkRequest(BenchmarkRequest item)
    {
        return FallbackAdjustmentDraftPolicy.NormalizeFallbackAdjustmentDraftCode(item.Code);
    }
}
