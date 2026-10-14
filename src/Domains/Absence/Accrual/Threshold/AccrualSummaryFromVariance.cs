namespace Acme.Workforce.Absence.Accrual.Threshold;

using Acme.Workforce.Absence.Holiday.Policy;

/// <summary>Functions of accrual summary from variance.</summary>
public static class AccrualSummaryFromVariance
{
    public static Dictionary<string, double> SummarizeHolidayVariancesForSummary(List<HolidayVariance> items)
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

    public static List<AccrualSummary> DeriveAccrualSummariesFromVariance(List<HolidayVariance> items)
    {
        var outValue = new List<AccrualSummary>();
        foreach (var item in items)
        {
            var derived = AccrualSummary.NewAccrualSummary(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ApplyAccrualSummary(AccrualSummary item)
    {
        return HolidayVariancePolicy.HolidayVarianceWeight(4, true);
    }
}
