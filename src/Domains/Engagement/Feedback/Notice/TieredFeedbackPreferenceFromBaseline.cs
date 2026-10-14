namespace Acme.Workforce.Engagement.Feedback.Notice;

using Acme.Workforce.Engagement.Recognition.Baseline;

/// <summary>Functions of tiered feedback preference from baseline.</summary>
public static class TieredFeedbackPreferenceFromBaseline
{
    public static Dictionary<string, double> SummarizeRollingRecognitionBaselinesForPreference(List<RollingRecognitionBaseline> items)
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

    public static List<TieredFeedbackPreference> DeriveTieredFeedbackPreferencesFromBaseline(List<RollingRecognitionBaseline> items)
    {
        var outValue = new List<TieredFeedbackPreference>();
        foreach (var item in items)
        {
            var derived = TieredFeedbackPreference.NewTieredFeedbackPreference(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitTieredFeedbackPreference(TieredFeedbackPreference item)
    {
        return StrictRecognitionCyclePolicy.StrictRecognitionCycleBucket(item.NetValue());
    }
}
