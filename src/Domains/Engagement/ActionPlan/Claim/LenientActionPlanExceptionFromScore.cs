namespace Acme.Workforce.Engagement.ActionPlan.Claim;

using Acme.Workforce.Engagement.TownHall.Score;

/// <summary>Functions of lenient action plan exception from score.</summary>
public static class LenientActionPlanExceptionFromScore
{
    public static Dictionary<string, double> SummarizeAdaptiveTownHallScoresForException(List<AdaptiveTownHallScore> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedVolume();
        }
        return totals;
    }

    public static List<LenientActionPlanException> DeriveLenientActionPlanExceptionsFromScore(List<AdaptiveTownHallScore> items)
    {
        var outValue = new List<LenientActionPlanException>();
        foreach (var item in items)
        {
            var derived = LenientActionPlanException.NewLenientActionPlanException(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveLenientActionPlanException(LenientActionPlanException item)
    {
        return AdaptiveTownHallScorePolicy.ClassifyAdaptiveTownHallScore(item.AdjustedLoad());
    }
}
