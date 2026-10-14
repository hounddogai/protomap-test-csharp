namespace Acme.Workforce.Workplace.Desk.Agreement;

using Acme.Workforce.Workplace.Occupancy.Plan;

/// <summary>Functions of lenient desk forecast from review.</summary>
public static class LenientDeskForecastFromReview
{
    public static Dictionary<string, double> SummarizeStrictOccupancyReviewsForForecast(List<StrictOccupancyReview> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetTotal();
        }
        return totals;
    }

    public static List<LenientDeskForecast> DeriveLenientDeskForecastsFromReview(List<StrictOccupancyReview> items)
    {
        var outValue = new List<LenientDeskForecast>();
        foreach (var item in items)
        {
            var derived = LenientDeskForecast.NewLenientDeskForecast(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectLenientDeskForecast(LenientDeskForecast item)
    {
        return ScheduledOccupancyPlanPolicy.NormalizeScheduledOccupancyPlanCode(item.Code);
    }
}
