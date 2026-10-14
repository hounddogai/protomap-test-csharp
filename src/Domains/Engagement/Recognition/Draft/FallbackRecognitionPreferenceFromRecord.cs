namespace Acme.Workforce.Engagement.Recognition.Draft;

using Acme.Workforce.Engagement.Recognition.RecordItem;

/// <summary>Functions of fallback recognition preference from record.</summary>
public static class FallbackRecognitionPreferenceFromRecord
{
    public static Dictionary<string, double> SummarizeRecognitionRecordsForPreference(List<RecognitionRecord> items)
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

    public static List<FallbackRecognitionPreference> DeriveFallbackRecognitionPreferencesFromRecord(List<RecognitionRecord> items)
    {
        var outValue = new List<FallbackRecognitionPreference>();
        foreach (var item in items)
        {
            var derived = FallbackRecognitionPreference.NewFallbackRecognitionPreference(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeFallbackRecognitionPreference(FallbackRecognitionPreference item)
    {
        return BatchedRecognitionSlotPolicy.BatchedRecognitionSlotBucket(item.ProjectedLoad());
    }
}
