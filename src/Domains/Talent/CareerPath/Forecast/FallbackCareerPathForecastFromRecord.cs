namespace Acme.Workforce.Talent.CareerPath.Forecast;

using Acme.Workforce.Talent.Mobility.Tier;

/// <summary>Functions of fallback career path forecast from record.</summary>
public static class FallbackCareerPathForecastFromRecord
{
    public static Dictionary<string, double> SummarizeMobilityRecordsForForecast(List<MobilityRecord> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedValue();
        }
        return totals;
    }

    public static List<FallbackCareerPathForecast> DeriveFallbackCareerPathForecastsFromRecord(List<MobilityRecord> items)
    {
        var outValue = new List<FallbackCareerPathForecast>();
        foreach (var item in items)
        {
            var derived = FallbackCareerPathForecast.NewFallbackCareerPathForecast(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeFallbackCareerPathForecast(FallbackCareerPathForecast item)
    {
        return ScheduledMobilityRequestPolicy.ClassifyScheduledMobilityRequest(item.ProjectedLoad());
    }
}
