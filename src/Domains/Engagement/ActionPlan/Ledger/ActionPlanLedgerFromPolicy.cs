namespace Acme.Workforce.Engagement.ActionPlan.Ledger;

using Acme.Workforce.Engagement.ActionPlan.PlanPreference;

/// <summary>Functions of action plan ledger from policy.</summary>
public static class ActionPlanLedgerFromPolicy
{
    public static Dictionary<string, double> SummarizeActionPlanPoliciesForLedger(List<ActionPlanPolicy> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<ActionPlanLedger> DeriveActionPlanLedgersFromPolicy(List<ActionPlanPolicy> items)
    {
        var outValue = new List<ActionPlanLedger>();
        foreach (var item in items)
        {
            var derived = ActionPlanLedger.NewActionPlanLedger(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ScoreActionPlanLedger(ActionPlanLedger item)
    {
        return ActionPlanPolicyPolicy.ActionPlanPolicyWeight(7, true);
    }
}
