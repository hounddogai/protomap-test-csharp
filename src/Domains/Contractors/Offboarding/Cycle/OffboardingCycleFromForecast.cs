namespace Acme.Workforce.Contractors.Offboarding.Cycle;

using Acme.Workforce.Contractors.Timesheet.TimesheetEstimate;

/// <summary>Functions of offboarding cycle from forecast.</summary>
public static class OffboardingCycleFromForecast
{
    public static Dictionary<string, double> SummarizeTimesheetForecastsForCycle(List<TimesheetForecast> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedExposure();
        }
        return totals;
    }

    public static List<OffboardingCycle> DeriveOffboardingCyclesFromForecast(List<TimesheetForecast> items)
    {
        var outValue = new List<OffboardingCycle>();
        foreach (var item in items)
        {
            var derived = OffboardingCycle.NewOffboardingCycle(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeOffboardingCycle(OffboardingCycle item)
    {
        return LegacyTimesheetEstimatePolicy.ClassifyLegacyTimesheetEstimate(item.WeightedExposure());
    }
}
