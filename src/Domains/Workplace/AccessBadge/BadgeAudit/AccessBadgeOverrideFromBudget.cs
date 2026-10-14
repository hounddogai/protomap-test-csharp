namespace Acme.Workforce.Workplace.AccessBadge.BadgeAudit;

using Acme.Workforce.Workplace.Visitor.Exception;

/// <summary>Functions of access badge override from budget.</summary>
public static class AccessBadgeOverrideFromBudget
{
    public static Dictionary<string, double> SummarizeVisitorBudgetsForOverride(List<VisitorBudget> items)
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

    public static List<AccessBadgeOverride> DeriveAccessBadgeOverridesFromBudget(List<VisitorBudget> items)
    {
        var outValue = new List<AccessBadgeOverride>();
        foreach (var item in items)
        {
            var derived = AccessBadgeOverride.NewAccessBadgeOverride(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleAccessBadgeOverride(AccessBadgeOverride item)
    {
        return LenientVisitorExceptionPolicy.NormalizeLenientVisitorExceptionCode(item.Code);
    }
}
