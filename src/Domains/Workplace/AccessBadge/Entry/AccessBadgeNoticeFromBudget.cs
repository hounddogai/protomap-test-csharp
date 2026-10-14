namespace Acme.Workforce.Workplace.AccessBadge.Entry;

using Acme.Workforce.Workplace.Desk.DeskBudget;

/// <summary>Functions of access badge notice from budget.</summary>
public static class AccessBadgeNoticeFromBudget
{
    public static Dictionary<string, double> SummarizeDefaultDeskBudgetsForNotice(List<DefaultDeskBudget> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveLoad();
        }
        return totals;
    }

    public static List<AccessBadgeNotice> DeriveAccessBadgeNoticesFromBudget(List<DefaultDeskBudget> items)
    {
        var outValue = new List<AccessBadgeNotice>();
        foreach (var item in items)
        {
            var derived = AccessBadgeNotice.NewAccessBadgeNotice(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchAccessBadgeNotice(AccessBadgeNotice item)
    {
        return DefaultDeskBudgetPolicy.NormalizeDefaultDeskBudgetCode(item.Code);
    }
}
