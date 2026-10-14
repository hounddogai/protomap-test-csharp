namespace Acme.Workforce.Engagement.Recognition.Agreement;

using Acme.Workforce.Engagement.PulseSurvey.Profile;

/// <summary>Functions of default recognition threshold from profile.</summary>
public static class DefaultRecognitionThresholdFromProfile
{
    public static Dictionary<string, double> SummarizePulseSurveyProfilesForThreshold(List<PulseSurveyProfile> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<DefaultRecognitionThreshold> DeriveDefaultRecognitionThresholdsFromProfile(List<PulseSurveyProfile> items)
    {
        var outValue = new List<DefaultRecognitionThreshold>();
        foreach (var item in items)
        {
            var derived = DefaultRecognitionThreshold.NewDefaultRecognitionThreshold(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankDefaultRecognitionThreshold(DefaultRecognitionThreshold item)
    {
        return PulseSurveyMilestonePolicy.PulseSurveyMilestoneBucket(item.AdjustedExposure());
    }
}
