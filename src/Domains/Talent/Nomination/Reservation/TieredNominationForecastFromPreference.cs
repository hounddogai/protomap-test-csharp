namespace Acme.Workforce.Talent.Nomination.Reservation;

using Acme.Workforce.Talent.SuccessionPlan.Entry;

/// <summary>Functions of tiered nomination forecast from preference.</summary>
public static class TieredNominationForecastFromPreference
{
    public static Dictionary<string, double> SummarizeSeasonalSuccessionPlanPreferencesForForecast(List<SeasonalSuccessionPlanPreference> items)
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

    public static List<TieredNominationForecast> DeriveTieredNominationForecastsFromPreference(List<SeasonalSuccessionPlanPreference> items)
    {
        var outValue = new List<TieredNominationForecast>();
        foreach (var item in items)
        {
            var derived = TieredNominationForecast.NewTieredNominationForecast(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyTieredNominationForecast(TieredNominationForecast item)
    {
        return SuccessionPlanEntryPolicy.NormalizeSuccessionPlanEntryCode(item.Code);
    }
}
