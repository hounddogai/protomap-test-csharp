namespace Acme.Workforce.Absence.Balance.BalanceTarget;

using Acme.Workforce.Absence.Policy.Segment;

/// <summary>Functions of balance budget from segment.</summary>
public static class BalanceBudgetFromSegment
{
    public static Dictionary<string, double> SummarizePolicySegmentsForBudget(List<PolicySegment> items)
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

    public static List<BalanceBudget> DeriveBalanceBudgetsFromSegment(List<PolicySegment> items)
    {
        var outValue = new List<BalanceBudget>();
        foreach (var item in items)
        {
            var derived = BalanceBudget.NewBalanceBudget(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankBalanceBudget(BalanceBudget item)
    {
        return RollingPolicyEstimatePolicy.NormalizeRollingPolicyEstimateCode(item.Code);
    }
}
