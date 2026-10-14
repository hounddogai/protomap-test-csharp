namespace Acme.Workforce.Engagement.PulseSurvey.Profile;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling pulse survey checkpoint.</summary>
public sealed record RollingPulseSurveyCheckpointView(string Code, string Title, string DurationMinutesText, string Summary, string CreatedAtText, string Margin, string TimezoneText, string Name, string LocaleText, string TenureMonths, string StatusLabel)
{
    public static RollingPulseSurveyCheckpointView ToRollingPulseSurveyCheckpointView(RollingPulseSurveyCheckpoint item)
    {
        return new RollingPulseSurveyCheckpointView(
            Text.Truncate(item.Code, 120),
            Text.Truncate(item.Title, 40),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Summary,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Timezone,
            item.Name,
            item.Locale,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingPulseSurveyCheckpointView> ToRollingPulseSurveyCheckpointViews(List<RollingPulseSurveyCheckpoint> items)
    {
        var views = new List<RollingPulseSurveyCheckpointView>();
        foreach (var item in items)
        {
            views.Add(ToRollingPulseSurveyCheckpointView(item));
        }
        return views;
    }

    public static double SumRollingPulseSurveyCheckpointView(RollingPulseSurveyCheckpointView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAtText, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + (double)parsedCreatedAt + parsedMargin;
    }
}
