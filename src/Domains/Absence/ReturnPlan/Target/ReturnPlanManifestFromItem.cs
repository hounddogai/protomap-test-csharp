namespace Acme.Workforce.Absence.ReturnPlan.Target;

using Acme.Workforce.Absence.LeaveRequest.Item;

/// <summary>Functions of return plan manifest from item.</summary>
public static class ReturnPlanManifestFromItem
{
    public static Dictionary<string, double> SummarizeLeaveRequestItemsForManifest(List<LeaveRequestItem> items)
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

    public static List<ReturnPlanManifest> DeriveReturnPlanManifestsFromItem(List<LeaveRequestItem> items)
    {
        var outValue = new List<ReturnPlanManifest>();
        foreach (var item in items)
        {
            var derived = ReturnPlanManifest.NewReturnPlanManifest(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveReturnPlanManifest(ReturnPlanManifest item)
    {
        return LeaveRequestScorePolicy.ClassifyLeaveRequestScore(item.AdjustedExposure());
    }
}
