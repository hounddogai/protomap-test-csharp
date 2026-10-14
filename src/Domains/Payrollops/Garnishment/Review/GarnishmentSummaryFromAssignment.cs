namespace Acme.Workforce.Payrollops.Garnishment.Review;

using Acme.Workforce.Payrollops.Deduction.Rollup;

/// <summary>Functions of garnishment summary from assignment.</summary>
public static class GarnishmentSummaryFromAssignment
{
    public static Dictionary<string, double> SummarizeDeductionAssignmentsForSummary(List<DeductionAssignment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetLoad();
        }
        return totals;
    }

    public static List<GarnishmentSummary> DeriveGarnishmentSummariesFromAssignment(List<DeductionAssignment> items)
    {
        var outValue = new List<GarnishmentSummary>();
        foreach (var item in items)
        {
            var derived = GarnishmentSummary.NewGarnishmentSummary(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeGarnishmentSummary(GarnishmentSummary item)
    {
        return DeductionAssignmentPolicy.DeductionAssignmentBucket(item.AdjustedLoad());
    }
}
