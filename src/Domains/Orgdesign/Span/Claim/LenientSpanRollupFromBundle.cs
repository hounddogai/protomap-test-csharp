namespace Acme.Workforce.Orgdesign.Span.Claim;

using Acme.Workforce.Orgdesign.Position.Rule;

/// <summary>Functions of lenient span rollup from bundle.</summary>
public static class LenientSpanRollupFromBundle
{
    public static Dictionary<string, double> SummarizeTieredPositionBundlesForRollup(List<TieredPositionBundle> items)
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

    public static List<LenientSpanRollup> DeriveLenientSpanRollupsFromBundle(List<TieredPositionBundle> items)
    {
        var outValue = new List<LenientSpanRollup>();
        foreach (var item in items)
        {
            var derived = LenientSpanRollup.NewLenientSpanRollup(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignLenientSpanRollup(LenientSpanRollup item)
    {
        return TieredPositionBundlePolicy.TieredPositionBundleBucket(item.EffectiveLoad());
    }
}
