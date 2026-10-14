namespace Acme.Workforce.Talent.CareerPath.Review;

using Acme.Workforce.Talent.SuccessionPlan.PlanVariance;

/// <summary>Functions of career path claim from adjustment.</summary>
public static class CareerPathClaimFromAdjustment
{
    public static Dictionary<string, double> SummarizeRegionalSuccessionPlanAdjustmentsForClaim(List<RegionalSuccessionPlanAdjustment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetValue();
        }
        return totals;
    }

    public static List<CareerPathClaim> DeriveCareerPathClaimsFromAdjustment(List<RegionalSuccessionPlanAdjustment> items)
    {
        var outValue = new List<CareerPathClaim>();
        foreach (var item in items)
        {
            var derived = CareerPathClaim.NewCareerPathClaim(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SplitCareerPathClaim(CareerPathClaim item)
    {
        return RegionalSuccessionPlanAdjustmentPolicy.RegionalSuccessionPlanAdjustmentWeight(1, true);
    }
}
