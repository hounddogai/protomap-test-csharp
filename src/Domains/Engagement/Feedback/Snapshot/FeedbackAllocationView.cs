namespace Acme.Workforce.Engagement.Feedback.Snapshot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a feedback allocation.</summary>
public sealed record FeedbackAllocationView(string Code, string TitleText, string Pinned, string Maximum, string Reviewer, string StatusLabel)
{
    public static FeedbackAllocationView ToFeedbackAllocationView(FeedbackAllocation item)
    {
        return new FeedbackAllocationView(
            Text.Truncate(item.Code, 120),
            Text.Truncate(item.Title, 40),
            $"{item.Pinned}",
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Reviewer,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FeedbackAllocationView> ToFeedbackAllocationViews(List<FeedbackAllocation> items)
    {
        var views = new List<FeedbackAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToFeedbackAllocationView(item));
        }
        return views;
    }

    public static double SumFeedbackAllocationView(FeedbackAllocationView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return parsedMaximum;
    }
}
