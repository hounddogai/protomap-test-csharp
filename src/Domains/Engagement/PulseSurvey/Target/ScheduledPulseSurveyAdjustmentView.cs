namespace Acme.Workforce.Engagement.PulseSurvey.Target;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled pulse survey adjustment.</summary>
public sealed record ScheduledPulseSurveyAdjustmentView(string CodeText, string Title, string Reviewer, string Timezone, string DurationMinutes, string Hours, string Version, string Attempts, string CreatedAtText, string Rate, string Badge, string StatusLabel)
{
    public static ScheduledPulseSurveyAdjustmentView ToScheduledPulseSurveyAdjustmentView(ScheduledPulseSurveyAdjustment item)
    {
        return new ScheduledPulseSurveyAdjustmentView(
            item.Code,
            item.Title,
            Text.Truncate(item.Reviewer, 80),
            Text.Truncate(item.Timezone, 120),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Badge,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledPulseSurveyAdjustmentView> ToScheduledPulseSurveyAdjustmentViews(List<ScheduledPulseSurveyAdjustment> items)
    {
        var views = new List<ScheduledPulseSurveyAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledPulseSurveyAdjustmentView(item));
        }
        return views;
    }

    public static double SumScheduledPulseSurveyAdjustmentView(ScheduledPulseSurveyAdjustmentView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + parsedHours + (double)parsedVersion;
    }
}
