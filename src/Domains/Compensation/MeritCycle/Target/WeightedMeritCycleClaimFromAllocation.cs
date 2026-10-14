namespace Acme.Workforce.Compensation.MeritCycle.Target;

using Acme.Workforce.Compensation.PayBand.Baseline;

/// <summary>Functions of weighted merit cycle claim from allocation.</summary>
public static class WeightedMeritCycleClaimFromAllocation
{
    public static Dictionary<string, double> SummarizeGlobalPayBandAllocationsForClaim(List<GlobalPayBandAllocation> items)
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

    public static List<WeightedMeritCycleClaim> DeriveWeightedMeritCycleClaimsFromAllocation(List<GlobalPayBandAllocation> items)
    {
        var outValue = new List<WeightedMeritCycleClaim>();
        foreach (var item in items)
        {
            var derived = WeightedMeritCycleClaim.NewWeightedMeritCycleClaim(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveWeightedMeritCycleClaim(WeightedMeritCycleClaim item)
    {
        return PayBandBaselinePolicy.NormalizePayBandBaselineCode(item.Code);
    }
}
