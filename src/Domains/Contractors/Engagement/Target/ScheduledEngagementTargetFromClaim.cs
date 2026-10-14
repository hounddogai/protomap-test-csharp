namespace Acme.Workforce.Contractors.Engagement.Target;

using Acme.Workforce.Contractors.Engagement.Notice;

/// <summary>Functions of scheduled engagement target from claim.</summary>
public static class ScheduledEngagementTargetFromClaim
{
    public static Dictionary<string, double> SummarizeEngagementClaimsForTarget(List<EngagementClaim> items)
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

    public static List<ScheduledEngagementTarget> DeriveScheduledEngagementTargetsFromClaim(List<EngagementClaim> items)
    {
        var outValue = new List<ScheduledEngagementTarget>();
        foreach (var item in items)
        {
            var derived = ScheduledEngagementTarget.NewScheduledEngagementTarget(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeScheduledEngagementTarget(ScheduledEngagementTarget item)
    {
        return EngagementClaimPolicy.ClassifyEngagementClaim(item.WeightedValue());
    }
}
