namespace Acme.Workforce.Contractors.Rate.Preference;

using Acme.Workforce.Contractors.Offboarding.OverrideItem;

/// <summary>Functions of rate rule from review.</summary>
public static class RateRuleFromReview
{
    public static Dictionary<string, double> SummarizeRegionalOffboardingReviewsForRule(List<RegionalOffboardingReview> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetValue();
        }
        return totals;
    }

    public static List<RateRule> DeriveRateRulesFromReview(List<RegionalOffboardingReview> items)
    {
        var outValue = new List<RateRule>();
        foreach (var item in items)
        {
            var derived = RateRule.NewRateRule(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double RankRateRule(RateRule item)
    {
        return SeasonalOffboardingSlotPolicy.SeasonalOffboardingSlotWeight(9, true);
    }
}
