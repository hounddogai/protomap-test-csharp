namespace Acme.Workforce.Workplace.Visitor.VisitorOverride;

using Acme.Workforce.Workplace.Occupancy.Score;

/// <summary>Functions of visitor forecast from schedule.</summary>
public static class VisitorForecastFromSchedule
{
    public static Dictionary<string, double> SummarizeOccupancySchedulesForForecast(List<OccupancySchedule> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<VisitorForecast> DeriveVisitorForecastsFromSchedule(List<OccupancySchedule> items)
    {
        var outValue = new List<VisitorForecast>();
        foreach (var item in items)
        {
            var derived = VisitorForecast.NewVisitorForecast(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanVisitorForecast(VisitorForecast item)
    {
        return OccupancyAdjustmentPolicy.NormalizeOccupancyAdjustmentCode(item.Code);
    }
}
