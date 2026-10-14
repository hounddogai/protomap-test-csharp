namespace Acme.Workforce.Absence.LeaveRequest.Snapshot;

using Acme.Workforce.Absence.ReturnPlan.Window;

/// <summary>Functions of adaptive leave request policy from item.</summary>
public static class AdaptiveLeaveRequestPolicyFromItem
{
    public static Dictionary<string, double> SummarizeTieredReturnPlanItemsForPolicy(List<TieredReturnPlanItem> items)
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

    public static List<AdaptiveLeaveRequestPolicy> DeriveAdaptiveLeaveRequestPoliciesFromItem(List<TieredReturnPlanItem> items)
    {
        var outValue = new List<AdaptiveLeaveRequestPolicy>();
        foreach (var item in items)
        {
            var derived = AdaptiveLeaveRequestPolicy.NewAdaptiveLeaveRequestPolicy(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateAdaptiveLeaveRequestPolicy(AdaptiveLeaveRequestPolicy item)
    {
        return TieredReturnPlanItemPolicy.ClassifyTieredReturnPlanItem(item.NetVolume());
    }
}
