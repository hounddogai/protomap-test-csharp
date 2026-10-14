namespace Acme.Workforce.Absence.ReturnPlan.SeasonalPlanManifest;

using Acme.Workforce.Absence.LeaveRequest.RequestBudget;

/// <summary>Functions of regional return plan item from profile.</summary>
public static class RegionalReturnPlanItemFromProfile
{
    public static Dictionary<string, double> SummarizeLeaveRequestProfilesForItem(List<LeaveRequestProfile> items)
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

    public static List<RegionalReturnPlanItem> DeriveRegionalReturnPlanItemsFromProfile(List<LeaveRequestProfile> items)
    {
        var outValue = new List<RegionalReturnPlanItem>();
        foreach (var item in items)
        {
            var derived = RegionalReturnPlanItem.NewRegionalReturnPlanItem(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanRegionalReturnPlanItem(RegionalReturnPlanItem item)
    {
        return LeaveRequestAllocationPolicy.LeaveRequestAllocationBucket(item.NetLoad());
    }
}
