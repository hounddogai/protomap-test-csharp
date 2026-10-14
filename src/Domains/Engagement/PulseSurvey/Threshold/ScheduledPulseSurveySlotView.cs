namespace Acme.Workforce.Engagement.PulseSurvey.Threshold;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled pulse survey slot.</summary>
public sealed record ScheduledPulseSurveySlotView(string Code, string Title, string Ratio, string Hours, string CostCenter, string Name, string DurationMinutesText, string MaximumText, string StatusLabel)
{
    public static ScheduledPulseSurveySlotView ToScheduledPulseSurveySlotView(ScheduledPulseSurveySlot item)
    {
        return new ScheduledPulseSurveySlotView(
            Text.Truncate(item.Code, 120),
            Text.Truncate(item.Title, 120),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.CostCenter,
            item.Name,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledPulseSurveySlotView> ToScheduledPulseSurveySlotViews(List<ScheduledPulseSurveySlot> items)
    {
        var views = new List<ScheduledPulseSurveySlotView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledPulseSurveySlotView(item));
        }
        return views;
    }

    public static double SumScheduledPulseSurveySlotView(ScheduledPulseSurveySlotView view)
    {
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        return parsedRatio + parsedHours + (double)parsedDurationMinutes;
    }
}
