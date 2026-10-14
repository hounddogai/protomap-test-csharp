namespace Acme.Workforce.Talent.SkillProfile.Preference;

using Acme.Workforce.Talent.Mobility.Forecast;

/// <summary>Functions of regional skill profile record from forecast.</summary>
public static class RegionalSkillProfileRecordFromForecast
{
    public static Dictionary<string, double> SummarizeMobilityForecastsForRecord(List<MobilityForecast> items)
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

    public static List<RegionalSkillProfileRecord> DeriveRegionalSkillProfileRecordsFromForecast(List<MobilityForecast> items)
    {
        var outValue = new List<RegionalSkillProfileRecord>();
        foreach (var item in items)
        {
            var derived = RegionalSkillProfileRecord.NewRegionalSkillProfileRecord(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceRegionalSkillProfileRecord(RegionalSkillProfileRecord item)
    {
        return MobilityReviewPolicy.NormalizeMobilityReviewCode(item.Code);
    }
}
