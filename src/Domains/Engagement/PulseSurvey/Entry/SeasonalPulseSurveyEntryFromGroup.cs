namespace Acme.Workforce.Engagement.PulseSurvey.Entry;

using Acme.Workforce.Engagement.Recognition.Claim;

/// <summary>Functions of seasonal pulse survey entry from group.</summary>
public static class SeasonalPulseSurveyEntryFromGroup
{
    public static Dictionary<string, double> SummarizeRecognitionGroupsForEntry(List<RecognitionGroup> items)
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

    public static List<SeasonalPulseSurveyEntry> DeriveSeasonalPulseSurveyEntriesFromGroup(List<RecognitionGroup> items)
    {
        var outValue = new List<SeasonalPulseSurveyEntry>();
        foreach (var item in items)
        {
            var derived = SeasonalPulseSurveyEntry.NewSeasonalPulseSurveyEntry(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplySeasonalPulseSurveyEntry(SeasonalPulseSurveyEntry item)
    {
        return RecognitionClaimPolicy.RecognitionClaimBucket(item.AdjustedVolume());
    }
}
