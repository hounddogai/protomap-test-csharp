namespace Acme.Workforce.Engagement.Feedback.Quota;

using Acme.Workforce.Engagement.PulseSurvey.Preference;

/// <summary>Functions of feedback notice from preference.</summary>
public static class FeedbackNoticeFromPreference
{
    public static Dictionary<string, double> SummarizeStrictPulseSurveyPreferencesForNotice(List<StrictPulseSurveyPreference> items)
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

    public static List<FeedbackNotice> DeriveFeedbackNoticesFromPreference(List<StrictPulseSurveyPreference> items)
    {
        var outValue = new List<FeedbackNotice>();
        foreach (var item in items)
        {
            var derived = FeedbackNotice.NewFeedbackNotice(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectFeedbackNotice(FeedbackNotice item)
    {
        return StrictPulseSurveyPreferencePolicy.ClassifyStrictPulseSurveyPreference(item.ProjectedValue());
    }
}
