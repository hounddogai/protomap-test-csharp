namespace Acme.Workforce.Payrollops.Garnishment.Agreement;

using Acme.Workforce.Payrollops.Garnishment.Review;

/// <summary>Functions of capped garnishment review from summary.</summary>
public static class CappedGarnishmentReviewFromSummary
{
    public static Dictionary<string, double> SummarizeGarnishmentSummariesForReview(List<GarnishmentSummary> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedLoad();
        }
        return totals;
    }

    public static List<CappedGarnishmentReview> DeriveCappedGarnishmentReviewsFromSummary(List<GarnishmentSummary> items)
    {
        var outValue = new List<CappedGarnishmentReview>();
        foreach (var item in items)
        {
            var derived = CappedGarnishmentReview.NewCappedGarnishmentReview(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitCappedGarnishmentReview(CappedGarnishmentReview item)
    {
        return GarnishmentSummaryPolicy.GarnishmentSummaryBucket(item.AdjustedValue());
    }
}
