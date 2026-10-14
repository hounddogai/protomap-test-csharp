namespace Acme.Workforce.Engagement.PulseSurvey.Target;

using Acme.Workforce.Engagement.Feedback.Quota;

/// <summary>Functions of pulse survey target from forecast.</summary>
public static class PulseSurveyTargetFromForecast
{
    public static Dictionary<string, double> SummarizeAdaptiveFeedbackForecastsForTarget(List<AdaptiveFeedbackForecast> items)
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

    public static List<PulseSurveyTarget> DerivePulseSurveyTargetsFromForecast(List<AdaptiveFeedbackForecast> items)
    {
        var outValue = new List<PulseSurveyTarget>();
        foreach (var item in items)
        {
            var derived = PulseSurveyTarget.NewPulseSurveyTarget(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignPulseSurveyTarget(PulseSurveyTarget item)
    {
        return StrictFeedbackQuotaPolicy.StrictFeedbackQuotaBucket(item.MeasuredValue());
    }
}
