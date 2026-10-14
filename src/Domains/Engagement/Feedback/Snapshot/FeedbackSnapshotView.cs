namespace Acme.Workforce.Engagement.Feedback.Snapshot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a feedback snapshot.</summary>
public sealed record FeedbackSnapshotView(string CodeText, string Label, string Team, string DepartmentText, string Weight, string RatingText, string MarginText, string AmountText, string Revision, string FloorValue, string Source, string StatusLabel)
{
    public static FeedbackSnapshotView ToFeedbackSnapshotView(FeedbackSnapshot item)
    {
        return new FeedbackSnapshotView(
            item.Code,
            item.Label,
            Text.Truncate(item.Team, 120),
            item.Department,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Source, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FeedbackSnapshotView> ToFeedbackSnapshotViews(List<FeedbackSnapshot> items)
    {
        var views = new List<FeedbackSnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToFeedbackSnapshotView(item));
        }
        return views;
    }

    public static double SumFeedbackSnapshotView(FeedbackSnapshotView view)
    {
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.RatingText, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.MarginText, CultureInfo.InvariantCulture);
        return parsedWeight + parsedRating + parsedMargin;
    }
}
