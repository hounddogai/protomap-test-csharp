namespace Acme.Workforce.Engagement.TownHall.HallTier;

using Acme.Workforce.Engagement.Feedback.Quota;

/// <summary>Functions of town hall budget from notice.</summary>
public static class TownHallBudgetFromNotice
{
    public static Dictionary<string, double> SummarizeFeedbackNoticesForBudget(List<FeedbackNotice> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedValue();
        }
        return totals;
    }

    public static List<TownHallBudget> DeriveTownHallBudgetsFromNotice(List<FeedbackNotice> items)
    {
        var outValue = new List<TownHallBudget>();
        foreach (var item in items)
        {
            var derived = TownHallBudget.NewTownHallBudget(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitTownHallBudget(TownHallBudget item)
    {
        return StrictFeedbackQuotaPolicy.StrictFeedbackQuotaBucket(item.AdjustedVolume());
    }
}
