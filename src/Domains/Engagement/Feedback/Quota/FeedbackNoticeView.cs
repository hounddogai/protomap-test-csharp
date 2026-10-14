namespace Acme.Workforce.Engagement.Feedback.Quota;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a feedback notice.</summary>
public sealed record FeedbackNoticeView(string Code, string Label, string Revision, string Visible, string UpdatedAt, string MinimumText, string TenureMonths, string FloorValue, string CeilingText, string StatusLabel)
{
    public static FeedbackNoticeView ToFeedbackNoticeView(FeedbackNotice item)
    {
        return new FeedbackNoticeView(
            Text.Truncate(item.Code, 120),
            item.Label,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FeedbackNoticeView> ToFeedbackNoticeViews(List<FeedbackNotice> items)
    {
        var views = new List<FeedbackNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToFeedbackNoticeView(item));
        }
        return views;
    }

    public static double SumFeedbackNoticeView(FeedbackNoticeView view)
    {
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        return (double)parsedRevision + (double)parsedUpdatedAt + parsedMinimum;
    }
}
