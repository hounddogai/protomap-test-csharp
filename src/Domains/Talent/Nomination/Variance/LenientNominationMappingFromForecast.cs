namespace Acme.Workforce.Talent.Nomination.Variance;

using Acme.Workforce.Talent.SkillProfile.Forecast;

/// <summary>Functions of lenient nomination mapping from forecast.</summary>
public static class LenientNominationMappingFromForecast
{
    public static Dictionary<string, double> SummarizeDefaultSkillProfileForecastsForMapping(List<DefaultSkillProfileForecast> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetValue();
        }
        return totals;
    }

    public static List<LenientNominationMapping> DeriveLenientNominationMappingsFromForecast(List<DefaultSkillProfileForecast> items)
    {
        var outValue = new List<LenientNominationMapping>();
        foreach (var item in items)
        {
            var derived = LenientNominationMapping.NewLenientNominationMapping(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileLenientNominationMapping(LenientNominationMapping item)
    {
        return SkillProfileSegmentPolicy.SkillProfileSegmentBucket(item.EffectiveVolume());
    }
}
