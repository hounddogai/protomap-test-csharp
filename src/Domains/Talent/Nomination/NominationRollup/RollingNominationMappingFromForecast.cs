namespace Acme.Workforce.Talent.Nomination.NominationRollup;

using Acme.Workforce.Talent.Pool.PoolWindow;

/// <summary>Functions of rolling nomination mapping from forecast.</summary>
public static class RollingNominationMappingFromForecast
{
    public static Dictionary<string, double> SummarizeCappedPoolForecastsForMapping(List<CappedPoolForecast> items)
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

    public static List<RollingNominationMapping> DeriveRollingNominationMappingsFromForecast(List<CappedPoolForecast> items)
    {
        var outValue = new List<RollingNominationMapping>();
        foreach (var item in items)
        {
            var derived = RollingNominationMapping.NewRollingNominationMapping(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreRollingNominationMapping(RollingNominationMapping item)
    {
        return ScheduledPoolWindowPolicy.NormalizeScheduledPoolWindowCode(item.Code);
    }
}
