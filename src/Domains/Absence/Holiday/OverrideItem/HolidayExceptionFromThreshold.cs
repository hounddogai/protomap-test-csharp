namespace Acme.Workforce.Absence.Holiday.OverrideItem;

using Acme.Workforce.Absence.Holiday.Variance;

/// <summary>Functions of holiday exception from threshold.</summary>
public static class HolidayExceptionFromThreshold
{
    public static Dictionary<string, double> SummarizeHolidayThresholdsForException(List<HolidayThreshold> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedExposure();
        }
        return totals;
    }

    public static List<HolidayException> DeriveHolidayExceptionsFromThreshold(List<HolidayThreshold> items)
    {
        var outValue = new List<HolidayException>();
        foreach (var item in items)
        {
            var derived = HolidayException.NewHolidayException(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveHolidayException(HolidayException item)
    {
        return LegacyHolidayBundlePolicy.ClassifyLegacyHolidayBundle(item.EffectiveValue());
    }
}
