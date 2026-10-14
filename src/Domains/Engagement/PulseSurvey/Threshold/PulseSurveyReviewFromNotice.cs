namespace Acme.Workforce.Engagement.PulseSurvey.Threshold;

using Acme.Workforce.Engagement.ActionPlan.Notice;

/// <summary>Functions of pulse survey review from notice.</summary>
public static class PulseSurveyReviewFromNotice
{
    public static Dictionary<string, double> SummarizeActionPlanNoticesForReview(List<ActionPlanNotice> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedExposure();
        }
        return totals;
    }

    public static List<PulseSurveyReview> DerivePulseSurveyReviewsFromNotice(List<ActionPlanNotice> items)
    {
        var outValue = new List<PulseSurveyReview>();
        foreach (var item in items)
        {
            var derived = PulseSurveyReview.NewPulseSurveyReview(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcilePulseSurveyReview(PulseSurveyReview item)
    {
        return TieredActionPlanReservationPolicy.TieredActionPlanReservationBucket(item.NetExposure());
    }
}
