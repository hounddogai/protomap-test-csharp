namespace Acme.Workforce.Absence.Balance.Draft;

using Acme.Workforce.Absence.ReturnPlan.Threshold;

/// <summary>Functions of pending balance template from review.</summary>
public static class PendingBalanceTemplateFromReview
{
    public static Dictionary<string, double> SummarizeCappedReturnPlanReviewsForTemplate(List<CappedReturnPlanReview> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveVolume();
        }
        return totals;
    }

    public static List<PendingBalanceTemplate> DerivePendingBalanceTemplatesFromReview(List<CappedReturnPlanReview> items)
    {
        var outValue = new List<PendingBalanceTemplate>();
        foreach (var item in items)
        {
            var derived = PendingBalanceTemplate.NewPendingBalanceTemplate(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanPendingBalanceTemplate(PendingBalanceTemplate item)
    {
        return AdaptiveReturnPlanThresholdPolicy.AdaptiveReturnPlanThresholdBucket(item.AdjustedValue());
    }
}
