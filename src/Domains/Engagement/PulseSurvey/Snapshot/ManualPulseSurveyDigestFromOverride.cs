namespace Acme.Workforce.Engagement.PulseSurvey.Snapshot;

using Acme.Workforce.Engagement.TownHall.Review;

/// <summary>Functions of manual pulse survey digest from override.</summary>
public static class ManualPulseSurveyDigestFromOverride
{
    public static Dictionary<string, double> SummarizeScheduledTownHallOverridesForDigest(List<ScheduledTownHallOverride> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<ManualPulseSurveyDigest> DeriveManualPulseSurveyDigestsFromOverride(List<ScheduledTownHallOverride> items)
    {
        var outValue = new List<ManualPulseSurveyDigest>();
        foreach (var item in items)
        {
            var derived = ManualPulseSurveyDigest.NewManualPulseSurveyDigest(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleManualPulseSurveyDigest(ManualPulseSurveyDigest item)
    {
        return ScheduledTownHallOverridePolicy.ScheduledTownHallOverrideBucket(item.AdjustedValue());
    }
}
