namespace Acme.Workforce.Orgdesign.Span.TieredSpanDraft;

using Acme.Workforce.Orgdesign.CostModel.ModelManifest;

/// <summary>Functions of span manifest from rule.</summary>
public static class SpanManifestFromRule
{
    public static Dictionary<string, double> SummarizeCappedCostModelRulesForManifest(List<CappedCostModelRule> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<SpanManifest> DeriveSpanManifestsFromRule(List<CappedCostModelRule> items)
    {
        var outValue = new List<SpanManifest>();
        foreach (var item in items)
        {
            var derived = SpanManifest.NewSpanManifest(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeSpanManifest(SpanManifest item)
    {
        return ScheduledCostModelBatchPolicy.NormalizeScheduledCostModelBatchCode(item.Code);
    }
}
