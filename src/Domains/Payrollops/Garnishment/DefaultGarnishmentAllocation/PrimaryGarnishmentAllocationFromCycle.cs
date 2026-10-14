namespace Acme.Workforce.Payrollops.Garnishment.DefaultGarnishmentAllocation;

using Acme.Workforce.Payrollops.RetroPay.Cycle;

/// <summary>Functions of primary garnishment allocation from cycle.</summary>
public static class PrimaryGarnishmentAllocationFromCycle
{
    public static Dictionary<string, double> SummarizeLegacyRetroPayCyclesForAllocation(List<LegacyRetroPayCycle> items)
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

    public static List<PrimaryGarnishmentAllocation> DerivePrimaryGarnishmentAllocationsFromCycle(List<LegacyRetroPayCycle> items)
    {
        var outValue = new List<PrimaryGarnishmentAllocation>();
        foreach (var item in items)
        {
            var derived = PrimaryGarnishmentAllocation.NewPrimaryGarnishmentAllocation(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergePrimaryGarnishmentAllocation(PrimaryGarnishmentAllocation item)
    {
        return SeasonalRetroPayClaimPolicy.ClassifySeasonalRetroPayClaim(item.AdjustedTotal());
    }
}
