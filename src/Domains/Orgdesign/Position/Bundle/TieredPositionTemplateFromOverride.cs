namespace Acme.Workforce.Orgdesign.Position.Bundle;

using Acme.Workforce.Orgdesign.Span.Slot;

/// <summary>Functions of tiered position template from override.</summary>
public static class TieredPositionTemplateFromOverride
{
    public static Dictionary<string, double> SummarizeSpanOverridesForTemplate(List<SpanOverride> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedValue();
        }
        return totals;
    }

    public static List<TieredPositionTemplate> DeriveTieredPositionTemplatesFromOverride(List<SpanOverride> items)
    {
        var outValue = new List<TieredPositionTemplate>();
        foreach (var item in items)
        {
            var derived = TieredPositionTemplate.NewTieredPositionTemplate(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleTieredPositionTemplate(TieredPositionTemplate item)
    {
        return SpanProfilePolicy.SpanProfileBucket(item.ProjectedTotal());
    }
}
