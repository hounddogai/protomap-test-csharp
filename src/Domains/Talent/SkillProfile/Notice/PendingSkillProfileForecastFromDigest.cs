namespace Acme.Workforce.Talent.SkillProfile.Notice;

using Acme.Workforce.Talent.CareerPath.Draft;

/// <summary>Functions of pending skill profile forecast from digest.</summary>
public static class PendingSkillProfileForecastFromDigest
{
    public static Dictionary<string, double> SummarizeSeasonalCareerPathDigestsForForecast(List<SeasonalCareerPathDigest> items)
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

    public static List<PendingSkillProfileForecast> DerivePendingSkillProfileForecastsFromDigest(List<SeasonalCareerPathDigest> items)
    {
        var outValue = new List<PendingSkillProfileForecast>();
        foreach (var item in items)
        {
            var derived = PendingSkillProfileForecast.NewPendingSkillProfileForecast(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolvePendingSkillProfileForecast(PendingSkillProfileForecast item)
    {
        return SeasonalCareerPathDigestPolicy.ClassifySeasonalCareerPathDigest(item.MeasuredValue());
    }
}
