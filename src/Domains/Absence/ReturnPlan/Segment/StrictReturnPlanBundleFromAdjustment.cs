namespace Acme.Workforce.Absence.ReturnPlan.Segment;

using Acme.Workforce.Absence.Holiday.Variance;

/// <summary>Functions of strict return plan bundle from adjustment.</summary>
public static class StrictReturnPlanBundleFromAdjustment
{
    public static Dictionary<string, double> SummarizeRegionalHolidayAdjustmentsForBundle(List<RegionalHolidayAdjustment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<StrictReturnPlanBundle> DeriveStrictReturnPlanBundlesFromAdjustment(List<RegionalHolidayAdjustment> items)
    {
        var outValue = new List<StrictReturnPlanBundle>();
        foreach (var item in items)
        {
            var derived = StrictReturnPlanBundle.NewStrictReturnPlanBundle(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectStrictReturnPlanBundle(StrictReturnPlanBundle item)
    {
        return RegionalHolidayAdjustmentPolicy.NormalizeRegionalHolidayAdjustmentCode(item.Code);
    }
}
