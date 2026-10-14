namespace Acme.Workforce.Workplace.Desk.DeskProfile;

using Acme.Workforce.Workplace.AccessBadge.BadgeRequest;

/// <summary>Functions of scheduled desk rollup from estimate.</summary>
public static class ScheduledDeskRollupFromEstimate
{
    public static Dictionary<string, double> SummarizeAccessBadgeEstimatesForRollup(List<AccessBadgeEstimate> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<ScheduledDeskRollup> DeriveScheduledDeskRollupsFromEstimate(List<AccessBadgeEstimate> items)
    {
        var outValue = new List<ScheduledDeskRollup>();
        foreach (var item in items)
        {
            var derived = ScheduledDeskRollup.NewScheduledDeskRollup(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeScheduledDeskRollup(ScheduledDeskRollup item)
    {
        return LegacyAccessBadgeMetricPolicy.NormalizeLegacyAccessBadgeMetricCode(item.Code);
    }
}
