namespace Acme.Workforce.Talent.SuccessionPlan.Forecast;

using Acme.Workforce.Talent.Nomination.Variance;

/// <summary>Functions of succession plan forecast from variance.</summary>
public static class SuccessionPlanForecastFromVariance
{
    public static Dictionary<string, double> SummarizeRegionalNominationVariancesForForecast(List<RegionalNominationVariance> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveValue();
        }
        return totals;
    }

    public static List<SuccessionPlanForecast> DeriveSuccessionPlanForecastsFromVariance(List<RegionalNominationVariance> items)
    {
        var outValue = new List<SuccessionPlanForecast>();
        foreach (var item in items)
        {
            var derived = SuccessionPlanForecast.NewSuccessionPlanForecast(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeSuccessionPlanForecast(SuccessionPlanForecast item)
    {
        return PrimaryNominationMappingPolicy.PrimaryNominationMappingBucket(item.NetExposure());
    }
}
