namespace Acme.Workforce.Orgdesign.Span.Batch;

using Acme.Workforce.Orgdesign.Headcount.Bundle;

/// <summary>Functions of manual span batch from variance.</summary>
public static class ManualSpanBatchFromVariance
{
    public static Dictionary<string, double> SummarizeStrictHeadcountVariancesForBatch(List<StrictHeadcountVariance> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetLoad();
        }
        return totals;
    }

    public static List<ManualSpanBatch> DeriveManualSpanBatchesFromVariance(List<StrictHeadcountVariance> items)
    {
        var outValue = new List<ManualSpanBatch>();
        foreach (var item in items)
        {
            var derived = ManualSpanBatch.NewManualSpanBatch(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeManualSpanBatch(ManualSpanBatch item)
    {
        return HeadcountBundlePolicy.HeadcountBundleBucket(item.EffectiveVolume());
    }
}
