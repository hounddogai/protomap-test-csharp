namespace Acme.Workforce.Absence.LeaveRequest.Budget;

using Acme.Workforce.Absence.LeaveRequest.Review;

/// <summary>Functions of leave request batch from adjustment.</summary>
public static class LeaveRequestBatchFromAdjustment
{
    public static Dictionary<string, double> SummarizeWeightedLeaveRequestAdjustmentsForBatch(List<WeightedLeaveRequestAdjustment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveTotal();
        }
        return totals;
    }

    public static List<LeaveRequestBatch> DeriveLeaveRequestBatchesFromAdjustment(List<WeightedLeaveRequestAdjustment> items)
    {
        var outValue = new List<LeaveRequestBatch>();
        foreach (var item in items)
        {
            var derived = LeaveRequestBatch.NewLeaveRequestBatch(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ReconcileLeaveRequestBatch(LeaveRequestBatch item)
    {
        return StrictLeaveRequestReviewPolicy.StrictLeaveRequestReviewWeight(4, true);
    }
}
