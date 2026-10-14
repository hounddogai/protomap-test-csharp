namespace Acme.Workforce.Engagement.Feedback.Group;

using Acme.Workforce.Engagement.TownHall.Adjustment;

/// <summary>Functions of rolling feedback proposal from cycle.</summary>
public static class RollingFeedbackProposalFromCycle
{
    public static Dictionary<string, double> SummarizeSeasonalTownHallCyclesForProposal(List<SeasonalTownHallCycle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedVolume();
        }
        return totals;
    }

    public static List<RollingFeedbackProposal> DeriveRollingFeedbackProposalsFromCycle(List<SeasonalTownHallCycle> items)
    {
        var outValue = new List<RollingFeedbackProposal>();
        foreach (var item in items)
        {
            var derived = RollingFeedbackProposal.NewRollingFeedbackProposal(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SettleRollingFeedbackProposal(RollingFeedbackProposal item)
    {
        return SeasonalTownHallCyclePolicy.SeasonalTownHallCycleWeight(4, true);
    }
}
