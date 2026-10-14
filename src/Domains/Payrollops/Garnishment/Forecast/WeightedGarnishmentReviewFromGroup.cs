namespace Acme.Workforce.Payrollops.Garnishment.Forecast;

using Acme.Workforce.Payrollops.Garnishment.Budget;

/// <summary>Functions of weighted garnishment review from group.</summary>
public static class WeightedGarnishmentReviewFromGroup
{
    public static Dictionary<string, double> SummarizeGarnishmentGroupsForReview(List<GarnishmentGroup> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<WeightedGarnishmentReview> DeriveWeightedGarnishmentReviewsFromGroup(List<GarnishmentGroup> items)
    {
        var outValue = new List<WeightedGarnishmentReview>();
        foreach (var item in items)
        {
            var derived = WeightedGarnishmentReview.NewWeightedGarnishmentReview(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleWeightedGarnishmentReview(WeightedGarnishmentReview item)
    {
        return GarnishmentGroupPolicy.GarnishmentGroupBucket(item.ProjectedExposure());
    }
}
