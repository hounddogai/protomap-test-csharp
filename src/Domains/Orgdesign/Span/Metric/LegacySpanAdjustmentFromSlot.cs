namespace Acme.Workforce.Orgdesign.Span.Metric;

using Acme.Workforce.Orgdesign.Span.Estimate;

/// <summary>Functions of legacy span adjustment from slot.</summary>
public static class LegacySpanAdjustmentFromSlot
{
    public static Dictionary<string, double> SummarizeGlobalSpanSlotsForAdjustment(List<GlobalSpanSlot> items)
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

    public static List<LegacySpanAdjustment> DeriveLegacySpanAdjustmentsFromSlot(List<GlobalSpanSlot> items)
    {
        var outValue = new List<LegacySpanAdjustment>();
        foreach (var item in items)
        {
            var derived = LegacySpanAdjustment.NewLegacySpanAdjustment(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankLegacySpanAdjustment(LegacySpanAdjustment item)
    {
        return GlobalSpanSlotPolicy.GlobalSpanSlotBucket(item.EffectiveTotal());
    }
}
