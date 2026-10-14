namespace Acme.Workforce.Contractors.Engagement.Template;

using Acme.Workforce.Contractors.Engagement.Reservation;

/// <summary>Functions of engagement rule from forecast.</summary>
public static class EngagementRuleFromForecast
{
    public static Dictionary<string, double> SummarizeEngagementForecastsForRule(List<EngagementForecast> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedValue();
        }
        return totals;
    }

    public static List<EngagementRule> DeriveEngagementRulesFromForecast(List<EngagementForecast> items)
    {
        var outValue = new List<EngagementRule>();
        foreach (var item in items)
        {
            var derived = EngagementRule.NewEngagementRule(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchEngagementRule(EngagementRule item)
    {
        return WeightedEngagementReservationPolicy.WeightedEngagementReservationBucket(item.WeightedExposure());
    }
}
