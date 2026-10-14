namespace Acme.Workforce.Engagement.PulseSurvey.Template;

using Acme.Workforce.Engagement.Feedback.Preference;

/// <summary>Functions of pulse survey notice from preference.</summary>
public static class PulseSurveyNoticeFromPreference
{
    public static Dictionary<string, double> SummarizeFeedbackPreferencesForNotice(List<FeedbackPreference> items)
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

    public static List<PulseSurveyNotice> DerivePulseSurveyNoticesFromPreference(List<FeedbackPreference> items)
    {
        var outValue = new List<PulseSurveyNotice>();
        foreach (var item in items)
        {
            var derived = PulseSurveyNotice.NewPulseSurveyNotice(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolvePulseSurveyNotice(PulseSurveyNotice item)
    {
        return FeedbackSchedulePolicy.FeedbackScheduleBucket(item.MeasuredValue());
    }
}
