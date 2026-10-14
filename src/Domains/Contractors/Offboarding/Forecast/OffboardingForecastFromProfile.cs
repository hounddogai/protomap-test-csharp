namespace Acme.Workforce.Contractors.Offboarding.Forecast;

using Acme.Workforce.Contractors.Engagement.Profile;

/// <summary>Functions of offboarding forecast from profile.</summary>
public static class OffboardingForecastFromProfile
{
    public static Dictionary<string, double> SummarizeEngagementProfilesForForecast(List<EngagementProfile> items)
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

    public static List<OffboardingForecast> DeriveOffboardingForecastsFromProfile(List<EngagementProfile> items)
    {
        var outValue = new List<OffboardingForecast>();
        foreach (var item in items)
        {
            var derived = OffboardingForecast.NewOffboardingForecast(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchOffboardingForecast(OffboardingForecast item)
    {
        return EngagementProfilePolicy.ClassifyEngagementProfile(item.NetLoad());
    }
}
