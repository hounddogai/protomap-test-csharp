namespace Acme.Workforce.Engagement.Sentiment.Milestone;

using System.Globalization;

/// <summary>How screens and exports show a sentiment milestone.</summary>
public sealed record SentimentMilestoneView(string Code, string Label, string ChannelText, string Notes, string Grade, string Pinned, string UpdatedAtText, string StatusLabel)
{
    public static SentimentMilestoneView ToSentimentMilestoneView(SentimentMilestone item)
    {
        return new SentimentMilestoneView(
            item.Code,
            item.Label,
            item.Channel,
            item.Notes,
            item.Grade,
            $"{item.Pinned}",
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SentimentMilestoneView> ToSentimentMilestoneViews(List<SentimentMilestone> items)
    {
        var views = new List<SentimentMilestoneView>();
        foreach (var item in items)
        {
            views.Add(ToSentimentMilestoneView(item));
        }
        return views;
    }

    public static double SumSentimentMilestoneView(SentimentMilestoneView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAtText, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt;
    }
}
