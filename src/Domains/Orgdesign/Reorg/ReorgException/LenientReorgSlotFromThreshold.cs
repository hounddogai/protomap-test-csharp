namespace Acme.Workforce.Orgdesign.Reorg.ReorgException;

using Acme.Workforce.Orgdesign.Span.Policy;

/// <summary>Functions of lenient reorg slot from threshold.</summary>
public static class LenientReorgSlotFromThreshold
{
    public static Dictionary<string, double> SummarizeCappedSpanThresholdsForSlot(List<CappedSpanThreshold> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<LenientReorgSlot> DeriveLenientReorgSlotsFromThreshold(List<CappedSpanThreshold> items)
    {
        var outValue = new List<LenientReorgSlot>();
        foreach (var item in items)
        {
            var derived = LenientReorgSlot.NewLenientReorgSlot(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveLenientReorgSlot(LenientReorgSlot item)
    {
        return FallbackSpanScorePolicy.FallbackSpanScoreBucket(item.WeightedValue());
    }
}
