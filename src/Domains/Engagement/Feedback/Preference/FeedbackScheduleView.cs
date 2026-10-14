namespace Acme.Workforce.Engagement.Feedback.Preference;

using System.Globalization;

/// <summary>How screens and exports show a feedback schedule.</summary>
public sealed record FeedbackScheduleView(string CodeText, string Label, string Maximum, string OwnerId, string Score, string StatusLabel)
{
    public static FeedbackScheduleView ToFeedbackScheduleView(FeedbackSchedule item)
    {
        return new FeedbackScheduleView(
            item.Code,
            item.Label,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.OwnerId,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FeedbackScheduleView> ToFeedbackScheduleViews(List<FeedbackSchedule> items)
    {
        var views = new List<FeedbackScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToFeedbackScheduleView(item));
        }
        return views;
    }

    public static double SumFeedbackScheduleView(FeedbackScheduleView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return parsedMaximum + parsedScore;
    }
}
