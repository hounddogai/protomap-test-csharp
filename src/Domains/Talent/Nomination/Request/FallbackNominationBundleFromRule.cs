namespace Acme.Workforce.Talent.Nomination.Request;

using Acme.Workforce.Talent.SuccessionPlan.Item;

/// <summary>Functions of fallback nomination bundle from rule.</summary>
public static class FallbackNominationBundleFromRule
{
    public static Dictionary<string, double> SummarizeSeasonalSuccessionPlanRulesForBundle(List<SeasonalSuccessionPlanRule> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<FallbackNominationBundle> DeriveFallbackNominationBundlesFromRule(List<SeasonalSuccessionPlanRule> items)
    {
        var outValue = new List<FallbackNominationBundle>();
        foreach (var item in items)
        {
            var derived = FallbackNominationBundle.NewFallbackNominationBundle(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ScoreFallbackNominationBundle(FallbackNominationBundle item)
    {
        return SuccessionPlanItemPolicy.SuccessionPlanItemWeight(3, true);
    }
}
