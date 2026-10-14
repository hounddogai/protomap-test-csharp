namespace Acme.Workforce.Workplace.Visitor.Policy;

using Acme.Workforce.Workplace.FloorPlan.Notice;

/// <summary>Functions of visitor policy from target.</summary>
public static class VisitorPolicyFromTarget
{
    public static Dictionary<string, double> SummarizeDefaultFloorPlanTargetsForPolicy(List<DefaultFloorPlanTarget> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<VisitorPolicy> DeriveVisitorPoliciesFromTarget(List<DefaultFloorPlanTarget> items)
    {
        var outValue = new List<VisitorPolicy>();
        foreach (var item in items)
        {
            var derived = VisitorPolicy.NewVisitorPolicy(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankVisitorPolicy(VisitorPolicy item)
    {
        return RollingFloorPlanSegmentPolicy.ClassifyRollingFloorPlanSegment(item.ProjectedExposure());
    }
}
