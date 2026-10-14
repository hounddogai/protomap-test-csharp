namespace Acme.Workforce.Contractors.Rate.Bundle;

using Acme.Workforce.Contractors.Vendor.VendorSchedule;

/// <summary>Functions of fallback rate allocation from schedule.</summary>
public static class FallbackRateAllocationFromSchedule
{
    public static Dictionary<string, double> SummarizeTieredVendorSchedulesForAllocation(List<TieredVendorSchedule> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveVolume();
        }
        return totals;
    }

    public static List<FallbackRateAllocation> DeriveFallbackRateAllocationsFromSchedule(List<TieredVendorSchedule> items)
    {
        var outValue = new List<FallbackRateAllocation>();
        foreach (var item in items)
        {
            var derived = FallbackRateAllocation.NewFallbackRateAllocation(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double RankFallbackRateAllocation(FallbackRateAllocation item)
    {
        return TieredVendorSchedulePolicy.TieredVendorScheduleWeight(2, true);
    }
}
