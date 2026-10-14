namespace Acme.Workforce.Engagement.TownHall.Tier;

using Acme.Workforce.Engagement.Feedback.FeedbackProposal;

/// <summary>Functions of town hall tier from proposal.</summary>
public static class TownHallTierFromProposal
{
    public static Dictionary<string, double> SummarizeLegacyFeedbackProposalsForTier(List<LegacyFeedbackProposal> items)
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

    public static List<TownHallTier> DeriveTownHallTiersFromProposal(List<LegacyFeedbackProposal> items)
    {
        var outValue = new List<TownHallTier>();
        foreach (var item in items)
        {
            var derived = TownHallTier.NewTownHallTier(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyTownHallTierExtra(TownHallTier item)
    {
        return ScheduledFeedbackForecastPolicy.NormalizeScheduledFeedbackForecastCode(item.Code);
    }
}
