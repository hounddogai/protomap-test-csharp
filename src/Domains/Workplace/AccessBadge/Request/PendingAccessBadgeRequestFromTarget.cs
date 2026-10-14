namespace Acme.Workforce.Workplace.AccessBadge.Request;

using Acme.Workforce.Workplace.FloorPlan.Notice;

/// <summary>Functions of pending access badge request from target.</summary>
public static class PendingAccessBadgeRequestFromTarget
{
    public static Dictionary<string, double> SummarizeDefaultFloorPlanTargetsForRequest(List<DefaultFloorPlanTarget> items)
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

    public static List<PendingAccessBadgeRequest> DerivePendingAccessBadgeRequestsFromTarget(List<DefaultFloorPlanTarget> items)
    {
        var outValue = new List<PendingAccessBadgeRequest>();
        foreach (var item in items)
        {
            var derived = PendingAccessBadgeRequest.NewPendingAccessBadgeRequest(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ResolvePendingAccessBadgeRequest(PendingAccessBadgeRequest item)
    {
        return LegacyFloorPlanNoticePolicy.LegacyFloorPlanNoticeWeight(4, true);
    }
}
