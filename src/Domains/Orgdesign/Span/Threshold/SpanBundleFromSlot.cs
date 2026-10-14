namespace Acme.Workforce.Orgdesign.Span.Threshold;

using Acme.Workforce.Orgdesign.Span.Entry;

/// <summary>Functions of span bundle from slot.</summary>
public static class SpanBundleFromSlot
{
    public static Dictionary<string, double> SummarizeCappedSpanSlotsForBundle(List<CappedSpanSlot> items)
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

    public static List<SpanBundle> DeriveSpanBundlesFromSlot(List<CappedSpanSlot> items)
    {
        var outValue = new List<SpanBundle>();
        foreach (var item in items)
        {
            var derived = SpanBundle.NewSpanBundle(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveSpanBundle(SpanBundle item)
    {
        return SpanEntryPolicy.NormalizeSpanEntryCode(item.Code);
    }
}
