namespace Acme.Workforce.Engagement.Feedback.Notice;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched feedback notice.</summary>
public sealed record BatchedFeedbackNoticeView(string Code, string LabelText, string Skill, string Attempts, string Rate, string Title, string StartsAt, string Checksum, string StatusLabel)
{
    public static BatchedFeedbackNoticeView ToBatchedFeedbackNoticeView(BatchedFeedbackNotice item)
    {
        return new BatchedFeedbackNoticeView(
            Text.Truncate(item.Code, 64),
            item.Label,
            item.Skill,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Title,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Checksum,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedFeedbackNoticeView> ToBatchedFeedbackNoticeViews(List<BatchedFeedbackNotice> items)
    {
        var views = new List<BatchedFeedbackNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedFeedbackNoticeView(item));
        }
        return views;
    }

    public static double SumBatchedFeedbackNoticeView(BatchedFeedbackNoticeView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + parsedRate + (double)parsedStartsAt;
    }
}
