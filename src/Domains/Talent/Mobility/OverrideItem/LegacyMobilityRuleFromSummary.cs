namespace Acme.Workforce.Talent.Mobility.OverrideItem;

using Acme.Workforce.Talent.CareerPath.PathSummary;

/// <summary>Functions of legacy mobility rule from summary.</summary>
public static class LegacyMobilityRuleFromSummary
{
    public static Dictionary<string, double> SummarizeRegionalCareerPathSummariesForRule(List<RegionalCareerPathSummary> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveTotal();
        }
        return totals;
    }

    public static List<LegacyMobilityRule> DeriveLegacyMobilityRulesFromSummary(List<RegionalCareerPathSummary> items)
    {
        var outValue = new List<LegacyMobilityRule>();
        foreach (var item in items)
        {
            var derived = LegacyMobilityRule.NewLegacyMobilityRule(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankLegacyMobilityRule(LegacyMobilityRule item)
    {
        return FallbackCareerPathAdjustmentPolicy.FallbackCareerPathAdjustmentBucket(item.WeightedValue());
    }
}
