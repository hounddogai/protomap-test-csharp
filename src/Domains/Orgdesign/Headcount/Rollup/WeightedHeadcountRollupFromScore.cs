namespace Acme.Workforce.Orgdesign.Headcount.Rollup;

using Acme.Workforce.Orgdesign.Reorg.Draft;

/// <summary>Functions of weighted headcount rollup from score.</summary>
public static class WeightedHeadcountRollupFromScore
{
    public static Dictionary<string, double> SummarizeRegionalReorgScoresForRollup(List<RegionalReorgScore> items)
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

    public static List<WeightedHeadcountRollup> DeriveWeightedHeadcountRollupsFromScore(List<RegionalReorgScore> items)
    {
        var outValue = new List<WeightedHeadcountRollup>();
        foreach (var item in items)
        {
            var derived = WeightedHeadcountRollup.NewWeightedHeadcountRollup(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectWeightedHeadcountRollup(WeightedHeadcountRollup item)
    {
        return StrictReorgDraftPolicy.ClassifyStrictReorgDraft(item.MeasuredValue());
    }
}
