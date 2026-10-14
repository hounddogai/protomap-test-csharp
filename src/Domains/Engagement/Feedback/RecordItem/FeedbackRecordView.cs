namespace Acme.Workforce.Engagement.Feedback.RecordItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a feedback record.</summary>
public sealed record FeedbackRecordView(string Code, string Label, string Description, string Score, string TeamText, string LeaveDays, string StatusLabel)
{
    public static FeedbackRecordView ToFeedbackRecordView(FeedbackRecord item)
    {
        return new FeedbackRecordView(
            item.Code,
            Text.Truncate(item.Label, 40),
            Text.Truncate(item.Description, 64),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Team,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FeedbackRecordView> ToFeedbackRecordViews(List<FeedbackRecord> items)
    {
        var views = new List<FeedbackRecordView>();
        foreach (var item in items)
        {
            views.Add(ToFeedbackRecordView(item));
        }
        return views;
    }

    public static double SumFeedbackRecordView(FeedbackRecordView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        return parsedScore + (double)parsedLeaveDays;
    }
}
