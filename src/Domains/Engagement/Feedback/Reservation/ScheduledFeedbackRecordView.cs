namespace Acme.Workforce.Engagement.Feedback.Reservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled feedback record.</summary>
public sealed record ScheduledFeedbackRecordView(string Code, string LabelText, string Retries, string Cost, string HoursText, string SegmentText, string StatusLabel)
{
    public static ScheduledFeedbackRecordView ToScheduledFeedbackRecordView(ScheduledFeedbackRecord item)
    {
        return new ScheduledFeedbackRecordView(
            item.Code,
            Text.Truncate(item.Label, 120),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Segment,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledFeedbackRecordView> ToScheduledFeedbackRecordViews(List<ScheduledFeedbackRecord> items)
    {
        var views = new List<ScheduledFeedbackRecordView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledFeedbackRecordView(item));
        }
        return views;
    }

    public static double SumScheduledFeedbackRecordView(ScheduledFeedbackRecordView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        return (double)parsedRetries + parsedCost + parsedHours;
    }
}
