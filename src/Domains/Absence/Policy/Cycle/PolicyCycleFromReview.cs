namespace Acme.Workforce.Absence.Policy.Cycle;

using Acme.Workforce.Absence.Balance.Milestone;

/// <summary>Functions of policy cycle from review.</summary>
public static class PolicyCycleFromReview
{
    public static Dictionary<string, double> SummarizeBalanceReviewsForCycle(List<BalanceReview> items)
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

    public static List<PolicyCycle> DerivePolicyCyclesFromReview(List<BalanceReview> items)
    {
        var outValue = new List<PolicyCycle>();
        foreach (var item in items)
        {
            var derived = PolicyCycle.NewPolicyCycle(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchPolicyCycle(PolicyCycle item)
    {
        return BalanceWindowPolicy.BalanceWindowBucket(item.ProjectedTotal());
    }
}
