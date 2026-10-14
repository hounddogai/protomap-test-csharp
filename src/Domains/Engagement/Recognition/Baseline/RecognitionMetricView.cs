namespace Acme.Workforce.Engagement.Recognition.Baseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a recognition metric.</summary>
public sealed record RecognitionMetricView(string Code, string Name, string Minimum, string LeaveDaysText, string Locale, string DurationMinutes, string ExternalRefText, string SequenceText, string GraceDaysText, string BadgeText, string StatusLabel)
{
    public static RecognitionMetricView ToRecognitionMetricView(RecognitionMetric item)
    {
        return new RecognitionMetricView(
            Text.Truncate(item.Code, 120),
            item.Name,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Locale,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.ExternalRef,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Badge,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RecognitionMetricView> ToRecognitionMetricViews(List<RecognitionMetric> items)
    {
        var views = new List<RecognitionMetricView>();
        foreach (var item in items)
        {
            views.Add(ToRecognitionMetricView(item));
        }
        return views;
    }

    public static double SumRecognitionMetricView(RecognitionMetricView view)
    {
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDaysText, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        return parsedMinimum + (double)parsedLeaveDays + (double)parsedDurationMinutes;
    }
}
