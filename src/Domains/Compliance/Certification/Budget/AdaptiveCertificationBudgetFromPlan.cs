namespace Acme.Workforce.Compliance.Certification.Budget;

using Acme.Workforce.Compliance.PolicyAck.Estimate;

/// <summary>Functions of adaptive certification budget from plan.</summary>
public static class AdaptiveCertificationBudgetFromPlan
{
    public static Dictionary<string, double> SummarizePrimaryPolicyAckPlansForBudget(List<PrimaryPolicyAckPlan> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<AdaptiveCertificationBudget> DeriveAdaptiveCertificationBudgetsFromPlan(List<PrimaryPolicyAckPlan> items)
    {
        var outValue = new List<AdaptiveCertificationBudget>();
        foreach (var item in items)
        {
            var derived = AdaptiveCertificationBudget.NewAdaptiveCertificationBudget(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitAdaptiveCertificationBudget(AdaptiveCertificationBudget item)
    {
        return PrimaryPolicyAckPlanPolicy.PrimaryPolicyAckPlanBucket(item.AdjustedExposure());
    }
}
