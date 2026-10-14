namespace Acme.Workforce.Absence.Holiday.Variance;

using Acme.Workforce.Absence.Holiday.HolidayAllocation;

/// <summary>Functions of legacy holiday bundle from allocation.</summary>
public static class LegacyHolidayBundleFromAllocation
{
    public static Dictionary<string, double> SummarizeSeasonalHolidayAllocationsForBundle(List<SeasonalHolidayAllocation> items)
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

    public static List<LegacyHolidayBundle> DeriveLegacyHolidayBundlesFromAllocation(List<SeasonalHolidayAllocation> items)
    {
        var outValue = new List<LegacyHolidayBundle>();
        foreach (var item in items)
        {
            var derived = LegacyHolidayBundle.NewLegacyHolidayBundle(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ResolveLegacyHolidayBundle(LegacyHolidayBundle item)
    {
        return HolidayProfilePolicy.HolidayProfileWeight(9, true);
    }
}
