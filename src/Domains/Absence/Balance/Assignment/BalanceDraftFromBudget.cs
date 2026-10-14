namespace Acme.Workforce.Absence.Balance.Assignment;

using Acme.Workforce.Absence.Policy.OverrideItem;

/// <summary>Functions of balance draft from budget.</summary>
public static class BalanceDraftFromBudget
{
    public static Dictionary<string, double> SummarizePolicyBudgetsForDraft(List<PolicyBudget> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetVolume();
        }
        return totals;
    }

    public static List<BalanceDraft> DeriveBalanceDraftsFromBudget(List<PolicyBudget> items)
    {
        var outValue = new List<BalanceDraft>();
        foreach (var item in items)
        {
            var derived = BalanceDraft.NewBalanceDraft(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SettleBalanceDraft(BalanceDraft item)
    {
        return PolicyOverridePolicy.PolicyOverrideWeight(2, true);
    }
}
