namespace Acme.Workforce.Absence.Accrual.OverrideItem;

using Acme.Workforce.Absence.Holiday.HolidayReservation;

/// <summary>Functions of primary accrual plan from window.</summary>
public static class PrimaryAccrualPlanFromWindow
{
    public static Dictionary<string, double> SummarizeRegionalHolidayWindowsForPlan(List<RegionalHolidayWindow> items)
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

    public static List<PrimaryAccrualPlan> DerivePrimaryAccrualPlansFromWindow(List<RegionalHolidayWindow> items)
    {
        var outValue = new List<PrimaryAccrualPlan>();
        foreach (var item in items)
        {
            var derived = PrimaryAccrualPlan.NewPrimaryAccrualPlan(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizePrimaryAccrualPlan(PrimaryAccrualPlan item)
    {
        return WeightedHolidayMappingPolicy.WeightedHolidayMappingBucket(item.AdjustedLoad());
    }
}
