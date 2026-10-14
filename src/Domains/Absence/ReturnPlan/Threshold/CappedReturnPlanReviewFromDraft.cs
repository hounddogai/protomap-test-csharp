namespace Acme.Workforce.Absence.ReturnPlan.Threshold;

using Acme.Workforce.Absence.ReturnPlan.Audit;

/// <summary>Functions of capped return plan review from draft.</summary>
public static class CappedReturnPlanReviewFromDraft
{
    public static Dictionary<string, double> SummarizeReturnPlanDraftsForReview(List<ReturnPlanDraft> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<CappedReturnPlanReview> DeriveCappedReturnPlanReviewsFromDraft(List<ReturnPlanDraft> items)
    {
        var outValue = new List<CappedReturnPlanReview>();
        foreach (var item in items)
        {
            var derived = CappedReturnPlanReview.NewCappedReturnPlanReview(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitCappedReturnPlanReview(CappedReturnPlanReview item)
    {
        return ReturnPlanDraftPolicy.ReturnPlanDraftBucket(item.EffectiveVolume());
    }
}
