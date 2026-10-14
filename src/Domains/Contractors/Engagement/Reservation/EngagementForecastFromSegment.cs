namespace Acme.Workforce.Contractors.Engagement.Reservation;

using Acme.Workforce.Contractors.Engagement.Policy;

/// <summary>Functions of engagement forecast from segment.</summary>
public static class EngagementForecastFromSegment
{
    public static Dictionary<string, double> SummarizeAdaptiveEngagementSegmentsForForecast(List<AdaptiveEngagementSegment> items)
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

    public static List<EngagementForecast> DeriveEngagementForecastsFromSegment(List<AdaptiveEngagementSegment> items)
    {
        var outValue = new List<EngagementForecast>();
        foreach (var item in items)
        {
            var derived = EngagementForecast.NewEngagementForecast(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SplitEngagementForecast(EngagementForecast item)
    {
        return LenientEngagementPolicyPolicy.LenientEngagementPolicyWeight(4, true);
    }
}
