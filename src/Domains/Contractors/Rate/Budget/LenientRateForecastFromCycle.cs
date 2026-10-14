namespace Acme.Workforce.Contractors.Rate.Budget;

using Acme.Workforce.Contractors.Timesheet.Request;

/// <summary>Functions of lenient rate forecast from cycle.</summary>
public static class LenientRateForecastFromCycle
{
    public static Dictionary<string, double> SummarizeTimesheetCyclesForForecast(List<TimesheetCycle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedLoad();
        }
        return totals;
    }

    public static List<LenientRateForecast> DeriveLenientRateForecastsFromCycle(List<TimesheetCycle> items)
    {
        var outValue = new List<LenientRateForecast>();
        foreach (var item in items)
        {
            var derived = LenientRateForecast.NewLenientRateForecast(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeLenientRateForecast(LenientRateForecast item)
    {
        return ScheduledTimesheetTargetPolicy.ClassifyScheduledTimesheetTarget(item.NetTotal());
    }
}
