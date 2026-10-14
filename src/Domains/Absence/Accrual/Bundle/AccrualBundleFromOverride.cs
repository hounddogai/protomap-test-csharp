namespace Acme.Workforce.Absence.Accrual.Bundle;

using Acme.Workforce.Absence.Holiday.OverrideItem;

/// <summary>Functions of accrual bundle from override.</summary>
public static class AccrualBundleFromOverride
{
    public static Dictionary<string, double> SummarizeHolidayOverridesForBundle(List<HolidayOverride> items)
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

    public static List<AccrualBundle> DeriveAccrualBundlesFromOverride(List<HolidayOverride> items)
    {
        var outValue = new List<AccrualBundle>();
        foreach (var item in items)
        {
            var derived = AccrualBundle.NewAccrualBundle(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveAccrualBundle(AccrualBundle item)
    {
        return HolidayBudgetPolicy.ClassifyHolidayBudget(item.NetValue());
    }
}
