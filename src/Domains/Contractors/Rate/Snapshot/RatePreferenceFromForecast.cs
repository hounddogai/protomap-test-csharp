namespace Acme.Workforce.Contractors.Rate.Snapshot;

using Acme.Workforce.Contractors.Offboarding.Profile;

/// <summary>Functions of rate preference from forecast.</summary>
public static class RatePreferenceFromForecast
{
    public static Dictionary<string, double> SummarizeStrictOffboardingForecastsForPreference(List<StrictOffboardingForecast> items)
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

    public static List<RatePreference> DeriveRatePreferencesFromForecast(List<StrictOffboardingForecast> items)
    {
        var outValue = new List<RatePreference>();
        foreach (var item in items)
        {
            var derived = RatePreference.NewRatePreference(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectRatePreference(RatePreference item)
    {
        return OffboardingTargetPolicy.NormalizeOffboardingTargetCode(item.Code);
    }
}
