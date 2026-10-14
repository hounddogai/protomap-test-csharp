namespace Acme.Workforce.Contractors.Offboarding.Segment;

using Acme.Workforce.Contractors.Engagement.Reservation;

/// <summary>Functions of legacy offboarding threshold from forecast.</summary>
public static class LegacyOffboardingThresholdFromForecast
{
    public static Dictionary<string, double> SummarizeEngagementForecastsForThreshold(List<EngagementForecast> items)
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

    public static List<LegacyOffboardingThreshold> DeriveLegacyOffboardingThresholdsFromForecast(List<EngagementForecast> items)
    {
        var outValue = new List<LegacyOffboardingThreshold>();
        foreach (var item in items)
        {
            var derived = LegacyOffboardingThreshold.NewLegacyOffboardingThreshold(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignLegacyOffboardingThreshold(LegacyOffboardingThreshold item)
    {
        return EngagementForecastPolicy.NormalizeEngagementForecastCode(item.Code);
    }
}
