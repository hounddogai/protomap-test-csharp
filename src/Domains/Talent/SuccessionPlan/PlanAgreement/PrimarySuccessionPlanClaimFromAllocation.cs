namespace Acme.Workforce.Talent.SuccessionPlan.PlanAgreement;

using Acme.Workforce.Talent.Mobility.Allocation;

/// <summary>Functions of primary succession plan claim from allocation.</summary>
public static class PrimarySuccessionPlanClaimFromAllocation
{
    public static Dictionary<string, double> SummarizeRollingMobilityAllocationsForClaim(List<RollingMobilityAllocation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetTotal();
        }
        return totals;
    }

    public static List<PrimarySuccessionPlanClaim> DerivePrimarySuccessionPlanClaimsFromAllocation(List<RollingMobilityAllocation> items)
    {
        var outValue = new List<PrimarySuccessionPlanClaim>();
        foreach (var item in items)
        {
            var derived = PrimarySuccessionPlanClaim.NewPrimarySuccessionPlanClaim(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimatePrimarySuccessionPlanClaim(PrimarySuccessionPlanClaim item)
    {
        return RollingMobilityAllocationPolicy.ClassifyRollingMobilityAllocation(item.MeasuredValue());
    }
}
