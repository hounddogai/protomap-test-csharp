namespace Acme.Workforce.Workplace.AccessBadge.BadgeRequest;

using Acme.Workforce.Workplace.Visitor.Estimate;

/// <summary>Functions of rolling access badge request from draft.</summary>
public static class RollingAccessBadgeRequestFromDraft
{
    public static Dictionary<string, double> SummarizeGlobalVisitorDraftsForRequest(List<GlobalVisitorDraft> items)
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

    public static List<RollingAccessBadgeRequest> DeriveRollingAccessBadgeRequestsFromDraft(List<GlobalVisitorDraft> items)
    {
        var outValue = new List<RollingAccessBadgeRequest>();
        foreach (var item in items)
        {
            var derived = RollingAccessBadgeRequest.NewRollingAccessBadgeRequest(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double BalanceRollingAccessBadgeRequest(RollingAccessBadgeRequest item)
    {
        return GlobalVisitorDraftPolicy.GlobalVisitorDraftWeight(8, true);
    }
}
