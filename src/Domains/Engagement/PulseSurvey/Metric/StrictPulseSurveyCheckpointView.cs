namespace Acme.Workforce.Engagement.PulseSurvey.Metric;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict pulse survey checkpoint.</summary>
public sealed record StrictPulseSurveyCheckpointView(string CodeText, string Label, string Pinned, string StartsAt, string DescriptionText, string ActiveText, string Department, string StatusLabel)
{
    public static StrictPulseSurveyCheckpointView ToStrictPulseSurveyCheckpointView(StrictPulseSurveyCheckpoint item)
    {
        return new StrictPulseSurveyCheckpointView(
            item.Code,
            Text.Truncate(item.Label, 40),
            $"{item.Pinned}",
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Description,
            $"{item.Active}",
            item.Department,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictPulseSurveyCheckpointView> ToStrictPulseSurveyCheckpointViews(List<StrictPulseSurveyCheckpoint> items)
    {
        var views = new List<StrictPulseSurveyCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToStrictPulseSurveyCheckpointView(item));
        }
        return views;
    }

    public static double SumStrictPulseSurveyCheckpointView(StrictPulseSurveyCheckpointView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt;
    }
}
