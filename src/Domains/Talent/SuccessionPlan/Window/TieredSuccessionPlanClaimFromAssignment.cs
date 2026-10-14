namespace Acme.Workforce.Talent.SuccessionPlan.Window;

using Acme.Workforce.Talent.CareerPath.Review;

/// <summary>Functions of tiered succession plan claim from assignment.</summary>
public static class TieredSuccessionPlanClaimFromAssignment
{
    public static Dictionary<string, double> SummarizeCareerPathAssignmentsForClaim(List<CareerPathAssignment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedExposure();
        }
        return totals;
    }

    public static List<TieredSuccessionPlanClaim> DeriveTieredSuccessionPlanClaimsFromAssignment(List<CareerPathAssignment> items)
    {
        var outValue = new List<TieredSuccessionPlanClaim>();
        foreach (var item in items)
        {
            var derived = TieredSuccessionPlanClaim.NewTieredSuccessionPlanClaim(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SummarizeTieredSuccessionPlanClaim(TieredSuccessionPlanClaim item)
    {
        return CareerPathClaimPolicy.CareerPathClaimWeight(9, true);
    }
}
