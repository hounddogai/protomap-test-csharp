namespace Acme.Workforce.Engagement.Feedback.Proposal;

using System.Globalization;

/// <summary>How screens and exports show a feedback baseline.</summary>
public sealed record FeedbackBaselineView(string CodeText, string TitleText, string Summary, string Fte, string UpdatedAt, string SequenceText, string Team, string Badge, string SegmentText, string VersionText, string EndsAt, string StatusLabel)
{
    public static FeedbackBaselineView ToFeedbackBaselineView(FeedbackBaseline item)
    {
        return new FeedbackBaselineView(
            item.Code,
            item.Title,
            item.Summary,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Team,
            item.Badge,
            item.Segment,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FeedbackBaselineView> ToFeedbackBaselineViews(List<FeedbackBaseline> items)
    {
        var views = new List<FeedbackBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToFeedbackBaselineView(item));
        }
        return views;
    }

    public static double SumFeedbackBaselineView(FeedbackBaselineView view)
    {
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.SequenceText, CultureInfo.InvariantCulture);
        return parsedFte + (double)parsedUpdatedAt + (double)parsedSequence;
    }
}
