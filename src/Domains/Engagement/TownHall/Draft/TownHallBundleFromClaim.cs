namespace Acme.Workforce.Engagement.TownHall.Draft;

using Acme.Workforce.Engagement.Feedback.Rule;

/// <summary>Functions of town hall bundle from claim.</summary>
public static class TownHallBundleFromClaim
{
    public static Dictionary<string, double> SummarizeLegacyFeedbackClaimsForBundle(List<LegacyFeedbackClaim> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetVolume();
        }
        return totals;
    }

    public static List<TownHallBundle> DeriveTownHallBundlesFromClaim(List<LegacyFeedbackClaim> items)
    {
        var outValue = new List<TownHallBundle>();
        foreach (var item in items)
        {
            var derived = TownHallBundle.NewTownHallBundle(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileTownHallBundle(TownHallBundle item)
    {
        return DefaultFeedbackRulePolicy.DefaultFeedbackRuleBucket(item.EffectiveValue());
    }
}
