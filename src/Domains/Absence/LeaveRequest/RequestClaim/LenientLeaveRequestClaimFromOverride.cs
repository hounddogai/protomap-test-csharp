namespace Acme.Workforce.Absence.LeaveRequest.RequestClaim;

using Acme.Workforce.Absence.ReturnPlan.Summary;

/// <summary>Functions of lenient leave request claim from override.</summary>
public static class LenientLeaveRequestClaimFromOverride
{
    public static Dictionary<string, double> SummarizeLenientReturnPlanOverridesForClaim(List<LenientReturnPlanOverride> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveTotal();
        }
        return totals;
    }

    public static List<LenientLeaveRequestClaim> DeriveLenientLeaveRequestClaimsFromOverride(List<LenientReturnPlanOverride> items)
    {
        var outValue = new List<LenientLeaveRequestClaim>();
        foreach (var item in items)
        {
            var derived = LenientLeaveRequestClaim.NewLenientLeaveRequestClaim(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateLenientLeaveRequestClaim(LenientLeaveRequestClaim item)
    {
        return LenientReturnPlanOverridePolicy.LenientReturnPlanOverrideBucket(item.NetTotal());
    }
}
