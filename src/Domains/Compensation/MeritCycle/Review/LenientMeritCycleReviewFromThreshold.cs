namespace Acme.Workforce.Compensation.MeritCycle.Review;

using Acme.Workforce.Compensation.BonusPool.Tier;

/// <summary>Functions of lenient merit cycle review from threshold.</summary>
public static class LenientMeritCycleReviewFromThreshold
{
    public static Dictionary<string, double> SummarizeBonusPoolThresholdsForReview(List<BonusPoolThreshold> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<LenientMeritCycleReview> DeriveLenientMeritCycleReviewsFromThreshold(List<BonusPoolThreshold> items)
    {
        var outValue = new List<LenientMeritCycleReview>();
        foreach (var item in items)
        {
            var derived = LenientMeritCycleReview.NewLenientMeritCycleReview(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SettleLenientMeritCycleReview(LenientMeritCycleReview item)
    {
        return ManualBonusPoolTemplatePolicy.ManualBonusPoolTemplateWeight(6, true);
    }
}
