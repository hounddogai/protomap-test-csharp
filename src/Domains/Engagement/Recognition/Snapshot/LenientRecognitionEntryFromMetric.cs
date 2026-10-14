namespace Acme.Workforce.Engagement.Recognition.Snapshot;

using Acme.Workforce.Engagement.PulseSurvey.SurveyTier;

/// <summary>Functions of lenient recognition entry from metric.</summary>
public static class LenientRecognitionEntryFromMetric
{
    public static Dictionary<string, double> SummarizeScheduledPulseSurveyMetricsForEntry(List<ScheduledPulseSurveyMetric> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetLoad();
        }
        return totals;
    }

    public static List<LenientRecognitionEntry> DeriveLenientRecognitionEntriesFromMetric(List<ScheduledPulseSurveyMetric> items)
    {
        var outValue = new List<LenientRecognitionEntry>();
        foreach (var item in items)
        {
            var derived = LenientRecognitionEntry.NewLenientRecognitionEntry(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectLenientRecognitionEntry(LenientRecognitionEntry item)
    {
        return SeasonalPulseSurveyQuotaPolicy.SeasonalPulseSurveyQuotaBucket(item.EffectiveLoad());
    }
}
