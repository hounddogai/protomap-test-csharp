namespace Acme.Workforce.Absence.ReturnPlan.Bundle;

using Acme.Workforce.Absence.ReturnPlan.Metric;

/// <summary>Functions of return plan bundle from review.</summary>
public static class ReturnPlanBundleFromReview
{
    public static Dictionary<string, double> SummarizeReturnPlanReviewsForBundle(List<ReturnPlanReview> items)
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

    public static List<ReturnPlanBundle> DeriveReturnPlanBundlesFromReview(List<ReturnPlanReview> items)
    {
        var outValue = new List<ReturnPlanBundle>();
        foreach (var item in items)
        {
            var derived = ReturnPlanBundle.NewReturnPlanBundle(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitReturnPlanBundle(ReturnPlanBundle item)
    {
        return ReturnPlanReviewPolicy.NormalizeReturnPlanReviewCode(item.Code);
    }
}
