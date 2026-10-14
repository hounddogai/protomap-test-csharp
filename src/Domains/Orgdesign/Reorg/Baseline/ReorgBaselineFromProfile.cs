namespace Acme.Workforce.Orgdesign.Reorg.Baseline;

using Acme.Workforce.Orgdesign.Span.Slot;

/// <summary>Functions of reorg baseline from profile.</summary>
public static class ReorgBaselineFromProfile
{
    public static Dictionary<string, double> SummarizeSpanProfilesForBaseline(List<SpanProfile> items)
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

    public static List<ReorgBaseline> DeriveReorgBaselinesFromProfile(List<SpanProfile> items)
    {
        var outValue = new List<ReorgBaseline>();
        foreach (var item in items)
        {
            var derived = ReorgBaseline.NewReorgBaseline(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanReorgBaseline(ReorgBaseline item)
    {
        return SpanOverridePolicy.SpanOverrideBucket(item.EffectiveLoad());
    }
}
