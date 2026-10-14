namespace Acme.Workforce.Absence.ReturnPlan.Policy;

using Acme.Workforce.Absence.Accrual.Segment;

/// <summary>Functions of return plan quota from threshold.</summary>
public static class ReturnPlanQuotaFromThreshold
{
    public static Dictionary<string, double> SummarizeAccrualThresholdsForQuota(List<AccrualThreshold> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetValue();
        }
        return totals;
    }

    public static List<ReturnPlanQuota> DeriveReturnPlanQuotasFromThreshold(List<AccrualThreshold> items)
    {
        var outValue = new List<ReturnPlanQuota>();
        foreach (var item in items)
        {
            var derived = ReturnPlanQuota.NewReturnPlanQuota(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double MergeReturnPlanQuota(ReturnPlanQuota item)
    {
        return AccrualThresholdPolicy.AccrualThresholdWeight(5, true);
    }
}
