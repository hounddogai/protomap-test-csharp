namespace Acme.Workforce.Talent.SuccessionPlan.Claim;

using Acme.Workforce.Talent.CareerPath.Ledger;

/// <summary>Functions of succession plan claim from group.</summary>
public static class SuccessionPlanClaimFromGroup
{
    public static Dictionary<string, double> SummarizeCareerPathGroupsForClaim(List<CareerPathGroup> items)
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

    public static List<SuccessionPlanClaim> DeriveSuccessionPlanClaimsFromGroup(List<CareerPathGroup> items)
    {
        var outValue = new List<SuccessionPlanClaim>();
        foreach (var item in items)
        {
            var derived = SuccessionPlanClaim.NewSuccessionPlanClaim(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanSuccessionPlanClaim(SuccessionPlanClaim item)
    {
        return CareerPathRollupPolicy.ClassifyCareerPathRollup(item.EffectiveVolume());
    }
}
