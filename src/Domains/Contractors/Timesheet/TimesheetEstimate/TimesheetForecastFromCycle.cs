namespace Acme.Workforce.Contractors.Timesheet.TimesheetEstimate;

using Acme.Workforce.Contractors.Engagement.Summary;

/// <summary>Functions of timesheet forecast from cycle.</summary>
public static class TimesheetForecastFromCycle
{
    public static Dictionary<string, double> SummarizeEngagementCyclesForForecast(List<EngagementCycle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedVolume();
        }
        return totals;
    }

    public static List<TimesheetForecast> DeriveTimesheetForecastsFromCycle(List<EngagementCycle> items)
    {
        var outValue = new List<TimesheetForecast>();
        foreach (var item in items)
        {
            var derived = TimesheetForecast.NewTimesheetForecast(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankTimesheetForecast(TimesheetForecast item)
    {
        return EngagementMappingPolicy.NormalizeEngagementMappingCode(item.Code);
    }
}
