namespace Acme.Workforce.Orgdesign.Span.Slot;

using Acme.Workforce.Orgdesign.Position.Mapping;

/// <summary>Functions of span override from ledger.</summary>
public static class SpanOverrideFromLedger
{
    public static Dictionary<string, double> SummarizePositionLedgersForOverride(List<PositionLedger> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedVolume();
        }
        return totals;
    }

    public static List<SpanOverride> DeriveSpanOverridesFromLedger(List<PositionLedger> items)
    {
        var outValue = new List<SpanOverride>();
        foreach (var item in items)
        {
            var derived = SpanOverride.NewSpanOverride(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreSpanOverride(SpanOverride item)
    {
        return PositionMappingPolicy.PositionMappingBucket(item.WeightedValue());
    }
}
