namespace Acme.Workforce.Absence.ReturnPlan.Window;

using Acme.Workforce.Absence.LeaveRequest.RequestClaim;

/// <summary>Functions of tiered return plan item from preference.</summary>
public static class TieredReturnPlanItemFromPreference
{
    public static Dictionary<string, double> SummarizeLegacyLeaveRequestPreferencesForItem(List<LegacyLeaveRequestPreference> items)
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

    public static List<TieredReturnPlanItem> DeriveTieredReturnPlanItemsFromPreference(List<LegacyLeaveRequestPreference> items)
    {
        var outValue = new List<TieredReturnPlanItem>();
        foreach (var item in items)
        {
            var derived = TieredReturnPlanItem.NewTieredReturnPlanItem(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankTieredReturnPlanItem(TieredReturnPlanItem item)
    {
        return ScheduledLeaveRequestMappingPolicy.ClassifyScheduledLeaveRequestMapping(item.MeasuredValue());
    }
}
