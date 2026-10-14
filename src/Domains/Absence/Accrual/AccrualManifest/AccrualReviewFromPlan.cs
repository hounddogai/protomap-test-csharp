namespace Acme.Workforce.Absence.Accrual.AccrualManifest;

using Acme.Workforce.Absence.LeaveRequest.Batch;

/// <summary>Functions of accrual review from plan.</summary>
public static class AccrualReviewFromPlan
{
    public static Dictionary<string, double> SummarizeTieredLeaveRequestPlansForReview(List<TieredLeaveRequestPlan> items)
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

    public static List<AccrualReview> DeriveAccrualReviewsFromPlan(List<TieredLeaveRequestPlan> items)
    {
        var outValue = new List<AccrualReview>();
        foreach (var item in items)
        {
            var derived = AccrualReview.NewAccrualReview(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SummarizeAccrualReview(AccrualReview item)
    {
        return PendingLeaveRequestBatchPolicy.PendingLeaveRequestBatchWeight(6, true);
    }
}
