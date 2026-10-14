namespace Acme.Workforce.Engagement.PulseSurvey.Metric;

using System.Globalization;

/// <summary>How screens and exports show a scheduled pulse survey estimate.</summary>
public sealed record ScheduledPulseSurveyEstimateView(string Code, string Title, string Skill, string Attempts, string ActiveText, string Ceiling, string StatusLabel)
{
    public static ScheduledPulseSurveyEstimateView ToScheduledPulseSurveyEstimateView(ScheduledPulseSurveyEstimate item)
    {
        return new ScheduledPulseSurveyEstimateView(
            item.Code,
            item.Title,
            item.Skill,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledPulseSurveyEstimateView> ToScheduledPulseSurveyEstimateViews(List<ScheduledPulseSurveyEstimate> items)
    {
        var views = new List<ScheduledPulseSurveyEstimateView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledPulseSurveyEstimateView(item));
        }
        return views;
    }

    public static double SumScheduledPulseSurveyEstimateView(ScheduledPulseSurveyEstimateView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + parsedCeiling;
    }
}
