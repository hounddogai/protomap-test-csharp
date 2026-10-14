namespace Acme.Workforce.Workplace.Visitor.Adjustment;

using Acme.Workforce.Workplace.AccessBadge.BadgeBudget;

/// <summary>Functions of global visitor adjustment from rollup.</summary>
public static class GlobalVisitorAdjustmentFromRollup
{
    public static Dictionary<string, double> SummarizeDefaultAccessBadgeRollupsForAdjustment(List<DefaultAccessBadgeRollup> items)
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

    public static List<GlobalVisitorAdjustment> DeriveGlobalVisitorAdjustmentsFromRollup(List<DefaultAccessBadgeRollup> items)
    {
        var outValue = new List<GlobalVisitorAdjustment>();
        foreach (var item in items)
        {
            var derived = GlobalVisitorAdjustment.NewGlobalVisitorAdjustment(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveGlobalVisitorAdjustment(GlobalVisitorAdjustment item)
    {
        return ScheduledAccessBadgeReservationPolicy.NormalizeScheduledAccessBadgeReservationCode(item.Code);
    }
}
