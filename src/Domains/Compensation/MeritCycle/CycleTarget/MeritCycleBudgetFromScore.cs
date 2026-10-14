namespace Acme.Workforce.Compensation.MeritCycle.CycleTarget;

using Acme.Workforce.Compensation.PayBand.Score;

/// <summary>Functions of merit cycle budget from score.</summary>
public static class MeritCycleBudgetFromScore
{
    public static Dictionary<string, double> SummarizeFallbackPayBandScoresForBudget(List<FallbackPayBandScore> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedTotal();
        }
        return totals;
    }

    public static List<MeritCycleBudget> DeriveMeritCycleBudgetsFromScore(List<FallbackPayBandScore> items)
    {
        var outValue = new List<MeritCycleBudget>();
        foreach (var item in items)
        {
            var derived = MeritCycleBudget.NewMeritCycleBudget(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimateMeritCycleBudget(MeritCycleBudget item)
    {
        return ScheduledPayBandReviewPolicy.ClassifyScheduledPayBandReview(item.EffectiveValue());
    }
}
