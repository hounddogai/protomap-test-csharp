namespace Acme.Workforce.Absence.Balance.Summary;

using Acme.Workforce.Absence.Balance.Policy;

/// <summary>Functions of pending balance audit from policy.</summary>
public static class PendingBalanceAuditFromPolicy
{
    public static Dictionary<string, double> SummarizeRegionalBalancePoliciesForAudit(List<RegionalBalancePolicy> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedTotal();
        }
        return totals;
    }

    public static List<PendingBalanceAudit> DerivePendingBalanceAuditsFromPolicy(List<RegionalBalancePolicy> items)
    {
        var outValue = new List<PendingBalanceAudit>();
        foreach (var item in items)
        {
            var derived = PendingBalanceAudit.NewPendingBalanceAudit(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double PlanPendingBalanceAudit(PendingBalanceAudit item)
    {
        return ManualBalanceMetricPolicy.ManualBalanceMetricWeight(8, true);
    }
}
