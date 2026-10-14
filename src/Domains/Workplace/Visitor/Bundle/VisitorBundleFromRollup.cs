namespace Acme.Workforce.Workplace.Visitor.Bundle;

using Acme.Workforce.Workplace.AccessBadge.Rollup;

/// <summary>Functions of visitor bundle from rollup.</summary>
public static class VisitorBundleFromRollup
{
    public static Dictionary<string, double> SummarizeAccessBadgeRollupsForBundle(List<AccessBadgeRollup> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedVolume();
        }
        return totals;
    }

    public static List<VisitorBundle> DeriveVisitorBundlesFromRollup(List<AccessBadgeRollup> items)
    {
        var outValue = new List<VisitorBundle>();
        foreach (var item in items)
        {
            var derived = VisitorBundle.NewVisitorBundle(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchVisitorBundle(VisitorBundle item)
    {
        return AccessBadgeReviewPolicy.AccessBadgeReviewBucket(item.NetValue());
    }
}
